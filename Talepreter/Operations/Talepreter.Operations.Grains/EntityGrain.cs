using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.DB.Common;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Execute;
using Talepreter.Contracts;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains.States;
using Talepreter.Operations.Plugin;
using Talepreter.Common;

namespace Talepreter.Operations.Grains
{
    [GenerateSerializer]
    public abstract class EntityGrain<TState, TDbContext, TSelf> : GrainWithStateBase<TState>, ICommandGrain
        where TState : EntityGrainStateBase
        where TDbContext : IDbContext
        where TSelf : ICommandGrain // this is needed for plugins, to separate base grains with plugin grains
    {
        protected readonly IPublisher _publisher;
        private readonly ResponsibleService _serviceId;

        protected override string Id => GrainReference.GrainId.ToString();

        protected EntityGrain(IPersistentState<TState> persistentState, ILogger logger, IPublisher publisher, ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger)
        {
            _publisher = publisher;
            _serviceId = serviceId.ServiceId.Map();
        }

        public async Task<ExecutionResult> Execute(CommandId commandId)
        {
            var ctx = Validate(nameof(Execute)).IsNull(commandId, nameof(commandId));

            using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>()
                ?? throw new GrainOperationException(this, nameof(Execute), $"{typeof(TDbContext).Name} initialization failed");

            Command command = null!;
            using var source = new CancellationTokenSource();
            source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);
            var token = source.Token;
            try
            {
                // main query
                var commands = await dbContext.ExecuteAwaitingCommands(commandId.TaleId, commandId.TaleVersionId, commandId.ChapterId, commandId.PageId, commandId.Phase)
                    .Where(x => x.Index == commandId.Index && x.SubIndex == commandId.SubIndex).ToArrayAsync(token);
                if (commands.Length == 0) // 0 result means code problem, we could not find the command which should have existed
                {
                    ctx.Fatal($"Command execution found no command to execute for {commandId}");
                    return ExecutionResult.None;
                }

                token.ThrowIfCancellationRequested();

                var result = ExecutionResult.None;
                command = commands[0];
                // already executed and has a result?
                if (command.Result != CommandExecutionResult.None && command.Result != CommandExecutionResult.Delayed) // if already executed then this could be duplicate message, simply skip
                {
                    ctx.Debug($"Command {commandId} is already executed with result {command.Result}, nothing else to do");
                    result = command.Result switch
                    {
                        CommandExecutionResult.Success => ExecutionResult.Success,
                        CommandExecutionResult.Delayed => ExecutionResult.Blocked,
                        CommandExecutionResult.Failed => ExecutionResult.Faulted,
                        _ => ExecutionResult.None,
                    };
                    return result;
                }
                token.ThrowIfCancellationRequested();

                // execute commands
                EntityDbBase target = null!;
                if (command.Tag == CommandIds.Trigger) target = await ExecuteTriggerCommand(command, dbContext, token);
                else target = await ExecuteCommand(command, dbContext, token);
                await ExecutePlugins(command, dbContext, target, token);

                token.ThrowIfCancellationRequested();

                await SaveState(async (state) =>
                {
                    command.Attempts += 1;
                    command.Error = null; // this is success path, or error is hidden
                    command.Result = CommandExecutionResult.Success;

                    // TODO: watch for db concurrency issues
                    // technically cannot happen as we are in grain and only one grain will be able to process on this command even if we have duplicate messages
                    // also DB has single record (values we got define primary key)
                    await dbContext.SaveChangesAsync(token);
                });

                token.ThrowIfCancellationRequested();

                return ExecutionResult.Success;
            }
            catch (OperationCanceledException ex)
            {
                dbContext.RejectChanges();
                try
                {
                    ctx.Error($"Command execution timed out for {command}");
                    command.Result = CommandExecutionResult.Failed;
                    command.Attempts += 1;
                    command.Error = ex.Message;
                    await dbContext.SaveChangesAsync(token);
                    return ExecutionResult.Timedout;
                }
                catch (Exception e)
                {
                    ctx.Fatal(e, $"Command execution recovery failed for {command}: {e.Message}");
                    return ExecutionResult.Faulted;
                }
            }
            catch (CommandExecutionBlockedException ex)
            {
                dbContext.RejectChanges();
                try
                {
                    if (command!.Attempts >= 9)
                    {
                        ctx.Error($"Command execution is blocked but also retried many times for {command}: {ex.Message}, will be marked as failed");
                        command.Result = CommandExecutionResult.Failed;
                        command.Attempts += 1;
                        command.Error = ex.Message;
                        await dbContext.SaveChangesAsync(token);
                        PingExecutionResult(command, ex);
                        return ExecutionResult.Faulted;
                    }
                    else
                    {
                        ctx.Debug($"Command execution is blocked for {command}: {ex.Message}, will be delayed");
                        command.Result = CommandExecutionResult.Delayed;
                        command.Attempts += 1;
                        command.Error = ex.Message;
                        await dbContext.SaveChangesAsync(token);
                        return ExecutionResult.Blocked;
                    }
                }
                catch (Exception e)
                {
                    ctx.Fatal(e, $"Command execution recovery failed for {command}: {e.Message}");
                    return ExecutionResult.Faulted;
                }
            }
            catch (CommandExecutionException ex)
            {
                dbContext.RejectChanges();
                try
                {
                    ctx.Error($"Command execution failed for {command}: {ex.Message}");
                    command.Result = CommandExecutionResult.Failed;
                    command.Attempts += 1;
                    command.Error = ex.Message;
                    await dbContext.SaveChangesAsync(token);
                    PingExecutionResult(command, ex);
                    return ExecutionResult.Faulted;
                }
                catch (Exception e)
                {
                    ctx.Fatal(e, $"Command execution recovery failed for {command}: {e.Message}");
                    return ExecutionResult.Faulted;
                }
            }
            catch (CommandValidationException ex)
            {
                dbContext.RejectChanges();
                try
                {
                    ctx.Error($"Command validation failed for {command}: {ex.Message}");
                    command.Result = CommandExecutionResult.Failed;
                    command.Attempts += 1;
                    command.Error = ex.Message;
                    await dbContext.SaveChangesAsync(token);
                    PingExecutionResult(command, ex);
                    return ExecutionResult.Faulted;
                }
                catch (Exception e)
                {
                    ctx.Fatal(e, $"Command execution recovery failed for {command}: {e.Message}");
                    return ExecutionResult.Faulted;
                }
            }
            catch (Exception ex)
            {
                dbContext.RejectChanges();
                try
                {
                    ctx.Error(ex, $"Command execution got unexpected error for {command}: {ex.Message}");
                    command.Result = CommandExecutionResult.Failed;
                    command.Attempts += 1;
                    command.Error = ex.Message;
                    await dbContext.SaveChangesAsync(token);
                    PingExecutionResult(command, ex);
                    return ExecutionResult.Faulted;
                }
                catch (Exception e)
                {
                    ctx.Fatal(e, $"Command execution recovery failed for {command}: {e.Message}");
                    return ExecutionResult.Faulted;
                }
            }
        }

        // --

        private void PingExecutionResult(Command command, Exception? ex)
        {
            // this is for UI tracker
            var response = new ExecuteCommandResponse
            {
                TaleId = command.TaleId,
                WriterId = command.WriterId,
                OperationTime = command.OperationTime,
                TaleVersionId = command.TaleVersionId,
                ChapterId = command.ChapterId,
                PageId = command.PageId,
                Index = command.Index,
                Tag = command.Tag,
                Service = _serviceId,
                Error = ex != null ? new ErrorInfo
                {
                    Message = ex.Message,
                    Stacktrace = ex.StackTrace ?? "",
                    Type = ex.GetType().Name,
                } : null,
                Target = command.Target,
                SubIndex = command.SubIndex
            };
            _publisher.Publish(response, TalepreterTopology.ExecuteExchange, TalepreterTopology.ExecuteResultRoutingKey);
        }

        protected async Task ExecutePlugins(Command command, TDbContext dbContext, EntityDbBase target, CancellationToken token)
        {
            var plugins = _scope.ServiceProvider.GetServices<IPluginCommandExecutor<TSelf, TDbContext>>();
            List<bool> results = [];
            foreach (var plugin in plugins) results.Add(await plugin.ExecuteCommand(command, dbContext, _logger, target, token));
            if (results.Any(x => x)) dbContext.SetModified(target, nameof(IExpandedEntity.PluginData));
        }

        protected abstract IContainerGrain FetchContainer(CommandId commandId);

        /// <summary>
        /// overwrite this for each grain and command. every grain operates differently, and furthermore they can use different objects, which is impossible to know unfortunately
        /// return object depends on some cases
        /// - Real Grain + real/custom command : target will be returned even if not updated
        /// - Plugin Grain + real/custom command : plugin object will be fetched
        /// </summary>
        /// <remarks>
        /// one assumption for now: regardless if grain is interested in command, since preprocessing marked the command as needed and interested, we will need an object to work on, and SINGLE object. 
        /// commands only target single objects, but also each command can be separated into multiple subcommands (which are separated by subindex, which can have, and most probably will, multiple targets)
        /// still we send DbContext to plugins so if they want to create more objects or do different checks then they are free, but should not be needed, they are simply supposed to update their data and it should be enough
        /// </remarks>
        protected abstract Task<EntityDbBase> ExecuteCommand(Command command, TDbContext dbContext, CancellationToken token);

        /// <summary>
        /// exact same with ExecuteCommand but only reserved for triggers
        /// </summary>
        protected abstract Task<EntityDbBase> ExecuteTriggerCommand(Command command, TDbContext dbContext, CancellationToken token);

        protected async Task<ExtensionData> ExecuteOnExtension(Command command, TDbContext dbContext, CancellationToken token)
        {
            var extensionData = await dbContext.PluginRecords.OfTale(command).Where(x => x.Id == command.GrainId).FirstOrDefaultAsync(token);
            if (extensionData == null)
            {
                extensionData = new ExtensionData()
                {
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    WriterId = command.WriterId,
                    LastUpdate = command.OperationTime,

                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.GrainId,
                    PluginData = new Container(command.GrainId),

                    BaseId = command.Target,
                    Type = command.Tag,
                    PublishState = PublishState.Skipped, // by default it is false, plugins must set this to true to initiate publishing plugin data too
                    IsNew = true
                };
                dbContext.PluginRecords.Add(extensionData);
            }
            else
            {
                extensionData.LastUpdatedChapter = command.ChapterId;
                extensionData.LastUpdatedPageInChapter = command.PageId;
                extensionData.WriterId = command.WriterId;
                extensionData.LastUpdate = command.OperationTime;
            }

            return extensionData;
        }
    }
}

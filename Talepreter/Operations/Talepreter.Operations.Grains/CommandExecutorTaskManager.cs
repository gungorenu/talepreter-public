using Talepreter.BaseTypes;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts;
using Talepreter.Exceptions;
using Microsoft.Extensions.Logging;
using Talepreter.Common.RabbitMQ;
using Talepreter.Contracts.Execute;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Orleans.Grains.Plugins;

namespace Talepreter.Operations.Grains
{
    public abstract class CommandExecutorTaskManager<TDbContext> : TaskManagerBase, ICommandExecutorTaskManager
        where TDbContext : IDbContext
    {
        private readonly ResultTaskManager<ExecutionResult> _taskMgr;
        private int _taskCount = 0;

        public CommandExecutorTaskManager(IGrainFactory grainFactory, IPublisher publisher, ITalepreterServiceIdentifier serviceId, ILogger logger)
            : base(grainFactory, publisher, serviceId, logger)
        {
            _taskMgr = new ResultTaskManager<ExecutionResult>(e => typeof(CommandExecutionBlockedException).Equals(e), e => e == ExecutionResult.Blocked);
        }

        public abstract string ContainerGrainName { get; }

        public async Task Execute(int chapter, int page)
        {
            try
            {
                // system uses a special phase pattern, at every phase we execute some commands but then wait until all in same phase ends before moving to next phase
                // main reason is to handle special cases where a command needs to execute before or after others.
                // sometimes the dependency/delay pattern does not work because task manager will look at task bag and eventually come to tasks that require others to finish
                // also some commands actually always work without needing for others and suddenly in task manager they might be executed before others while we actually wanted to wait for others first, task manager will not know this
                // instead of retrying even knowing they will fail (or even worse producing wrong results due to order) we delay tasks in the phase pattern.
                // this shall cause some processing overhead (go to DB a few times) but there are other issues that is solved with this (plugins add commands on end like followup commands and they can only be detected in this manner, going back to DB again)
                // by hardcoded logic below, phase count can be any but it must be consecutive, so if phase 0 and 1 exist then can only 2, but not 3
                // take 0: pre-execute, 1:normal-execute, 2:post-execute, -1:last, and avoid others. -1 phase can only be added during execution
                var results = await ExecutePhase(chapter, page, 0);
                if (results.Item1 == ExecutionResult.Success)
                {
                    for (int phase = 1; true; phase++)
                    {
                        results = await ExecutePhase(chapter, page, phase);
                        if (results.Item1 != ExecutionResult.Success || results.Item2 == 0) break;
                    }
                }
                // these are last and dynamic created commands, they are not supposed to be visible at beginning
                if (results.Item1 == ExecutionResult.Success) results = await ExecutePhase(chapter, page, -1);

                ExecutionResult result = results.Item1;
                await ReportBackResult(chapter, page, result);

                _logger.LogInformation($"{_grainLogId} Command executing finalized for page {chapter}#{page} with result {result}, with {_taskMgr.SuccessfullTaskCount}/{_taskMgr.FaultedTaskCount}/{_taskMgr.TimedoutTaskCount} completed/faulted/timedout commands");
            }
            catch (OperationCanceledException)
            {
                try
                {
                    _logger.LogError($"{_grainLogId} Command executing got time out for page {chapter}#{page}");
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(chapter, page, ExecutionResult.Timedout);
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Command execution recovery failed for page {chapter}#{page}: {cex.Message}");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, $"{_grainLogId} Command executing got an error for page {chapter}#{page}");
                    _tokenSource.Cancel();
                    await Task.Delay(500);
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(chapter, page, ExecutionResult.Faulted);
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Command execution recovery failed for page {chapter}#{page}: {cex.Message}");
                }
            }
        }

        private async Task<Tuple<ExecutionResult, int>> ExecutePhase(int chapter, int page, int phase)
        {
            using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>() ?? throw new CommandProcessingException($"{typeof(TDbContext).Name} initialization failed"); ;
            var commands = await dbContext.ExecuteAwaitingCommands(_taleId, _taleVersionId, chapter, page, phase).ToArrayAsync(_tokenSource.Token);
            ExecutionResult res = ExecutionResult.Success;
            if (commands == null || commands.Length == 0) return new Tuple<ExecutionResult, int>(res, 0);

            _taskCount += commands.Length;
            // completely random picked ordering mechanism
            // TODO: in conjuction with the task manager, this block can be improved but then task picking of task manager has to change.
            // right now it will cause some delay due to retry mechanism but it works
            foreach (var command in commands) command.CalculatedIndex = command.Index * 1000 + command.SubIndex;

            foreach (var command in commands.OrderBy(x => x.CalculatedIndex))
            {
                var cmd = Map(command, chapter, page);
                _taskMgr.AppendTasks((token) => Execute(cmd, _grainFactory, _tokenSource.Token));
            }

            var results = _taskMgr.Start(_tokenSource.Token);
            var allErrors = _taskMgr.Errors;
            PublishErrors(allErrors);

            if (_taskMgr.FaultedTaskCount > 0) res = ExecutionResult.Faulted;
            else if (_taskMgr.TimedoutTaskCount > 0) res = ExecutionResult.Timedout;
            else if (_taskMgr.SuccessfullTaskCount == _taskCount)
            {
                if (results == null) res = ExecutionResult.Faulted;
                else if (!results.All(x => x == ExecutionResult.Success)) res = results.First(x => x != ExecutionResult.Success);
                else res = ExecutionResult.Success;
            }
            else res = ExecutionResult.Blocked; // edge case, it should not happen

            return new Tuple<ExecutionResult, int>(res, commands.Length);
        }

        private async Task ReportBackResult(int chapter, int page, ExecutionResult result)
        {
            var pageGrain = _grainFactory.FetchPage(_taleId, _taleVersionId, chapter, page);
            await pageGrain.OnExecuteComplete(ContainerGrainName, result);
        }

        private void PublishErrors(Exception[]? errors)
        {
            if (errors == null || errors.Length == 0) return;

            foreach (var err in errors.OfType<CommandException>())
            {
                var response = new ExecuteCommandResponse
                {
                    TaleId = err.TaleId,
                    WriterId = _writerId,
                    OperationTime = _operationTime,
                    TaleVersionId = err.TaleVersionId,
                    ChapterId = err.ChapterId,
                    PageId = err.PageId,
                    Index = err.Index,
                    Tag = err.Tag,
                    Target = err.Target,
                    Error = new ErrorInfo
                    {
                        Message = err.Message,
                        Stacktrace = err.StackTrace ?? "",
                        Type = err.GetType().Name,
                    },
                    Service = _serviceId
                };

                _publisher.Publish(response, TalepreterTopology.ExecuteExchange, TalepreterTopology.ExecuteResultRoutingKey);
            }
        }

        private static async Task<ExecutionResult> Execute(ExecuteCommand message, IGrainFactory grainFactory, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            token.ThrowIfCancellationRequested();

            // grain handles everything, if delay needed then it will return Blocked which will delay re-execution for some time
            var grain = FetchCommandGrain(grainFactory, message.TargetGrainType, message.TargetGrainId);
            var result = await grain.Execute(new CommandId
            {
                TaleId = message.TaleId,
                TaleVersionId = message.TaleVersionId,
                WriterId = message.WriterId,
                OperationTime = message.OperationTime,
                ChapterId = message.ChapterId,
                Phase = message.Phase,
                SubIndex = message.SubIndex,
                PageId = message.PageId,
                Index = message.Index,
                Tag = message.Tag,
                Target = message.Target
            });
            token.ThrowIfCancellationRequested();
            return result;
        }

        private static ICommandGrain FetchCommandGrain(IGrainFactory factory, string grainTarget, string grainId)
        {
            if (grainTarget == typeof(IActorGrain).Name) return factory.GetGrain<IActorGrain>(grainId);
            else if (grainTarget == typeof(IActorTraitGrain).Name) return factory.GetGrain<IActorTraitGrain>(grainId);
            else if (grainTarget == typeof(IAnecdoteGrain).Name) return factory.GetGrain<IAnecdoteGrain>(grainId);
            else if (grainTarget == typeof(IPersonGrain).Name) return factory.GetGrain<IPersonGrain>(grainId);
            else if (grainTarget == typeof(ISettlementGrain).Name) return factory.GetGrain<ISettlementGrain>(grainId);
            else if (grainTarget == typeof(IWorldGrain).Name) return factory.GetGrain<IWorldGrain>(grainId);
            else if (grainTarget == typeof(IActorPluginGrain).Name) return factory.GetGrain<IActorPluginGrain>(grainId);
            else if (grainTarget == typeof(IAnecdotePluginGrain).Name) return factory.GetGrain<IAnecdotePluginGrain>(grainId);
            else if (grainTarget == typeof(IPersonPluginGrain).Name) return factory.GetGrain<IPersonPluginGrain>(grainId);
            else if (grainTarget == typeof(IWorldPluginGrain).Name) return factory.GetGrain<IWorldPluginGrain>(grainId);

            throw new InvalidOperationException($"Command target grain {grainTarget}:{grainId} is not known or recognized");
        }

        private ExecuteCommand Map(Command cmd, int chapter, int page)
        {
            return new ExecuteCommand
            {
                TaleId = _taleId,
                TaleVersionId = _taleVersionId,
                OperationTime = _operationTime,
                WriterId = _writerId,
                ChapterId = chapter,
                PageId = page,
                Index = cmd.Index,
                Phase = cmd.Phase,
                Tag = cmd.Tag,
                SubIndex = cmd.SubIndex,
                TargetGrainId = cmd.GrainId,
                TargetGrainType = cmd.GrainType,
                Target = cmd.Target
            };
        }

        protected override void OnDispose()
        {
            _taskMgr?.Dispose();
        }
    }
}

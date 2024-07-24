using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Contracts;
using Talepreter.Contracts.Process;
using Talepreter.Exceptions;
using Talepreter.Operations.Plugin;

namespace Talepreter.Operations.Processing
{
    public abstract class BaseCommandProcessor<TDbContext> : ICommandProcessor
        where TDbContext : IDbContext
    {
        protected readonly ILogger<BaseCommandProcessor<TDbContext>> _logger;
        protected readonly IGrainFactory _grainFactory;
        protected readonly ResponsibleService _responsibleService;

        public BaseCommandProcessor(IGrainFactory grainFactory, ILogger<BaseCommandProcessor<TDbContext>> logger, ITalepreterServiceIdentifier serviceIdentifier)
        {
            _logger = logger;
            _grainFactory = grainFactory;

            _responsibleService = serviceIdentifier.ServiceId.Map();
        }

        /// <summary>
        /// sets grain info to the command, could be anything service related, called only if service is interested in it
        /// default grain id is TaleVersionId\TAG:Target
        /// </summary>
        public abstract void SetGrainInfo(Command command);

        // --

        /// <summary>
        /// by default no service is interested in a message, every svc must define it
        /// </summary>
        protected abstract Task<Command[]?> IsInterested(ProcessCommand message, TDbContext dbContext, CancellationToken token);

        /// <summary>
        /// Validate command and throw exception if it is not acceptable
        /// </summary>
        protected virtual Task ValidateCommand(ProcessCommand command, TDbContext dbContext, CancellationToken token)
        {
            if (command.TaleId == Guid.Empty) throw new CommandValidationException(command, "Tale id is empty");
            if (command.TaleVersionId == Guid.Empty) throw new CommandValidationException(command, "Tale version is empty");
            if (command.WriterId == Guid.Empty) throw new CommandValidationException(command, "Writer is empty");
            if (command.ChapterId < 0) throw new CommandValidationException(command, "Chapter is negative");
            if (command.PageId < 0) throw new CommandValidationException(command, "Page is negative");
            if (command.Index < 0) throw new CommandValidationException(command, "Index is negative");

            return Task.CompletedTask;
        }

        public async Task Process(ProcessCommand message, IServiceScope scope, CancellationToken token)
        {
            using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>() ?? throw new CommandProcessingException($"{typeof(TDbContext).Name} initialization failed"); ;
            ArgumentNullException.ThrowIfNull(message, nameof(message));

            token.ThrowIfCancellationRequested();
            if (message.Tag == CommandIds.Trigger)
            {
                _logger.LogCritical($"Trigger command is internal use command and it cannot come from outside. Command is rejected: '{message}'");
                throw new CommandValidationException(message, "Trigger command is internal use command and it cannot come from outside");
            }

            try
            {
                token.ThrowIfCancellationRequested();

                var count = await dbContext.ExecuteAwaitingCommands(message.TaleId, message.TaleVersionId, message.ChapterId, message.PageId, message.Phase).Where(x => x.Index == message.Index).CountAsync(token);
                if (count > 0)
                {
                    // duplicate message or suspicious entry, they should not exist
                    _logger.LogWarning($"Processing message {message} is skipped because there are already commands in DB");
                    return;
                }

                _logger.LogDebug($"Processing message {message}");

                await ValidateCommand(message, dbContext, token);

                token.ThrowIfCancellationRequested();

                var baseCommands = await IsInterested(message, dbContext, token) ?? [];

                token.ThrowIfCancellationRequested();

                var pluginCommands = new List<Command>();
                var plugins = scope.ServiceProvider.GetServices<IPluginCommandProcessor<TDbContext>>();
                foreach (var plugin in plugins)
                {
                    token.ThrowIfCancellationRequested();

                    await plugin.ValidateCommand(message, dbContext, _logger, token);

                    var list = await plugin.CustomCommands(message, dbContext, _logger, token);
                    if (list != null) pluginCommands.AddRange(list);
                }

                token.ThrowIfCancellationRequested();

                _logger.LogDebug($"Processing message {message} collected {baseCommands?.Length ?? 0} base and {pluginCommands.Count} subcommands");
                var totalList = new List<Command>(baseCommands ?? []);
                if (baseCommands != null) foreach (var baseCmd in baseCommands) SetGrainInfo(baseCmd);

                if (pluginCommands.Count > 0) totalList.AddRange(pluginCommands);

                if (totalList.Count > 0) // do we have anything to save?
                {
                    _logger.LogDebug($"Processing message {message} adds {totalList.Count} commands to page to execute");
                    dbContext.Commands.AddRange(totalList);
                    await dbContext.SaveChangesAsync(token); // << check this one to see if it will create db concurrency exception
                }
                else _logger.LogDebug($"Processing message {message} yielded no commands to execute, skipping");
            }
            catch (CommandValidationException ex)
            {
                _logger.LogError($"Command validation failed for message {message}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Processing message {message} failed with error: {ex.Message}");
                throw;
            }
        }
    }
}

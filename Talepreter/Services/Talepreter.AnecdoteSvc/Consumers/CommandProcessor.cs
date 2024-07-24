using Microsoft.EntityFrameworkCore;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Contracts.Process;
using Talepreter.Operations;
using Talepreter.Operations.Processing;

namespace Talepreter.AnecdoteSvc.Consumers
{
    public class CommandProcessor : BaseCommandProcessor<AnecdoteSvcDBContext>
    {
        public CommandProcessor(IGrainFactory grainFactory, ILogger<BaseCommandProcessor<AnecdoteSvcDBContext>> logger, ITalepreterServiceIdentifier serviceIdentifier)
            : base(grainFactory, logger, serviceIdentifier)
        {
        }

        protected override async Task<Command[]?> IsInterested(ProcessCommand command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<Command> commands = [];
            switch (command.Tag)
            {
                case CommandIds.Page:
                    // currently there is nothing here but plugins might use it
                    var currentDate = command.BlockInfo.GetEndDate();
                    var triggers = await dbContext.GetActiveTriggersBefore(command.TaleId, command.TaleVersionId, currentDate).ToArrayAsync(token);
                    commands.AddRange(triggers.Select(command.MapTrigger));
                    break;
                case CommandIds.Anecdote:
                case CommandIds.Settlement:
                case CommandIds.Actor:
                    commands.Add(command.Map());
                    break;
                default: break;
            }

            return [.. commands];
        }

        public override void SetGrainInfo(Command command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            switch (command.Tag)
            {
                case CommandIds.Anecdote:
                    command.GrainType = typeof(IAnecdoteGrain).Name;
                    command.GrainId = GrainFetcher.FetchAnecdote(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Actor:
                    command.GrainType = typeof(IAnecdotePluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchActor(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Settlement:
                    command.GrainType = typeof(IAnecdotePluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchSettlement(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Trigger: break; // these are already set when triggers are created
                default: throw new InvalidOperationException($"Anecdote svc does not know how to set grain for command {command.Tag}");
            }
        }

        public override string ToString() => "AnecdoteCommandProcessor";

        protected override async Task ValidateCommand(ProcessCommand command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            await base.ValidateCommand(command, dbContext, token);
            token.ThrowIfCancellationRequested();

            switch (command.Tag)
            {
                case CommandIds.Anecdote: BasicValidationSchemes.Anecdote(command); break;
                case CommandIds.Actor: BasicValidationSchemes.Actor(command); break;
                case CommandIds.Settlement: BasicValidationSchemes.Settlement(command); break;
                case CommandIds.Trigger: BasicValidationSchemes.Trigger(command); break;
                default: return;
            }
        }
    }
}

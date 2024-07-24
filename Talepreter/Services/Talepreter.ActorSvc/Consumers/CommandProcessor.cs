using Microsoft.EntityFrameworkCore;
using Talepreter.ActorSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Contracts.Process;
using Talepreter.Exceptions;
using Talepreter.Operations;
using Talepreter.Operations.Processing;

namespace Talepreter.ActorSvc.Consumers
{
    public class CommandProcessor : BaseCommandProcessor<ActorSvcDBContext>
    {
        public CommandProcessor(IGrainFactory grainFactory, ILogger<BaseCommandProcessor<ActorSvcDBContext>> logger, ITalepreterServiceIdentifier serviceIdentifier)
            : base(grainFactory, logger, serviceIdentifier)
        {
        }

        protected override async Task<Command[]?> IsInterested(ProcessCommand command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<Command> commands = [];
            switch (command.Tag)
            {
                // NOTE: this operation below is the reason and intention of this app actually, to handle from single command to alter many entities automatically
                // the plugins will do more tasks and alter more things, check more than just simple properties
                // right now as seen below, the app tracks which actors are mentioned or about to die reaching their natural lifespan limits
                case CommandIds.Page:
                    // set seen location/date for all mentioned actors
                    var actorCommands = await SetSeenForActors(command, dbContext, token);
                    if (actorCommands != null) commands.AddRange(actorCommands);

                    // setup commands for ALL actors who are supposed to die and ALL actor traits who are supposed to expire
                    var currentDate = command.BlockInfo.GetEndDate();
                    var triggers = await dbContext.GetActiveTriggersBefore(command.TaleId, command.TaleVersionId, currentDate).ToArrayAsync(token);
                    commands.AddRange(triggers.Select(command.MapTrigger));
                    break;
                case CommandIds.Settlement:
                case CommandIds.Actor:
                case CommandIds.ActorTrait:
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
                case CommandIds.Page:
                    command.GrainType = typeof(IActorPluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchPage(command.TaleId, command.TaleVersionId, command.ChapterId, command.PageId);
                    break;
                case CommandIds.Actor:
                    command.GrainType = typeof(IActorGrain).Name;
                    command.GrainId = GrainFetcher.FetchActor(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.ActorTrait:
                    if (command.Parent == null) throw new CommandValidationException(command, "ACTORTRAIT command has no parent set");
                    command.GrainType = typeof(IActorTraitGrain).Name;
                    command.GrainId = GrainFetcher.FetchActorTrait(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Settlement:
                    command.GrainType = typeof(IActorPluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchSettlement(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Trigger: break; // these are already set when triggers are created
                default: throw new InvalidOperationException($"Actor svc does not know how to set grain for command {command.Tag}");
            }
        }

        public override string ToString() => "ActorCommandProcessor";

        protected override async Task ValidateCommand(ProcessCommand command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            await base.ValidateCommand(command, dbContext, token);

            switch (command.Tag)
            {
                case CommandIds.Page: BasicValidationSchemes.Page(command); break;
                case CommandIds.Actor: BasicValidationSchemes.Actor(command); break;
                case CommandIds.ActorTrait: BasicValidationSchemes.ActorTrait(command); break;
                case CommandIds.Settlement: BasicValidationSchemes.Settlement(command); break;
                case CommandIds.Trigger: BasicValidationSchemes.Trigger(command); break;
                default: return;
            }
        }

        private async Task<Command[]?> SetSeenForActors(ProcessCommand command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var actorMention = command.NamedParameters!.FirstOrDefault(x => x.Name == CommandIds.PageCommand.MetActors);
            if (actorMention == null) return null!;

            // TODO move this logic elsewhere perhaps? actor list is always comma separated strings
            var actorList = actorMention.Value.SplitInto(",");
            var allActors = await dbContext.Actors.OfTale(command).Where(x => actorList.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(token);
            if (actorList.Any(x => !allActors.Contains(x))) throw new CommandValidationException(command, "Page command actor list contains some actors which are not recognized");

            token.ThrowIfCancellationRequested();

            // location is a string, date is long, and we also add stay duration too, but not travel location by default
            var seenLocation = command.BlockInfo.Location;
            var seenDate = command.BlockInfo.Date + command.BlockInfo.Stay;
            // minor issue: seenData might exceed expected lifespan of actor. we will check it on execution level, not here
            var commands = actorList.Select(actor => new Command
            {
                TaleId = command.TaleId,
                TaleVersionId = command.TaleVersionId,
                WriterId = command.WriterId,
                OperationTime = command.OperationTime,
                ChapterId = command.ChapterId,
                PageId = command.PageId,
                Phase = command.Phase,
                GrainId = GrainFetcher.FetchActor(command.TaleId, command.TaleVersionId, actor),
                GrainType = typeof(IActorGrain).Name,
                Index = command.Index,
                Prequisite = 0,
                HasChild = false,
                Tag = CommandIds.Actor,
                Target = actor,
                Parent = null,
                ArrayParameters = null,
                Comments = null,
                NamedParameters =
                        [
                            BaseTypes.NamedParameter.CreateL(CommandIds.ActorCommand.SeenDate, value: seenDate),
                            BaseTypes.NamedParameter.Create(CommandIds.ActorCommand.SeenLocation, value: seenLocation)
                        ],
                Result = CommandExecutionResult.None,
                Attempts = 0,
                Error = null
            });

            return [.. commands];
        }
    }
}
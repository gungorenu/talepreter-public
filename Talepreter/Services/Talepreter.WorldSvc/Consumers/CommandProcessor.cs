using Talepreter.WorldSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Process;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Exceptions;
using Talepreter.Common;
using Talepreter.Operations.Processing;
using Talepreter.Operations;
using Microsoft.EntityFrameworkCore;

namespace Talepreter.WorldSvc.Consumers
{
    public class CommandProcessor : BaseCommandProcessor<WorldSvcDBContext>
    {
        public CommandProcessor(IGrainFactory grainFactory, ILogger<BaseCommandProcessor<WorldSvcDBContext>> logger, ITalepreterServiceIdentifier serviceIdentifier)
            : base(grainFactory, logger, serviceIdentifier)
        {
        }

        protected override async Task<Command[]?> IsInterested(ProcessCommand command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<Command> commands = [];
            switch (command.Tag)
            {
                case CommandIds.World:
                case CommandIds.Settlement:
                case CommandIds.Chapter:
                    commands.Add(command.Map());
                    break;
                case CommandIds.Page:
                    commands.Add(command.MapPage());
                    var result = SetSettlementVisit(command, token);
                    if (result != null) commands.Add(result);

                    // this is special case, worldsvc as is does not have anything but plugins do so we add this here to support plugin added triggers
                    var currentDate = command.BlockInfo.GetEndDate();
                    var triggers = await dbContext.GetActiveTriggersBefore(command.TaleId, command.TaleVersionId, currentDate).ToArrayAsync(token);
                    commands.AddRange(triggers.Select(command.MapTrigger));

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
                // yes, this is correct, chapter and page grains cannot process commands, they are different
                case CommandIds.World:
                case CommandIds.Chapter:
                case CommandIds.Page:
                    command.GrainType = typeof(IWorldGrain).Name;
                    command.GrainId = GrainFetcher.FetchWorld(command.TaleId, command.TaleVersionId);
                    break;
                case CommandIds.Settlement:
                    command.GrainType = typeof(ISettlementGrain).Name;
                    command.GrainId = GrainFetcher.FetchSettlement(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Trigger: break; // these are already set when triggers are created
                default: throw new InvalidOperationException($"World svc does not know how to set grain for command {command.Tag}");
            }
        }

        public override string ToString() => "WorldCommandProcessor";

        protected override async Task ValidateCommand(ProcessCommand command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            await base.ValidateCommand(command, dbContext, token);
            token.ThrowIfCancellationRequested();

            switch (command.Tag)
            {
                case CommandIds.World: BasicValidationSchemes.World(command); break;
                case CommandIds.Settlement: BasicValidationSchemes.Settlement(command); break;
                case CommandIds.Chapter: BasicValidationSchemes.Chapter(command); break;
                case CommandIds.Page: BasicValidationSchemes.Page(command); await ValidatePageCommand(command, dbContext, token); break;
                case CommandIds.Trigger: BasicValidationSchemes.Trigger(command); break;
                default: return;
            }
        }

        private Command? SetSettlementVisit(ProcessCommand command, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var currentDate = command.BlockInfo.Date + command.BlockInfo.Stay;

            var location = command.BlockInfo.Location;
            if (location == null) return null;

            Location? stayPlace = null;
            var args = location.SplitInto(",");
            if (args.Length == 2) stayPlace = new Location { Settlement = args[0], Extension = args[1] };
            else if (args.Length == 1) stayPlace = new Location { Settlement = args[0], Extension = null };
            else throw new CommandValidationException(command, "Page location value is not acceptable");

            if (string.IsNullOrEmpty(stayPlace.Settlement)) throw new CommandValidationException(command, "Page location value is empty");
            if (!string.IsNullOrEmpty(stayPlace.Extension)) return null; // this means it is actually not at settlement but a direction like "Stockholm, north east" which means Stockholm is not visited

            return new Command
            {
                TaleId = command.TaleId,
                TaleVersionId = command.TaleVersionId,
                WriterId = command.WriterId,
                OperationTime = command.OperationTime,
                ChapterId = command.ChapterId,
                PageId = command.PageId,
                Phase = command.Phase,
                GrainId = GrainFetcher.FetchSettlement(command.TaleId, command.TaleVersionId, stayPlace.Settlement),
                GrainType = typeof(ISettlementGrain).Name,
                Index = command.Index,
                Prequisite = 0,
                HasChild = false,
                Tag = CommandIds.Settlement,
                Target = stayPlace.Settlement,
                Parent = null,
                ArrayParameters = null,
                Comments = null,
                NamedParameters = [BaseTypes.NamedParameter.Create(CommandIds.SettlementCommand.Visited, value: $"{command.BlockInfo.Date} {currentDate}")],
                Result = CommandExecutionResult.None,
                Attempts = 0,
                Error = null
            };
        }

        private async Task ValidatePageCommand(ProcessCommand message, WorldSvcDBContext dbContext, CancellationToken token)
        {
            if (message.ChapterId == 0 && message.PageId == 0) return;
            if (message.ChapterId < 0 && message.PageId < 0) throw new CommandValidationException(message, $"Page {message.ChapterId}#{message.PageId} has negative values for chapter/page");

            Page? lastPage;

            if (message.PageId == 0) // look back to previous chapter
            {
                lastPage = await dbContext.Pages.OfTale(message).Where(x => x.LastUpdatedChapter == message.ChapterId - 1)
                    .OrderByDescending(x => x.LastUpdatedPageInChapter).FirstOrDefaultAsync(token) ?? throw new CommandValidationException(message, $"Chapter {message.ChapterId - 1} has no page");
            }
            else
            {
                lastPage = await dbContext.Pages.OfTale(message).Where(x => x.LastUpdatedChapter == message.ChapterId && x.LastUpdatedPageInChapter == message.PageId - 1)
                    .OrderByDescending(x => x.LastUpdatedPageInChapter).FirstOrDefaultAsync(token)
                    ?? throw new CommandValidationException(message, $"Chapter {message.ChapterId - 1} has no page");
            }

            if (lastPage == null) throw new CommandValidationException(message, $"Previous page before {message.ChapterId}#{message.PageId} could not be found");

            long recordedLastDate = lastPage.StartDate + lastPage.StayAtLocation + (lastPage.Travel?.Duration ?? 0);
            Location recordedLastLocation = lastPage.Travel?.Destination ?? lastPage.Location;

            if (message.BlockInfo.Date != recordedLastDate) throw new CommandValidationException(message, $"Page {message.ChapterId}#{message.PageId} jumped in date");
            if (recordedLastLocation.ToString() != message.BlockInfo.Location) throw new CommandValidationException(message, $"Page {message.ChapterId}#{message.PageId} jumped in location");
        }
    }
}

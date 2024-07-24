using Talepreter.PersonSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Contracts.Process;
using Talepreter.Common;
using Talepreter.Exceptions;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Operations.Processing;
using Talepreter.Operations;
using Microsoft.EntityFrameworkCore;

namespace Talepreter.PersonSvc.Consumers
{
    public class CommandProcessor : BaseCommandProcessor<PersonSvcDBContext>
    {
        public CommandProcessor(IGrainFactory grainFactory, ILogger<BaseCommandProcessor<PersonSvcDBContext>> logger, ITalepreterServiceIdentifier serviceIdentifier)
            : base(grainFactory, logger, serviceIdentifier)
        {
        }

        protected override async Task<Command[]?> IsInterested(ProcessCommand command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<Command> commands = [];
            switch (command.Tag)
            {
                case CommandIds.Settlement:
                case CommandIds.Person:
                    commands.Add(command.Map());
                    break;
                // check comment on ActorSvc same name command processor
                case CommandIds.Page:
                    // set seen location/date for all mentioned persons
                    var personCommands = await SetSeenForPersons(command, dbContext, token);
                    if (personCommands != null) commands.AddRange(personCommands);

                    // setup commands for ALL persons who are supposed to die
                    var currentDate = command.BlockInfo.GetEndDate();
                    var triggers = dbContext.GetActiveTriggersBefore(command.TaleId, command.TaleVersionId, currentDate).ToArray();
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
                case CommandIds.Settlement:
                    command.GrainType = typeof(IPersonPluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchSettlement(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Page:
                    command.GrainType = typeof(IPersonPluginGrain).Name;
                    command.GrainId = GrainFetcher.FetchPage(command.TaleId, command.TaleVersionId, command.ChapterId, command.PageId);
                    break;
                case CommandIds.Person:
                    command.GrainType = typeof(IPersonGrain).Name;
                    command.GrainId = GrainFetcher.FetchPerson(command.TaleId, command.TaleVersionId, command.Target);
                    break;
                case CommandIds.Trigger: break; // these are already set when triggers are created
                default: throw new InvalidOperationException($"Person svc does not know how to set grain for command {command.Tag}");
            }
        }

        public override string ToString() => "PersonCommandProcessor";

        protected override async Task ValidateCommand(ProcessCommand command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            await base.ValidateCommand(command, dbContext, token);
            token.ThrowIfCancellationRequested();

            switch (command.Tag)
            {
                case CommandIds.Person: BasicValidationSchemes.Person(command); break;
                case CommandIds.Page: BasicValidationSchemes.Page(command); break;
                case CommandIds.Settlement: BasicValidationSchemes.Settlement(command); break;
                case CommandIds.Trigger: BasicValidationSchemes.Trigger(command); break;
                default: return;
            }
        }

        private async Task<Command[]?> SetSeenForPersons(ProcessCommand command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            // command is already validated

            var personMention = command.NamedParameters!.FirstOrDefault(x => x.Name == CommandIds.PageCommand.MetPersons);
            if (personMention == null) return null;

            // TODO move this logic elsewhere perhaps? person list is always comma separated strings
            var personList = personMention.Value.SplitInto(",");
            var allPersons = await dbContext.Persons.OfTale(command).Where(x => personList.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(token);
            if (personList.Any(x => !allPersons.Contains(x))) throw new CommandValidationException(command, "Page command person list contains some persons which are not recognized");

            // location is a string, date is long, and we also add stay duration too, but not travel location by default
            var seenLocation = command.BlockInfo.Location;
            var seenDate = command.BlockInfo.Date + command.BlockInfo.Stay;
            // minor issue: seenData might exceed expected lifespan of person. we will check it on execution level, not here
            var commands = personList.Select(person => new Command
            {
                TaleId = command.TaleId,
                TaleVersionId = command.TaleVersionId,
                WriterId = command.WriterId,
                OperationTime = command.OperationTime,
                ChapterId = command.ChapterId,
                PageId = command.PageId,
                Phase = command.Phase,
                GrainId = GrainFetcher.FetchPerson(command.TaleId, command.TaleVersionId, person),
                GrainType = typeof(IPersonGrain).Name,
                Index = command.Index,
                Prequisite = 0,
                HasChild = false,
                Tag = CommandIds.Person,
                Target = person,
                Parent = null,
                ArrayParameters = null,
                Comments = null,
                NamedParameters = [BaseTypes.NamedParameter.CreateL(CommandIds.PersonCommand.SeenDate, value: seenDate), BaseTypes.NamedParameter.Create(CommandIds.PersonCommand.SeenLocation, value: seenLocation)],
                Result = CommandExecutionResult.None,
                Attempts = 0,
                Error = null
            });

            return [.. commands];
        }
    }
}

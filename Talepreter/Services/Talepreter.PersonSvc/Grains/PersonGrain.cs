using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.PersonSvc.DBContext;
using Talepreter.Common;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Operations;
using Talepreter.Contracts.Execute.Events;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.PersonSvc.Grains
{
    public class PersonGrain : EntityGrain<PersonGrainState, PersonSvcDBContext, IPersonGrain>, IPersonGrain
    {
        public PersonGrain([PersistentState("persistentState", "PersonSvcStorage")] IPersistentState<PersonGrainState> persistentState,
            ILogger<PersonGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchPersonContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var persons = await dbContext.Persons.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (persons.Length > 1) throw new CommandExecutionException(command, "More than one person found with unique key");
            if (persons.Length < 1) throw new CommandExecutionException(command, "Person not found for the trigger");
            if (command.NamedParameters == null) throw new CommandValidationException(command, "Trigger command has no named parameter");

            var triggerId = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Id);
            if (triggerId == null || string.IsNullOrEmpty(triggerId.Value)) throw new CommandValidationException(command, "Trigger command has no trigger id");
            var triggerType = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Type);
            if (triggerType == null || string.IsNullOrEmpty(triggerType.Value)) throw new CommandValidationException(command, "Trigger command has no trigger type set");
            var trigger = await dbContext.Triggers.Where(x => x.TaleId == command.TaleId && x.TaleVersionId == command.TaleVersionId && x.Id == triggerId.Value).FirstAsync(token);

            token.ThrowIfCancellationRequested();

            Person person = persons[0];
            person = persons[0];
            person.LastUpdatedChapter = command.ChapterId;
            person.LastUpdatedPageInChapter = command.PageId;
            person.LastUpdate = command.OperationTime;
            person.WriterId = command.WriterId;

            switch (triggerType.Value)
            {
                // only one here
                case CommandIds.TriggerCommand.TriggerList.PersonDeath:
                    if (person.ExpireState == ExpirationStates.Timeless || person.ExpireState == ExpirationStates.Expired)
                    {
                        _logger.LogCritical($"Person {person.Id} got trigger of {CommandIds.TriggerCommand.TriggerList.PersonDeath} but expiration state was {person.ExpireState}, which could be a system failure");
                        trigger.State = TriggerState.Invalid;
                        return person;
                    }
                    person.ExpireState = ExpirationStates.Expired;
                    person.ExpiredAt = trigger.TriggerAt;
                    _publisher.Publish(command.MapEvent(EventIds.PersonDeath, trigger.TriggerAt, (args) => args[EventIds.PersonDeathParameters.PersonId] = person.Id),
                        TalepreterTopology.EventExchange, TalepreterTopology.EventRoutingKey);
                    trigger.State = TriggerState.Triggered;
                    break;
                default: break; // we do not know this trigger, maybe from plugins
            }

            return person;
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var persons = await dbContext.Persons.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (persons.Length > 1) throw new CommandExecutionException(command, "More than one person found with unique key");
            Person person = null!;
            if (persons.Length == 1)
            {
                person = persons[0];
                person.LastUpdatedChapter = command.ChapterId;
                person.LastUpdatedPageInChapter = command.PageId;
                person.LastUpdate = command.OperationTime;
                person.WriterId = command.WriterId;
            }
            else
            {
                if (command.Tag != CommandIds.Person) throw new CommandExecutionException(command, "Person entity must exist before executing a plugin command");

                person = new Person
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    ExpireState = ExpirationStates.Timeless,
                    Notes = new NotesMetadata { List = new List<NoteEntry>() },
                    Tags = [],
                    IsNew = true
                };
                dbContext.Persons.Add(person);
            }

            token.ThrowIfCancellationRequested();

            // only Person command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Person) return person;

            // this is a special case, it allows us add comments without updating seen location/date
            var isSilent = false;
            if (command.NamedParameters != null)
            {
                var par = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.PersonCommand.Silent);
                if (par != null) isSilent = true;
            }

            long? expectedDeath = null, diedAt = null;

            if (command.NamedParameters != null) // apply named parameters
            {
                foreach (var namedParam in command.NamedParameters)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        switch (namedParam.Name)
                        {
                            case CommandIds.PersonCommand.Birth:
                                person.ExpireState = ExpirationStates.Alive;
                                person.StartsAt = namedParam.Value.ToLong();
                                break;
                            case CommandIds.PersonCommand.DiedAt:
                                if (person.ExpireState == ExpirationStates.Expired) continue; // a special case, if died then cannot die again
                                person.ExpireState = ExpirationStates.Expired;
                                if (person.ExpiresAt != null) person.ExpiredAt = diedAt = Math.Min(namedParam.Value.ToLong(), person.ExpiresAt.Value);
                                else person.ExpiredAt = diedAt = namedParam.Value.ToLong();
                                break;
                            case CommandIds.PersonCommand.ExpectedDeath:
                                person.ExpireState = ExpirationStates.Alive;
                                person.ExpiresAt = expectedDeath = namedParam.Value.ToLong();
                                break;
                            case CommandIds.PersonCommand.Identity:
                                person.Identity = namedParam.Value;
                                break;
                            case CommandIds.PersonCommand.Physics:
                                person.Physics = namedParam.Value;
                                break;
                            case CommandIds.PersonCommand.SeenDate:
                                if (isSilent) throw new CommandExecutionException(command, "Person cannot be silent and seen");
                                var currentDate = namedParam.Value.ToLong();
                                if (currentDate < (person.LastSeen ?? 0L))
                                    throw new CommandExecutionException(command, "Person last seen date cannot be changed towards past");
                                person.LastSeen = currentDate;
                                break;
                            case CommandIds.PersonCommand.SeenLocation: // this is a special pattern, comma separated, settlement name is first one, ex: "Stockholm, East of <SETTLEMENT>", during publishing this information can be swapped by GUI
                                if (isSilent) throw new CommandExecutionException(command, "Person cannot be silent and seen");
                                person.LastSeenLocation = await namedParam.ParseLocation(command, dbContext, token);
                                break;
                            case CommandIds.PersonCommand.Tags:
                                var tags = namedParam.GetArray(',');
                                switch (namedParam.Type)
                                {
                                    case NamedParameterType.Set:
                                        person.Tags = [.. tags];
                                        break;
                                    case NamedParameterType.Add:
                                        var tempAdd = new List<string>(person.Tags);
                                        foreach (var entry in tags) tempAdd.Add(entry);
                                        person.Tags = [.. tempAdd];
                                        break;
                                    case NamedParameterType.Remove:
                                        var tempRemove = new List<string>(person.Tags);
                                        foreach (var entry in tags) tempRemove.Remove(entry);
                                        person.Tags = [.. tempRemove];
                                        break;
                                    default:
                                        person.Tags = [];
                                        break;
                                }
                                break;
                            case CommandIds.PersonCommand.Timeless:
                                person.ExpireState = ExpirationStates.Timeless;
                                break;
                            default: continue; // other parameters are plugin stuff
                        }
                    }
                    catch (Exception ex) // dirty trick to pick validation error
                    {
                        throw new CommandValidationException(command, ex.Message);
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            var triggerId = CommandIds.TriggerCommand.CreateTriggerIdForPersonDeath(person.Id);
            if (expectedDeath != null && diedAt == null)
            {
                // update trigger time
                var updated = await dbContext.UpdateTrigger(command.TaleId, command.TaleVersionId, triggerId, expectedDeath.Value, token);
                if (!updated) dbContext.Triggers.Add(command.MapTrigger<IPersonGrain>(triggerId, expectedDeath!.Value, person.Id,
                    GrainFetcher.FetchPerson(command.TaleId, command.TaleVersionId, person.Id), CommandIds.TriggerCommand.TriggerList.PersonDeath, null));
            }
            else if (diedAt != null || person.ExpireState == ExpirationStates.Timeless)
                await dbContext.CancelTrigger(command.TaleId, command.TaleVersionId, triggerId, token);

            if (!string.IsNullOrEmpty(command.Comments))
            {
                person.Notes!.List.Add(new NoteEntry
                {
                    Chapter = command.ChapterId,
                    Page = command.PageId,
                    Notes = command.Comments
                });
            }

            return person;
        }
    }

    [GenerateSerializer]
    public class PersonGrainState : EntityGrainStateBase
    {
    }
}

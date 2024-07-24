using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.ActorSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Execute.Events;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.Operations;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;

namespace Talepreter.ActorSvc.Grains
{
    public class ActorGrain : EntityGrain<ActorGrainState, ActorSvcDBContext, IActorGrain>, IActorGrain
    {
        public ActorGrain([PersistentState("persistentState", "ActorSvcStorage")] IPersistentState<ActorGrainState> persistentState,
            ILogger<ActorGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchActorContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var actors = await dbContext.Actors.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (actors.Length > 1) throw new CommandExecutionException(command, "More than one actor found with unique key");
            if (actors.Length < 1) throw new CommandExecutionException(command, "Actor not found for the trigger");
            if (command.NamedParameters == null) throw new CommandValidationException(command, "Trigger command has no named parameter");

            token.ThrowIfCancellationRequested();

            var triggerId = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Id);
            if (triggerId == null || string.IsNullOrEmpty(triggerId.Value)) throw new CommandValidationException(command, "Trigger command has no trigger id");
            var triggerType = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Type);
            if (triggerType == null || string.IsNullOrEmpty(triggerType.Value)) throw new CommandValidationException(command, "Trigger command has no trigger type set");
            var trigger = await dbContext.Triggers.Where(x => x.TaleId == command.TaleId && x.TaleVersionId == command.TaleVersionId && x.Id == triggerId.Value).FirstAsync(token);

            Actor actor = actors[0];
            actor = actors[0];
            actor.LastUpdatedChapter = command.ChapterId;
            actor.LastUpdatedPageInChapter = command.PageId;
            actor.LastUpdate = command.OperationTime;
            actor.WriterId = command.WriterId;

            switch (triggerType.Value)
            {
                // only one here
                case CommandIds.TriggerCommand.TriggerList.ActorDeath:
                    if (actor.ExpireState == ExpirationStates.Timeless || actor.ExpireState == ExpirationStates.Expired)
                    {
                        _logger.LogCritical($"Actor {actor.Id} got trigger of {CommandIds.TriggerCommand.TriggerList.ActorDeath} but expiration state was {actor.ExpireState}, which could be a system failure");
                        trigger.State = TriggerState.Invalid;
                        return actor;
                    }
                    actor.ExpireState = ExpirationStates.Expired;
                    actor.ExpiredAt = trigger.TriggerAt;
                    _publisher.Publish(command.MapEvent(EventIds.ActorDeath, trigger.TriggerAt, (args) => args[EventIds.ActorDeathParameters.ActorId] = actor.Id),
                        TalepreterTopology.EventExchange, TalepreterTopology.EventRoutingKey);

                    trigger.State = TriggerState.Triggered;
                    break;
                default: break; // we do not know this trigger, maybe from plugins
            }

            return actor;
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var actors = await dbContext.Actors.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (actors.Length > 1) throw new CommandExecutionException(command, "More than one actor found with unique key");
            Actor actor = null!;
            if (actors.Length == 1)
            {
                actor = actors[0];
                actor.LastUpdatedChapter = command.ChapterId;
                actor.LastUpdatedPageInChapter = command.PageId;
                actor.LastUpdate = command.OperationTime;
                actor.WriterId = command.WriterId;
            }
            else
            {
                if (command.Tag != CommandIds.Actor) throw new CommandExecutionException(command, "Actor entity must exist before executing a plugin command");

                actor = new Actor
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    ExpireState = ExpirationStates.Timeless,
                    Notes = new ActorNotesMetadata { List = new List<ActorNoteEntry>() },
                    IsNew = true
                };
                dbContext.Actors.Add(actor);
            }

            // only Actor command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Actor) return actor;

            token.ThrowIfCancellationRequested();

            // example Actor command in my tale would be, and my GUI application will parse it to fit command properly:
            // - ACTOR: John : birth: 1234, expecteddeath: 5678, identity: { cool main guy, and some strong bastard }, seenlocation: { Stockholm, East of <SETTLEMENT> }, seendate: 1578, +note: Profession > John is a carpenter
            // I will not go through why format is like that, pure personal reasons.
            NamedParameterType? noteAction = null!;
            string? noteTitle = null!;

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
                            case CommandIds.ActorCommand.Birth:
                                actor.ExpireState = ExpirationStates.Alive;
                                actor.StartsAt = namedParam.Value.ToLong();
                                break;
                            case CommandIds.ActorCommand.DiedAt:
                                if (actor.ExpireState == ExpirationStates.Timeless) throw new CommandValidationException(command, "Actor is immortal, cannot be killed");
                                if (actor.ExpireState == ExpirationStates.Expired) throw new CommandValidationException(command, "Actor is dead, cannot be killed again");
                                actor.ExpireState = ExpirationStates.Expired;
                                if (actor.ExpiresAt != null) actor.ExpiredAt = diedAt = Math.Min(namedParam.Value.ToLong(), actor.ExpiresAt.Value);
                                else actor.ExpiredAt = diedAt = namedParam.Value.ToLong();
                                break;
                            case CommandIds.ActorCommand.ExpectedDeath:
                                actor.ExpireState = ExpirationStates.Alive;
                                actor.ExpiresAt = expectedDeath = namedParam.Value.ToLong();
                                break;
                            case CommandIds.ActorCommand.Identity:
                                actor.Identity = namedParam.Value;
                                break;
                            case CommandIds.ActorCommand.Physics:
                                actor.Physics = namedParam.Value;
                                break;
                            case CommandIds.ActorCommand.SeenDate:
                                var currentDate = namedParam.Value.ToLong();
                                if (currentDate < (actor.LastSeen ?? 0L))
                                    throw new CommandValidationException(command, "Actor last seen date cannot be changed towards past");
                                actor.LastSeen = currentDate;
                                break;
                            case CommandIds.ActorCommand.SeenLocation: // this is a special pattern, comma separated, settlement name is first one, ex: "Stockholm, East of <SETTLEMENT>", during publishing this information can be swapped by GUI
                                actor.LastSeenLocation = await namedParam.ParseLocation(command, dbContext, token);
                                break;
                            case CommandIds.ActorCommand.Note: // this works very weirdly but for a personal reason, you can check other comments
                                noteTitle = namedParam.Value;
                                noteAction = namedParam.Type;
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

            var triggerId = CommandIds.TriggerCommand.CreateTriggerIdForActorDeath(actor.Id);
            if (expectedDeath != null && diedAt == null)
            {
                // update trigger time
                var updated = await dbContext.UpdateTrigger(command.TaleId, command.TaleVersionId, triggerId, expectedDeath.Value, token);
                if (!updated) dbContext.Triggers.Add(command.MapTrigger<IActorGrain>(triggerId, expectedDeath.Value, actor.Id,
                    GrainFetcher.FetchActor(command.TaleId, command.TaleVersionId, actor.Id), CommandIds.TriggerCommand.TriggerList.ActorDeath, null));
            }
            else if (diedAt != null) await dbContext.CancelTrigger(command.TaleId, command.TaleVersionId, triggerId, token);

            token.ThrowIfCancellationRequested();

            // this is done like this due to personal reasons.
            if (noteTitle != null)
            {
                var currentChapterId = command.ChapterId;
                var existingNote = actor.Notes?.List.FirstOrDefault(x => x.Title == noteTitle);
                switch (noteAction.Value)
                {
                    case NamedParameterType.Add:
                        actor.Notes ??= new ActorNotesMetadata() { List = new List<ActorNoteEntry>() };
                        if (existingNote == null) actor.Notes.List.Add(new ActorNoteEntry { Title = noteTitle!, Notes = command.Comments ?? "" });
                        else
                        {
                            actor.Notes.List.Remove(existingNote);
                            actor.Notes.List.Add(new ActorNoteEntry { Title = noteTitle, Notes = $"{existingNote.Notes}\r\n{command.Comments ?? ""}".Trim() });
                        }
                        break;
                    case NamedParameterType.Remove:
                        if (existingNote != null) actor.Notes?.List.Remove(existingNote);
                        break;
                    case NamedParameterType.Reset:
                        actor.Notes = null;
                        break;
                    case NamedParameterType.Set:
                        actor.Notes ??= new ActorNotesMetadata() { List = new List<ActorNoteEntry>() };
                        if (existingNote != null) actor.Notes?.List.Remove(existingNote);
                        actor.Notes?.List.Add(new ActorNoteEntry { Title = noteTitle!, Notes = command.Comments ?? "" });
                        break;
                    default: throw new CommandValidationException(command, $"'{CommandIds.ActorCommand.Note}' named parameter type is unrecognized.");
                }
            }

            return actor;
        }
    }

    [GenerateSerializer]
    public class ActorGrainState : EntityGrainStateBase
    {
    }
}

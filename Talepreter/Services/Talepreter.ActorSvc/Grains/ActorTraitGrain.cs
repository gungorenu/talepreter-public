using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.ActorSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.Common;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Operations;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common.RabbitMQ;
using Talepreter.Contracts.Execute.Events;

namespace Talepreter.ActorSvc.Grains
{
    public class ActorTraitGrain : EntityGrain<ActorTraitGrainState, ActorSvcDBContext, IActorTraitGrain>, IActorTraitGrain
    {
        public ActorTraitGrain([PersistentState("persistentState", "ActorSvcStorage")] IPersistentState<ActorTraitGrainState> persistentState,
            ILogger<ActorTraitGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchActorContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var traits = await dbContext.Traits.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (traits.Length > 1) throw new CommandExecutionException(command, "More than one actor trait found with unique key");
            if (traits.Length < 1) throw new CommandExecutionException(command, "Actor trait not found for the trigger");
            if (command.NamedParameters == null) throw new CommandValidationException(command, "Trigger command has no named parameter");

            var triggerId = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Id);
            if (triggerId == null || string.IsNullOrEmpty(triggerId.Value)) throw new CommandValidationException(command, "Trigger command has no trigger id");
            var triggerType = command.NamedParameters.FirstOrDefault(x => x.Name == CommandIds.TriggerCommand.Type);
            if (triggerType == null || string.IsNullOrEmpty(triggerType.Value)) throw new CommandValidationException(command, "Trigger command has no trigger type set");
            var trigger = await dbContext.Triggers.Where(x => x.TaleId == command.TaleId && x.TaleVersionId == command.TaleVersionId && x.Id == triggerId.Value).FirstAsync(token);

            token.ThrowIfCancellationRequested();

            ActorTrait trait = traits[0];
            trait = traits[0];
            trait.LastUpdatedChapter = command.ChapterId;
            trait.LastUpdatedPageInChapter = command.PageId;
            trait.LastUpdate = command.OperationTime;
            trait.WriterId = command.WriterId;

            switch (triggerType.Value)
            {
                // only one here
                case CommandIds.TriggerCommand.TriggerList.ActorTraitExpire:
                    if (trait.ExpireState == ExpirationStates.Timeless || trait.ExpireState == ExpirationStates.Expired)
                    {
                        _logger.LogCritical($"Actor trait {trait.Id} got trigger of {CommandIds.TriggerCommand.TriggerList.ActorTraitExpire} but expiration state was {trait.ExpireState}, which could be a system failure");
                        trigger.State = TriggerState.Invalid;
                        return trait;
                    }
                    trait.ExpireState = ExpirationStates.Expired;
                    trait.ExpiredAt = trigger.TriggerAt;
                    _publisher.Publish(command.MapEvent(EventIds.ActorTraitExpiration, trigger.TriggerAt, (args) =>
                    {
                        args[EventIds.ActorTraitExpirationParameters.ActorTraitId] = trait.Id;
                        args[EventIds.ActorTraitExpirationParameters.ActorId] = trait.OwnerName;
                    }), TalepreterTopology.EventExchange, TalepreterTopology.EventRoutingKey);
                    trigger.State = TriggerState.Triggered;
                    break;
                default: break; // we do not know this trigger, maybe from plugins
            }

            return trait;
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var actorTraits = await dbContext.Traits.OfTale(command).Where(x => x.Id == command.Target).Include(x => x.Owner).ToArrayAsync(token);
            if (actorTraits.Length > 1) throw new CommandExecutionException(command, "More than one actor trait found with unique key");
            ActorTrait trait = null!;
            if (actorTraits.Length == 1)
            {
                trait = actorTraits[0];
                trait.LastUpdatedChapter = command.ChapterId;
                trait.LastUpdatedPageInChapter = command.PageId;
                trait.LastUpdate = command.OperationTime;
                trait.WriterId = command.WriterId;
            }
            else
            {
                if (command.Tag != CommandIds.ActorTrait) throw new CommandExecutionException(command, "Actor trait entity must exist before executing a plugin command");

                trait = new ActorTrait
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    ExpireState = ExpirationStates.Timeless,
                    IsNew = true
                };
                dbContext.Traits.Add(trait);
            }

            // changing ownership
            if (trait.OwnerName != command.Parent)
            {
                trait.OldOwnerName = trait.OwnerName;
                trait.OldOwner = trait.Owner;
                if (!string.IsNullOrEmpty(command.Parent))
                {
                    var actors = await dbContext.Actors.OfTale(command).Where(x => x.Id == command.Parent).ToArrayAsync(token);
                    if (actors.Length > 1) throw new CommandExecutionException(command, "More than one actor found with unique key");
                    if (actors.Length == 0) throw new CommandExecutionBlockedException(command, $"Actor {command.Parent} does not exist");
                    trait.Owner = actors[0];
                    trait.OwnerName = actors[0].Id;
                }
            }

            token.ThrowIfCancellationRequested();

            // only ActorTrait command is processed here, rest is on plugins
            if (command.Tag != CommandIds.ActorTrait) return trait;

            long? expectedExpire = null, expiredAt = null;

            if (command.NamedParameters != null) // apply named parameters
            {
                foreach (var namedParam in command.NamedParameters)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        switch (namedParam.Name)
                        {
                            case CommandIds.ActorTraitCommand.ExpectedExpire:
                                trait.ExpireState = ExpirationStates.Alive;
                                trait.ExpiresAt = expectedExpire = namedParam.Value.ToLong();
                                break;
                            case CommandIds.ActorTraitCommand.ExpiredAt:
                                trait.ExpireState = ExpirationStates.Expired;
                                if (trait.ExpiresAt != null) trait.ExpiredAt = expiredAt = Math.Min(namedParam.Value.ToLong(), trait.ExpiresAt.Value);
                                else trait.ExpiredAt = expiredAt = namedParam.Value.ToLong();
                                trait.OldOwner = trait.Owner;
                                trait.Owner = null!;
                                break;
                            case CommandIds.ActorTraitCommand.Start:
                                trait.ExpireState = ExpirationStates.Alive;
                                trait.StartsAt = namedParam.Value.ToLong();
                                break;
                            case CommandIds.ActorTraitCommand.Type:
                                trait.Type = namedParam.Value; // this depends on how it is perceived
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

            var triggerId = CommandIds.TriggerCommand.CreateTriggerIdForActorTraitExpire(trait.Id);
            if (expectedExpire != null && expiredAt == null)
            {
                // update trigger time
                var updated = await dbContext.UpdateTrigger(command.TaleId, command.TaleVersionId, triggerId, expectedExpire.Value, token);
                if (!updated) dbContext.Triggers.Add(command.MapTrigger<IActorTraitGrain>(triggerId, expectedExpire!.Value, trait.Id,
                    GrainFetcher.FetchActorTrait(command.TaleId, command.TaleVersionId, trait.Id), CommandIds.TriggerCommand.TriggerList.ActorTraitExpire, null));
            }
            else if (expiredAt != null) await dbContext.CancelTrigger(command.TaleId, command.TaleVersionId, triggerId, token);

            if (!string.IsNullOrEmpty(command.Comments)) trait.Description = command.Comments;

            return trait;
        }
    }

    [GenerateSerializer]
    public class ActorTraitGrainState : EntityGrainStateBase
    {
    }
}

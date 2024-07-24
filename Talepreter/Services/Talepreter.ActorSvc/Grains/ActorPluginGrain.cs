using Orleans.Runtime;
using Talepreter.ActorSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;

namespace Talepreter.ActorSvc.Grains
{
    public class ActorPluginGrain : EntityGrain<ActorPluginGrainState, ActorSvcDBContext, IActorPluginGrain>, IActorPluginGrain
    {
        public ActorPluginGrain([PersistentState("persistentState", "ActorSvcStorage")] IPersistentState<ActorPluginGrainState> persistentState, 
            ILogger<ActorPluginGrain> logger, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId) 
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchActorContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, ActorSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }
    }

    [GenerateSerializer]
    public class ActorPluginGrainState : EntityGrainStateBase
    {
    }
}

using Orleans.Runtime;
using Talepreter.ActorSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;

namespace Talepreter.ActorSvc.Grains
{
    [GenerateSerializer]
    public class ActorContainerGrain : ContainerGrain<ActorContainerGrainState, ActorSvcDBContext, ActorContainerGrain>, IActorContainerGrain
    {
        public ActorContainerGrain([PersistentState("persistentState", "ActorSvcStorage")] IPersistentState<ActorContainerGrainState> persistentState, 
            ILogger<ActorContainerGrain> logger,
            ITalepreterServiceIdentifier serviceIdentifier,
            IPublisher publisher) 
            : base(persistentState, logger, serviceIdentifier, publisher)
        {
        }

        protected override string ExecuteRoutingKey => "execute-actor-svc";

        protected override string PublishRoutingKey => "publish-actor-svc";

        protected override string GrainName => typeof(IActorContainerGrain).Name;
    }

    [GenerateSerializer]
    public class ActorContainerGrainState : ContainerGrainStateBase
    {
    }
}

using Orleans.Runtime;
using Talepreter.WorldSvc.DBContext;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;

namespace Talepreter.WorldSvc.Grains
{
    [GenerateSerializer]
    public class WorldContainerGrain : ContainerGrain<WorldContainerGrainState, WorldSvcDBContext, WorldContainerGrain>, IWorldContainerGrain
    {
        public WorldContainerGrain([PersistentState("persistentState", "WorldSvcStorage")] IPersistentState<WorldContainerGrainState> persistentState, 
            ILogger<WorldContainerGrain> logger,
            ITalepreterServiceIdentifier serviceIdentifier,
            IPublisher publisher)
            : base(persistentState, logger, serviceIdentifier, publisher)
        {
        }

        protected override string ExecuteRoutingKey => "execute-world-svc";

        protected override string PublishRoutingKey => "publish-world-svc";

        protected override string GrainName => typeof(IWorldContainerGrain).Name;
    }

    [GenerateSerializer]
    public class WorldContainerGrainState : ContainerGrainStateBase
    {
    }
}

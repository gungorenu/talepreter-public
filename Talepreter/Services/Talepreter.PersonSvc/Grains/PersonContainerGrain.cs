using Orleans.Runtime;
using Talepreter.PersonSvc.DBContext;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;

namespace Talepreter.PersonSvc.Grains
{
    [GenerateSerializer]
    public class PersonContainerGrain : ContainerGrain<PersonContainerGrainState, PersonSvcDBContext, PersonContainerGrain>, IPersonContainerGrain
    {
        public PersonContainerGrain([PersistentState("persistentState", "PersonSvcStorage")] IPersistentState<PersonContainerGrainState> persistentState, 
            ILogger<PersonContainerGrain> logger,
            ITalepreterServiceIdentifier serviceIdentifier,
            IPublisher publisher)
            : base(persistentState, logger, serviceIdentifier, publisher)
        {
        }

        protected override string ExecuteRoutingKey => "execute-person-svc";

        protected override string PublishRoutingKey => "publish-person-svc";

        protected override string GrainName => typeof(IPersonContainerGrain).Name;
    }

    [GenerateSerializer]
    public class PersonContainerGrainState : ContainerGrainStateBase
    {
    }
}

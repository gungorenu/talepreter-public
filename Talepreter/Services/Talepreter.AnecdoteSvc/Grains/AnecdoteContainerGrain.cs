using Orleans.Runtime;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;

namespace Talepreter.AnecdoteSvc.Grains
{
    [GenerateSerializer]
    public class AnecdoteContainerGrain : ContainerGrain<AnecdoteContainerGrainState, AnecdoteSvcDBContext, AnecdoteContainerGrain>, IAnecdoteContainerGrain
    {
        public AnecdoteContainerGrain([PersistentState("persistentState", "AnecdoteSvcStorage")] IPersistentState<AnecdoteContainerGrainState> persistentState, 
            ILogger<AnecdoteContainerGrain> logger,
            ITalepreterServiceIdentifier serviceIdentifier,
            IPublisher publisher)
            : base(persistentState, logger, serviceIdentifier, publisher)
        {
        }

        protected override string ExecuteRoutingKey => "execute-anecdote-svc";

        protected override string PublishRoutingKey => "publish-anecdote-svc";

        protected override string GrainName => typeof(IAnecdoteContainerGrain).Name;
    }

    [GenerateSerializer]
    public class AnecdoteContainerGrainState : ContainerGrainStateBase
    {
    }
}

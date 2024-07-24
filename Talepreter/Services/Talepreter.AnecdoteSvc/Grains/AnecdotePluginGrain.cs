using Orleans.Runtime;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;

namespace Talepreter.AnecdoteSvc.Grains
{
    public class AnecdotePluginGrain : EntityGrain<AnecdotePluginGrainState, AnecdoteSvcDBContext, IAnecdotePluginGrain>, IAnecdotePluginGrain
    {
        public AnecdotePluginGrain([PersistentState("persistentState", "AnecdoteSvcStorage")] IPersistentState<AnecdotePluginGrainState> persistentState, 
            ILogger<AnecdotePluginGrain> logger, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId) 
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchAnecdoteContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }
    }

    [GenerateSerializer]
    public class AnecdotePluginGrainState : EntityGrainStateBase
    {
    }
}

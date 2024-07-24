using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.WorldSvc.DBContext;

namespace Talepreter.WorldSvc.Grains
{
    public class WorldPluginGrain : EntityGrain<WorldPluginGrainState, WorldSvcDBContext, IWorldPluginGrain>, IWorldPluginGrain
    {
        public WorldPluginGrain([PersistentState("persistentState", "WorldSvcStorage")] IPersistentState<WorldPluginGrainState> persistentState, 
            ILogger<WorldPluginGrain> logger, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchWorldContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }
    }

    [GenerateSerializer]
    public class WorldPluginGrainState : EntityGrainStateBase
    {
    }
}

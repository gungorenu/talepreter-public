using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.PersonSvc.DBContext;

namespace Talepreter.PersonSvc.Grains
{
    public class PersonPluginGrain : EntityGrain<PersonPluginGrainState, PersonSvcDBContext, IPersonPluginGrain>, IPersonPluginGrain
    {
        public PersonPluginGrain([PersistentState("persistentState", "PersonSvcStorage")] IPersistentState<PersonPluginGrainState> persistentState, 
            ILogger<PersonPluginGrain> logger, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchPersonContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, PersonSvcDBContext dbContext, CancellationToken token)
        {
            return await ExecuteOnExtension(command, dbContext, token);
        }
    }

    [GenerateSerializer]
    public class PersonPluginGrainState : EntityGrainStateBase
    {
    }
}

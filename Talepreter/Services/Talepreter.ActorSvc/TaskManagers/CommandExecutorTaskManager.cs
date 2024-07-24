using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.ActorSvc.DBContext;

namespace Talepreter.ActorSvc.TaskManagers
{
    public class CommandExecutorTaskManager : CommandExecutorTaskManager<ActorSvcDBContext>
    {
        public CommandExecutorTaskManager(IGrainFactory grainFactory, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId, 
            ILogger<CommandExecutorTaskManager> logger) 
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IActorContainerGrain).Name;
    }
}

using Talepreter.ActorSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;

namespace Talepreter.ActorSvc.TaskManagers
{
    public class CommandProcessorTaskManager : CommandProcessorTaskManager<ActorSvcDBContext>
    {
        public CommandProcessorTaskManager(IGrainFactory grainFactory,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId,
            ILogger<CommandProcessorTaskManager> logger)
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IActorContainerGrain).Name;
    }
}

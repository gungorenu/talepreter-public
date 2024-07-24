using Talepreter.WorldSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Operations.Grains;
using Talepreter.Contracts.Orleans.Grains.Containers;

namespace Talepreter.WorldSvc.TaskManagers
{
    public class CommandProcessorTaskManager : CommandProcessorTaskManager<WorldSvcDBContext>
    {
        public CommandProcessorTaskManager(IGrainFactory grainFactory,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId,
            ILogger<CommandProcessorTaskManager> logger)
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IWorldContainerGrain).Name;
    }
}

using Talepreter.PersonSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Operations.Grains;
using Talepreter.Contracts.Orleans.Grains.Containers;

namespace Talepreter.PersonSvc.TaskManagers
{
    public class CommandProcessorTaskManager : CommandProcessorTaskManager<PersonSvcDBContext>
    {
        public CommandProcessorTaskManager(IGrainFactory grainFactory, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId, 
            ILogger<CommandProcessorTaskManager> logger) 
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IPersonContainerGrain).Name;
    }
}

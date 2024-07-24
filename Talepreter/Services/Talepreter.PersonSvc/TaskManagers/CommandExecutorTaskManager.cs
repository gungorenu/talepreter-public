using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.PersonSvc.DBContext;

namespace Talepreter.PersonSvc.TaskManagers
{
    public class CommandExecutorTaskManager : CommandExecutorTaskManager<PersonSvcDBContext>
    {
        public CommandExecutorTaskManager(IGrainFactory grainFactory, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId, 
            ILogger<CommandExecutorTaskManager> logger) 
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IPersonContainerGrain).Name;
    }
}

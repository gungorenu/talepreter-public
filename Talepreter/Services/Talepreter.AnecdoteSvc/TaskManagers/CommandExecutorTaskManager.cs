using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.AnecdoteSvc.DBContext;

namespace Talepreter.AnecdoteSvc.TaskManagers
{
    public class CommandExecutorTaskManager : CommandExecutorTaskManager<AnecdoteSvcDBContext>
    {
        public CommandExecutorTaskManager(IGrainFactory grainFactory, 
            IPublisher publisher, 
            ITalepreterServiceIdentifier serviceId, 
            ILogger<CommandExecutorTaskManager> logger) 
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IAnecdoteContainerGrain).Name;
    }
}

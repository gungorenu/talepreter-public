using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;

namespace Talepreter.AnecdoteSvc.TaskManagers
{
    public class CommandProcessorTaskManager : CommandProcessorTaskManager<AnecdoteSvcDBContext>
    {
        public CommandProcessorTaskManager(IGrainFactory grainFactory,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId,
            ILogger<CommandProcessorTaskManager> logger)
            : base(grainFactory, publisher, serviceId, logger)
        {
        }

        public override string ContainerGrainName => typeof(IAnecdoteContainerGrain).Name;
    }
}

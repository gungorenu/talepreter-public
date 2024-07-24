using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Consumer;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.Operations.Processing
{
    /// <summary>
    /// readers messages of raw command and sends them to processors (base or plugin) which will create commands ready to be executed
    /// </summary>
    /// <remarks>
    /// expects processors of "IProcessor&lt;PageCommand&gt;"
    /// </remarks>
    public class BaseProcessCommandReaderService : RabbitMQMessageReaderService
    {
        public BaseProcessCommandReaderService(IRabbitMQConnectionFactory connFactory,
            ILogger<RabbitMQMessageReaderService> logger,
            IServiceScopeFactory scopeFactory,
            ITalepreterServiceIdentifier serviceId)
            : base(connFactory, logger, scopeFactory, serviceId)
        {
        }

        protected override string Setup()
        {
            var exchangeArgs = new Dictionary<string, object>
            {
                { "x-delayed-type", "direct" }
            };
            Channel.ExchangeDeclare(TalepreterTopology.WriteExchange, "x-delayed-message", true, false, exchangeArgs);

            var queue = TalepreterTopology.WriteQueue(ServiceId);
            var queueArgs = new Dictionary<string, object>
            {
                {"x-queue-type", "quorum" }
            };
            Channel.QueueDeclare(queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);
            Channel.QueueBind(queue, TalepreterTopology.WriteExchange, TalepreterTopology.WriteRoutingKey, null);
            return queue;
        }
    }
}

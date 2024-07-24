using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common.RabbitMQ.Publisher;
using Talepreter.Common.RabbitMQ.Consumer;

namespace Talepreter.Common.RabbitMQ
{
    public static class Extensions
    {
        public static IBasicProperties TalepreterMessageProperties(this IModel channel, Type messageType, ServiceId serviceId = ServiceId.None, int delay = 0, int delayCount =0, bool? persistent = null, byte? deliveryMode = null, string? contentType = null, string? correlation = null)
        {
            var props = channel.CreateBasicProperties();
            props.CorrelationId = correlation;
            props.Persistent = persistent ?? true;
            props.DeliveryMode = deliveryMode ?? 2;
            props.ContentType = contentType ?? "application/json";
            props.Headers ??= new Dictionary<string, object>();
            props.Headers[RabbitMQMessageReader.T_MESSAGE_TYPE] = messageType.FullName;
            props.Headers[RabbitMQMessageReader.T_DELAY_COUNT] = 0;
            props.Headers[RabbitMQMessageReader.T_SERVICE_ID] = serviceId.ToString();

            // delay logic
            if (delay > 0)
            {
                props.Headers[RabbitMQMessageReader.X_DELAY] = delay;
                props.Headers[RabbitMQMessageReader.T_DELAY_COUNT] = delayCount + 1; ;
            }

            return props;
        }

        /// <summary>
        /// Registers all RabbitMQ needed services
        /// </summary>
        public static IServiceCollection RegisterRabbitMQ(this IServiceCollection services)
            => services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>()
                .AddSingleton<IPooledObjectPolicy<IModel>, PooledPublishChannelPolicy>()
                .AddSingleton<IPublisher, PublisherWithChannelPool>();
    }
}

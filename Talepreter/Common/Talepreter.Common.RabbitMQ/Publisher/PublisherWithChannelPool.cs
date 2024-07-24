using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Text;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Exceptions;

namespace Talepreter.Common.RabbitMQ.Publisher
{
    public class PublisherWithChannelPool : IPublisher
    {
        private readonly ObjectPool<IModel> _channelPool;
        private readonly ILogger<IPublisher> _logger;
        private readonly ServiceId _serviceId;

        public PublisherWithChannelPool(IRabbitMQConnectionFactory factory, IPooledObjectPolicy<IModel> channelPolicy, ILogger<IPublisher> logger, ITalepreterServiceIdentifier serviceId)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(factory.MaxPublishChannel, 1, "MaxPublishChannel");

            _serviceId = serviceId.ServiceId;
            _logger = logger;
            var provider = new DefaultObjectPoolProvider
            {
                MaximumRetained = factory.MaxPublishChannel
            };
            _channelPool = provider.Create(channelPolicy);
        }

        public void Publish<TMessage>(TMessage message, string exchange, string routing, int delay = 0, int delayCount = 0, bool? persistent = null, byte? deliveryMode = null, string? contentType = null, string? correlation = null)
        {
            var channel = _channelPool.Get();
            try
            {
                if (channel == null) throw new PublisherJobException("Channel pool could not instantiate new channel to use");

                _logger.LogDebug($"Publishing message {message} to {exchange}:{routing}");
                var props = channel.TalepreterMessageProperties(typeof(TMessage), _serviceId, delay, delayCount, persistent, deliveryMode, contentType, correlation);
                var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

                channel.BasicPublish(exchange, routing, props, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Publishing message {message} to {exchange}:{routing} failed!");
                throw;
            }
            finally
            {
                _channelPool.Return(channel);
            }
        }
    }
}

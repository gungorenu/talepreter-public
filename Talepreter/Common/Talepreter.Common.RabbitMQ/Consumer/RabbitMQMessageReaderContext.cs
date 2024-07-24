using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Exceptions;

namespace Talepreter.Common.RabbitMQ.Consumer
{
    public class RabbitMQMessageReaderContext : IDisposable, IReadContext
    {
        private IServiceScope _scope;
        private bool isDisposed = false;
        private readonly object _message;
        private readonly IConsumerDescription _reader;
        private readonly Type _consumerType;

        public RabbitMQMessageReaderContext(ILogger logger,
            IConsumerDescription consumer,
            BasicDeliverEventArgs @event,
            IServiceScope scope, // sadly needed for consumer locating
            object message,
            Type consumerType)
        {
            _reader = consumer;
            Context = @event;
            _scope = scope;
            Logger = logger;
            _message = message;
            _consumerType = consumerType;
        }

        public BasicDeliverEventArgs Context { get; init; }
        public ILogger Logger { get; init; }
        public IServiceProvider Provider => _scope.ServiceProvider;

        public ulong DeliveryTag => Context.DeliveryTag;
        public int DelayCount
        {
            get
            {
                ObjectDisposedException.ThrowIf(isDisposed, this);
                if (Context.BasicProperties.Headers == null) return 0;
                if (Context.BasicProperties.Headers.TryGetValue(RabbitMQMessageReader.T_DELAY_COUNT, out object? value))
                    return $"{value}".ToInt();
                return 0;
            }
        }

        public async Task ConsumeMessage(CancellationToken token)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (DelayCount >= _reader.MaxDelayCount) throw new ConsumerJobException($"Retry limit exceeded for message: {_message}");
            var msgType = _message.GetType();

            var consumer = _scope.ServiceProvider.GetRequiredService(_consumerType) ??
                throw new ConsumerJobException($"Consumer for message could not be created: {_message}");
            var method = consumer.GetType().GetMethod("Consume", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                [msgType, typeof(IReadContext), typeof(CancellationToken)]) ??
                throw new ConsumerJobException($"Consumer has not method matching for interface, reflection error: {_message}");

            var task = (Task)method.Invoke(consumer, [_message, this, token])!;
            await task;
        }

        public void Respond<TResponse>(TResponse response, string routing, int delay = 0, string? correlation = null)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(response));
            PublishInternal(typeof(TResponse), body, Context.Exchange, routing, true, 2, "application/json", delay, correlation ?? Context.BasicProperties.CorrelationId);
            Logger.LogDebug($"Reader-{_reader}: Responds {typeof(TResponse).Name} >> {_message}");
        }
        public void Respond<TResponse>(TResponse response, string exchange, string routing, int delay = 0, string? correlation = null)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(response));
            PublishInternal(typeof(TResponse), body, exchange, routing, true, 2, "application/json", delay, correlation ?? Context.BasicProperties.CorrelationId);
            Logger.LogDebug($"Reader-{_reader}: Responds {typeof(TResponse).Name} >> {_message}");
        }
        public void Success()
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            _reader.Channel.BasicAck(DeliveryTag, false);
            Logger.LogDebug($"Reader-{_reader}: Success >> {_message}");
        }
        public void Delete()
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            _reader.Channel.BasicNack(DeliveryTag, false, false);
            Logger.LogInformation($"Reader-{_reader}: Deletes >> {_message}");
        }
        public void Reject(bool requeue = false)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            _reader.Channel.BasicReject(DeliveryTag, requeue);
            Logger.LogInformation($"Reader-{_reader}: Rejects >> {_message}");
        }
        public void Duplicate()
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            _reader.Channel.BasicReject(DeliveryTag, false);
            Logger.LogDebug($"Reader-{_reader}: Duplicate >> {_message}");
        }
        public void Delay(int delay)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(_message));
            PublishInternal(_message.GetType(), body, Context.Exchange, Context.RoutingKey, Context.BasicProperties.Persistent, Context.BasicProperties.DeliveryMode, Context.BasicProperties.ContentType, delay, Context.BasicProperties.CorrelationId);

            // we have to also delete it, new message will come with delay
            _reader.Channel.BasicReject(DeliveryTag, false);

            Logger.LogInformation($"Reader-{_reader}: Delays >> {_message} @ {delay}ms");
        }

        private void PublishInternal(Type type, byte[] body, string exchange, string routing, bool persistent, byte deliveryMode, string contentType, int delay = 0, string? correlation = null)
        {
            var props = _reader.Channel.TalepreterMessageProperties(type, _reader.ServiceId, delay, DelayCount, persistent, deliveryMode, contentType, correlation);
            props.AppId = Context.BasicProperties.AppId;

            _reader.Channel.BasicPublish(exchange, routing, props, body);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed) return;

            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null!;
            }
            isDisposed = true;
        }
    }
}

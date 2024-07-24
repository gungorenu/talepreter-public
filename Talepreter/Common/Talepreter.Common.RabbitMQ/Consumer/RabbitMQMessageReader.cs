using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Exceptions;

namespace Talepreter.Common.RabbitMQ.Consumer
{
    public abstract class RabbitMQMessageReader : IDisposable, IConsumerDescription
    {
        internal const string T_MESSAGE_TYPE = "talepreter-message-type";
        internal const string T_DELAY_COUNT = "talepreter-delay-count";
        internal const string T_SERVICE_ID = "talepreter-service-id";
        internal const string X_DELAY = "x-delay";

        private bool isDisposed = false;

        public RabbitMQMessageReader(IRabbitMQConnectionFactory connFactory,
            ILogger logger,
            IServiceScopeFactory scopeFactory, // sadly needed for consumer locating
            ITalepreterServiceIdentifier serviceId)
        {
            ScopeFactory = scopeFactory;
            Logger = logger;
            Channel = connFactory.Connection.CreateModel();
            Factory = connFactory;
            ServiceId = serviceId.ServiceId;

            Logger.LogDebug($"Initializing reader {this}");
            var queueName = Setup();

            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += HandleOnReceivedAsync;
            Channel.BasicConsume(queueName, false, consumer);
        }

        private readonly List<IConsumerTypePair> _consumerTypePairs = [];

        protected ILogger Logger { get; private init; }
        protected IServiceScopeFactory ScopeFactory { get; private init; }

        public ServiceId ServiceId { get; private init; }
        public IModel Channel { get; private set; }
        public IRabbitMQConnectionFactory Factory { get; private init; }
        public virtual int MaxDelayCount => 10;
        public virtual int ExecuteTimeout => Factory.ExecuteTimeout;

        protected async Task HandleOnReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            using var source = new CancellationTokenSource();
            var description = "";
            try
            {
                source.CancelAfter(ExecuteTimeout * 1000);

                var body = Encoding.UTF8.GetString(@event.Body.ToArray());
                var headers = @event.BasicProperties.Headers;
                if (headers == null)
                {
                    Logger.LogWarning($"Reader-{this}: Unknown type message, headers is not set");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }
                if (!@event.BasicProperties.Headers.TryGetValue(T_MESSAGE_TYPE, out var typeName))
                {
                    Logger.LogWarning($"Reader-{this}: Unknown type message, type info is missing");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }
                if (typeName == null || string.IsNullOrEmpty(typeName.ToString()))
                {
                    Logger.LogWarning($"Reader-{this}: Unknown type message, type info is null or empty");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }

                var typeNameString = Encoding.UTF8.GetString((byte[])typeName);
                Type? type = AppDomain.CurrentDomain.GetAssemblies().Reverse().Select(a => a.GetType(typeNameString)).FirstOrDefault(t => t != null);
                if (type == null)
                {
                    Logger.LogWarning($"Reader-{this}: Unknown type message, type is not recognized");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }

                var message = System.Text.Json.JsonSerializer.Deserialize(body, type);
                if (message == null)
                {
                    Logger.LogWarning($"Reader-{this}: Null Message of type {type.FullName}");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }
                description = message.ToString();

                var pair = _consumerTypePairs.FirstOrDefault(x => x.MessageType == type);
                if (pair == null)
                {
                    Logger.LogCritical($"Reader-{this}: Reader setup has no consumer registered for this message");
                    Channel?.BasicNack(@event.DeliveryTag, false, false);
                    return;
                }

                source.Token.ThrowIfCancellationRequested();

                using var consumerContext = new RabbitMQMessageReaderContext(Logger, this, @event, ScopeFactory.CreateScope(), message, pair.ConsumerType);
                await consumerContext.ConsumeMessage(source.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning($"Reader-{this}: Timeout >> {description}");
                Channel?.BasicNack(@event.DeliveryTag, false, false);
            }
            catch (ConsumerJobException cex) // catch known exceptions
            {
                Logger.LogWarning(cex, $"Reader-{this}: Failed >> {description}");
                Channel?.BasicNack(@event.DeliveryTag, false, false);
            }
            catch (Exception ex) // catch any exception
            {
                Logger.LogError(ex, $"Reader-{this}: Faulted >> {description}");
                Channel?.BasicNack(@event.DeliveryTag, false, false);
            }
        }

        /// <summary>
        /// Must declare queue, exchange if necessary and bindings
        /// </summary>
        /// <returns>queue name</returns>
        protected abstract string Setup();

        public void RegisterConsumer<TMessage>()
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            _consumerTypePairs.Add(new ConsumerTypePair<TMessage, IConsumer<TMessage>>());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed) return;

            try
            {
                if (Channel != null)
                {
                    Channel?.Close();
                    Channel?.Dispose();
                    Channel = null!;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Reader {this} could not dispose RabbitMQ channel");
            }
            finally
            {
                isDisposed = true;
            }
        }

        public override string ToString() => $"{GetType().Name}";

        private interface IConsumerTypePair
        {
            Type MessageType { get; }
            Type ConsumerType { get; }
        }

        private class ConsumerTypePair<TMessage, TConsumer> : IConsumerTypePair
            where TConsumer : IConsumer<TMessage>
        {
            public ConsumerTypePair()
            {
                MessageType = typeof(TMessage);
                ConsumerType = typeof(TConsumer);
            }
            public Type MessageType { get; init; }
            public Type ConsumerType { get; init; }
        }
    }
}

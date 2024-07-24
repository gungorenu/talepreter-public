using RabbitMQ.Client;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.Common.RabbitMQ
{
    public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
    {
        private bool _isDisposed;

        public RabbitMQConnectionFactory()
        {
            var user = EnvironmentVariableHandler.ReadEnvVar("RabbitMQUser");
            var pwd = EnvironmentVariableHandler.ReadEnvVar("RabbitMQPwd");
            var server = EnvironmentVariableHandler.ReadEnvVar("RabbitMQServer");
            var virtualHost = EnvironmentVariableHandler.ReadEnvVar("RabbitMQVirtualHost");
            var concurrentConsumerCount = EnvironmentVariableHandler.ReadEnvVar("RabbitMQConcurrentConsumerCount").ToInt();
            ExecuteTimeout = Timeouts.RabbitMQExecuteTimeout;
            MaxPublishChannel = EnvironmentVariableHandler.ReadEnvVar("RabbitMQMaxPublishChannel").ToInt();

            Factory = new ConnectionFactory()
            {
                HostName = server,
                UserName = user,
                Password = pwd,
                VirtualHost = virtualHost,
                DispatchConsumersAsync = true,
                ConsumerDispatchConcurrency = concurrentConsumerCount
            };

            Connection = Factory.CreateConnection();
        }

        public IConnectionFactory Factory { get; private set; }
        public IConnection Connection { get; private set; }
        public int ExecuteTimeout { get; private set; }
        public int MaxPublishChannel { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (Connection != null)
                    {
                        Connection?.Close();
                        Connection?.Dispose();
                        Connection = null!;
                    }
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

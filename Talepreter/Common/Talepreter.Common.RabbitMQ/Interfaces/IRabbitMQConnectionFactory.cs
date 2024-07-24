using RabbitMQ.Client;

namespace Talepreter.Common.RabbitMQ.Interfaces
{
    public interface IRabbitMQConnectionFactory
    {
        IConnectionFactory Factory { get; }
        IConnection Connection { get; }
        int ExecuteTimeout { get; }
        int MaxPublishChannel { get; }
    }
}

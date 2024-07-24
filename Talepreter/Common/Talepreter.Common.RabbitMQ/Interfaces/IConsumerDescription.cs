using RabbitMQ.Client;

namespace Talepreter.Common.RabbitMQ.Interfaces
{
    public interface IConsumerDescription
    {
        IModel Channel { get; }
        int MaxDelayCount { get; }
        ServiceId ServiceId { get; }
    }
}

namespace Talepreter.Common.RabbitMQ.Interfaces
{
    public interface IPublisher
    {
        void Publish<TMessage>(TMessage message, string exchange, string routing, int delay = 0, int delayCount = 0, bool? persistent = null, byte? deliveryMode = null, string? contentType = null, string? correlation = null);
    }
}

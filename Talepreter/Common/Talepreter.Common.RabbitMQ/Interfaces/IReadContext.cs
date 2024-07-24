using Microsoft.Extensions.Logging;

namespace Talepreter.Common.RabbitMQ.Interfaces
{
    public interface IReadContext
    {
        void Respond<TResponse>(TResponse response, string exchange, string routing, int delay = 0, string? correlation = null);
        void Respond<TResponse>(TResponse response, string routing, int delay = 0, string? correlation = null);
        void Success();
        void Delete();
        void Reject(bool requeue = false);
        void Delay(int delay);
        void Duplicate();

        ILogger Logger { get; }
        IServiceProvider Provider { get; }

        int DelayCount { get; }
    }
}

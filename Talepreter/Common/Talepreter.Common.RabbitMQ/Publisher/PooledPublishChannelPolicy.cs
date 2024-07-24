using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.Common.RabbitMQ.Publisher
{
    public class PooledPublishChannelPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly IRabbitMQConnectionFactory _connFactory;

        public PooledPublishChannelPolicy(IRabbitMQConnectionFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public IModel Create() => _connFactory.Connection.CreateModel();

        public bool Return(IModel obj)
        {
            if (obj == null) return false;
            if (obj.IsOpen) return true;
            obj.Dispose();
            return false;
        }
    }
}

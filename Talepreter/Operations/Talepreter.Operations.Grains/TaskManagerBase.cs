using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common;
using Talepreter.Contracts;
using Microsoft.Extensions.Logging;

namespace Talepreter.Operations.Grains
{
    public abstract class TaskManagerBase : ITaskManagerBase
    {
        protected readonly CancellationTokenSource _tokenSource;
        protected readonly IGrainFactory _grainFactory;
        protected readonly IPublisher _publisher;
        protected readonly ResponsibleService _serviceId;
        protected readonly ILogger _logger;

        private bool _isDisposed = false;
        protected IServiceScope _scope = default!;
        protected Guid _taleId;
        protected Guid _taleVersionId;
        protected Guid _writerId;
        protected DateTime _operationTime;
        protected GrainId _grainId;
        protected string _grainLogId = default!;

        public TaskManagerBase(IGrainFactory grainFactory, IPublisher publisher, ITalepreterServiceIdentifier serviceId, ILogger logger)
        {
            _tokenSource = new CancellationTokenSource(Timeouts.TaskManagerTimeout * 1000);
            _serviceId = serviceId.ServiceId.Map();
            _publisher = publisher;
            _grainFactory = grainFactory;
            _logger = logger;
        }

        public void Initialize(IServiceScope scope, Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, GrainId grainId, string grainLogId)
        {
            _scope = scope;
            _taleId = taleId;
            _taleVersionId = taleVersionId;
            _writerId = writerId;
            _operationTime = operationTime;
            _grainId = grainId;
            _grainLogId = grainLogId;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                    OnDispose();
                }

                _isDisposed = true;
            }
        }

        protected abstract void OnDispose();

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

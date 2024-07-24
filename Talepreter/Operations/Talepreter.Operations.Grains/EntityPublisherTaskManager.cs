using Microsoft.Extensions.Logging;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Exceptions;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Publish;
using Microsoft.Extensions.DependencyInjection;
using Talepreter.Document.DBContext;

namespace Talepreter.Operations.Grains
{
    public abstract class EntityPublisherTaskManager<TDbContext> : TaskManagerBase, IEntityPublisherTaskManager
        where TDbContext : IDbContext
    {
        private readonly ResultTaskManager<PublishResult> _taskMgr;
        private readonly IDocumentModelMapper _mapper;
        private readonly IDocumentDBContext _docDBContext;
        private int _taskCount = 0;

        public EntityPublisherTaskManager(IGrainFactory grainFactory,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId,
            ILogger logger,
            IDocumentModelMapper mapper,
            IDocumentDBContext documentDbContext)
            : base(grainFactory, publisher, serviceId, logger)
        {
            _taskMgr = new ResultTaskManager<PublishResult>();
            _mapper = mapper;
            _docDBContext = documentDbContext;
        }

        public abstract string ContainerGrainName { get; }

        public async Task Publish()
        {
            try
            {
                // original idea was to ping every grain to publish itself but I know that some services will have many entities (thousands) and it will take so long to publish them through grains (anecdotes can be many and persons are not removed from tale at any time so they pile up)
                // therefore task manager creates execute tasks here and publishes them instead of going to grains
                // so main driver of the design is performance only
                // TODO: find another idea because loading every object here in dbContext (like thousands) will cause both memory and perf issues

                using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>() ?? throw new CommandProcessingException($"{typeof(TDbContext).Name} initialization failed");
                var collections = PublishAwaitingEntities(dbContext, _taleId, _taleVersionId);
                foreach (var collection in collections)
                {
                    foreach (var entity in collection.Entities)
                    {
                        _taskMgr.AppendTasks((token) => Publish(entity, collection.CollectionName, _tokenSource.Token));
                        _taskCount++;
                    }
                }

                var results = _taskMgr.Start(_tokenSource.Token);
                var allErrors = _taskMgr.Errors;
                PublishErrors(allErrors);

                PublishResult result = PublishResult.None;
                if (_taskMgr.FaultedTaskCount > 0) result = PublishResult.Faulted;
                else if (_taskMgr.TimedoutTaskCount > 0) result = PublishResult.Timedout;
                else if (_taskMgr.SuccessfullTaskCount == _taskCount) result = PublishResult.Success;
                else result = PublishResult.Blocked; // edge case, it should not happen

                await ReportBackResult(result);

                _logger.LogInformation($"{_grainLogId} Entity publishing finalized with result {result}, with {_taskMgr.SuccessfullTaskCount}/{_taskMgr.FaultedTaskCount}/{_taskMgr.TimedoutTaskCount} completed/faulted/timedout commands");
            }
            catch (OperationCanceledException)
            {
                try
                {
                    _logger.LogError($"{_grainLogId} Entity publishing got time out");
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(PublishResult.Timedout);
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Entity publishing recovery failed: {cex.Message}");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, $"{_grainLogId} Entity publishing got an error for page");
                    _tokenSource.Cancel();
                    await Task.Delay(500);
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(PublishResult.Faulted);
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Entity publishing recovery failed: {cex.Message}");
                }
            }
        }

        private async Task ReportBackResult(PublishResult result)
        {
            var publishGrain = _grainFactory.FetchPublish(_taleId, _taleVersionId);
            await publishGrain.OnPublishComplete(ContainerGrainName, result);
        }

        private void PublishErrors(Exception[]? errors)
        {
            if (errors == null || errors.Length == 0) return;

            foreach (var err in errors.OfType<EntityPublishException>())
            {
                var response = new PublishEntityResponse
                {
                    TaleId = err.TaleId,
                    WriterId = _writerId,
                    OperationTime = _operationTime,
                    TaleVersionId = err.TaleVersionId,
                    Error = new ErrorInfo
                    {
                        Message = err.Message,
                        Stacktrace = err.StackTrace ?? "",
                        Type = err.GetType().Name,
                    },
                    Service = _serviceId,
                    EntityId = err.EntityId,
                };

                _publisher.Publish(response, TalepreterTopology.ExecuteExchange, TalepreterTopology.ExecuteResultRoutingKey);
            }
        }

        private async Task<PublishResult> Publish(EntityDbBase entity, string collectionName, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            token.ThrowIfCancellationRequested();

            var model = _mapper.MapEntity(entity) ?? throw new EntityPublishException("Model returned null from mapper", entity.TaleId, entity.TaleVersionId, entity.Id);
            if (model.CollectionName != collectionName) throw new EntityPublishException("Model collection and mapper returned collection are not equal, but must be", entity.TaleId, entity.TaleVersionId, entity.Id);

            await _docDBContext.Put(model, token);

            return PublishResult.Success;
        }

        public virtual IEnumerable<PublishableEntities> PublishAwaitingEntities(TDbContext dbContext, Guid taleId, Guid taleVersionId)
        {
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Extensions,
                Entities = dbContext.PluginRecords.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
        }

        protected override void OnDispose()
        {
            _taskMgr?.Dispose();
        }
    }
}

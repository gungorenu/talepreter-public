using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.ActorSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;

namespace Talepreter.ActorSvc.TaskManagers
{
    public class EntityPublisherTaskManager : EntityPublisherTaskManager<ActorSvcDBContext>
    {
        public EntityPublisherTaskManager(IGrainFactory grainFactory,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId,
            ILogger<EntityPublisherTaskManager> logger,
            IDocumentModelMapper mapper,
            IDocumentDBContext documentDbContext)
            : base(grainFactory, publisher, serviceId, logger, mapper, documentDbContext)
        {
        }

        public override string ContainerGrainName => typeof(IActorContainerGrain).Name;

        public override IEnumerable<PublishableEntities> PublishAwaitingEntities(ActorSvcDBContext dbContext, Guid taleId, Guid taleVersionId)
        {
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Actors,
                Entities = dbContext.Actors.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.ActorTraits,
                Entities = dbContext.Traits.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            foreach (var query in base.PublishAwaitingEntities(dbContext, taleId, taleVersionId)) yield return query;
        }
    }
}

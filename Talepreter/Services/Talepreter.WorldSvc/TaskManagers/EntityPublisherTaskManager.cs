using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Document.DBContext;
using Talepreter.Operations.Grains;
using Talepreter.WorldSvc.DBContext;

namespace Talepreter.WorldSvc.TaskManagers
{
    public class EntityPublisherTaskManager : EntityPublisherTaskManager<WorldSvcDBContext>
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

        public override string ContainerGrainName => typeof(IWorldContainerGrain).Name;


        public override IEnumerable<PublishableEntities> PublishAwaitingEntities(WorldSvcDBContext dbContext, Guid taleId, Guid taleVersionId)
        {
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Pages,
                Entities = dbContext.Pages.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Chapters,
                Entities = dbContext.Chapters.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Settlements,
                Entities = dbContext.Settlements.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Worlds,
                Entities = dbContext.Worlds.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            foreach (var query in base.PublishAwaitingEntities(dbContext, taleId, taleVersionId)) yield return query;
        }

    }
}

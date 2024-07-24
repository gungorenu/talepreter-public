using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Document.DBContext;
using Talepreter.Operations.Grains;
using Talepreter.PersonSvc.DBContext;

namespace Talepreter.PersonSvc.TaskManagers
{
    public class EntityPublisherTaskManager : EntityPublisherTaskManager<PersonSvcDBContext>
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

        public override string ContainerGrainName => typeof(IPersonContainerGrain).Name;

        public override IEnumerable<PublishableEntities> PublishAwaitingEntities(PersonSvcDBContext dbContext, Guid taleId, Guid taleVersionId)
        {
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Persons,
                Entities = dbContext.Persons.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            foreach (var query in base.PublishAwaitingEntities(dbContext, taleId, taleVersionId)) yield return query;
        }
    }
}

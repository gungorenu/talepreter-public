using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Operations.Grains;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;

namespace Talepreter.AnecdoteSvc.TaskManagers
{
    public class EntityPublisherTaskManager : EntityPublisherTaskManager<AnecdoteSvcDBContext>
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

        public override string ContainerGrainName => typeof(IAnecdoteContainerGrain).Name;


        public override IEnumerable<PublishableEntities> PublishAwaitingEntities(AnecdoteSvcDBContext dbContext, Guid taleId, Guid taleVersionId)
        {
            yield return new PublishableEntities
            {
                CollectionName = DocumentDBStructure.Collections.Anecdotes,
                Entities = dbContext.Anecdotes.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState == PublishState.None)
            };
            foreach (var query in base.PublishAwaitingEntities(dbContext,taleId, taleVersionId)) yield return query;
        }
    }
}

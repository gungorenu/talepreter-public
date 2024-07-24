using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;

namespace Talepreter.ActorSvc.Consumers
{
    public class DocumentModelMapper : IDocumentModelMapper
    {
        public EntityBase MapEntity(EntityDbBase entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (typeof(DBContext.Actor).Equals(entity.GetType())) return Map((DBContext.Actor)entity);
            if (typeof(DBContext.ActorTrait).Equals(entity.GetType())) return Map((DBContext.ActorTrait)entity);

            throw new EntityPublishException("Entity is not recognized or accepted", entity.TaleId, entity.TaleVersionId, entity.Id);
        }

        private ActorTrait Map(DBContext.ActorTrait entity)
        {
            return new ActorTrait
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                StartsAt = entity.StartsAt,
                ExpiresAt = entity.ExpiresAt,
                ExpiredAt = entity.ExpiredAt,
                ExpireState = entity.ExpireState.Map(),
                Description = entity.Description,
                OwnerName = entity.OwnerName,
                OwnerId = $"{entity.TaleId}:{entity.TaleVersionId}\\ACTOR:{entity.OwnerName}",
                Type = entity.Type,
                PluginData = entity.PluginData
            };
        }

        private Actor Map(DBContext.Actor entity)
        {
            return new Actor
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                StartsAt = entity.StartsAt,
                ExpiresAt = entity.ExpiresAt,
                ExpiredAt = entity.ExpiredAt,
                ExpireState = entity.ExpireState.Map(),
                Physics = entity.Physics,
                Identity = entity.Identity,
                LastSeen = entity.LastSeen,
                LastSeenLocation = entity.LastSeenLocation.Map(),
                Notes = entity.Notes?.List.Select(x => new ActorNoteEntry { Title = x.Title, Value = x.Notes }).ToArray() ?? [],
                PluginData = entity.PluginData
            };
        }
    }
}

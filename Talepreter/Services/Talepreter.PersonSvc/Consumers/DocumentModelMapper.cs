using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;

namespace Talepreter.PersonSvc.Consumers
{
    public class DocumentModelMapper : IDocumentModelMapper
    {
        public EntityBase MapEntity(EntityDbBase entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (typeof(DBContext.Person).Equals(entity.GetType())) return Map((DBContext.Person)entity);
            throw new EntityPublishException("Entity is not recognized or accepted", entity.TaleId, entity.TaleVersionId, entity.Id);
        }

        private Person Map(DBContext.Person entity)
        {
            return new Person
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
                Tags = [.. entity.Tags],
                Notes = entity.Notes?.List.Select(x => new NoteEntry { Chapter = x.Chapter, Page = x.Page, Value = x.Notes }).ToArray() ?? [],
                PluginData = entity.PluginData
            };
        }
    }
}

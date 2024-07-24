using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;

namespace Talepreter.WorldSvc.Consumers
{
    public class DocumentModelMapper : IDocumentModelMapper
    {
        public EntityBase MapEntity(EntityDbBase entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (typeof(DBContext.Chapter).Equals(entity.GetType())) return Map((DBContext.Chapter)entity);
            if (typeof(DBContext.Page).Equals(entity.GetType())) return Map((DBContext.Page)entity);
            if (typeof(DBContext.World).Equals(entity.GetType())) return Map((DBContext.World)entity);
            if (typeof(DBContext.Settlement).Equals(entity.GetType())) return Map((DBContext.Settlement)entity);
            throw new EntityPublishException("Entity is not recognized or accepted", entity.TaleId, entity.TaleVersionId, entity.Id);
        }

        private Chapter Map(DBContext.Chapter entity)
        {
            return new Chapter
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                Reference = entity.Reference,
                Summary = entity.Summary,
                Title = entity.Title,
                WorldName = entity.WorldName,
            };
        }

        private Page Map(DBContext.Page entity)
        {
            return new Page
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                Notes = entity.Notes,
                ChapterId = entity.ChapterId,
                Location = entity.Location.Map()!,
                StartDate = entity.StartDate,
                StayAtLocation = entity.StayAtLocation,
                Travel = entity.Travel != null ? new Journey
                {
                    Destination = entity.Travel.Destination.Map()!,
                    Duration = entity.Travel.Duration
                } : null,
            };
        }

        private World Map(DBContext.World entity)
        {
            return new World
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                Description = entity.Description,
                PluginData = entity.PluginData
            };
        }

        private Settlement Map(DBContext.Settlement entity)
        {
            return new Settlement
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                Description = entity.Description,
                FirstVisited = entity.FirstVisited,
                LastVisited = entity.LastVisited,
                PluginData = entity.PluginData
            };
        }
    }
}

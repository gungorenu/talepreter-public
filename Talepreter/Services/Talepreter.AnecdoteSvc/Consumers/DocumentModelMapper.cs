using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;

namespace Talepreter.AnecdoteSvc.Consumers
{
    public class DocumentModelMapper : IDocumentModelMapper
    {
        public EntityBase MapEntity(EntityDbBase entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            if (typeof(DBContext.Anecdote).Equals(entity.GetType())) return Map((DBContext.Anecdote)entity);
            throw new EntityPublishException("Entity is not recognized or accepted", entity.TaleId, entity.TaleVersionId, entity.Id);
        }

        private Anecdote Map(DBContext.Anecdote entity)
        {
            return new Anecdote
            {
                EntityId = entity.Id,
                TaleId = entity.TaleId,
                TaleVersionId = entity.TaleVersionId,
                WriterId = entity.WriterId,
                LastUpdate = entity.LastUpdate,
                LastUpdatedChapter = entity.LastUpdatedChapter,
                LastUpdatedPageInChapter = entity.LastUpdatedPageInChapter,
                Children = entity.Children?.Select(x => x.Id).ToArray() ?? [],
                Mentions = entity.Entries.List?.Select(x => new MentionEntry
                {
                    Actors = [.. x.Actors],
                    Chapter = x.Chapter,
                    Content = x.Content,
                    Date = x.Date,
                    Location = x.Location.Map(),
                    Page = x.Page,
                }).ToArray() ?? [],
                ParentEntityId = entity.ParentId,
                PluginData = entity.PluginData
            };
        }

    }
}

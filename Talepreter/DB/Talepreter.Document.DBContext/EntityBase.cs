using System.Text.Json.Serialization;

namespace Talepreter.Document.DBContext
{
    public abstract class EntityBase
    {
        /// <summary>
        /// mongodb uses this as primary key, so it has to be unique
        /// </summary>
        public abstract string Id { get; }

        [JsonIgnore]
        public abstract string CollectionName { get; }

        /// <summary>
        /// this is our own id which is unique per collection in a tale publish but not unique in a big table when mongodb takes it among with other tales and publishes so during mapping from EF DB, we swap it
        /// </summary>
        public string EntityId { get; init; } = default!;
        public Guid TaleId { get; init; }
        public Guid TaleVersionId { get; init; }
        public Guid WriterId { get; init; }
        public DateTime LastUpdate { get; init; }
        public int LastUpdatedChapter { get; init; }
        public int LastUpdatedPageInChapter { get; init; }
    }
}

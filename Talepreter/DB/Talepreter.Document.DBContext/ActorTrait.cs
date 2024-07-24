using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class ActorTrait : ExpiringEntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\ACTORTRAIT:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.ActorTraits;

        public string OwnerId { get; init; } = default!;
        public string OwnerName { get; init; } = default!;
        public string Type { get; init; } = default!;
        public string? Description { get; init; }

        public Container? PluginData { get; init; } = default!;

    }
}

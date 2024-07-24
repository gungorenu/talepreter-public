using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class Actor : ExpiringEntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\ACTOR:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Actors;

        public string? Physics { get; init; } = default!;
        public string? Identity { get; init; } = default!;
        public long? LastSeen { get; init; }
        public Location? LastSeenLocation { get; init; }
        public ActorNoteEntry[] Notes { get; init; } = default!;
        public Container? PluginData { get; init; }

    }

    public class ActorNoteEntry
    {
        public string Title { get; init; } = default!;
        public string Value { get; init; } = default!;
    }
}

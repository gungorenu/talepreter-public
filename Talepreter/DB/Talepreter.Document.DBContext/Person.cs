using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class Person: ExpiringEntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\PERSON:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Persons;

        public string[] Tags { get; init; } = default!;
        public string? Physics { get; init; } = default!;
        public string? Identity { get; init; } = default!;
        public long? LastSeen { get; init; }
        public Location? LastSeenLocation { get; init; }
        public NoteEntry[] Notes { get; init; } = default!;

        public Container? PluginData { get; init; } = default!;
    }

    public class NoteEntry
    {
        public int Chapter { get; init; } = default!;
        public int Page { get; init; } = default!;
        public string Value { get; init; } = default!;
    }
}

using System.Text.Json.Serialization;
using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class Anecdote : EntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\ANECDOTE:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Anecdotes;

        [JsonIgnore]
        public string? ParentId => string.IsNullOrEmpty(ParentEntityId) ? null : $"{TaleId}:{TaleVersionId}\\ANECDOTE:{ParentEntityId}";
        [JsonIgnore]
        public string[] ChildEntityIds => Children.Select(x => $"{TaleId}:{TaleVersionId}\\ANECDOTE:{x}").ToArray();

        public string? ParentEntityId { get; init; } = default!;
        public string[] Children { get; init; } = default!;
        public MentionEntry[] Mentions { get; init; } = default!;

        public Container? PluginData { get; init; } = default!;
    }

    public class MentionEntry
    {
        public int Chapter { get; init; } = 0;
        public int Page { get; init; } = 0;
        public string? Content { get; init; }
        public long? Date { get; init; }
        public Location? Location { get; init; }
        public string[] Actors { get; init; } = default!;
    }
}

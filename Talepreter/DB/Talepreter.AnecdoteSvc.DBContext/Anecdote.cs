using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.AnecdoteSvc.DBContext
{
    public class Anecdote : EntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(AnecdoteSvcDBContext.Anecdotes);

        public Anecdote? Parent { get; set; } = default!;
        public string? ParentId { get; set; } = default!;
        public ICollection<Anecdote> Children { get; set; } = default!;
        public MentionEntryMetadata Entries { get; set; } = default!;

        public Container? PluginData { get; set; } = default!;
    }

    public class MentionEntryMetadata
    {
        public ICollection<MentionEntry> List { get; set; } = default!;
    }
}

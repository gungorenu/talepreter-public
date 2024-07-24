using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.PersonSvc.DBContext
{
    public class Person : ExpiringEntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(PersonSvcDBContext.Persons);

        public string[] Tags { get; set; } = default!;
        public string? Physics { get; set; } = default!;
        public string? Identity { get; set; } = default!;
        public long? LastSeen { get; set; }
        public Location? LastSeenLocation { get; set; }
        public NotesMetadata? Notes { get; set; } = default!;

        public Container? PluginData { get; set; } = default!;
    }

    public class NotesMetadata
    {
        public ICollection<NoteEntry> List { get; set; } = default!;
    }
}

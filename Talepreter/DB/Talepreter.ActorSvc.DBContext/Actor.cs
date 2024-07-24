using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.ActorSvc.DBContext
{
    public class Actor : ExpiringEntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(ActorSvcDBContext.Actors);

        public ActorNotesMetadata? Notes { get; set; } = default!;
        public ICollection<ActorTrait> Traits { get; set; } = default!;
        public string? Physics { get; set; } = default!;
        public string? Identity { get; set; } = default!;
        public long? LastSeen { get; set; }
        public Location? LastSeenLocation { get; set; }

        public Container? PluginData { get; set; } = default!;
    }

    public class ActorNotesMetadata
    {
        public ICollection<ActorNoteEntry> List { get; set; } = default!;
    }
}

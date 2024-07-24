using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.ActorSvc.DBContext
{
    public class ActorTrait : ExpiringEntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(ActorSvcDBContext.Traits);

        public Actor Owner { get; set; } = default!;
        public string OwnerName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string? Description { get; set; }

        public Container? PluginData { get; set; } = default!;

        public string OldOwnerName { get; set; } = default!;
        public Actor OldOwner { get; set; } = default!;
    }
}

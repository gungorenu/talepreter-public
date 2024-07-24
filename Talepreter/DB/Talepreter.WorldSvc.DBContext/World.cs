using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.WorldSvc.DBContext
{
    public class World : EntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(WorldSvcDBContext.Worlds);

        public string? Description { get; set; }

        public ICollection<Chapter> Chapters { get; set; } = default!;

        public Container? PluginData { get; set; } = default!;
    }
}

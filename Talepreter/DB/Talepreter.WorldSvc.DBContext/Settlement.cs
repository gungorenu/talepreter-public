using Talepreter.BaseTypes;
using Talepreter.DB.Common;

namespace Talepreter.WorldSvc.DBContext
{
    public class Settlement : EntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(WorldSvcDBContext.Settlements);

        public string? Description { get; set; }
        public long? FirstVisited { get; set; } = null;
        public long? LastVisited { get; set; } = null;

        public Container? PluginData { get; set; } = default!;
    }
}

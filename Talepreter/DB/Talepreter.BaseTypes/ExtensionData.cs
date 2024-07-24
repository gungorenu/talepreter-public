using Talepreter.DB.Common;

namespace Talepreter.BaseTypes
{
    public class ExtensionData : EntityDbBase, IExpandedEntity
    {
        public override string EntityContainer => nameof(IDbContext.PluginRecords);

        public string BaseId { get; set; } = default!; // original target
        public string Type { get; set; } = default!; // original tag

        public Container? PluginData { get; set; } = default!;
    }
}

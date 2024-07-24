using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class Settlement : EntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\SETTLEMENT:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Settlements;

        public string? Description { get; init; }
        public long? FirstVisited { get; init; } = null;
        public long? LastVisited { get; init; } = null;

        public Container? PluginData { get; init; } = default!;
    }
}

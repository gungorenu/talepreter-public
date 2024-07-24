using Talepreter.DB.Common;

namespace Talepreter.Document.DBContext
{
    public class World : EntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\WORLD:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Worlds; // in fact this will be singleton

        public string? Description { get; init; }
        public Container? PluginData { get; init; } = default!;
    }
}

namespace Talepreter.Document.DBContext
{
    public class Chapter: EntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\CHAPTER:{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Chapters;

        public string WorldName { get; init; } = default!;
        public string Title { get; init; } = default!;
        public string? Summary { get; init; }
        public string? Reference { get; init; }
    }
}

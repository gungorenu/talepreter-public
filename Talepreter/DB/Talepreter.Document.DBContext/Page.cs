namespace Talepreter.Document.DBContext
{
    public class Page : EntityBase
    {
        public override string Id => $"{TaleId}:{TaleVersionId}\\PAGE:{ChapterId}#{EntityId}";
        public override string CollectionName => DocumentDBStructure.Collections.Pages;

        public string ChapterId { get; init; } = default!;
        public Location Location { get; init; } = default!;
        public long StartDate { get; init; }
        public long StayAtLocation { get; init; } = 0;
        public Journey? Travel { get; init; }
        public string? Notes { get; init; }
    }
}

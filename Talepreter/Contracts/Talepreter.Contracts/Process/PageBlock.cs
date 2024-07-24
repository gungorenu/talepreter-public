namespace Talepreter.Contracts.Process
{
    public class PageBlock
    {
        public long Date { get; init; }
        public long Stay { get; init; } = 0;
        public string Location { get; init; } = default!;

        public string? Travel { get; init; }
        public long? Voyage { get; init; }

        public long GetEndDate() => Date + Stay + (Voyage ?? 0);
    }
}

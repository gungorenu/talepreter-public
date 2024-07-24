namespace Talepreter.Operations
{
    public class PageTimelineInfo
    {
        public record Location(string Settlement, string? Extension);

        public long Date { get; init; }
        public int Stay { get; init; } = 0;
        public Location CurrentLocation { get; init; } = default!;
        public Location StayLocation { get; init; } = default!;

        public Location? Travel { get; init; }
        public long? Voyage { get; init; }

        public long GetEndDate() => Date + Stay + (Voyage ?? 0);
    }
}

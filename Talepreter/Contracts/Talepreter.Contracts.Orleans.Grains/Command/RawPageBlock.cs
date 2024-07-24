namespace Talepreter.Contracts.Orleans.Grains.Command
{
    [GenerateSerializer]
    public class RawPageBlock
    {
        [Id(0)]
        public long Date { get; init; }
        [Id(1)]
        public long Stay { get; init; } = 0;
        [Id(2)]
        public string Location { get; init; } = default!;
        [Id(3)]
        public string? Travel { get; init; }
        [Id(4)]
        public long? Voyage { get; init; }
    }
}

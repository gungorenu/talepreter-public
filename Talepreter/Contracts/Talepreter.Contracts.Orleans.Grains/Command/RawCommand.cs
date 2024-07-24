namespace Talepreter.Contracts.Orleans.Grains.Command
{
    [GenerateSerializer]
    public class RawCommand
    {
        [Id(0)]
        public int Phase { get; init; } = 1;
        [Id(1)]
        public int Index { get; init; }
        [Id(2)]
        public int? Prequisite { get; init; }
        [Id(3)]
        public bool? HasChild { get; init; }

        [Id(4)]
        public string Tag { get; init; } = default!;
        [Id(5)]
        public string Target { get; init; } = default!;
        [Id(6)]
        public string? Parent { get; init; }
        [Id(7)]
        public NamedParameter[]? NamedParameters { get; init; } = default!;
        [Id(8)]
        public string[]? ArrayParameters { get; init; } = default!;
        [Id(9)]
        public string? Comments { get; init; }

        public override string ToString() => $"RawCommand: [{Index} = {Tag}]";
    }
}

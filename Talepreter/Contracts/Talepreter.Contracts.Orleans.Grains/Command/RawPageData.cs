namespace Talepreter.Contracts.Orleans.Grains.Command
{
    [GenerateSerializer]
    public class RawPageData
    {
        [Id(0)]
        public RawCommand[] Commands { get; init; } = [];

        [Id(1)]
        public RawPageBlock PageBlock { get; init; } = default!;
    }
}

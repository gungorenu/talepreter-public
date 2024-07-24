using Talepreter.Exceptions;

namespace Talepreter.Contracts.Orleans.Execute
{
    [GenerateSerializer]
    public class CommandId : ICommandIdentifier
    {
        [Id(0)]
        public Guid TaleId { get; init; }
        [Id(1)]
        public Guid WriterId { get; init; }
        [Id(2)]
        public DateTime OperationTime { get; init; }
        [Id(3)]
        public Guid TaleVersionId { get; init; }
        [Id(4)]
        public int ChapterId { get; init; } = 0;
        [Id(5)]
        public int PageId { get; init; } = 0;
        [Id(6)]
        public int Index { get; init; } = 0;
        [Id(7)]
        public int Phase { get; init; } = 0;
        [Id(8)]
        public int SubIndex { get; init; } = 0;
        [Id(9)]
        public string Tag { get; init; } = default!;
        [Id(10)]
        public string Target { get; init; } = default!;

        public override string ToString() => $"CommandId:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId}.{Index}/{SubIndex} = {Tag}: {Target}]";
    }
}

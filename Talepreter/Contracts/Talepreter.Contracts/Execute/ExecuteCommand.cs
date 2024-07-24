using Talepreter.Exceptions;

namespace Talepreter.Contracts.Execute
{
    public class ExecuteCommand: ICommandIdentifier
    {
        public Guid TaleId { get; init; }
        public Guid WriterId { get; init; }
        public DateTime OperationTime { get; init; }
        public Guid TaleVersionId { get; init; }
        public int ChapterId { get; init; } = 0;
        public int PageId { get; init; } = 0;
        public int Index { get; init; } = 0;
        public int Phase { get; init; }
        public int SubIndex { get; init; } = 0;
        public string Tag { get; init; } = default!;
        public string Target { get; init; } = default!;
        public string TargetGrainType { get; init; } = default!;
        public string TargetGrainId { get; init; } = default!;

        public override string ToString() => $"ExecuteCommand:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId}.{Index}/{SubIndex} = {Tag}]";
    }
}

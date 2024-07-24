namespace Talepreter.Contracts.Process
{
    public class ProcessCommandResponse
    {
        public Guid TaleId { get; init; }
        public Guid WriterId { get; init; }
        public DateTime OperationTime { get; init; }
        public Guid TaleVersionId { get; init; }
        public int ChapterId { get; init; }
        public int PageId { get; init; }
        public int Index { get; init; }
        public string Tag { get; init; } = default!;
        public string Target { get; init; } = default!;

        public ResponsibleService Service { get; init; } = ResponsibleService.None;
        public ErrorInfo? Error { get; init; } = default!;

        public override string ToString() => $"ProcessCommandResponse:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId}.{Index} = {Tag}] {Target} {Error?.Message ?? ""}";
    }
}

namespace Talepreter.Contracts.Execute.Events
{
    public class ExecutionEvent
    {
        public Guid TaleId { get; init; }
        public Guid TaleVersionId { get; init; }
        public int ChapterId { get; init; } = 0;
        public int PageId { get; init; } = 0;
        public string Code { get; init; } = default!;
        public Dictionary<string, string> Tags { get; init; } = [];

        // not everything has these info, sometimes they have, sometimes not (triggers do not have such stuff)
        public Guid? WriterId { get; init; }
        public DateTime? OperationTime { get; init; }
        public long? Date { get; init; }

        public override string ToString() => $"Event:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId} = {Code}]";
    }
}

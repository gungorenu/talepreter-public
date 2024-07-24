namespace Talepreter.Contracts.Execute
{
    public class ExecuteStatusUpdate
    {
        public Guid TaleId { get; init; }
        public Guid TaleVersionId { get; init; }
        public ExecuteStatus Status { get; init; }
        public int ChapterId { get; init; }
        public int PageId { get; init; }
    }

    public enum ExecuteStatus
    {
        None = 0,
        Success = 1,
        Faulted = 2,
        Timeout = 3
    }
}

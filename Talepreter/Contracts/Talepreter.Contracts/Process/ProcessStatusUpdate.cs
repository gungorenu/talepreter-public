namespace Talepreter.Contracts.Process
{
    public class ProcessStatusUpdate
    {
        public Guid TaleId { get; init; }
        public Guid TaleVersionId { get; init; }
        public ProcessStatus Status { get; init; }
        public int ChapterId { get; init; }
        public int PageId { get; init; }
    }

    public enum ProcessStatus
    {
        None = 0,
        Success = 1,
        Faulted = 2,
        Timeout = 3
    }
}

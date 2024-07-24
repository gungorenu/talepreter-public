namespace Talepreter.Contracts.Publish
{
    public class PublishStatusUpdate
    {
        public Guid TaleId { get; init; }
        public Guid TaleVersionId { get; init; }
        public PublishStatus Status { get; init; }
    }

    public enum PublishStatus
    {
        None = 0,
        Success = 1,
        Faulted = 2,
        Timeout = 3
    }
}

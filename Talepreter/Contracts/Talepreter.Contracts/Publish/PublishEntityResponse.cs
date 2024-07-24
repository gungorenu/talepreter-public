using Talepreter.Common;

namespace Talepreter.Contracts.Publish
{
    public class PublishEntityResponse: ITaleIdentifier
    {
        public Guid TaleId { get; init; }
        public Guid WriterId { get; init; }
        public DateTime OperationTime { get; init; }
        public Guid TaleVersionId { get; init; }
        public string EntityId { get; init; } = default!;
        
        public ResponsibleService Service { get; init; } = ResponsibleService.None;
        public ErrorInfo? Error { get; init; } = default!;

        public override string ToString() => $"PublishEntityResponse:{TaleId}/{TaleVersionId}: {EntityId}";
    }
}

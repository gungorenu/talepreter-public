using Talepreter.Common;
using Talepreter.Exceptions;

namespace Talepreter.Contracts.Process
{
    public class ProcessCommand: ICommandIdentifier, ITaleIdentifier
    {
        public Guid TaleId { get; init; }
        public Guid WriterId { get; init; }
        public DateTime OperationTime { get; init; }
        public Guid TaleVersionId { get; init; }
        public int ChapterId { get; init; }
        public int PageId { get; init; }
        public int Phase { get; init; }

        // regardless of interest, every command should have this page block info because some commands need to know current time during processing time
        // this will be filled by the PAGE command at every page. most commands will ignore/skip this
        public PageBlock BlockInfo { get; init; } = default!;

        public int Index { get; init; }
        public int? Prequisite { get; init; }
        public bool? HasChild { get; init; }

        public string Tag { get; init; } = default!;
        public string Target { get; init; } = default!;
        public string? Parent { get; init; }
        public NamedParameter[]? NamedParameters { get; init; } = default!;
        public string[]? ArrayParameters { get; init; } = default!;
        public string? Comments { get; init; }

        public override string ToString() => $"ProcessCommand:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId}.{Index} = {Tag}]";
    }
}

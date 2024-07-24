using Talepreter.Common;
using Talepreter.Exceptions;

namespace Talepreter.BaseTypes
{
    /// <summary>
    /// Note: same as Contracts.PageCommand but used for storage only and has some different properties
    /// this is not only raw data but also task definition and result storage
    /// </summary>
    public class Command : IComparable<Command>, ICommandIdentifier, ITaleIdentifier
    {
        public Guid TaleId { get; set; }
        public Guid WriterId { get; set; }
        public DateTime OperationTime { get; set; }
        public Guid TaleVersionId { get; set; }
        public int ChapterId { get; set; } = 0;
        public int PageId { get; set; } = 0;
        public int Phase { get; set; } = 1; // << yes, default is 1, 0 means something different

        // --

        /// <summary>
        /// sadly we will need this two here too, system is flexible to understand what grain is target of each command, sometimes custom commands and grains
        /// </summary>
        public string GrainId { get; set; } = default!;
        public string GrainType { get; set; } = default!;

        // --

        public int Index { get; set; } = 0;
        public int SubIndex { get; set; } // do not set this
        public int? Prequisite { get; set; }
        public bool? HasChild { get; set; }

        // --

        // this block is actual raw data
        public string Tag { get; set; } = default!;
        public string Target { get; set; } = default!;
        public string? Parent { get; set; }
        public ICollection<NamedParameter>? NamedParameters { get; set; } = default!;
        public string[]? ArrayParameters { get; set; } = default!;
        public string? Comments { get; set; }

        // --

        // commands will be executed and will produce a result always, sometimes will not succeed and for reporting back we need to keep some values here for simplicity
        public CommandExecutionResult Result { get; set; } = CommandExecutionResult.None;
        // not retry, execute, we will use this for retry if the command fails with possibility about delay need
        public int Attempts { get; set; } = 0;
        public string? Error { get; set; }

        // -- 

        public long CalculatedIndex { get; set; }

        public int CompareTo(Command? other)
        {
            if( ReferenceEquals(this, other ) ) return 0;
            if( other == null) return 1;
            return other.CalculatedIndex.CompareTo(CalculatedIndex); // sort desc
        }

        public override string ToString() => $"Command:{TaleId}/{TaleVersionId}: [{ChapterId}#{PageId}.{Index} = {Tag}]";
    }
}

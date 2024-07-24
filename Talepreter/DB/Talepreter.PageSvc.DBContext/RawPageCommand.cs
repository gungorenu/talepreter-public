namespace Talepreter.PageSvc.DBContext
{
    public class RawPageCommand
    {
        public Guid TaleId { get; set; }
        public Guid WriterId { get; set; }
        public DateTime LastUpdate { get; set; }
        public int ChapterId { get; set; }
        public int PageId { get; set; }
        public int Index { get; set; }
        public int? RequiredIndex { get; set; }
        public bool? HasChild { get; set; }
        public int PrequisiteDepth { get; set; } = 0;

        public string Tag { get; set; } = default!;
        public string Target { get; set; } = default!;
        public string? Parent { get; set; } = null;
        public NamedParametersMetadata NamedParameters { get; set; } = default!;
        public List<string> ArrayParameters { get; set; } = default!;
        public string? Comments { get; set; }
    }

    public class NamedParametersMetadata
    {
        public ICollection<NamedParameter> List { get; set; } = default!;
    }
}

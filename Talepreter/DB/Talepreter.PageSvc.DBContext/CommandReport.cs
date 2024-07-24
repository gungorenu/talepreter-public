namespace Talepreter.PageSvc.DBContext
{
    public class CommandReport
    {
        public Guid TaleVersionId { get; set; }
        public Guid WriterId { get; set; }
        public DateTime LastUpdate { get; set; }

        public int ChapterId { get; set; }
        public int PageId { get; set; }
        public int Index { get; set; }

        public ServiceResponses ServiceResponses { get; set; } = ServiceResponses.None;
        public int? HasErrors { get; set; } = null;
        public List<string>? Errors { get; set; }
    }
}

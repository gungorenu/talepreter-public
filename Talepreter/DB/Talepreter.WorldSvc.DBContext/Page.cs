using Talepreter.BaseTypes;

namespace Talepreter.WorldSvc.DBContext
{
    public class Page : EntityDbBase
    {
        public override string EntityContainer => nameof(WorldSvcDBContext.Pages);

        public Chapter Owner { get; set; } = default!;
        public string ChapterId { get; set; } = default!;
        public Location Location { get; set; } = default!;
        public long StartDate { get; set; }
        public long StayAtLocation { get; set; } = 0;
        public Journey? Travel { get; set; }
        public string? Notes { get; set; }
    }
}

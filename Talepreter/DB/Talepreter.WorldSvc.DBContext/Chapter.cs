using Talepreter.BaseTypes;

namespace Talepreter.WorldSvc.DBContext
{
    public class Chapter : EntityDbBase
    {
        public override string EntityContainer => nameof(WorldSvcDBContext.Chapters);

        public string WorldName { get; set; } = default!;
        public World World { get; set; } = default!;

        public string Title { get; set; } = default!;
        public string? Summary { get; set; }
        public string? Reference { get; set; }

        public ICollection<Page> Pages { get; set; } = default!;
    }
}

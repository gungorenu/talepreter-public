using Talepreter.BaseTypes;

namespace Talepreter.AnecdoteSvc.DBContext
{
    public class MentionEntry
    {
        public int Chapter { get; set; } = 0;
        public int Page { get; set; } = 0;
        public string? Content { get; set; }
        public long? Date { get; set; }
        public Location? Location { get; set; }
        public string[] Actors { get; set; } = default!;
    }
}

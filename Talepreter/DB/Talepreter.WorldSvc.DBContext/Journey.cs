using Talepreter.BaseTypes;

namespace Talepreter.WorldSvc.DBContext
{
    public class Journey
    {
        public long Duration { get; set; }
        public Location Destination { get; set; } = default!;
    }
}

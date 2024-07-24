namespace Talepreter.Document.DBContext
{
    public class Journey
    {
        public long Duration { get; init; }
        public Location Destination { get; init; } = default!;
    }
}

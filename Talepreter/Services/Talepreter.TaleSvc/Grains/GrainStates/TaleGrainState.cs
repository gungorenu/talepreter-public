namespace Talepreter.TaleSvc.Grains.GrainStates
{
    [GenerateSerializer]
    public class TaleGrainState
    {
        [Id(0)]
        public Guid WriterId { get; set; } = Guid.Empty;
        [Id(1)]
        public DateTime LastUpdate { get; set; }
        [Id(2)]
        public List<Guid> VersionTracker { get; } = [];
    }
}

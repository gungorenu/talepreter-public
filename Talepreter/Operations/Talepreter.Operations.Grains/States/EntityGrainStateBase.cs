namespace Talepreter.Operations.Grains.States
{
    [GenerateSerializer]
    public class EntityGrainStateBase
    {
        [Id(0)]
        public Guid WriterId { get; set; }
        [Id(1)]
        public DateTime LastUpdate { get; set; }
    }
}

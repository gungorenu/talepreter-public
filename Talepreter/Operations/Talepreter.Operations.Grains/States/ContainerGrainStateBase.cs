namespace Talepreter.Operations.Grains.States
{
    [GenerateSerializer]
    public class ContainerGrainStateBase
    {
        [Id(0)]
        public Guid TaleId { get; set; }
        [Id(1)]
        public Guid TaleVersionId { get; set; }
        [Id(2)]
        public Guid WriterId { get; set; }
        [Id(3)]
        public DateTime LastUpdated { get; set; }
    }
}

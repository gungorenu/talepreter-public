using Talepreter.Contracts.Orleans.System;

namespace Talepreter.Operations.Grains
{
    [GenerateSerializer]
    public class TaleEntityStateBase 
    {
        [Id(0)]
        public Guid TaleId { get; set; } = Guid.Empty;
        [Id(1)]
        public Guid WriterId { get; set; } = Guid.Empty;
        [Id(2)]
        public DateTime LastUpdate { get; set; }
        [Id(3)] 
        public ControllerGrainStatus Status { get; set; } = ControllerGrainStatus.Idle;
    }
}

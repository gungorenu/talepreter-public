using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Operations.Grains;

namespace Talepreter.TaleSvc.Grains.GrainStates
{
    [GenerateSerializer]
    public class PageGrainState : TaleEntityStateBase
    {
        public PageGrainState()
        {
            ExecuteResults = new Dictionary<string, ExecutionResult>
            {
                [typeof(IActorContainerGrain).Name] = ExecutionResult.None,
                [typeof(IAnecdoteContainerGrain).Name] = ExecutionResult.None,
                [typeof(IPersonContainerGrain).Name] = ExecutionResult.None,
                [typeof(IWorldContainerGrain).Name] = ExecutionResult.None
            };

            ProcessResults = new Dictionary<string, ProcessResult>
            {
                [typeof(IActorContainerGrain).Name] = ProcessResult.None,
                [typeof(IAnecdoteContainerGrain).Name] = ProcessResult.None,
                [typeof(IPersonContainerGrain).Name] = ProcessResult.None,
                [typeof(IWorldContainerGrain).Name] = ProcessResult.None
            };
        }

        [Id(0)]
        public Guid TaleVersionId { get; set; } = Guid.Empty;
        [Id(1)]
        public int ChapterId { get; set; } = -1;
        [Id(2)]
        public int PageId { get; set; } = -1;
        [Id(3)]
        public Dictionary<string, ExecutionResult> ExecuteResults { get; }
        [Id(4)]
        public Dictionary<string, ProcessResult> ProcessResults { get; }
    }
}

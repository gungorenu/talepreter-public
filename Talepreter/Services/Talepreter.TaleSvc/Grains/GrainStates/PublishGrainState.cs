using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Operations.Grains;

namespace Talepreter.TaleSvc.Grains.GrainStates
{
    [GenerateSerializer]
    public class PublishGrainState : TaleEntityStateBase
    {
        public PublishGrainState()
        {
            PublishResults = new Dictionary<string, PublishResult>
            {
                [typeof(IActorContainerGrain).Name] = PublishResult.None,
                [typeof(IAnecdoteContainerGrain).Name] = PublishResult.None,
                [typeof(IPersonContainerGrain).Name] = PublishResult.None,
                [typeof(IWorldContainerGrain).Name] = PublishResult.None
            };
            LastExecutedPage = new ChapterPagePair() { Chapter = -1, Page = -1 };
        }

        [Id(0)]
        public Guid TaleVersionId { get; set; }
        [Id(1)]
        public Dictionary<int, ExecutionResult> ExecuteResults { get; } = [];
        [Id(2)]
        public Dictionary<int, ProcessResult> ProcessResults { get; } = [];
        [Id(3)]
        public Dictionary<string, PublishResult> PublishResults { get; }
        [Id(4)]
        public ChapterPagePair LastExecutedPage { get; set; }

        public int ChapterCount() => ExecuteResults.Count;
    }
}

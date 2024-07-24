using Talepreter.Contracts.Orleans.Grains.Command;

namespace Talepreter.Operations.Grains
{
    public interface ICommandProcessorTaskManager : ITaskManagerBase
    {
        Task Process(int chapter, int page, RawPageData rawPageData);
    }
}

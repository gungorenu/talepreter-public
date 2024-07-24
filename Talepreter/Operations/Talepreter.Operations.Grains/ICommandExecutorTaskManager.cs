namespace Talepreter.Operations.Grains
{
    public interface ICommandExecutorTaskManager : ITaskManagerBase
    {
        Task Execute(int chapter, int page);
    }
}

using Talepreter.Contracts.Orleans.Execute;

namespace Talepreter.Contracts.Orleans.Grains
{
    /// <summary>
    /// our basic command processing grain, base type
    /// </summary>
    public interface ICommandGrain : IGrainWithStringKey
    {
        /// <summary>
        /// executes a command
        /// </summary>
        Task<ExecutionResult> Execute(CommandId commandId);
    }
}

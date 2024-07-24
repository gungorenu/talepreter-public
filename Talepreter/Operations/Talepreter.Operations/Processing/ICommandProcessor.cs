using Microsoft.Extensions.DependencyInjection;
using Talepreter.Contracts.Process;

namespace Talepreter.Operations.Processing
{
    public interface ICommandProcessor
    {
        Task Process(ProcessCommand command, IServiceScope scope, CancellationToken token);
    }
}

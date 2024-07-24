using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Talepreter.Operations.Grains
{
    public interface ITaskManagerBase : IDisposable
    {
        void Initialize(IServiceScope scope, Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, GrainId grainId, string grainLogId);
    }
}

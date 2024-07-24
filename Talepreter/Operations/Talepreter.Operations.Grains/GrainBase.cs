using Microsoft.Extensions.DependencyInjection;

namespace Talepreter.Operations.Grains
{
    public abstract class GrainBase : Grain, IGrain, IIncomingGrainCallFilter
    {
        protected IServiceScope _scope = null!;

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                _scope = ServiceProvider.CreateScope();
                await context.Invoke();
            }
            finally
            {
                _scope?.Dispose();
                _scope = null!;
            }
        }
    }
}

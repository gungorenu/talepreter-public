using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Talepreter.Exceptions;

namespace Talepreter.Operations.Grains
{
    [GenerateSerializer]
    public abstract class GrainWithStateBase<TState> : GrainBase, IGrainIdentifier
        where TState : class
    {
        private readonly IPersistentState<TState> _state;
        protected readonly ILogger _logger;

        protected GrainWithStateBase(IPersistentState<TState> persistentState, ILogger logger)
        {
            _state = persistentState;
            _logger = logger;
        }

        protected ValidationContext Validate(string methodName) => ValidationContext.Validate(_logger, GetType().Name, () => Id, methodName);

        string IGrainIdentifier.Id => Id;

        protected abstract string Id { get; }

        protected TState State => _state.State;

        protected async Task SaveState()
        {
            await _state.WriteStateAsync();
        }

        protected async Task SaveState(Func<TState, Task> action)
        {
            await action(_state.State);
            await _state.WriteStateAsync();
        }

        protected async Task ClearState()
        {
            await _state.ClearStateAsync();
        }
    }
}

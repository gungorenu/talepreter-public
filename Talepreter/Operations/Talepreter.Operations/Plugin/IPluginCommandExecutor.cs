using Microsoft.Extensions.Logging;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Grains;

namespace Talepreter.Operations.Plugin
{
    public interface IPluginCommandExecutor<TSelf, TDbContext>
        where TDbContext : IDbContext
        where TSelf : ICommandGrain
    {
        Task<bool> ExecuteCommand(Command command, TDbContext dbContext, ILogger logger, EntityDbBase target, CancellationToken token);
    }
}

using Microsoft.Extensions.Logging;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Process;

namespace Talepreter.Operations.Plugin
{
    public interface IPluginCommandProcessor<TDbContext>
        where TDbContext : IDbContext
    {
        /// <summary>
        /// creates command reference(s) that plugin is interested
        /// this is a special case that from single command plugin will create multiple commands sometimes. plugins can do that while svcs will not do this
        /// indexes will be set by caller automatically (subindexes)
        /// </summary>
        Task<Command[]?> CustomCommands(ProcessCommand message, TDbContext dbContext, ILogger logger, CancellationToken token);

        /// <summary>
        /// throw CommandValidationException if validation fails
        /// </summary>
        Task ValidateCommand(ProcessCommand message, TDbContext dbContext, ILogger logger, CancellationToken token);
    }
}

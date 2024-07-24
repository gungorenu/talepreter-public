using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Talepreter.Common;
using Talepreter.Document.DBContext;

namespace Talepreter.DocumentDB.DBMigrations
{
    public class Migrator : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<Migrator> _logger;
        private readonly ITalepreterServiceIdentifier _svcIdentifier;
        private readonly IDocumentDBContext _dbContext;

        public Migrator(IHostApplicationLifetime hostApplicationLifetime, 
            ILogger<Migrator> logger, 
            ITalepreterServiceIdentifier svcIdentifier, 
            IDocumentDBContext dbContext)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _svcIdentifier = svcIdentifier;
            _dbContext = dbContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.Setup();

                _logger.LogInformation("Done migration runner!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Migration runner error:{ex.Message} | {ex.StackTrace} | {ex}");
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting migration runner for [{_svcIdentifier.Name}]!");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stoping migration runner for [{_svcIdentifier.Name}]!");
            await base.StopAsync(cancellationToken);
        }
    }
}

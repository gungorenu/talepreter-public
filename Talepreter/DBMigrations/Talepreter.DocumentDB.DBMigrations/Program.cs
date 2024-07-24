using Talepreter.Common;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Talepreter.DocumentDB.DBMigrations;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    Host.CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.AddSingleton<ITalepreterServiceIdentifier>(_ => new TalepreterServiceIdentifier(ServiceId.DocumentDB));
            services.AddHostedService<Migrator>();
            services.AddSingleton<IDocumentDBContext,DocumentDBContext>();
        })
        .Build()
        .Run();
});

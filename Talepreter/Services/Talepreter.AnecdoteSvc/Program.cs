using Microsoft.EntityFrameworkCore;
using Serilog;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.Orleans;
using Talepreter.Common.RabbitMQ;
using Talepreter.AnecdoteSvc;
using Talepreter.Operations;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    var host = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo => silo.ConfigureTalepreterOrleans("AnecdoteSvcStorage", "AnecdoteSilo", (int)ServiceId.AnecdoteSvc))
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.RegisterTalepreterService(ServiceId.AnecdoteSvc);
            services.AddDbContext<AnecdoteSvcDBContext>(options => options.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection")), ServiceLifetime.Transient);
            services.AddHostedService<BaseHostedService>();
            services.RegisterRabbitMQ();
            services.RegisterOperators();
            services.RegisterDocumentDB();
            // call this last
            services.RegisterPlugins();
        })
        .Build();

    host.Run();
});


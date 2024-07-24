using Microsoft.EntityFrameworkCore;
using Serilog;
using Talepreter.WorldSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.Orleans;
using Talepreter.Common.RabbitMQ;
using Talepreter.WorldSvc;
using Talepreter.Operations;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    var host = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo => silo.ConfigureTalepreterOrleans("WorldSvcStorage", "WorldSilo", (int)ServiceId.WorldSvc))
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.RegisterTalepreterService(ServiceId.WorldSvc);
            services.AddDbContext<WorldSvcDBContext>(options => options.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection")), ServiceLifetime.Transient);
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


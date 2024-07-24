using Microsoft.EntityFrameworkCore;
using Serilog;
using Talepreter.ActorSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.Orleans;
using Talepreter.Common.RabbitMQ;
using Talepreter.ActorSvc;
using Talepreter.Operations;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    var host = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo => silo.ConfigureTalepreterOrleans("ActorSvcStorage", "ActorSilo", (int)ServiceId.ActorSvc))
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.RegisterTalepreterService(ServiceId.ActorSvc);
            services.AddDbContext<ActorSvcDBContext>(options => options.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection")), ServiceLifetime.Transient);
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


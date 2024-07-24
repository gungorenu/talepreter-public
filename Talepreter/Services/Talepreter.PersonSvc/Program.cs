using Microsoft.EntityFrameworkCore;
using Serilog;
using Talepreter.PersonSvc.DBContext;
using Talepreter.Common;
using Talepreter.Common.Orleans;
using Talepreter.Common.RabbitMQ;
using Talepreter.PersonSvc;
using Talepreter.Operations;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    var host = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo => silo.ConfigureTalepreterOrleans("PersonSvcStorage", "PersonSilo", (int)ServiceId.PersonSvc))
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.RegisterTalepreterService(ServiceId.PersonSvc);
            services.AddDbContext<PersonSvcDBContext>(options => options.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection")), ServiceLifetime.Transient);
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


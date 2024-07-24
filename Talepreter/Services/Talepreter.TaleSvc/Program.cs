using Serilog;
using Talepreter.Common;
using Talepreter.Common.Orleans;
using Talepreter.Common.RabbitMQ;
using Talepreter.Document.DBContext;

ServiceStarter.StartService(() =>
{
    var host = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo => silo.ConfigureTalepreterOrleans("TaleSvcStorage", "TaleSilo", (int)ServiceId.TaleSvc))
        .ConfigureServices(services =>
        {
            services.AddLogging();
            services.AddSerilog();
            services.RegisterTalepreterService(ServiceId.TaleSvc);
            services.AddHostedService<BaseHostedService>();
            services.RegisterRabbitMQ();
            services.RegisterDocumentDB();
        })
        .Build();

    host.Run();
});


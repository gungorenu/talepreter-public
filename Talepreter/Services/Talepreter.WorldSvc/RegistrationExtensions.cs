using Talepreter.WorldSvc.Consumers;
using Talepreter.Operations.Processing;
using Talepreter.Operations.Grains;
using Talepreter.WorldSvc.TaskManagers;

namespace Talepreter.WorldSvc
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection RegisterOperators(this IServiceCollection services)
        {
            services.AddScoped<ICommandProcessor, CommandProcessor>();
            services.AddTransient<ICommandProcessorTaskManager, CommandProcessorTaskManager>();
            services.AddTransient<ICommandExecutorTaskManager, CommandExecutorTaskManager>();
            services.AddTransient<IEntityPublisherTaskManager, EntityPublisherTaskManager>();
            services.AddSingleton<IDocumentModelMapper, DocumentModelMapper>();
            return services;
        }
    }
}

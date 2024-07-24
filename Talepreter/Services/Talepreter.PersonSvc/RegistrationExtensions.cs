using Talepreter.PersonSvc.Consumers;
using Talepreter.Operations.Processing;
using Talepreter.Operations.Grains;
using Talepreter.PersonSvc.TaskManagers;

namespace Talepreter.PersonSvc
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

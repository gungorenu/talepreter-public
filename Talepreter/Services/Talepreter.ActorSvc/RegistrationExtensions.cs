using Talepreter.ActorSvc.Consumers;
using Talepreter.Operations.Processing;
using Talepreter.Operations.Grains;
using Talepreter.ActorSvc.TaskManagers;

namespace Talepreter.ActorSvc
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

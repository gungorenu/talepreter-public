using Talepreter.AnecdoteSvc.Consumers;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Processing;
using Talepreter.AnecdoteSvc.TaskManagers;

namespace Talepreter.AnecdoteSvc
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

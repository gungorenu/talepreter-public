using Serilog;

namespace Talepreter.Common
{
    public static class LoggingHelper
    {
        public static void SetupSerilog()
        {
            // reader and consumer logs are same
            var logQueueReaders = EnvironmentVariableHandler.TryReadEnvVar("LoggingQueueReaders");
            var queueReaderLogging = Serilog.Events.LogEventLevel.Debug;
            if (logQueueReaders != null) _ = Enum.TryParse(logQueueReaders, out queueReaderLogging);

            var logQueuePublishers = EnvironmentVariableHandler.TryReadEnvVar("LoggingQueuePublishers");
            var queuePublisherLogging = Serilog.Events.LogEventLevel.Debug;
            if (logQueuePublishers != null) _ = Enum.TryParse(logQueuePublishers, out queuePublisherLogging);

            var logCommandProcessors = EnvironmentVariableHandler.TryReadEnvVar("LoggingCommandProcessors");
            var commandProcessorLogging = Serilog.Events.LogEventLevel.Debug;
            if (logCommandProcessors != null) _ = Enum.TryParse(logCommandProcessors, out commandProcessorLogging);

            var logCommandExecutors = EnvironmentVariableHandler.TryReadEnvVar("LoggingCommandExecutors");
            var commandExecutorLogging = Serilog.Events.LogEventLevel.Debug;
            if (logCommandExecutors != null) _ = Enum.TryParse(logCommandExecutors, out commandExecutorLogging);

            var logGrains = EnvironmentVariableHandler.TryReadEnvVar("LoggingGrains");
            var grainsLogging = Serilog.Events.LogEventLevel.Debug;
            if (logGrains != null) _ = Enum.TryParse(logGrains, out grainsLogging);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans", Serilog.Events.LogEventLevel.Warning)

                // rabbitmq stuff
                .MinimumLevel.Override("Talepreter.Common.RabbitMQ.Consumer.RabbitMQMessageReaderService", queueReaderLogging)
                .MinimumLevel.Override("Talepreter.Common.RabbitMQ.Interfaces.IPublisher", queuePublisherLogging)

                // processing
                .MinimumLevel.Override("Talepreter.Operations.Processing", commandProcessorLogging)
                .MinimumLevel.Override("Talepreter.ActorSvc.TaskManagers.CommandProcessorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.AnecdoteSvc.TaskManagers.CommandProcessorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.PersonSvc.TaskManagers.CommandProcessorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.WorldSvc.TaskManagers.CommandProcessorTaskManager", commandExecutorLogging)

                // executing
                .MinimumLevel.Override("Talepreter.Execute", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.ActorSvc.TaskManagers.CommandExecutorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.AnecdoteSvc.TaskManagers.CommandExecutorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.PersonSvc.TaskManagers.CommandExecutorTaskManager", commandExecutorLogging)
                .MinimumLevel.Override("Talepreter.WorldSvc.TaskManagers.CommandExecutorTaskManager", commandExecutorLogging)

                // grains
                .MinimumLevel.Override("Talepreter.Operations.Grains", grainsLogging)
                .MinimumLevel.Override("Talepreter.TaleSvc.Grains", grainsLogging)
                .MinimumLevel.Override("Talepreter.ActorSvc.Grains", grainsLogging)
                .MinimumLevel.Override("Talepreter.WorldSvc.Grains", grainsLogging)
                .MinimumLevel.Override("Talepreter.PersonSvc.Grains", grainsLogging)
                .MinimumLevel.Override("Talepreter.AnecdoteSvc.Grains", grainsLogging)

                .WriteTo.Console()
                .CreateLogger();

            Log.Information(new string('-', 144));
            Log.Information(new string('-', 12) + $" STARTING UP " + new string('-', 119));
            Log.Information(new string('-', 144));
        }
    }
}

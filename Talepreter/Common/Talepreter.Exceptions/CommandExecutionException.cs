namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public class CommandExecutionException : CommandException
    {
        public CommandExecutionException()
        {
        }
        public CommandExecutionException(string message) : base(message)
        {
        }
        public CommandExecutionException(string message, Exception ex) : base(message, ex)
        {
        }
        public CommandExecutionException(ICommandIdentifier command) : base(command)
        {
        }
        public CommandExecutionException(ICommandIdentifier command, string message) : base(command, message)
        {
        }
        public CommandExecutionException(ICommandIdentifier command, string message, Exception ex) : base(command, message, ex)
        {
        }
    }
}

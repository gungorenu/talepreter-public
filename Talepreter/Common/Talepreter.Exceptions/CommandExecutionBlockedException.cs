namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public class CommandExecutionBlockedException : CommandException
    {
        public CommandExecutionBlockedException()
        {
        }
        public CommandExecutionBlockedException(string message) : base(message)
        {
        }
        public CommandExecutionBlockedException(string message, Exception ex) : base(message, ex)
        {
        }
        public CommandExecutionBlockedException(ICommandIdentifier command) : base(command)
        {
        }
        public CommandExecutionBlockedException(ICommandIdentifier command, string message) : base(command, message)
        {
        }
        public CommandExecutionBlockedException(ICommandIdentifier command, string message, Exception ex) : base(command, message, ex)
        {
        }
    }
}

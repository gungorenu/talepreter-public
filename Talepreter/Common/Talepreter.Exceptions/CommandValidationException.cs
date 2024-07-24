namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public class CommandValidationException : CommandException
    {
        public CommandValidationException()
        {
        }
        public CommandValidationException(string message) : base(message)
        {
        }
        public CommandValidationException(string message, Exception ex) : base(message, ex)
        {
        }
        public CommandValidationException(ICommandIdentifier command) : base(command)
        {
        }
        public CommandValidationException(ICommandIdentifier command, string message) : base(command, message)
        {
        }
        public CommandValidationException(ICommandIdentifier command, string message, Exception ex) : base(command, message, ex)
        {
        }
    }
}

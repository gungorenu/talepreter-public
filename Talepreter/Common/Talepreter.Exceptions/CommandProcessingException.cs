namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public class CommandProcessingException : CommandException
    {
        public CommandProcessingException()
        {
        }
        public CommandProcessingException(string message) : base(message)
        {
        }
        public CommandProcessingException(string message, Exception ex) : base(message, ex)
        {
        }
        public CommandProcessingException(ICommandIdentifier command) : base(command)
        {
        }
        public CommandProcessingException(ICommandIdentifier command, string message) : base(command, message)
        {
        }
        public CommandProcessingException(ICommandIdentifier command, string message, Exception ex) : base(command, message, ex)
        {
        }
    }
}

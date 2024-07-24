namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public class GrainOperationException : Exception
    {
        public GrainOperationException() { }
        public GrainOperationException(string message) : base(message) { }
        public GrainOperationException(string message, Exception innerException) : base(message, innerException) { }
        public GrainOperationException(IGrainIdentifier grain, string methodName, string message) : base($"[{grain.GetType().Name}:{methodName}] {grain.Id} {message}") { }
    }
}

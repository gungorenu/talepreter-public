namespace Talepreter.Exceptions
{
    /// <summary>
    /// naming is bad, this is used at GUI because the name "publish" is the term for operation itself (write/process >> execute >> publish)
    /// TODO: rename this
    /// </summary>
    [GenerateSerializer]
    public class PublishOperationException : Exception
    {
        public PublishOperationException() { }
        public PublishOperationException(string message) : base(message) { }
        public PublishOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}

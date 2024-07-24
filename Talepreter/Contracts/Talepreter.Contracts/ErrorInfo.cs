namespace Talepreter.Contracts
{
    public class ErrorInfo
    {
        public string Message { get; init; } = default!;
        public string Type { get; init; } = default!;
        public string Stacktrace { get; init; } = default!;
    }
}

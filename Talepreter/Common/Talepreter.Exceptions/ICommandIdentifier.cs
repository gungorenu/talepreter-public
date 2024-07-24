namespace Talepreter.Exceptions
{
    public interface ICommandIdentifier
    {
        Guid TaleId { get; }
        Guid TaleVersionId { get; }
        int ChapterId { get; }
        int PageId { get; }
        string Tag { get; }
        string Target { get; }
        int Index { get; }
    }
}

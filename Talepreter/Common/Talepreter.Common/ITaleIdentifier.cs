namespace Talepreter.Common
{
    /// <summary>
    /// this is implemented for a very simple reason, to avoid writing same thing in every query over and over again
    /// also it prevents issues like X == X in queries which will return non-tale/publish related stuff
    /// other than this purpose there is no additional reason behind it
    /// </summary>
    public interface ITaleIdentifier
    {
        Guid TaleId { get; }
        Guid TaleVersionId { get; }
    }
}

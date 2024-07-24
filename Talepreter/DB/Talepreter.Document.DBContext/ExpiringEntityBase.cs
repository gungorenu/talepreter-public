namespace Talepreter.Document.DBContext
{
    public enum ExpirationStates
    {
        Timeless = 0,
        Alive = 1,
        Expired = 2
    }

    public abstract class ExpiringEntityBase : EntityBase
    {
        public long? StartsAt { get; init; }
        public long? ExpiresAt { get; init; }
        public long? ExpiredAt { get; init; }
        public ExpirationStates ExpireState { get; init; } = ExpirationStates.Timeless;
    }
}

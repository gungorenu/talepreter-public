namespace Talepreter.BaseTypes
{
    public abstract class ExpiringEntityDbBase : EntityDbBase
    {
        /// <summary>
        /// object has a lifespan (ex: birthday)
        /// </summary>
        public long? StartsAt { get; set; }
        /// <summary>
        /// object defines end of a lifespan (ex: expected death)
        /// </summary>
        public long? ExpiresAt { get; set; }
        /// <summary>
        /// object ended its lifespan (ex: death)
        /// </summary>
        public long? ExpiredAt { get; set; }
        public ExpirationStates ExpireState { get; set; } = ExpirationStates.Timeless;
    }
}

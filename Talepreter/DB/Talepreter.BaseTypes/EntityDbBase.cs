namespace Talepreter.BaseTypes
{
    public abstract class EntityDbBase
    {
        /// <summary>
        /// this is a dirty hack that defines which type the entity is, which DbSet it resides
        /// </summary>
        public abstract string EntityContainer { get; }

        // some data might be integer but still in string format, reason comes from how raw page command id is
        public string Id { get; set; } = default!; 
        public Guid TaleId { get; set; }
        public Guid TaleVersionId { get; set; }
        public Guid WriterId { get; set; }
        public DateTime LastUpdate { get; set; }
        public int LastUpdatedChapter { get; set; }
        public int LastUpdatedPageInChapter { get; set; }

        /// <summary>
        /// most plugin data is used to check something else, they are copies of other entities, like Anecdote stores Actors so it can validate actor list
        /// in such cases publishing is not needed, it is safe to skip them. of course plugins might have different ideas if they have custom entities
        /// </summary>
        public PublishState PublishState { get; set; } = PublishState.None;

        /// <summary>
        /// set by system if this object is created manually by a command within execution, false if it comes from DB
        /// </summary>
        public bool IsNew { get; init; } = false;
    }
}

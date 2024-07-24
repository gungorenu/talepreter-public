namespace Talepreter.BaseTypes
{
    public class PublishableEntities
    {
        public string CollectionName { get; init; } = default!;
        public IQueryable<EntityDbBase> Entities { get; init; } = default!;
    }
}

using Microsoft.EntityFrameworkCore;

namespace Talepreter.BaseTypes
{
    public interface IDbContext : IDisposable
    {
        DbSet<Command> Commands { get; }
        DbSet<Trigger> Triggers { get; }
        DbSet<ExtensionData> PluginRecords { get; set; }

        Task<int> SaveChangesAsync(CancellationToken token);

        Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token);
        Task<int> BackupEntitiesTo(Guid taleId, Guid taleVersionId, Guid newVersionId, CancellationToken token);
        Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token);
        IQueryable<Command> ExecuteAwaitingCommands(Guid taleId, Guid taleVersionId, int chapter, int page, int phase);
        IQueryable<Trigger> GetActiveTriggersBefore(Guid taleId, Guid taleVersionId, long date);
        Task CancelTrigger(Guid taleId, Guid taleVersionId, string id, CancellationToken token);
        Task DeleteTrigger(Guid taleId, Guid taleVersionId, string id, CancellationToken token);
        Task<bool> UpdateTrigger(Guid taleId, Guid taleVersionId, string id, long newTime, CancellationToken token);
        void RejectChanges();
        void SetModified<T>(T entity, params string[] props) where T: class;
        Task AppendCommand(Command command, CancellationToken token); // << very dangerous, only when you know what you are doing
    }
}

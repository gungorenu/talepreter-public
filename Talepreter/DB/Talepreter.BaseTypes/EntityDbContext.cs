using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Talepreter.BaseTypes
{
    public abstract class EntityDbContext : DbContext, IDbContext
    {
        public EntityDbContext() { }
        public EntityDbContext(DbContextOptions contextOptions) : base(contextOptions) { }

        // Raw Data
        public DbSet<Command> Commands { get; set; }

        // Extension Data
        public DbSet<ExtensionData> PluginRecords { get; set; }

        // Triggers
        public DbSet<Trigger> Triggers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterCommandExecuteTaskEntity();
            modelBuilder.RegisterExtensionData();
            modelBuilder.RegisterTriggerData();
            modelBuilder.RegisterExpandedEntity<ExtensionData>();

            modelBuilder.HasSequence<int>("SubIndexSequence", schema: "shared").IncrementsBy(1);

            base.OnModelCreating(modelBuilder);
        }

        public virtual async Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await PluginRecords.PurgeEntities(taleId, taleVersionId, token);
            await Commands.PurgeCommands(taleId, taleVersionId, token);
            await Triggers.PurgeTriggers(taleId, taleVersionId, token);
        }

        public virtual async Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await PluginRecords.ResetPublishState(taleId, taleVersionId, token);
        }

        public IQueryable<Command> ExecuteAwaitingCommands(Guid taleId, Guid taleVersionId, int chapter, int page, int phase)
            => Commands.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.ChapterId == chapter && x.PageId == page && x.Phase == phase);

        public IQueryable<Trigger> GetActiveTriggersBefore(Guid taleId, Guid taleVersionId, long date)
            => Triggers.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.TriggerAt < date && x.State == TriggerState.Set);

        public async Task CancelTrigger(Guid taleId, Guid taleVersionId, string id, CancellationToken token)
        {
            var trigger = await Triggers.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.Id == id).FirstOrDefaultAsync(token);
            if (trigger != null) trigger.State = TriggerState.Canceled;
        }

        public async Task DeleteTrigger(Guid taleId, Guid taleVersionId, string id, CancellationToken token)
        {
            await Triggers.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.Id == id).ExecuteDeleteAsync(token);
        }

        public async Task<bool> UpdateTrigger(Guid taleId, Guid taleVersionId, string id, long newTime, CancellationToken token)
        {
            var trigger = await Triggers.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.Id == id).FirstOrDefaultAsync(token);
            if (trigger == null) return false;

            if (trigger.TriggerAt > newTime) throw new InvalidOperationException("Trigger schedule cannot be set to earlier, only later");
            trigger.TriggerAt = newTime;
            return true;
        }

        public async Task<int> BackupEntitiesTo(Guid taleId, Guid taleVersionId, Guid newVersionId, CancellationToken token)
        {
            var parTaleId = new SqlParameter("taleId", taleId);
            var parTaleVersionId = new SqlParameter("sourceVersionId", taleVersionId);
            var parNewVersionId = new SqlParameter("targetVersionId", newVersionId);
            return await Database.ExecuteSqlAsync($"EXEC dbo.TPBACKUPTOVERSION @taleId = {parTaleId}, @sourceVersionId = {parTaleVersionId}, @targetVersionId = {parNewVersionId}", token);
        }

        public void RejectChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        public void SetModified<T>(T entity, params string[] props) where T : class
        {
            var entry = Entry(entity);
            if (entry.State == EntityState.Unchanged) entry.State = EntityState.Modified;
            foreach (var p in props) entry.Property(p).IsModified = true;
        }

        public async Task AppendCommand(Command command, CancellationToken token)
        {
            await Commands.AddAsync(command, token);
        }
    }
}

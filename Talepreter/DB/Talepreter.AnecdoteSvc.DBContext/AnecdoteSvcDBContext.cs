using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;

namespace Talepreter.AnecdoteSvc.DBContext
{
    public class AnecdoteSvcDBContext : EntityDbContext, IDbContext
    {
        public AnecdoteSvcDBContext() { }
        public AnecdoteSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        // Intermediate Data
        public DbSet<Anecdote> Anecdotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterEntityDBBase<Anecdote>();
            modelBuilder.RegisterExpandedEntity<Anecdote>();

            modelBuilder.Entity<Anecdote>().OwnsOne(a => a.Entries, navigation =>
            {
                navigation.ToJson();
                navigation.OwnsMany(n => n.List, navigation => navigation.OwnsOne(p => p.Location, navigation2 => navigation2.ToJson()));
            });
            modelBuilder.Entity<Anecdote>().
                HasMany(e => e.Children).
                WithOne(a => a.Parent).
                HasForeignKey(e => new { e.TaleVersionId, e.ParentId }).
                HasPrincipalKey(a => new { a.TaleVersionId, a.Id }).
                IsRequired(false).
                OnDelete(DeleteBehavior.ClientSetNull);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection"), b => b.MigrationsAssembly("Talepreter.AnecdoteSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Anecdotes.PurgeEntities(taleId, taleVersionId, token);
            await base.PurgeEntities(taleId, taleVersionId, token);
        }

        public override async Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Anecdotes.ResetPublishState(taleId, taleVersionId, token);
            await base.ResetPublishState(taleId, taleVersionId, token);
        }
    }
}

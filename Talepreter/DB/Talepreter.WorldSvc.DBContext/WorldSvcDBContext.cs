using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;

namespace Talepreter.WorldSvc.DBContext
{
    public class WorldSvcDBContext : EntityDbContext, IDbContext
    {
        public WorldSvcDBContext() { }
        public WorldSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        // Intermediate Data
        public DbSet<World> Worlds { get; set; } // always supposed to be one, in a tale multiple worlds cannot exist for now
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Settlement> Settlements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterEntityDBBase<World>();
            modelBuilder.RegisterEntityDBBase<Chapter>();
            modelBuilder.RegisterEntityDBBase<Page>();
            modelBuilder.RegisterEntityDBBase<Settlement>();
            modelBuilder.RegisterExpandedEntity<World>();
            modelBuilder.RegisterExpandedEntity<Settlement>();

            modelBuilder.Entity<World>().Property(p => p.Id).IsRequired();
            modelBuilder.Entity<Chapter>().Property(p => p.Title).IsRequired();
            modelBuilder.Entity<Page>().OwnsOne(p => p.Location, navigation =>
            {
                navigation.WithOwner();
            });
            modelBuilder.Entity<Page>().Property(p => p.StartDate).IsRequired();
            modelBuilder.Entity<Page>().OwnsOne(p => p.Travel, navigation =>
            {
                navigation.WithOwner();
                navigation.OwnsOne(s => s.Destination);
            });
            modelBuilder.Entity<Page>().HasIndex(p => new { p.TaleVersionId, p.ChapterId, p.Id });

            modelBuilder.Entity<World>().
                HasMany(w => w.Chapters).
                WithOne(c => c.World).
                HasForeignKey(e => new { e.TaleVersionId, e.WorldName }).
                HasPrincipalKey(a => new { a.TaleVersionId, a.Id }).
                OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chapter>().
                HasMany(c => c.Pages).
                WithOne(p => p.Owner).
                HasForeignKey(e => new { e.TaleVersionId, e.ChapterId }).
                HasPrincipalKey(a => new { a.TaleVersionId, a.Id }).
                OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection"), b => b.MigrationsAssembly("Talepreter.WorldSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Settlements.PurgeEntities(taleId, taleVersionId, token);
            await Pages.PurgeEntities(taleId, taleVersionId, token);
            await Chapters.PurgeEntities(taleId, taleVersionId, token);
            await Worlds.PurgeEntities(taleId, taleVersionId, token);
            await base.PurgeEntities(taleId, taleVersionId, token);
        }

        public override async Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Settlements.ResetPublishState(taleId, taleVersionId, token);
            await Pages.ResetPublishState(taleId, taleVersionId, token);
            await Chapters.ResetPublishState(taleId, taleVersionId, token);
            await Worlds.ResetPublishState(taleId, taleVersionId, token);
            await base.ResetPublishState(taleId, taleVersionId, token);
        }
    }
}

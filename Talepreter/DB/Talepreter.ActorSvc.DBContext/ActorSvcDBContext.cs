using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;

namespace Talepreter.ActorSvc.DBContext
{
    public class ActorSvcDBContext : EntityDbContext, IDbContext
    {
        public ActorSvcDBContext() { }
        public ActorSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        // Intermediate Data
        public DbSet<Actor> Actors { get; set; }
        public DbSet<ActorTrait> Traits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterExpiringEntityDBBase<Actor>();
            modelBuilder.RegisterExpiringEntityDBBase<ActorTrait>();

            modelBuilder.RegisterExpandedEntity<Actor>();
            modelBuilder.RegisterExpandedEntity<ActorTrait>();

            modelBuilder.Entity<Actor>().
                HasMany(e => e.Traits).
                WithOne(a => a.Owner).
                HasForeignKey(e => new { e.TaleVersionId, e.OwnerName }).
                HasPrincipalKey(a => new { a.TaleVersionId, a.Id }).
                OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Actor>().OwnsOne(a => a.Notes, navigation =>
            {
                navigation.ToJson();
                navigation.OwnsMany(n => n.List);
            });
            modelBuilder.Entity<Actor>().OwnsOne(a => a.LastSeenLocation, navigation => navigation.ToJson());

            modelBuilder.Entity<ActorTrait>().Property(p => p.Type).IsRequired();
            modelBuilder.Entity<ActorTrait>().Ignore(p => p.OldOwnerName);
            modelBuilder.Entity<ActorTrait>().Ignore(p => p.OldOwner);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection"), b => b.MigrationsAssembly("Talepreter.ActorSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Traits.PurgeEntities(taleId, taleVersionId, token);
            await Actors.PurgeEntities(taleId, taleVersionId, token);
            await base.PurgeEntities(taleId, taleVersionId, token);
        }

        public override async Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Traits.ResetPublishState(taleId, taleVersionId, token);
            await Actors.ResetPublishState(taleId, taleVersionId, token);
            await base.ResetPublishState(taleId, taleVersionId, token);
        }
    }
}

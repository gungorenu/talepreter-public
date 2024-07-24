using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;

namespace Talepreter.PersonSvc.DBContext
{
    public class PersonSvcDBContext : EntityDbContext, IDbContext
    {
        public PersonSvcDBContext() { }
        public PersonSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        // Intermediate Data
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterExpiringEntityDBBase<Person>();
            modelBuilder.RegisterExpandedEntity<Person>();

            modelBuilder.Entity<Person>().OwnsOne(a => a.Notes, navigation =>
            {
                navigation.ToJson();
                navigation.OwnsMany(n => n.List);
            });
            modelBuilder.Entity<Person>().OwnsOne(p => p.LastSeenLocation, navigation =>
            {
                navigation.WithOwner();
            });

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection"), b => b.MigrationsAssembly("Talepreter.PersonSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task PurgeEntities(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Persons.PurgeEntities(taleId, taleVersionId, token);
            await base.PurgeEntities(taleId, taleVersionId, token);
        }

        public override async Task ResetPublishState(Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            await Persons.ResetPublishState(taleId, taleVersionId, token);
            await base.ResetPublishState(taleId, taleVersionId, token);
        }
    }
}

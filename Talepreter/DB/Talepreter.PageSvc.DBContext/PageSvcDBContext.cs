using Microsoft.EntityFrameworkCore;
using Talepreter.Common;

namespace Talepreter.PageSvc.DBContext
{
    public class PageSvcDBContext : DbContext
    {
        public PageSvcDBContext() { }
        public PageSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        public DbSet<RawPageCommand> RawPageCommands { get; set; }
        public DbSet<CommandReport> CommandReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawPageCommand>().HasKey(p => new { p.TaleId, p.ChapterId, p.PageId, p.Index });
            modelBuilder.Entity<RawPageCommand>().Property(p => p.TaleId).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.WriterId).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.LastUpdate).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.ChapterId).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.PageId).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.Index).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.PrequisiteDepth).IsRequired(true).HasDefaultValue(0);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.Tag).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().Property(p => p.Target).IsRequired(true);
            modelBuilder.Entity<RawPageCommand>().OwnsOne(a => a.NamedParameters, navigation =>
            {
                navigation.ToJson();
                navigation.OwnsMany(n => n.List);
            });
            modelBuilder.Entity<RawPageCommand>().HasIndex(p => new { p.TaleId, p.ChapterId, p.PageId, p.Index, p.PrequisiteDepth });

            modelBuilder.Entity<CommandReport>().HasKey(p => new { p.TaleVersionId, p.ChapterId, p.PageId, p.Index });
            modelBuilder.Entity<CommandReport>().Property(p => p.TaleVersionId).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.WriterId).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.LastUpdate).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.ChapterId).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.PageId).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.Index).IsRequired(true);
            modelBuilder.Entity<CommandReport>().Property(p => p.ServiceResponses).IsRequired(true).HasDefaultValue(ServiceResponses.None);
            modelBuilder.Entity<CommandReport>().HasIndex(p => new { p.TaleVersionId, p.ChapterId, p.PageId, p.Index, p.HasErrors }).HasFilter("[HasErrors] IS NOT NULL");

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                EnvironmentVariableHandler.ReadEnvVar("DBConnection")
                , b => b.MigrationsAssembly("Talepreter.PageSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }
    }
}

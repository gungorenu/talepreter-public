using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;

namespace Talepreter.SettlementSvc.DBContext
{
    public class SettlementSvcDBContext : DbContext
    {
        public SettlementSvcDBContext() { }
        public SettlementSvcDBContext(DbContextOptions contextOptions) : base(contextOptions) { }

        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<ExtensionData> PluginData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterExpiringEntityDBBase<Settlement,string>();
            modelBuilder.RegisterEntityDBBase<ExtensionData, string>();
            modelBuilder.RegisterExpandedEntity<Settlement>();
            modelBuilder.RegisterExpandedEntity<ExtensionData>();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                EnvironmentVariableHandler.ReadEnvVar("DBConnection")
                , b => b.MigrationsAssembly("Talepreter.SettlementSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }
    }
}

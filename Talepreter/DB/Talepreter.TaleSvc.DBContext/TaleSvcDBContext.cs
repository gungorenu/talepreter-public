using Microsoft.EntityFrameworkCore;
using Talepreter.Common;

namespace Talepreter.TaleSvc.DBContext
{
    public class TaleSvcDBContext : DbContext
    {
        public TaleSvcDBContext() { }
        public TaleSvcDBContext(DbContextOptions<TaleSvcDBContext> contextOptions) : base(contextOptions) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("DBConnection"), b => b.MigrationsAssembly("Talepreter.TaleSvc.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }
    }
}

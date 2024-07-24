using Microsoft.EntityFrameworkCore;
using Talepreter.Common;

namespace Talepreter.OrleansClustering.DBContext
{
    public class OrleansClusteringDBContext : DbContext
    {
        public OrleansClusteringDBContext() { }
        public OrleansClusteringDBContext(DbContextOptions<OrleansClusteringDBContext> contextOptions) : base(contextOptions) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(EnvironmentVariableHandler.ReadEnvVar("OrleansClusteringDBConnection"), b => b.MigrationsAssembly("Talepreter.OrleansClustering.DBMigrations"));
            base.OnConfiguring(optionsBuilder);
        }
    }
}

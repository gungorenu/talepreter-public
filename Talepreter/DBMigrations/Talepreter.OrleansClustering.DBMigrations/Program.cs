using Talepreter.OrleansClustering.DBContext;
using Talepreter.DBMigrations.Base;

DBMigrationHost.ExecuteMigrations<OrleansClusteringDBContext>(Talepreter.Common.ServiceId.OrleansClustering);

using Talepreter.DBMigrations.Base;
using Talepreter.WorldSvc.DBContext;

DBMigrationHost.ExecuteMigrations<WorldSvcDBContext>(Talepreter.Common.ServiceId.WorldSvc);

using Talepreter.TaleSvc.DBContext;
using Talepreter.DBMigrations.Base;

DBMigrationHost.ExecuteMigrations<TaleSvcDBContext>(Talepreter.Common.ServiceId.TaleSvc);

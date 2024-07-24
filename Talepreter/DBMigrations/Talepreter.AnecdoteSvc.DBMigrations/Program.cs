using Talepreter.DBMigrations.Base;
using Talepreter.AnecdoteSvc.DBContext;

DBMigrationHost.ExecuteMigrations<AnecdoteSvcDBContext>(Talepreter.Common.ServiceId.AnecdoteSvc);

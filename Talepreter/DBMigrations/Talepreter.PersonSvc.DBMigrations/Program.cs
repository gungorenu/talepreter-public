using Talepreter.DBMigrations.Base;
using Talepreter.PersonSvc.DBContext;

DBMigrationHost.ExecuteMigrations<PersonSvcDBContext>(Talepreter.Common.ServiceId.PersonSvc);

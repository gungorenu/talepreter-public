using Talepreter.DBMigrations.Base;
using Talepreter.ActorSvc.DBContext;

DBMigrationHost.ExecuteMigrations<ActorSvcDBContext>(Talepreter.Common.ServiceId.ActorSvc);

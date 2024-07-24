namespace Talepreter.PageSvc.DBContext
{
    [Flags]
    public enum ServiceResponses
    {
        None = 0,
        ActorSvc = 1,
        AnecdoteSvc = 2,
        NPCSvc = 4,
        SettlementSvc = 8,
        WorldSvc = 16
    }
}

namespace Talepreter.Common.RabbitMQ
{
    public static class TalepreterTopology
    {
        public static string WriteExchange => "write";
        public static string ExecuteExchange => "execute";
        public static string EventExchange => "events";

        // --

        public static string WriteQueue(ServiceId svcId) => "write-" + svcId.ToString().ToLower();
        public static string WriteResultQueue => "write-result";
        public static string ExecuteResultQueue => "execute-result";
        public static string EventsQueue => "events";
        public static string StatusUpdateQueue => "status";

        // --

        public static string WriteRoutingKey => "write";
        public static string WriteResultRoutingKey => "write-result";
        public static string ExecuteResultRoutingKey => "execute-result";
        public static string EventRoutingKey => "event";
        public static string StatusUpdateRoutingKey => "status-update";
    }
}

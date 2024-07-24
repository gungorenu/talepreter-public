namespace Talepreter.BaseTypes
{
    public class Trigger
    {
        public string Id { get; set; } = default!; // Id is unique and entity based
        public Guid TaleId { get; set; }
        public Guid TaleVersionId { get; set; }
        public Guid WriterId { get; set; }
        public DateTime LastUpdate { get; set; }

        // trigger state
        public TriggerState State { get; set; } = TriggerState.Set;
        public long TriggerAt { get; set; } // date of tale, exact same counter with Page's date so must be meaningful and non-zero

        // trigger target
        public string Target { get; set; } = default!; // id of object itself, can be used for foreign key
        public string GrainType { get; set; } = default!; // grain name of the target object
        public string GrainId { get; set; } = default!; // Id of target object
        public string Type { get; set; } = default!; // custom event name, generally this defines what the trigger is about
        public string? Parameter { get; set; } = default!; // additional info to pass, sometimes meaningful
    }
}

namespace Talepreter.Contracts.Orleans.System
{
    [GenerateSerializer, Flags] // it is Flags because of a cumulative result return, it is not a big deal and can be removed too
    public enum ControllerGrainStatus
    {
        [Id(0)] Idle = 0,
        [Id(1)] Processing = 1,
        [Id(2)] Processed = 2, // first phase success
        [Id(3)] Executing = 4,
        [Id(4)] Executed = 8, // second phase success
        [Id(5)] Publishing = 16,
        [Id(6)] Published = 32, // third phase success
        [Id(7)] Cancelled = 64,
        [Id(8)] Faulted = 128,
        [Id(9)] Timedout = 256,
        [Id(10)] Purged = 512
    }
}

namespace Talepreter.BaseTypes
{
    // actually this is just dead/alive checker but this will be used for more things than just Actor/Person so naming is weird
    public enum ExpirationStates
    {
        Timeless = 0, // default, and can mean also immortal
        Alive = 1, // meaningful only if has a lifespan defined and has an end
        Expired = 2 // Dead, lifespan ended. commands will still continue processing though
    }
}

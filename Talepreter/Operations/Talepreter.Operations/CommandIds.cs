namespace Talepreter.Operations
{
    /// <summary>
    /// this class holds entire command and parameter matrix. the top level strings are command ids, the sub classes represent named parameters each command can have (recognized by system)
    /// for sure plugins might introduce more
    /// </summary>
    public static class CommandIds
    {
        public const string World = "WORLD";
        public const string Settlement = "SETTLEMENT";
        public const string Chapter = "CHAPTER";
        public const string Page = "PAGE";
        public const string Actor = "ACTOR";
        public const string ActorTrait = "ACTORTRAIT";
        public const string Person = "PERSON";
        public const string Anecdote = "ANECDOTE";
        public const string Trigger = "TRIGGER";

        public static class CommandAttributes // all start with _, every command will have these
        {
            public const string Today = "_today"; // page end date
            public const string PageStartAt = "_pagestartat"; // page start date
            public const string Location = "_location"; // page end location
            public const string PageStartLocation = "_pagestartlocation"; // page start location
            public const string TravelTo = "_travel"; // destination
            public const string Voyage = "_voyage"; // travel duration
            public const string Stay = "_stay"; // stay duration
        }

        public static class WorldCommand // empty, does not expect any parameter
        {
        }
        public static class SettlementCommand // empty, does not expect any parameter
        {
            public const string Visited = "visited";
        }
        public static class ChapterCommand
        {
            public const string Title = "title";
            public const string Reference = "reference";
        }
        public static class PageCommand
        {
            public const string Date = "date";
            public const string Location = "location";
            public const string Stay = "stay";
            public const string Travel = "travel";
            public const string Voyage = "voyage";
            public const string MetActors = "metactors";
            public const string MetPersons = "metpersons";
        }
        public static class ActorCommand
        {
            public const string Birth = "birth";
            public const string DiedAt = "diedat";
            public const string ExpectedDeath = "expecteddeath";
            public const string Physics = "physics";
            public const string Identity = "identity";
            public const string SeenDate = "seendate";
            public const string SeenLocation = "seenlocation";
            public const string Note = "note";
        }
        public static class ActorTraitCommand
        {
            public const string Start = "start";
            public const string ExpiredAt = "expiredat";
            public const string ExpectedExpire = "expectedexpire";
            public const string Type = "type";
        }
        public static class PersonCommand
        {
            public const string Birth = "birth";
            public const string DiedAt = "diedat";
            public const string ExpectedDeath = "expecteddeath";
            public const string Timeless = "timeless";
            public const string Physics = "physics";
            public const string Identity = "identity";
            public const string Tags = "tags";
            public const string Silent = "silent";
            public const string SeenDate = "seendate";
            public const string SeenLocation = "seenlocation";
        }
        public static class AnecdoteCommand
        {
            public const string Overwrite = "overwrite";
            public const string Date = "date";
            public const string Location = "location";
        }
        public static class TriggerCommand
        {
            public const string Id = "id";
            public const string Parameter = "parameter";
            public const string Type = "type";

            public static class TriggerList
            {
                public const string ActorDeath = "actor-death";
                public const string PersonDeath = "person-death";
                public const string ActorTraitExpire = "actor-trait-expire";
            }

            public static string CreateTriggerIdForActorDeath(string actorId) => $"{Actor}:{actorId}:{TriggerList.ActorDeath}";
            public static string CreateTriggerIdForPersonDeath(string personId) => $"{Person}:{personId}:{TriggerList.PersonDeath}";
            public static string CreateTriggerIdForActorTraitExpire(string traitId) => $"{ActorTrait}:{traitId}:{TriggerList.ActorTraitExpire}";
        }
    }
}

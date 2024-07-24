namespace Talepreter.Contracts.Execute.Events
{
    public static class EventIds
    {
        public const string ActorDeath = "ACTORDEATH";
        public const string PersonDeath = "PERSONDEATH";
        public const string ActorTraitExpiration = "ACTORTRAITDEATH";

        public static class ActorDeathParameters
        {
            public const string ActorId = "ActorId";
        }
        public static class ActorTraitExpirationParameters
        {
            public const string ActorId = "ActorId";
            public const string ActorTraitId = "ActorTraitId";
        }
        public static class PersonDeathParameters
        {
            public const string PersonId = "PersonId";
        }
    }
}

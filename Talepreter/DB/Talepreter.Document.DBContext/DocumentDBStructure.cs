namespace Talepreter.Document.DBContext
{
    public static class DocumentDBStructure
    {
        public static class Collections
        {
            public const string Actors = "Actors";
            public const string ActorTraits = "ActorTraits";
            public const string Anecdotes = "Anecdotes";
            public const string Persons = "Persons";
            public const string Pages = "Pages";
            public const string Chapters = "Chapters";
            public const string Worlds = "Worlds";
            public const string Settlements = "Settlements";
            
            public const string Extensions = "Extensions"; // right now we have nothing for here, even in plugins, so this is for future
        }
    }
}

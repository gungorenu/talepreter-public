namespace Talepreter.Operations.Grains
{
    public static class DocumentModelExtensions
    {
        public static Document.DBContext.Location? Map(this BaseTypes.Location? location)
        {
            if (location == null) return null;
            return new Document.DBContext.Location() { Settlement = location.Settlement, Extension = location.Extension };
        }

        public static Document.DBContext.ExpirationStates Map(this BaseTypes.ExpirationStates state)
            => state switch
            {
                BaseTypes.ExpirationStates.Expired => Document.DBContext.ExpirationStates.Expired,
                BaseTypes.ExpirationStates.Timeless => Document.DBContext.ExpirationStates.Timeless,
                BaseTypes.ExpirationStates.Alive => Document.DBContext.ExpirationStates.Alive,
                _ => throw new NotSupportedException($"{state} is not recognized by model {nameof(Document.DBContext.ExpirationStates)}")
            };
    }
}

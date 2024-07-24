using System.Text.Json;

namespace Talepreter.Document.DBContext
{
    public class ExtensionData
    {
        public static JsonSerializerOptions SerializationOptions { get; private set; }

        static ExtensionData()
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            SerializationOptions = options;
        }

        public ExtensionData()
        {
        }

        public string Id { get; init; } = default!;
        public Dictionary<string, string> Tags { get; init; } = [];
        public Dictionary<string, ExtensionData> Children { get; init; } = [];
    }
}

namespace Talepreter.Contracts.Process
{
    public class NamedParameter
    {
        public NamedParameterType Type { get; init; } = NamedParameterType.Set;
        public string Name { get; init; } = default!;
        public string Value { get; init; } = default!;

        public static NamedParameter Create(string name, NamedParameterType type = NamedParameterType.Set, string? value = null)
            => new() { Type = type, Name = name, Value = value! };

        public static NamedParameter CreateL(string name, NamedParameterType type = NamedParameterType.Set, long? value = null)
            => new() { Type = type, Name = name, Value = value.ToString() ?? null! };

        public static NamedParameter CreateI(string name, NamedParameterType type = NamedParameterType.Set, int? value = null)
            => new() { Type = type, Name = name, Value = value.ToString() ?? null! };
    }
}

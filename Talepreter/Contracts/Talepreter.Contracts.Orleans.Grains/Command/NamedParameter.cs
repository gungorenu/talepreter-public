namespace Talepreter.Contracts.Orleans.Grains.Command
{
    [GenerateSerializer]
    public class NamedParameter
    {
        [Id(0)]
        public NamedParameterType Type { get; init; } = NamedParameterType.Set;
        [Id(1)]
        public string Name { get; init; } = default!;
        [Id(2)]
        public string Value { get; init; } = default!;

        public static NamedParameter Create(string name, NamedParameterType type = NamedParameterType.Set, string value = default!)
        {
            return new NamedParameter() { Name = name, Type = type, Value = value };
        }
    }
}

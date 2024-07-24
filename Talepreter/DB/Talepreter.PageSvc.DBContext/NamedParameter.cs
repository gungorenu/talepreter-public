namespace Talepreter.PageSvc.DBContext
{
    public class NamedParameter
    {
        public NamedParameterTypes Type { get; init; } = NamedParameterTypes.Set;
        public string Name { get; init; } = default!;
        public string Value { get; init; } = default!;
    }
}

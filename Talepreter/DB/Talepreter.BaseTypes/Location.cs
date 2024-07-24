namespace Talepreter.BaseTypes
{
    public class Location
    {
        public string Settlement { get; set; } = default!;
        public string? Extension { get; set; }

        public override string ToString() => string.IsNullOrEmpty(Extension) ? Settlement : $"{Settlement},{Extension}";
    }
}

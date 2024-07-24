namespace Talepreter.Exceptions
{
    [GenerateSerializer]
    public abstract class CommandException : Exception
    {
        public CommandException()
        {
        }
        public CommandException(string message) : base(message)
        {
        }
        public CommandException(string message, Exception ex) : base(message, ex)
        {
        }
        public CommandException(ICommandIdentifier command)
        {
            Initialize(command);
        }
        public CommandException(ICommandIdentifier command, string message) : base(message)
        {
            Initialize(command);
        }
        public CommandException(ICommandIdentifier command, string message, Exception ex) : base(message, ex)
        {
            Initialize(command);
        }

        public Guid TaleId { get => Guid.Parse(Data["TaleId"]!.ToString()!); set => Data["TaleId"] = value; }
        public Guid TaleVersionId { get => Guid.Parse(Data["TaleVersion"]!.ToString()!); set => Data["TaleVersion"] = value; }
        public int ChapterId { get => int.Parse(Data["ChapterId"]!.ToString()!); set => Data["ChapterId"] = value; }
        public int PageId { get => int.Parse(Data["PageId"]!.ToString()!); set => Data["PageId"] = value; }
        public string Tag { get => Data["Command"]!.ToString()!; set => Data["Command"] = value; }
        public string Target { get => Data["Target"]!.ToString()!; set => Data["Target"] = value; }
        public int Index { get => int.Parse(Data["Index"]!.ToString()!); set => Data["Index"] = value; }

        protected void Initialize(ICommandIdentifier command)
        {
            TaleId = command.TaleId;
            TaleVersionId = command.TaleVersionId;
            ChapterId = command.ChapterId;
            PageId = command.PageId;
            Tag = command.Tag;
            Index = command.Index;
            Target = command.Target;
        }
    }
}

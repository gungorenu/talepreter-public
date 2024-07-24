using Talepreter.Exceptions;
using NPVE = Talepreter.Operations.Processing.NamedParameterValidationEntry;
using PC = Talepreter.Contracts.Process.ProcessCommand;
using static Talepreter.Operations.CommandIds;
using NamedParameterType = Talepreter.Contracts.Process.NamedParameterType;

namespace Talepreter.Operations.Processing
{
    /// <summary>
    /// these are basic validation, each method is command name
    /// </summary>
    public static class BasicValidationSchemes
    {
        public static void World(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .NoParent()
                .ExpectParametersOnly();
        }

        public static void Settlement(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .NoParent()
                .ExpectParametersOnly(SettlementCommand.Visited)
                .ExpectParameterTypeIn(new NPVE(SettlementCommand.Visited, NamedParameterType.Set));
        }

        public static void Chapter(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .NoParent()
                .ExpectParametersOnly(ChapterCommand.Title, ChapterCommand.Reference)
                .RequiredParametersMissing(ChapterCommand.Title)
                .ExpectParameterTypeIn(new NPVE(ChapterCommand.Title, NamedParameterType.Set), new NPVE(ChapterCommand.Reference, NamedParameterType.Set));
        }

        public static void Page(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .MustParent()
                .ExpectParametersOnly(PageCommand.MetActors, PageCommand.MetPersons)
                .ExpectParameterTypeIn(new NPVE(PageCommand.MetActors, NamedParameterType.Set), new NPVE(PageCommand.MetPersons, NamedParameterType.Set));
        }

        public static void Person(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .ExpectParametersOnly(PersonCommand.Birth, PersonCommand.DiedAt, PersonCommand.ExpectedDeath, PersonCommand.Timeless, PersonCommand.Physics,
                    PersonCommand.Identity, PersonCommand.Tags, PersonCommand.Silent, PersonCommand.SeenDate, PersonCommand.SeenLocation)
                .ExpectPositiveNumericParameter(PersonCommand.Birth)
                .ExpectPositiveNumericParameter(PersonCommand.DiedAt)
                .ExpectPositiveNumericParameter(PersonCommand.ExpectedDeath)
                .ExpectPositiveNumericParameter(PersonCommand.SeenDate)
                .ExpectParameterTypeIn(
                    new NPVE(PersonCommand.Birth, NamedParameterType.Set),
                    new NPVE(PersonCommand.ExpectedDeath, NamedParameterType.Set),
                    new NPVE(PersonCommand.Timeless, NamedParameterType.Set),
                    new NPVE(PersonCommand.Physics, NamedParameterType.Set),
                    new NPVE(PersonCommand.Identity, NamedParameterType.Set),
                    new NPVE(PersonCommand.Silent, NamedParameterType.Set),
                    new NPVE(PersonCommand.SeenDate, NamedParameterType.Set),
                    new NPVE(PersonCommand.SeenLocation, NamedParameterType.Set),
                    new NPVE(PersonCommand.Tags, NamedParameterType.Set, NamedParameterType.Reset, NamedParameterType.Add, NamedParameterType.Remove),
                    new NPVE(PersonCommand.DiedAt, NamedParameterType.Set));
        }

        public static void Anecdote(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .ExpectParametersOnly(AnecdoteCommand.Overwrite, AnecdoteCommand.Date, AnecdoteCommand.Location)
                .ExpectPositiveNumericParameter(AnecdoteCommand.Date)
                .ExpectParameterTypeIn(new NPVE(AnecdoteCommand.Overwrite, NamedParameterType.Reset), new NPVE(AnecdoteCommand.Location, NamedParameterType.Set),
                    new NPVE(AnecdoteCommand.Date, NamedParameterType.Set));
        }

        public static void Actor(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .ExpectParametersOnly(ActorCommand.Birth, ActorCommand.DiedAt, ActorCommand.ExpectedDeath, ActorCommand.Physics, ActorCommand.Identity,
                    ActorCommand.SeenDate, ActorCommand.SeenLocation, ActorCommand.Note)
                .NoParent()
                .ExpectPositiveNumericParameter(ActorCommand.Birth)
                .ExpectPositiveNumericParameter(ActorCommand.DiedAt)
                .ExpectPositiveNumericParameter(ActorCommand.ExpectedDeath)
                .ExpectPositiveNumericParameter(ActorCommand.SeenDate)
                .ExpectParameterTypeIn(
                    new NPVE(ActorCommand.Birth, NamedParameterType.Set),
                    new NPVE(ActorCommand.ExpectedDeath, NamedParameterType.Set),
                    new NPVE(ActorCommand.Physics, NamedParameterType.Set),
                    new NPVE(ActorCommand.Identity, NamedParameterType.Set),
                    new NPVE(ActorCommand.SeenDate, NamedParameterType.Set),
                    new NPVE(ActorCommand.SeenLocation, NamedParameterType.Set),
                    new NPVE(ActorCommand.Note, NamedParameterType.Set, NamedParameterType.Reset, NamedParameterType.Add, NamedParameterType.Remove),
                    new NPVE(ActorCommand.DiedAt, NamedParameterType.Set));
        }

        public static void ActorTrait(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .MustParent()
                .ExpectParametersOnly(ActorTraitCommand.Type, ActorTraitCommand.Start, ActorTraitCommand.ExpiredAt, ActorTraitCommand.ExpectedExpire)
                .RequiredParametersMissing(ActorTraitCommand.Type)
                .ExpectPositiveNumericParameter(ActorTraitCommand.Start)
                .ExpectPositiveNumericParameter(ActorTraitCommand.ExpiredAt)
                .ExpectPositiveNumericParameter(ActorTraitCommand.ExpectedExpire)
                .ExpectParameterTypeIn(
                    new NPVE(ActorTraitCommand.Type, NamedParameterType.Set),
                    new NPVE(ActorTraitCommand.Start, NamedParameterType.Set),
                    new NPVE(ActorTraitCommand.ExpectedExpire, NamedParameterType.Set),
                    new NPVE(ActorTraitCommand.ExpiredAt, NamedParameterType.Set));
        }

        public static void Trigger(PC command)
        {
            command.ValidateNamedParameters()
                .ValidateArrayParameters()
                .NoParent()
                .RequiredParametersMissing(TriggerCommand.Id, TriggerCommand.Type, TriggerCommand.Parameter)
                .ExpectParametersOnly(TriggerCommand.Id, TriggerCommand.Type, TriggerCommand.Parameter)
                .ExpectParameterTypeIn(
                    new NPVE(TriggerCommand.Id, NamedParameterType.Set),
                    new NPVE(TriggerCommand.Type, NamedParameterType.Set),
                    new NPVE(TriggerCommand.Parameter, NamedParameterType.Set));
        }

        public static PC ValidateNamedParameters(this PC command)
        {
            if (command.NamedParameters == null) return command;
            var duplicates = command.NamedParameters!.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(y => y.Key).ToArray();
            if (duplicates != null && duplicates.Length > 0) throw new CommandValidationException(command, $"Named parameter {duplicates[0]} is duplicate");
            if (command.NamedParameters.Any(x => string.IsNullOrEmpty(x.Name))) throw new CommandValidationException(command, $"Named parameter name is empty");
            return command;
        }

        public static PC NoParent(this PC command)
        {
            if (!string.IsNullOrEmpty(command.Parent)) throw new CommandValidationException(command, $"{command.Tag} cannot have parent");
            return command;
        }

        public static PC MustParent(this PC command)
        {
            if (string.IsNullOrEmpty(command.Parent)) throw new CommandValidationException(command, $"{command.Tag} must have parent");
            return command;
        }

        public static PC ValidateArrayParameters(this PC command)
        {
            if (command.ArrayParameters == null) return command;
            var duplicates = command.ArrayParameters!.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToArray();
            if (duplicates != null && duplicates.Length > 0) throw new CommandValidationException(command, $"Array parameter {duplicates[0]} is duplicate");
            if (command.ArrayParameters.Any(string.IsNullOrEmpty))
                throw new CommandValidationException(command, $"Array parameter is empty");
            return command;
        }

        public static PC ExpectParametersOnly(this PC command, params string[] args)
        {
            if (args == null || args.Length == 0) return command;
            // args with : means it is plugin args, like "prefix:arg-name", base args do not have ":"
            var commandArgs = command.NamedParameters?.Where(x => !x.Name.StartsWith('_') && !x.Name.Contains(':')).Select(x => x.Name).ToArray() ?? [];
            if (commandArgs.Length == 0) return command;
            var unexpectedArg = commandArgs.FirstOrDefault(x => !args.Contains(x));
            if (unexpectedArg != null) throw new CommandValidationException(command, $"Named parameter {unexpectedArg} is not recognized");
            return command;
        }

        public static PC ExpectPluginParametersOnly(this PC command, string prefix, params string[] args)
        {
            if (args == null || args.Length == 0) return command;
            // args with : means it is plugin args, like "prefix:arg-name"
            var commandArgs = command.NamedParameters?.Where(x => x.Name.StartsWith($"{prefix.ToLower()}:")).Select(x => x.Name).ToArray() ?? [];
            if (commandArgs.Length == 0) return command;
            var unexpectedArg = commandArgs.FirstOrDefault(x => !args.Contains(x));
            if (unexpectedArg != null) throw new CommandValidationException(command, $"Named parameter {unexpectedArg} is not recognized");
            return command;
        }

        public static PC RequiredParametersMissing(this PC command, params string[] args)
        {
            if (command.NamedParameters == null) throw new CommandValidationException(command, "Named parameter list is empty but command requires some parameters");
            // args with : means it is plugin args, like "prefix:arg-name", base args do not have ":"
            var commandArgs = command.NamedParameters?.Where(x => !x.Name.Contains(':')).Select(x => x.Name).ToArray() ?? [];
            var missingArg = args.FirstOrDefault(x => !commandArgs.Contains(x));
            if (missingArg != null) throw new CommandValidationException(command, $"Named parameter {missingArg} is required but missing");
            return command;
        }

        public static PC RequiredPluginParametersMissing(this PC command, string prefix, params string[] args)
        {
            if (command.NamedParameters == null) throw new CommandValidationException(command, "Named parameter list is empty but command requires some parameters");
            // args with : means it is plugin args, like "prefix:arg-name"
            var commandArgs = command.NamedParameters?.Where(x => x.Name.StartsWith($"{prefix.ToLower()}:")).Select(x => x.Name).ToArray() ?? [];
            var missingArg = args.FirstOrDefault(x => !commandArgs.Contains(x));
            if (missingArg != null) throw new CommandValidationException(command, $"Named parameter {missingArg} is required but missing");
            return command;
        }

        // not used but keep for plugins
        public static PC ExpectNumericParameter(this PC command, string paramName)
        {
            var arg = command.NamedParameters?.Where(x => x.Name == paramName)?.FirstOrDefault();
            if (arg != null && !long.TryParse(arg.Value, out _)) throw new CommandValidationException(command, $"Named parameter {paramName} must be a number");
            return command;
        }

        public static PC ExpectPositiveNumericParameter(this PC command, string paramName)
        {
            var arg = command.NamedParameters?.Where(x => x.Name == paramName)?.FirstOrDefault();
            if (arg != null)
            {
                if (long.TryParse(arg.Value, out var s))
                {
                    if (s < 0) throw new CommandValidationException(command, $"Named parameter {paramName} must be positive number");
                }
                else throw new CommandValidationException(command, $"Named parameter {paramName} must be a number");
            }
            return command;
        }

        public static PC ExpectEnumParameter<T>(this PC command, string paramName) where T : struct, Enum
        {
            var arg = command.NamedParameters?.Where(x => x.Name == paramName)?.FirstOrDefault();
            if (arg != null && !Enum.TryParse(arg.Value, true, out T res)) throw new CommandValidationException(command, $"Named parameter {paramName} is expected to be enum type {typeof(T).Name} but value {arg.Value} is not recognized");
            return command;
        }

        public static PC ExpectParameterTypeIn(this PC command, params NPVE[] entries)
        {
            if (entries == null || entries.Length == 0) return command;
            foreach (var entry in entries)
            {
                var par = command.NamedParameters?.FirstOrDefault(x => x.Name == entry.Name);
                if (par == null) continue;
                if (!entry.AcceptedTypes.Contains(par.Type)) throw new CommandValidationException(command, $"Named parameter {par.Name} is not accepted with {par.Type} in command {command.Tag}");
            }
            return command;
        }
    }

    public record NamedParameterValidationEntry(string Name, params NamedParameterType[] AcceptedTypes);
}

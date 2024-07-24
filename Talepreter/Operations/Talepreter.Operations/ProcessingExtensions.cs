using System.ComponentModel;
using Talepreter.Common;
using Talepreter.Contracts.Process;
using Talepreter.Exceptions;

namespace Talepreter.Operations
{
    public static class ProcessingExtensions
    {
        public static BaseTypes.NamedParameter Map(this NamedParameter self) =>
            new()
            {
                Name = self.Name,
                Value = self.Value,
                Type = self.Type.Map()
            };

        public static BaseTypes.NamedParameterType Map(this NamedParameterType self)
            => self switch
            {
                NamedParameterType.Set => BaseTypes.NamedParameterType.Set,
                NamedParameterType.Add => BaseTypes.NamedParameterType.Add,
                NamedParameterType.Remove => BaseTypes.NamedParameterType.Remove,
                NamedParameterType.Reset => BaseTypes.NamedParameterType.Reset,
                _ => throw new InvalidEnumArgumentException($"NamedParameterType {self} is not recognized")
            };

        public static BaseTypes.Command Map(this ProcessCommand message)
        {
            var command = new BaseTypes.Command
            {
                TaleId = message.TaleId,
                WriterId = message.WriterId,
                OperationTime = message.OperationTime,
                TaleVersionId = message.TaleVersionId,
                ChapterId = message.ChapterId,
                PageId = message.PageId,
                Phase = message.Phase,

                Index = message.Index,
                Prequisite = message.Prequisite,
                HasChild = message.HasChild,

                Tag = message.Tag,
                Target = message.Target,
                Parent = message.Parent,
                NamedParameters = message.NamedParameters?.Select(x => x.Map()).ToArray() ?? null,
                ArrayParameters = message.ArrayParameters,
                Comments = message.Comments,

                Result = BaseTypes.CommandExecutionResult.None,
                Attempts = 0,
                Error = null,
            };

            return command;
        }

        public static BaseTypes.Command MapPage(this ProcessCommand message)
        {
            if (message.Tag != CommandIds.Page) throw new CommandValidationException(message, $"This call must be done on Page command only but received {message}");
            if (message.BlockInfo == null) throw new CommandValidationException(message, $"Page command block info is null");

            List<NamedParameter> parameters = new(message.NamedParameters ?? []);
            if (!parameters.Any(x => x.Name == CommandIds.PageCommand.Date)) parameters.Add(NamedParameter.CreateL(CommandIds.PageCommand.Date, value: message.BlockInfo.Date));
            if (!parameters.Any(x => x.Name == CommandIds.PageCommand.Stay)) parameters.Add(NamedParameter.CreateL(CommandIds.PageCommand.Stay, value: message.BlockInfo.Stay));
            if (!parameters.Any(x => x.Name == CommandIds.PageCommand.Location)) parameters.Add(NamedParameter.Create(CommandIds.PageCommand.Location, value: message.BlockInfo.Location));

            if (!parameters.Any(x => x.Name == CommandIds.PageCommand.Travel) && message.BlockInfo.Travel != null)
                parameters.Add(NamedParameter.Create(CommandIds.PageCommand.Travel, value: message.BlockInfo.Travel));
            if (!parameters.Any(x => x.Name == CommandIds.PageCommand.Voyage) && message.BlockInfo.Voyage != null)
                parameters.Add(NamedParameter.CreateL(CommandIds.PageCommand.Voyage, value: message.BlockInfo.Voyage));

            var command = new BaseTypes.Command
            {
                TaleId = message.TaleId,
                WriterId = message.WriterId,
                OperationTime = message.OperationTime,
                TaleVersionId = message.TaleVersionId,
                ChapterId = message.ChapterId,
                PageId = message.PageId,
                Phase = message.Phase,

                Index = message.Index,
                Prequisite = message.Prequisite,
                HasChild = message.HasChild,

                Tag = message.Tag,
                Target = message.Target,
                Parent = message.Parent,
                NamedParameters = parameters.Select(x => x.Map()).ToArray() ?? null,
                ArrayParameters = message.ArrayParameters,
                Comments = message.Comments,

                Result = BaseTypes.CommandExecutionResult.None,
                Attempts = 0,
                Error = null
            };

            return command;
        }

        public static BaseTypes.Command MapWith(this ProcessCommand message, string grainId, string grainType, int? prequisite = null, bool? hasChild = null, string? tag = null,
            string? target = null, string? parent = null, NamedParameter[]? namedParams = null, string[]? arrayParams = null, string? comments = null, int? phase = null)
        {
            var command = new BaseTypes.Command
            {
                TaleId = message.TaleId,
                WriterId = message.WriterId,
                OperationTime = message.OperationTime,
                TaleVersionId = message.TaleVersionId,
                ChapterId = message.ChapterId,
                PageId = message.PageId,
                Phase = phase ?? message.Phase,

                GrainId = grainId,
                GrainType = grainType,

                Index = message.Index,
                Prequisite = prequisite ?? message.Prequisite,
                HasChild = hasChild ?? message.HasChild,

                Tag = tag ?? message.Tag,
                Target = target ?? message.Target,
                Parent = parent ?? message.Parent,
                NamedParameters = namedParams?.Select(x => x.Map()).ToArray() ?? (message.NamedParameters?.Select(x => x.Map()).ToArray() ?? null),
                ArrayParameters = arrayParams ?? message.ArrayParameters,
                Comments = comments ?? message.Comments,

                Result = BaseTypes.CommandExecutionResult.None,
                Attempts = 0,
                Error = null,
            };

            return command;
        }

        public static BaseTypes.Command MapTrigger(this ProcessCommand message, BaseTypes.Trigger trigger)
        {
            var command = new BaseTypes.Command
            {
                TaleId = message.TaleId,
                WriterId = message.WriterId,
                OperationTime = message.OperationTime,
                TaleVersionId = message.TaleVersionId,
                ChapterId = message.ChapterId,
                PageId = message.PageId,
                Phase = message.Phase,

                Index = message.Index,
                Prequisite = message.Prequisite,
                HasChild = message.HasChild,

                Tag = CommandIds.Trigger,
                Target = trigger.Target,
                NamedParameters = [BaseTypes.NamedParameter.Create(CommandIds.TriggerCommand.Id, value: trigger.Id),
                    BaseTypes.NamedParameter.Create(CommandIds.TriggerCommand.Type, value: trigger.Type),
                    BaseTypes.NamedParameter.Create(CommandIds.TriggerCommand.Parameter, value: trigger.Parameter)],
                Parent = null,
                ArrayParameters = null,
                Comments = null,

                GrainType = trigger.GrainType,
                GrainId = trigger.GrainId,

                Result = BaseTypes.CommandExecutionResult.None,
                Attempts = 0,
                Error = null
            };

            return command;
        }

        public static long CommandDate(this BaseTypes.Command command)
        {
            var par = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.Today);
            return par?.Value.ToLong() ?? 0L;
        }
    }
}

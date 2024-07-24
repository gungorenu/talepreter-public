using Microsoft.EntityFrameworkCore;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Contracts.Execute.Events;
using Talepreter.Exceptions;

namespace Talepreter.Operations
{
    public static class ExecuteExtensions
    {
        public static T[] GetArray<T>(this NamedParameter param, char separator, Func<string, T> converter)
        {
            if (string.IsNullOrEmpty(param.Value)) return [];
            return param.Value.SplitInto($"{separator}", converter);
        }

        public static string[] GetArray(this NamedParameter param, char separator)
        {
            if (string.IsNullOrEmpty(param.Value)) return [];
            return param.Value.SplitInto($"{separator}");
        }

        public static async Task<Location?> ParseLocation<TDbContext>(this NamedParameter param, Command command, TDbContext dbContext, CancellationToken token)
            where TDbContext : IDbContext
        {
            ArgumentNullException.ThrowIfNull(nameof(param));
            ArgumentNullException.ThrowIfNull(nameof(command));
            ArgumentNullException.ThrowIfNull(nameof(dbContext));
            token.ThrowIfCancellationRequested();

            var args = param.GetArray(',', s => s);
            Location? result = null;

            if (args.Length == 2) result = new Location { Settlement = args[0], Extension = args[1] };
            else if (args.Length == 1) result = new Location { Settlement = args[0], Extension = null };
            else throw new CommandValidationException(command, "Location value is not acceptable");

            var doesSettlementExist = await dbContext.PluginRecords.OfTale(command).Where(x => x.BaseId == args[0] && x.Type == CommandIds.Settlement).CountAsync(token);
            if (doesSettlementExist == 0) // here is an example of concurrency issue. maybe settlement command already exists but not yet executed due to ordering or any other reason. we still throw exc but a different one, this is not a validation exc now
                throw new CommandExecutionBlockedException(command, $"Settlement {args[0]} does not exist");

            token.ThrowIfCancellationRequested();
            return result;
        }

        public static async Task<PageTimelineInfo> ParseTimeline<TDbContext>(this Command command, TDbContext dbContext, CancellationToken token)
            where TDbContext : IDbContext
        {
            ArgumentNullException.ThrowIfNull(nameof(command));
            ArgumentNullException.ThrowIfNull(nameof(dbContext));
            token.ThrowIfCancellationRequested();

            var locationPar = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.Location) ?? throw new CommandValidationException(command, "Page command must have location info");
            var stayLocationPar = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.PageStartLocation) ?? throw new CommandValidationException(command, "Page command must have stay location info");
            var datePar = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.Today) ?? throw new CommandValidationException(command, "Page command must have current date info");
            var stayPar = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.Stay) ?? throw new CommandValidationException(command, "Page command must have stay info");

            var location = await locationPar.ParseLocation(command, dbContext, token) ?? throw new CommandValidationException(command, "Current location parsing failed");
            var stayLocation = await stayLocationPar.ParseLocation(command, dbContext, token) ?? throw new CommandValidationException(command, "Stay location parsing failed");
            var date = datePar.Value.ToLong();
            var stay = stayPar.Value.ToInt();

            token.ThrowIfCancellationRequested();

            var travelPar = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.TravelTo);
            Location? travel = null!;
            if (travelPar != null) travel = await travelPar.ParseLocation(command, dbContext, token) ?? throw new CommandValidationException(command, "Travel parsing failed");
            var voyage = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.CommandAttributes.Voyage)?.Value.ToInt() ?? 0;

            token.ThrowIfCancellationRequested();

            return new PageTimelineInfo
            {
                CurrentLocation = new PageTimelineInfo.Location(location.Settlement, location.Extension),
                StayLocation = new PageTimelineInfo.Location(stayLocation.Settlement, stayLocation.Extension),
                Date = date,
                Stay = stay,
                Travel = travel != null ? new PageTimelineInfo.Location(travel.Settlement, travel.Extension) : null,
                Voyage = voyage
            };
        }

        public static ExecutionEvent MapEvent(this Command command, string code, long? date = null, Action<Dictionary<string, string>>? setArgs = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));

            var args = new Dictionary<string, string>();
            setArgs?.Invoke(args);

            var @event = new ExecutionEvent
            {
                TaleId = command.TaleId,
                WriterId = command.WriterId,
                OperationTime = command.OperationTime,
                TaleVersionId = command.TaleVersionId,
                ChapterId = command.ChapterId,
                PageId = command.PageId,
                Code = code,
                Date = date,
                Tags = args
            };

            return @event;
        }
    }
}

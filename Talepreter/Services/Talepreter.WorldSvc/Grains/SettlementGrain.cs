using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.WorldSvc.DBContext;
using Talepreter.Common;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Operations;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.WorldSvc.Grains
{
    public class SettlementGrain : EntityGrain<SettlementGrainState, WorldSvcDBContext, ISettlementGrain>, ISettlementGrain
    {
        public SettlementGrain([PersistentState("persistentState", "WorldSvcStorage")] IPersistentState<SettlementGrainState> persistentState,
            ILogger<SettlementGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchWorldContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            // in Talepreter itself there is nothing to expire about settlements, but this may be required for plugins, and we need to return a settlement instance back for them
            // otherwise there is nothing special here
            var settlements = await dbContext.Settlements.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (settlements.Length > 1) throw new CommandExecutionException(command, "More than one settlement found with unique key");
            if (settlements.Length < 1) throw new CommandExecutionException(command, "Settlement not found for the trigger");
            return settlements[0];
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var settlements = await dbContext.Settlements.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (settlements.Length > 1) throw new CommandExecutionException(command, "More than one person found with unique key");
            Settlement settlement = null!;
            if (settlements.Length == 1)
            {
                settlement = settlements[0];
                settlement.LastUpdatedChapter = command.ChapterId;
                settlement.LastUpdatedPageInChapter = command.PageId;
                settlement.LastUpdate = command.OperationTime;
                settlement.WriterId = command.WriterId;
            }
            else
            {
                if (command.Tag != CommandIds.Settlement) throw new CommandExecutionException(command, "Settlement entity must exist before executing a plugin command");

                settlement = new Settlement
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    IsNew = true
                };
                dbContext.Settlements.Add(settlement);
            }

            token.ThrowIfCancellationRequested();

            // only Settlement command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Settlement) return settlement;

            if (command.NamedParameters != null) // apply named parameters
            {
                try
                {
                    var namedParam = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.SettlementCommand.Visited);
                    if (namedParam != null)
                    {
                        var dates = namedParam.Value.SplitInto(" ");
                        var firstDate = dates[0].ToInt();
                        var lastDate = dates[1].ToInt();

                        if (firstDate == 0 || lastDate == 0) throw new CommandValidationException(command, "Date value is 0");
                        if (!settlement.FirstVisited.HasValue) settlement.FirstVisited = firstDate;
                        if (settlement.LastVisited.HasValue && settlement.LastVisited.Value < lastDate) settlement.LastVisited = lastDate;
                        else if(!settlement.LastVisited.HasValue) settlement.LastVisited = lastDate;
                    }
                }
                catch (Exception ex) // dirty trick to pick validation error
                {
                    throw new CommandValidationException(command, ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(command.Comments)) settlement.Description = command.Comments;

            return settlement;
        }
    }

    [GenerateSerializer]
    public class SettlementGrainState : EntityGrainStateBase
    {
    }
}

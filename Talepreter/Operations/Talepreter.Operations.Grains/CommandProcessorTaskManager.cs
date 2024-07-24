using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Common.RabbitMQ;
using Talepreter.Contracts;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Contracts.Process;
using Talepreter.Exceptions;
using Talepreter.Operations.Processing;

namespace Talepreter.Operations.Grains
{
    public abstract class CommandProcessorTaskManager<TDbContext> : TaskManagerBase, ICommandProcessorTaskManager
        where TDbContext : IDbContext
    {
        private readonly TaskManager _taskMgr;

        public CommandProcessorTaskManager(IGrainFactory grainFactory, IPublisher publisher, ITalepreterServiceIdentifier serviceId, ILogger logger)
            : base(grainFactory, publisher, serviceId, logger)
        {
            _taskMgr = new TaskManager();
        }

        public abstract string ContainerGrainName { get; }

        public async Task Process(int chapter, int page, RawPageData rawPageData)
        {
            try
            {
                foreach (var command in rawPageData.Commands)
                {
                    var processor = _scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
                    var cmd = Map(command, rawPageData.PageBlock, chapter, page);
                    _taskMgr.AppendTasks((token) => processor.Process(cmd, _scope, _tokenSource.Token));
                }

                _taskMgr.Start(_tokenSource.Token);
                var allErrors = _taskMgr.Errors;
                PublishErrors(allErrors);

                ProcessResult res = ProcessResult.None;
                if (_taskMgr.FaultedTaskCount > 0) res = ProcessResult.Faulted;
                else if (_taskMgr.TimedoutTaskCount > 0) res = ProcessResult.Timedout;
                else if (_taskMgr.SuccessfullTaskCount == rawPageData.Commands.Length) res = ProcessResult.Success;
                else res = ProcessResult.Blocked; // edge case, it should not happen

                await ReportBackResult(chapter, page, res);

                _logger.LogInformation($"{_grainLogId} Command processing finalized for page {chapter}#{page} with result {res}, with {_taskMgr.SuccessfullTaskCount}/{_taskMgr.FaultedTaskCount}/{_taskMgr.TimedoutTaskCount} completed/faulted/timedout commands");
            }
            catch (OperationCanceledException)
            {
                try
                {
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(chapter, page, ProcessResult.Timedout);
                    _logger.LogError($"{_grainLogId} Command processing got time out for page {chapter}#{page}");
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Command processing got timeout for page {chapter}#{page} but also recovery failed with error: {cex.Message}");
                }
            }
            catch (Exception ex)
            {
                _tokenSource.Cancel();
                await Task.Delay(500);
                try
                {
                    var allErrors = _taskMgr.Errors;
                    PublishErrors(allErrors);
                    await ReportBackResult(chapter, page, ProcessResult.Faulted);
                    _logger.LogError(ex, $"{_grainLogId} Command processing got an error for page {chapter}#{page}");
                }
                catch (Exception cex)
                {
                    _logger.LogCritical(cex, $"{_grainLogId} Command processing got an error for page {chapter}#{page} but also recovery failed with error: {cex.Message}");
                }
            }
        }

        private async Task ReportBackResult(int chapter, int page, ProcessResult result)
        {
            var pageGrain = _grainFactory.FetchPage(_taleId, _taleVersionId, chapter, page);
            await pageGrain.OnProcessComplete(ContainerGrainName, result);
        }

        private void PublishErrors(Exception[]? errors)
        {
            if (errors == null || errors.Length == 0) return;

            foreach (var err in errors.OfType<CommandException>())
            {
                var response = new ProcessCommandResponse
                {
                    TaleId = err.TaleId,
                    WriterId = _writerId,
                    OperationTime = _operationTime,
                    TaleVersionId = err.TaleVersionId,
                    ChapterId = err.ChapterId,
                    PageId = err.PageId,
                    Index = err.Index,
                    Tag = err.Tag,
                    Target = err.Target,
                    Error = new ErrorInfo
                    {
                        Message = err.Message,
                        Stacktrace = err.StackTrace ?? "",
                        Type = err.GetType().Name,
                    },
                    Service = _serviceId
                };

                _publisher.Publish(response, TalepreterTopology.WriteExchange, TalepreterTopology.WriteResultRoutingKey);
            }
        }

        private ProcessCommand Map(RawCommand cmd, RawPageBlock block, int chapter, int page)
        {
            var namedPars = new List<Contracts.Process.NamedParameter>(cmd.NamedParameters?.Select(x => new Contracts.Process.NamedParameter
            {
                Name = x.Name,
                Value = x.Value,
                Type = x.Type switch
                {
                    Contracts.Orleans.Grains.Command.NamedParameterType.Add => Contracts.Process.NamedParameterType.Add,
                    Contracts.Orleans.Grains.Command.NamedParameterType.Set => Contracts.Process.NamedParameterType.Set,
                    Contracts.Orleans.Grains.Command.NamedParameterType.Reset => Contracts.Process.NamedParameterType.Reset,
                    Contracts.Orleans.Grains.Command.NamedParameterType.Remove => Contracts.Process.NamedParameterType.Remove,
                    _ => throw new GrainOperationException($"Named parameter type {x.Type} is unknown and could not be mapped")
                }
            }).ToArray() ?? [])
            // every command will have those that can be used for various things like events where there is no access to date or location
            {
                new() { Name = CommandIds.CommandAttributes.Today, Type = Contracts.Process.NamedParameterType.Set, Value = (block.Date + block.Stay + (block.Voyage ?? 0)).ToString() },
                new() { Name = CommandIds.CommandAttributes.PageStartAt, Type = Contracts.Process.NamedParameterType.Set, Value = block.Date.ToString() },
                new() { Name = CommandIds.CommandAttributes.PageStartLocation, Type = Contracts.Process.NamedParameterType.Set, Value = block.Location },
                new() { Name = CommandIds.CommandAttributes.Location, Type = Contracts.Process.NamedParameterType.Set, Value = block.Travel ?? block.Location },
                new() { Name = CommandIds.CommandAttributes.Stay, Type = Contracts.Process.NamedParameterType.Set, Value = block.Stay.ToString() },
            };
            if (block.Voyage != null) namedPars.Add(new() { Name = CommandIds.CommandAttributes.Voyage, Type = Contracts.Process.NamedParameterType.Set, Value = block.Voyage.Value.ToString() });
            if (block.Travel != null) namedPars.Add(new() { Name = CommandIds.CommandAttributes.TravelTo, Type = Contracts.Process.NamedParameterType.Set, Value = block.Travel });

            return new ProcessCommand
            {
                TaleId = _taleId,
                TaleVersionId = _taleVersionId,
                OperationTime = _operationTime,
                WriterId = _writerId,
                ChapterId = chapter,
                PageId = page,
                Phase = cmd.Phase,
                BlockInfo = new PageBlock
                {
                    Date = block.Date,
                    Location = block.Location,
                    Stay = block.Stay,
                    Travel = block.Travel,
                    Voyage = block.Voyage
                },
                Index = cmd.Index,
                Prequisite = cmd.Prequisite,
                HasChild = cmd.HasChild,
                Tag = cmd.Tag,
                Target = cmd.Target,
                Parent = cmd.Parent,
                Comments = cmd.Comments,
                ArrayParameters = cmd.ArrayParameters,
                NamedParameters = [.. namedPars]
            };
        }

        protected override void OnDispose()
        {
            _taskMgr?.Dispose();
        }
    }
}

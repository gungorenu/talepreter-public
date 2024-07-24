using Orleans.Runtime;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.System;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Operations.Grains;
using Talepreter.TaleSvc.Grains.GrainStates;

namespace Talepreter.TaleSvc.Grains
{
    [GenerateSerializer]
    public class PageGrain : GrainWithStateBase<PageGrainState>, IPageGrain
    {
        private readonly IPublisher _publisher;

        protected override string Id => $"{State.TaleId}\\{State.TaleVersionId}:{State.ChapterId}#{State.PageId}";

        public PageGrain([PersistentState("persistentState", "TaleSvcStorage")] IPersistentState<PageGrainState> persistentState,
            ILogger<PageGrain> logger,
            IPublisher publisher)
            : base(persistentState, logger)
        {
            _publisher = publisher;
        }

        public Task<ControllerGrainStatus> GetStatus() => Task.FromResult(State.Status);

        public async Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int page)
        {
            var ctx = Validate(nameof(Initialize)).TaleId(taleId).TaleVersionId(taleVersionId).Writer(writerId).Chapter(chapter)
                .IsHealthy(State.Status, ControllerGrainStatus.Idle);

            await SaveState(async (state) =>
            {
                state.TaleId = taleId;
                state.TaleVersionId = taleVersionId;
                state.ChapterId = chapter;
                state.PageId = page;
                state.WriterId = writerId;
                state.LastUpdate = operationTime;

                async Task task1()
                {
                    var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                    await actorContainerGrain.Initialize(taleId, taleVersionId, writerId, operationTime);
                }
                async Task task2()
                {
                    var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                    await anecdoteContainerGrain.Initialize(taleId, taleVersionId, writerId, operationTime);
                }
                async Task task3()
                {
                    var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                    await personContainerGrain.Initialize(taleId, taleVersionId, writerId, operationTime);
                }
                async Task task4()
                {
                    var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);
                    await worldContainerGrain.Initialize(taleId, taleVersionId, writerId, operationTime);
                }
                await Task.WhenAll(task1(), task2(), task3(), task4());
            });
            ctx.Debug($"is initialized");
        }

        public async Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData)
        {
            var ctx = Validate(nameof(BeginProcess)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page).IsNull(rawPageData, nameof(rawPageData))
                .IsHealthy(State.Status, ControllerGrainStatus.Idle);

            await SaveState(async (state) =>
            {
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
                state.Status = ControllerGrainStatus.Processing;

                var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);

                // fire/forget because they will respond back to us, timeout will be handled by orleans reminders
                // note: Execute call is OneWay, so it finishes call and returns immediately
                await actorContainerGrain.BeginProcess(writerId, operationTime, state.ChapterId, state.PageId, rawPageData);
                await anecdoteContainerGrain.BeginProcess(writerId, operationTime, state.ChapterId, state.PageId, rawPageData);
                await personContainerGrain.BeginProcess(writerId, operationTime, state.ChapterId, state.PageId, rawPageData);
                await worldContainerGrain.BeginProcess(writerId, operationTime, state.ChapterId, state.PageId, rawPageData);

                // TODO: start reminder
            });

            ctx.Debug($"initiated process operation");
        }

        public async Task OnProcessComplete(string callerContainer, ProcessResult result)
        {
            var ctx = Validate(nameof(OnProcessComplete)).Initialize(State.TaleId).IsEmpty(callerContainer, nameof(callerContainer))
                .Custom(!State.ProcessResults.TryGetValue(callerContainer, out var value), "Page grain does not recognize the caller");

            if (result == ProcessResult.Success && State.Status != ControllerGrainStatus.Processing)
            {
                ctx.Debug($"container {callerContainer} completed processing with {result} but state is {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"container {callerContainer} completed processing with {result} but state is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            await SaveState(async (state) =>
            {
                state.ProcessResults[callerContainer] = result;
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Cancelled, callerContainer, ProcessResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Timedout, callerContainer, ProcessResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Purged, callerContainer, ProcessResult.Cancelled)) // same state as cancel
                    return; // purged already? we inform back

                if (state.ProcessResults.Values.All(x => x != ProcessResult.None))
                {
                    var result = state.ProcessResults.Values.Aggregate(ProcessResult.None, (a, b) => a | b);
                    // precedence: Faulted > Cancelled > Timeout > Success
                    if (result.HasFlag(ProcessResult.Faulted))
                    {
                        result = ProcessResult.Faulted;
                        state.Status = ControllerGrainStatus.Faulted;
                    }
                    else if (result.HasFlag(ProcessResult.Cancelled))
                    {
                        result = ProcessResult.Cancelled;
                        state.Status = ControllerGrainStatus.Cancelled;
                    }
                    else if (result.HasFlag(ProcessResult.Timedout)
                        || result.HasFlag(ProcessResult.Blocked)) // special case, blocked is internal use status, it should not come here
                    {
                        result = ProcessResult.Timedout;
                        state.Status = ControllerGrainStatus.Timedout;
                    }
                    else if (result != ProcessResult.Success)
                    {
                        ctx.Fatal($"Process result had to be success but it gave another value {result}");
                        return;
                    }
                    else state.Status = ControllerGrainStatus.Processed;

                    var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, state.ChapterId);
                    await chapterGrain.OnProcessComplete(state.PageId, result);
                    ctx.Debug($"all containers processed, final result for processing {result}");
                }
            });
            ctx.Debug($"container {callerContainer} completed processing with {result}");
        }

        public async Task BeginExecute(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginExecute)).Initialize(State.TaleId).Writer(writerId).IsHealthy(State.Status, ControllerGrainStatus.Processed);

            await SaveState(async (state) =>
            {
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
                state.Status = ControllerGrainStatus.Executing;

                var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);

                // fire/forget because they will respond back to us, timeout will be handled by orleans reminders
                // note: Execute call is OneWay, so it finishes call and returns immediately
                await actorContainerGrain.BeginExecute(writerId, operationTime, state.ChapterId, state.PageId);
                await anecdoteContainerGrain.BeginExecute(writerId, operationTime, state.ChapterId, state.PageId);
                await personContainerGrain.BeginExecute(writerId, operationTime, state.ChapterId, state.PageId);
                await worldContainerGrain.BeginExecute(writerId, operationTime, state.ChapterId, state.PageId);

                // TODO: start reminder
            });
            ctx.Debug($"initiated execute operation");
        }

        public async Task OnExecuteComplete(string callerContainer, ExecutionResult result)
        {
            var ctx = Validate(nameof(OnExecuteComplete)).Initialize(State.TaleId).IsEmpty(callerContainer, nameof(callerContainer))
                .Custom(!State.ProcessResults.TryGetValue(callerContainer, out _), "Page grain does not recognize the caller");

            if (result == ExecutionResult.Success && State.Status != ControllerGrainStatus.Executing)
            {
                ctx.Debug($"container {callerContainer} completed executing with {result} but state is {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"container {callerContainer} completed executing with {result} but state is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            await SaveState(async (state) =>
            {
                state.ExecuteResults[callerContainer] = result;
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Cancelled, callerContainer, ExecutionResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Timedout, callerContainer, ExecutionResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Purged, callerContainer, ExecutionResult.Cancelled)) // same state as cancel
                    return; // purged already? we inform back

                if (state.ExecuteResults.Values.All(x => x != ExecutionResult.None))
                {
                    var result = state.ExecuteResults.Values.Aggregate(ExecutionResult.None, (a, b) => a | b);
                    // precedence: Faulted > Cancelled > Timeout > Success
                    if (result.HasFlag(ExecutionResult.Faulted))
                    {
                        result = ExecutionResult.Faulted;
                        state.Status = ControllerGrainStatus.Faulted;
                    }
                    else if (result.HasFlag(ExecutionResult.Cancelled))
                    {
                        result = ExecutionResult.Cancelled;
                        state.Status = ControllerGrainStatus.Cancelled;
                    }
                    else if (result.HasFlag(ExecutionResult.Timedout)
                        || result.HasFlag(ExecutionResult.Blocked)) // special case, blocked is internal use status, it should not come here
                    {
                        result = ExecutionResult.Timedout;
                        state.Status = ControllerGrainStatus.Timedout;
                    }
                    else if (result != ExecutionResult.Success)
                    {
                        ctx.Fatal("Execute result had to be success but it gave another value");
                        return;
                    }
                    else state.Status = ControllerGrainStatus.Executed;

                    var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, state.ChapterId);
                    await chapterGrain.OnExecuteComplete(state.PageId, result);
                    ctx.Debug($"all containers executed, final result for executing {result}");
                }
            });
            ctx.Debug($"container {callerContainer} completed executing with {result}");
        }

        public async Task Stop(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(OnExecuteComplete)).Initialize(State.TaleId).Writer(writerId);

            await SaveState((state) =>
            {
                state.LastUpdate = operationTime;
                state.WriterId = writerId;
                state.Status = ControllerGrainStatus.Cancelled;
                return Task.CompletedTask;
            });
            ctx.Debug($"stopped operations");
        }

        // --

        private async Task<bool> ReportOnProcessComplete(PageGrainState state, ValidationContext ctx, ControllerGrainStatus status, string callerContainer, ProcessResult result)
        {
            if (state.Status == status)
            {
                var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, state.ChapterId);
                await chapterGrain.OnProcessComplete(state.PageId, result);
                ctx.Debug($"container {callerContainer} completed processing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }

        private async Task<bool> ReportOnExecuteComplete(PageGrainState state, ValidationContext ctx, ControllerGrainStatus status, string callerContainer, ExecutionResult result)
        {
            if (state.Status == status)
            {
                var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, state.ChapterId);
                await chapterGrain.OnExecuteComplete(state.PageId, result);
                ctx.Debug($"container {callerContainer} completed executing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }

        /*
        private ProcessCommand Map(Guid writerId, DateTime operationTime, RawCommand cmd, RawPageBlock block)
        {
            return new ProcessCommand
            {
                TaleId = State.TaleId,
                TaleVersionId = State.TaleVersionId,
                OperationTime = operationTime,
                WriterId = writerId,
                ChapterId = State.ChapterId,
                PageId = State.PageId,
                BlockInfo = new PageBlock
                {
                    Date = block.Date,
                    Location = block.Location,
                    Stay = block.Stay,
                    Travel = block.Travel,
                    Voyage = block.Voyage
                },
                Index = cmd.Index,
                Depth = cmd.Depth,
                Prequisite = cmd.Prequisite,
                HasChild = cmd.HasChild,
                Tag = cmd.Tag,
                Target = cmd.Target,
                Parent = cmd.Parent,
                Comments = cmd.Comments,
                ArrayParameters = cmd.ArrayParameters,
                NamedParameters = cmd.NamedParameters?.Select(x => new Contracts.Process.NamedParameter
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
                }).ToArray() ?? []
            };
        }*/
    }
}

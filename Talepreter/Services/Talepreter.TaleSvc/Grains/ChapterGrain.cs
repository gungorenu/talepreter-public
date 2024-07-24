using Orleans.Runtime;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.System;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;
using Talepreter.TaleSvc.Grains.GrainStates;

namespace Talepreter.TaleSvc.Grains
{
    [GenerateSerializer]
    public class ChapterGrain : GrainWithStateBase<ChapterGrainState>, IChapterGrain
    {
        protected override string Id => $"{State.TaleId}\\{State.TaleVersionId}:{State.ChapterId}";

        public ChapterGrain([PersistentState("persistentState", "TaleSvcStorage")] IPersistentState<ChapterGrainState> persistentState, ILogger<ChapterGrain> logger)
            : base(persistentState, logger)
        {
        }

        public Task<ControllerGrainStatus> GetStatus() => Task.FromResult(State.Status);

        public Task<int> LastExecutedPage()
        {
            _ = Validate(nameof(Initialize)).TaleId(State.TaleId).TaleVersionId(State.TaleVersionId);
            if (State.Status != ControllerGrainStatus.Executed &&
                State.Status != ControllerGrainStatus.Published &&
                State.Status != ControllerGrainStatus.Idle) return Task.FromResult(-1);

            for (int p = State.ExecuteResults.Count - 1; p >= 0; p--)
            {
                var pageResult = State.ExecuteResults[p];
                if (pageResult == ExecutionResult.Success) return Task.FromResult(p);
            }
            return Task.FromResult(-1);
        }

        public async Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int? pageFromBackup = null)
        {
            var ctx = Validate(nameof(Initialize)).TaleId(taleId).TaleVersionId(taleVersionId).Writer(writerId).Chapter(chapter).IsHealthy(State.Status, ControllerGrainStatus.Idle);

            await SaveState((state) =>
            {
                if (pageFromBackup != null)
                {
                    for (int p = 0; p <= pageFromBackup.Value; p++)
                    {
                        state.ProcessResults[p] = ProcessResult.Success;
                        state.ExecuteResults[p] = ExecutionResult.Success;
                    }
                }

                state.TaleId = taleId;
                state.TaleVersionId = taleVersionId;
                state.ChapterId = chapter;
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
                return Task.CompletedTask;
            });
            ctx.Debug($"is initialized");
        }

        public async Task<bool> AddPage(Guid writerId, DateTime operationTime, int page)
        {
            var ctx = Validate(nameof(AddPage)).Initialize(State.TaleId).Writer(writerId)
                .IsHealthy(State.Status,
                    ControllerGrainStatus.Idle, // creation from scratch
                    ControllerGrainStatus.Executed, // a middle place, when there are multiple pages to add and in the middle of those pages
                    ControllerGrainStatus.Published) // appending to already existing
                .Page(page).Custom(State.ProcessResults.ContainsKey(page), $"Chapter already has page {page}");

            bool pageExists = false;
            // chapter exists already? check if healthy
            if (State.ExecuteResults.TryGetValue(page, out var execResult))
            {
                pageExists = true;
                switch (execResult)
                {
                    case ExecutionResult.Faulted:
                    case ExecutionResult.Blocked:
                    case ExecutionResult.Timedout:
                        throw new GrainOperationException(this, nameof(AddPage), $"Chapter grain already has page in fault execute state");
                    default: break;
                }
            }
            else if (State.ProcessResults.TryGetValue(page, out var procResult))
            {
                pageExists = true;
                switch (procResult)
                {
                    case ProcessResult.Faulted:
                    case ProcessResult.Blocked:
                    case ProcessResult.Timedout:
                        throw new GrainOperationException(this, nameof(AddPage), $"Chapter grain already has page in fault process state");
                    default: break;
                }
            }

            var pageGrain = GrainFactory.FetchPage(State.TaleId, State.TaleVersionId, State.ChapterId, page);
            var pageStatus = await pageGrain.GetStatus();
            switch (pageStatus)
            {
                case ControllerGrainStatus.Idle:
                case ControllerGrainStatus.Published:
                case ControllerGrainStatus.Executed: break;
                default: throw new GrainOperationException(this, nameof(AddPage), $"Page grain exists and is in a fault state");
            }

            if (pageExists)
            {
                ctx.Debug($"page {page} already exists in healthy state, so not added");
                return false;
            }

            await SaveState(async (state) =>
            {
                await pageGrain.Initialize(state.TaleId, state.TaleVersionId, writerId, operationTime, state.ChapterId, page);
                state.ProcessResults[page] = ProcessResult.None;
                state.ExecuteResults[page] = ExecutionResult.None;
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
            });
            ctx.Debug($"added {page} page");
            return true;
        }

        public async Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData)
        {
            var ctx = Validate(nameof(BeginProcess)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page)
                .IsHealthy(State.Status, ControllerGrainStatus.Idle, ControllerGrainStatus.Executed, ControllerGrainStatus.Published).IsNull(rawPageData, nameof(rawPageData));

            await SaveState(async (state) =>
            {
                state.Status = ControllerGrainStatus.Processing;
                var grain = GrainFactory.FetchPage(State.TaleId, State.TaleVersionId, chapter, page);
                await grain.BeginProcess(writerId, operationTime, chapter, page, rawPageData);
            });
            ctx.Debug($"initiated process operation");
        }

        public async Task OnProcessComplete(int callerPage, ProcessResult result)
        {
            var ctx = Validate(nameof(OnProcessComplete)).Initialize(State.TaleId).Page(callerPage);

            if (result == ProcessResult.Success && State.Status != ControllerGrainStatus.Processing)
            {
                ctx.Debug($"page {callerPage} completed processing with {result} but state is {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"page {callerPage} completed processing with {result} but state is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            if (!State.ProcessResults.ContainsKey(callerPage)) throw new GrainOperationException(this, nameof(OnProcessComplete), $"Chapter grain does not recognize the page");
            await SaveState(async (state) =>
            {
                state.ProcessResults[callerPage] = result;
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Cancelled, callerPage, ProcessResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Timedout, callerPage, ProcessResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Purged, callerPage, ProcessResult.Cancelled)) // same state as cancel
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
                        ctx.Fatal("Process result had to be success but it gave another value");
                        return;
                    }
                    else state.Status = ControllerGrainStatus.Processed;

                    var publishGrain = GrainFactory.FetchPublish(state.TaleId, state.TaleVersionId);
                    await publishGrain.OnProcessComplete(state.ChapterId, callerPage, result);
                    ctx.Debug($"all pages processed, final result for processing {result}");
                }
            });
            ctx.Debug($"page {callerPage} completed processing with {result}");
        }

        public async Task BeginExecute(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginExecute)).Initialize(State.TaleId).Writer(writerId).IsHealthy(State.Status, ControllerGrainStatus.Processed);

            // we have results already? this can happen when we run same version but with minor command changes only
            await SaveState(async (state) =>
            {
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
                state.Status = ControllerGrainStatus.Executing;

                for (int pageId = 0; pageId < state.PageCount(); pageId++)
                {
                    var processResult = state.ProcessResults[pageId];
                    if (processResult != ProcessResult.Success) throw new GrainOperationException(this, nameof(BeginExecute), "Page within chapter has a failure in processing so execution cannot proceed");

                    var executeResult = state.ExecuteResults[pageId];
                    if (executeResult != ExecutionResult.Success && executeResult != ExecutionResult.None) throw new GrainOperationException(this, nameof(BeginExecute), "Page within chapter has a failure in executing so execution cannot proceed");

                    if (executeResult == ExecutionResult.None)
                    {
                        var pageGrain = GrainFactory.FetchPage(state.TaleId, state.TaleVersionId, state.ChapterId, pageId);
                        await pageGrain.BeginExecute(state.WriterId, state.LastUpdate);
                        ctx.Debug($"initiated execute operation");
                        // TODO: start reminder
                        return;
                    }
                }
                ctx.Debug($"could not execute because all chapters are executed");
            });
        }

        public async Task OnExecuteComplete(int callerPage, ExecutionResult result)
        {
            var ctx = Validate(nameof(OnExecuteComplete)).Initialize(State.TaleId).Page(callerPage).Custom(!State.ExecuteResults.ContainsKey(callerPage), "Chapter grain does not have the page");

            if (result == ExecutionResult.Success && State.Status != ControllerGrainStatus.Executing)
            {
                ctx.Debug($"page {callerPage} completed executing with {result} but state is {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"page {callerPage} completed executing with {result} but state is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            await SaveState(async (state) =>
            {
                state.ExecuteResults[callerPage] = result;
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Cancelled, callerPage, ExecutionResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Timedout, callerPage, ExecutionResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Purged, callerPage, ExecutionResult.Cancelled)) // same state as cancel
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
                        ctx.Fatal($"Execute result had to be success but it gave another value {result}");
                        return;
                    }
                    else state.Status = ControllerGrainStatus.Executed;

                    var publishGrain = GrainFactory.FetchPublish(state.TaleId, state.TaleVersionId);
                    await publishGrain.OnExecuteComplete(state.ChapterId, callerPage, result);
                    ctx.Debug($"all pages executed, final result for executing {result}");
                }
            });
            ctx.Debug($"page {callerPage} completed executing with {result}");
        }

        public async Task Stop(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Stop)).Initialize(State.TaleId).Writer(writerId);

            await SaveState(async (state) =>
            {
                state.LastUpdate = operationTime;
                state.WriterId = writerId;
                state.Status = ControllerGrainStatus.Cancelled;

                foreach (var page in state.ProcessResults.Keys)
                {
                    var pageGrain = GrainFactory.FetchPage(state.TaleId, state.TaleVersionId, state.ChapterId, page);
                    await pageGrain.Stop(writerId, operationTime);
                }
            });
            ctx.Debug($"stopped operations");
        }

        // --

        private async Task<bool> ReportOnProcessComplete(ChapterGrainState state, ValidationContext ctx, ControllerGrainStatus status, int callerPage, ProcessResult result)
        {
            if (state.Status == status)
            {
                var publishGrain = GrainFactory.FetchPublish(state.TaleId, state.TaleVersionId);
                await publishGrain.OnProcessComplete(state.ChapterId, callerPage, result);
                ctx.Debug($"page {callerPage} completed processing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }

        private async Task<bool> ReportOnExecuteComplete(ChapterGrainState state, ValidationContext ctx, ControllerGrainStatus status, int callerPage, ExecutionResult result)
        {
            if (state.Status == status)
            {
                var publishGrain = GrainFactory.FetchPublish(state.TaleId, state.TaleVersionId);
                await publishGrain.OnExecuteComplete(state.ChapterId, callerPage, result);
                ctx.Debug($"page {callerPage} completed executing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }
    }
}

using Orleans.Runtime;
using Talepreter.Common;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Contracts.Orleans.System;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Document.DBContext;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;
using Talepreter.TaleSvc.Grains.GrainStates;

namespace Talepreter.TaleSvc.Grains
{
    [GenerateSerializer]
    public class PublishGrain : GrainWithStateBase<PublishGrainState>, IPublishGrain
    {
        private readonly IDocumentDBContext _docDBContext;
        protected override string Id => $"{State.TaleId}\\{State.TaleVersionId}";

        public PublishGrain([PersistentState("persistentState", "TaleSvcStorage")] IPersistentState<PublishGrainState> persistentState, ILogger<PublishGrain> logger, IDocumentDBContext docDBContext)
            : base(persistentState, logger)
        {
            _docDBContext = docDBContext;
        }

        public Task<ControllerGrainStatus> GetStatus() => Task.FromResult(State.Status);

        public Task<ChapterPagePair> LastExecutedPage()
        {
            _ = Validate(nameof(Initialize)).TaleId(State.TaleId).TaleVersionId(State.TaleVersionId);
            if (State.Status != ControllerGrainStatus.Executed &&
                State.Status != ControllerGrainStatus.Published &&
                State.Status != ControllerGrainStatus.Idle) return Task.FromResult(new ChapterPagePair { Chapter = -1, Page = -1 });
            return Task.FromResult(State.LastExecutedPage);
        }

        public async Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Initialize)).TaleId(taleId).TaleVersionId(taleVersionId).Writer(writerId).IsHealthy(State.Status, ControllerGrainStatus.Idle);

            await SaveState(async (state) =>
            {
                state.TaleId = taleId;
                state.TaleVersionId = taleVersionId;
                state.WriterId = writerId;
                state.LastUpdate = operationTime;

                async Task task1()
                {
                    var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                    await actorContainerGrain.Initialize(state.TaleId, state.TaleVersionId, writerId, operationTime);
                }
                async Task task2()
                {
                    var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                    await anecdoteContainerGrain.Initialize(state.TaleId, state.TaleVersionId, writerId, operationTime);
                }
                async Task task3()
                {
                    var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                    await personContainerGrain.Initialize(state.TaleId, state.TaleVersionId, writerId, operationTime);
                }
                async Task task4()
                {
                    var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);
                    await worldContainerGrain.Initialize(state.TaleId, state.TaleVersionId, writerId, operationTime);
                }
                await Task.WhenAll(task1(), task2(), task3(), task4());
            });
            ctx.Debug($"is initialized");
        }

        public async Task<bool> AddChapterPage(Guid writerId, DateTime operationTime, int chapter, int page)
        {
            var ctx = Validate(nameof(AddChapterPage)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page)
                .IsHealthy(State.Status,
                    ControllerGrainStatus.Idle, // creation from scratch
                    ControllerGrainStatus.Executed, // a middle place, when there are multiple pages to add and in the middle of those pages
                    ControllerGrainStatus.Published); // appending to already existing

            bool chapterExists = false;
            // chapter exists already? check if healthy
            if (State.ExecuteResults.TryGetValue(chapter, out var execResult))
            {
                chapterExists = true;
                switch (execResult)
                {
                    case ExecutionResult.Faulted:
                    case ExecutionResult.Blocked:
                    case ExecutionResult.Timedout:
                        throw new GrainOperationException(this, nameof(AddChapterPage), $"Publish grain already has chapter in fault execute state");
                    default: break;
                }
            }
            else if (State.ProcessResults.TryGetValue(chapter, out var procResult))
            {
                chapterExists = true;
                switch (procResult)
                {
                    case ProcessResult.Faulted:
                    case ProcessResult.Blocked:
                    case ProcessResult.Timedout:
                        throw new GrainOperationException(this, nameof(AddChapterPage), $"Publish grain already has chapter in fault process state");
                    default: break;
                }
            }

            var chapterGrain = GrainFactory.FetchChapter(State.TaleId, State.TaleVersionId, chapter);
            if (!chapterExists) await chapterGrain.Initialize(State.TaleId, State.TaleVersionId, writerId, operationTime, chapter);
            var pageAdded = await chapterGrain.AddPage(writerId, operationTime, page);
            if (!pageAdded)
            {
                if (!chapterExists) ctx.Fatal($"page {chapter}#{page} already exists in healthy state but chapter did not exist");
                else ctx.Debug($"page {chapter}#{page} already exists in healthy state, so not added");
                return false;
            }

            await SaveState((state) =>
            {
                state.PublishResults[nameof(IActorContainerGrain)] = PublishResult.None;
                state.PublishResults[nameof(IAnecdoteContainerGrain)] = PublishResult.None;
                state.PublishResults[nameof(IPersonContainerGrain)] = PublishResult.None;
                state.PublishResults[nameof(IWorldContainerGrain)] = PublishResult.None;

                if (!chapterExists)
                {
                    state.ProcessResults[chapter] = ProcessResult.None;
                    state.ExecuteResults[chapter] = ExecutionResult.None;
                }
                else if (pageAdded)
                {
                    state.ProcessResults[chapter] = ProcessResult.None;
                    state.ExecuteResults[chapter] = ExecutionResult.None;
                }

                state.WriterId = writerId;
                state.LastUpdate = operationTime;
                state.Status = ControllerGrainStatus.Idle; // this may overwrite existing status like Executed or Published
                return Task.CompletedTask;
            });
            ctx.Debug($"added {chapter}#{page} page");
            return true;
        }

        public async Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData)
        {
            var ctx = Validate(nameof(BeginProcess)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page)
                .IsHealthy(State.Status, ControllerGrainStatus.Idle, ControllerGrainStatus.Published).IsNull(rawPageData, nameof(rawPageData));

            await SaveState(async (state) =>
            {
                state.Status = ControllerGrainStatus.Processing;
                var grain = GrainFactory.FetchChapter(State.TaleId, State.TaleVersionId, chapter);
                await grain.BeginProcess(writerId, operationTime, chapter, page, rawPageData);
            });
            ctx.Debug($"initiated process operation");
        }

        public async Task OnProcessComplete(int callerChapter, int callerPage, ProcessResult result)
        {
            var ctx = Validate(nameof(OnProcessComplete)).Initialize(State.TaleId).Chapter(callerChapter).Page(callerPage);

            if (result == ProcessResult.Success && State.Status != ControllerGrainStatus.Processing)
            {
                ctx.Debug($"page {callerChapter}#{callerPage} completed processing with {result} but state is {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"page {callerChapter}#{callerPage} completed processing with {result} but tale version is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            if (!State.ProcessResults.ContainsKey(callerChapter)) throw new GrainOperationException(this, nameof(OnProcessComplete), $"Publish grain does not recognize the chapter");
            await SaveState(async (state) =>
            {
                state.ProcessResults[callerChapter] = result;

                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Cancelled, callerChapter, callerPage, ProcessResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Timedout, callerChapter, callerPage, ProcessResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnProcessComplete(state, ctx, ControllerGrainStatus.Purged, callerChapter, callerPage, ProcessResult.Cancelled)) // same state as cancel
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

                    var taleGrain = GrainFactory.FetchTale(state.TaleId);
                    await taleGrain.OnProcessComplete(state.TaleVersionId, callerChapter, callerPage, result);
                    ctx.Debug($"all chapters processed, final result for processing {result}");
                }
            });

            ctx.Debug($"page {callerChapter}#{callerPage} completed processing with {result}");
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

                for (int chapterId = 0; chapterId < state.ChapterCount(); chapterId++)
                {
                    var processResult = state.ProcessResults[chapterId];
                    if (processResult != ProcessResult.Success) throw new GrainOperationException(this, nameof(BeginExecute), "Chapter within publish version has a failure in processing so execution cannot proceed");

                    var executeResult = state.ExecuteResults[chapterId];
                    if (executeResult != ExecutionResult.Success && executeResult != ExecutionResult.None) throw new GrainOperationException(this, nameof(BeginExecute), "Chapter within publish version has a failure in executing so execution cannot proceed");

                    if (executeResult == ExecutionResult.None)
                    {
                        var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, chapterId);
                        await chapterGrain.BeginExecute(state.WriterId, state.LastUpdate);
                        ctx.Debug($"initiated execute operation");
                        // TODO: start reminder
                        return;
                    }
                }
                ctx.Debug($"could not execute because all chapters are executed");
            });
        }

        public async Task OnExecuteComplete(int callerChapter, int callerPage, ExecutionResult result)
        {
            var ctx = Validate(nameof(OnExecuteComplete)).Initialize(State.TaleId).Chapter(callerChapter).Page(callerPage)
                .Custom(!State.ExecuteResults.ContainsKey(callerChapter), "Publish grain does not have the chapter");

            if (result == ExecutionResult.Success && State.Status != ControllerGrainStatus.Executing)
            {
                ctx.Debug($"page {callerChapter}#{callerPage} completed executing with {result} but tale version is at state {State.Status} so skipping");
                return; // duplicate success messages
            }
            else if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"page {callerChapter}#{callerPage} completed executing with {result} but tale version is at a fault state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            await SaveState(async (state) =>
            {
                state.ExecuteResults[callerChapter] = result;

                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Cancelled, callerChapter, callerPage, ExecutionResult.Cancelled))
                    return; // cancelled already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Timedout, callerChapter, callerPage, ExecutionResult.Timedout))
                    return; // timed out already? we inform back
                if (await ReportOnExecuteComplete(state, ctx, ControllerGrainStatus.Purged, callerChapter, callerPage, ExecutionResult.Cancelled)) // same state as cancel
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
                    else
                    {
                        state.Status = ControllerGrainStatus.Executed;
                        state.LastExecutedPage = new ChapterPagePair { Chapter = callerChapter, Page = callerPage };
                    }

                    var taleGrain = GrainFactory.FetchTale(state.TaleId);
                    await taleGrain.OnExecuteComplete(state.TaleVersionId, callerChapter, callerPage, result);
                    ctx.Debug($"all chapters executed, final result for executing {result}");
                }
            });
            ctx.Debug($"page {callerChapter}#{callerPage} completed executing with {result}");
        }

        public async Task Purge(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Purge)).Initialize(State.TaleId).Writer(writerId);

            await SaveState(async (state) =>
            {
                state.LastUpdate = operationTime;
                state.WriterId = writerId;
                state.Status = ControllerGrainStatus.Purged;

                async Task task1()
                {
                    var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                    await actorContainerGrain.Purge();
                }
                async Task task2()
                {
                    var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                    await anecdoteContainerGrain.Purge();
                }
                async Task task3()
                {
                    var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                    await personContainerGrain.Purge();
                }
                async Task task4()
                {
                    var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);
                    await worldContainerGrain.Purge();
                }
                async Task task5()
                {
                    using var source = new CancellationTokenSource();
                    source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);
                    await PurgePublishData(source.Token);
                }
                await Task.WhenAll(task1(), task2(), task3(), task4(), task5());
            });
            ctx.Debug($"purged data");
        }

        public async Task Stop(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Stop)).Initialize(State.TaleId).Writer(writerId);

            await SaveState(async (state) =>
            {
                state.LastUpdate = operationTime;
                state.WriterId = writerId;
                state.Status = ControllerGrainStatus.Cancelled;

                foreach (var chapter in state.ProcessResults.Keys)
                {
                    var chapterGrain = GrainFactory.FetchChapter(state.TaleId, state.TaleVersionId, chapter);
                    await chapterGrain.Stop(state.WriterId, state.LastUpdate);
                }
            });
            ctx.Debug($"stopped operations");
        }

        public async Task BeginPublish(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginPublish)).Initialize(State.TaleId).Writer(writerId).IsHealthy(State.Status, ControllerGrainStatus.Executed, ControllerGrainStatus.Published);

            using var source = new CancellationTokenSource();
            source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);

            // we reset publish state in case it is a republish operation
            await PurgePublishData(source.Token);

            await SaveState(async (state) =>
            {
                if (state.Status == ControllerGrainStatus.Published) // reset previous values
                {
                    state.PublishResults[nameof(IActorContainerGrain)] = PublishResult.None;
                    state.PublishResults[nameof(IAnecdoteContainerGrain)] = PublishResult.None;
                    state.PublishResults[nameof(IPersonContainerGrain)] = PublishResult.None;
                    state.PublishResults[nameof(IWorldContainerGrain)] = PublishResult.None;
                }
                state.Status = ControllerGrainStatus.Publishing;

                var actorContainerGrain = GrainFactory.FetchActorContainer(state.TaleId, state.TaleVersionId);
                var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(state.TaleId, state.TaleVersionId);
                var personContainerGrain = GrainFactory.FetchPersonContainer(state.TaleId, state.TaleVersionId);
                var worldContainerGrain = GrainFactory.FetchWorldContainer(state.TaleId, state.TaleVersionId);

                // fire/forget because they will respond back to us, timeout will be handled by orleans reminders
                // note: Publish call is OneWay, so it finishes call and returns immediately
                await actorContainerGrain.BeginPublish(writerId, operationTime);
                await anecdoteContainerGrain.BeginPublish(writerId, operationTime);
                await personContainerGrain.BeginPublish(writerId, operationTime);
                await worldContainerGrain.BeginPublish(writerId, operationTime);

                // TODO: start reminder
            });
            ctx.Debug($"initiated publish operation");
        }

        public async Task OnPublishComplete(string callerContainer, PublishResult result)
        {
            var ctx = Validate(nameof(OnPublishComplete)).Initialize(State.TaleId).Custom(!State.PublishResults.ContainsKey(callerContainer), "Publish grain does not have the container");

            if (State.Status == ControllerGrainStatus.Cancelled ||
                State.Status == ControllerGrainStatus.Timedout ||
                State.Status == ControllerGrainStatus.Purged)
            {
                ctx.Debug($"completed publishing with {result} but tale version is at state {State.Status} so skipping");
                return; // we ignore if cancel called / timed out / purged
            }

            await SaveState(async (state) =>
            {
                state.PublishResults[callerContainer] = result;
                if (state.PublishResults.All(x => x.Value != PublishResult.None))
                {
                    var result = state.PublishResults.Values.Aggregate(PublishResult.None, (a, b) => a | b);
                    if (result.HasFlag(PublishResult.Faulted))
                    {
                        result = PublishResult.Faulted;
                        state.Status = ControllerGrainStatus.Faulted;
                    }
                    else if (result.HasFlag(PublishResult.Cancelled))
                    {
                        result = PublishResult.Cancelled;
                        state.Status = ControllerGrainStatus.Cancelled;
                    }
                    else if (result.HasFlag(PublishResult.Timedout))
                    {
                        result = PublishResult.Timedout;
                        state.Status = ControllerGrainStatus.Timedout;
                    }
                    else if (result != PublishResult.Success)
                    {
                        ctx.Fatal($"Publish result had to be success but it gave another value {result}");
                        return;
                    }
                    else state.Status = ControllerGrainStatus.Published;

                    var taleGrain = GrainFactory.FetchTale(state.TaleId);
                    await taleGrain.OnPublishComplete(state.TaleVersionId, result);
                    ctx.Debug($"all containers published, final result for publishing {result}");
                }
            });
            ctx.Debug($"container {callerContainer} completed publishing with {result}");
        }

        public async Task BackupTo(Guid writerId, DateTime operationTime, Guid newVersionId)
        {
            var ctx = Validate(nameof(BackupTo)).Initialize(State.TaleId).Writer(writerId).Custom(newVersionId == Guid.Empty, "Publish grain got empty writer id")
                .IsHealthy(State.Status, ControllerGrainStatus.Executed, ControllerGrainStatus.Published);

            // TODO: do this in a better structure perhaps, but must be in parallel
            async Task task1()
            {
                var actorContainerGrain = GrainFactory.FetchActorContainer(State.TaleId, newVersionId);
                await actorContainerGrain.Initialize(State.TaleId, newVersionId, writerId, operationTime, State.TaleVersionId);
            }
            async Task task2()
            {
                var anecdoteContainerGrain = GrainFactory.FetchAnecdoteContainer(State.TaleId, newVersionId);
                await anecdoteContainerGrain.Initialize(State.TaleId, newVersionId, writerId, operationTime, State.TaleVersionId);
            }
            async Task task3()
            {
                var personContainerGrain = GrainFactory.FetchPersonContainer(State.TaleId, newVersionId);
                await personContainerGrain.Initialize(State.TaleId, newVersionId, writerId, operationTime, State.TaleVersionId);
            }
            async Task task4()
            {
                var worldContainerGrain = GrainFactory.FetchWorldContainer(State.TaleId, newVersionId);
                await worldContainerGrain.Initialize(State.TaleId, newVersionId, writerId, operationTime, State.TaleVersionId);
            }
            await Task.WhenAll(task1(), task2(), task3(), task4());

            var target = GrainFactory.FetchPublish(State.TaleId, newVersionId);
            await target.BackupFrom(State.TaleId, newVersionId, writerId, operationTime, State.LastExecutedPage);
            ctx.Debug($"used version data to backup over {newVersionId}");
        }

        public async Task BackupFrom(Guid taleId, Guid taleVersionId, Guid writerId, DateTime lastUpdated, ChapterPagePair lastExecuted)
        {
            var ctx = Validate(nameof(BackupFrom)).TaleId(taleId).TaleVersionId(taleVersionId).Writer(writerId).IsNull(lastExecuted, nameof(lastExecuted));

            await SaveState(async (state) =>
            {
                for (int c = 0; c <= lastExecuted.Chapter; c++)
                {
                    state.ProcessResults[c] = ProcessResult.Success;
                    state.ExecuteResults[c] = ExecutionResult.Success;
                }

                var chapterGrain = GrainFactory.FetchChapter(taleId, taleVersionId, lastExecuted.Chapter);
                await chapterGrain.Initialize(taleId, taleVersionId, writerId, lastUpdated, lastExecuted.Chapter);

                state.TaleId = taleId;
                state.TaleVersionId = taleVersionId;
                state.WriterId = writerId;
                state.LastUpdate = lastUpdated;
                state.Status = ControllerGrainStatus.Executed;
                state.LastExecutedPage = lastExecuted;
            });
            ctx.Debug($"initialized from backup");
        }

        // --

        private async Task PurgePublishData(CancellationToken token)
        {
            await _docDBContext.PurgePublish(State.TaleId, State.TaleVersionId, token);
        }

        private async Task<bool> ReportOnProcessComplete(PublishGrainState state, ValidationContext ctx, ControllerGrainStatus status, int callerChapter, int callerPage, ProcessResult result)
        {
            if (state.Status == status)
            {
                var taleGrain = GrainFactory.FetchTale(state.TaleId);
                await taleGrain.OnProcessComplete(state.TaleVersionId, callerChapter, callerPage, result);
                ctx.Debug($"page {callerChapter}#{callerPage} completed processing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }

        private async Task<bool> ReportOnExecuteComplete(PublishGrainState state, ValidationContext ctx, ControllerGrainStatus status, int callerChapter, int callerPage, ExecutionResult result)
        {
            if (state.Status == status)
            {
                var taleGrain = GrainFactory.FetchTale(state.TaleId);
                await taleGrain.OnExecuteComplete(state.TaleVersionId, callerChapter, callerPage, result);
                ctx.Debug($"page {callerChapter}#{callerPage} completed executing with {result} which is a fault so reporting back");
                return true;
            }
            return false;
        }
    }
}

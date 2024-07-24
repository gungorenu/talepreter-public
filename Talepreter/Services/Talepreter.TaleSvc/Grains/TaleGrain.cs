using Orleans.Runtime;
using Talepreter.Common.RabbitMQ;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Execute;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Contracts.Orleans.Write;
using Talepreter.Contracts.Process;
using Talepreter.Contracts.Publish;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains;
using Talepreter.TaleSvc.Grains.GrainStates;

namespace Talepreter.TaleSvc.Grains
{
    [GenerateSerializer]
    public class TaleGrain : GrainWithStateBase<TaleGrainState>, ITaleGrain
    {
        private readonly IPublisher _publisher;

        protected override string Id => $"{TaleId}";

        public TaleGrain([PersistentState("persistentState", "TaleSvcStorage")] IPersistentState<TaleGrainState> persistentState,
            ILogger<TaleGrain> logger,
            IPublisher publisher)
            : base(persistentState, logger)
        {
            _publisher = publisher;
        }

        private Guid TaleId => GrainReference.GrainId.GetGuidKey();

        public Task<Guid[]> GetVersions()
        {
            return Task.FromResult(State.VersionTracker.ToArray());
        }

        public async Task Initialize(Guid taleVersionId, Guid writerId, DateTime operationTime, Guid? backupOfVersionId = null)
        {
            var ctx = Validate(nameof(Initialize)).TaleVersionId(taleVersionId).Writer(writerId)
                .Custom(backupOfVersionId.HasValue && backupOfVersionId == Guid.Empty, $"Null/Empty argument {nameof(backupOfVersionId)}")
                .Custom(State.VersionTracker.Contains(taleVersionId), "Tale publish id already exists");

            if (backupOfVersionId != null)
            {
                var backupGrain = GrainFactory.FetchPublish(TaleId, backupOfVersionId.Value);
                var stateOfBackup = await backupGrain.GetStatus();
                if (stateOfBackup != Contracts.Orleans.System.ControllerGrainStatus.Published && stateOfBackup != Contracts.Orleans.System.ControllerGrainStatus.Executed)
                    throw new GrainOperationException(this, nameof(Initialize), $"Backup publish is faulty, using that version as backup source is not accepted");

                await backupGrain.BackupTo(writerId, operationTime, taleVersionId);

                await SaveState((state) =>
                {
                    state.VersionTracker.Add(taleVersionId);
                    state.WriterId = writerId;
                    state.LastUpdate = operationTime;
                    return Task.CompletedTask;
                });

                ctx.Information($"initialized {taleVersionId} as backup of {backupOfVersionId}");
            }
            else
            {
                await SaveState(async (state) =>
                {
                    var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
                    await grain.Initialize(TaleId, taleVersionId, writerId, operationTime);
                    state.VersionTracker.Add(taleVersionId);
                    state.WriterId = writerId;
                    state.LastUpdate = operationTime;
                });
                ctx.Information($"initialized {taleVersionId}");
            }
        }

        public async Task<bool> AddChapterPage(Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int page)
        {
            var ctx = Validate(nameof(AddChapterPage)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId).Chapter(chapter).Page(page)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist");

            var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
            var result = await grain.AddChapterPage(writerId, operationTime, chapter, page);
            ctx.Information($"added new page {chapter}#{page} to tale publish {taleVersionId}");
            return result;
        }

        public async Task BeginProcess(Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData)
        {
            var ctx = Validate(nameof(BeginProcess)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId).Chapter(chapter).Page(page)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist").IsNull(rawPageData, nameof(rawPageData));

            var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
            await grain.BeginProcess(writerId, operationTime, chapter, page, rawPageData);
            ctx.Information($"initiated processing for tale publish {taleVersionId}");
        }

        public async Task BeginExecute(Guid taleVersionId, Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginExecute)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist");

            var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
            await grain.BeginExecute(writerId, operationTime);
            ctx.Information($"initiated execution for tale version {taleVersionId}");
        }

        public async Task PurgePublish(Guid taleVersionId, Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Purge)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist");

            await SaveState(async (state) =>
            {
                var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
                await grain.Purge(writerId, operationTime);
                state.VersionTracker.Remove(taleVersionId);
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
            });
            ctx.Information($"purged tale version {taleVersionId}");
        }

        public async Task Purge(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Purge)).Initialize(TaleId).Writer(writerId);

            await SaveState(async (state) =>
            {
                foreach( var entry in state.VersionTracker)
                {
                    var grain = GrainFactory.FetchPublish(TaleId, entry);
                    await grain.Purge(writerId, operationTime);
                }
                state.VersionTracker.Clear();
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
            });
            ctx.Information($"purged all versions of tale");
        }

        public async Task Stop(Guid taleVersionId, Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(Stop)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist");

            await SaveState(async (state) =>
            {
                var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
                await grain.Stop(writerId, operationTime);
                state.WriterId = writerId;
                state.LastUpdate = operationTime;
            });
            ctx.Information($"stopped tale version {taleVersionId}");
        }

        public async Task BeginPublish(Guid taleVersionId, Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginPublish)).Initialize(TaleId).TaleVersionId(taleVersionId).Writer(writerId)
                .Custom(!State.VersionTracker.Contains(taleVersionId), $"Tale publish does not exist");

            var grain = GrainFactory.FetchPublish(TaleId, taleVersionId);
            await grain.BeginPublish(writerId, operationTime);
            ctx.Information($"published tale version {taleVersionId}");
        }

        // --

        public Task OnProcessComplete(Guid taleVersionId, int callerChapter, int callerPage, ProcessResult result)
        {
            var ctx = Validate(nameof(OnProcessComplete)).Initialize(TaleId).TaleVersionId(taleVersionId).Chapter(callerChapter).Page(callerPage);
            if (!State.VersionTracker.Contains(taleVersionId))
            {
                ctx.Debug($"tale version {taleVersionId} with {callerChapter}#{callerPage} completed processing with {result} but tale version is not known so doing nothing");
                return Task.CompletedTask;
            }

            ProcessStatus processResult;
            if (result.HasFlag(ProcessResult.Faulted) ||
                result.HasFlag(ProcessResult.Cancelled) ||
                result.HasFlag(ProcessResult.Blocked)) processResult = ProcessStatus.Faulted;
            else if (result.HasFlag(ProcessResult.Timedout)) processResult = ProcessStatus.Timeout;
            else if (result == ProcessResult.Success) processResult = ProcessStatus.Success;
            else throw new GrainOperationException(this, nameof(OnProcessComplete), $"Operation result {result} is not recognized");

            // publish message, dont call grains due to deadlock potential
            _publisher.Publish(new ProcessStatusUpdate
            {
                TaleId = TaleId,
                Status = processResult,
                TaleVersionId = taleVersionId,
                ChapterId = callerChapter,
                PageId = callerPage,
            }, TalepreterTopology.EventExchange, TalepreterTopology.StatusUpdateRoutingKey);

            ctx.Information($"tale version {taleVersionId} with {callerChapter}#{callerPage} completed processing with {result}");
            return Task.CompletedTask;
        }

        public Task OnExecuteComplete(Guid taleVersionId, int callerChapter, int callerPage, ExecutionResult result)
        {
            var ctx = Validate(nameof(OnExecuteComplete)).Initialize(TaleId).TaleVersionId(taleVersionId).Chapter(callerChapter).Page(callerPage);
            if (!State.VersionTracker.Contains(taleVersionId))
            {
                ctx.Debug($"tale version {taleVersionId} with {callerChapter}#{callerPage} completed executing with {result} but tale version is not known so doing nothing");
                return Task.CompletedTask;
            }

            ExecuteStatus executeResult;
            if (result.HasFlag(ExecutionResult.Faulted) ||
                result.HasFlag(ExecutionResult.Cancelled) ||
                result.HasFlag(ExecutionResult.Blocked)) executeResult = ExecuteStatus.Faulted;
            else if (result.HasFlag(ExecutionResult.Timedout)) executeResult = ExecuteStatus.Timeout;
            else if (result == ExecutionResult.Success) executeResult = ExecuteStatus.Success;
            else throw new GrainOperationException(this, nameof(OnExecuteComplete), $"Operation result {result} is not recognized");

            // publish message, dont call grains due to deadlock potential
            _publisher.Publish(new ExecuteStatusUpdate
            {
                TaleId = TaleId,
                Status = executeResult,
                TaleVersionId = taleVersionId,
                ChapterId = callerChapter,
                PageId = callerPage,
            }, TalepreterTopology.EventExchange, TalepreterTopology.StatusUpdateRoutingKey);

            ctx.Information($"tale version {taleVersionId} with {callerChapter}#{callerPage} completed executing with {result}");
            return Task.CompletedTask;
        }

        public Task OnPublishComplete(Guid taleVersionId, PublishResult result)
        {
            var ctx = Validate(nameof(OnPublishComplete)).Initialize(TaleId).TaleVersionId(taleVersionId);
            if (!State.VersionTracker.Contains(taleVersionId))
            {
                ctx.Debug($"tale version {taleVersionId} completed publishing with {result} but tale version is not known so doing nothing");
                return Task.CompletedTask;
            }

            PublishStatus publishResult;
            if (result.HasFlag(PublishResult.Faulted) || result.HasFlag(PublishResult.Cancelled)) publishResult = PublishStatus.Faulted;
            else if (result.HasFlag(PublishResult.Timedout)) publishResult = PublishStatus.Timeout;
            else if (result == PublishResult.Success) publishResult = PublishStatus.Success;
            else throw new GrainOperationException(this, nameof(OnPublishComplete), $"Operation result {result} is not recognized");

            // publish message, dont call grains due to deadlock potential
            _publisher.Publish(new PublishStatusUpdate
            {
                TaleId = TaleId,
                Status = publishResult,
                TaleVersionId = taleVersionId,

            }, TalepreterTopology.EventExchange, TalepreterTopology.StatusUpdateRoutingKey);

            ctx.Information($"tale version {taleVersionId} completed publishing with {result}");
            return Task.CompletedTask;
        }
    }
}

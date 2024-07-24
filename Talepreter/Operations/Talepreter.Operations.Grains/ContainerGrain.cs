using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Common;
using Talepreter.Common.RabbitMQ.Interfaces;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Exceptions;
using Talepreter.Operations.Grains.States;

namespace Talepreter.Operations.Grains
{
    public abstract class ContainerGrain<TState, TDbContext, TSelf> : GrainWithStateBase<TState>, IContainerGrain
        where TState : ContainerGrainStateBase
        where TDbContext : IDbContext
        where TSelf : IContainerGrain
    {
        private readonly ITalepreterServiceIdentifier _serviceId;
        private readonly IPublisher _publisher;

        protected override string Id => $"{State.TaleId}\\{State.TaleVersionId}:{GetType().Name}";

        protected ContainerGrain(IPersistentState<TState> persistentState, ILogger logger, ITalepreterServiceIdentifier serviceId, IPublisher publisher)
            : base(persistentState, logger)
        {
            _serviceId = serviceId;
            _publisher = publisher;
        }

        public async Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, Guid? backupOfVersionId = null)
        {
            var ctx = Validate(nameof(Initialize)).TaleId(taleId).TaleVersionId(taleVersionId).Writer(writerId);

            if (backupOfVersionId != null)
            {
                var backupGrain = GrainFactory.FetchPublish(taleId, backupOfVersionId.Value);
                var stateOfBackup = await backupGrain.GetStatus();
                if (stateOfBackup != Contracts.Orleans.System.ControllerGrainStatus.Published && stateOfBackup != Contracts.Orleans.System.ControllerGrainStatus.Executed)
                    throw new GrainOperationException(this, nameof(Initialize), $"Backup publish is faulty, using that version as backup source is not accepted");

                using var source = new CancellationTokenSource();
                source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);
                var token = source.Token;

                try
                {
                    using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>()
                        ?? throw new GrainOperationException(this, nameof(Purge), $"{typeof(TDbContext).Name} initialization failed");
                    await dbContext.BackupEntitiesTo(taleId, backupOfVersionId.Value, taleVersionId, token);
                }
                catch (Exception ex)
                {
                    ctx.Error(ex, $"Container grain could not backup for tale version {backupOfVersionId} to {taleVersionId}");
                    throw new GrainOperationException(this, nameof(Initialize), ex.Message); // TODO: do not hide hierarchy
                }

                await SaveState((state) =>
                {
                    state.TaleId = taleId;
                    state.TaleVersionId = taleVersionId;
                    state.WriterId = writerId;
                    state.LastUpdated = operationTime;
                    return Task.CompletedTask;
                });
                ctx.Debug($"is initialized as backup of {backupOfVersionId}");
            }
            else
            {
                await SaveState((state) =>
                {
                    state.TaleId = taleId;
                    state.TaleVersionId = taleVersionId;
                    state.WriterId = writerId;
                    state.LastUpdated = operationTime;
                    return Task.CompletedTask;
                });
                ctx.Debug($"is initialized");
            }
        }

        public Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData)
        {
            var ctx = Validate(nameof(BeginProcess)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page).IsNull(rawPageData, nameof(rawPageData));

            try
            {
                // important, we cannot do "using" here, we need to instantiate here, but dispose it in the task below
                var commandProcessorMgr = ServiceProvider.GetRequiredService<ICommandProcessorTaskManager>()
                    ?? throw new GrainOperationException(this, nameof(BeginProcess), $"could not initialize {typeof(ICommandProcessorTaskManager).Name}");

                // we should not access grain stuff from that Task.Run block
                var taleId = State.TaleId;
                var taleVersionId = State.TaleVersionId;
                var grainId = GrainReference.GrainId;
                var grainLogId = $"[{typeof(TSelf).Name}:<{nameof(BeginProcess)}>] {Id}";
                var scope = ServiceProvider.CreateScope();

                // fire/forget, we will get response from the task when it is complete
                _ = Task.Run(async () =>
                {
                    commandProcessorMgr.Initialize(scope, taleId, taleVersionId, writerId, operationTime, grainId, grainLogId);
                    await commandProcessorMgr.Process(chapter, page, rawPageData);
                    commandProcessorMgr.Dispose();
                });

                // TODO: initialize reminder for timeout
                ctx.Debug($"Container grain initiated processing for {chapter}#{page} with {rawPageData.Commands.Length} commands");
            }
            catch (Exception ex)
            {
                ctx.Error(ex, $"Container grain faulted in execute commands for {chapter}#{page}");
                throw new GrainOperationException(this, nameof(BeginExecute), ex.Message); // TODO: do not hide hierarchy
            }
            return Task.CompletedTask;
        }

        public Task BeginExecute(Guid writerId, DateTime operationTime, int chapter, int page)
        {
            var ctx = Validate(nameof(BeginExecute)).Initialize(State.TaleId).Writer(writerId).Chapter(chapter).Page(page);

            try
            {
                // important, we cannot do "using" here, we need to instantiate here, but dispose it in the task below
                var commandExecutorMgr = ServiceProvider.GetRequiredService<ICommandExecutorTaskManager>()
                    ?? throw new GrainOperationException(this, nameof(BeginExecute), $"could not initialize {typeof(ICommandExecutorTaskManager).Name}");

                // we should not access grain stuff from that Task.Run block
                var taleId = State.TaleId;
                var taleVersionId = State.TaleVersionId;
                var grainId = GrainReference.GrainId;
                var grainLogId = $"[{typeof(TSelf).Name}:<{nameof(BeginExecute)}>] {Id}";
                var scope = ServiceProvider.CreateScope();

                // fire/forget, we will get response from the task when it is complete
                _ = Task.Run(async () =>
                {
                    commandExecutorMgr.Initialize(scope, taleId, taleVersionId, writerId, operationTime, grainId, grainLogId);
                    await commandExecutorMgr.Execute(chapter, page);
                    commandExecutorMgr.Dispose();
                });

                // TODO: initialize reminder for timeout
                ctx.Debug($"Container grain initiated executing for {chapter}#{page}");
            }
            catch (Exception ex)
            {
                ctx.Error(ex, $"Container grain faulted in execute commands for {chapter}#{page}");
                throw new GrainOperationException(this, nameof(BeginExecute), ex.Message); // TODO: do not hide hierarchy
            }
            return Task.CompletedTask;
        }

        public async Task Purge()
        {
            var ctx = Validate(nameof(Purge)).Initialize(State.TaleId);

            using var source = new CancellationTokenSource();
            source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);
            var token = source.Token;

            try
            {
                using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>()
                    ?? throw new GrainOperationException(this, nameof(Purge), $"{typeof(TDbContext).Name} initialization failed");
                await dbContext.PurgeEntities(State.TaleId, State.TaleVersionId, token);
                ctx.Debug($"purged data");
            }
            catch (Exception ex)
            {
                ctx.Error(ex, $"Container grain could not purge");
                throw new GrainOperationException(this, nameof(Purge), ex.Message); // TODO: do not hide hierarchy
            }
        }

        public Task BeginPublish(Guid writerId, DateTime operationTime)
        {
            var ctx = Validate(nameof(BeginPublish)).Initialize(State.TaleId).Writer(writerId);

            try
            {
                // important, we cannot do "using" here, we need to instantiate here, but dispose it in the task below
                var commandExecutorMgr = ServiceProvider.GetRequiredService<IEntityPublisherTaskManager>()
                    ?? throw new GrainOperationException(this, nameof(BeginPublish), $"could not initialize {typeof(IEntityPublisherTaskManager).Name}");

                // we should not access grain stuff from that Task.Run block
                var taleId = State.TaleId;
                var taleVersionId = State.TaleVersionId;
                var grainId = GrainReference.GrainId;
                var grainLogId = $"[{typeof(TSelf).Name}:<{nameof(BeginPublish)}>] {Id}";
                var scope = ServiceProvider.CreateScope();

                // fire/forget, we will get response from the task when it is complete
                _ = Task.Run(async () =>
                {
                    commandExecutorMgr.Initialize(scope, taleId, taleVersionId, writerId, operationTime, grainId, grainLogId);
                    await commandExecutorMgr.Publish();
                    commandExecutorMgr.Dispose();
                });

                // TODO: initialize reminder for timeout
                ctx.Debug($"Container grain initiated publishing entities");
            }
            catch (Exception ex)
            {
                ctx.Error(ex, $"Container grain initiated publishing entities entities");
                throw new GrainOperationException(this, nameof(BeginPublish), ex.Message); // TODO: do not hide hierarchy
            }
            return Task.CompletedTask;
        }

        public async Task OnPublishCompleted(PublishResult result)
        {
            var ctx = Validate(nameof(OnPublishCompleted)).Initialize(State.TaleId);

            if (result == PublishResult.None)
            {
                ctx.Fatal("Container grain got no result after execution");
                return; // this should not happen, maybe throw exception?
            }

            using var source = new CancellationTokenSource();
            source.CancelAfter(Timeouts.GrainOperationTimeout * 1000);
            var token = source.Token;
            try
            {
                using var dbContext = _scope.ServiceProvider.GetRequiredService<TDbContext>()
                    ?? throw new CommandProcessingException($"{typeof(TDbContext).Name} initialization failed");
                if (result == PublishResult.Success)
                {
                    // TODO: publish will not look at DB now
                    var areAllCommandsExecuted = false; // await dbContext.AreAllEntitiesPublished(State.TaleId, State.TaleVersionId, token);
                    // success, all done, we can inform page
                    if (areAllCommandsExecuted)
                    {
                        var publishGrain = GrainFactory.FetchPublish(State.TaleId, State.TaleVersionId);
                        await publishGrain.OnPublishComplete(GrainName, PublishResult.Success);
                        ctx.Debug($"published data");
                        return; // we are done for this chapter and page
                    }
                    //else means we are not done yet, we wait
                    ctx.Debug($"published data but not complete yet");
                    return;
                }
                else // we report our issue
                {
                    var publishGrain = GrainFactory.FetchPublish(State.TaleId, State.TaleVersionId);
                    await publishGrain.OnPublishComplete(GrainName, result);
                    ctx.Debug($"publishing data faulted");
                }
            }
            catch (Exception ex)
            {
                ctx.Error(ex, $"Container grain could not check publish operation result");
                throw new GrainOperationException(this, nameof(OnPublishCompleted), ex.Message); // TODO: do not hide hierarchy
            }
        }

        /// <summary>
        /// service specific info, queue bindings depend on this
        /// </summary>
        protected abstract string ExecuteRoutingKey { get; }

        /// <summary>
        /// service specific info, queue bindings depend on this
        /// </summary>
        protected abstract string PublishRoutingKey { get; }

        /// <summary>
        /// owner grain name, required for informing page grain
        /// </summary>
        protected abstract string GrainName { get; }
    }
}

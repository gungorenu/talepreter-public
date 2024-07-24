using Orleans.Concurrency;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.Publish;
using Talepreter.Contracts.Orleans.Write;

namespace Talepreter.Contracts.Orleans.Grains
{
    public interface ITaleGrain : IGrainWithGuidKey
    {
        /// <summary>
        /// returns all known versions
        /// </summary>
        [ReadOnly]
        Task<Guid[]> GetVersions();

        /// <summary>
        /// sets up publish object ready for processing
        /// </summary>
        Task Initialize(Guid taleVersionId, Guid writerId, DateTime operationTime, Guid? backupOfVersionId = null);

        /// <summary>
        /// adds a new page to tale (may also add a new chapter if it does not exist)
        /// </summary>
        [AlwaysInterleave, ReadOnly]
        Task<bool> AddChapterPage(Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int page);

        /// <summary>
        /// adds commands of the page and starts processing
        /// </summary>
        Task BeginProcess(Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData);

        /// <summary>
        /// clears up a used tale publish id, deleting all data it stores in every svc and also publish data
        /// </summary>
        Task PurgePublish(Guid taleVersionId, Guid writerId, DateTime operationTime);

        /// <summary>
        /// clears up all publishes from tale
        /// </summary>
        Task Purge(Guid writerId, DateTime operationTime);

        /// <summary>
        /// stops the specified publish operation if it continues
        /// </summary>
        Task Stop(Guid taleVersionId, Guid writerId, DateTime operationTime);

        /// <summary>
        /// initiates execution procedure for the tale publish object
        /// </summary>
        [AlwaysInterleave, ReadOnly]
        Task BeginExecute(Guid taleVersionId, Guid writerId, DateTime operationTime);

        /// <summary>
        /// initiates publishg procedure for the tale publish object
        /// </summary>
        [AlwaysInterleave, ReadOnly]
        Task BeginPublish(Guid taleVersionId, Guid writerId, DateTime operationTime);

        // --

        /// <summary>
        /// response of process command. publish id will be tracked for successful publishes and for copy purposes
        /// </summary>
        /// <param name="taleVersionId">publish id of caller grain</param>
        [AlwaysInterleave, ReadOnly]
        Task OnProcessComplete(Guid taleVersionId, int chapter, int page, ProcessResult result);

        /// <summary>
        /// response of execute command. publish id will be tracked for successful publishes and for copy purposes
        /// </summary>
        /// <param name="taleVersionId">publish id of caller grain</param>
        [AlwaysInterleave, ReadOnly]
        Task OnExecuteComplete(Guid taleVersionId, int chapter, int page, ExecutionResult result);

        /// <summary>
        /// response of publish command. publish id will be tracked for successful publishes and for copy purposes
        /// </summary>
        [AlwaysInterleave, ReadOnly]
        Task OnPublishComplete(Guid taleVersionId, PublishResult result);
    }
}

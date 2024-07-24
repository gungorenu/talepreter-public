using Orleans.Concurrency;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains.Command;
using Talepreter.Contracts.Orleans.System;
using Talepreter.Contracts.Orleans.Write;

namespace Talepreter.Contracts.Orleans.Grains
{
    public interface IChapterGrain : IGrainWithStringKey
    {
        /// <summary>
        /// gets status of chapter (all pages merged)
        /// </summary>
        [ReadOnly, AlwaysInterleave]
        Task<ControllerGrainStatus> GetStatus();

        /// <summary>
        /// gets the last successfully executed page, state must be healthy or returns -1
        /// </summary>
        [ReadOnly, AlwaysInterleave]
        Task<int> LastExecutedPage();

        /// <summary>
        /// initialization from values
        /// </summary>
        Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, int chapter, int? pageFromBackup = null);

        /// <summary>
        /// adds a new page to tale
        /// can be called again for existing pages
        /// </summary>
        Task<bool> AddPage(Guid writerId, DateTime operationTime, int page);

        /// <summary>
        /// adds commands of the page and starts processing
        /// </summary>
        Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData);

        /// <summary>
        /// stops current operation, will not initiate next page operation
        /// </summary>
        Task Stop(Guid writerId, DateTime operationTime);

        /// <summary>
        /// call current page execution operation, it will also track it and initiate next page if previous one is completed
        /// starts operations async, without awaiting. response will come from others when their stuff is complete
        /// calling this again will execute from scratch but pages might give success result immediately if they already have done it before
        /// </summary>
        Task BeginExecute(Guid writerId, DateTime operationTime);

        // --

        /// <summary>
        /// response of process command. if all pages of chapter give answer then it will response back to chapter grain with total status (success, failed etc)
        /// </summary>
        /// <param name="callerPage">page id of caller grain</param>
        Task OnProcessComplete(int callerPage, ProcessResult result);

        /// <summary>
        /// response of execute command. if page gives response then if success then will continue with next page
        /// if all pages are succesful then will respond back to publish grain with result
        /// </summary>
        /// <param name="callerPage">page that completed execution</param>
        Task OnExecuteComplete(int callerPage, ExecutionResult result);
    }
}

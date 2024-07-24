using Orleans.Concurrency;
using Talepreter.Contracts.Orleans.Grains.Command;

namespace Talepreter.Contracts.Orleans.Grains
{
    /// <summary>
    /// an interface for container grains only, not supposed to be used elsewhere
    /// </summary>
    public interface IContainerGrain: IGrainWithStringKey
    {
        /// <summary>
        /// sets up the base information for the grain for further operations
        /// </summary>
        Task Initialize(Guid taleId, Guid taleVersionId, Guid writerId, DateTime operationTime, Guid? backupOfVersion = null);

        /// <summary>
        /// deletes all command data
        /// </summary>
        Task Purge();

        /// <summary>
        /// starts command processing procedure
        /// it waits until all commands are processed and has a result. it will also have a timer set to check itself and check table a final time before raising timed out response back
        /// </summary>
        [OneWay]
        Task BeginProcess(Guid writerId, DateTime operationTime, int chapter, int page, RawPageData rawPageData);

        /// <summary>
        /// starts command execution procedure, posts all commands to execute queue
        /// it waits until all commands are executed and has a result. it will also have a timer set to check itself and check table a final time before raising timed out response back
        /// timeout is 5sec, which should be long enough
        /// </summary>
        [OneWay]
        Task BeginExecute(Guid writerId, DateTime operationTime, int chapter, int page);

        /// <summary>
        /// initiates publish operation on all db content
        /// </summary>
        [OneWay]
        Task BeginPublish(Guid writerId, DateTime operationTime);
    }
}

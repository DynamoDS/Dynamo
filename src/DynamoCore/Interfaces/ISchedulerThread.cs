using Dynamo.Core.Threading;

namespace Dynamo.Interfaces
{
    public interface ISchedulerThread
    {
        /// <summary>
        /// DynamoScheduler calls this method to initialize and start this 
        /// instance of scheduler thread. This call marks the point from which 
        /// it is safe to call into DynamoScheduler.
        /// </summary>
        /// <param name="owningScheduler">A reference to the DynamoScheduler 
        /// object which owns this instance of scheduler thread.</param>
        void Initialize(IScheduler owningScheduler);

        /// <summary>
        /// DynamoScheduler calls this method to shutdown the scheduler thread.
        /// </summary>
        void Shutdown();
    }

}

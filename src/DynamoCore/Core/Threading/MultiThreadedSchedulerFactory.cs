using Dynamo.Scheduler;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// A SchedulerFactory that creates a new scheduler on each invocation of 
    /// NewScheduler also passing it a new thread.
    /// </summary>
    public class MultiThreadedSchedulerFactory : ISchedulerFactory
    {
        /// <summary>
        /// Create a new MultiSchedulerFactory.
        /// </summary>
        public MultiThreadedSchedulerFactory() { }

        /// <summary>
        /// Create a completely new Scheduler for use with a Workspace along with a new
        /// DynamoSchedulerThread
        /// </summary>
        /// <returns></returns>
        public DynamoScheduler Build()
        {
            return new DynamoScheduler(new DynamoSchedulerThread(), TaskProcessMode.Asynchronous);
        }
    }
}
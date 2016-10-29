using Dynamo.Scheduler;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// A SchedulerFactory that creates a new scheduler on each invocation of 
    /// NewScheduler also passing it a new thread.
    /// </summary>
    public class MultiThreadedSchedulerFactory : ISchedulerFactory
    {
        TaskProcessMode processMode;

        /// <summary>
        /// Create a new MultiSchedulerFactory.
        /// </summary>
        public MultiThreadedSchedulerFactory(TaskProcessMode processMode)
        {
            this.processMode = processMode;
        }

        /// <summary>
        /// Create a completely new Scheduler for use with a Workspace along with a new
        /// DynamoSchedulerThread
        /// </summary>
        /// <returns></returns>
        public DynamoScheduler Build()
        {
            return new DynamoScheduler(new DynamoSchedulerThread(), processMode);
        }
    }
}
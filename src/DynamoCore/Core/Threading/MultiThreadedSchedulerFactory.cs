namespace Dynamo.Core.Threading
{
    /// <summary>
    /// A SchedulerFactory that creates a new scheduler on each invocation of 
    /// NewScheduler also passing it a new thread.
    /// </summary>
    public class MultiThreadedSchedulerFactory : ISchedulerFactory
    {
        private readonly bool isTestMode;

        /// <summary>
        /// Create a new MultiSchedulerFactory.
        /// </summary>
        /// <param name="isTestMode"></param>
        public MultiThreadedSchedulerFactory(bool isTestMode = false)
        {
            this.isTestMode = isTestMode;
        }

        /// <summary>
        /// Create a completely new Scheduler for use with a Workspace along with a new
        /// DynamoSchedulerThread
        /// </summary>
        /// <returns></returns>
        public DynamoScheduler Build()
        {
            return new DynamoScheduler(new DynamoSchedulerThread(), this.isTestMode);
        }
    }
}
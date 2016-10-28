using Dynamo.Scheduler;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// A SchedulerFactory that produces Schedulers that all use
    /// the same thread
    /// </summary>
    public class SingleThreadedSchedulerFactory : ISchedulerFactory
    {
        private readonly ISchedulerThread thread;

        /// <summary>
        /// Create a new SingleThreadedSchedulerFactory that reuses the thread passed
        /// as argument.
        /// </summary>
        /// <param name="thread">The scheduler thread to be used</param>
        public SingleThreadedSchedulerFactory(ISchedulerThread thread)
        {
            this.thread = thread;
        }

        /// <summary>
        /// Create a new Scheduler using the same thread
        /// </summary>
        /// <returns></returns>
        public DynamoScheduler Build()
        {
            return new DynamoScheduler(thread, TaskProcessMode.Synchronous);
        }
    }
}
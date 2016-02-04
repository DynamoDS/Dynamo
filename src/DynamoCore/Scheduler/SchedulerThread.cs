using System.Threading;

namespace Dynamo.Scheduler
{
    class DynamoSchedulerThread : ISchedulerThread
    {
        private Thread internalThread;
        private IScheduler scheduler;

        /// <summary>
        /// DynamoScheduler calls this method to initialize and start this
        /// instance of scheduler thread. This call marks the point from which
        /// it is safe to call into DynamoScheduler.
        /// </summary>
        /// <param name="owningScheduler">A reference to the DynamoScheduler
        /// object which owns this instance of scheduler thread.</param>
        public void Initialize(IScheduler owningScheduler)
        {
            scheduler = owningScheduler;
            internalThread = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = "DynamoSchedulerThread",
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };

            internalThread.Start();
        }

        /// <summary>
        /// DynamoScheduler calls this method to shutdown the scheduler thread.
        /// </summary>
        public void Shutdown()
        {
            internalThread.Join(); // Wait for background thread to terminate.
        }

        private void ThreadProc()
        {
            while (true)
            {
                // Process exactly one task (if any) and wait.
                const bool waitIfTaskQueueIsEmpty = true;
                if (!scheduler.ProcessNextTask(waitIfTaskQueueIsEmpty))
                    break;
            }
        }
    }
}

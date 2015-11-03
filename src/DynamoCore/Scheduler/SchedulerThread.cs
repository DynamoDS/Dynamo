using System.Threading;

namespace Dynamo.Scheduler
{
    class DynamoSchedulerThread : ISchedulerThread
    {
        private Thread internalThread;
        private IScheduler scheduler;

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

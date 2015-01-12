using Dynamo.Interfaces;
using System.Threading;

namespace Dynamo.Core.Threading
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
                Name = "DynamoSchedulerThread"
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

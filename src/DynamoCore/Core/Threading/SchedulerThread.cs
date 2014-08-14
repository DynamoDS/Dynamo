using Dynamo.Interfaces;
using System.Threading;

namespace Dynamo.Core.Threading
{
    class DynamoSchedulerThread : ISchedulerThread
    {
        private Thread internalThread;
        private DynamoScheduler scheduler;
        private readonly AutoResetEvent shutdownEvent = new AutoResetEvent(false);

        public void Initialize(DynamoScheduler owningScheduler)
        {
            scheduler = owningScheduler;
            internalThread = new Thread(ThreadProc) { IsBackground = true };
            internalThread.Start();
        }

        public void Shutdown()
        {
            shutdownEvent.Set();
            internalThread.Join();
        }

        private void ThreadProc()
        {
            while (true)
            {
                // Process exactly one task (if any) and wait.
                const bool waitIfTaskQueueIsEmpty = true;
                scheduler.ProcessNextTask(waitIfTaskQueueIsEmpty);

                if (shutdownEvent.WaitOne(1))
                    break; // Shutdown requested.
            }
        }
    }
}

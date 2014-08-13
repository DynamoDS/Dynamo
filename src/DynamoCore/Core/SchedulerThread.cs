using System.Threading;

namespace Dynamo.Core.Threading
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
        void Initialize(DynamoScheduler owningScheduler);

        /// <summary>
        /// DynamoScheduler calls this method to shutdown the scheduler thread.
        /// </summary>
        void Shutdown();
    }

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

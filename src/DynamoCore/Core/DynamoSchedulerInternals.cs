using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamo.Core
{
    partial class DynamoScheduler
    {
        #region TimeStampGenerator Nested Class

        class TimeStampGenerator
        {
            private long timeStampValue = 1024;
            private readonly object timeStampMutex = new object();

            internal long Next
            {
                get
                {
                    lock (timeStampMutex)
                    {
                        long value = timeStampValue;
                        timeStampValue = timeStampValue + 1;
                        return value;
                    }
                }
            }
        }

        #endregion

        #region Private Class Data Members

        private enum EventIndex : int
        {
            TaskAvailable, Shutdown
        }

        private ManualResetEvent[] waitHandles = new ManualResetEvent[]
        {
              new ManualResetEvent(false), // Task available event
              new ManualResetEvent(false)  // Scheduler shutdown event
        };

        private bool taskQueueModified = false;
        private List<AsyncTask> taskQueue = new List<AsyncTask>();
        private TimeStampGenerator timeStamp = new TimeStampGenerator();

        #endregion

        #region Private Class Helper Methods

        private void ScheduleTask(AsyncTask asyncTask)
        {
            lock (taskQueue)
            {
                taskQueue.Add(asyncTask);
                asyncTask.TaskScheduled(); // Update internal time-stamp.
                taskQueueModified = true; // Mark task queue as being dirty.

                // Signal task availability so scheduler picks it up.
                waitHandles[(int)EventIndex.TaskAvailable].Set();
            }
        }

        private void ProcessTaskInternal(AsyncTask asyncTask)
        {
            try
            {
                asyncTask.Execute(); // Internally sets the ExecutionStartTime
            }
            catch (Exception exception)
            {
                // HandleTaskCompletion internally sets the ExecutionEndTime time,
                // it also invokes the registered callback Action if there is one.
                // 
                asyncTask.HandleTaskCompletion(exception);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Core
{
    abstract class AsyncTask
    {
        #region Private Class Data Members

        private Exception exception = null;
        private DynamoScheduler scheduler = null;

        #endregion

        #region Public Class Operational Methods

        internal AsyncTask(DynamoScheduler scheduler)
        {
            this.scheduler = scheduler;
            this.ScheduledTime = scheduler.NextTimeStamp;
        }

        internal void TaskScheduled()
        {
            if (this.ScheduledTime != 0)
            {
                var message = "Task cannot be scheduled twice";
                throw new InvalidOperationException(message);
            }

            this.ScheduledTime = scheduler.NextTimeStamp;
        }

        internal void Execute()
        {
            this.ExecutionStartTime = scheduler.NextTimeStamp;
            this.ExecuteCore();
        }

        internal void HandleTaskCompletion(Exception exception)
        {
            this.ExecutionEndTime = scheduler.NextTimeStamp;
            this.exception = exception;
            this.HandleTaskCompletionCore();
        }

        #endregion

        #region Public Class Properties

        internal long CreationTime { get; private set; }
        internal long ScheduledTime { get; private set; }
        internal long ExecutionStartTime { get; private set; }
        internal long ExecutionEndTime { get; private set; }
        internal Exception Exception { get { return this.exception; } }

        #endregion

        #region Protected/Private Class Helper Methods

        protected abstract void ExecuteCore();
        protected abstract void HandleTaskCompletionCore();

        #endregion
    }
}

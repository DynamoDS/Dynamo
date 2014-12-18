using System;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// DelegateBasedAsyncTask allows for a delegate or System.Action object 
    /// to be scheduled for asynchronous execution on the ISchedulerThread. 
    /// </summary>
    /// 
    internal class DelegateBasedAsyncTask : AsyncTask
    {
        private Action actionToPerform;

        internal override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        #region Public Class Operational Methods

        internal DelegateBasedAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
        }

        internal void Initialize(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            actionToPerform = action;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            actionToPerform();
        }

        protected override void HandleTaskCompletionCore()
        {
            // Does nothing here after invocation of the Action.
        }

        #endregion
    }
}

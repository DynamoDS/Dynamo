using System;

namespace Dynamo.Scheduler
{
    /// <summary>
    /// DelegateBasedAsyncTask allows for a delegate or System.Action object 
    /// to be scheduled for asynchronous execution on the ISchedulerThread. 
    /// </summary>
    /// 
    public class DelegateBasedAsyncTask : AsyncTask
    {
        private Action actionToPerform;

        public override TaskPriority Priority
        {
            get { return TaskPriority.BelowNormal; }
        }

        #region Public Class Operational Methods

        /// <summary>
        /// construct a new empty DelegateBasedAsyncTask
        /// </summary>
        /// <param name="scheduler"> the scheduler to run the task on</param>
        public DelegateBasedAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
        }
        /// <summary>
        /// construct a new DelegateBasedAsyncTask by supplying an action delegate that will run
        /// on the scheduler specified
        /// </summary>
        /// <param name="scheduler"> the scheduler to run the task on</param>
        /// <param name="action"> the action to perform when this task is executed</param>
        public DelegateBasedAsyncTask(IScheduler scheduler, Action action)
            : base(scheduler)
        {
            this.Initialize(action);
        }

        private void Initialize(Action action)
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

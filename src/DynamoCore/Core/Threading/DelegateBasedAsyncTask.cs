using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Core.Threading
{
    internal class DelegateBasedAsyncTask : AsyncTask
    {
        private Action actionToPerform;

        #region Public Class Operational Methods

        internal DelegateBasedAsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
            : base(scheduler, callback)
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

        protected override void ExecuteCore()
        {
            actionToPerform();
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        #endregion
    }
}

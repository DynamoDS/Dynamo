#if ENABLE_DYNAMO_SCHEDULER

namespace Dynamo.Core.Threading
{
    class QueryMirrorDataAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        internal override AsyncTask.TaskPriority Priority
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

        #region Public Class Operational Methods

        internal QueryMirrorDataAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
        {
            throw new System.NotImplementedException();
        }

        protected override void HandleTaskCompletionCore()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

#endif

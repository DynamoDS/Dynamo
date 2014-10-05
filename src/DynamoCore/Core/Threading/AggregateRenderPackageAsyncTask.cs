#if ENABLE_DYNAMO_SCHEDULER

namespace Dynamo.Core.Threading
{
    class AggregateRenderPackageAsyncTask : AsyncTask
    {
        #region Public Class Operational Methods

        internal AggregateRenderPackageAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
        {
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        #endregion
    }
}

#endif

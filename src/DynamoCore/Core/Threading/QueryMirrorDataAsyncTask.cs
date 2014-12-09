using System;

using Dynamo.DSEngine;

using ProtoCore.Mirror;

#if ENABLE_DYNAMO_SCHEDULER

namespace Dynamo.Core.Threading
{
    struct QueryMirrorDataParams
    {
        internal DynamoScheduler DynamoScheduler { get; set; }
        internal EngineController EngineController { get; set; }
        internal string VariableName { get; set; }
    }

    class QueryMirrorDataAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        private string variableName;
        private MirrorData cachedMirrorData;
        private EngineController engineController;

        internal override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        internal MirrorData MirrorData
        {
            get { return cachedMirrorData; }
        }

        #endregion

        #region Public Class Operational Methods

        internal QueryMirrorDataAsyncTask(QueryMirrorDataParams initParams)
            : base(initParams.DynamoScheduler)
        {
            if (initParams.EngineController == null)
                throw new ArgumentNullException("initParams.EngineController");
            if (string.IsNullOrEmpty(initParams.VariableName))
                throw new ArgumentNullException("initParams.VariableName");

            variableName = initParams.VariableName;
            engineController = initParams.EngineController;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            var runtimeMirror = engineController.GetMirror(variableName);
            if (runtimeMirror != null)
                cachedMirrorData = runtimeMirror.GetData();
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        #endregion
    }
}

#endif

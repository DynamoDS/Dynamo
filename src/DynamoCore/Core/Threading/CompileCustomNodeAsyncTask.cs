
using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Core.Threading
{
#if ENABLE_DYNAMO_SCHEDULER

    class CompileCustomNodeParams
    {
        internal EngineController EngineController { get; set; }
        internal CustomNodeDefinition Definition { get; set; }
        internal IEnumerable<string> Parameters { get; set; }
        internal IEnumerable<AssociativeNode> Outputs { get; set; }
    }

    class CompileCustomNodeAsyncTask : AsyncTask
    {
        #region Public Class Operational Methods

        internal CompileCustomNodeAsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
            : base(scheduler, callback)
        {
        }

        internal bool Initialize(CompileCustomNodeParams initParams)
        {
            return false;
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

#endif
}

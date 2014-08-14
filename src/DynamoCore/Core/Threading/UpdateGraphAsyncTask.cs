using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Core.Threading
{
    class UpdateGraphAsyncTask : AsyncTask
    {
        internal UpdateGraphAsyncTask(DynamoScheduler scheduler, Action<AsyncTask> callback)
            : base(scheduler, callback)
        {
        }

        internal void Initialize(EngineController controller, IEnumerable<NodeModel> updatedNodes)
        {
        }

        protected override void ExecuteCore()
        {
        }

        protected override void HandleTaskCompletionCore()
        {
        }
    }
}

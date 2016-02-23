using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Scheduler
{
    class RemoveRenderPackageAsyncTask: AsyncTask
    {
        public RemoveRenderPackageAsyncTask(IScheduler scheduler):base(scheduler)
        {
        }

        public override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        protected override void HandleTaskExecutionCore()
        {
        }
    }
}

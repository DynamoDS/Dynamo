#region
using System.Diagnostics;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using RevitServices.Threading;
using RevitServices.Transactions;

#endregion

namespace Dynamo.Applications
{
    internal class RevitDynamoRunner : DynamoRunner
    {
#if ENABLE_DYNAMO_SCHEDULER

        protected override void Evaluate(HomeWorkspaceModel workspace)
        {
            // SCHEDULER: RevitDynamoRunner is to be retired.
            throw new System.NotImplementedException();
        }

#else

        protected override void Evaluate(HomeWorkspaceModel workspace)
        {
            //Run in idle thread no matter what
            IdlePromise.ExecuteOnIdleSync(() => base.Evaluate(workspace));
        }

#endif
    }
}

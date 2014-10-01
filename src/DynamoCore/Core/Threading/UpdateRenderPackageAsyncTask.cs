using System.Collections;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

#if ENABLE_DYNAMO_SCHEDULER

namespace Dynamo.Core.Threading
{
    class UpdateRenderPackageAsyncTask : AsyncTask
    {
        internal UpdateRenderPackageAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        #region Public Class Operational Methods

        internal bool Initialize(EngineController controller, IEnumerable<NodeModel> nodes)
        {
            // NodeModel.IsSelected
            // NodeModel.DisplayLabels
            // Clear render package
            // if (State == ElementState.Error || ...) return;
            // Get AstIdentifierForPreview.Name
            // GetDrawableIds()
            return true;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
        {
            // EngineController.GetMirror
            // mirror.GetData
            // AddToLabelMap
            // EngineController.GetGraphicItems
            // graphicItem.Tessellate
        }

        protected override void HandleTaskCompletionCore()
        {
            // Dispatcher.BeginInvoke((x) => {
            //      NodeModel.SetRenderPackage(rp);
            // }
        }

        #endregion
    }
}

#endif

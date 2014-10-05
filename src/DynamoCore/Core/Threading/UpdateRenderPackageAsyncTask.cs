#if ENABLE_DYNAMO_SCHEDULER

using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

namespace Dynamo.Core.Threading
{
    class UpdateRenderPackageParams
    {
        internal int MaxTesselationDivisions { get; set; }
        internal string PreviewIdentifierName { get; set; }
        internal NodeModel Node { get; set; }
        internal EngineController EngineController { get; set; }
        internal IEnumerable<string> DrawableIds { get; set; }
    }

    class UpdateRenderPackageAsyncTask : AsyncTask
    {
        internal UpdateRenderPackageAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
        }

        #region Public Class Operational Methods

        internal bool Initialize(UpdateRenderPackageParams initParams)
        {
            var nodeModel = initParams.Node;
            if (!nodeModel.IsUpdated && (!nodeModel.RequiresRecalc))
                return false; // Not has not been updated at all.

            // visualizationManager.MaxTesselationDivisions
            // NodeModel.IsSelected
            // NodeModel.DisplayLabels
            // Clear render package
            // Get AstIdentifierForPreview.Name
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

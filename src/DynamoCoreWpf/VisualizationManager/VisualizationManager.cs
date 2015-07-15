using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    /*
    * The VisualizationManager is reponsible for handling events from the DynamoModel
    * that result in the updating of RenderPackages on NodeModels. After these updates,
    * the VisualizationManager raises events which can be handled by the view, to
    * update rendered geometry. 
    * 
    * The sequence diagram is as follows:
    * 
    *   DynamoModel               Visualization Manager                       Watch3D
    *        |                             |                                      |
    *   EvaluationCompleted--------------->x                                      |
    *        |                     RequestAllNodesVisualsUpdate                   |
    *        |                     RequestNodeVisualUpdateAsync                   |
    *        |                     NotifyRenderPackagesReadyAsyncTask             |
    *        |                     OnNodeModelRenderPackagesReady                 |
    *        |                     RenderComplete-------------------------------->x
    *        |                             |                               RenderCompleteHandler                 
    *        |                             |                               RequestBranchUpdate
    *        |                     AggregateRenderPackageAsyncTask<---------------x 
    *        |                     ResultsReadyToVisualize----------------------->x
    *        |                             |                               ResultsReadyToVisualizeHandler
    *        |                             |                               Render
    */ 
    public class VisualizationManager : NotificationObject, IVisualizationManager
    {
        #region private members

        private string alternateContextName = "Host";
        private bool drawToAlternateContext = true;
        protected readonly DynamoModel dynamoModel;
        private readonly List<IRenderPackage> currentTaggedPackages = new List<IRenderPackage>();
        private bool alternateDrawingContextAvailable;
 
        #endregion

        #region public properties

        /// <summary>
        /// Is another context available for drawing?
        /// This property can be queried indirectly by the view to enable or disable
        /// UI functionality based on whether an alternate drawing context is available.
        /// </summary>
        public bool AlternateDrawingContextAvailable
        {
            get { return alternateDrawingContextAvailable; }
            set
            {
                alternateDrawingContextAvailable = value;
                RaisePropertyChanged("AlternateDrawingContextAvailable");
            }
        }

        /// <summary>
        /// Should we draw to the alternate context if it is available?
        /// </summary>
        public bool DrawToAlternateContext
        {
            get { return drawToAlternateContext; }
            set
            {
                if (value == false)
                {
                    //if the present value has us drawing to the alternate
                    //context and we would like to stop doing so, we need 
                    //to trigger an event requesting alternate contexts
                    //to drop their visualizations
                    if (drawToAlternateContext)
                    {
                        drawToAlternateContext = value;
                        OnRequestAlternateContextClear();
                    }
                }
                else
                {
                    //we would like to reenable drawing to an alternate context.
                    //trigger the standard visualization complete event
                    if (!drawToAlternateContext)
                    {
                        drawToAlternateContext = value;
                        OnRenderComplete();
                    }
                }
                RaisePropertyChanged("DrawToAlternateContext");
            }
        }

        /// <summary>
        /// The name of the alternate context for use in the UI.
        /// </summary>
        public string AlternateContextName
        {
            get { return alternateContextName; }
            set { alternateContextName = value; }
        }

        #endregion

        #region events

        /// <summary>
        /// An event triggered when there are results to visualize.
        /// </summary>
        public event Action<VisualizationEventArgs> ResultsReadyToVisualize;
        protected virtual void OnResultsReadyToVisualize(VisualizationEventArgs args)
        {
            if (ResultsReadyToVisualize != null)
                ResultsReadyToVisualize(args);
        }

        /// <summary>
        /// An event triggered on the completion of visualization update.
        /// </summary>
        public event Action RenderComplete;
        protected virtual void OnRenderComplete()
        {
            if (RenderComplete != null)
                RenderComplete();
        }

        /// <summary>
        /// An event triggered when want any alternate drawing contexts to be cleared.
        /// </summary>
        public event Action RequestAlternateContextClear;
        protected virtual void OnRequestAlternateContextClear()
        {
            if (RequestAlternateContextClear != null)
                RequestAlternateContextClear();
        }

        #endregion

        #region constructors

        public VisualizationManager(DynamoModel model)
        {
            dynamoModel = model;
            dynamoModel.DeletionComplete += dynamoModel_DeletionComplete; 
        }

        #endregion

        #region public methods

        /// <summary>
        /// Display a label for one or several render packages 
        /// based on the paths of those render packages.
        /// </summary>
        public void TagRenderPackageForPath(string path)
        {
            var packages = new List<IRenderPackage>();

            //This also isn't thread safe
            foreach (var node in dynamoModel.CurrentWorkspace.Nodes)
            {
                lock (node.RenderPackagesMutex)
                {
                    //Note(Luke): this seems really inefficent, it's doing an O(n) search for a tag
                    //This is also a target for memory optimisation

                    packages.AddRange(
                        node.RenderPackages.Where(x => x.Description == path || x.Description.Contains(path + ":")));
                }
            }

            if (packages.Any())
            {
                //clear any labels that might have been drawn on this
                //package already and add the one we want
                if (currentTaggedPackages.Any())
                {
                    currentTaggedPackages.ForEach(x => x.DisplayLabels = false);
                    currentTaggedPackages.Clear();
                }

                packages.ToList().ForEach(x => x.DisplayLabels = true);
                currentTaggedPackages.AddRange(packages);

                var allPackages = new List<IRenderPackage>();
                var selPackages = new List<IRenderPackage>();

                foreach (var node in dynamoModel.CurrentWorkspace.Nodes)
                {
                    lock (node.RenderPackagesMutex)
                    {
                        allPackages.AddRange(
                            node.RenderPackages.Where(x => x.HasRenderingData && !x.IsSelected));
                        selPackages.AddRange(
                            node.RenderPackages.Where(x => x.HasRenderingData && x.IsSelected));
                    }
                }

                OnResultsReadyToVisualize(
                    new VisualizationEventArgs(allPackages, selPackages, Guid.Empty));
            }
        }

        /// <summary>
        /// Unhook all event handlers.
        /// </summary>
        public void Dispose()
        {
            dynamoModel.DeletionComplete -= dynamoModel_DeletionComplete;
        }

        #endregion

        #region private methods

        private void dynamoModel_DeletionComplete(object sender, EventArgs e)
        {
            var hws = dynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            if (hws != null)
            {
                if (hws.RunSettings.RunType == RunType.Manual ||
                    hws.RunSettings.RunType == RunType.Periodic)
                {
                    // We need to force a visualization update.
                    OnRenderComplete();
                }
            }
        }

        private void OnNodeModelRenderPackagesReady(AsyncTask asyncTask)
        {
            // By design the following method is invoked on the context of 
            // ISchedulerThread, if access to any UI element is desired within
            // the method, dispatch those actions on UI dispatcher *inside* the
            // method, *do not* dispatch the following call here as derived 
            // handler may need it to remain on the ISchedulerThread's context.
            // 

            // Fire event to tell render targets to request their visuals
            OnRenderComplete();

            // Call overridden method on visualization manager to
            // process whatever internal logic there is around
            // drawing a visualization.
            HandleRenderPackagesReadyCore();
        }

        #endregion

        #region protected methods

        protected virtual void HandleRenderPackagesReadyCore()
        {
            // Default visualization manager does nothing here.
        }

        #endregion

    }

    /// <summary>
    /// The VisualizationEventArgs class is used to convey RenderPackage
    /// and identity data to handlers of the ResultsReadyToVisualize event
    /// on the VisualizationManager.
    /// </summary>
    public sealed class VisualizationEventArgs : EventArgs
    {
        /// <summary>
        /// A list of render packages corresponding to a branch or a whole graph.
        /// </summary>
        public IEnumerable<IRenderPackage> Packages { get; private set; }
        public IEnumerable<IRenderPackage> SelectedPackages { get; private set; }

        /// <summary>
        /// The id of the view for which the description belongs.
        /// </summary>
        public Guid Id { get; protected set; }

        public VisualizationEventArgs(IEnumerable<IRenderPackage> packages, IEnumerable<IRenderPackage> selectedPackages, Guid viewId)
        {
            Packages = packages;
            SelectedPackages = selectedPackages;
            Id = viewId; 
        }
    }

}

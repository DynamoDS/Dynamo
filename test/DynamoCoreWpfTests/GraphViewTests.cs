using System.Collections.Generic;
using System.Threading;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Visualization;
using Dynamo.Wpf.Rendering;
using DynamoCoreWpfTests.Utility;

namespace DynamoCoreWpfTests
{

    /// <summary>
    /// This class should not be used for DynamoView unit tests
    /// </summary>
    public class GrapViewTests : DynamoTestUIBase
    {
        /// <summary>
        /// Signal used for performance time measurement
        /// </summary>
        private static ManualResetEvent signalEvent = new ManualResetEvent(false);

        /// <summary>
        /// Function used to measure file open which saved in manual mode
        /// </summary>
        /// <param name="path"></param>
        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Call this to disable the event handler to get render package
        /// </summary>
        public void DisableRendering()
        {
            // Dispose the background watch 3D ViewModel so no tessellation or rendering happen
            foreach (var node in Model.CurrentWorkspace.Nodes)
            {
                ViewModel.BackgroundPreviewViewModel.UnregisterNodeEventHandlers(node);
            }
        }

        /// <summary>
        /// Event handler for EvaluationCompleted event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">EvaluationCompletedEventArgs</param>
        void OnGraphEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            if (e.EvaluationSucceeded)
            {
                // Signal the signalEvent ready
                signalEvent.Set();
            }
        }

        /// <summary>
        /// Function used to measure graph run with view
        /// </summary>
        public override void Run()
        {
            // A more precise way to measure the time spent is from when
            // run() called to when even EvaluationCompleted is trigger on workspace
            (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCompleted += OnGraphEvaluationCompleted;

            base.Run();
            signalEvent.WaitOne();
            signalEvent.Reset();
            // Unsubscribe
            (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCompleted -= OnGraphEvaluationCompleted;
        }

        /// <summary>
        /// Function used to measure graph geometry tessellation
        /// </summary>
        public void Tessellation()
        {
            var renderPackageFactory = new HelixRenderPackageFactory();

            // A list of GeometryHolder for all the nodes belong to the graph
            var nodeGeometries = new List<GeometryHolder>();

            foreach (var node in Model.CurrentWorkspace.Nodes)
            {
                nodeGeometries.Add(new GeometryHolder(Model, renderPackageFactory, node));
            }

            foreach (var holder in nodeGeometries)
            {
                if (holder.HasGeometry)
                    continue;
            }
        }
    }
}
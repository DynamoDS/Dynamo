using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Core.Threading;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf.Views.Preview
{
    public class Watch3DNodeView : Watch3DView
    {
        private NodeModel node;

        public Watch3DNodeView(NodeModel node)
        {
            this.node = node;
            resizeThumb.Visibility = Visibility.Visible;
            foreach (var p in node.InPorts)
            {
                p.PortDisconnected += p_PortDisconnected;
            }
        }

        void p_PortDisconnected(PortModel obj)
        {
            ResetGeometryDictionary();
        }

        protected override void EvaluationCompletedHandler(object sender, EvaluationCompletedEventArgs e)
        {
            // The background preview, which will be initialized before this view,
            // will take care of updating render packages for all nodes that were
            // evaluated. For this handler, we only need to do the aggregation.

            var model = viewModel.Model;

            var scheduler = model.Scheduler;
            if (scheduler == null) // Shutdown has begun.
                return;

            // Schedule a AggregateRenderPackageAsyncTask here so that the 
            // background geometry preview gets refreshed.
            // 
            var task = new AggregateRenderPackageAsyncTask(scheduler);
            task.Initialize(model.CurrentWorkspace, node);
            task.Completed += RenderPackageAggregationCompletedHandler;
            scheduler.ScheduleForExecution(task);
        }

        protected override void NodePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var updatedNode = sender as NodeModel;
            switch (e.PropertyName)
            {
                case "IsUpstreamVisible":
                    var gathered = new List<NodeModel>();
                    WorkspaceUtilities.GatherAllUpstreamNodes(updatedNode,
                    gathered, model => model.IsUpstreamVisible);
                    UpdatedNodeRenderPackagesAndAggregateAsync(gathered);
                    break;
            }

            base.NodePropertyChangedHandler(sender, e);
        }
    }
}

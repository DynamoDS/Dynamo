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
                p.PortDisconnected += PortDisconnectedHandler;
                p.PortConnected += PortConnectedHandler;
            }
        }

        void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Mark upstream nodes as updated.
            // Trigger an aggregation.
            var gathered = new List<NodeModel>();
            node.UpstreamNodes(gathered, n => n.IsUpstreamVisible);
            if (gathered.Any())
            {
                gathered.ForEach(n => n.IsUpdated = true);
            }

            UpdatedNodeRenderPackagesAndAggregateAsync(gathered);
        }

        void PortDisconnectedHandler(PortModel obj)
        {
            ResetGeometryDictionary();
        }

        protected override void EvaluationCompletedHandler(object sender, EvaluationCompletedEventArgs e)
        {
            // The background preview, which will be initialized before this view,
            // will take care of updating render packages for all nodes that were
            // evaluated. For this handler, we only need to do the aggregation.
            ScheduleAggregationForNode();
        }

        private void ScheduleAggregationForNode()
        {
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

            // Don't bother with node property changes 
            // that are not in this branch.

            if (!updatedNode.IsUpstreamOf(node))
                return;

            switch (e.PropertyName)
            {
                case "IsUpstreamVisible":

                    var upstream = new List<NodeModel>();
                    updatedNode.UpstreamNodes(upstream);

                    foreach (var n in upstream)
                    {
                        var geoms = FindAllGeometryModel3DsForNode(n).ToList();
                        if (updatedNode.IsUpstreamVisible)
                        {
                            // Only unhide the geom if its preview value is set to true
                            geoms.ForEach(g=>g.Value.Visibility = n.IsVisible? Visibility.Visible : Visibility.Hidden);
                        }
                        else
                        {
                            geoms.ForEach(g => g.Value.Visibility = Visibility.Hidden);
                        }
                    }

                    View.InvalidateRender();

                    break;
            }   

            base.NodePropertyChangedHandler(sender, e);
        }
    }
}

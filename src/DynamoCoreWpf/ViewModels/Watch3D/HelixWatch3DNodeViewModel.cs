using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class HelixWatch3DNodeViewModel : HelixWatch3DViewModel
    {
        private NodeModel node;

        public static HelixWatch3DNodeViewModel Start(NodeModel node, Watch3DViewModelStartupParams parameters)
        {
            var vm = new HelixWatch3DNodeViewModel(node, parameters);
            vm.OnStartup();
            return vm;
        }

        private HelixWatch3DNodeViewModel(NodeModel node, Watch3DViewModelStartupParams parameters):
            base(parameters)
        {
            this.node = node;
            IsResizable = true;

            RegisterPortEventHandlers(node);
        }

        protected override void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Mark upstream nodes as updated.
            // Trigger an aggregation.
            var gathered = new List<NodeModel>();
            node.UpstreamNodesMatchingPredicate(gathered, n => n.IsUpstreamVisible);
            if (gathered.Any())
            {
                gathered.ForEach(n => n.IsUpdated = true);
            }

            gathered.ForEach(n=>n.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory));
        }

        protected override void PortDisconnectedHandler(PortModel obj)
        {
            OnClear();
        }

        protected override void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    updatedNode.AllUpstreamNodes(upstream);

                    foreach (var n in upstream)
                    {
                        var geoms = FindAllGeometryModel3DsForNode(n.AstIdentifierBase).ToList();
                        if (updatedNode.IsUpstreamVisible)
                        {
                            // Only unhide the geom if its preview value is set to true
                            geoms.ForEach(g => g.Value.Visibility = n.IsVisible ? Visibility.Visible : Visibility.Hidden);
                        }
                        else
                        {
                            geoms.ForEach(g => g.Value.Visibility = Visibility.Hidden);
                        }
                    }

                    OnRequestViewRefresh();

                    break;
            }

            base.OnNodePropertyChanged(sender, e);
        }

        protected override void OnUpdatedRenderPackagesAvailable(NodeModel updatedNode,
            IEnumerable<IRenderPackage> renderPackages)
        {
            var upstream = new List<NodeModel>();
            node.AllUpstreamNodes(upstream);

            if (node == null || !upstream.Contains(updatedNode))
            {
                return;
            }

            base.OnUpdatedRenderPackagesAvailable(updatedNode, renderPackages);
        }
    }
}

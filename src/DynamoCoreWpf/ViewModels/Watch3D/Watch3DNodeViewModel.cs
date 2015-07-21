using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class Watch3DNodeViewModel : Watch3DViewModel
    {
        private NodeModel node;

        public Watch3DNodeViewModel(NodeModel node, DynamoModel model, IRenderPackageFactory factory, DynamoViewModel viewModel):
            base(model, factory, viewModel)
        {
            this.node = node;

            IsResizable = true;
            Name = string.Format("{0}_preview", node.GUID);

            RegisterPortEventHandlers(node);
        }

        protected override void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Mark upstream nodes as updated.
            // Trigger an aggregation.
            var gathered = new List<NodeModel>();
            node.UpstreamNodes(gathered, n => n.IsUpstreamVisible);
            if (gathered.Any())
            {
                gathered.ForEach(n => n.IsUpdated = true);
            }

            gathered.ForEach(n=>n.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory));
        }

        protected override void PortDisconnectedHandler(PortModel obj)
        {
            ResetGeometryDictionary();
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
                    updatedNode.UpstreamNodes(upstream);

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
            node.UpstreamNodes(upstream);

            if (node == null || !upstream.Contains(updatedNode))
            {
                return;
            }

            base.OnUpdatedRenderPackagesAvailable(updatedNode, renderPackages);
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class HelixWatch3DNodeViewModel : HelixWatch3DViewModel
    {
        private readonly NodeModel node;

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
            UpdateUpstream();
        }

        private void UpdateUpstream()
        {
            OnClear();

            var gathered = new List<NodeModel>();
            node.VisibleUpstreamNodes(gathered);

            gathered.ForEach(n => n.IsUpdated = true);
            gathered.ForEach(n => n.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory));
        }

        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    UpdateUpstream();
                    break;
            }
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
                    UpdateUpstream();
                    break;
            }

            base.OnNodePropertyChanged(sender, e);
        }

        protected override void OnUpdatedRenderPackagesAvailable(NodeModel updatedNode,
            IEnumerable<IRenderPackage> renderPackages)
        {
            if (node == null) return;

            var visibleUpstream = new List<NodeModel>();
            node.VisibleUpstreamNodes(visibleUpstream);

            if (!visibleUpstream.Contains(updatedNode))
            {
                return;
            }

            base.OnUpdatedRenderPackagesAvailable(updatedNode, renderPackages);
        }
    }
}

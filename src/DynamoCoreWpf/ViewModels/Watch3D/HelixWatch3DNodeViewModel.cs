using System;
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
        private readonly NodeModel watchNode;

        public static HelixWatch3DNodeViewModel Start(NodeModel node, Watch3DViewModelStartupParams parameters)
        {
            var vm = new HelixWatch3DNodeViewModel(node, parameters);
            vm.OnStartup();
            return vm;
        }

        private HelixWatch3DNodeViewModel(NodeModel node, Watch3DViewModelStartupParams parameters):
            base(parameters)
        {
            watchNode = node;
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
            watchNode.VisibleUpstreamNodes(gathered);

            gathered.ForEach(n => n.IsUpdated = true);
            gathered.ForEach(n => n.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory));
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

            if (!updatedNode.IsUpstreamOf(watchNode))
                return;

            switch (e.PropertyName)
            {
                case "IsUpstreamVisible":
                    UpdateUpstream();
                    break;
            }

            base.OnNodePropertyChanged(sender, e);
        }

        protected override void OnRenderPackagesUpdated(NodeModel node,
            IEnumerable<IRenderPackage> renderPackages)
        {
            var updatedNode = model.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == node.GUID);
            if (updatedNode == null) return;

            var visibleUpstream = new List<NodeModel>();
            watchNode.VisibleUpstreamNodes(visibleUpstream);

            if (!visibleUpstream.Contains(updatedNode))
            {
                return;
            }

            base.OnRenderPackagesUpdated(node, renderPackages);
        }
    }
}

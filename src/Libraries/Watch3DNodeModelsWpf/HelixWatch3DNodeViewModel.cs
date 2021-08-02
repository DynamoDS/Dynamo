using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.ViewModels.Watch3D;
using Watch3DNodeModels;
using Dynamo.Selection;
using Dynamo.Visualization;

namespace Watch3DNodeModelsWpf
{
    public class HelixWatch3DNodeViewModel : HelixWatch3DViewModel
    {
        public override bool IsBackgroundPreview
        {
            get { return false; }
        }

        public HelixWatch3DNodeViewModel(Watch3D node, Watch3DViewModelStartupParams parameters)
        : base(node, parameters)
        {
            IsResizable = true;

            RegisterPortEventHandlers(node);

            node.Serialized += SerializeCamera;
            node.Deserialized += watchNode_Deserialized;

            Name = string.Format("{0} Preview", node.GUID);
        }

        protected internal override void OnWatchExecution()
        {
            var watch3D = watchModel as Watch3D;
            if (watch3D != null)
            {
                watch3D.WasExecuted = true;
            }
        }

        void watchNode_Deserialized(XmlNode obj)
        {
            var cameraNode = obj.ChildNodes.Cast<XmlNode>().FirstOrDefault(innerNode => innerNode.Name.Equals("camera", StringComparison.OrdinalIgnoreCase));
            var cameraData = DeserializeCamera(cameraNode);
            SetCameraData(cameraData);
        }

        protected override void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            UpdateUpstream();
        }

        protected internal override void UpdateUpstream()
        {
            OnClear();

            var gathered = watchModel.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner)).ToList();

            gathered.ForEach(n => n.WasRenderPackageUpdatedAfterExecution = false);
            gathered.ForEach(n => n.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory, false, true));
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

        protected override void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as NodeModel;
            if (node == null)
            {
                return;
            }

            if(e.PropertyName == "CachedValue")
            {
                var connected = watchModel.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner));
                if (!connected.Contains(node))
                {
                    return;
                }

                node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController, renderPackageFactory, true, true);
            }

            if (e.PropertyName == "IsFrozen")
            {
                var visibleUpstream = new List<NodeModel>();
                watchModel.VisibleUpstreamNodes(visibleUpstream);

                if (!visibleUpstream.Contains(node))
                { 
                    return;
                }

                HashSet<NodeModel> gathered = new HashSet<NodeModel>();
                node.GetDownstreamNodes(node, gathered);
                SetGeometryFrozen(gathered);
            }

            node.WasRenderPackageUpdatedAfterExecution = false;
        }

        protected override void PortDisconnectedHandler(PortModel obj)
        {
            OnClear();
        }

        
        protected override void OnRenderPackagesUpdated(NodeModel node,
            RenderPackageCache renderPackages)
        {
            var updatedNode = dynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == node.GUID);
            if (updatedNode == null) return;

            var visibleUpstream = watchModel.InPorts.SelectMany(p => p.Connectors.Select(c => c.Start.Owner)).ToList();

            if (!visibleUpstream.Contains(updatedNode))
            {
                return;
            }

            base.OnRenderPackagesUpdated(node, renderPackages);
        }

        protected override void OnWorkspaceSaving(XmlDocument doc)
        {
            // In the node version of this view model, we don't save when 
            // the workspace is saving. See Watch3D.SeralizeCore where we call
            // the view model's SerializeCamera method, and Watch3D.DeserializeCore 
            // where we call the view model's DeserializeCamera method.
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);
            UnregisterNodeEventHandlers(this.watchModel);
            UnregisterEventHandlers();
            //since we are removing this node - we must detach all events
            //from all workspaces.
            this.dynamoModel.Workspaces.ToList().ForEach(ws => OnWorkspaceRemoved(ws));
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionChangedHandler;
            (this.watchModel as Watch3D).Serialized -= SerializeCamera;
            (this.watchModel as Watch3D).Deserialized -= watchNode_Deserialized;
            OnClear();
        }
    }
}

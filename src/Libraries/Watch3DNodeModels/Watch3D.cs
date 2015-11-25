using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.Visualization;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;
using Watch3DNodeModels.Properties;

namespace Watch3DNodeModels
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Watch3DDescription", typeof(Resources))]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview", "Dynamo.Nodes.3DPreview", "Dynamo.Nodes.Watch3D", "DynamoWatch3D.Watch3D")]
    [IsDesignScriptCompatible]
    public class Watch3D : NodeModel
    {
        // If the view model, which maintains the camera, 
        // is not created until the view customization is applied,
        // as in the case of a Watch3D node,
        // we cache the camera position data returned from the file
        // to be applied when the view model is constructed.
        internal XmlNode initialCameraData;

        internal event Action<XmlNode> Deserialized;
        internal void OnDeserialized(XmlNode node)
        {
            if (Deserialized != null)
            {
                Deserialized(node);
            }
        }

        internal event Action<XmlElement> Serialized;
        internal void OnSerialized(XmlElement element)
        {
            if (Serialized != null)
            {
                Serialized(element);
            }
        }

        public double WatchWidth { get; private set; }
        public double WatchHeight { get; private set; }

        public delegate void VoidHandler();

        #region constructors

        public Watch3D()
        {
            InPortData.Add(new PortData("", Resources.Watch3DPortDataInputToolTip));
            OutPortData.Add(new PortData("", Resources.Watch3DPortDataInputToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            WatchWidth = 200;
            WatchHeight = 200;

            ShouldDisplayPreviewCore = false;

        }

        #endregion

        #region public methods

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        public void SetWatchSize(double w, double h)
        {
            WatchWidth = w;
            WatchHeight = h;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            new IdentifierListNode
                            {
                                LeftNode = AstFactory.BuildIdentifier("DataBridge"),
                                RightNode = AstFactory.BuildIdentifier("BridgeData")
                            },
                            2,
                            new[] { 0 },
                            new List<AssociativeNode>
                            {
                                AstFactory.BuildStringNode(GUID.ToString()),
                                AstFactory.BuildNullNode()
                            }))
                };
            }

            var resultAst = new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0])
            };

            return resultAst;
        }

        #endregion

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);

            if (nodeElement.OwnerDocument == null) return;

            var viewElement = nodeElement.OwnerDocument.CreateElement("view");
            nodeElement.AppendChild(viewElement);
            var viewHelper = new XmlElementHelper(viewElement);

            viewHelper.SetAttribute("width", WatchWidth);
            viewHelper.SetAttribute("height", WatchHeight);

            OnSerialized(viewElement);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            try
            {
                foreach (var node in nodeElement.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "view"))
                {
                    if (node.Attributes == null || node.Attributes.Count == 0) continue;

                    WatchWidth = Convert.ToDouble(node.Attributes["width"].Value, CultureInfo.InvariantCulture);
                    WatchHeight = Convert.ToDouble(node.Attributes["height"].Value, CultureInfo.InvariantCulture);

                    // Cache the data if we're using a node view customization 
                    // to create the view model.
                    initialCameraData = node;

                    // Trigger the event, in case the view model already exists
                    OnDeserialized(node);
                }
            }
            catch (Exception ex)
            {
                Log(LogMessage.Error(ex));
                Log("View attributes could not be read from the file.");
            }
        }

        public override bool RequestVisualUpdateAsync(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            // No visualization update is required for this node type.
            return false;
        }
    }
}

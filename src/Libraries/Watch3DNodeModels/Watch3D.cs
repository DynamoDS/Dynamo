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
using Newtonsoft.Json;

namespace Watch3DNodeModels
{
    /// <summary>
    /// Class used for representation of Helix camera.
    /// </summary>
    public class Watch3DCamera
    {
        private const string DefaultCameraName = "Background Preview";

        /// <summary>
        /// Name of watch3d camera, e.g. "761132b8-b5fa-46b2-98a9-82436e0d8812 Preview"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// EyeX of Watch 3d camera
        /// </summary>
        public double EyeX { get; set; }

        /// <summary>
        /// EyeY of Watch 3d camera
        /// </summary>
        public double EyeY { get; set; }

        /// <summary>
        /// EyeZ of Watch 3d camera
        /// </summary>
        public double EyeZ { get; set; }

        /// <summary>
        /// LookX of Watch 3d camera
        /// </summary>
        public double LookX { get; set; }

        /// <summary>
        /// LookY of Watch 3d camera
        /// </summary>
        public double LookY { get; set; }

        /// <summary>
        /// LookZ of Watch 3d camera
        /// </summary>
        public double LookZ { get; set; }

        /// <summary>
        /// UpX of Watch 3d camera
        /// </summary>
        public double UpX { get; set; }

        /// <summary>
        /// UpY of Watch 3d camera
        /// </summary>
        public double UpY { get; set; }

        /// <summary>
        /// UpZ of Watch 3d camera
        /// </summary>
        public double UpZ { get; set; }

        public Watch3DCamera()
        {
            Name = DefaultCameraName;
            EyeX = -17;
            EyeY = 24;
            EyeZ = 50;
            LookX = 12;
            LookY = -13;
            LookZ = -58;
            UpX = 0;
            UpY = 1;
            UpZ = 0;
        }
    }

    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Watch3DDescription", typeof(Resources))]
    [OutPortTypes("var")]
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
        public bool WasExecuted { get; internal set; }

        public delegate void VoidHandler();

        #region constructors

        [JsonConstructor]
        private Watch3D(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            WatchWidth = 200;
            WatchHeight = 200;
            ShouldDisplayPreviewCore = false;
            Camera = new Watch3DCamera();
        }

        public Watch3D()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", Resources.Watch3DPortDataInputToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Resources.Watch3DPortDataInputToolTip)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            WatchWidth = 200;
            WatchHeight = 200;

            ShouldDisplayPreviewCore = false;
            Camera = new Watch3DCamera();
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

        #region public properties

        public Watch3DCamera Camera
        {
            get;
            private set;
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

                    // Deserialized can be null, when we are running on Unix. There is no WPF 
                    // and that's why no Helix. But we still have to process camera position to use it in Flood.
                    if (Deserialized == null)
                    {
                        DeserializeCamera();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogMessage.Error(ex));
                Log("View attributes could not be read from the file.");
            }
        }

        /// <summary>
        /// Deserializes camera from XML, if there is no any Deserialized action. 
        /// It's used for creation of camera, that will be sent to Flood.
        /// </summary>
        private void DeserializeCamera()
        {
            var cameraNode = initialCameraData.ChildNodes
                            .Cast<XmlNode>()
                            .FirstOrDefault
                            (innerNode => innerNode.Name.Equals("camera", StringComparison.OrdinalIgnoreCase));

            if (cameraNode == null || cameraNode.Attributes == null || cameraNode.Attributes.Count == 0)
            {
                return;
            }
            var name = cameraNode.Attributes["Name"].Value;
            var ex = float.Parse(cameraNode.Attributes["eyeX"].Value, CultureInfo.InvariantCulture);
            var ey = float.Parse(cameraNode.Attributes["eyeY"].Value, CultureInfo.InvariantCulture);
            var ez = float.Parse(cameraNode.Attributes["eyeZ"].Value, CultureInfo.InvariantCulture);
            var lx = float.Parse(cameraNode.Attributes["lookX"].Value, CultureInfo.InvariantCulture);
            var ly = float.Parse(cameraNode.Attributes["lookY"].Value, CultureInfo.InvariantCulture);
            var lz = float.Parse(cameraNode.Attributes["lookZ"].Value, CultureInfo.InvariantCulture);
            var ux = float.Parse(cameraNode.Attributes["upX"].Value, CultureInfo.InvariantCulture);
            var uy = float.Parse(cameraNode.Attributes["upY"].Value, CultureInfo.InvariantCulture);
            var uz = float.Parse(cameraNode.Attributes["upZ"].Value, CultureInfo.InvariantCulture);

            Camera = new Watch3DCamera
            {
                Name = name,
                EyeX = ex,
                EyeY = ey,
                EyeZ = ez,
                LookX = lx,
                LookY = ly,
                LookZ = lz,
                UpX = ux,
                UpY = uy,
                UpZ = uz
            };
        }

        public override bool RequestVisualUpdateAsync(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            // No visualization update is required for this node type.
            return false;
        }
    }
}

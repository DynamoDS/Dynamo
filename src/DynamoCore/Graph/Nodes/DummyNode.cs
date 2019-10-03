using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// DummyNode is used for tests or in case if node couldn't be loaded.
    /// </summary>
    [NodeName("Legacy Node")]
    [NodeDescription("DummyNodeDescription", typeof(Dynamo.Properties.Resources))]
    [IsMetaNode]
    [IsVisibleInDynamoLibrary(false)]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.DummyNode")]
    public class DummyNode : NodeModel
    {
        public enum Nature
        {
            Deprecated, Unresolved
        }

        [JsonConstructor]
        private DummyNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            throw new InvalidOperationException("Dummy nodes should not be serialized to JSON format.");
        }

        /// <summary>
        /// This function creates DummyNode.
        /// DummyNode is used for tests or in case if node couldn't be loaded.
        /// </summary>
        public DummyNode()
        {
            LegacyNodeName = "Dynamo.Graph.Nodes.DummyNode";
            LegacyFullName = LegacyNodeName;
            LegacyAssembly = string.Empty;
            NodeNature = Nature.Unresolved;
            Description = GetDescription();
            ShouldDisplayPreviewCore = false;
        }

        /// <summary>
        /// This function creates DummyNode with specified number of ports.
        /// </summary>
        /// <param name="inputCount">Number of input ports</param>
        /// <param name="outputCount">Number of output ports</param>
        /// <param name="legacyName">Name of the node</param>
        /// <param name="originalElement">Xml node</param>
        /// <param name="legacyAssembly">Assembly of the node</param>
        /// <param name="nodeNature">Node can be Deprecated or Unresolved</param>
        public DummyNode(
            int inputCount,
            int outputCount,
            string legacyName,
            XmlElement originalElement,
            string legacyAssembly,
            Nature nodeNature)
        {
            InputCount = inputCount;
            OutputCount = outputCount;
            LegacyNodeName = legacyName;
            LegacyFullName = legacyName;
            Name = legacyName;
            OriginalNodeContent = originalElement;
            LegacyAssembly = legacyAssembly;
            NodeNature = nodeNature;

            Description = GetDescription();
            ShouldDisplayPreviewCore = false;

            if (originalElement != null)
            {
                var legacyFullName = originalElement.Attributes["function"];
                if (legacyFullName != null)
                    LegacyFullName = legacyFullName.Value;
            }

            UpdatePorts();

            // Take the position from the old node (because a dummy node
            // should always be created at the location of the old node).
            var helper = new XmlElementHelper(originalElement);
            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);

            //Take the GUID from the old node (dummy nodes should have their
            //GUID's. This will allow the Groups to work as expected. MAGN-7568)
            GUID = helper.ReadGuid("guid", this.GUID);
        }

        /// <summary>
        /// This function creates DummyNode with specified number of ports.
        /// </summary>
        /// <param name="id">Id of the original node</param>
        /// <param name="inputCount">Number of input ports</param>
        /// <param name="outputCount">Number of output ports</param>
        /// <param name="legacyAssembly">Assembly of the node</param>
        /// <param name="originalElement">Original JSON description of the node</param>
        public DummyNode(
            string id,
            int inputCount,
            int outputCount,
            string legacyAssembly,
            JObject originalElement)
        {
            GUID = new Guid(id);

            InputCount = inputCount;
            OutputCount = outputCount;

            string legacyName = "Unresolved";
            LegacyNodeName = legacyName;
            LegacyFullName = legacyName;
            Name = legacyName;

            OriginalNodeContent = originalElement;

            LegacyAssembly = legacyAssembly;
            NodeNature = DummyNode.Nature.Unresolved;

            Description = GetDescription();
            ShouldDisplayPreviewCore = false;

            if (originalElement != null)
            {
                var legacyFullName = originalElement["FunctionSignature"];
                if (legacyFullName != null)
                    LegacyFullName = legacyFullName.ToString();
            }

            UpdatePorts();
        }

        private void LoadNode(XmlNode nodeElement)
        {
            XmlElement originalElement = OriginalXmlNodeContent;

            var inputCount = nodeElement.Attributes["inputCount"];
            var outputCount = nodeElement.Attributes["outputCount"];
            var legacyName = nodeElement.Attributes["legacyNodeName"];

            InputCount = Int32.Parse(inputCount.Value);
            OutputCount = Int32.Parse(outputCount.Value);
            LegacyNodeName = legacyName.Value;

            if (nodeElement.ChildNodes != null)
            {
                foreach (XmlNode childNode in nodeElement.ChildNodes)
                    if (childNode.Name.Equals("OriginalNodeContent"))
                        OriginalNodeContent = (XmlElement)nodeElement.FirstChild.FirstChild;
            }

            if (originalElement != null)
            {
                var legacyFullName = originalElement.Attributes["type"];
                if (legacyFullName != null)
                    LegacyFullName = legacyFullName.Value;
            }
            else if (nodeElement.Attributes["OriginalNodeContent"] != null)
            {
                //we have some json, so lets parse it.
                var jsonObject = JObject.Parse(nodeElement.Attributes["OriginalNodeContent"].Value);
                if (jsonObject != null)
                {
                    this.OriginalNodeContent = jsonObject;
                }
            }



            var legacyAsm = nodeElement.Attributes["legacyAssembly"];
            if (legacyAsm != null)
                LegacyAssembly = legacyAsm.Value;

            var nodeNature = nodeElement.Attributes["nodeNature"];
            if (nodeNature != null)
            {
                var nature = Enum.Parse(typeof(Nature), nodeNature.Value);
                NodeNature = ((Nature)nature);
            }


            UpdatePorts();
        }

        private void UpdatePorts()
        {
            InPorts.Clear();
            for (int input = 0; input < InputCount; input++)
            {
                var name = string.Format("Port {0}", input + 1);
                InPorts.Add(new PortModel(PortType.Input, this, new PortData(name, "")));
            }

            OutPorts.Clear();
            for (int output = 0; output < OutputCount; output++)
            {
                var name = string.Format("Port {0}", output + 1);
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData(name, "")));
            }

            RegisterAllPorts();
        }

        private void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement originalElement = OriginalXmlNodeContent;

            if (context == SaveContext.Copy || context == SaveContext.Undo)
            {
                //Dump all the information into memory

                nodeElement.SetAttribute("inputCount", InputCount.ToString());
                nodeElement.SetAttribute("outputCount", OutputCount.ToString());
                nodeElement.SetAttribute("legacyNodeName", LegacyNodeName);
                nodeElement.SetAttribute("legacyAssembly", LegacyAssembly);
                nodeElement.SetAttribute("nodeNature", NodeNature.ToString());

                if (originalElement != null)
                {
                    XmlElement originalNode = xmlDoc.CreateElement("OriginalNodeContent");
                    XmlElement nodeContent = nodeElement.OwnerDocument.CreateElement(originalElement.Name);

                    foreach (XmlAttribute attribute in originalElement.Attributes)
                        nodeContent.SetAttribute(attribute.Name, attribute.Value);

                    for (int i = 0; i < originalElement.ChildNodes.Count; i++)
                    {
                        XmlNode child =
                            nodeContent.OwnerDocument.ImportNode(originalElement.ChildNodes[i], true);
                        nodeContent.AppendChild(child.CloneNode(true));
                    }

                    originalNode.AppendChild(nodeContent);
                    nodeElement.AppendChild(originalNode);
                }
                //if the node actually came from JSON we need to
                //still preserve that incase this node was copied or undo/redone
                else if (OriginalNodeContent is JObject)
                {
                    nodeElement.SetAttribute("OriginalNodeContent", OriginalNodeContent.ToString());
                }
            }

            if (context == SaveContext.File)
            {
                //When save files, only save the original node's content, 
                //instead of saving the dummy node.
                if (originalElement != null)
                {
                    nodeElement.RemoveAll();
                    foreach (XmlAttribute attribute in originalElement.Attributes)
                        nodeElement.SetAttribute(attribute.Name, attribute.Value);

                    //overwrite the guid/x/y value of the original node.
                    nodeElement.SetAttribute("guid", nodeElement.GetAttribute("guid"));
                    nodeElement.SetAttribute("x", nodeElement.GetAttribute("x"));
                    nodeElement.SetAttribute("y", nodeElement.GetAttribute("y"));

                    for (int i = 0; i < originalElement.ChildNodes.Count; i++)
                    {
                        XmlNode child = nodeElement.OwnerDocument.ImportNode(originalElement.ChildNodes[i], true);
                        nodeElement.AppendChild(child.CloneNode(true));
                    }
                }
                else
                {
                    nodeElement.SetAttribute("inputCount", InputCount.ToString());
                    nodeElement.SetAttribute("outputCount", OutputCount.ToString());
                    nodeElement.SetAttribute("legacyNodeName", LegacyNodeName);
                    nodeElement.SetAttribute("legacyAssembly", LegacyAssembly);
                    nodeElement.SetAttribute("nodeNature", NodeNature.ToString());
                }
            }
        }

        #region SerializeCore/DeserializeCore

        protected override XmlElement CreateElement(XmlDocument xmlDocument, SaveContext context)
        {
            XmlElement originalElement = OriginalXmlNodeContent;

            if (context == SaveContext.File && originalElement != null)
            {
                XmlElement originalNode = xmlDocument.CreateElement(originalElement.Name);
                return originalNode;
            }
            else
            {
                return base.CreateElement(xmlDocument, context);
            }
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            SaveNode(element.OwnerDocument, element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            LoadNode(element);
        }

        #endregion

        internal string GetDescription()
        {
            if (NodeNature == Nature.Deprecated)
            {
                if (string.IsNullOrEmpty(LegacyAssembly))
                {
                    const string format = "Node of type '{0}' is now deprecated";
                    return string.Format(format, LegacyNodeName);
                }
                else
                {
                    const string format = "Node of type '{0}' ({1}) is now deprecated";
                    return string.Format(format, LegacyNodeName, LegacyAssembly);
                }
            }

            if (NodeNature == Nature.Unresolved)
            {
                if (string.IsNullOrEmpty(LegacyAssembly))
                {
                    const string format = "Node of type '{0}' cannot be resolved";
                    return string.Format(format, LegacyNodeName);
                }
                else
                {
                    const string format = "Node of type '{0}' ({1}) cannot be resolved";
                    return string.Format(format, LegacyNodeName, LegacyAssembly);
                }
            }

            const string message = "Unhandled 'DummyNode.NodeNature' value: {0}";
            throw new InvalidOperationException(string.Format(message, NodeNature));
        }

        /// <summary>
        /// Returns the number of input ports
        /// </summary>
        public int InputCount { get; private set; }

        /// <summary>
        /// Returns the number of output ports
        /// </summary>
        public int OutputCount { get; private set; }

        /// <summary>
        /// Returns the node name
        /// </summary>
        public string LegacyNodeName { get; private set; }

        /// <summary>
        /// Returns the node assembly
        /// </summary>
        public string LegacyAssembly { get; private set; }

        /// <summary>
        /// Returns the original node DSFunction description or UI node type
        /// </summary>
        public string LegacyFullName { get; private set; }

        /// <summary>
        /// Node can be Deprecated or Unresolved
        /// </summary>
        public Nature NodeNature { get; private set; }

        /// <summary>
        /// Xml node
        /// </summary>
        public object OriginalNodeContent { get; private set; }

        /// <summary>
        /// This property returns the originalXmlContent if it exists or returns null.
        /// </summary>
        private XmlElement OriginalXmlNodeContent
        {
            get
            {
                if (OriginalNodeContent == null)
                    return null;

                XmlElement originalXmlElement = OriginalNodeContent as XmlElement;
                if (originalXmlElement == null)
                {
                    return null;
                }

                return originalXmlElement;
            }
        }

        /// <summary>
        /// Deserializes and returns the nodeModel that is represented by the original content of this DummyNode.
        /// If this node cannot be resolved, returns a new DummyNode
        /// </summary>
        /// <param name="json"></param>
        /// <param name="libraryServices"></param>
        /// <param name="factory"></param>
        /// <param name="isTestMode"></param>
        /// <param name="manager"></param>
        internal NodeModel GetNodeModelForDummyNode(string json, LibraryServices libraryServices,
                                                  NodeFactory factory, bool isTestMode, CustomNodeManager manager)
        {
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                Culture = CultureInfo.InvariantCulture,
                Converters = new List<JsonConverter>{
                        new NodeReadConverter(manager, libraryServices, factory, isTestMode),
                        new TypedParameterConverter()
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var result = SerializationExtensions.ReplaceTypeDeclarations(json, true);
            var resolvedNodeModel = JsonConvert.DeserializeObject<NodeModel>(result, settings);

            // If the resolved node model is not a dummy node, then copy the node view properties from the dummy node to the resolved version of that node. 
            if (!(resolvedNodeModel is DummyNode))
            {
                SetNodeViewDataOnResolvedNode(this, resolvedNodeModel);
            }
            else
            {
                this.Log(string.Format("This graph has a node with id:{0} and name:{1}, but it could not be resolved",
                                       resolvedNodeModel.GUID, resolvedNodeModel.Name)
                                       , WarningLevel.Moderate);
            }

            return resolvedNodeModel;
        }

        /// <summary>
        /// This will set the dummy node's node view properties to the resolved node
        /// </summary>
        /// <param name="dummyNode"></param>
        /// <param name="resolvedNode"></param>
        private void SetNodeViewDataOnResolvedNode(NodeModel dummyNode, NodeModel resolvedNode)
        {
            if (dummyNode == null || resolvedNode == null)
            {
                return;
            }

            resolvedNode.X = dummyNode.X;
            resolvedNode.Y = dummyNode.Y;
            resolvedNode.IsFrozen = dummyNode.IsFrozen;
            resolvedNode.IsSetAsInput = dummyNode.IsSetAsInput;
            resolvedNode.IsSetAsOutput = dummyNode.IsSetAsOutput;

            // NOTE: The name needs to be set using UpdateValue to cause the view to update
            resolvedNode.UpdateValue(new UpdateValueParams("Name", dummyNode.Name));
            resolvedNode.UpdateValue(new UpdateValueParams("IsVisible", dummyNode.IsVisible.ToString()));
        }
    }
}

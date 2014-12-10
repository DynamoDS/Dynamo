using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;

namespace DSCoreNodesUI
{
    [NodeName("Legacy Node")]
    [NodeDescription("This is an obsolete node")]
    [IsMetaNode]
    [IsVisibleInDynamoLibrary(false)]
    [NodeSearchable(false)]
    [IsDesignScriptCompatible]
    public class DummyNode : NodeModel
    {
        public enum Nature
        {
            Deprecated, Unresolved
        }

        public DummyNode(WorkspaceModel workspace)
            : base(workspace)
        {
            this.LegacyNodeName = "DSCoreNodesUI.DummyNode";
            this.LegacyAssembly = string.Empty;
            this.NodeNature = Nature.Unresolved;
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var inputCount = nodeElement.Attributes["inputCount"];
            var outputCount = nodeElement.Attributes["outputCount"];
            var legacyName = nodeElement.Attributes["legacyNodeName"];

            this.InputCount = Int32.Parse(inputCount.Value);
            this.OutputCount = Int32.Parse(outputCount.Value);
            this.LegacyNodeName = legacyName.Value;

            if (nodeElement.ChildNodes != null) 
            {
                foreach (XmlNode childNode in nodeElement.ChildNodes)
                    if (childNode.Name.Equals("OriginalNodeContent"))
                        this.OriginalNodeContent = (XmlElement)nodeElement.FirstChild.FirstChild;
            }
            
            var legacyAsm = nodeElement.Attributes["legacyAssembly"];
            if (legacyAsm != null)
                this.LegacyAssembly = legacyAsm.Value;

            var nodeNature = nodeElement.Attributes["nodeNature"];
            if (nodeNature != null)
            {
                var nature = Enum.Parse(typeof(Nature), nodeNature.Value);
                this.NodeNature = ((Nature)nature);
            }

            for (int input = 0; input < this.InputCount; input++)
            {
                var name = string.Format("Port {0}", input + 1);
                InPortData.Add(new PortData(name, ""));
            }

            for (int output = 0; output < this.OutputCount; output++)
            {
                var name = string.Format("Port {0}", output + 1);
                OutPortData.Add(new PortData(name, ""));
            }

            this.RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc,
            XmlElement nodeElement, SaveContext context)
        {
            if (context == SaveContext.Copy || context == SaveContext.Undo)
            {
                //Dump all the information into memory

                nodeElement.SetAttribute("inputCount", this.InputCount.ToString());
                nodeElement.SetAttribute("outputCount", this.OutputCount.ToString());
                nodeElement.SetAttribute("legacyNodeName", this.LegacyNodeName);
                nodeElement.SetAttribute("legacyAssembly", this.LegacyAssembly);
                nodeElement.SetAttribute("nodeNature", this.NodeNature.ToString());

                if(this.OriginalNodeContent != null)
                {
                    XmlElement originalNode = xmlDoc.CreateElement("OriginalNodeContent");
                    XmlElement nodeContent = nodeElement.OwnerDocument.CreateElement(this.OriginalNodeContent.Name);

                    foreach (XmlAttribute attribute in this.OriginalNodeContent.Attributes)
                        nodeContent.SetAttribute(attribute.Name, attribute.Value);

                    for (int i = 0; i < this.OriginalNodeContent.ChildNodes.Count; i++)
                    {
                        XmlNode child = 
                            nodeContent.OwnerDocument.ImportNode(this.OriginalNodeContent.ChildNodes[i], true);
                        nodeContent.AppendChild(child.CloneNode(true));
                    }

                    originalNode.AppendChild(nodeContent);
                    nodeElement.AppendChild(originalNode);
                }
            }

            if (context == SaveContext.File)
            {
                //When save files, only save the original node's content, 
                //instead of saving the dummy node.
                if (this.OriginalNodeContent != null)
                {
                    XmlElement originalNode = nodeElement.OwnerDocument.CreateElement(this.OriginalNodeContent.Name);
                    foreach (XmlAttribute attribute in this.OriginalNodeContent.Attributes)
                        originalNode.SetAttribute(attribute.Name, attribute.Value);

                    //overwrite the guid/x/y value of the original node.
                    originalNode.SetAttribute("guid", nodeElement.GetAttribute("guid"));
                    originalNode.SetAttribute("x", nodeElement.GetAttribute("x"));
                    originalNode.SetAttribute("y", nodeElement.GetAttribute("y"));

                    for (int i = 0; i < this.OriginalNodeContent.ChildNodes.Count; i++)
                    {
                        XmlNode child =
                            originalNode.OwnerDocument.ImportNode(this.OriginalNodeContent.ChildNodes[i], true);
                        originalNode.AppendChild(child.CloneNode(true));
                    }

                    nodeElement.ParentNode.ReplaceChild(originalNode, nodeElement);
                }
                else {
                    nodeElement.SetAttribute("inputCount", this.InputCount.ToString());
                    nodeElement.SetAttribute("outputCount", this.OutputCount.ToString());
                    nodeElement.SetAttribute("legacyNodeName", this.LegacyNodeName);
                    nodeElement.SetAttribute("legacyAssembly", this.LegacyAssembly);
                    nodeElement.SetAttribute("nodeNature", this.NodeNature.ToString());
                }
            }
        }

        #region SerializeCore/DeserializeCore

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            this.SaveNode(element.OwnerDocument, element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            InPortData.Clear();  // In/out ports are going to be recreated in 
            OutPortData.Clear(); // LoadNode, clear them so they don't accumulate.

            base.DeserializeCore(element, context);
            this.LoadNode(element);
        }

        #endregion

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }

        public string GetDescription()
        {
            if (this.NodeNature == Nature.Deprecated)
            {
                if (string.IsNullOrEmpty(this.LegacyAssembly))
                {
                    var format = "Node of type '{0}' is now deprecated";
                    return string.Format(format, this.LegacyNodeName);
                }
                else
                {
                    var format = "Node of type '{0}' ({1}) is now deprecated";
                    return string.Format(format, this.LegacyNodeName, this.LegacyAssembly);
                }
            }
            else if (this.NodeNature == Nature.Unresolved)
            {
                if (string.IsNullOrEmpty(this.LegacyAssembly))
                {
                    var format = "Node of type '{0}' cannot be resolved";
                    return string.Format(format, this.LegacyNodeName);
                }
                else
                {
                    var format = "Node of type '{0}' ({1}) cannot be resolved";
                    return string.Format(format, this.LegacyNodeName, this.LegacyAssembly);
                }
            }

            var message = "Unhandled 'DummyNode.NodeNature' value: {0}";
            throw new InvalidOperationException(string.Format(message, NodeNature.ToString()));
        }

        public override string Description
        {
            get { return this.GetDescription(); }
            set { base.Description = value; }
        }

        public int InputCount { get; private set; }
        public int OutputCount { get; private set; }
        public string LegacyNodeName { get; private set; }
        public string LegacyAssembly { get; private set; }
        public Nature NodeNature { get; private set; }
        public XmlElement OriginalNodeContent { get; private set; }
    }
}

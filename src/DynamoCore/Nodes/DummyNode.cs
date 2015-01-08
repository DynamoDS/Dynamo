using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;

namespace DSCoreNodesUI
{
    [NodeName(/*NXLT*/"Legacy Node")]
    [NodeDescription(/*NXLT*/"This is an obsolete node")]
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

        public DummyNode()
        {
            LegacyNodeName = /*NXLT*/"DSCoreNodesUI.DummyNode";
            LegacyAssembly = string.Empty;
            NodeNature = Nature.Unresolved;
            Description = GetDescription(); 
            
            ShouldDisplayPreviewCore = false;
        }

        public DummyNode(int inputCount, int outputCount, string legacyName, XmlElement originalElement, string legacyAssembly, Nature nodeNature)
        {
            InputCount = inputCount;
            OutputCount = outputCount;
            LegacyNodeName = legacyName;
            OriginalNodeContent = originalElement;
            LegacyAssembly = legacyAssembly;
            NodeNature = nodeNature;

            Description = GetDescription();
            ShouldDisplayPreviewCore = false;

            UpdatePorts();
        }

        private void LoadNode(XmlNode nodeElement)
        {
            var inputCount = nodeElement.Attributes[/*NXLT*/"inputCount"];
            var outputCount = nodeElement.Attributes[/*NXLT*/"outputCount"];
            var legacyName = nodeElement.Attributes[/*NXLT*/"legacyNodeName"];

            InputCount = Int32.Parse(inputCount.Value);
            OutputCount = Int32.Parse(outputCount.Value);
            LegacyNodeName = legacyName.Value;

            if (nodeElement.ChildNodes != null)
            {
                foreach (XmlNode childNode in nodeElement.ChildNodes)
                    if (childNode.Name.Equals("OriginalNodeContent"))
                        OriginalNodeContent = (XmlElement)nodeElement.FirstChild.FirstChild;
            }

            var legacyAsm = nodeElement.Attributes[/*NXLT*/"legacyAssembly"];
            if (legacyAsm != null)
                LegacyAssembly = legacyAsm.Value;

            var nodeNature = nodeElement.Attributes[/*NXLT*/"nodeNature"];
            if (nodeNature != null)
            {
                var nature = Enum.Parse(typeof(Nature), nodeNature.Value);
                NodeNature = ((Nature)nature);
            }

            UpdatePorts();
        }

        private void UpdatePorts()
        {
            InPortData.Clear();
            for (int input = 0; input < InputCount; input++)
            {
                var name = string.Format("Port {0}", input + 1);
                InPortData.Add(new PortData(name, string.Empty));
            }

            OutPortData.Clear();
            for (int output = 0; output < OutputCount; output++)
            {
                var name = string.Format("Port {0}", output + 1);
                OutPortData.Add(new PortData(name, string.Empty));
            }

            RegisterAllPorts();
        }

        private void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (context == SaveContext.Copy || context == SaveContext.Undo)
            {
                //Dump all the information into memory

                nodeElement.SetAttribute(/*NXLT*/"inputCount", InputCount.ToString());
                nodeElement.SetAttribute(/*NXLT*/"outputCount", OutputCount.ToString());
                nodeElement.SetAttribute(/*NXLT*/"legacyNodeName", LegacyNodeName);
                nodeElement.SetAttribute(/*NXLT*/"legacyAssembly", LegacyAssembly);
                nodeElement.SetAttribute(/*NXLT*/"nodeNature", NodeNature.ToString());

                if (OriginalNodeContent != null)
                {
                    XmlElement originalNode = xmlDoc.CreateElement("OriginalNodeContent");
                    XmlElement nodeContent = nodeElement.OwnerDocument.CreateElement(OriginalNodeContent.Name);

                    foreach (XmlAttribute attribute in OriginalNodeContent.Attributes)
                        nodeContent.SetAttribute(attribute.Name, attribute.Value);

                    for (int i = 0; i < OriginalNodeContent.ChildNodes.Count; i++)
                    {
                        XmlNode child =
                            nodeContent.OwnerDocument.ImportNode(OriginalNodeContent.ChildNodes[i], true);
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
                if (OriginalNodeContent != null)
                {
                    XmlElement originalNode = nodeElement.OwnerDocument.CreateElement(OriginalNodeContent.Name);
                    foreach (XmlAttribute attribute in OriginalNodeContent.Attributes)
                        originalNode.SetAttribute(attribute.Name, attribute.Value);

                    //overwrite the guid/x/y value of the original node.
                    originalNode.SetAttribute(/*NXLT*/"guid", nodeElement.GetAttribute("guid"));
                    originalNode.SetAttribute(/*NXLT*/"x", nodeElement.GetAttribute("x"));
                    originalNode.SetAttribute(/*NXLT*/"y", nodeElement.GetAttribute("y"));

                    for (int i = 0; i < OriginalNodeContent.ChildNodes.Count; i++)
                    {
                        XmlNode child =
                            originalNode.OwnerDocument.ImportNode(OriginalNodeContent.ChildNodes[i], true);
                        originalNode.AppendChild(child.CloneNode(true));
                    }

                    nodeElement.ParentNode.ReplaceChild(originalNode, nodeElement);
                }
                else
                {
                    nodeElement.SetAttribute(/*NXLT*/"inputCount", InputCount.ToString());
                    nodeElement.SetAttribute(/*NXLT*/"outputCount", OutputCount.ToString());
                    nodeElement.SetAttribute(/*NXLT*/"legacyNodeName", LegacyNodeName);
                    nodeElement.SetAttribute(/*NXLT*/"legacyAssembly", LegacyAssembly);
                    nodeElement.SetAttribute(/*NXLT*/"nodeNature", NodeNature.ToString());
                }
            }
        }

        #region SerializeCore/DeserializeCore

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

        public string GetDescription()
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

            const string message = /*NXLT*/"Unhandled 'DummyNode.NodeNature' value: {0}";
            throw new InvalidOperationException(string.Format(message, NodeNature));
        }
        
        public int InputCount { get; private set; }
        public int OutputCount { get; private set; }
        public string LegacyNodeName { get; private set; }
        public string LegacyAssembly { get; private set; }
        public Nature NodeNature { get; private set; }
        public XmlElement OriginalNodeContent { get; private set; }
    }
}

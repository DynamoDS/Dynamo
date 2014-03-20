using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
        public DummyNode()
        {
            this.LegacyNodeName = "DSCoreNodesUI.DummyNode";
        }

        public void SetupCustomUIElements(Dynamo.Controls.dynNodeView nodeUI)
        {
            var src = @"/DSCoreNodesUI;component/Resources/DummyNode.png";

            Image dummyNodeImage = new Image()
            {
                Source = new BitmapImage(new Uri(src, UriKind.Relative))
            };

            nodeUI.inputGrid.Children.Add(dummyNodeImage);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var inputCount = nodeElement.Attributes["inputCount"];
            var outputCount = nodeElement.Attributes["outputCount"];
            var legacyName = nodeElement.Attributes["legacyNodeName"];

            this.InputCount = Int32.Parse(inputCount.Value);
            this.OutputCount = Int32.Parse(outputCount.Value);
            this.LegacyNodeName = legacyName.Value;

            for (int input = 0; input < this.InputCount; input++)
            {
                var name = string.Format("Port {0}", input + 1);
                InPortData.Add(new PortData(name, "", typeof(object)));
            }

            for (int output = 0; output < this.OutputCount; output++)
            {
                var name = string.Format("Port {0}", output + 1);
                OutPortData.Add(new PortData(name, "", typeof(object)));
            }

            this.RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc,
            XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputCount", this.InputCount.ToString());
            nodeElement.SetAttribute("outputCount", this.OutputCount.ToString());
            nodeElement.SetAttribute("legacyNodeName", this.LegacyNodeName);
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

        private string GetDescription()
        {
            return string.Format(
                "Obsolete node type {0}", this.LegacyNodeName);
        }

        public override string Description
        {
            get { return this.GetDescription(); }
            set { base.Description = value; }
        }

        public int InputCount { get; private set; }
        public int OutputCount { get; private set; }
        public string LegacyNodeName { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using Dynamo.Graph;
using Dynamo.Graph.Nodes;

using System.Runtime.CompilerServices;

using ProtoCore.AST.AssociativeAST;
using CoreNodeModels.Properties;

using DSColor = DSCore.Color;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Workspaces;


namespace CoreNodeModels.Input
{
    [NodeName("Color Palette")]
    [NodeDescription("ColorPaletteDescription", typeof(Resources))]
    [NodeCategory("Core.Color.Create")]
    [NodeSearchTags("ColorUISearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortNames("Color")]
    [OutPortTypes("Color")]
    [OutPortDescriptions("Selected Color.")]
    public class ColorPalette : NodeModel
    {
        private DSColor dscolor = DSColor.ByARGB(255, 0, 0, 0);
        public DSColor DsColor
        {
            get { return dscolor; }
            set
            {
                dscolor = value;
                OnNodeModified();
                RaisePropertyChanged("DsColor");
            }
        }
        /// <summary>
        ///     Node constructor.
        /// </summary>
        public ColorPalette()
        {
            RegisterAllPorts();
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "DsColor":
                    this.DsColor = this.DeserializeValue(value);
                    return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        private DSColor DeserializeValue(string val)
        {
            try
            {
                //Splits the xml string and returns each of the ARGB values as a string array.
                string[] argb = Regex.Split(val, @"\D+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                return DSColor.ByARGB(int.Parse(argb[3]), int.Parse(argb[0]), int.Parse(argb[1]), int.Parse(argb[2]));
            }
            catch
            {
                return DSColor.ByARGB(255, 0, 0, 0);
            }
        }
        private string SerializeValue()
        {
            return DsColor.ToString();
        }
        /// <summary>
        ///     Store color value when graph is saved.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement color = element.OwnerDocument.CreateElement("DsColor");
            color.InnerText = SerializeValue();
            element.AppendChild(color);
        }
        /// <summary>
        ///     Restore stored value and set "dscolor" to it.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);

            var colorNode = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "DsColor");

            if (colorNode != null)
            {
                DsColor = DeserializeValue(colorNode.InnerText);
            }

        }
        /// <summary>
        ///     AST Output.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var av = AstFactory.BuildIntNode(Convert.ToInt32(DsColor.Alpha));
            var ar = AstFactory.BuildIntNode(Convert.ToInt32(DsColor.Red));
            var ag = AstFactory.BuildIntNode(Convert.ToInt32(DsColor.Green));
            var ab = AstFactory.BuildIntNode(Convert.ToInt32(DsColor.Blue));

            var colorNode = AstFactory.BuildFunctionCall(
                    new Func<int, int, int, int, DSColor>(DSColor.ByARGB), new List<AssociativeNode> { av, ar, ag, ab });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), colorNode) };
        }

        /// <summary>
        ///     Indicates whether node is input node
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }
    }
}
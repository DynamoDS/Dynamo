using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Xml;

using Dynamo.Graph;
using Dynamo.Graph.Nodes;

using ProtoCore.AST.AssociativeAST;
using CoreNodeModels.Properties;

using DSColor = DSCore.Color;

using Autodesk.DesignScript.Runtime;


namespace CoreNodeModels.Input
{
    [NodeName("Color Palette")]
    [NodeDescription("CustomNodeModelDescription", typeof(Resources))]
    [NodeCategory("Core.Color.Create")]
    [NodeSearchTags("ColorUISearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortNames("Color")]
    [OutPortTypes("Color")]
    [OutPortDescriptions("Selected Color.")]
    public class ColorPalette : NodeModel
    {
        private Color scolor = Color.FromArgb(255, 0, 0, 0);                     
        /// <summary>
        ///     Color value.
        /// </summary>
        public Color sColor
        {
            get { return scolor; }
            set
            {
                scolor = value;
                RaisePropertyChanged("sColor");
                OnNodeModified();
            }
        }                
        /// <summary>
        ///     Node constructor.
        /// </summary>
        public ColorPalette()
        {
            RegisterAllPorts();
        }

        private Color DeserializeValue(string val)
        {
            try
            {
                return sColor = (Color)ColorConverter.ConvertFromString(val);
            }
            catch
            {
                return sColor = Color.FromArgb(255, 0, 0, 0);
            }
        }

        private string SerializeValue()
        {
            return scolor.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Store color value when graph is saved.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement color = element.OwnerDocument.CreateElement("sColor");
            color.InnerText = SerializeValue();
            element.AppendChild(color);
        }

        /// <summary>
        ///     Restore stored value and set "scolor" to it.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);

            var colorNode = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "sColor");

            if (colorNode != null)
            {
                scolor = DeserializeValue(colorNode.InnerText);
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
            var av = AstFactory.BuildIntNode(Convert.ToInt32(sColor.A));
            var ar = AstFactory.BuildIntNode(Convert.ToInt32(sColor.R));
            var ag = AstFactory.BuildIntNode(Convert.ToInt32(sColor.G));
            var ab = AstFactory.BuildIntNode(Convert.ToInt32(sColor.B));

            var colorNode = AstFactory.BuildFunctionCall(
                    new Func<int, int, int, int, DSColor>(DSColor.ByARGB), new List<AssociativeNode> { av, ar, ag, ab });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), colorNode) };
        }

        /// <summary>
        ///     Override string representation of color in watch node.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Color(Red = {0}, Green = {1}, Blue = {2}, Alpha = {3})", scolor.R, scolor.G, scolor.B, scolor.A);
        }
    }
}

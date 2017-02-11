using System;
using System.Collections.Generic;
using System.Linq;
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
        private DSColor dscolor = DSColor.ByARGB(255,0,0,0);
        public DSColor dsColor
        {
            get { return dscolor; }
            set
            {                              
                dscolor = value;                
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
        private DSColor DeserializeValue(string val)
         {
            try
            {
                var t = val;
                return dsColor; //= (DSColor)ColorConverter.ConvertFromString(val);
            }
            catch
            {
                return dsColor = DSColor.ByARGB(255, 0, 0, 0);
            }
        }
        private string SerializeValue()
        {
            return dsColor.ToString();
        }
        /// <summary>
        ///     Store color value when graph is saved.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement color = element.OwnerDocument.CreateElement("dsColor");
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

            var colorNode = element.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "dsColor");

            if (colorNode != null)
            {
                dsColor = DeserializeValue(colorNode.InnerText);
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
            var av = AstFactory.BuildIntNode(Convert.ToInt32(dsColor.Alpha));
            var ar = AstFactory.BuildIntNode(Convert.ToInt32(dsColor.Red));
            var ag = AstFactory.BuildIntNode(Convert.ToInt32(dsColor.Green));
            var ab = AstFactory.BuildIntNode(Convert.ToInt32(dsColor.Blue));

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
            return string.Format("Color(Red = {0}, Green = {1}, Blue = {2}, Alpha = {3})", dsColor.Red, dsColor.Green, dsColor.Blue, dsColor.Alpha);
        }
    }
}

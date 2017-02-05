using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using CoreNodeModels.Properties;
using Newtonsoft.Json;
using System.Windows.Media;
using DSColor = DSCore.Color;

using System;
using System.Globalization;
using System.Xml;
using System.Linq;

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
        private ObservableCollection<Xceed.Wpf.Toolkit.ColorItem> cList = new ObservableCollection<Xceed.Wpf.Toolkit.ColorItem>()
        {
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,48,130,189), "Blue 1"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,106,173,213), "Blue 2"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,158,202,225), "Blue 3"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,199,219,238), "Blue 4"),

            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,229,87,37), "Orange 1"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,246,140,63), "Orange 2"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,229,87,37), "Orange 3"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,252,208,162), "Orange 4"),

            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,49,163,83), "Green 1"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,117,195,118), "Green 2"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,162,211,154), "Green 3"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,200,228,191), "Green 4"),

            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,117,107,177), "Purple 1"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,158,154,199), "Purple 2"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,189,189,219), "Purple 3"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,218,218,234), "Purple 4"),

            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,99,99,99), "Grey 1"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,150,150,150), "Grey 2"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,189,189,189), "Grey 3"),
            new Xceed.Wpf.Toolkit.ColorItem(Color.FromArgb(255,217,217,217), "Grey 4"),
        };

        /// <summary>
        ///     List of standard colors.
        /// </summary>
        public ObservableCollection<Xceed.Wpf.Toolkit.ColorItem> ColorList
        {
            get { return cList; }
        }

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using System.Runtime.CompilerServices;

using Dynamo.Graph;
using Dynamo.Graph.Nodes;

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
    public class ColorPalette : NodeModel, INotifyPropertyChanged
    {
        private DSColor dscolor = DSColor.ByARGB(255, 0, 0, 0);
        private bool Isundo = false;
        public DSColor DsColor
        {
            get { return dscolor; }
            set
            {                
                if (dscolor.Equals(value))
                {
                    Isundo = true;                 
                }
                dscolor = value;                            
                if (!Isundo)
                {
                    Isundo = false;
                    Update = dscolor;                   
                }               
                OnNodeModified();
                OnPropertyChanged();
            }
        }
        
        public DSColor Update
        {
            set
            {
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        ///     Override string representation of color in watch node.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Color(Alpha = {3}, Red = {0}, Green = {1}, Blue = {2})", DsColor.Alpha, DsColor.Red, DsColor.Green, DsColor.Blue);
        }
    }
}
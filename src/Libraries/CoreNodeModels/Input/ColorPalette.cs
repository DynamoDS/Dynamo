using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoCore.AST.AssociativeAST;
using DSColor = DSCore.Color;

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

        [JsonProperty(PropertyName = "InputValue")]
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

        //override ExtensionNode NodeType from NodeModel base class
        public override string NodeType
        {
            get
            {
                return "ColorInputNode";
            }
        }

        public override NodeInputData InputData
        {
            get
            {
                //this object which we'll convert to a json string matches the format the schema expects for colors
                var colorObj = new { Red = Convert.ToInt32(DsColor.Red), Blue = Convert.ToInt32(DsColor.Blue), Green = Convert.ToInt32(DsColor.Green), Alpha = Convert.ToInt32(DsColor.Alpha) };

                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,

                    Type = NodeInputTypes.colorInput,
                    Description = this.Description,
                    Value = JsonConvert.SerializeObject(colorObj),
                };
            }
        }
        /// <summary>
        ///     Node constructor.
        /// </summary>
        public ColorPalette()
        {
            RegisterAllPorts();
        }
        
        [JsonConstructor]
        public ColorPalette(JObject InputValue, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            // RGBA to ARGB
            try
            {
                this.DsColor = DSColor.ByARGB((int)InputValue["A"], (int)InputValue["R"], (int)InputValue["G"], (int)InputValue["B"]);
            }

            catch
            {
                this.DsColor = DSColor.ByARGB(255, 0, 0, 0);
            }

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
        ///     Indicates whether node is input node. 
        ///     Used to bind visibility of UI for user selection.
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }
    }
}
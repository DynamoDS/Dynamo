using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("IntegerSliderNodeDescription", typeof(Resources))]
    [NodeSearchTags("IntegerSliderSearchTags", typeof(Resources))]
    [InPortTypes("UI Input")]
    [OutPortTypes("int")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Input.IntegerSlider")]
    public class IntegerSlider : SliderBase<int>
    {
        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "NumberInputNode";
            }
        }

        public string NumberType
        {
            get
            {
                return "Integer";
            }
        }
        public override NodeInputData InputData
        {
           get
            {
                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    Type = NodeInputTypes.numberInput,
                    Description = this.Description,
                    Value = Value.ToString(CultureInfo.InvariantCulture),

                    MinimumValue = this.Min,
                    MaximumValue = this.Max,
                    StepValue = this.Step,
                    NumberType = this.NumberType,

                };
            }
        }


        [JsonConstructor]
        private IntegerSlider(IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts): base(inPorts, outPorts)
        {
            Min = 0;
            Max = 100;
            Step = 1;
            Value = 1;
            ShouldDisplayPreviewCore = false;
        }

        public IntegerSlider()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 1;
            ShouldDisplayPreviewCore = false;
        }

        //If the value field in the slider has a number greater than
        //In32.Maxvalue (or MinValue), the value will be changed to Int32.MaxValue (or MinValue)
        //The value will be changed, but to update the UI, this property is overridden here. 
        public override int Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;              
                RaisePropertyChanged("Value");
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "Min":
                case "MinText":
                    Min = ConvertStringToInt(value);
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = ConvertStringToInt(value);
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = ConvertStringToInt(value);                   
                    return true; // UpdateValueCore handled.
                case "Step":
                case "StepText":
                    Step = ConvertStringToInt(value);
                    return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] {assignment};
        }

        protected override int DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var xmlDocument = element.OwnerDocument;
            var subNode = xmlDocument.CreateElement("Range");
            subNode.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("step", Step.ToString(CultureInfo.InvariantCulture));
            element.AppendChild(subNode);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.

            foreach (XmlNode subNode in element.ChildNodes)
            {
                if (!subNode.Name.Equals("Range"))
                    continue;
                if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                    continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "min":
                            Min = ConvertStringToInt(attr.Value);
                            break;
                        case "max":
                            Max = ConvertStringToInt(attr.Value);
                            break;
                        case "step":
                            Step = ConvertStringToInt(attr.Value);
                            break;
                        default:
                            Log(string.Format("{0} attribute could not be deserialized for {1}", attr.Name, GetType()));
                            break;
                    }
                }

                break;
            }
        }

        #endregion
    }
}

namespace Dynamo.Nodes
{
    public class IntegerSlider
    {
        [NodeMigration(version: "0.7.5.0")]
        public static NodeMigrationData Migrate_0750(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "CoreNodeModels.Input.IntegerSlider", "Integer Slider", true);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}

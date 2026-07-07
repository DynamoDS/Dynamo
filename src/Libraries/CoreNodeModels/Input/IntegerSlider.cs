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
    [Obsolete("IntegerSlider will be removed in favor of IntegerSlider64Bit in a future version of Dynamo")]
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("IntegerSliderNodeDescription", typeof(Resources))]
    [NodeSearchTags("IntegerSliderSearchTags", typeof(Resources))]
    [InPortTypes("UI Input")]
    [OutPortTypes("int")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
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
                    Type2 = NodeInputTypes.numberInput,
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
            IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
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

            return new[] { assignment };
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

    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("IntegerSliderNodeDescription", typeof(Resources))]
    [NodeSearchTags("IntegerSliderSearchTags", typeof(Resources))]
    [InPortTypes("UI Input")]
    [OutPortTypes("int")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider64Bit : SliderBase<long>
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
                    Type2 = NodeInputTypes.numberInput,
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
        private IntegerSlider64Bit(IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts): base(inPorts, outPorts)
        {
            Min = 0;
            Max = 100;
            Step = 1;
            Value = 1;
            ShouldDisplayPreviewCore = false;

            var outport = outPorts.First();
            outport.ToolTip = "Int64";
        }

        public IntegerSlider64Bit()
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 1;
            ShouldDisplayPreviewCore = false;
        }

        // If the value field in the slider has a number greater than
        // long.MaxValue (or MinValue), the value will be changed to long.MaxValue (or MinValue)
        // The property setter is overridden here to update the UI, in case the value is changed. 
        public override long Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;              
                RaisePropertyChanged(nameof(Value));
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case nameof(Min):
                case "MinText":
                case nameof(Max):
                case "MaxText":
                case nameof(Step):
                case "StepText":
                case nameof(Value):
                case "ValueText":
                    if (string.IsNullOrEmpty(value))
                        return false;

                    // Reject anything that isn't a strict Int64 literal (no decimals/thousands
                    // separators). Distinguish out-of-range from non-numeric so the user gets
                    // an accurate error message; both cases keep the last valid value.
                    if (!TryParseInt64(value, out long parsed, out bool isOutOfRange))
                    {
                        Info(Resources.IntegerSliderNonIntegerInputMessage, true);

                        // The textbox is OneWay-bound and already displays the rejected text.
                        // Nothing reverts it automatically since Min/Max/Step/Value never changed.
                        // Re-raise the change notification for the canonical property so
                        // SliderViewModel re-reads the last valid value and the stale, invalid
                        // text the user typed is visibly replaced.
                        RaisePropertyChanged(ToCanonicalPropertyName(name));
                        return false;
                    }

                    if (isOutOfRange)
                    {
                        Info(Resources.IntegerSliderInfoMessage, true);
                    }
                    else
                    {
                        ClearInfoMessages();
                    }

                    switch (name)
                    {
                        case nameof(Min):
                        case "MinText":
                            Min = parsed;
                            return true;
                        case nameof(Max):
                        case "MaxText":
                            Max = parsed;
                            return true;
                        case nameof(Step):
                        case "StepText":
                            Step = parsed;
                            return true;
                        case nameof(Value):
                        case "ValueText":
                            Value = parsed;
                            return true;
                    }
                    break;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        /// <summary>
        /// Attempts to parse a strict 64-bit integer literal. Returns false only when
        /// <paramref name="value"/> is not an integer literal at all (contains a decimal point,
        /// letters, etc.). If <paramref name="value"/> is a well-formed integer (optional sign
        /// followed only by digits) that exceeds the Int64 range, <paramref name="isOutOfRange"/>
        /// is set to true and <paramref name="result"/> is clamped to Int64.MaxValue/MinValue,
        /// matching how the slider already clamps values dragged or set past Min/Max.
        /// </summary>
        private static bool TryParseInt64(string value, out long result, out bool isOutOfRange)
        {
            isOutOfRange = false;

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                return true;

            var start = value.Length > 0 && (value[0] == '-' || value[0] == '+') ? 1 : 0;
            if (start < value.Length && value.Skip(start).All(char.IsDigit))
            {
                isOutOfRange = true;
                result = value[0] == '-' ? long.MinValue : long.MaxValue;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        /// Maps a text-box property name (e.g. "MinText") to the canonical model property
        /// name (e.g. "Min") that <see cref="SliderViewModel{T}"/> listens for when deciding
        /// which bound text (MinText/MaxText/StepText/ValueText) to refresh.
        /// </summary>
        private static string ToCanonicalPropertyName(string name)
        {
            switch (name)
            {
                case nameof(Min):
                case "MinText":
                    return "Min";
                case nameof(Max):
                case "MaxText":
                    return "Max";
                case nameof(Step):
                case "StepText":
                    return "Step";
                default:
                    return "Value";
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] {assignment};
        }

        protected override long DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt64(val, CultureInfo.InvariantCulture);
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
            var subNode = xmlDocument.CreateElement(nameof(Range));
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
                if (!subNode.Name.Equals(nameof(Range)))
                    continue;
                if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                    continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "min":
                            Min = ConvertStringToInt64(attr.Value);
                            break;
                        case "max":
                            Max = ConvertStringToInt64(attr.Value);
                            break;
                        case "step":
                            Step = ConvertStringToInt64(attr.Value);
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

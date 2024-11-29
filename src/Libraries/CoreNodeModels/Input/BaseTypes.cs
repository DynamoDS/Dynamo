using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CoreNodeModels.Properties;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("StringInputNodeDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.StringInput", "Dynamo.Nodes.dynStringInput", "DSCoreNodesUI.Input.StringInput")]
    public class StringInput : String
    {
        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public static Dictionary<Guid, Tuple<double, double>> NodeSizes = new Dictionary<Guid, Tuple<double, double>>();

        public double Width { get; set; } = 200; // Default width
        public double Height { get; set; } = 31; // Default height

        public Guid GUID { get; set; } = Guid.NewGuid();

        public override string NodeType
        {
            get
            {
                return "StringInputNode";
            }
        }

        [JsonConstructor]
        private StringInput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Value = "";
            ShouldDisplayPreviewCore = false;
        }

        public StringInput()
        {
            RegisterAllPorts();
            Value = "";
            ShouldDisplayPreviewCore = false;
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "Value")
            {
                Value = value; 
                return true; // UpdateValueCore handled.
            }

            // There's another 'UpdateValueCore' method in 'String' base class,
            // since they are both bound to the same property, 'StringInput' 
            // should be given a chance to handle the property value change first
            // before the base class 'String'.
            return base.UpdateValueCore(updateValueParams);
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement(typeof(string).FullName);

            var helper = new XmlElementHelper(outEl);
            helper.SetAttribute("value", SerializeValue());
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            // Use XmlElementHelper to read attributes for Width and Height
            var helper = new XmlElementHelper(nodeElement);

            // Assign Width and Height with defaults if missing
            Width = helper.ReadDouble("Width", this.Width); 
            Height = helper.ReadDouble("Height", this.Height); 

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(attr.Value);
                        }
                    }
                }
            }
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            string value = Value;
            if (context == CompilationContext.NodeToCode)
            {
                value = value.Replace(@"\", @"\\")
                    .Replace("\"", "\\\"");
            }
            var rhs = AstFactory.BuildStringNode(value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public delegate double ConversionDelegate(double value);

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("DoubleInputNodeDescription", typeof(Resources))]
    [OutPortTypes("double")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.DoubleInput", "Dynamo.Nodes.dynDoubleInput", "DSCoreNodesUI.Input.DoubleInput")]
    public class DoubleInput : NodeModel
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
        public override NodeInputData InputData
        {
           get {
                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    Type = NodeInputTypes.numberInput,
                    Type2 = NodeInputTypes.numberInput,
                    Description = this.Description,
                    Value = Value,

                    NumberType = this.NumberType,

                };
            }
        }

        public string NumberType
        {
            get
            {
                return "Double";
            }
        }

        [JsonConstructor]
        private DoubleInput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
            ConvertToken = Convert;
            Value = "0";
        }

        public DoubleInput()
        {
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", "Double")));
            RegisterAllPorts();

            ShouldDisplayPreviewCore = false;
            ConvertToken = Convert;
            Value = "0";
        }

        public virtual double Convert(double value)
        {
            return value;
        }

        void Preferences_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NumberFormat":
                    RaisePropertyChanged("Value");
                    break;
            }
        }

        private List<IDoubleSequence> _parsed;
        private string _value;
        protected ConversionDelegate ConvertToken;

        /// <summary>
        /// This property sets the value of the number node, but is validated
        /// on the view - it does not allow range syntax
        /// or unassigned identifier syntax.i.e *start..end*
        /// This property is only validated for new user input.
        /// </summary>
        [JsonProperty("InputValue",Order = 10),JsonConverter(typeof(DoubleInputValueSerializationConverter))]
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != null && _value.Equals(value))
                    return;

                _value = value;

                var idList = new List<string>();

                try
                {
                    _parsed = ParseValue(value, new[] { '\n' }, idList, ConvertToken);

                    InPorts.Clear();

                    foreach (var id in idList)
                    {
                        InPorts.Add(new PortModel(PortType.Input, this, new PortData(id, "variable")));
                    }

                    ClearErrorsAndWarnings();

                    ArgumentLacing = InPorts.Any() ? LacingStrategy.Longest : LacingStrategy.Disabled;
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }

                if (value != null)
                {
                    OnNodeModified();
                }
                else
                {
                    ClearDirtyFlag();
                }
                
                RaisePropertyChanged("Value");
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "Value")
            {
                Value = value;
                return true; // UpdateValueCore handled.
            }
            return base.UpdateValueCore(updateValueParams);
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called

            XmlElement outEl = element.OwnerDocument.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value);
            element.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            foreach (
                XmlNode subNode in
                    element.ChildNodes.Cast<XmlNode>()
                        .Where(subNode => subNode.Name.Equals(typeof(double).FullName)))
            {
                Value = subNode.Attributes[0].Value;
            }
        }

        #endregion

        private static readonly Regex IdentifierPattern = new Regex(@"(?<id>[a-zA-Z_][^ ]*)|\[(?<id>\w(?:[^}\\]|(?:\\}))*)\]");

        public static List<IDoubleSequence> ParseValue(string text, char[] seps, List<string> identifiers, ConversionDelegate convertToken)
        {
            var idSet = new HashSet<string>(identifiers);
            var doubleSequenceList = text.Replace(" ", "")
                                            .Split(seps, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => new OneNumber(ParseToken(x, idSet,identifiers), convertToken) as IDoubleSequence)
                                            .ToList();
            return doubleSequenceList;
        }

        private static IDoubleInputToken ParseToken(string id, HashSet<string> identifiers, List<string> list)
        {
            double dbl;
            if (double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                return new DoubleToken(dbl);

            var match = IdentifierPattern.Match(id);
            if (match.Success)
            {
                var tokenId = match.Groups["id"].Value;
                if (!identifiers.Contains(tokenId))
                {
                    identifiers.Add(tokenId);
                    list.Add(tokenId);
                }
                return new IdentifierToken(tokenId);
            }

            throw new Exception("Bad identifier syntax: \"" + id + "\"");
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            var paramDict = InPorts.Select(x => x.Name)
                   .Zip<string, AssociativeNode, Tuple<string, AssociativeNode>>(inputAstNodes, Tuple.Create)
                   .ToDictionary(x => x.Item1, x => x.Item2);

            AssociativeNode rhs;

            if (null == _parsed)
            {
                rhs = AstFactory.BuildNullNode();
            }
            else
            {
                List<AssociativeNode> newInputs = _parsed.Count == 1
                    ? new List<AssociativeNode> { _parsed[0].GetAstNode(paramDict) }
                    : _parsed.Select(x => x.GetAstNode(paramDict)).ToList();

                rhs = newInputs.Count == 1
                        ? newInputs[0]
                        : AstFactory.BuildExprList(newInputs);
            }

            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        public interface IDoubleSequence
        {
            object GetFSchemeValue(Dictionary<string, double> idLookup);
            IEnumerable<double> GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private class OneNumber : IDoubleSequence
        {
            private readonly IDoubleInputToken _token;
            private readonly double? _result;
            private readonly ConversionDelegate _convert;

            public OneNumber(IDoubleInputToken t, ConversionDelegate convertToken)
            {
                _token = t;
                _convert = convertToken;

                if (_token is DoubleToken)
                    _result = _convert(GetValue(null).First());
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup).First();
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                yield return _result ?? _token.GetValue(idLookup);
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                if (_result == null)
                {
                    return _token.GetAstNode(idLookup);
                }
                else
                {
                    return _result.HasValue
                        ? (new DoubleToken(_result.Value)).GetAstNode(idLookup)
                        : new NullNode() as AssociativeNode;
                }
            }
        }

        interface IDoubleInputToken
        {
            double GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private struct IdentifierToken : IDoubleInputToken
        {
            private readonly string _id;

            public IdentifierToken(string id)
            {
                _id = id;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return idLookup[_id];
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                return idLookup[_id];
            }
        }

        private struct DoubleToken : IDoubleInputToken
        {
            private readonly double _d;

            public DoubleToken(double d)
            {
                _d = d;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return _d;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                if (Math.Floor(_d) == _d)
                    return AstFactory.BuildIntNode((long)_d);
                return AstFactory.BuildDoubleNode(_d);
            }
        }

        private class DoubleInputValueSerializationConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(string);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                double doubleVal = System.Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
                return doubleVal.ToString(CultureInfo.InvariantCulture);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                double d = 0.0;
                double.TryParse((string)value, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
                writer.WriteValue(d);
            }
        }

        [NodeMigration(version: "2.0.0.0")]
        public static NodeMigrationData Migrate_Range(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var oldNode = data.MigratedNodes.ElementAt(0);
            var valEls = oldNode.GetElementsByTagName("System.Double"); // The Value node
            var val = valEls[0].Attributes["value"].Value;

            double result = 0.0;
            if (double.TryParse(val, out result))
            {
                return data;
            }

            var newNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            newNode.Attributes["CodeText"].Value = val + ";";
            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}

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
using ProtoCore.DSASM;

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
            var rhs = AstFactory.BuildStringNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public delegate double ConversionDelegate(double value);

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("DoubleInputNodeDescription", typeof(Resources))]
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
        [JsonProperty("InputValue"),JsonConverter(typeof(DoubleInputValueSerializationConverter))]
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
        private static readonly string[] RangeSeparatorTokens = { "..", ":", };

        public static List<IDoubleSequence> ParseValue(string text, char[] seps, List<string> identifiers, ConversionDelegate convertToken)
        {
            var idSet = new HashSet<string>(identifiers);
            return text.Replace(" ", "").Split(seps, StringSplitOptions.RemoveEmptyEntries).Select(
                delegate(string x)
                {
                    var rangeIdentifiers = x.Split(
                        RangeSeparatorTokens,
                        StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                    if (rangeIdentifiers.Length > 3)
                        throw new Exception("Bad range syntax: not of format \"start..end[..(increment|#count)]\"");

                    if (rangeIdentifiers.Length == 0)
                        throw new Exception("No identifiers found.");

                    IDoubleInputToken startToken = ParseToken(rangeIdentifiers[0], idSet, identifiers);

                    if (rangeIdentifiers.Length > 1)
                    {
                        if (rangeIdentifiers[1].StartsWith("#"))
                        {
                            var countToken = rangeIdentifiers[1].Substring(1);
                            IDoubleInputToken endToken = ParseToken(countToken, idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#") || rangeIdentifiers[2].StartsWith("~"))
                                    throw new Exception("Cannot use range or approx. identifier on increment field when one has already been used to specify a count.");
                                return new Sequence(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }

                            return new Sequence(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }
                        else
                        {
                            IDoubleInputToken endToken = ParseToken(rangeIdentifiers[1], idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#"))
                                {
                                    var count = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken countToken = ParseToken(count, idSet, identifiers);

                                    return new CountRange(startToken, countToken, endToken, convertToken);
                                }

                                if (rangeIdentifiers[2].StartsWith("~"))
                                {
                                    var approx = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken approxToken = ParseToken(approx, idSet, identifiers);

                                    return new ApproxRange(startToken, approxToken, endToken, convertToken);
                                }

                                return new Range(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }

                            double identifierValue0, identifierValue1;
                            var canBeParsed0 = System.Double.TryParse(rangeIdentifiers[0], out identifierValue0);
                            var canBeParsed1 = System.Double.TryParse(rangeIdentifiers[1], out identifierValue1);

                            //both of the value can be parsed as double
                            if (canBeParsed0 && canBeParsed1)
                            {
                                if (identifierValue0 < identifierValue1)
                                    return new Range(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                                else
                                    return new Range(startToken, new DoubleToken(-1), endToken, convertToken) as IDoubleSequence;
                            }

                            //the input cannot be parsed as double, return a default function and let it handle the error
                            return new Range(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }

                    }

                    return new OneNumber(startToken, convertToken) as IDoubleSequence;
                }).ToList();
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

        private class Sequence : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _count;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Sequence(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken count, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _count = count;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _count is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup);
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _step.GetValue(idLookup);

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _start.GetValue(idLookup);
                    var count = (int)_count.GetValue(idLookup);

                    if (count < 0)
                    {
                        count *= -1;
                        start += step * (count - 1);
                        step *= -1;
                    }

                    return CreateSequence(start, step, count);
                }
                return _result;
            }

            private static IEnumerable<double> CreateSequence(double start, double step, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return start;
                    start += step;
                }
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    From = _start.GetAstNode(idLookup),
                    To = _count.GetAstNode(idLookup),
                    Step = _step.GetAstNode(idLookup),
                    HasRangeAmountOperator = true,
                    StepOperator = RangeStepOperator.StepSize
                };
                return rangeExpr;
            }
        }

        private class Range : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _end;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Range(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _end = end;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _end is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup);
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _convert(_step.GetValue(idLookup));

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _convert(_start.GetValue(idLookup));
                    var end = _convert(_end.GetValue(idLookup));

                    return Process(start, step, end);
                }
                return _result;
            }

            private IEnumerable<double> _Range(double start, double step, double stop)
            {
                var current = start;
                while (current <= stop)
                {
                    yield return current;
                    current += step;
                }
            }

            protected virtual IEnumerable<double> Process(double start, double step, double end)
            {
                if (step < 0)
                {
                    step *= -1;
                    var tmp = end;
                    end = start;
                    start = tmp;
                }

                var countingUp = start < end;

                return countingUp
                    ? _Range(start, step, end)
                    : _Range(end, step, start).Reverse();
            }

            protected virtual RangeStepOperator GetRangeExpressionOperator()
            {
                return RangeStepOperator.StepSize;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    From = _start.GetAstNode(idLookup),
                    To = _end.GetAstNode(idLookup),
                    Step = _step.GetAstNode(idLookup),
                    StepOperator = GetRangeExpressionOperator()
                };
                return rangeExpr;
            }
        }

        private class CountRange : Range
        {
            public CountRange(IDoubleInputToken startToken, IDoubleInputToken countToken, IDoubleInputToken endToken, ConversionDelegate convertToken)
                : base(startToken, countToken, endToken, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double count, double end)
            {
                var c = (int)count;

                var neg = c < 0;

                c = Math.Abs(c) - 1;

                if (neg)
                    c *= -1;

                return base.Process(start, Math.Abs(start - end) / c, end);
            }

            protected override RangeStepOperator GetRangeExpressionOperator()
            {
                return RangeStepOperator.Number;
            }
        }

        private class ApproxRange : Range
        {
            public ApproxRange(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken)
                : base(start, step, end, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double approx, double end)
            {
                var neg = approx < 0;

                var a = Math.Abs(approx);

                var dist = end - start;
                var stepnum = 1;
                if (dist != 0)
                {
                    var ceil = (int)Math.Ceiling(dist / a);
                    var floor = (int)Math.Floor(dist / a);

                    if (ceil != 0 && floor != 0)
                    {
                        var ceilApprox = Math.Abs(dist / ceil - a);
                        var floorApprox = Math.Abs(dist / floor - a);
                        stepnum = ceilApprox < floorApprox ? ceil : floor;
                    }
                }

                if (neg)
                    stepnum *= -1;

                return base.Process(start, Math.Abs(dist) / stepnum, end);
            }

            protected override RangeStepOperator GetRangeExpressionOperator()
            {
                return RangeStepOperator.ApproximateSize;
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
                    return AstFactory.BuildIntNode((int)_d);
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

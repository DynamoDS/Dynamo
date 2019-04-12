using System;
using System.Collections.Generic;
using System.Globalization;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    [NodeName("Date Time")]
    [NodeDescription("DateTimeDescription", typeof(Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [OutPortTypes("dateTime")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.DateTime", "DSCoreNodesUI.Input.DateTime")]
    public class DateTime : BasicInteractive<System.DateTime>
    {
        [JsonConstructor]
        private DateTime(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
        }

        public DateTime()
        {
            Value = System.DateTime.UtcNow;
            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
        }

        public override NodeInputData InputData
        {
            get
            {
                return new NodeInputData()
                {
                    Id = this.GUID,
                    Name = this.Name,
                    Type = NodeInputData.getNodeInputTypeFromType(typeof(System.DateTime)),
                    Description = this.Description,
                    //format dateTime with swagger spec in mind:  ISO 8601.
                    Value = Value.ToString("o", CultureInfo.InvariantCulture),
                };
            }
        }

        /// <summary>
        /// The NodeType property provides a name which maps to the
        /// server type for the node. This property should only be
        /// used for serialization.
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "DateTimeInputNode";
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var yearNode = AstFactory.BuildIntNode(Value.Year);
            var monthNode = AstFactory.BuildIntNode(Value.Month);
            var dayNode = AstFactory.BuildIntNode(Value.Day);
            var hourNode = AstFactory.BuildIntNode(Value.Hour);
            var minuteNode = AstFactory.BuildIntNode(Value.Minute);
            var secondNode = AstFactory.BuildIntNode(Value.Second);
            var msNode = AstFactory.BuildIntNode(Value.Millisecond);

            var funcNode =
                AstFactory.BuildFunctionCall(new Func<int, int, int, int, int, int, int, System.DateTime>(DSCore.DateTime.ByDateAndTime),
                    new List<AssociativeNode>
                    {
                        yearNode,
                        monthNode,
                        dayNode,
                        hourNode,
                        minuteNode,
                        secondNode,
                        msNode,
                    });

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)
            };
        }

        protected override System.DateTime DeserializeValue(string val)
        {
            System.DateTime result;
            result = System.DateTime.TryParseExact(val, PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ?
                result : PreferenceSettings.DynamoDefaultTime;
            return System.DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }

        protected override string SerializeValue()
        {
            return Value.ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture);
        }
    }
}

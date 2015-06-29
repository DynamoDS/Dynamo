using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Date Time")]
    [NodeDescription("Create a DateTime object by selecting a date and time.")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [IsDesignScriptCompatible]
    public class DateTime : BasicInteractive<System.DateTime>
    {
        public DateTime()
        {
            Value = System.DateTime.Now;
            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
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
                        msNode
                    });

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)
            };
        }

        protected override System.DateTime DeserializeValue(string val)
        {
            System.DateTime result;
            return System.DateTime.TryParse(val, out result) ? result : new System.DateTime();
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

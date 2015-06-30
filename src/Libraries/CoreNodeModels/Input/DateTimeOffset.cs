using System;
using System.Collections.Generic;
using System.Globalization;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Date Time Offset")]
    [NodeDescription("Create a DateTimeOffset object by specifying a date, a time, and an offset from UTC.")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [IsDesignScriptCompatible]
    public class DateTimeOffset : BasicInteractive<System.DateTimeOffset>
    {
        private const string format = "dd MMMM yyyy h:mm tt zzz";

        public DateTimeOffset()
        {
            Value = System.DateTimeOffset.Now;
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
            var offHoursNode = AstFactory.BuildIntNode(Value.Offset.Hours);
            var offMinutesNodes = AstFactory.BuildIntNode(Value.Offset.Minutes);

            var funcNode =
                AstFactory.BuildFunctionCall(new Func<int, int, int, int, int, int, int, int, int, System.DateTimeOffset>(DSCore.DateTimeOffset.ByDateTimeOffset),
                    new List<AssociativeNode>
                    {
                        yearNode,
                        monthNode,
                        dayNode,
                        hourNode,
                        minuteNode,
                        secondNode,
                        msNode,
                        offHoursNode,
                        offMinutesNodes
                    });

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode)
            };
        }

        protected override System.DateTimeOffset DeserializeValue(string val)
        {
            System.DateTimeOffset result;
            return System.DateTimeOffset.TryParseExact(val, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result) ? result : new System.DateTimeOffset();
        }

        protected override string SerializeValue()
        {
            return Value.ToString(format,CultureInfo.InvariantCulture);
        }
    }
}

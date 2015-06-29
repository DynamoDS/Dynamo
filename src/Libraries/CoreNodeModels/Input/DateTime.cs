using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Date Time")]
    [NodeDescription("Create a DateTime object by selecting a date and time.")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [IsDesignScriptCompatible]
    public class DateTime : NodeModel
    {
        private System.DateTime dateTimeCore;

        public System.DateTime DateTimeCore
        {
            get
            {
                return dateTimeCore;
            }
            set
            {
                dateTimeCore = value; 
                OnNodeModified();
            }
        }

        public DateTime()
        {
            OutPortData.Add(new PortData("date/time", "A DateTime object."));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var yearNode = AstFactory.BuildIntNode(DateTimeCore.Year);
            var monthNode = AstFactory.BuildIntNode(DateTimeCore.Month);
            var dayNode = AstFactory.BuildIntNode(DateTimeCore.Day);
            var hourNode = AstFactory.BuildIntNode(DateTimeCore.Hour);
            var minuteNode = AstFactory.BuildIntNode(DateTimeCore.Minute);
            var secondNode = AstFactory.BuildIntNode(DateTimeCore.Second);
            var msNode = AstFactory.BuildIntNode(DateTimeCore.Millisecond);

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
    }
}

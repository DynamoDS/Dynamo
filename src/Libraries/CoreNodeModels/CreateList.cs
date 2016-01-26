using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    [NodeName("List.Create")]
    [NodeDescription("ListCreateDescription", typeof(Resources))]
    [NodeSearchTags("ListCreateSearchTags", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.CreateList")]
    public class CreateList : VariableInputNode
    {
        public CreateList()
        {
            InPortData.Add(new PortData("item0", Resources.CreateListPortDataIndex0ToolTip));
            OutPortData.Add(new PortData("list", Resources.CreateListPortDataResultToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputName(int index)
        {
            return "item" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return string.Format(Resources.ListCreateInPortToolTip, index);
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                var connectedInput = Enumerable.Range(0, InPortData.Count)
                                               .Where(HasConnectedInput)
                                               .Select(x => new IntNode(x) as AssociativeNode)
                                               .ToList();

                var paramNumNode = new IntNode(InPortData.Count);
                var positionNode = AstFactory.BuildExprList(connectedInput);
                var arguments = AstFactory.BuildExprList(inputAstNodes);
                var functionNode = new IdentifierListNode
                {
                    LeftNode = new IdentifierNode("DSCore.List"),
                    RightNode = new IdentifierNode("__Create")
                };
                var inputParams = new List<AssociativeNode>
                {
                    functionNode,
                    paramNumNode,
                    positionNode,
                    arguments,
                    AstFactory.BuildBooleanNode(false)
                };

                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionCall("_SingleFunctionObject", inputParams))
                };
            }

            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildExprList(inputAstNodes))
            };
        }
    }
}

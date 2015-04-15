using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using DSCoreNodesUI.Properties;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("List.Create")]
    [NodeDescription("ListCreateDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("ListCreateSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [IsDesignScriptCompatible]
    public class CreateList : VariableInputNode
    {
        public CreateList()
        {
            InPortData.Add(new PortData("index0", Resources.CreateListPortDataIndex0ToolTip));
            OutPortData.Add(new PortData("list", Resources.CreateListPortDataResultToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputName(int index)
        {
            return "index" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "Item Index #" + index;
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
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

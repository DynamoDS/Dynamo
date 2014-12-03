using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Wpf;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public class CreateListNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<CreateList>
    {
        public void CustomizeView(CreateList model, Dynamo.Controls.NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

    [NodeName("List.Create")]
    [NodeDescription("Makes a new list out of the given inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [IsDesignScriptCompatible]
    public class CreateList : VariableInputNode
    {
        public CreateList(WorkspaceModel workspace) : base(workspace)
        {
            InPortData.Add(new PortData("index0", "Item Index #0"));
            OutPortData.Add(new PortData("list", "A list"));

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

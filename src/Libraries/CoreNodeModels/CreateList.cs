using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    [NodeName("List Create")]
    [NodeDescription("ListCreateDescription", typeof(Resources))]
    [NodeSearchTags("ListCreateSearchTags", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.CreateList", "List.Create")]
    public class CreateList : VariableInputNode
    {
        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        private CreateList(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public CreateList()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("item0", Resources.CreateListPortDataIndex0ToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("list", Resources.CreateListPortDataResultToolTip)));

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
            if (InPorts.Count > 1)
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
                var connectedInput = Enumerable.Range(0, InPorts.Count)
                                               .Where(index=>InPorts[index].IsConnected)
                                               .Select(x => new IntNode(x) as AssociativeNode)
                                               .ToList();

                var paramNumNode = new IntNode(InPorts.Count);
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
                        AstFactory.BuildFunctionCall("__CreateFunctionObject", inputParams))
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

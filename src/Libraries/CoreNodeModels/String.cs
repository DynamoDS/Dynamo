using System.Collections.Generic;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    /// <summary>
    /// Base class to represent a single input string node. It supports 
    /// partiallied applied function. 
    /// </summary>
    public class ToStringNodeBase : NodeModel
    {
        public ToStringNodeBase(string functionName)
        {
            this.functionName = functionName;
        }

        private string functionName;

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode rhs = null;

            if (IsPartiallyApplied)
            {
                var connectedInputs = new List<AssociativeNode>();
                var functionNode = new IdentifierNode(functionName);
                var paramNumNode = new IntNode(1);
                var positionNode = AstFactory.BuildExprList(connectedInputs);
                var arguments = AstFactory.BuildExprList(inputAstNodes);
                var inputParams = new List<AssociativeNode>
                {
                    functionNode,
                    paramNumNode,
                    positionNode,
                    arguments,
                    AstFactory.BuildBooleanNode(true)
                };

                rhs = AstFactory.BuildFunctionCall("_SingleFunctionObject", inputParams);
            }
            else
            {
                rhs = AstFactory.BuildFunctionCall(functionName, inputAstNodes);
            }

            return new[]
            {
               AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs)
            };
        }
    }

    [NodeName("String from Object")]
    [NodeDescription("StringfromObjectDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromObject", "DSCoreNodesUI.FromObject")]
    public class FromObject: ToStringNodeBase 
    {
        public FromObject() : base("__ToStringFromObject")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("obj", Resources.FromObjectPortDataObjToolTip));
            OutPortData.Add(new PortData("str", Resources.FormulaPortDataResultToolTip));
            RegisterAllPorts();
        }
    }

    [NodeName("String from Array")]
    [NodeDescription("StringfromArrayDescription", typeof(Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromArraySearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.StringNodes.FromArray", "DSCoreNodesUI.FromArray")]
    public class FromArray : ToStringNodeBase 
    {
        public FromArray() : base("__ToStringFromArray")
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("arr", Resources.FromArrayPortDataArrayToolTip));
            OutPortData.Add(new PortData("str", Resources.FromArrayPortDataResultToolTip));
            RegisterAllPorts();
        }
    }
}

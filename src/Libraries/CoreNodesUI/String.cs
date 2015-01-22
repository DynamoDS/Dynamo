using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;

using DSCoreNodesUI.Properties;

namespace DSCoreNodesUI.StringNodes
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
    [NodeDescription("StringfromObjectDescription", typeof(Properties.Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromObjectSearchTags", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
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
    [NodeDescription("StringfromArrayDescription", typeof(Properties.Resources))]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("FromArraySearchTags", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
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

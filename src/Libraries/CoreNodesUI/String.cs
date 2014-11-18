using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodesUI.StringNodes
{
    /// <summary>
    /// Base class to represent a single input string node. It supports 
    /// partiallied applied function. 
    /// </summary>
    public class ToStringNodeBase : NodeModel
    {
        public ToStringNodeBase(WorkspaceModel workspace, string functionName)
            : base(workspace)
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

    [NodeName("FromObject")]
    [NodeDescription("Convert an object to a string representation.\n\n__ToStringFromObject(obj:var):string")]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("string.fromobject", "tostring", "2string", "number2string", "numbertostring")]
    [IsDesignScriptCompatible]
    public class FromObject: ToStringNodeBase 
    {
        public FromObject(WorkspaceModel workspace)
            : base(workspace, "__ToStringFromObject")
        {
            NickName = "String.FromObject";
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("obj", "Object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the object"));
            RegisterAllPorts();
        }
    }

    [NodeName("FromArray")]
    [NodeDescription("Convert an array to a string representation.\n\n__ToStringFromArray(arr:var[]..[]):string")]
    [NodeCategory("Core.String.Actions")]
    [NodeSearchTags("string.fromarray", "tostring", "2string", "list2string", "listtostring", "array2string", "arraytostring")]
    [IsDesignScriptCompatible]
    public class FromArray : ToStringNodeBase 
    {
        public FromArray(WorkspaceModel workspace)
            : base(workspace, "__ToStringFromArray")
        {
            NickName = "String.FromArray";
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("arr", "The array of object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the array"));
            RegisterAllPorts();
        }
    }
}

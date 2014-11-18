using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodesUI.StringNodes
{
    [NodeName("FromObject")]
    [NodeDescription("Convert an object to a string representation.\n\n__ToStringFromObject(obj:var):string")]
    [NodeCategory("Core.String.Actions")]
    [IsDesignScriptCompatible]
    public class FromObject: NodeModel
    {
        public FromObject(WorkspaceModel workspace)
            : base(workspace)
        {
            NickName = "String.FromObject";
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("obj", "Object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the object"));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("__ToStringFromObject", inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }

    [NodeName("FromArray")]
    [NodeDescription("Convert an array to a string representation.\n\n__ToStringFromArray(arr:var[]..[]):string")]
    [NodeCategory("Core.String.Actions")]
    [IsDesignScriptCompatible]
    public class FromArray : NodeModel
    {
        public FromArray(WorkspaceModel workspace)
            : base(workspace)
        {
            NickName = "String.FromArray";
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("arr", "The array of object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the array"));
            RegisterAllPorts();
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("__ToStringFromArray", inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }
}

using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodesUI
{
    [NodeName("String.FromObject")]
    [NodeDescription("Convert an object to a string representation")]
    [NodeCategory("Core.String")]
    [IsDesignScriptCompatible]
    public class StringFromObject: NodeModel
    {
        public StringFromObject(WorkspaceModel workspace) : base(workspace)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("obj", "Object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the object"));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("__ToStringForObject", inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }

    [NodeName("String.FromArray")]
    [NodeDescription("Convert an array to a string representation")]
    [NodeCategory("Core.String")]
    [IsDesignScriptCompatible]
    public class StringFromArray : NodeModel
    {
        public StringFromArray(WorkspaceModel workspace) : base(workspace)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            InPortData.Add(new PortData("arr", "The array of object to be serialized"));
            OutPortData.Add(new PortData("str", "String representation of the array"));
            RegisterAllPorts();
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("__ToStringForArray", inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }
}

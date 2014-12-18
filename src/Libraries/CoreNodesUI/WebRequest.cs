using System;
using System.Collections.Generic;
using DSCore;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Web Request")]
    [NodeDescription("Make a web request given a url.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    [IsDesignScriptCompatible]
    public class WebRequest : NodeModel
    {
        public override bool ForceReExecuteOfNode
        {
            get
            {
                return true;
            }
        }

        public WebRequest()
        {
            InPortData.Add(new PortData("url", "The url for the web request."));
            OutPortData.Add(new PortData("result", "The result of the web request."));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            OnAstUpdated();

            var functionCall = AstFactory.BuildFunctionCall(new Func<string, string>(Web.WebRequestByUrl), inputAstNodes);

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)};
        }
    }
}

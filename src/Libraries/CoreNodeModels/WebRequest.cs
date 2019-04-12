using System;
using System.Collections.Generic;
using CoreNodeModels.Properties;
using DSCore;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    [NodeName("Web Request")]
    [NodeDescription("WebRequestDescription", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_WEB)]
    [IsDesignScriptCompatible]
    [OutPortTypes("object")]
    [AlsoKnownAs("DSCoreNodesUI.WebRequest")]
    public class WebRequest : NodeModel
    {
        [JsonConstructor]
        private WebRequest(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            CanUpdatePeriodically = true;
        }

        public WebRequest()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("url", Resources.WebRequestPortDataUrlToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("result", Resources.WebRequestPortDataResultToolTip)));
            RegisterAllPorts();

            CanUpdatePeriodically = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall(new Func<string, string>(Web.WebRequestByUrl), inputAstNodes);

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)};
        }
    }
}

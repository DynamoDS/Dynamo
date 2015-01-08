using System;
using System.Collections.Generic;
using DSCore;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using DSCoreNodesUI.Properties;

namespace DSCoreNodesUI
{
    [NodeName(/*NXLT*/"Web Request")]
    [NodeDescription(/*NXLT*/"WebRequestDescription", typeof(Properties.Resources))]
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
            InPortData.Add(new PortData(/*NXLT*/"url", Resources.WebRequestPortDataUrlToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"result", Resources.WebRequestPortDataResultToolTip));
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

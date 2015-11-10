﻿using System;
using System.Collections.Generic;
using DSCore;
using ProtoCore.AST.AssociativeAST;
using DSCoreNodesUI.Properties;
using Dynamo.Graph.Nodes;

namespace DSCoreNodesUI
{
    [NodeName("Web Request")]
    [NodeDescription("WebRequestDescription", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    [IsDesignScriptCompatible]
    public class WebRequest : NodeModel
    {
        public WebRequest()
        {
            InPortData.Add(new PortData("url", Resources.WebRequestPortDataUrlToolTip));
            OutPortData.Add(new PortData("result", Resources.WebRequestPortDataResultToolTip));
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

﻿using System;
using System.Collections.Generic;
using DSCoreNodesUI;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public enum TomDickHarry
    {
        Tom,
        Dick,
        Harry
    };

    [NodeName("Tom Dick and Harry")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Every tom dick and harry.")]
    [IsDesignScriptCompatible]
    public class TomDickHarryList : EnumBase
    {
        /// <summary>
        /// This exmple shows how to extend EnumBase to show an Enum as a 
        /// list of values in a drop-down on a node.
        /// </summary>
        public TomDickHarryList() : base(typeof(TomDickHarry)) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {

            var functionCall = AstFactory.BuildFunctionCall("insanity.SanityCheck",
                                                            "ANumber",
                                                            new List<AssociativeNode>());
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }
}

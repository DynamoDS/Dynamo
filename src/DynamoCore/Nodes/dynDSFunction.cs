using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace Dynamo.Nodes
{
    [NodeName("Function Node")]
    [NodeDescription("DesignScript Builtin Functions")]
    [IsInteractive(false)]
    [NodeHiddenInBrowser] 
    [NodeSearchableAttribute(false)]
    public class DSFunction : NodeModel
    {
        private ProcedureNode procedure;

        public string FunctionName
        {
            get
            {
                return procedure.name;
            }
        }

        public DSFunction(ProcedureNode dsProcedure)
        {
            procedure = dsProcedure;
            foreach (var arg in procedure.argInfoList)
            {
                InPortData.Add(new PortData(arg.Name, "parameter", typeof(object)));
            }
            OutPortData.Add(new PortData("", "Object inspected", typeof(object)));
            RegisterAllPorts();

            NickName = procedure.name;
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputs)
        {
            return builder.Build(this, inputs);
        }
    }
}

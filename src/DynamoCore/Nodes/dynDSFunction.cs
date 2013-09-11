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
    /// <summary>
    /// Description about a DesignScript function. 
    /// 
    /// TODO: More information may be needed. 
    /// </summary>
    public class DSFunctionDescritpion
    {
        private string functionName = string.Empty;
        private string displayName = string.Empty;
        private List<string> argumentNames = new List<string>();

        public string Name
        {
            get { return functionName; }
        }

        public string DisplayName
        {
            get { return displayName;  }
        }
        
        public List<string> ArgumentNames
        {
            get { return argumentNames; }
        }

        public DSFunctionDescritpion(string name, string displayName, List<string> argumentNames)
        {
            this.functionName = name;
            this.displayName = displayName;
            this.argumentNames = argumentNames;
        }
    }

    /// <summary>
    /// DesignScript function node. All functions from DesignScript share the
    /// same function node but internally have different procedure node.
    /// </summary>
    [NodeName("Function Node")]
    [NodeDescription("DesignScript Builtin Functions")]
    [IsInteractive(false)]
    [NodeSearchableAttribute(false)]
    public class DSFunction : NodeModel
    {
        private DSFunctionDescritpion functionData; 

        public string FunctionName
        {
            get
            {
                return functionData.Name;
            }
        }

        public DSFunction(DSFunctionDescritpion functionData)
        {
            this.functionData = functionData;

            foreach (var arg in this.functionData.ArgumentNames)
            {
                InPortData.Add(new PortData(arg, "parameter", typeof(object)));
            }
            OutPortData.Add(new PortData("", "return value", typeof(object)));
            RegisterAllPorts();

            NickName = functionData.DisplayName;
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, 
                                                        List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputs)
        {
            return builder.Build(this, inputs);
        }
    }
}

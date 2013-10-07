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
    public enum DSFunctionType
    {
        GeneralFunction,
        Constructor,
        StaticMethod,
        InstanceMethod,
        StaticProperty,
        InstanceProperty
    }

    /// <summary>
    /// Description about a DesignScript function. 
    /// 
    /// TODO: More information may be needed. 
    /// </summary>
    public class DSFunctionDescritpion
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set;}
        public List<string> ArgumentNames { get; private set;}
        public List<string> ReturnKeys { get; private set;}
        public DSFunctionType Type { get; private set; }

        public DSFunctionDescritpion(string name, 
                                    string displayName, 
                                    List<string> argumentNames, 
                                    List<string> returnKeys = null)
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.ArgumentNames = argumentNames;
            this.ReturnKeys = returnKeys;
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
        public string FunctionName
        {
            get;
            private set;
        }

        public DSFunction(DSFunctionDescritpion functionData)
        {
            this.FunctionName = functionData.Name;
            
            foreach (var arg in functionData.ArgumentNames)
            {
                InPortData.Add(new PortData(arg, "parameter", typeof(object)));
            }

            // Returns a dictionary
            if (functionData.ReturnKeys != null && functionData.ReturnKeys.Count > 1)
            {
                foreach (var key in functionData.ReturnKeys)
                {
                    OutPortData.Add(new PortData(key, "return value", typeof(object)));
                }
            }
            else
            {
                OutPortData.Add(new PortData("", "return value", typeof(object)));
            }

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

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
    public enum DSLibraryItemType
    {
        GenericFunction,
        Constructor,
        StaticMethod,
        InstanceMethod,
        StaticProperty,
        InstanceProperty
    }

    public abstract class DSLibraryItem
    {
        public string Assembly { get; set; }
        public string Category { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public DSLibraryItemType Type { get; set; }

        public DSLibraryItem(string assembly, string category, string className, string name, string displayName, DSLibraryItemType type)
        {
            Assembly = assembly;
            Category = category;
            ClassName = className;
            Name = name;
            DisplayName = displayName;
            Type = type;
        }
    }
    
    public class DSFunctionItem: DSLibraryItem
    {
        public List<string> Arguments { get; set;}
        public List<string> ReturnKeys { get; set;}

        public DSFunctionItem(string assembly,
                                    string category,
                                    string className, 
                                    string name,
                                    string displayName, 
                                    DSLibraryItemType type,
                                    List<string> arguments, 
                                    List<string> returnKeys = null):
            base(assembly, category, className, name, displayName, type)
        {
            this.Arguments = arguments;
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
        public DSFunctionItem functionItem { get; set; }

        public string Assembly
        {
            get
            {
                return functionItem.Assembly;
            }
        }

        public string FunctionName
        {
            get
            {
                if (IsStaticMember() || IsConstructor())
                {
                    return functionItem.ClassName + "." + functionItem.Name;
                }
                else
                {
                    return functionItem.Name;
                }
            }
        }

        public bool IsInstanceMember()
        {
            return functionItem.Type == DSLibraryItemType.InstanceMethod ||
                   functionItem.Type == DSLibraryItemType.InstanceProperty;
        }

        public bool IsStaticMember()
        {
            return functionItem.Type == DSLibraryItemType.StaticMethod ||
                   functionItem.Type == DSLibraryItemType.StaticProperty;
        }

        public bool IsConstructor()
        {
            return functionItem.Type == DSLibraryItemType.Constructor;
        }

        public DSFunction(DSFunctionItem item)
        {
            functionItem = item;

            if (IsInstanceMember())
            {
                InPortData.Add(new PortData("this", "Class Instance", typeof(object)));
            }

            foreach (var arg in functionItem.Arguments)
            {
                InPortData.Add(new PortData(arg, "parameter", typeof(object)));
            }

            // Returns a dictionary
            if (functionItem.ReturnKeys != null && functionItem.ReturnKeys.Count >= 1)
            {
                foreach (var key in functionItem.ReturnKeys)
                {
                    OutPortData.Add(new PortData(key, "return value", typeof(object)));
                }
            }
            else
            {
                OutPortData.Add(new PortData("", "return value", typeof(object)));
            }

            RegisterAllPorts();
            NickName = functionItem.DisplayName;
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, 
                                                        List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputs)
        {
            return builder.Build(this, inputs);
        }
    }
}

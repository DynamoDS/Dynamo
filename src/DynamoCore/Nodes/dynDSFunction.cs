using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
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
        public string QualifiedName
        {
            get
            {
                return string.IsNullOrEmpty(ClassName) ? Name : ClassName + "." + Name;
            }
        }

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
        public DSFunctionItem Definition { get; set; }

        public bool IsInstanceMember()
        {
            return Definition.Type == DSLibraryItemType.InstanceMethod ||
                   Definition.Type == DSLibraryItemType.InstanceProperty;
        }

        public bool IsStaticMember()
        {
            return Definition.Type == DSLibraryItemType.StaticMethod ||
                   Definition.Type == DSLibraryItemType.StaticProperty;
        }

        public bool IsConstructor()
        {
            return Definition.Type == DSLibraryItemType.Constructor;
        }

        public DSFunction(DSFunctionItem item)
        {
            Definition = item;

            if (IsInstanceMember())
            {
                InPortData.Add(new PortData("this", "Class Instance", typeof(object)));
            }

            foreach (var arg in Definition.Arguments)
            {
                InPortData.Add(new PortData(arg, "parameter", typeof(object)));
            }

            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Count >= 1)
            {
                foreach (var key in Definition.ReturnKeys)
                {
                    OutPortData.Add(new PortData(key, "return value", typeof(object)));
                }
            }
            else
            {
                OutPortData.Add(new PortData("", "return value", typeof(object)));
            }

            RegisterAllPorts();
            NickName = Definition.DisplayName;
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, 
                                                        List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputs)
        {
            return builder.Build(this, inputs);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); 
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("name", this.Definition.QualifiedName);
        }
    }
}

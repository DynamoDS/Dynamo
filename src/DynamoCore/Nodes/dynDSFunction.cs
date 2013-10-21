using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace Dynamo.Nodes
{
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

        public DSFunction()
        {

        }

        public DSFunction(DSFunctionItem definition)
        {
            Initialize(definition);
        }

        public override bool RequiresRecalc
        {
            get
            {
                return Inputs.Values.Where(x => x != null).Any(x => x.Item2.isDirty);
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        /// <summary>
        /// Initialize a DS function node.
        /// </summary>
        /// <param name="funcDef"></param>
        private void Initialize(DSFunctionItem funcDef)
        {
            Definition = funcDef;

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

        /// <summary>
        /// Save document will call this method to serialize node to xml data
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeElement"></param>
        /// <param name="context"></param>
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement def = xmlDoc.CreateElement(typeof(DSFunctionItem).FullName);

            def.SetAttribute("Assembly", Definition.Assembly ?? "");
            def.SetAttribute("Category", Definition.Category ?? "");
            def.SetAttribute("ClassName", Definition.ClassName ?? "");
            def.SetAttribute("Name", Definition.Name ?? "");
            def.SetAttribute("DisplayName", Definition.DisplayName ?? "");
            def.SetAttribute("Type", Definition.Type.ToString());
            def.SetAttribute("Arguments", Definition.Arguments == null ? "" : string.Join(";", Definition.Arguments));
            def.SetAttribute("ReturnKeys", Definition.ReturnKeys == null ? "" : string.Join(";", Definition.ReturnKeys));

            nodeElement.AppendChild(def);
        }

        /// <summary>
        /// Open document will call this method to unsearilize xml data to node
        /// </summary>
        /// <param name="nodeElement"></param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlElement subNode in nodeElement.ChildNodes.Cast<XmlElement>().Where(subNode => subNode.Name.Equals(typeof(DSFunctionItem).FullName)))
            {
                XmlElementHelper helper = new XmlElementHelper(subNode);

                var assembly = helper.ReadString("Assembly", "");
                var category = helper.ReadString("Category", "");
                var className = helper.ReadString("ClassName", "");
                var name = helper.ReadString("Name", "");
                var displayName = helper.ReadString("DisplayName");
                var strType = helper.ReadString("Type", DSLibraryItemType.GenericFunction.ToString());
                var type = (DSLibraryItemType)System.Enum.Parse(typeof(DSLibraryItemType), strType);
                var arguments = helper.ReadString("Arguments", "").Split(new char[] { ';' }).ToList();
                var returnKeys = helper.ReadString("Returnkeys", "").Split(new char[] { ';' }).ToList();

                DSFunctionItem item = new DSFunctionItem(assembly, category, className, name, displayName, type, arguments, returnKeys);
                Initialize(item);
            }
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, 
                                                        List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputs)
        {
            return builder.Build(this, inputs);
        }

        /// <summary>
        /// Copy command will call it to serialize this node to xml data.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); 
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("name", this.Definition.QualifiedName);
        }
    }
}

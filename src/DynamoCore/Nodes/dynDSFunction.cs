﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;

namespace Dynamo.Nodes
{ 
    /// <summary>
    /// DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchableAttribute(false)]
    [IsMetaNodeAttribute]
    public class CustomNodeInstance : NodeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomNodeDefinition Definition { get; set; }

        public override string Description
        {
            get { return Definition.WorkspaceModel.Description; }
            set
            {
                Definition.WorkspaceModel.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        public override bool RequiresRecalc
        {
            get
            {
                return Inputs.Values.Where(x => x != null).Any(x => x.Item2.isDirty || x.Item2.RequiresRecalc);
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        public CustomNodeInstance() { }

        public CustomNodeInstance(CustomNodeDefinition definition)
        {
            Definition = definition;
            ResyncWithDefinition();
        }

        /// <summary>
        /// Updates this Custom Node's data to match its Definition.
        /// </summary>
        public void ResyncWithDefinition()
        {
            if (Definition.Parameters != null)
            {
                foreach (var arg in Definition.Parameters)
                {
                    InPortData.Add(new PortData(arg, "parameter", typeof(object)));
                }
            }

            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
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

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            throw new NotImplementedException();
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall(Definition.Name, inputAstNodes);

            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall)
            };

            if (OutPortData.Count == 1)
            {
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0), AstIdentifierForPreview));
            }
            else
            {
                resultAst.AddRange(
                    Definition.ReturnKeys != null
                        ? Definition.ReturnKeys.Select(
                            rtnKey =>
                                new IdentifierNode(AstIdentifierForPreview)
                                {
                                    ArrayDimensions =
                                        new ArrayNode { Expr = new StringNode { value = rtnKey } }
                                })
                        : Enumerable.Repeat(AstIdentifierForPreview, OutPortData.Count));
            }

            return resultAst;
        }
    }

    /// <summary>
    /// DesignScript function node. All functions from DesignScript share the
    /// same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node")]
    [NodeDescription("DesignScript Builtin Functions")]
    [IsInteractive(false)]
    [NodeSearchableAttribute(false)]
    [IsMetaNodeAttribute]
    public class DSFunction : NodeModel
    {
        public FunctionItem Definition { get; set; }

        public bool IsInstanceMember()
        {
            return Definition.Type == LibraryItemType.InstanceMethod ||
                   Definition.Type == LibraryItemType.InstanceProperty;
        }

        public bool IsStaticMember()
        {
            return Definition.Type == LibraryItemType.StaticMethod ||
                   Definition.Type == LibraryItemType.StaticProperty;
        }

        public bool IsConstructor()
        {
            return Definition.Type == LibraryItemType.Constructor;
        }

        public DSFunction() { }

        public DSFunction(FunctionItem definition)
        {
            Definition = definition;
            Initialize();
        }

        public override bool IsConvertible
        {
            get
            {
                return true;
            }
        }

        public override bool RequiresRecalc
        {
            get
            {
                return Inputs.Values.Where(x => x != null).Any(x => x.Item2.isDirty || x.Item2.RequiresRecalc);
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        /// <summary>
        /// Initialize a DS function node.
        /// </summary>
        private void Initialize()
        {
            if (IsInstanceMember())
            {
                InPortData.Add(new PortData("this", "Class Instance", typeof(object)));
            }

            if (Definition.Parameters != null)
            {
                foreach (var arg in Definition.Parameters)
                {
                    InPortData.Add(new PortData(arg, "parameter", typeof(object)));
                }
            }

            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
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
            XmlElement def = xmlDoc.CreateElement(typeof(FunctionItem).FullName);

            def.SetAttribute("Assembly", Definition.Assembly ?? "");
            def.SetAttribute("Category", Definition.Category ?? "");
            def.SetAttribute("ClassName", Definition.ClassName ?? "");
            def.SetAttribute("Name", Definition.Name ?? "");
            def.SetAttribute("DisplayName", Definition.DisplayName ?? "");
            def.SetAttribute("Type", Definition.Type.ToString());
            def.SetAttribute("Parameters", Definition.Parameters == null ? "" : string.Join(";", Definition.Parameters));
            def.SetAttribute("ReturnKeys", Definition.ReturnKeys == null ? "" : string.Join(";", Definition.ReturnKeys));

            nodeElement.AppendChild(def);
        }

        /// <summary>
        /// Open document will call this method to unsearilize xml data to node
        /// </summary>
        /// <param name="nodeElement"></param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            // In copy/paste, no need to recreate function defintion
            if (Definition != null)
            {
                return;
            }

            foreach (XmlElement subNode in nodeElement.ChildNodes.Cast<XmlElement>().Where(subNode => subNode.Name.Equals(typeof(FunctionItem).FullName)))
            {
                var helper = new XmlElementHelper(subNode);

                var assembly = helper.ReadString("Assembly", "");
                var category = helper.ReadString("Category", "");
                var className = helper.ReadString("ClassName", "");
                var name = helper.ReadString("Name", "");
                var displayName = helper.ReadString("DisplayName");
                var strType = helper.ReadString("Type", LibraryItemType.GenericFunction.ToString());
                var type = (LibraryItemType)System.Enum.Parse(typeof(LibraryItemType), strType);

                List<string> arguments = null;
                var argumentValue = helper.ReadString("Parameters", null);
                if (argumentValue != null)
                {
                    argumentValue = argumentValue.Trim();
                }
                if (!string.IsNullOrEmpty(argumentValue))
                {
                    arguments = argumentValue.Split(new[] { ';' }).ToList();
                }

                List<string> returnKeys = null;
                var returnKeyValue = helper.ReadString("ReturnKeys", null);
                if (returnKeyValue != null)
                {
                    returnKeyValue = returnKeyValue.Trim();
                }
                if (!string.IsNullOrEmpty(returnKeyValue))
                {
                    returnKeys = returnKeyValue.Split(new[] { ';' }).ToList();
                }

                if (!string.IsNullOrEmpty(assembly))
                {
                    dynSettings.Controller.EngineController.ImportLibraries(new List<string> { assembly });
                }

                Definition = new FunctionItem(assembly, category, className, name, displayName, type, arguments, returnKeys);
                Initialize();

                return;
            }
        }

        /// <summary>
        /// Copy command will call it to serialize this node to xml data.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); 
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("name", Definition.DisplayName);
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            string function = Definition.Name;
            AssociativeNode rhs = null;

            switch (Definition.Type)
            {
                case LibraryItemType.Constructor:
                case LibraryItemType.StaticMethod:

                    var staticCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
                    staticCall.LeftNode = new IdentifierNode(Definition.ClassName);
                    staticCall.RightNode = AstFactory.BuildFunctionCall(function, inputAstNodes);
                    rhs = staticCall;
                    break;

                case LibraryItemType.StaticProperty:

                    var staticProp = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
                    staticProp.LeftNode = new IdentifierNode(Definition.ClassName);
                    staticProp.RightNode = new IdentifierNode(Definition.Name);
                    rhs = staticProp;
                    break;

                case LibraryItemType.InstanceProperty:

                    // Only handle getter here. Setter could be handled in CBN.
                    rhs = new NullNode();
                    if (inputAstNodes != null && inputAstNodes.Count >= 1)
                    {
                        var thisNode = inputAstNodes[0];
                        if (thisNode != null && !(thisNode is NullNode))
                        {
                            var insProp = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
                            insProp.LeftNode = inputAstNodes[0];
                            insProp.RightNode = new IdentifierNode(Definition.Name);
                            rhs = insProp;
                        }
                    }

                    break;

                case LibraryItemType.InstanceMethod:

                    rhs = new NullNode();
                    if (inputAstNodes != null && inputAstNodes.Count >= 1)
                    {
                        var thisNode = inputAstNodes[0];
                        inputAstNodes.RemoveAt(0);  // remove this pointer

                        if (thisNode != null && !(thisNode is NullNode))
                        {
                            var memberFunc = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
                            memberFunc.LeftNode = thisNode;
                            memberFunc.RightNode = AstFactory.BuildFunctionCall(function, inputAstNodes);
                            rhs = memberFunc;
                        }
                    }
                    
                    break;

                default:
                    rhs = AstFactory.BuildFunctionCall(function, inputAstNodes);
                    break;
            }

            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, rhs)
            };

            if (OutPortData.Count == 1)
            {
                var outputIdentiferNode = GetAstIdentifierForOutputIndex(0);
                string outputIdentifier = GraphToDSCompiler.GraphUtilities.ASTListToCode(new List<AssociativeNode> { outputIdentiferNode });
                string thisIdentifier = GraphToDSCompiler.GraphUtilities.ASTListToCode(new List<AssociativeNode> { AstIdentifierForPreview});
                if (!string.Equals(outputIdentifier, thisIdentifier))
                {
                    resultAst.Add(AstFactory.BuildAssignment(outputIdentiferNode, AstIdentifierForPreview));
                }
            }
            else
            {
                var undefinedOutputs = Definition.ReturnKeys == null || Definition.ReturnKeys.Count == 0;

                resultAst.AddRange(
                    Enumerable.Range(0, OutPortData.Count)
                              .Select(
                                  outputIdx =>
                                      undefinedOutputs
                                          ? AstIdentifierForPreview
                                          : new IdentifierNode(AstIdentifierForPreview)
                                          {
                                              ArrayDimensions =
                                                  new ArrayNode
                                                  {
                                                      Expr = new StringNode { value = Definition.ReturnKeys[outputIdx] }
                                                  }
                                          }));
            }

            return resultAst;
        }
    }
}

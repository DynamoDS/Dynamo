using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;
using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;

namespace Dynamo.Nodes
{
    /// <summary>
    /// DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchable(false)]
    [IsMetaNode]
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
                return
                    Inputs.Values.Where(x => x != null)
                          .Any(x => x.Item2.isDirty || x.Item2.RequiresRecalc);
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

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
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
            { AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall) };

            if (OutPortData.Count == 1)
            {
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstIdentifierForPreview));
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
    [NodeHiddenInBrowser]
    [NodeSearchable(false)]
    [IsMetaNode]
    public class DSFunction : NodeModel
    {
        public FunctionDescriptor Definition { get; set; }

        public bool IsInstanceMember()
        {
            return Definition.Type == FunctionType.InstanceMethod
                || Definition.Type == FunctionType.InstanceProperty;
        }

        public bool IsStaticMember()
        {
            return Definition.Type == FunctionType.StaticMethod
                || Definition.Type == FunctionType.StaticProperty;
        }

        public bool IsConstructor()
        {
            return Definition.Type == FunctionType.Constructor;
        }

        public DSFunction() { }

        public DSFunction(FunctionDescriptor definition)
        {
            Definition = definition;
            Initialize();
        }

        public override string Description
        {
            get
            {
                return Definition.Signature;
            }
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
                return
                    Inputs.Values.Where(x => x != null)
                          .Any(x => x.Item2.isDirty || x.Item2.RequiresRecalc);
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
                InPortData.Add(new PortData("this", Definition.ClassName, typeof(object)));
            }

            if (Definition.Parameters != null)
            {
                foreach (var arg in Definition.Parameters)
                {
                    InPortData.Add(
                        new PortData(
                            arg.Parameter,
                            string.IsNullOrEmpty(arg.Type) ? "var" : arg.Type,
                            typeof(object)));
                }
            }

            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                foreach (var key in Definition.ReturnKeys)
                {
                    OutPortData.Add(new PortData(key, "var", typeof(object)));
                }
            }
            else
            {
                string returnType = IsConstructor() ? Definition.ClassName : Definition.ReturnType;
                OutPortData.Add(new PortData(">", returnType, typeof(object)));
            }

            RegisterAllPorts();
            NickName = Definition.QualifiedName;
        }

        /// <summary>
        /// Save document will call this method to serialize node to xml data
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeElement"></param>
        /// <param name="context"></param>
        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("assembly", Definition.Assembly ?? "");
            nodeElement.SetAttribute("function", Definition.MangledName ?? "");
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

            string assembly = null;
            string function;

            if (nodeElement.Attributes["assembly"] == null
                && nodeElement.Attributes["function"] == null)
            {
                // To open old file
                foreach (
                    XmlElement subNode in
                        nodeElement.ChildNodes.Cast<XmlElement>()
                                   .Where(
                                       subNode =>
                                           subNode.Name.Equals(typeof(FunctionDescriptor).FullName))
                    )
                {
                    var helper = new XmlElementHelper(subNode);
                    assembly = helper.ReadString("Assembly", "");
                    break;
                }
                function = nodeElement.Attributes["nickname"].Value.Replace(".get", ".");
            }
            else
            {
                assembly = nodeElement.Attributes["assembly"].Value;
                function = nodeElement.Attributes["function"].Value;
            }

            if (!string.IsNullOrEmpty(assembly))
            {
                dynSettings.Controller.EngineController.ImportLibrary(assembly);
                Definition = dynSettings.Controller.EngineController.GetFunctionDescriptor(
                    assembly,
                    function);
            }
            else
            {
                Definition = dynSettings.Controller.EngineController.GetFunctionDescriptor(function);
            }

            if (null == Definition)
            {
                throw new Exception("Cannot find function: " + function);
            }

            Initialize();
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
            helper.SetAttribute("name", Definition.MangledName);
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            string function = Definition.Name;
            AssociativeNode rhs;

            switch (Definition.Type)
            {
                case FunctionType.Constructor:
                case FunctionType.StaticMethod:

                    var staticCall = new IdentifierListNode
                    {
                        LeftNode = new IdentifierNode(Definition.ClassName),
                        RightNode = AstFactory.BuildFunctionCall(function, inputAstNodes)
                    };
                    rhs = staticCall;
                    break;

                case FunctionType.StaticProperty:

                    var staticProp = new IdentifierListNode
                    {
                        LeftNode = new IdentifierNode(Definition.ClassName),
                        RightNode = new IdentifierNode(Definition.Name)
                    };
                    rhs = staticProp;
                    break;

                case FunctionType.InstanceProperty:

                    // Only handle getter here. Setter could be handled in CBN.
                    rhs = new NullNode();
                    if (inputAstNodes != null && inputAstNodes.Count >= 1)
                    {
                        var thisNode = inputAstNodes[0];
                        if (thisNode != null && !(thisNode is NullNode))
                        {
                            var insProp = new IdentifierListNode
                            {
                                LeftNode = inputAstNodes[0],
                                RightNode = new IdentifierNode(Definition.Name)
                            };
                            rhs = insProp;
                        }
                    }

                    break;

                case FunctionType.InstanceMethod:

                    rhs = new NullNode();
                    if (inputAstNodes != null && inputAstNodes.Count >= 1)
                    {
                        var thisNode = inputAstNodes[0];
                        inputAstNodes.RemoveAt(0); // remove this pointer

                        if (thisNode != null && !(thisNode is NullNode))
                        {
                            var memberFunc = new IdentifierListNode
                            {
                                LeftNode = thisNode,
                                RightNode = AstFactory.BuildFunctionCall(function, inputAstNodes)
                            };
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
                string outputIdentifier = outputIdentiferNode.ToString();
                string thisIdentifier = AstIdentifierForPreview.ToString();
                if (!string.Equals(outputIdentifier, thisIdentifier))
                {
                    resultAst.Add(
                        AstFactory.BuildAssignment(outputIdentiferNode, AstIdentifierForPreview));
                }
            }
            else
            {
                var undefinedOutputs = Definition.ReturnKeys == null || !Definition.ReturnKeys.Any();

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
                                                      Expr =
                                                          new StringNode
                                                          {
                                                              value =
                                                                  Definition.ReturnKeys.ElementAt(
                                                                      outputIdx)
                                                          }
                                                  }
                                          }));
            }

            return resultAst;
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public class ZeroTouchNodeController : FunctionCallNodeController
    {
        public ZeroTouchNodeController(FunctionDescriptor zeroTouchDef) : base(zeroTouchDef) { }

        public new FunctionDescriptor Definition
        {
            get { return base.Definition as FunctionDescriptor; }
            set { base.Definition = value; }
        }

        public string Description { get { return Definition.Description; } }
        public string Category { get { return Definition.Category; } }
        public string MangledName { get { return Definition.MangledName; } }

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

        protected override void InitializeInputs(NodeModel model)
        {
            if (IsInstanceMember())
            {
                string varname = Definition.ClassName.Split('.').Last();
                varname = char.ToLowerInvariant(varname[0]) + varname.Substring(1);
                model.InPortData.Add(new PortData(varname, Definition.ClassName));
            }

            if (Definition.Parameters != null)
            {
                InitializeFunctionParameters(model, Definition.Parameters);
            }
        }

        protected virtual void InitializeFunctionParameters(NodeModel model, IEnumerable<TypedParameter> parameters)
        {
            foreach (var arg in parameters)
                model.InPortData.Add(new PortData(arg.Name, arg.Description, arg.DefaultValue));
        }

        protected override void InitializeOutputs(NodeModel model)
        {
            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                foreach (var key in Definition.ReturnKeys)
                {
                    model.OutPortData.Add(new PortData(key, "var"));
                }
            }
            else
            {
                string displayReturnType = IsConstructor()
                    ? Definition.UnqualifedClassName
                    : Definition.ReturnType;
                model.OutPortData.Add(new PortData(displayReturnType, displayReturnType));
            }
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            var asmPath = Definition.Assembly ?? "";

            if (context == SaveContext.File)
            {
                // We only make relative paths in a file saving operation.
                var docPath = Utilities.GetDocumentXmlPath(xmlDoc);
                asmPath = Utilities.MakeRelativePath(docPath, asmPath);
            }

            nodeElement.SetAttribute("assembly", asmPath);
            nodeElement.SetAttribute("function", Definition.MangledName ?? "");
        }

        public override void LoadNode(XmlNode nodeElement)
        {
            // In copy/paste, no need to recreate function defintion
            if (Definition != null) return;

            string assembly = null;
            string function;

            Trace.Assert(nodeElement.Attributes != null, "nodeElement.Attributes != null");

            if (nodeElement.Attributes["assembly"] == null && nodeElement.Attributes["function"] == null)
            {
                // To open old file
                var helper =
                    nodeElement.ChildNodes.Cast<XmlElement>()
                        .Where(
                            subNode => subNode.Name.Equals(typeof(FunctionDescriptor).FullName))
                        .Select(subNode => new XmlElementHelper(subNode))
                        .FirstOrDefault();

                if (helper != null)
                {
                    assembly = helper.ReadString("Assembly", "");
                }

                function = nodeElement.Attributes["nickname"].Value.Replace(".get", ".");
            }
            else
            {
                var xmlAttribute = nodeElement.Attributes["assembly"];
                if (xmlAttribute != null)
                    assembly = xmlAttribute.Value;
                function = nodeElement.Attributes["function"].Value;
            }

            var engine = dynSettings.Controller.EngineController;

            if (!string.IsNullOrEmpty(assembly))
            {
                var document = nodeElement.OwnerDocument;
                var docPath = Utilities.GetDocumentXmlPath(document);
                assembly = Utilities.MakeAbsolutePath(docPath, assembly);

                engine.ImportLibrary(assembly);
                Definition = engine.GetFunctionDescriptor(assembly, function);
            }
            else
            {
                Definition = engine.GetFunctionDescriptor(function);
            }

            if (null == Definition)
            {
                throw new UnresolvedFunctionException(function);
            }
        }

        public override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("name", Definition.MangledName);
        }

        protected AssociativeNode CreateFunctionObject(
            NodeModel model,
            AssociativeNode functionNode, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildFunctionObject(
                functionNode,
                model.InPorts.Count(),
                model.GetConnectedInputs(),
                inputs);
        }

        protected override AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode rhs;

            string function = Definition.Name;

            switch (Definition.Type)
            {
                case FunctionType.Constructor:
                case FunctionType.StaticMethod:
                    if (model.IsPartiallyApplied)
                    {
                        var functionNode = new IdentifierListNode
                        {
                            LeftNode = new IdentifierNode(Definition.ClassName),
                            RightNode = new IdentifierNode(Definition.Name)
                        };
                        rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
                    }
                    else
                    {
                        model.AppendReplicationGuides(inputAstNodes);
                        rhs = AstFactory.BuildFunctionCall(
                            Definition.ClassName,
                            Definition.Name,
                            inputAstNodes);
                    }
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
                    if (model.IsPartiallyApplied)
                    {
                        var functionNode = new IdentifierListNode
                        {
                            LeftNode = new IdentifierNode(Definition.ClassName),
                            RightNode = new IdentifierNode(Definition.Name)
                        };
                        rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
                    }
                    else
                    {
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
                    }

                    break;

                case FunctionType.InstanceMethod:
                    if (model.IsPartiallyApplied)
                    {
                        var functionNode = new IdentifierListNode
                        {
                            LeftNode = new IdentifierNode(Definition.ClassName),
                            RightNode = new IdentifierNode(Definition.Name)
                        };
                        rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
                    }
                    else
                    {
                        rhs = new NullNode();
                        model.AppendReplicationGuides(inputAstNodes);

                        if (inputAstNodes != null && inputAstNodes.Count >= 1)
                        {
                            var thisNode = inputAstNodes[0];
                            inputAstNodes.RemoveAt(0); // remove this pointer

                            if (thisNode != null && !(thisNode is NullNode))
                            {
                                var memberFunc = new IdentifierListNode
                                {
                                    LeftNode = thisNode,
                                    RightNode =
                                        AstFactory.BuildFunctionCall(function, inputAstNodes)
                                };
                                rhs = memberFunc;
                            }
                        }
                    }

                    break;

                default:
                    if (model.IsPartiallyApplied)
                    {
                        var functionNode = new IdentifierNode(function);
                        rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
                    }
                    else
                    {
                        model.AppendReplicationGuides(inputAstNodes);
                        rhs = AstFactory.BuildFunctionCall(function, inputAstNodes);
                    }
                    break;
            }

            return rhs;
        }
    }
}
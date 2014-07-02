using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public class ZeroTouchFunctionCallData : FunctionCallData
    {
        public void LoadNode(NodeModel model, XmlNode nodeElement)
        {
            // In copy/paste, no need to recreate function defintion
            if (Definition == null)
            {
                string assembly = null;
                string function;

                if (nodeElement.Attributes["assembly"] == null
                    && nodeElement.Attributes["function"] == null)
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
                    assembly = nodeElement.Attributes["assembly"].Value;
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

                InitializeNode(model);
            }

            Load(nodeElement);
        }

        protected virtual void Load(XmlNode nodeElement)
        {
            
        }

        public ZeroTouchFunctionCallData(FunctionDescriptor zeroTouchDef) : base(zeroTouchDef)
        {
            Definition = zeroTouchDef;
        }
        
        public new FunctionDescriptor Definition { get; private set; }
        public string Description { get { return Definition.Description; } }
        public string Category { get { return Definition.Category; } }
        public IEnumerable<string> ReturnKeys { get { return Definition.ReturnKeys; } }
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

        protected virtual void InitializeParameters(NodeModel model)
        {
            foreach (var arg in Definition.Parameters)
                model.InPortData.Add(new PortData(arg.Name, arg.Description, arg.DefaultValue));
        }

        public virtual void InitializeNode(NodeModel model)
        {
            if (Definition == null) return;

            if (IsInstanceMember())
            {
                string varname = Definition.ClassName.Split('.').Last();
                varname = char.ToLowerInvariant(varname[0]) + varname.Substring(1);
                model.InPortData.Add(new PortData(varname, Definition.ClassName));
            }

            if (Definition.Parameters != null)
            {
                InitializeParameters(model);
            }

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

            model.RegisterAllPorts();

            model.NickName = Definition.DisplayName;
        }

        public virtual void Save(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
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

        public virtual void SetupNodeUI(dynNodeView view) { }

        public virtual void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("name", Definition.MangledName);
        }

        public virtual void DeserializeCore(XmlElement element, SaveContext context)
        {
        }

        public virtual bool HandleModelEventCore(string eventName)
        {
            return false;
        }

        protected AssociativeNode CreateFunctionObject(NodeModel model,
                                                     AssociativeNode functionNode, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildFunctionObject(
                functionNode,
                model.InPorts.Count(),
                model.GetConnectedInputs(),
                inputs);
        }

        protected virtual void BuildOutputAst(
            NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            string function = Definition.Name;
            AssociativeNode rhs;

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

            if (model.OutPortData.Count == 1)
            {
                resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));

                var outputIdentiferNode = model.GetAstIdentifierForOutputIndex(0);
                string outputIdentifier = outputIdentiferNode.ToString();
                string thisIdentifier = model.AstIdentifierForPreview.ToString();
                if (!string.Equals(outputIdentifier, thisIdentifier))
                {
                    resultAst.Add(
                        AstFactory.BuildAssignment(outputIdentiferNode, model.AstIdentifierForPreview));
                }
            }
            else
            {
                var undefinedOutputs = Definition.ReturnKeys == null || !Definition.ReturnKeys.Any();

                if (undefinedOutputs || !model.IsPartiallyApplied)
                {
                    resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));
                }
                else
                {
                    var missingAmt = Enumerable.Range(0, model.InPortData.Count).Count(x => !model.HasInput(x));
                    var tmp =
                        AstFactory.BuildIdentifier("__partial_" + model.GUID.ToString().Replace('-', '_'));
                    resultAst.Add(AstFactory.BuildAssignment(tmp, rhs));
                    resultAst.AddRange(
                        Definition.ReturnKeys.Select(AstFactory.BuildStringNode)
                            .Select(
                                (rtnKey, index) =>
                                    AstFactory.BuildAssignment(
                                        model.GetAstIdentifierForOutputIndex(index),
                                        AstFactory.BuildFunctionObject(
                                            "__ComposeBuffered",
                                            3,
                                            new[] { 0, 1 },
                                            new List<AssociativeNode>
                                            {
                                                AstFactory.BuildExprList(
                                                    new List<AssociativeNode>
                                                    {
                                                        AstFactory.BuildFunctionObject(
                                                            "__GetOutput",
                                                            2,
                                                            new[] { 1 },
                                                            new List<AssociativeNode>
                                                            {
                                                                AstFactory.BuildNullNode(),
                                                                rtnKey
                                                            }),
                                                        tmp
                                                    }),
                                                AstFactory.BuildIntNode(missingAmt),
                                                AstFactory.BuildNullNode()
                                            }))));
                }
            }
        }


        public IEnumerable<AssociativeNode> BuildAst(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();
            BuildOutputAst(model, inputAstNodes, resultAst);
            return resultAst;
        }
    }
}
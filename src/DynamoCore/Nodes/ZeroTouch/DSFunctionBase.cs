using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Base class for NodeModels representing zero-touch-imported-function calls.
    /// </summary>
    public abstract class DSFunctionBase 
        : FunctionCallBase<ZeroTouchNodeController<FunctionDescriptor>, FunctionDescriptor>
    {
        protected DSFunctionBase(ZeroTouchNodeController<FunctionDescriptor> controller)
            : base(controller)
        {
            ArgumentLacing = LacingStrategy.Shortest;
            Category = Controller.Category;

            if (controller.Definition.IsObsolete)
                Warning(controller.Definition.ObsoleteMessage, true);

            if (controller.Definition.CanUpdatePeriodically)
                CanUpdatePeriodically = true;

            string signature = String.Empty;
            if (Controller.Definition is FunctionDescriptor)
                signature = Controller.Definition.Signature;
            Description = String.IsNullOrEmpty(Controller.Description) ? signature : Controller.Description + "\n\n" + signature;
        }
        
        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return Controller.ReturnKeys != null && Controller.ReturnKeys.Any()
                ? base.GetAstIdentifierForOutputIndex(outputIndex)
                : (OutPortData.Count == 1
                    ? AstIdentifierForPreview
                    : base.GetAstIdentifierForOutputIndex(outputIndex));
        }

        /// <summary>
        ///     Copy command will call it to serialize this node to xml data.
        /// </summary>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            Controller.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            if (Controller.Definition != null) return;
            Controller.SyncNodeWithDefinition(this);
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }
    }
    

    /// <summary>
    ///     Controller that synchronizes a node with a zero-touch function definition.
    /// </summary>
    public class ZeroTouchNodeController<T> : FunctionCallNodeController<T> where T : FunctionDescriptor
    {
        public ZeroTouchNodeController(T zeroTouchDef) : base(zeroTouchDef) { }

        /// <summary>
        ///     Description of function, taken from Definition.
        /// </summary>
        public string Description { get { return Definition.Description; } }

        /// <summary>
        ///     Category of function, taken from Definition.
        /// </summary>
        public string Category { get { return Definition.Category; } }

        /// <summary>
        ///     MangledName of function, taken from Definition.
        /// </summary>
        public string MangledName { get { return Definition.MangledName; } }

        /// <summary>
        ///     Is this function an instance member of a class?
        /// </summary>
        public bool IsInstanceMember()
        {
            return Definition.Type == FunctionType.InstanceMethod
                || Definition.Type == FunctionType.InstanceProperty;
        }

        /// <summary>
        ///     Is this function a static member of a class?
        /// </summary>
        public bool IsStaticMember()
        {
            return Definition.Type == FunctionType.StaticMethod
                || Definition.Type == FunctionType.StaticProperty;
        }

        /// <summary>
        ///     Is this function a constructor of a class?
        /// </summary>
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

        /// <summary>
        ///     Initializes a node's InPortData based on a list of parameters.
        /// </summary>
        /// <param name="model">Node to initialize.</param>
        /// <param name="parameters">Parameters used for initialization.</param>
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
        
        public override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var asmPath = Definition.Assembly ?? "";

            if (context == SaveContext.File)
            {
                // We only make relative paths in a file saving operation.
                var docPath = Utilities.GetDocumentXmlPath(element.OwnerDocument);
                asmPath = Utilities.MakeRelativePath(docPath, asmPath);
            }

            element.SetAttribute("assembly", asmPath);
            element.SetAttribute("function", Definition.MangledName ?? "");
        }

        public override void SyncNodeWithDefinition(NodeModel model)
        {
            base.SyncNodeWithDefinition(model);
            if (Definition != null && Definition.IsObsolete)
                model.Warning(Definition.ObsoleteMessage, true);
        }

        /// <summary>
        ///     Creates a FunctionObject representing a partial application of a function.
        /// </summary>
        /// <param name="model">Node to produce FunctionObject for.</param>
        /// <param name="functionNode">AST representing the function to make a FunctionObject out of.</param>
        /// <param name="inputs">Arguments to be applied partially.</param>
        protected AssociativeNode CreateFunctionObject(
            NodeModel model,
            AssociativeNode functionNode, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildFunctionObject(
                functionNode,
                model.InPorts.Count(),
                Enumerable.Range(0, model.InPorts.Count).Where(model.HasInput),
                inputs);
        }

        protected override AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode rhs;

            string function = Definition.FunctionName;

            switch (Definition.Type)
            {
                case FunctionType.Constructor:
                case FunctionType.StaticMethod:
                    if (model.IsPartiallyApplied)
                    {
                        var functionNode = new IdentifierListNode
                        {
                            LeftNode = new IdentifierNode(Definition.ClassName),
                            RightNode = new IdentifierNode(Definition.FunctionName)
                        };
                        rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
                    }
                    else
                    {
                        model.AppendReplicationGuides(inputAstNodes);
                        rhs = AstFactory.BuildFunctionCall(
                            Definition.ClassName,
                            Definition.FunctionName,
                            inputAstNodes);
                    }
                    break;

                case FunctionType.StaticProperty:

                    var staticProp = new IdentifierListNode
                    {
                        LeftNode = new IdentifierNode(Definition.ClassName),
                        RightNode = new IdentifierNode(Definition.FunctionName)
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
                            RightNode = new IdentifierNode(Definition.FunctionName)
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
                                    RightNode = new IdentifierNode(Definition.FunctionName)
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
                            RightNode = new IdentifierNode(Definition.FunctionName)
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
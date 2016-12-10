using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Library;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    ///     Base class for NodeModels representing zero-touch-imported-function calls.
    /// </summary>
    public abstract class DSFunctionBase 
        : FunctionCallBase<ZeroTouchNodeController<FunctionDescriptor>, FunctionDescriptor>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DSFunctionBase"/> class.
        /// </summary>
        /// <param name="controller">Function descritor.</param>
        protected DSFunctionBase(ZeroTouchNodeController<FunctionDescriptor> controller)
            : base(controller)
        {
            if (controller.Definition.IsLacingDisabled)
            {
                ArgumentLacing = LacingStrategy.Disabled;
            }
            else
            {
                ArgumentLacing = LacingStrategy.Auto;
            }
            Category = Controller.Category;

            if (controller.Definition.IsObsolete)
            {
                Warning(controller.Definition.ObsoleteMessage, true);
            }

            if (controller.Definition.CanUpdatePeriodically)
            {
                CanUpdatePeriodically = true;
            }

            string signature = String.Empty;
            if (Controller.Definition is FunctionDescriptor)
            {
                signature = Controller.Definition.Signature;
            }
            Description = String.IsNullOrEmpty(Controller.Description) ? signature : Controller.Description + "\n\n" + signature;
        }

        /// <summary>
        ///     Indicates if this node is allowed to be converted to AST node in nodes to code conversion.
        /// </summary>
        public override bool IsConvertible
        {
            get { return !IsPartiallyApplied; }
        }

        /// <summary>
        ///     Fetches the ProtoAST Identifier for a given output index.
        /// </summary>
        /// <param name="outputIndex">Index of the output port.</param>
        /// <returns>Identifier corresponding to the given output port.</returns>
        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (Controller.ReturnKeys != null && Controller.ReturnKeys.Any())
            {
                string id = AstIdentifierBase + "_out" + outputIndex;
                return AstFactory.BuildIdentifier(id);
            }
            else
            {
                return base.GetAstIdentifierForOutputIndex(outputIndex); 
            }
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

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }

        /// <summary>
        ///     Returns the possible type of output at specified port.
        /// </summary>
        /// <param name="index">Index of the port</param>
        /// <returns>The type</returns>
        public override ProtoCore.Type GetTypeHintForOutput(int index)
        {
            return Controller.Definition.ReturnType;
        } 
    }
    

    /// <summary>
    ///     Controller that synchronizes a node with a zero-touch function definition.
    /// </summary>
    public class ZeroTouchNodeController<T> : FunctionCallNodeController<T> where T : FunctionDescriptor
    {
        /// <summary>
        ///     Initializes a new instance of 
        /// the <see cref="ZeroTouchNodeController"/> class with FunctionDescriptor.
        /// </summary>
        /// <param name="definition">FunctionDescriptor describing the function 
        /// that this controller will call.</param>
        public ZeroTouchNodeController(T definition) : base(definition) { }

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
                model.InPorts.Add(new PortModel(PortType.Input, model, new PortData(varname, Definition.ClassName)));
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
                model.InPorts.Add(new PortModel(PortType.Input, model, new PortData(arg.Name, arg.Description, arg.DefaultValue)));
        }

        protected override void InitializeOutputs(NodeModel model)
        {
            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                var returns = Definition.Returns.ToList(); // for performance reasons
                var numReturns = returns.Count;

                var i = 0;

                foreach (var key in Definition.ReturnKeys)
                {
                    var portName = i < numReturns
                        ? returns[i].Item1 ?? key // return name is optional
                        : key;

                    var portDesc = i < numReturns
                        ? returns[i].Item2
                        : "var";
                    
                    model.OutPorts.Add(new PortModel(PortType.Output, model, new PortData(portName, portDesc)));
                    i++;
                }
            }
            else
            {
                string displayReturnType = Definition.ReturnType.ToShortString();

                var returns = Definition.Returns;

                if (returns.Any())
                {
                    model.OutPorts.Add(new PortModel(PortType.Output, model, new PortData(
                        returns.ElementAt(0).Item1 ?? displayReturnType,
                        returns.ElementAt(0).Item2 ?? displayReturnType)));
                    return;
                }

                model.OutPorts.Add(new PortModel(PortType.Output, model, new PortData(displayReturnType, displayReturnType)));
            }
        }
        
        /// <summary>
        /// Serializes data into given <see cref="XmlElement"/> object.
        /// </summary>
        /// <param name="element"><see cref="XmlElement"/> object to store data.</param>
        /// <param name="context">Saving context.</param>
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

        /// <summary>
        /// Syncronizes custom node instance with its definition
        /// </summary>
        /// <param name="model">The custom node instance</param>
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
                Enumerable.Range(0, model.InPorts.Count).Where(index=>model.InPorts[index].IsConnected),
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
                        model.UseLevelAndReplicationGuide(inputAstNodes);
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
                        model.UseLevelAndReplicationGuide(inputAstNodes);

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
                        model.UseLevelAndReplicationGuide(inputAstNodes);
                        rhs = AstFactory.BuildFunctionCall(function, inputAstNodes);
                    }
                    break;
            }

            return rhs;
        }
    }
}
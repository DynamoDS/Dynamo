using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Library;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    ///     DesignScript var-arg function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node w/ VarArgs"), NodeDescription("FunctionNodeDescription", typeof(Properties.Resources)),
    IsVisibleInDynamoLibrary(false), IsMetaNode]
    [AlsoKnownAs("Dynamo.Nodes.DSVarArgFunction")]
    public class DSVarArgFunction : DSFunctionBase
    {
        /// <summary>
        /// The function name with required parameters.
        /// </summary>
        public string FunctionSignature
        {
            get
            {
                return Controller.Definition.MangledName;
            }
        }

        /// <summary>
        /// It indicates which of the three types of function calls this node represents, 
        /// a call to an external graph, a call to a function with a vararg argument, 
        /// or a standard function.
        /// </summary>
        public string FunctionType
        {
            get
            {
                return "VariableArgument";
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DSVarArgFunction"/> class.
        /// </summary>
        /// <param name="descriptor">Function descritor.</param>
        public DSVarArgFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchVarArgNodeController<FunctionDescriptor>(descriptor))
        {
            VarInputController = new ZeroTouchVarInputController(this);
            defaultNumInputs = descriptor.Parameters.Count();
        }

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
         {
             get
             {
                 return "FunctionNode";
             }
        }

        /// <summary>
        /// Returns the default number of inputs for the node
        /// </summary>
        private readonly int defaultNumInputs;

        [JsonIgnore]
        internal int DefaultNumInputs { get { return defaultNumInputs; } }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            VarInputController.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            VarInputController.DeserializeCore(nodeElement, context);
        }

        internal override bool HandleModelEventCore(string eventName, int value, UndoRedoRecorder recorder)
        {
            return VarInputController.HandleModelEventCore(eventName, value, recorder)
                || base.HandleModelEventCore(eventName, value, recorder);
        }

        /// <summary>
        ///     Custom VariableInput controller for DSVarArgFunctions.
        /// </summary>
        [JsonIgnore]
        public VariableInputNodeController VarInputController { get; private set; }

        #region VarInput Controller
        private sealed class ZeroTouchVarInputController : VariableInputNodeController
        {
            private readonly ZeroTouchNodeController<FunctionDescriptor> nodeController;

            public ZeroTouchVarInputController(DSFunctionBase model)
                : base(model)
            {
                nodeController = model.Controller;
            }
            /// <summary>
            /// This method is to get the index of the adding Input when we click +
            /// nodeController.Definition.Parameters.Count() will return
            /// the number of inputs the node got by default. For example, String.Join
            /// got separator+string0. when we click +, base.GetInputIndexFromModel() return 2,
            /// (nodeController.Definition.Parameters.Count() -1) return 1. Then the new port will
            /// be string1
            /// </summary>
            /// <returns></returns>
            public override int GetInputIndexFromModel()
            {
                return base.GetInputIndexFromModel() - (nodeController.Definition.Parameters.Count() -1);
            }

            protected override string GetInputName(int index)
            {
                return nodeController.Definition.Parameters.Last().Name.TrimEnd('s') + index;
            }

            protected override string GetInputTooltip(int index)
            {
                var type = nodeController.Definition.Parameters.Last().Type;
                return type.ToShortString();
            }
        }
        #endregion

    }

    /// <summary>
    ///     Controller that extends Zero Touch synchronization with VarArg function compilation.
    /// </summary>
    public class ZeroTouchVarArgNodeController<T> : ZeroTouchNodeController<T>
        where T : FunctionDescriptor
    {
        /// <summary>
        ///     Initializes a new instance of
        /// the <see cref="ZeroTouchVarArgNodeController{T}"/> class with FunctionDescriptor.
        /// </summary>
        /// <param name="zeroTouchDef">FunctionDescriptor describing the function
        /// that this controller will call.</param>
        public ZeroTouchVarArgNodeController(T zeroTouchDef) : base(zeroTouchDef) { }

        protected override void InitializeFunctionParameters(NodeModel model, IEnumerable<TypedParameter> parameters)
        {
            var typedParameters = parameters as IList<TypedParameter> ?? parameters.ToList();
            base.InitializeFunctionParameters(model, typedParameters.Take(typedParameters.Count() - 1));
            if (parameters.Any())
            {
                var arg = parameters.Last();
                var argName = arg.Name.Remove(arg.Name.Length - 1) + "0";
                model.InPorts.Add(new PortModel(PortType.Input, model, new PortData(argName, arg.Description, arg.DefaultValue)));
            }
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            // A variadic zero-touch function foo(x1, ..., xn, params y) is invoked at
            // runtime as foo(x1, ..., xn, {y1, ..., ym}) -- every variadic input is
            // packed into a single array argument. Suppose paramCount == n + 1 and the
            // node inputs are i1, ..., in, y1, ..., ym.
            if (!model.IsPartiallyApplied)
            {
                int variadicStart = Definition.Parameters.Count() - 1;

                // When any port uses levels, per-port replication must be honored. AtLevel /
                // replication-guide annotations are only consumed at function-call argument
                // positions and are inert inside the packed ExprList literal, and DesignScript
                // cannot call a params method with separate arguments. So we route through a
                // generated wrapper whose formal parameters are the individual ports: the
                // per-port AtLevel and the node's lacing replication guides land on the
                // wrapper-call arguments (replicating exactly like a non-variadic node), and
                // the wrapper body packs the already-replicated values per invocation. See
                // DYN-10572. Graphs with no Use Levels keep the original pack-then-call path,
                // so their behavior (including their lacing) is unchanged.
                if (variadicStart >= 0 && variadicStart < inputAstNodes.Count
                    && AnyPortUsesLevels(model, inputAstNodes.Count)
                    && TryBuildReplicatingOutputAst(model, inputAstNodes, resultAst, variadicStart))
                {
                    return;
                }

                // Default path: pack all var arguments into an array {y1, ..., ym}
                // (skipping the first n == paramCount - 1 inputs).
                var argPack = AstFactory.BuildExprList(inputAstNodes.Skip(variadicStart).ToList());
                inputAstNodes = inputAstNodes.Take(variadicStart).ToList();
                inputAstNodes.Add(argPack);
            }

            base.BuildOutputAst(model, inputAstNodes, resultAst);
        }

        /// <summary>
        /// Returns true when any port has Use Levels enabled. Those settings cannot survive
        /// the default pack-then-call path (they are inert inside the packed ExprList), so the
        /// node must route through the per-port replicating wrapper instead.
        /// </summary>
        private static bool AnyPortUsesLevels(NodeModel model, int inputCount)
        {
            for (int i = 0; i < inputCount; i++)
            {
                if (model.InPorts[i].UseLevels)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Emits a generated wrapper function that exposes every port as a separate formal
        /// parameter and packs the variadic tail per invocation, then calls it with per-port
        /// Use Levels and lacing replication guides applied to the individual arguments.
        /// Returns false (leaving <paramref name="resultAst"/> untouched) if the wrapper
        /// source cannot be parsed, so the caller falls back to the default pack path.
        /// </summary>
        private bool TryBuildReplicatingOutputAst(
            NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst, int variadicStart)
        {
            // Build the call to the underlying function exactly as GetFunctionApplication
            // would: Class.Function for member functions, the bare name for globals.
            string callName = Definition.Type == FunctionType.GenericFunction
                ? Definition.FunctionName
                : Definition.ClassName + "." + Definition.FunctionName;

            // Unique, deterministic per node + arity so repeated runs redefine the same
            // wrapper (cache-stable) and distinct nodes never collide.
            string wrapperName = "__vararg_" + model.GUID.ToString("N") + "_" + inputAstNodes.Count;

            var paramNames = Enumerable.Range(0, inputAstNodes.Count).Select(i => "p" + i).ToList();
            var prefixArgs = paramNames.Take(variadicStart);
            var packedArg = "[" + string.Join(", ", paramNames.Skip(variadicStart)) + "]";
            var innerArgs = string.Join(", ", prefixArgs.Append(packedArg));

            string wrapperSource =
                "def " + wrapperName + "(" + string.Join(", ", paramNames) + ")" +
                " { return = " + callName + "(" + innerArgs + "); }";

            FunctionDefinitionNode wrapperDef;
            try
            {
                wrapperDef = ParserUtils.Parse(wrapperSource).Body
                    .OfType<FunctionDefinitionNode>()
                    .FirstOrDefault();
            }
            catch
            {
                return false;
            }

            if (wrapperDef == null)
                return false;

            resultAst.Add(wrapperDef);

            // Per-port AtLevel / lacing replication guides land on the wrapper-call
            // arguments, where they are honored just as on a non-variadic node.
            model.UseLevelAndReplicationGuide(inputAstNodes);

            var rhs = AstFactory.BuildFunctionCall(wrapperName, inputAstNodes);
            AssignIdentifiersForFunctionCall(model, rhs, resultAst);
            return true;
        }
    }
}
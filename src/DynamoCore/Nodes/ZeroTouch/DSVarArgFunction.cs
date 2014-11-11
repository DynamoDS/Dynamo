using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Autodesk.DesignScript.Runtime;
using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     DesignScript var-arg function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node w/ VarArgs"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSVarArgFunction : DSFunctionBase
    {
        public DSVarArgFunction(WorkspaceModel workspaceModel) : this(workspaceModel, null) { }

        public DSVarArgFunction(WorkspaceModel workspaceModel, FunctionDescriptor descriptor)
            : base(workspaceModel, new ZeroTouchVarArgNodeController(workspaceModel.DynamoModel.EngineController, descriptor))
        {
            VarInputController = new ZeroTouchVarInputController(this);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            VarInputController.SaveNode(xmlDoc, nodeElement, context);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            VarInputController.LoadNode(nodeElement);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            VarInputController.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            VarInputController.DeserializeCore(element, context);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            return VarInputController.HandleModelEventCore(eventName)
                || base.HandleModelEventCore(eventName);
        }

        /// <summary>
        ///     Custom VariableInput controller for DSVarArgFunctions.
        /// </summary>
        public VariableInputNodeController VarInputController { get; private set; }

        #region VarInput Controller
        private sealed class ZeroTouchVarInputController : VariableInputNodeController
        {
            private readonly ZeroTouchNodeController nodeController;

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
                return (string.IsNullOrEmpty(type) ? "var" : type);
            }
        }
        #endregion

    }

    /// <summary>
    ///     Controller that extends Zero Touch synchronization with VarArg function compilation.
    /// </summary>
    public class ZeroTouchVarArgNodeController : ZeroTouchNodeController
    {
        public ZeroTouchVarArgNodeController(EngineController engineController,
            FunctionDescriptor zeroTouchDef)
            : base(engineController, zeroTouchDef) { }

        protected override void InitializeFunctionParameters(NodeModel model, IEnumerable<TypedParameter> parameters)
        {
            var typedParameters = parameters as IList<TypedParameter> ?? parameters.ToList();
            base.InitializeFunctionParameters(model, typedParameters.Take(typedParameters.Count() - 1));
            var arg = parameters.LastOrDefault();
            var argName = arg.Name.Remove(arg.Name.Length - 1) + "0";
            model.InPortData.Add(new PortData(argName, arg.Description, arg.DefaultValue));
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            // All inputs are provided, then we should pack all inputs that
            // belong to variable input parameter into a single array. 
            if (!model.IsPartiallyApplied)
            {
                var paramCount = Definition.Parameters.Count();
                var packId = "__var_arg_pack_" + model.GUID;
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(packId),
                        AstFactory.BuildExprList(inputAstNodes.Skip(paramCount - 1).ToList())));

                inputAstNodes =
                    inputAstNodes.Take(paramCount - 1)
                        .Concat(new[] { AstFactory.BuildIdentifier(packId) })
                        .ToList();
            }

            base.BuildOutputAst(model, inputAstNodes, resultAst);
        }
    }
}
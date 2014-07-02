using System.Linq;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.UI;

namespace Dynamo.Nodes
{
    [NodeName("Function Node w/ VarArgs"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSVarArgFunction : DSFunctionBase, IWpfNode
    {
        public DSVarArgFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchVarArgNodeController(descriptor))
        {
            VarInputController = new ZeroTouchVarInputController(this);
        }

        public DSVarArgFunction() : this(null) { }

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

        private sealed class ZeroTouchVarInputController : VariableInputNodeController
        {
            private readonly ZeroTouchNodeController nodeController;

            public ZeroTouchVarInputController(DSFunctionBase model)
                : base(model)
            {
                nodeController = model.Controller;
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

        public VariableInputNodeController VarInputController { get; private set; }
        
        public void SetupCustomUIElements(dynNodeView view)
        {
            VarInputController.SetupNodeUI(view);
        }
    }
}
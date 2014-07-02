using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using Autodesk.DesignScript.Runtime;

namespace Dynamo.Nodes
{
    /// <summary>
    /// DesignScript function node. All functions from DesignScript share the
    /// same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSFunction : DSFunctionBase
    {
        public DSFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchNodeController(descriptor)) { }

        public DSFunction() : this(null) { }
    }

    public abstract class FunctionCallBase : NodeModel
    {
        public FunctionCallNodeController Controller { get; private set; }

        protected FunctionCallBase(FunctionCallNodeController controller)
        {
            Controller = controller;
            Controller.SyncNodeWithDefinition(this);
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return Controller.ReturnKeys != null && Controller.ReturnKeys.Any()
                ? AstFactory.BuildIdentifier(
                    AstIdentifierForPreview.Name,
                    AstFactory.BuildStringNode(
                        Controller.ReturnKeys.ElementAt(outputIndex)))
                : base.GetAstIdentifierForOutputIndex(outputIndex);
        }
    }

    public abstract class DSFunctionBase : FunctionCallBase
    {
        protected DSFunctionBase(ZeroTouchNodeController controller)
            : base(controller)
        {
            ArgumentLacing = LacingStrategy.Shortest;
        }

        public new ZeroTouchNodeController Controller
        {
            get { return base.Controller as ZeroTouchNodeController; }
        }

        public override string Description
        {
            get { return Controller.Description; }
        }

        public override string Category
        {
            get { return Controller.Category; }
        }

        public override bool IsConvertible
        {
            get { return true; }
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
            Controller.SaveNode(xmlDoc, nodeElement, context);
        }

        /// <summary>
        /// Open document will call this method to unsearilize xml data to node
        /// </summary>
        /// <param name="nodeElement"></param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            Controller.LoadNode(nodeElement);
            Controller.SyncNodeWithDefinition(this);
        }

        /// <summary>
        /// Copy command will call it to serialize this node to xml data.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            Controller.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            Controller.DeserializeCore(element, context);
        }

        override internal IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }
    }
}

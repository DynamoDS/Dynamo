using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Base class for NodeModels representing zero-touch-imported-function calls.
    /// </summary>
    public abstract class DSFunctionBase : FunctionCallBase
    {
        protected DSFunctionBase(WorkspaceModel workspaceModel, ZeroTouchNodeController controller)
            : base(workspaceModel, controller)
        {
            ArgumentLacing = LacingStrategy.Shortest;
        }

        /// <summary>
        ///     Controller used to sync node with a zero-touch function definition.
        /// </summary>
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

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return Controller.ReturnKeys != null && Controller.ReturnKeys.Any()
                ? base.GetAstIdentifierForOutputIndex(outputIndex)
                : (OutPortData.Count == 1
                    ? AstIdentifierForPreview
                    : base.GetAstIdentifierForOutputIndex(outputIndex));
        }

        /// <summary>
        ///     Save document will call this method to serialize node to xml data
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
        ///     Open document will call this method to unsearilize xml data to node
        /// </summary>
        /// <param name="nodeElement"></param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            if (Controller.Definition != null) return;

            Controller.LoadNode(nodeElement);
            Controller.SyncNodeWithDefinition(this);
        }

        /// <summary>
        ///     Copy command will call it to serialize this node to xml data.
        /// </summary>
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

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }
    }
}
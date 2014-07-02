using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.UI;

using ProtoCore.AST.AssociativeAST;

using Autodesk.DesignScript.Runtime;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;

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
            : base(new ZeroTouchFunctionCallData(descriptor)) { }

        public DSFunction() : this(null) { }
    }


    public abstract class DSFunctionBase : NodeModel, IWpfNode
    {
        public ZeroTouchFunctionCallData Definition { get; private set; }

        protected DSFunctionBase(ZeroTouchFunctionCallData definition)
        {
            ArgumentLacing = LacingStrategy.Shortest;
            Definition = definition;
            Definition.InitializeNode(this);
        }

        public override string Description
        {
            get { return Definition.Description; }
        }

        public override string Category
        {
            get { return Definition.Category; }
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
            Definition.Save(xmlDoc, nodeElement, context);
        }

        /// <summary>
        /// Open document will call this method to unsearilize xml data to node
        /// </summary>
        /// <param name="nodeElement"></param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            Definition.LoadNode(this, nodeElement);
        }

        /// <summary>
        /// Copy command will call it to serialize this node to xml data.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            Definition.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            Definition.DeserializeCore(element, context);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            return Definition.HandleModelEventCore(eventName)
                || base.HandleModelEventCore(eventName);
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                var indexedValue = new IdentifierNode(AstIdentifierForPreview)
                {
                    ArrayDimensions =
                        new ArrayNode
                        {
                            Expr =
                                new StringNode
                                {
                                    value = Definition.ReturnKeys.ElementAt(outputIndex)
                                }
                        }
                };

                return indexedValue;
            }

            return base.GetAstIdentifierForOutputIndex(outputIndex);
        }

        override internal IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Definition.BuildAst(this, inputAstNodes);
        }

        public void SetupCustomUIElements(dynNodeView view)
        {
            Definition.SetupNodeUI(view);
        }
    }
}

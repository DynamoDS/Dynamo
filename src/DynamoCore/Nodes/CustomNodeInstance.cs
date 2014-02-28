using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    /// DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchable(false)]
    [IsMetaNode]
    public class CustomNodeInstance : NodeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomNodeDefinition Definition { get; set; }

        public override string Description
        {
            get { return Definition.WorkspaceModel.Description; }
            set
            {
                Definition.WorkspaceModel.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        public override bool RequiresRecalc
        {
            get
            {
                return
                    Inputs.Values.Where(x => x != null)
                          .Any(x => x.Item2.isDirty || x.Item2.RequiresRecalc);
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        public CustomNodeInstance() { }

        public CustomNodeInstance(CustomNodeDefinition definition)
        {
            Definition = definition;
            ResyncWithDefinition();
        }

        /// <summary>
        /// Updates this Custom Node's data to match its Definition.
        /// </summary>
        public void ResyncWithDefinition()
        {
            DisableReporting();

            if (Definition.Parameters != null)
            {
                foreach (var arg in Definition.Parameters)
                {
                    InPortData.Add(new PortData(arg, "parameter", typeof(object)));
                }
            }

            // Returns a dictionary
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                foreach (var key in Definition.ReturnKeys)
                {
                    OutPortData.Add(new PortData(key, "return value", typeof(object)));
                }
            }
            else
            {
                OutPortData.Add(new PortData("", "return value", typeof(object)));
            }

            RegisterAllPorts();
            NickName = Definition.DisplayName;

            EnableReporting();
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            throw new NotImplementedException();
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall(Definition.Name, inputAstNodes);

            var resultAst = new List<AssociativeNode>
            { AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall) };

            if (OutPortData.Count == 1)
            {
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstIdentifierForPreview));
            }
            else
            {
                resultAst.AddRange(
                    Definition.ReturnKeys != null
                        ? Definition.ReturnKeys.Select(
                            rtnKey =>
                                new IdentifierNode(AstIdentifierForPreview)
                                {
                                    ArrayDimensions =
                                        new ArrayNode { Expr = new StringNode { value = rtnKey } }
                                })
                        : Enumerable.Repeat(AstIdentifierForPreview, OutPortData.Count));
            }

            return resultAst;
        }
    }
}
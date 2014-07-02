using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public abstract class FunctionCallNodeController
    {
        public IFunctionDescriptor Definition { get; protected set; }

        public string NickName { get { return Definition.DisplayName; } }
        public IEnumerable<string> ReturnKeys { get { return Definition.ReturnKeys; } }

        protected FunctionCallNodeController(IFunctionDescriptor def)
        {
            Definition = def;
        }


        public IEnumerable<AssociativeNode> BuildAst(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();
            BuildOutputAst(model, inputAstNodes, resultAst);
            return resultAst;
        }

        protected virtual void BuildOutputAst(
            NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            AssociativeNode rhs = GetFunctionApplication(model, inputAstNodes);

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

        protected abstract void InitializeInputs(NodeModel model);
        protected abstract void InitializeOutputs(NodeModel model);
        protected abstract AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes);

        public virtual void SaveNode(XmlDocument xmlDocument, XmlElement xmlElement, SaveContext saveContext) { }
        public virtual void LoadNode(XmlNode xmlNode) { }
        public virtual void DeserializeCore(XmlElement element, SaveContext context) { }
        public virtual void SerializeCore(XmlElement element, SaveContext context) { }

        public virtual void SyncNodeWithDefinition(NodeModel model)
        {
            if (Definition == null) return;

            InitializeInputs(model);
            InitializeOutputs(model);
            model.RegisterAllPorts();
            model.NickName = NickName;
        }
    }
}
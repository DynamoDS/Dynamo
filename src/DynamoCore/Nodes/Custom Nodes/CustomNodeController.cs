using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Controller that synchronizes a node with a custom node definition.
    /// </summary>
    public class CustomNodeController<T> : FunctionCallNodeController<T>
        where T : CustomNodeDefinition
    {
        public CustomNodeController(T def) : base(def) { }
        
        protected override void InitializeInputs(NodeModel model)
        {
            model.InPortData.Clear();

            if (Definition.DisplayParameters == null) return;

            foreach (string arg in Definition.DisplayParameters)
                model.InPortData.Add(new PortData(arg, "parameter"));
        }

        protected override void InitializeOutputs(NodeModel model)
        {
            model.OutPortData.Clear();
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                foreach (string key in Definition.ReturnKeys)
                    model.OutPortData.Add(new PortData(key, "return value"));
            }
            else
                model.OutPortData.Add(new PortData("", "return value"));
        }

        protected override AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            if (!model.IsPartiallyApplied)
                return AstFactory.BuildFunctionCall(Definition.FunctionName, inputAstNodes);

            var count = Definition.DisplayParameters.Count();
            return AstFactory.BuildFunctionObject(
                Definition.FunctionName,
                count,
                Enumerable.Range(0, count).Where(model.HasInput),
                inputAstNodes);
        }

        protected override void BuildAstForPartialMultiOutput(
            NodeModel model, AssociativeNode rhs, List<AssociativeNode> resultAst)
        {
            base.BuildAstForPartialMultiOutput(model, rhs, resultAst);

            var emptyList = AstFactory.BuildExprList(new List<AssociativeNode>());
            var previewIdInit = AstFactory.BuildAssignment(model.AstIdentifierForPreview, emptyList);

            resultAst.Add(previewIdInit);
            resultAst.AddRange(
                Definition.ReturnKeys.Select(
                    (rtnKey, idx) =>
                        AstFactory.BuildAssignment(
                            AstFactory.BuildIdentifier(
                                model.AstIdentifierForPreview.Name,
                                AstFactory.BuildStringNode(rtnKey)),
                            model.GetAstIdentifierForOutputIndex(idx))));
        }

        protected override void AssignIdentifiersForFunctionCall(
            NodeModel model, AssociativeNode rhs, List<AssociativeNode> resultAst)
        {
            if (model.OutPortData.Count == 1)
            {
                resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        model.GetAstIdentifierForOutputIndex(0),
                        model.AstIdentifierForPreview));
            }
            else
                base.AssignIdentifiersForFunctionCall(model, rhs, resultAst);
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            if (Definition == null)
            {
                var lhs = model.AstIdentifierForPreview;
                var rhs = AstFactory.BuildNullNode();
                resultAst.Add(AstFactory.BuildAssignment(lhs, rhs));
            }
            else
            {
                base.BuildOutputAst(model, inputAstNodes, resultAst);
            }
        }

        public override void SyncNodeWithDefinition(NodeModel model)
        {
            if (IsInSyncWithNode(model)) 
                return;
            
            base.SyncNodeWithDefinition(model);
            model.OnAstUpdated();
        }

        public override void SerializeCore(XmlElement nodeElement, SaveContext saveContext)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("ID");

            outEl.SetAttribute("value", Definition.FunctionId.ToString());
            nodeElement.AppendChild(outEl);
            nodeElement.SetAttribute("nickname", NickName);
        }

        /// <summary>
        ///   Return if the custom node instance is in sync with its definition.
        ///   It may be out of sync if .dyf file is opened and updated and then
        ///   .dyn file is opened. 
        /// </summary>
        public bool IsInSyncWithNode(NodeModel model)
        {
            return Definition == null || (Definition.DisplayParameters == null
                || (Definition.DisplayParameters.Count() == model.InPortData.Count()
                    && Definition.DisplayParameters.SequenceEqual(
                        model.InPortData.Select(p => p.NickName))))
                && (Definition.ReturnKeys == null
                    || Definition.ReturnKeys.Count() == model.OutPortData.Count()
                        && Definition.ReturnKeys.SequenceEqual(
                            model.OutPortData.Select(p => p.NickName)));
        }
    }
}
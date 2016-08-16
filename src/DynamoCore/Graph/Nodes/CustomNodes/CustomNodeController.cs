using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    /// <summary>
    ///     Controller that synchronizes a custom node with a node definition.
    /// </summary>
    public class CustomNodeController<T> : FunctionCallNodeController<T>
        where T : CustomNodeDefinition
    {
        public CustomNodeController(T def) : base(def) { }
        
        protected override void InitializeInputs(NodeModel model)
        {
            model.InPortData.Clear();

            if (Definition.DisplayParameters == null || Definition.Parameters == null)
                return;

            var inputs = Definition.DisplayParameters.Zip(Definition.Parameters, (dp, p) => Tuple.Create(dp, p.Description, p.DefaultValue));
            foreach (var p in inputs)
                model.InPortData.Add(new PortData(p.Item1, p.Item2, p.Item3));
        }

        protected override void InitializeOutputs(NodeModel model)
        {
            model.OutPortData.Clear();
            if (Definition.Returns.Any())
            {
                foreach (var pair in Definition.Returns)
                {
                    string key = pair.Item1;
                    string tooltip = pair.Item2;

                    if (string.IsNullOrEmpty(tooltip))
                    {
                        tooltip = Properties.Resources.ToolTipReturnValue;
                    }

                    model.OutPortData.Add(new PortData(key, tooltip));
                }
            }
            else
            {
                model.OutPortData.Add(new PortData("", Properties.Resources.ToolTipReturnValue));
            }
        }

        protected override AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            if (!model.IsPartiallyApplied)
            {
                model.UseLevelAndReplicationGuide(inputAstNodes);
                return AstFactory.BuildFunctionCall(Definition.FunctionName, inputAstNodes);
            }

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
                resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));
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

        /// <summary>
        /// Synchronizes custom node with its definition.
        /// </summary>
        /// <param name="model">Custom node model</param>
        public override void SyncNodeWithDefinition(NodeModel model)
        {
            if (IsInSyncWithNode(model)) 
                return;
            
            base.SyncNodeWithDefinition(model);

            model.OnNodeModified();
        }

        /// <summary>
        /// Serializes CustomNode.
        /// </summary>
        /// <param name="nodeElement">Xml node</param>
        /// <param name="saveContext">SaveContext is used in base class.</param>
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
            if (Definition == null)
                return true;

            if (Definition.DisplayParameters != null)
            {
                var paramNames = model.InPortData.Select(p => p.NickName);
                if (!Definition.DisplayParameters.SequenceEqual(paramNames))
                    return false;
            }

            if (Definition.Parameters != null)
            {
                var defParamTypes = Definition.Parameters.Select(p => p.Type.ToShortString());
                var paramTypes = model.InPortData.Select(p => p.ToolTipString);
                if (!defParamTypes.SequenceEqual(paramTypes))
                    return false;
            }

            if (Definition.ReturnKeys != null)
            {
                var returnKeys = model.OutPortData.Select(p => p.NickName);
                if (!Definition.ReturnKeys.SequenceEqual(returnKeys))
                    return false;
            }

            if (Definition.Returns != null)
            {
                var tooltips = model.OutPortData.Select(p => p.ToolTipString);
                if (!Definition.Returns.Select(r => string.IsNullOrEmpty(r.Item2) ? Properties.Resources.ToolTipReturnValue : r.Item2)
                                       .SequenceEqual(tooltips))
                    return false;
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Models;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Controller that synchronizes a node with a custom node definition.
    /// </summary>
    public class CustomNodeController : FunctionCallNodeController<CustomNodeDefinition>
    {
        private bool watchingDefForChanges;

        public CustomNodeController(CustomNodeDefinition def)
            : base(def)
        {
        }

        /// <summary>
        ///     Definition of a custom node.
        /// </summary>
        public new CustomNodeDefinition Definition
        {
            get { return base.Definition; }
            internal set { base.Definition = value; }
        }

        protected override void OnDefinitionChanging()
        {
            if (watchingDefForChanges)
            {
                Definition.Updated -= OnDefinitionChanged;
                watchingDefForChanges = false;
            }
            base.OnDefinitionChanging();
        }

        protected override void OnDefinitionChanged()
        {
            if (!watchingDefForChanges)
            {
                Definition.Updated += OnDefinitionChanged;
                watchingDefForChanges = true;
            }
            base.OnDefinitionChanged();
        }

        protected override void InitializeInputs(NodeModel model)
        {
            model.InPortData.Clear();

            if (Definition.Parameters == null) return;

            foreach (string arg in Definition.Parameters)
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

            var count = Definition.Parameters.Count();
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

        public override void SyncNodeWithDefinition(NodeModel model)
        {
            if (IsInSyncWithNode(model)) 
                return;
            
            model.DisableReporting();
            base.SyncNodeWithDefinition(model);
            model.EnableReporting();
            model.RequiresRecalc = true;
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext saveContext)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("ID");

            outEl.SetAttribute("value", Definition.FunctionId.ToString());
            nodeElement.AppendChild(outEl);
            nodeElement.SetAttribute("nickname", NickName);
        }

        public override void LoadNode(XmlNode nodeElement)
        {
            //TODO(Steve): Handle loading through custom node manager. First load definition, then dispatch to custom node instance for remainder of load.

            XmlNode idNode =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .LastOrDefault(subNode => subNode.Name.Equals("ID"));

            if (idNode == null || idNode.Attributes == null) return;
            
            string id = idNode.Attributes[0].Value;

            string nickname = nodeElement.Attributes["nickname"].Value;
            
            Guid funcId;
            if (!Guid.TryParse(id, out funcId) && nodeElement.Attributes != null)
            {
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nickname);
            }

            if (!VerifyFuncId(ref funcId, nickname))
                LoadProxyCustomNode(funcId, nickname);

            Definition = customNodeManager.GetFunctionDefinition(funcId);
        }

        public override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);

            var helper = new XmlElementHelper(element);
            var nickname = helper.ReadString("functionName");

            Guid funcId;
            if (!Guid.TryParse(helper.ReadString("functionId"), out funcId))
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nickname);

            if (!VerifyFuncId(ref funcId, nickname))
            {
                LoadProxyCustomNode(funcId, nickname);
                return;
            }

            Definition = customNodeManager.GetFunctionDefinition(funcId);
        }

        /// <summary>
        ///   Return if the custom node instance is in sync with its definition.
        ///   It may be out of sync if .dyf file is opened and updated and then
        ///   .dyn file is opened. 
        /// </summary>
        public bool IsInSyncWithNode(NodeModel model)
        {
            return Definition != null
                && ((Definition.Parameters == null
                    || (Definition.Parameters.Count() == model.InPortData.Count()
                        && Definition.Parameters.SequenceEqual(
                            model.InPortData.Select(p => p.NickName))))
                    && (Definition.ReturnKeys == null
                        || Definition.ReturnKeys.Count() == model.OutPortData.Count()
                            && Definition.ReturnKeys.SequenceEqual(
                                model.OutPortData.Select(p => p.NickName))));
        }

        private bool VerifyFuncId(ref Guid funcId, string nickname)
        {
            if (funcId == null) return false;

            // if the dyf does not exist on the search path...
            if (customNodeManager.Contains(funcId))
                return true;

            // if there is a node with this name, use it instead
            if (!customNodeManager.Contains(nickname)) return false;

            funcId = customNodeManager.GetGuidFromName(nickname);
            return true;
        }

        private void LoadProxyCustomNode(Guid funcId, string nickname)
        {
            var proxyDef = new CustomNodeDefinition(funcId)
            {
                //WorkspaceModel =
                //    new CustomNodeWorkspaceModel(nickname, "Custom Nodes") { FileName = null },
                IsProxy = true
            };

            string userMsg = "Failed to load custom node: " + nickname + ".  Replacing with proxy custom node.";

            Log(userMsg);

            // tell custom node loader, but don't provide path, forcing user to resave explicitly
            customNodeManager.SetFunctionDefinition(funcId, proxyDef);
        }
    }
}
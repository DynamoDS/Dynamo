#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

#endregion

namespace Dynamo.Nodes
{
    /// <summary>
    ///     DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchable(false)]
    [IsMetaNode]
    public partial class Function : NodeModel
    {
        protected internal Function(WorkspaceModel ws, CustomNodeDefinition def) : this(ws)
        {
            Definition = def;
            ResyncWithDefinition();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public Function(WorkspaceModel ws)
            : base(ws)
        {}

        public new string Name
        {
            get { return Definition.WorkspaceModel.Name; }
            set
            {
                Definition.WorkspaceModel.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public override string Description
        {
            get { return Definition == null ? string.Empty : Definition.WorkspaceModel.Description; }
            set
            {
                Definition.WorkspaceModel.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        [Obsolete("Use Definition.FunctionId.ToString()")]
        public string Symbol
        {
            get { return Definition.FunctionId.ToString(); }
        }

        public new string Category
        {
            get
            {
                return Workspace.DynamoModel.CustomNodeManager.NodeInfos.ContainsKey(Definition.FunctionId)
                    ? Workspace.DynamoModel.CustomNodeManager.NodeInfos[Definition.FunctionId].Category
                    : "Custom Nodes";
            }
        }


        /// <summary>
        /// </summary>
        public CustomNodeDefinition Definition { get; set; }

        /// <summary>
        ///     Updates this Custom Node's data to match its Definition.
        /// </summary>
        public void ResyncWithDefinition()
        {
            DisableReporting();

            if (Definition.Parameters != null)
            {
                InPortData.Clear();
                foreach (string arg in Definition.Parameters)
                    InPortData.Add(new PortData(arg, "parameter"));
            }

            OutPortData.Clear();
            if (Definition.ReturnKeys != null && Definition.ReturnKeys.Any())
            {
                foreach (string key in Definition.ReturnKeys)
                    OutPortData.Add(new PortData(key, "return value"));
            }
            else
                OutPortData.Add(new PortData("", "return value"));

            RegisterAllPorts();
            NickName = Definition.DisplayName;

            EnableReporting();
        }

        /// <summary>
        ///   Return if the custom node instance is in sync with its definition.
        ///   It may be out of sync if .dyf file is opened and updated and then
        ///   .dyn file is opened. 
        /// </summary>
        /// <returns></returns>
        public bool IsInSyncWithDefinition()
        {
            if (Definition.Parameters != null)
            {
                if (Definition.Parameters.Count() != InPortData.Count() ||
                    !Definition.Parameters.SequenceEqual(InPortData.Select(p => p.NickName)))
                    return false;
            }

            if (Definition.ReturnKeys != null)
            {
                if (Definition.ReturnKeys.Count() != OutPortData.Count() ||
                    !Definition.ReturnKeys.SequenceEqual(OutPortData.Select(p => p.NickName)))
                    return false;
            }

            return true;
        }

        //protected override InputNode Compile(IEnumerable<string> portNames)
        //{
        //    return SaveResult ? base.Compile(portNames) : new FunctionNode(Symbol, portNames);
        //}

        /// <summary>
        ///     Sets the inputs of this function.
        /// </summary>
        /// <param name="inputs"></param>
        [Obsolete]
        public void SetInputs(IEnumerable<string> inputs)
        {
            int i = 0;
            foreach (string input in inputs)
            {
                if (InPortData.Count > i)
                    InPortData[i].NickName = input;
                else
                    InPortData.Add(new PortData(input, "Input #" + (i + 1)));

                i++;
            }

            if (i < InPortData.Count)
            {
                //for (var k = i; k < InPortData.Count; k++)
                //    InPorts[k].KillAllConnectors();

                //MVVM: confirm that extension methods on observable collection do what we expect
                InPortData.RemoveRange(i, InPortData.Count - i);
            }
        }

        [Obsolete]
        public void SetOutputs(IEnumerable<string> outputs)
        {
            int i = 0;
            foreach (string output in outputs)
            {
                if (OutPortData.Count > i)
                    OutPortData[i].NickName = output;
                else
                    OutPortData.Add(new PortData(output, "Output #" + (i + 1)));

                i++;
            }

            if (i < OutPortData.Count)
            {
                //for (var k = i; k < OutPortData.Count; k++)
                //    OutPorts[k].KillAllConnectors();

                OutPortData.RemoveRange(i, OutPortData.Count - i);
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("ID");

            outEl.SetAttribute("value", Definition.FunctionId.ToString());
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Name");
            outEl.SetAttribute("value", NickName);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Description");
            outEl.SetAttribute("value", Description);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (string input in InPortData.Select(x => x.NickName))
            {
                XmlElement inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (string output in OutPortData.Select(x => x.NickName))
            {
                XmlElement outputEl = xmlDoc.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outEl.AppendChild(outputEl);
            }
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            List<XmlNode> childNodes = nodeElement.ChildNodes.Cast<XmlNode>().ToList();

            XmlNode nameNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Name"));
            if (nameNode != null && nameNode.Attributes != null)
                NickName = nameNode.Attributes[0].Value;

            XmlNode idNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("ID"));
            if (idNode != null && idNode.Attributes != null)
            {
                string id = idNode.Attributes[0].Value;
                Guid funcId;
                if (!Guid.TryParse(id, out funcId) && nodeElement.Attributes != null)
                    funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nodeElement.Attributes["nickname"].Value);
                if (!VerifyFuncId(ref funcId))
                {
                    LoadProxyCustomNode(funcId);
                }
                Definition = Workspace.DynamoModel.CustomNodeManager.GetFunctionDefinition(funcId);

                if (Definition.IsProxy)
                {
                    Error("Cannot load custom node");
                }
            }

            foreach (XmlNode subNode in childNodes)
            {
                if (subNode.Name.Equals("Outputs"))
                {
                    var data =
                        subNode.ChildNodes.Cast<XmlNode>()
                               .Select(
                                   (outputNode, i) =>
                                       new
                                       {
                                           data = new PortData(outputNode.Attributes[0].Value, "Output #" + (i + 1)),
                                           idx = i
                                       });

                    foreach (var dataAndIdx in data)
                    {
                        if (OutPortData.Count > dataAndIdx.idx)
                            OutPortData[dataAndIdx.idx] = dataAndIdx.data;
                        else
                            OutPortData.Add(dataAndIdx.data);
                    }
                }
                else if (subNode.Name.Equals("Inputs"))
                {
                    var data =
                        subNode.ChildNodes.Cast<XmlNode>()
                               .Select(
                                   (inputNode, i) =>
                                       new
                                       {
                                           data = new PortData(inputNode.Attributes[0].Value, "Input #" + (i + 1)),
                                           idx = i
                                       });

                    foreach (var dataAndIdx in data)
                    {
                        if (InPortData.Count > dataAndIdx.idx)
                            InPortData[dataAndIdx.idx] = dataAndIdx.data;
                        else
                            InPortData.Add(dataAndIdx.data);
                    }
                }

                    #region Legacy output support

                else if (subNode.Name.Equals("Output"))
                {
                    var data = new PortData(subNode.Attributes[0].Value, "function output");

                    if (OutPortData.Any())
                        OutPortData[0] = data;
                    else
                        OutPortData.Add(data);
                }

                #endregion
            }

            if (!IsInSyncWithDefinition())
            {
                ResyncWithDefinition();
            }
            else
            {
                RegisterAllPorts();
            }

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;

            //if (Definition != null)
            //    ResyncWithDefinition();
        }

        private bool VerifyFuncId(ref Guid funcId)
        {
            if (funcId == null)
                return false;

            // if the dyf does not exist on the search path...
            if (Workspace.DynamoModel.CustomNodeManager.Contains(funcId))
                return true;

            CustomNodeManager manager = Workspace.DynamoModel.CustomNodeManager;

            // if there is a node with this name, use it instead
            if (manager.Contains(NickName))
            {
                funcId = manager.GetGuidFromName(NickName);
                return true;
            }

            return false;
        }

        private void LoadProxyCustomNode(Guid funcId)
        {
            var proxyDef = new CustomNodeDefinition(funcId)
            {
                WorkspaceModel = new CustomNodeWorkspaceModel(this.Workspace.DynamoModel, NickName, "Custom Nodes") { FileName = null }
            };
            proxyDef.IsProxy = true;

            string userMsg = "Failed to load custom node: " + NickName + ".  Replacing with proxy custom node.";

            Workspace.DynamoModel.Logger.Log(userMsg);

            // tell custom node loader, but don't provide path, forcing user to resave explicitly
            Workspace.DynamoModel.CustomNodeManager.SetFunctionDefinition(funcId, proxyDef);
        }

        //public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        //{
        //    if (OutPortData.Count > 1)
        //    {
        //        var query = (Evaluate(args) as FScheme.Value.List).Item.Zip(
        //            OutPortData,
        //            (value, data) => new { value, data });

        //        foreach (var result in query)
        //            outPuts[result.data] = result.value;
        //    }
        //    else
        //        base.Evaluate(args, outPuts);
        //}

        //public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        //{
        //    //return ((FScheme.Value.Function)Controller.FSchemeEnvironment.LookupSymbol(Symbol))
        //    //    .Item.Invoke(args);

        //    throw new NotImplementedException("FSchemeEnvironment has been removed.");
        //}

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();

            if (OutPortData.Count == 1)
            {
                if (IsPartiallyApplied)
                {
                    var count = Definition.Parameters.Count();
                    AssociativeNode functionCall = AstFactory.BuildFunctionObject(
                        Definition.FunctionName,
                        count,
                        Enumerable.Range(0, count).Where(HasInput),
                        inputAstNodes);
                    resultAst.Add(AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall));
                    resultAst.Add(
                        AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstIdentifierForPreview));
                }
                else
                {
                    AssociativeNode functionCall = AstFactory.BuildFunctionCall(
                        Definition.FunctionName,
                        inputAstNodes);

                    resultAst.Add(AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall));

                    // assign the entire result to the only output port

                    var outId = GetAstIdentifierForOutputIndex(0);

                    if (AstIdentifierForPreview.Value != outId.Value)
                        resultAst.Add(AstFactory.BuildAssignment(outId, AstIdentifierForPreview));
                }
            }
            else
            {
                AssociativeNode functionCall = AstFactory.BuildFunctionCall(
                    Definition.FunctionName,
                    inputAstNodes);
                resultAst.Add(AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall));

                /* previewId = customNodeFunc(arg0, arg1 ...);
                 * outId0 = previewId[key0];
                 * outId1 = previewId[key1];
                 * ...
                 */

                // indexers for each output
                IEnumerable<AssociativeNode> indexers = Definition.ReturnKeys != null
                    ? Definition.ReturnKeys.Select(AstFactory.BuildStringNode) as IEnumerable<AssociativeNode>
                    : Enumerable.Range(0, OutPortData.Count).Select(AstFactory.BuildIntNode);

                // for each output, pull the output from the result
                // based on the associated return key and assign to
                // corresponding output identifier
                resultAst.AddRange(
                    indexers.Select(
                        (rtnKey, index) =>
                            AstFactory.BuildAssignment(
                                GetAstIdentifierForOutputIndex(index),
                                AstFactory.BuildIdentifier(AstIdentifierForPreview.Name, rtnKey))));
            }

            return resultAst;
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                helper.SetAttribute("functionId", Definition.FunctionId.ToString());
                helper.SetAttribute("functionName", NickName);
                helper.SetAttribute("functionDesc", Description);

                XmlDocument xmlDoc = element.OwnerDocument;
                foreach (string input in InPortData.Select(x => x.NickName))
                {
                    XmlElement inputEl = xmlDoc.CreateElement("functionInput");
                    inputEl.SetAttribute("inputValue", input);
                    element.AppendChild(inputEl);
                }

                foreach (string input in OutPortData.Select(x => x.NickName))
                {
                    XmlElement outputEl = xmlDoc.CreateElement("functionOutput");
                    outputEl.SetAttribute("outputValue", input);
                    element.AppendChild(outputEl);
                }
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                NickName = helper.ReadString("functionName");

                Guid funcId;
                if (!Guid.TryParse(helper.ReadString("functionId"), out funcId))
                    funcId = GuidUtility.Create(GuidUtility.UrlNamespace, NickName);

                if (!VerifyFuncId(ref funcId))
                {
                    LoadProxyCustomNode(funcId);
                    return;
                }

                Definition = Workspace.DynamoModel.CustomNodeManager.GetFunctionDefinition(funcId);

                XmlNodeList inNodes = element.SelectNodes("functionInput");
                XmlNodeList outNodes = element.SelectNodes("functionOutput");

                var inData =
                    inNodes.Cast<XmlNode>()
                        .Select(
                            (inputNode, i) =>
                                new
                                {
                                    data = new PortData(inputNode.Attributes[0].Value, "Input #" + (i + 1)),
                                    idx = i
                                });

                foreach (var dataAndIdx in inData)
                {
                    if (InPortData.Count > dataAndIdx.idx)
                        InPortData[dataAndIdx.idx] = dataAndIdx.data;
                    else
                        InPortData.Add(dataAndIdx.data);
                }

                var outData =
                    outNodes.Cast<XmlNode>()
                        .Select(
                            (outputNode, i) =>
                                new
                                {
                                    data = new PortData(outputNode.Attributes[0].Value, "Output #" + (i + 1)),
                                    idx = i
                                });

                foreach (var dataAndIdx in outData)
                {
                    if (OutPortData.Count > dataAndIdx.idx)
                        OutPortData[dataAndIdx.idx] = dataAndIdx.data;
                    else
                        OutPortData.Add(dataAndIdx.data);
                }

                //Added it the same way as LoadNode. But unsure of when 'Output' ChildNodes will
                //be added to element. As of now I dont think it is added during serialize

                #region Legacy output support

                foreach (var portData in 
                    from XmlNode subNode in element.ChildNodes
                    where subNode.Name.Equals("Output")
                    select new PortData(subNode.Attributes[0].Value, "function output"))
                {
                    if (OutPortData.Any())
                        OutPortData[0] = portData;
                    else
                        OutPortData.Add(portData);
                }

                #endregion

                RegisterAllPorts();

                Description = helper.ReadString("functionDesc");
            }
        }

        #endregion
    }

    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function parameter, use with custom nodes")]
    [NodeSearchTags("variable", "argument", "parameter")]
    [IsInteractive(false)]
    [NotSearchableInHomeWorkspace]
    [IsDesignScriptCompatible]
    public partial class Symbol : NodeModel
    {
        private string inputSymbol = "";

        public Symbol(WorkspaceModel ws)
            : base(ws)
        {
            OutPortData.Add(new PortData("", "Symbol"));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public string InputSymbol
        {
            get { return inputSymbol; }
            set
            {
                inputSymbol = value;
                ReportModification();
                RaisePropertyChanged("InputSymbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return string.IsNullOrEmpty(InputSymbol)
                ? AstIdentifierForPreview
                : AstFactory.BuildIdentifier(InputSymbol);
        }

        //protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        //{
        //    Dictionary<int, INode> result;
        //    if (!preBuilt.TryGetValue(this, out result))
        //    {
        //        result = new Dictionary<int, INode>();
        //        result[outPort] = new SymbolNode(GUID.ToString());
        //        preBuilt[this] = result;
        //    }
        //    return result[outPort];
        //}

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (var subNode in
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                InputSymbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        /*
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 0, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
        */
    }

    [NodeName("Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function output, use with custom nodes")]
    [IsInteractive(false)]
    [NotSearchableInHomeWorkspace]
    [IsDesignScriptCompatible]
    public partial class Output : NodeModel
    {
        private string symbol = "";

        public Output(WorkspaceModel ws)
            : base(ws)
        {
            InPortData.Add(new PortData("", ""));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                ReportModification();
                RaisePropertyChanged("Symbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > OutPortData.Count)
                throw new ArgumentOutOfRangeException("outputIndex", @"Index must correspond to an OutPortData index.");

            return AstIdentifierForPreview;
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode assignment;
            if (null == inputAstNodes || inputAstNodes.Count == 0)
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, AstFactory.BuildNullNode());
            else
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0]);

            return new[] { assignment };
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (var subNode in 
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                Symbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        /*
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 0);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
        */
    }
}

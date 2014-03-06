using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
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
    public partial class Function : NodeWithOneOutput
    {
        protected internal Function(CustomNodeDefinition def)
        {
            Definition = def;
            ResyncWithDefinition();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// Updates this Custom Node's data to match its Definition.
        /// </summary>
        public void ResyncWithDefinition()
        {
            DisableReporting();

            if (Definition.Parameters != null)
            {
                InPortData.Clear();
                foreach (var arg in Definition.Parameters)
                {
                    InPortData.Add(new PortData(arg, "parameter", typeof(object)));
                }
            }

            OutPortData.Clear();
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

        public Function() { }

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
            get
            {
                if (Definition == null)
                    return string.Empty;
                return Definition.WorkspaceModel.Description;
            }
            set
            {
                Definition.WorkspaceModel.Description = value;
                RaisePropertyChanged("Description");
            }
        }

        [Obsolete("Use Definition.FunctionId.ToString()")]
        public string Symbol
        {
            get
            {
                return Definition.FunctionId.ToString();
            }
        }

        public new string Category
        {
            get
            {

                if (
                    dynSettings.Controller.CustomNodeManager.NodeInfos.ContainsKey(
                        Definition.FunctionId))
                    return
                        dynSettings.Controller.CustomNodeManager.NodeInfos[
                            this.Definition.FunctionId].Category;
                else
                {
                    return "Custom Nodes";
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public CustomNodeDefinition Definition { get; set; }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return SaveResult ? base.Compile(portNames) : new FunctionNode(Symbol, portNames);
        }

        /// <summary>
        /// Sets the inputs of this function.
        /// </summary>
        /// <param name="inputs"></param>
        [Obsolete]
        public void SetInputs(IEnumerable<string> inputs)
        {
            int i = 0;
            foreach (string input in inputs)
            {
                if (InPortData.Count > i)
                {
                    InPortData[i].NickName = input;
                }
                else
                {
                    InPortData.Add(new PortData(input, "Input #" + (i + 1), typeof(object)));
                }

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
                {
                    OutPortData[i].NickName = output;
                }
                else
                {
                    OutPortData.Add(new PortData(output, "Output #" + (i + 1), typeof(object)));
                }

                i++;
            }

            if (i < OutPortData.Count)
            {
                //for (var k = i; k < OutPortData.Count; k++)
                //    OutPorts[k].KillAllConnectors();

                OutPortData.RemoveRange(i, OutPortData.Count - i);
            }
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("ID");

            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Name");
            outEl.SetAttribute("value", NickName);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Description");
            outEl.SetAttribute("value", Description);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (var input in InPortData.Select(x => x.NickName))
            {
                var inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (var output in OutPortData.Select(x => x.NickName))
            {
                var outputEl = xmlDoc.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outEl.AppendChild(outputEl);
            }
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Name"))
                {
                    NickName = subNode.Attributes[0].Value;
                }
            }

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("ID"))
                {
                    Guid funcId;
                    if (!Guid.TryParse(subNode.Attributes[0].Value, out funcId))
                    {
                        funcId = GuidUtility.Create(
                            GuidUtility.UrlNamespace, nodeElement.Attributes["nickname"].Value);
                    }
                    if (!VerifyFuncId(ref funcId))
                    {
                        LoadProxyCustomNode(funcId);
                        return;
                    }
                    Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funcId);
                }
            }

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Outputs"))
                {
                    int i = 0;
                    foreach (XmlNode outputNode in subNode.ChildNodes)
                    {
                        var data = new PortData(
                            outputNode.Attributes[0].Value, "Output #" + (i + 1), typeof(object));

                        if (OutPortData.Count > i)
                        {
                            OutPortData[i] = data;
                        }
                        else
                        {
                            OutPortData.Add(data);
                        }

                        i++;
                    }
                }
                else if (subNode.Name.Equals("Inputs"))
                {
                    int i = 0;
                    foreach (XmlNode inputNode in subNode.ChildNodes)
                    {
                        var data = new PortData(
                            inputNode.Attributes[0].Value, "Input #" + (i + 1), typeof(object));

                        if (InPortData.Count > i)
                        {
                            InPortData[i] = data;
                        }
                        else
                        {
                            InPortData.Add(data);
                        }

                        i++;
                    }
                }
                    #region Legacy output support

                else if (subNode.Name.Equals("Output"))
                {
                    var data = new PortData(
                        subNode.Attributes[0].Value, "function output", typeof(object));

                    if (OutPortData.Any())
                        OutPortData[0] = data;
                    else
                        OutPortData.Add(data);
                }

                #endregion
            }

            RegisterAllPorts();

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;

            //if (Definition != null)
            //    ResyncWithDefinition();
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("functionId", Symbol);
                helper.SetAttribute("functionName", NickName);
                helper.SetAttribute("functionDesc", Description);

                XmlDocument xmlDoc = element.OwnerDocument;
                foreach (var input in InPortData.Select(x => x.NickName))
                {
                    var inputEl = xmlDoc.CreateElement("functionInput");
                    inputEl.SetAttribute("inputValue", input);
                    element.AppendChild(inputEl);
                }

                foreach (var input in OutPortData.Select(x => x.NickName))
                {
                    var outputEl = xmlDoc.CreateElement("functionOutput");
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
                XmlElementHelper helper = new XmlElementHelper(element);
                NickName = helper.ReadString("functionName");

                Guid funcId;
                if(!Guid.TryParse(helper.ReadString("functionId"), out funcId))
                {
                    funcId = GuidUtility.Create(GuidUtility.UrlNamespace, NickName);
                }

                if (!VerifyFuncId(ref funcId))
                {
                    LoadProxyCustomNode(funcId);
                    return;
                }

                Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funcId);
                
                XmlNodeList inNodes = element.SelectNodes("functionInput");
                XmlNodeList outNodes = element.SelectNodes("functionOutput");
                int i = 0;
                foreach (XmlNode inputNode in inNodes)
                {
                    string name = inputNode.Attributes[0].Value;
                    var data = new PortData(name, "Input #" + (i + 1), typeof(object));
                    if (InPortData.Count > i)
                        InPortData[i] = data;
                    else
                        InPortData.Add(data);
                    i++;
                }
                i = 0;
                foreach (XmlNode outputNode in outNodes)
                {
                    string name = outputNode.Attributes[0].Value;
                    var data = new PortData(name, "Output #" + (i + 1), typeof(object));
                    if (OutPortData.Count > i)
                        OutPortData[i] = data;
                    else
                        OutPortData.Add(data);
                    i++;
                }

                //Added it the same way as LoadNode. But unsure of when 'Output' ChildNodes will
                //be added to element. As of now I dont think it is added during serialize

                #region Legacy output support

                foreach (XmlNode subNode in element.ChildNodes)
                {
                    if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(
                            subNode.Attributes[0].Value, "function output", typeof(object));

                        if (OutPortData.Any())
                            OutPortData[0] = data;
                        else
                            OutPortData.Add(data);
                    }
                }

                #endregion


                RegisterAllPorts();

                Description = helper.ReadString("functionDesc");
            }
        }

        #endregion

        private bool VerifyFuncId(ref Guid funcId)
        {
            // if the dyf does not exist on the search path...
            if (dynSettings.Controller.CustomNodeManager.Contains(funcId))
                return true;

            var manager = dynSettings.Controller.CustomNodeManager;

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
                WorkspaceModel =
                    new CustomNodeWorkspaceModel(
                        NickName, "Custom Nodes")
                    {
                        FileName = null
                    }
            };

            var userMsg = "Failed to load custom node: " + NickName +
                          ".  Replacing with proxy custom node.";

            DynamoLogger.Instance.Log(userMsg);

            // tell custom node loader, but don't provide path, forcing user to resave explicitly
            dynSettings.Controller.CustomNodeManager.SetFunctionDefinition(funcId, proxyDef);
            Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funcId);
            
            ArgumentLacing = LacingStrategy.Disabled;
            ResyncWithDefinition();
            RegisterAllPorts();
            State = ElementState.Error;
        }

        public override void Evaluate(
            FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            if (OutPortData.Count > 1)
            {
                var query = (Evaluate(args) as FScheme.Value.List).Item.Zip(
                    OutPortData, (value, data) => new { value, data });

                foreach (var result in query)
                    outPuts[result.data] = result.value;
            }
            else
                base.Evaluate(args, outPuts);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //return ((FScheme.Value.Function)Controller.FSchemeEnvironment.LookupSymbol(Symbol))
            //    .Item.Invoke(args);

            throw new NotImplementedException("FSchemeEnvironment has been removed.");
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall(Definition.FunctionName, inputAstNodes);

            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, functionCall)
            };

            if (OutPortData.Count == 1)
            {
                // assign the entire result to the only output port
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstIdentifierForPreview));
            }
            else
            {
                /* previewId = customNodeFunc(arg0, arg1 ...);
                 * outId0 = previewId[key0];
                 * outId1 = previewId[key1];
                 * ...
                 */

                // indexers for each output
                var indexers = Definition.ReturnKeys != null
                    ? Definition.ReturnKeys.Select(AstFactory.BuildStringNode) as
                        IEnumerable<AssociativeNode>
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
    }

    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function parameter, use with custom nodes")]
    [NodeSearchTags("variable", "argument", "parameter")]
    [IsInteractive(false)]
    [IsDesignScriptCompatible]
    public partial class Symbol : NodeModel
    {
        public Symbol()
        {
            OutPortData.Add(new PortData("", "Symbol", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private string _inputSymbol = "";

        public string InputSymbol
        {
            get { return _inputSymbol; }
            set
            {
                _inputSymbol = value;
                ReportModification();
                RaisePropertyChanged("InputSymbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (string.IsNullOrEmpty(InputSymbol))
                return AstIdentifierForPreview;
            else
                return AstFactory.BuildIdentifier(InputSymbol);
        }

        protected internal override INode Build(
            Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new SymbolNode(GUID.ToString());
                preBuilt[this] = result;
            }
            return result[outPort];
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    InputSymbol = subNode.Attributes[0].Value;
                }
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
    [IsDesignScriptCompatible]
    public partial class Output : NodeModel
    {
        public Output()
        {
            InPortData.Add(new PortData("", "", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private string _symbol = "";

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
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

            return new AssociativeNode[] { assignment };
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    Symbol = subNode.Attributes[0].Value;
                }
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

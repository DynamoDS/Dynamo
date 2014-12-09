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
    ///     DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("Instance of a Custom Node")]
    [IsInteractive(false)]
    [NodeSearchable(false)]
    [IsMetaNode]
    public partial class Function : FunctionCallBase
    {
        public Function(WorkspaceModel workspaceModel) : this(workspaceModel, null) { }

        protected internal Function(WorkspaceModel workspace, CustomNodeDefinition def)
            : base(workspace, new CustomNodeController(workspace.DynamoModel, def))
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

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
                var infos = Workspace.DynamoModel.CustomNodeManager.NodeInfos;
                return infos.ContainsKey(Definition.FunctionId)
                    ? infos[Definition.FunctionId].Category
                    : "Custom Nodes";
            }
        }

        /// <summary>
        /// </summary>
        public new CustomNodeController Controller
        {
            get { return base.Controller as CustomNodeController; }
        }

        public CustomNodeDefinition Definition { get { return Controller.Definition; } }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            Controller.SaveNode(xmlDoc, nodeElement, context);
            
            var outEl = xmlDoc.CreateElement("Name");
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

        /// <summary>
        /// Create a definition for custom node and add inputs and outputs
        /// </summary>
        /// <param name="funcID">ID of the definition</param>
        /// <param name="inputs">Number of inputs</param>
        /// <param name="outputs">Number of outputs</param>
        internal void LoadNode(Guid funcID, int inputs, int outputs)
        {
            // create a definition fo custom node
            Controller.LoadNode(funcID, this.NickName);
            
            PortData data;
            if (outputs > -1)
            {
                // create outputs for the node
                for (int i = 0; i < outputs; i++)
                {
                    data = new PortData("", "Output #" + (i + 1));
                    if (OutPortData.Count > i)
                        OutPortData[i] = data;
                    else
                        OutPortData.Add(data);
                }
            }

            if (inputs > -1)
            {
                // create inputs for the node
                for (int i = 0; i < inputs; i++)
                {
                    data = new PortData("", "Input #" + (i + 1));
                    if (InPortData.Count > i)
                        InPortData[i] = data;
                    else
                        InPortData.Add(data);
                }
            }

            // make the custom node instance be in sync 
            // with its definition if it's needed
            if (!Controller.IsInSyncWithNode(this))
            {
                Controller.SyncNodeWithDefinition(this);
            }
            else
            {
                RegisterAllPorts();
            }

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            Controller.LoadNode(nodeElement);

            List<XmlNode> childNodes = nodeElement.ChildNodes.Cast<XmlNode>().ToList();

            XmlNode nameNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Name"));
            if (nameNode != null && nameNode.Attributes != null)
                NickName = nameNode.Attributes[0].Value;

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

            if (!Controller.IsInSyncWithNode(this))
            {
                Controller.SyncNodeWithDefinition(this);
            }
            else
            {
                RegisterAllPorts();
            }

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            
            if (context != SaveContext.Undo) return;

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

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context != SaveContext.Undo) return;

            var helper = new XmlElementHelper(element);
            NickName = helper.ReadString("functionName");

            Controller.DeserializeCore(element, context);

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

        #endregion

        public void ResyncWithDefinition()
        {
            Controller.SyncNodeWithDefinition(this);
        }

        public void ResyncWithDefinition(CustomNodeDefinition def)
        {
            Controller.Definition = def;
            ResyncWithDefinition();
            RequiresRecalc = true;
        }
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

        public Symbol(WorkspaceModel workspace) : base(workspace)
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
            return
                AstFactory.BuildIdentifier(
                    InputSymbol == null ? AstIdentifierBase : InputSymbol + "__" + AstIdentifierBase);
        }

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

        public Output(WorkspaceModel workspace) : base(workspace)
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
    }
}

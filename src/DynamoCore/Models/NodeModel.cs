using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using Autodesk.DesignScript.Interfaces;

using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Selection;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using String = System.String;
using StringNode = ProtoCore.AST.AssociativeAST.StringNode;
using ProtoCore.DSASM;

namespace Dynamo.Models
{
    public abstract class NodeModel : ModelBase, IBlockingModel
    {
        #region private members

        private bool overrideNameWithNickName;
        private LacingStrategy argumentLacing = LacingStrategy.First;
        private bool displayLabels;
        private bool interactionEnabled = true;
        private bool isUpstreamVisible;
        private bool isVisible;
        private string nickName;
        private ElementState state;
        private string toolTipText = "";
        private bool saveResult;
        private string description;
        private const string FailureString = "Node evaluation failed";

        // Data caching related class members. There are multiple parties at
        // play when it comes to caching MirrorData for a NodeModel, this value
        // is accessed from UI thread for display (e.g. preview bubble) and it's
        // updated by QueryMirrorDataAsyncTask on ISchedulerThread's context. 
        // Access to the cached data is guarded by cachedMirrorDataMutex object.
        // 
        private bool isUpdated = false;
        private MirrorData cachedMirrorData = null;
        private readonly object cachedMirrorDataMutex = new object();

        // Input and output port related data members.
        private ObservableCollection<PortModel> inPorts = new ObservableCollection<PortModel>();
        private ObservableCollection<PortModel> outPorts = new ObservableCollection<PortModel>();
        private readonly Dictionary<PortModel, PortData> portDataDict = new Dictionary<PortModel, PortData>();

        private List<IRenderPackage> _renderPackages = new List<IRenderPackage>();

        #endregion

        #region public members

        public WorkspaceModel Workspace { get; internal set; }

        public Dictionary<int, Tuple<int, NodeModel>> Inputs = new Dictionary<int, Tuple<int, NodeModel>>();

        public Dictionary<int, HashSet<Tuple<int, NodeModel>>> Outputs =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

        public Object RenderPackagesMutex = new object();
        public List<IRenderPackage> RenderPackages
        {
            get
            {
                lock (RenderPackagesMutex)
                {
                    return _renderPackages; 
                }
            }
            set
            {
                lock (RenderPackagesMutex)
                {
                    _renderPackages = value;
                }
                RaisePropertyChanged("RenderPackages");
            }
        }

        public bool HasRenderPackages { get; set; }

        /// <summary>
        /// The unique name that was created the node by
        /// </summary>
        public virtual string CreationName { get { return this.Name; } }

        #endregion

        #region events

        public event DispatchedToUIThreadHandler DispatchedToUI;

        #endregion

        #region public properties

        /// <summary>
        ///     Definitions for the Input Ports of this NodeModel.
        /// </summary>
        public ObservableCollection<PortData> InPortData { get; private set; }
        
        /// <summary>
        ///     Definitions for the Output Ports of this NodeModel.
        /// </summary>
        public ObservableCollection<PortData> OutPortData { get; private set; }

        /// <summary>
        ///     All of the connectors entering and exiting the NodeModel.
        /// </summary>
        public IEnumerable<ConnectorModel> AllConnectors
        {
            get
            {
                return inPorts.Concat(outPorts).SelectMany(port => port.Connectors);
            }
        }

        /// <summary>
        ///     Returns whether this node represents a built-in or custom function.
        /// </summary>
        public bool IsCustomFunction
        {
            get { return this is Function; }
        }

        /// <summary>
        ///     Returns whether the node is to be included in visualizations.
        /// </summary>
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        /// <summary>
        ///     Returns whether the node aggregates its upstream connections
        ///     for visualizations.
        /// </summary>
        public bool IsUpstreamVisible
        {
            get { return isUpstreamVisible; }
            set
            {
                isUpstreamVisible = value;
                RaisePropertyChanged("IsUpstreamVisible");
            }
        }

        /// <summary>
        ///     The Node's state, which determines the coloring of the Node in the canvas.
        /// </summary>
        public ElementState State
        {
            get { return state; }
            set
            {
                if (value != ElementState.Error && value != ElementState.AstBuildBroken)
                    ClearTooltipText();

                state = value;

                // Suppress notification if not reporting. Reporting is disabled 
                // in cases like clearing the workbench to avoid nodes recoloring 
                // as connectors are deleted.
                if (IsReportingModifications)
                    RaisePropertyChanged("State");
            }
        }

        /// <summary>
        ///   If the state of node is Error or AstBuildBroken
        /// </summary>
        public bool IsInErrorState
        {
            get
            {
                return state == ElementState.AstBuildBroken || state == ElementState.Error;
            }
        }

        /// <summary>
        ///     Text that is displayed as this Node's tooltip.
        /// </summary>
        public string ToolTipText
        {
            get { return toolTipText; }
            set
            {
                toolTipText = value;
                RaisePropertyChanged("ToolTipText");
            }
        }

        /// <summary>
        ///     Should we override the displayed name with this Node's NickName property?
        /// </summary>
        public bool OverrideNameWithNickName
        {
            get { return overrideNameWithNickName; }
            set
            {
                overrideNameWithNickName = value;
                RaisePropertyChanged("OverrideNameWithNickName");
            }
        }

        /// <summary>
        ///     The name that is displayed in the UI for this NodeModel.
        /// </summary>
        public string NickName
        {
            get { return nickName; }
            set
            {
                nickName = value;
                RaisePropertyChanged("NickName");
            }
        }

        /// <summary>
        ///     Collection of PortModels representing all Input ports.
        /// </summary>
        public ObservableCollection<PortModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        /// <summary>
        ///     Collection of PortModels representing all Output ports.
        /// </summary>
        public ObservableCollection<PortModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        /// <summary>
        ///     Control how arguments lists of various sizes are laced.
        /// </summary>
        public LacingStrategy ArgumentLacing
        {
            get { return argumentLacing; }
            set
            {
                if (argumentLacing != value)
                {
                    argumentLacing = value;
                    RequiresRecalc = true;
                    RaisePropertyChanged("ArgumentLacing");
                }
            }
        }

        /// <summary>
        ///     Name property
        /// </summary>
        /// <value>
        ///     If the node has a name attribute, return it.  Other wise return empty string.
        /// </value>
        public string Name
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
                if (type.Namespace == "Dynamo.Nodes" && !type.IsAbstract && attribs.Length > 0
                    && type.IsSubclassOf(typeof(NodeModel)))
                {
                    var elCatAttrib = attribs[0] as NodeNameAttribute;
                    return elCatAttrib.Name;
                }
                return "";
            }
        }

        /// <summary>
        ///     Category property
        /// </summary>
        /// <value>
        ///     If the node has a category, return it.  Other wise return empty string.
        /// </value>
        public virtual string Category
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
                if (type.Namespace == "Dynamo.Nodes" && !type.IsAbstract && attribs.Length > 0
                    && type.IsSubclassOf(typeof(NodeModel)))
                {
                    var elCatAttrib = attribs[0] as NodeCategoryAttribute;
                    return elCatAttrib.ElementCategory;
                }
                return "";
            }
        }

        public MirrorData CachedValue
        {
            get
            {
                lock (cachedMirrorDataMutex)
                {
                    return cachedMirrorData;
                }
            }
        }

        /// <summary>
        ///     If the node is updated in LiveRunner's execution
        /// </summary>
        public bool IsUpdated
        {
            get { return isUpdated; }
            set
            {
                isUpdated = value;
                if (isUpdated)
                {
                    // When a NodeModel is updated, its
                    // cached data should be invalidated.
                    lock (cachedMirrorDataMutex)
                    {
                        cachedMirrorData = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Determines whether or not the output of this Element will be saved. If true, Evaluate() will not be called
        ///     unless IsDirty is true. Otherwise, Evaluate will be called regardless of the IsDirty value.
        /// </summary>
        internal bool SaveResult
        {
            get { return saveResult && Enumerable.Range(0, InPortData.Count).All(HasInput); }
            set { saveResult = value; }
        }

        /// <summary>
        ///     Is this node an entry point to the program?
        /// </summary>
        public bool IsTopmost
        {
            get { return OutPorts == null || OutPorts.All(x => !x.Connectors.Any()); }
        }

        /// <summary>
        ///     Search tags for this Node.
        /// </summary>
        public List<string> Tags
        {
            get
            {
                return
                    GetType()
                        .GetCustomAttributes(typeof(NodeSearchTagsAttribute), true)
                        .Cast<NodeSearchTagsAttribute>()
                        .SelectMany(x => x.Tags)
                        .ToList();
            }
        }

        /// <summary>
        ///     Description of this Node.
        /// </summary>
        public virtual string Description
        {
            get
            {
                description = description ?? GetDescriptionStringFromAttributes();
                return description;
            }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        /// <summary>
        ///     Is UI interaction enabled for this Node?
        /// </summary>
        public bool InteractionEnabled
        {
            get { return interactionEnabled; }
            set
            {
                interactionEnabled = value;
                RaisePropertyChanged("InteractionEnabled");
            }
        }

        /// <summary>
        ///     ProtoAST Identifier for result of the node before any output unpacking has taken place.
        ///     If there is only one output for the node, this is equivalent to GetAstIdentifierForOutputIndex(0).
        /// </summary>
        public IdentifierNode AstIdentifierForPreview
        {
            get { return AstFactory.BuildIdentifier(AstIdentifierBase); }
        }

        /// <summary>
        ///     If this node is allowed to be converted to AST node in nodes to code conversion.
        /// </summary>
        public virtual bool IsConvertible
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Return a variable whose value will be displayed in preview window.
        ///     Derived nodes may overwrite this function to display default value
        ///     of this node. E.g., code block node may want to display the value
        ///     of the left hand side variable of last statement.
        /// </summary>
        public virtual string AstIdentifierBase
        {
            get { return AstBuilder.StringConstants.VarPrefix + GUID.ToString().Replace("-", string.Empty); }
        }

        /// <summary>
        ///     Enable or disable label display. Default is false.
        /// </summary>
        public bool DisplayLabels
        {
            get { return displayLabels; }
            set
            {
                if (displayLabels != value)
                {
                    displayLabels = value;
                    RaisePropertyChanged("DisplayLabels");
                }
            }
        }

        /// <summary>
        ///     Is this node being applied partially, resulting in a partial function?
        /// </summary>
        public bool IsPartiallyApplied
        {
            get { return !Enumerable.Range(0, InPortData.Count).All(HasInput); }
        }

        /// <summary>
        ///     Get the description from type information
        /// </summary>
        /// <returns>The value or "No description provided"</returns>
        public string GetDescriptionStringFromAttributes()
        {
            Type t = GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
            return rtAttribs.Length > 0
                ? ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription
                : "No description provided";
        }

        /// <summary>
        ///     Fetches the ProtoAST Identifier for a given output port.
        /// </summary>
        /// <param name="outputIndex">Index of the output port.</param>
        /// <returns>Identifier corresponding to the given output port.</returns>
        public virtual IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > OutPortData.Count)
                throw new ArgumentOutOfRangeException("outputIndex", @"Index must correspond to an OutPortData index.");

            //if (OutPortData.Count == 1)
            //    return AstFactory.BuildIdentifier(/* (IsPartiallyApplied ? "_local_" : "") + */ AstIdentifierBase);

            string id = AstIdentifierBase + "_out" + outputIndex;
            return AstFactory.BuildIdentifier(id);
        }

        #endregion

        protected NodeModel(WorkspaceModel workspaceModel)
        {
            this.Workspace = workspaceModel;

            InPortData = new ObservableCollection<PortData>();
            OutPortData = new ObservableCollection<PortData>();

            IsVisible = true;
            IsUpstreamVisible = true;

            PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                switch (args.PropertyName)
                {
                    case ("OverrideName"):
                        RaisePropertyChanged("NickName");
                        break;
                    case ("IsSelected"):
                        // Synchronize the selected state of any render packages for this node
                        // with the selection state of the node.
                        if (HasRenderPackages)
                        {
                            lock (RenderPackagesMutex)
                            {
                                RenderPackages.ForEach(rp => ((RenderPackage) rp).Selected = IsSelected);
                            }
                        }
                        break;
                }
            };

            //Fetch the element name from the custom attribute.
            object[] nameArray = GetType().GetCustomAttributes(typeof(NodeNameAttribute), true);

            if (nameArray.Length > 0)
            {
                var elNameAttrib = nameArray[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                    NickName = elNameAttrib.Name;
            }
            else
                NickName = "";

            IsSelected = false;
            State = ElementState.Dead;
            ArgumentLacing = LacingStrategy.Disabled;
            IsReportingModifications = true;
        }

        /// <summary>
        ///     Called when this node is being removed from the Workspace.
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        ///     Implement on derived classes to cleanup resources when
        /// </summary>
        public virtual void Cleanup() { }

        public MirrorData GetValue(int outPortIndex)
        {
            return Workspace.DynamoModel.EngineController.GetMirror(
                GetAstIdentifierForOutputIndex(outPortIndex).Value).GetData();
        }

        #region Modification Reporting

        /// <summary>
        ///     Is this Node reporting state modifications?
        /// </summary>
        protected internal bool IsReportingModifications { get; set; }

        /// <summary>
        ///     Disable reporting of state modifications.
        /// </summary>
        //[Obsolete("Use IsReportingModifications = false")]
        protected internal void DisableReporting()
        {
            IsReportingModifications = false;
        }

        /// <summary>
        ///     Enable reporting of state modifications.
        /// </summary>
        //[Obsolete("Use IsReportingModifications = true")]
        protected internal void EnableReporting()
        {
            IsReportingModifications = true;
            ValidateConnections();
        }

        /// <summary>
        ///     Report to Dynamo that this node's state has been modified.
        /// </summary>
        protected internal void ReportModification()
        {
            if (IsReportingModifications && Workspace != null)
                Workspace.Modified();
        }

        #endregion

        #region Load/Save

        /// <summary>
        ///     Override this to implement custom save data for your Element. If overridden, you should also override
        ///     LoadNode() in order to read the data back when loaded.
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole Workspace containing this Element.</param>
        /// <param name="nodeElement">The XmlElement representing this Element.</param>
        /// <param name="context">Why is this being called?</param>
        protected virtual void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context) { }

        /// <summary>
        ///     Saves this node into an XML Document.
        /// </summary>
        /// <param name="xmlDoc">Overall XmlDocument representing the entire Workspace being saved.</param>
        /// <param name="dynEl">The XmlElement representing this node in the Workspace.</param>
        /// <param name="context">The context of this save operation.</param>
        public void Save(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            SaveNode(xmlDoc, dynEl, context);
            
            var portsWithDefaultValues = 
                inPorts.Select((port, index) => new { port, index })
                   .Where(x => x.port.UsingDefaultValue);

            //write port information
            foreach (var port in portsWithDefaultValues)
            {
                XmlElement portInfo = xmlDoc.CreateElement("PortInfo");
                portInfo.SetAttribute("index", port.index.ToString(CultureInfo.InvariantCulture));
                portInfo.SetAttribute("default", true.ToString());
                dynEl.AppendChild(portInfo);
            }
        }

        /// <summary>
        ///     Override this to implement loading of custom data for your Element. If overridden, you should also override
        ///     SaveNode() in order to write the data when saved.
        /// </summary>
        /// <param name="nodeElement">The XmlNode representing this Element.</param>
        protected virtual void LoadNode(XmlNode nodeElement) { }

        public void Load(XmlNode elNode)
        {
            LoadNode(elNode);

            var portInfoProcessed = new HashSet<int>();

            //read port information
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "PortInfo")
                {
                    int index = int.Parse(subNode.Attributes["index"].Value);
                    portInfoProcessed.Add(index);
                    bool def = bool.Parse(subNode.Attributes["default"].Value);
                    inPorts[index].UsingDefaultValue = def;
                }
            }

            //set defaults
            foreach (var port in inPorts.Select((x, i) => new { x, i }).Where(x => !portInfoProcessed.Contains(x.i)))
                port.x.UsingDefaultValue = false;
        }

        /// <summary>
        ///     Called when the node's Workspace has been saved.
        /// </summary>
        protected internal virtual void OnSave() { }
        
        #endregion

        #region ProtoAST Compilation

        /// <summary>
        /// Override this to declare the outputs for each of this Node's output ports.
        /// </summary>
        /// <param name="inputAstNodes">Ast for inputs indexed by input port index.</param>
        /// <returns>Sequence of AssociativeNodes representing this Node's code output.</returns>
        public virtual IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return
                OutPortData.Enumerate()
                           .Select(
                               output => AstFactory.BuildAssignment(
                                   GetAstIdentifierForOutputIndex(output.Index),
                                   new NullNode()));
        }

        /// <summary>
        /// Wraps the publically overrideable `BuildOutputAst` method so that it works with Preview.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        internal virtual IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            OnBuilt();

            IEnumerable<AssociativeNode> result = null;

            try
            {
                result = BuildOutputAst(inputAstNodes);
            }
            catch (Exception e)
            {
                // If any exception from BuildOutputAst(), we emit
                // a function call "var_guid = %nodeAstFailed(full.node.name)"
                // for this node, set the state of node to AstBuildBroken and
                // disply the corresponding error message. 
                // 
                // The return value of function %nodeAstFailed() is always 
                // null.
                var errorMsg = AstBuilder.StringConstants.AstBuildBrokenMessage;
                var fullMsg = String.Format(errorMsg, e.Message);
                this.NotifyAstBuildBroken(fullMsg);

                var fullName = this.GetType().ToString();
                var astNodeFullName = AstFactory.BuildStringNode(fullName);
                var arguments = new List<AssociativeNode> { astNodeFullName };
                var func = AstFactory.BuildFunctionCall(Constants.kNodeAstFailed, arguments); 

                return new []
                {
                    AstFactory.BuildAssignment(AstIdentifierForPreview, func)
                };
            }

            if (OutPortData.Count == 1)
            {
                var firstOuputIdent = GetAstIdentifierForOutputIndex(0);
                if (!AstIdentifierForPreview.Equals(firstOuputIdent))
                {
                    result = result.Concat(
                    new[]
                    {
                        AstFactory.BuildAssignment(AstIdentifierForPreview, firstOuputIdent)
                    });
                }
                return result;
            }

            var emptyList = AstFactory.BuildExprList(new List<AssociativeNode>());
            var previewIdInit = AstFactory.BuildAssignment(AstIdentifierForPreview, emptyList);

            return
                result.Concat(new[] { previewIdInit })
                      .Concat(
                          OutPortData.Select(
                              (outNode, index) =>
                                  AstFactory.BuildAssignment(
                                      new IdentifierNode(AstIdentifierForPreview)
                                      {
                                          ArrayDimensions =
                                              new ArrayNode
                                              {
                                                  Expr = new StringNode { value = outNode.NickName }
                                              }
                                      },
                                      GetAstIdentifierForOutputIndex(index))));
        }

        /// <summary>
        ///     Callback for when this NodeModel has been compiled.
        /// </summary>
        protected virtual void OnBuilt()
        {
        }

        /// <summary>
        /// Apppend replication guide to the input parameter based on lacing
        /// strategy.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public void AppendReplicationGuides(List<AssociativeNode> inputs)
        {
            if (inputs == null || !inputs.Any())
                return;

            switch (ArgumentLacing)
            {
                case LacingStrategy.Longest:

                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        inputs[i] = AstFactory.AddReplicationGuide(
                                                inputs[i],
                                                new List<int> { 1 },
                                                true);
                    }
                    break;

                case LacingStrategy.CrossProduct:

                    int guide = 1;
                    for (int i = 0; i < inputs.Count(); ++i)
                    {
                        inputs[i] = AstFactory.AddReplicationGuide(
                                                inputs[i],
                                                new List<int> { guide },
                                                false);
                        guide++;
                    }
                    break;
            }
        }
        #endregion

        #region Input and Output Connections

        internal void ConnectInput(int inputData, int outputData, NodeModel node)
        {
            Inputs[inputData] = Tuple.Create(outputData, node);
            RequiresRecalc = true;
        }

        internal void ConnectOutput(int portData, int inputData, NodeModel nodeLogic)
        {
            if (!Outputs.ContainsKey(portData))
                Outputs[portData] = new HashSet<Tuple<int, NodeModel>>();
            Outputs[portData].Add(Tuple.Create(inputData, nodeLogic));
        }

        internal void DisconnectInput(int data)
        {
            Inputs[data] = null;
            RequiresRecalc = true;
        }

        /// <summary>
        ///     Attempts to get the input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <param name="input">If an input is found, it will be assigned.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool TryGetInput(int data, out Tuple<int, NodeModel> input)
        {
            return Inputs.TryGetValue(data, out input) && input != null;
        }

        /// <summary>
        ///     Attempts to get the output for a certain port.
        /// </summary>
        /// <param name="output">Index to look for an output for.</param>
        /// <param name="newOutputs">If an output is found, it will be assigned.</param>
        /// <returns>True if there is an output, false otherwise.</returns>
        public bool TryGetOutput(int output, out HashSet<Tuple<int, NodeModel>> newOutputs)
        {
            return Outputs.TryGetValue(output, out newOutputs);
        }

        /// <summary>
        ///     Checks if there is an input for a certain port.
        /// </summary>
        /// <param name="data">Index of the port to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasInput(int data)
        {
            return HasConnectedInput(data) || (InPorts.Count > data && InPorts[data].UsingDefaultValue);
        }

        /// <summary>
        ///     Checks if there is a connected input for a certain port. This does
        ///     not count default values as an input.
        /// </summary>
        /// <param name="data">Index of the port to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasConnectedInput(int data)
        {
            return Inputs.ContainsKey(data) && Inputs[data] != null;
        }

        /// <summary>
        ///     Checks if there is an output for a certain port.
        /// </summary>
        /// <param name="portData">Index of the port to look for an output for.</param>
        /// <returns>True if there is an output, false otherwise.</returns>
        public bool HasOutput(int portData)
        {
            return Outputs.ContainsKey(portData) && Outputs[portData].Any();
        }

        internal void DisconnectOutput(int portData, int inPortData, NodeModel nodeModel)
        {
            HashSet<Tuple<int, NodeModel>> output;
            if (Outputs.TryGetValue(portData, out output))
                output.RemoveWhere(x => x.Item2 == nodeModel && x.Item1 == inPortData);
        }

        #endregion

        #region UI Framework

        private void ClearTooltipText()
        {
            ToolTipText = "";
        }

        /// <summary>
        /// Clears the errors/warnings that are generated when running the graph
        /// </summary>
        public virtual void ClearRuntimeError()
        {
            State = ElementState.Dead;
            ClearTooltipText();
            ValidateConnections();
        }

        public void SelectNeighbors()
        {
            IEnumerable<ConnectorModel> outConnectors = outPorts.SelectMany(x => x.Connectors);
            IEnumerable<ConnectorModel> inConnectors = inPorts.SelectMany(x => x.Connectors);

            foreach (var c in outConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.End.Owner)))
                DynamoSelection.Instance.Selection.Add(c.End.Owner);

            foreach (var c in inConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.Start.Owner)))
                DynamoSelection.Instance.Selection.Add(c.Start.Owner);
        }

        #region Thread Dispatch

        /// <summary>
        ///     Called by nodes for behavior that they want to dispatch on the UI thread
        ///     Triggers event to be received by the UI. If no UI exists, behavior will not be executed.
        /// </summary>
        /// <param name="a"></param>
        public void DispatchOnUIThread(Action a)
        {
            OnDispatchedToUI(this, new UIDispatcherEventArgs(a));
        }

        public void OnDispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            if (DispatchedToUI != null)
                DispatchedToUI(this, e);
        }

        #endregion

        #region Interaction


        internal void EnableInteraction()
        {
            ValidateConnections();
            InteractionEnabled = true;
        }

        #endregion

        #region Node State

        /// <summary>
        /// Color the connection according to it's port connectivity
        /// if all ports are connected, color green, else color orange
        /// </summary>
        public void ValidateConnections()
        {

            Action setState = (() =>
                {

                    // if there are inputs without connections
                    // mark as dead; otherwise, if the original state is dead,
                    // update it as active.
                    if (inPorts.Any(x => !x.Connectors.Any() && !(x.UsingDefaultValue && x.DefaultValueEnabled)))
                    {
                        if (State == ElementState.Active)
                        {
                            State = ElementState.Dead;
                        }
                    }
                    else
                    {
                        if (State == ElementState.Dead)
                        {
                            State = ElementState.Active;
                        }
                    }
                });

            // This is put in place to solve the crashing issue outlined in 
            // the following defect. ValidateConnections can be called from 
            // a background evaluation thread at any point in time, we do 
            // not want such calls to update UI in anyway while we're here 
            // (the UI update is caused by setting State property which leads
            // to tool-tip update that triggers InfoBubble to update its UI,
            // a problem that is currently being resolved and tested on a 
            // separate branch). When the InfoBubble restructuring gets over,
            // please ensure the following scenario is tested and continue to 
            // work:
            // 
            //      http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            // 

            if (this.Workspace.DynamoModel != null) this.Workspace.DynamoModel.OnRequestDispatcherBeginInvoke(setState);
        }

        public void Error(string p)
        {
            State = ElementState.Error;
            ToolTipText = p;
        }

        public void Warning(string p)
        {
            State = ElementState.Warning;
            ToolTipText = p;
        }

        /// <summary>
        /// Change the state of node to ElementState.AstBuildBroken and display
        /// "p" in tooltip window. 
        /// </summary>
        /// <param name="p"></param>
        public void NotifyAstBuildBroken(string p)
        {
            State = ElementState.AstBuildBroken;
            ToolTipText = p;
        }

        #endregion

        #region Port Management

        internal int GetPortIndexAndType(PortModel portModel, out PortType portType)
        {
            int index = inPorts.IndexOf(portModel);
            if (-1 != index)
            {
                portType = PortType.INPUT;
                return index;
            }

            index = outPorts.IndexOf(portModel);
            if (-1 != index)
            {
                portType = PortType.OUTPUT;
                return index;
            }

            portType = PortType.INPUT;
            return -1; // No port found.
        }

        /// <summary>
        ///     Since the ports can have a margin (offset) so that they can moved vertically from its
        ///     initial position, the center of the port needs to be calculted differently and not only
        ///     based on the index. The function adds the height of other nodes as well as their margins
        /// </summary>
        /// <param name="portModel"> The portModel whose height is to be found</param>
        /// <returns> Returns the offset of the given port from the top of the ports </returns>
        internal double GetPortVerticalOffset(PortModel portModel)
        {
            double verticalOffset = 2.9;
            PortType portType;
            int index = GetPortIndexAndType(portModel, out portType);

            //If the port was not found, then it should have just been deleted. Return from function
            if (index == -1)
                return verticalOffset;

            double portHeight = portModel.Height;

            if (portType == PortType.INPUT)
            {
                for (int i = 0; i < index; i++)
                    verticalOffset += inPorts[i].MarginThickness.Top + portHeight;
                verticalOffset += inPorts[index].MarginThickness.Top;
            }
            else if (portType == PortType.OUTPUT)
            {
                for (int i = 0; i < index; i++)
                    verticalOffset += outPorts[i].MarginThickness.Top + portHeight;
                verticalOffset += outPorts[index].MarginThickness.Top;
            }

            return verticalOffset;
        }

        /// <summary>
        ///     Reads inputs list and adds ports for each input.
        /// </summary>
        public void RegisterInputPorts()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                PortModel port = AddPort(PortType.INPUT, pd, count);
                
                //MVVM: AddPort now returns a port model. You can't set the data context here.
                //port.DataContext = this;

                portDataDict[port] = pd;
                count++;            
            }

            if (inPorts.Count > count)
            {
                foreach (PortModel inport in inPorts.Skip(count))
                {
                    inport.DestroyConnectors();
                    portDataDict.Remove(inport);
                }

                for (int i = inPorts.Count - 1; i >= count; i--)
                    inPorts.RemoveAt(i);
            }

            //Configure Snap Edges
            ConfigureSnapEdges(inPorts);
        }

        /// <summary>
        ///     Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutputPorts()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in OutPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                PortModel port = AddPort(PortType.OUTPUT, pd, count);

                //MVVM : don't set the data context in the model
                //port.DataContext = this;

                portDataDict[port] = pd;
                count++;              
            }

            if (outPorts.Count > count)
            {
                foreach (PortModel outport in outPorts.Skip(count))
                    outport.DestroyConnectors();

                for (int i = outPorts.Count - 1; i >= count; i--)
                    outPorts.RemoveAt(i);

                //OutPorts.RemoveRange(count, outPorts.Count - count);
            }

            //configure snap edges
            ConfigureSnapEdges(outPorts);
        }

        /// <summary>
        /// Configures the snap edges.
        /// </summary>
        /// <param name="ports">The ports.</param>
        private void ConfigureSnapEdges(ObservableCollection<PortModel> ports)
        {
            if (ports.Count == 1) //only one port
                ports[0].extensionEdges = SnapExtensionEdges.Top | SnapExtensionEdges.Bottom;
            else if (ports.Count == 2) //has two ports
            {
                ports[0].extensionEdges = SnapExtensionEdges.Top;
                ports[1].extensionEdges = SnapExtensionEdges.Bottom;
            }
            else if (ports.Count > 1)
            {
                ports[0].extensionEdges = SnapExtensionEdges.Top;
                ports[ports.Count - 1].extensionEdges = SnapExtensionEdges.Bottom;
                foreach (PortModel port in ports)
                {
                    if (!port.extensionEdges.HasFlag(SnapExtensionEdges.Top)
                        && !port.extensionEdges.HasFlag(SnapExtensionEdges.Bottom))
                        port.extensionEdges = SnapExtensionEdges.None;
                }
            } 
        }

        /// <summary>
        ///     Updates UI so that all ports reflect current state of InPortData and OutPortData.
        /// </summary>
        public void RegisterAllPorts()
        {
            RegisterInputPorts();
            RegisterOutputPorts();
            ValidateConnections();
        }

        /// <summary>
        ///     Add a port to this node. If the port already exists, return that port.
        /// </summary>
        /// <param name="portType"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public PortModel AddPort(PortType portType, PortData data, int index)
        {
            PortModel p;
            switch (portType)
            {
                case PortType.INPUT:
                    if (inPorts.Count > index)
                    {
                        p = inPorts[index];

                        //update the name on the node
                        //e.x. when the node is being re-registered during a custom
                        //node save
                        p.PortName = data.NickName;
                        if (data.HasDefaultValue)
                        {
                            p.UsingDefaultValue = true;
                            p.DefaultValueEnabled = true;
                        }

                        return p;
                    }

                    p = new PortModel(portType, this, data)
                    {
                        UsingDefaultValue = data.HasDefaultValue,
                        DefaultValueEnabled = data.HasDefaultValue
                    };

                    p.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == "UsingDefaultValue")
                            RequiresRecalc = true;
                    };

                    InPorts.Add(p);

                    //register listeners on the port
                    p.PortConnected += p_PortConnected;
                    p.PortDisconnected += p_PortDisconnected;
                    
                   
                    return p;

                case PortType.OUTPUT:
                    if (outPorts.Count > index)
                    {
                        p = outPorts[index];
                        p.PortName = data.NickName;
                        p.MarginThickness = new Thickness(0, data.VerticalMargin, 0, 0);
                        return p;
                    }

                    p = new PortModel(portType, this, data)
                    {
                        UsingDefaultValue = false,
                        MarginThickness = new Thickness(0, data.VerticalMargin, 0, 0)
                    };

                    OutPorts.Add(p);

                    //register listeners on the port
                    p.PortConnected += p_PortConnected;
                    p.PortDisconnected += p_PortDisconnected;

                    return p;
            }

            return null;
        }

      
        private void p_PortConnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (PortModel)sender;
            if (port.PortType == PortType.INPUT)
            {
                int data = InPorts.IndexOf(port);
                PortModel startPort = port.Connectors[0].Start;
                int outData = startPort.Owner.OutPorts.IndexOf(startPort);
                ConnectInput(data, outData, startPort.Owner);
                startPort.Owner.ConnectOutput(outData, data, this);
            }
        }

        private void p_PortDisconnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (PortModel)sender;
            if (port.PortType == PortType.INPUT)
            {
                int data = InPorts.IndexOf(port);
                PortModel startPort = port.Connectors[0].Start;
                DisconnectInput(data);
                startPort.Owner.DisconnectOutput(startPort.Owner.OutPorts.IndexOf(startPort), data, this);
            }
        }

        #endregion

        #endregion

        #region Code Serialization

        /// <summary>
        ///     Creates a Scheme representation of this dynNode and all connected dynNodes.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            string nick = NickName.Replace(' ', '_');

            if (!Enumerable.Range(0, InPortData.Count).Any(HasInput))
                return nick;

            string s = "";

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                s += "(" + nick;
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (int data in Enumerable.Range(0, InPortData.Count))
                {
                    Tuple<int, NodeModel> input;
                    TryGetInput(data, out input);
                    s += " " + input.Item2.PrintExpression();
                }
                s += ")";
            }
            else
            {
                s += "(lambda (" + string.Join(" ", InPortData.Where((_, i) => !HasInput(i)).Select(x => x.NickName))
                     + ") (" + nick;
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (int data in Enumerable.Range(0, InPortData.Count))
                {
                    s += " ";
                    Tuple<int, NodeModel> input;
                    if (TryGetInput(data, out input))
                        s += input.Item2.PrintExpression();
                    else
                        s += InPortData[data].NickName;
                }
                s += "))";
            }

            return s;
        }

        #endregion

        #region ISelectable Interface

        public override void Deselect()
        {
            ValidateConnections();
            IsSelected = false;
        }

        #endregion

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "NickName")
            {
                NickName = value;
                return true;
            }

            if (name == "UsingDefaultValue")
            {
                if (string.IsNullOrWhiteSpace(value))
                    return true;

                // Here we expect a string that represents an array of Boolean values which are separated by ";"
                var arr = value.Split(';');
                for (int i = 0; i < arr.Length; i++)
                {
                    InPorts[i].UsingDefaultValue = !bool.Parse(arr[i]);
                }
                return true;
            }

            if (name == "ArgumentLacing")
            {
                LacingStrategy strategy = LacingStrategy.Disabled;
                if (!Enum.TryParse(value, out strategy))
                    strategy = LacingStrategy.Disabled;
                this.ArgumentLacing = strategy;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);

            // Set the type attribute
            helper.SetAttribute("type", GetType().ToString());
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("nickname", NickName);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
            helper.SetAttribute("isVisible", IsVisible);
            helper.SetAttribute("isUpstreamVisible", IsUpstreamVisible);
            helper.SetAttribute("lacing", ArgumentLacing.ToString());

            if (context == SaveContext.Undo)
            {
                // Fix: MAGN-159 (nodes are not editable after undo/redo).
                helper.SetAttribute("interactionEnabled", interactionEnabled);
                helper.SetAttribute("nodeState", state.ToString());
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            GUID = helper.ReadGuid("guid", Guid.NewGuid());

            // Resolve node nick name.
            string nickName = helper.ReadString("nickname", string.Empty);
            if (!string.IsNullOrEmpty(nickName))
                this.nickName = nickName;
            else
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), true);
                var attrib = attribs[0] as NodeNameAttribute;
                if (null != attrib)
                    this.nickName = attrib.Name;
            }

            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);
            isVisible = helper.ReadBoolean("isVisible", true);
            isUpstreamVisible = helper.ReadBoolean("isUpstreamVisible", true);
            argumentLacing = helper.ReadEnum("lacing", LacingStrategy.Disabled);

            if (context == SaveContext.Undo)
            {
                // Fix: MAGN-159 (nodes are not editable after undo/redo).
                interactionEnabled = helper.ReadBoolean("interactionEnabled", true);
                state = helper.ReadEnum("nodeState", ElementState.Active);

                // We only notify property changes in an undo/redo operation. Normal
                // operations like file loading or copy-paste have the models created
                // in different ways and their views will always be up-to-date with 
                // respect to their models.
                RaisePropertyChanged("InteractionEnabled");
                RaisePropertyChanged("State");
                RaisePropertyChanged("NickName");
                RaisePropertyChanged("ArgumentLacing");
                RaisePropertyChanged("IsVisible");
                RaisePropertyChanged("IsUpstreamVisible");

                // Notify listeners that the position of the node has changed,
                // then all connected connectors will also redraw themselves.
                ReportPosition();
            }
        }

        #endregion

        #region Dirty Management

        //TODO: Refactor Property into Automatic with private(?) setter
        //TODO: Add RequestRecalc() method to replace setter --steve

        private bool dirty = true;

        /// <summary>
        ///     Does this Element need to be regenerated? Setting this to true will trigger a modification event
        ///     for the dynWorkspace containing it. If Automatic Running is enabled, setting this to true will
        ///     trigger an evaluation.
        /// </summary>
        public bool RequiresRecalc 
        {
            get { return dirty; }
            set
            {
                dirty = value;
                if (dirty)
                    ReportModification();
            } 
        }

        private bool forceReExec = false;

        /// <summary>
        ///     This property forces all AST nodes that generated from this node
        ///     to be executed, even there is no change in AST nodes.
        /// </summary>
        public virtual bool ForceReExecuteOfNode
        {
            get
            {
                return forceReExec;
            }
            set
            {
                forceReExec = value;
                RaisePropertyChanged("ForceReExecuteOfNode");
            }
        }
        #endregion

        #region Visualization Related Methods

#if ENABLE_DYNAMO_SCHEDULER

        /// <summary>
        /// Call this method to asynchronously update the cached MirrorData for 
        /// this NodeModel through DynamoScheduler. AstIdentifierForPreview is 
        /// being accessed within this method, therefore the method is typically
        /// called from the main/UI thread.
        /// </summary>
        /// 
        public void RequestValueUpdateAsync()
        {
            // A NodeModel should have its cachedMirrorData reset when it is 
            // requested to update its value. When the QueryMirrorDataAsyncTask 
            // returns, it will update cachedMirrorData with the latest value.
            // 
            lock (cachedMirrorDataMutex)
            {
                cachedMirrorData = null;
            }

            // Do not have an identifier for preview right now. For an example,
            // this can be happening at the beginning of a code block node creation.
            var variableName = AstIdentifierForPreview.Value;
            if (string.IsNullOrEmpty(variableName))
                return;

            var scheduler = Workspace.DynamoModel.Scheduler;
            var task = new QueryMirrorDataAsyncTask(new QueryMirrorDataParams()
            {
                DynamoScheduler = scheduler,
                EngineController = Workspace.DynamoModel.EngineController,
                VariableName = variableName
            });

            task.Completed += OnNodeValueQueried;
            scheduler.ScheduleForExecution(task);
        }

        private void OnNodeValueQueried(AsyncTask asyncTask)
        {
            lock (cachedMirrorDataMutex)
            {
                var task = asyncTask as QueryMirrorDataAsyncTask;
                cachedMirrorData = task.MirrorData;
            }

            RaisePropertyChanged("IsUpdated");
        }

        /// <summary>
        /// Call this method to asynchronously regenerate render package for 
        /// this node. This method accesses core properties of a NodeModel and 
        /// therefore is typically called on the main/UI thread.
        /// </summary>
        /// <param name="maxTesselationDivisions">The maximum number of 
        /// tessellation divisions to use for regenerating render packages.</param>
        /// 
        public void RequestVisualUpdateAsync(int maxTesselationDivisions)
        {
            if (Workspace.DynamoModel == null)
                return;

            // Imagine a scenario where "NodeModel.RequestVisualUpdateAsync" is being 
            // called in quick succession from the UI thread -- the first task may 
            // be updating '_renderPackages' when the second call gets here. In 
            // this case '_renderPackages' should be protected against concurrent 
            // accesses.
            // 
            lock (RenderPackagesMutex)
            {
                _renderPackages.Clear();
                HasRenderPackages = false;
            }

            RequestVisualUpdateAsyncCore(maxTesselationDivisions);
        }

        /// <summary>
        /// When called, the base implementation of this method schedules an 
        /// UpdateRenderPackageAsyncTask to regenerate its render packages 
        /// asynchronously. Derived classes can optionally override this method 
        /// to prevent render packages to be generated if they do not require 
        /// geometric preview.
        /// </summary>
        /// <param name="maxTesselationDivisions">The maximum number of 
        /// tessellation divisions to use for regenerating render packages.</param>
        /// 
        protected virtual void RequestVisualUpdateAsyncCore(int maxTesselationDivisions)
        {
            var initParams = new UpdateRenderPackageParams()
            {
                Node = this,
                MaxTesselationDivisions = maxTesselationDivisions,
                EngineController = Workspace.DynamoModel.EngineController,
                DrawableIds = GetDrawableIds(),
                PreviewIdentifierName = AstIdentifierForPreview.Name
            };

            var scheduler = Workspace.DynamoModel.Scheduler;
            var task = new UpdateRenderPackageAsyncTask(scheduler);
            if (task.Initialize(initParams))
            {
                task.Completed += OnRenderPackageUpdateCompleted;
                scheduler.ScheduleForExecution(task);
            }
        }

        /// <summary>
        /// This event handler is invoked when UpdateRenderPackageAsyncTask is 
        /// completed, at which point the render packages (specific to this node) 
        /// become available. Since this handler is called off the UI thread, the 
        /// '_renderPackages' must be guarded against concurrent access.
        /// </summary>
        /// <param name="asyncTask">The instance of UpdateRenderPackageAsyncTask
        /// that was responsible of generating the render packages.</param>
        /// 
        private void OnRenderPackageUpdateCompleted(AsyncTask asyncTask)
        {
            lock (RenderPackagesMutex)
            {
                var task = asyncTask as UpdateRenderPackageAsyncTask;
                _renderPackages.Clear();
                _renderPackages.AddRange(task.RenderPackages);
                HasRenderPackages = _renderPackages.Any();
            }
        }

#else

        /// <summary>
        /// Updates the render package for this node by
        /// getting the MirrorData objects corresponding to
        /// each of the node's ports and processing the underlying
        /// CLR data as IGraphicItems.
        /// </summary>
        public virtual void UpdateRenderPackage(int maxTesselationDivisions)
        {
            if (Workspace.DynamoModel == null)
            {
                return;
            }

            ClearRenderPackages();
            if (State == ElementState.Error ||
                !IsVisible ||
                CachedValue == null)
            {
                return;
            }

            var drawableIds = GetDrawableIds();

            int count = 0;
            var labelMap = new List<string>();
            var sizeMap = new List<double>();

            var ident = AstIdentifierForPreview.Name;

            var data = from varName in drawableIds
                        select Workspace.DynamoModel.EngineController.GetMirror(varName)
                        into mirror
                        where mirror != null
                        select mirror.GetData();

            foreach (var mirrorData in data) 
            {
                AddToLabelMap(mirrorData, labelMap, ident);
                count++;
            }

            count = 0;
            List<IRenderPackage> newRenderPackages = new List<IRenderPackage>();
            foreach (var varName in drawableIds)
            {
                var graphItems = Workspace.DynamoModel.EngineController.GetGraphicItems(varName);
                if (graphItems == null)
                    continue;

                foreach (var gItem in graphItems)
                {
                    var package = new RenderPackage(IsSelected, DisplayLabels);
                        
                    PushGraphicItemIntoPackage(gItem, 
                        package, 
                        labelMap.Count > count ? labelMap[count] : "?",
                        sizeMap.Count > count ? sizeMap[count] : -1.0,
                        maxTesselationDivisions );

                    package.ItemsCount++;
                    newRenderPackages.Add(package);
                    count++;
                }
            }

            RenderPackages = newRenderPackages;
            if (RenderPackages.Any())
            {
                HasRenderPackages = true;
            }
            else
            {
                HasRenderPackages = false;
            }
        }

        private void ClearRenderPackages()
        {
            lock (RenderPackagesMutex)
            {
                RenderPackages.Clear();
                HasRenderPackages = false;
            }
        }

        private void PushGraphicItemIntoPackage(IGraphicItem graphicItem, IRenderPackage package, string tag, 
            double size, int maxTesselationDivisions )
        {
            try
            {
                graphicItem.Tessellate(package, -1.0, maxTesselationDivisions);
                package.Tag = tag;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("PushGraphicItemIntoPackage: " + e);
            }
        }

        /// <summary>
        /// Add labels for each of a mirror data object's inner
        /// data object to a label map.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="map"></param>
        /// <param name="tag"></param>
        private void AddToLabelMap(MirrorData data, List<string> map, string tag)
        {
            if (data.IsCollection)
            {
                var list = data.GetElements();
                for (int i = 0; i < list.Count; i++)
                {
                    AddToLabelMap(list[i], map, string.Format("{0}:{1}", tag, i));
                }
            }
            else if (data.Data is IEnumerable)
            {
                var list = data.Data as IEnumerable;
                AddToLabelMap(list, map, tag);
            }
            else
            {
                map.Add(tag);
            }
        }

        /// <summary>
        /// Add labels for each object in an enumerable 
        /// too a label map
        /// </summary>
        /// <param name="list"></param>
        /// <param name="map"></param>
        /// <param name="tag"></param>
        private static void AddToLabelMap(IEnumerable list, List<string> map, string tag)
        {
            int count = 0;
            foreach(var obj in list)
            {
                var newTag = string.Format("{0}:{1}", tag, count);

                if (obj is IEnumerable)
                {
                    AddToLabelMap(obj as IEnumerable, map, newTag);
                }
                else
                {
                    map.Add(newTag);
                }
                count++;
            }
        }

        private void AddToSizeMap(MirrorData data, ICollection<double> map)
        {
            if (data.IsCollection)
            {
                var list = data.GetElements();
                foreach (MirrorData t in list)
                {
                    AddToSizeMap(t, map);
                }
            }
            else if (data.Data is IEnumerable)
            {
                var list = data.Data as IEnumerable;
                AddToSizeMap(list, map);
            }
            else
            {
                map.Add(ComputeBBoxDiagonalSize(data.Data));
            }
        }

        private static void AddToSizeMap(IEnumerable list, ICollection<double> map)
        {
            foreach (var obj in list)
            {
                if (obj is IEnumerable)
                {
                    AddToSizeMap(obj as IEnumerable, map);
                }
                else
                {
                    map.Add(ComputeBBoxDiagonalSize(obj));
                }
            }
        }

        private static double ComputeBBoxDiagonalSize(object obj)
        {
            var size = -1.0;

            var entity = obj as Geometry;
            if (entity != null)
            {
                size = entity.BoundingBox.MinPoint.DistanceTo(entity.BoundingBox.MaxPoint);
            }
            return size;
        }

#endif

        /// <summary>
        /// Gets list of drawable Ids as registered with visualization manager 
        /// for all the output port of the given node.
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>List of Drawable Ids</returns>
        private IEnumerable<string> GetDrawableIds()
        {
            var drawables = new List<String>();
            for (int i = 0; i < OutPortData.Count; ++i)
            {
                string identifier = GetDrawableId(i);
                if (!string.IsNullOrEmpty(identifier))
                    drawables.Add(identifier);
            }

            return drawables;
        }

        /// <summary>
        /// Gets the drawable Id as registered with visualization manager for
        /// the given output port on the given node.
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="outPortIndex">Output port index</param>
        /// <returns>Drawable Id</returns>
        private string GetDrawableId(int outPortIndex)
        {
            var output = GetAstIdentifierForOutputIndex(outPortIndex);
            if (output == null)
                return null;

            return output.ToString();
        }

        #endregion

        #region Node Migration Helper Methods

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string nickname, string funcName)
        {
            return MigrateToDsFunction(data, "", nickname, funcName);
        }

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string assembly, string nickname, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", nickname);
            element.SetAttribute("function", funcName);

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        protected static NodeMigrationData MigrateToDsVarArgFunction(
            NodeMigrationData data, string assembly, string nickname, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateVarArgFunctionNodeFrom(xmlNode);
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", nickname);
            element.SetAttribute("function", funcName);

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        #endregion

        public event EventHandler BlockingStarted;
        public virtual void OnBlockingStarted(EventArgs e)
        {
            if (BlockingStarted != null)
            {
                BlockingStarted(this, e);
            }
        }

        public event EventHandler BlockingEnded;
        public virtual void OnBlockingEnded(EventArgs e)
        {
            if (BlockingEnded != null)
            {
                BlockingEnded(this, e);
            }
        }

        public bool ShouldDisplayPreview
        {
            get
            {
                // Previews are only shown in Home Workspace.
                if (!(this.Workspace is HomeWorkspaceModel))
                    return false;

                return this.ShouldDisplayPreviewCore;
            }
        }

        protected virtual bool ShouldDisplayPreviewCore
        {
            get
            {
                return true; // Default implementation: always show preview.
            } 
        }
      
    }

    public enum ElementState
    {
        Dead,
        Active,
        Warning,
        Error,
        AstBuildBroken
    };


    public enum LacingStrategy
    {
        Disabled,
        First,
        Shortest,
        Longest,
        CrossProduct
    };

    public enum PortEventType
    {
        MouseEnter,
        MouseLeave,
        MouseLeftButtonDown
    };

    public enum PortPosition
    {
        First,
        Top,
        Middle,
        Last
    }
    [Flags]
    public enum SnapExtensionEdges
    {
        None,
        Top = 0x1,
        Bottom = 0x2
    }


    public delegate void PortsChangedHandler(object sender, EventArgs e);

    public delegate void DispatchedToUIThreadHandler(object sender, UIDispatcherEventArgs e);

    #region class attributes

    [AttributeUsage(AttributeTargets.All)]
    public class NodeNameAttribute : Attribute
    {
        public NodeNameAttribute(string elementName)
        {
            Name = elementName;
        }

        public string Name { get; set; }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : Attribute
    {
        public NodeCategoryAttribute(string category)
        {
            ElementCategory = category;
        }

        public string ElementCategory { get; set; }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchTagsAttribute : Attribute
    {
        public NodeSearchTagsAttribute(params string[] tags)
        {
            Tags = tags.ToList();
        }

        public List<string> Tags { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class NotSearchableInHomeWorkspace : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class NotSearchableInCustomNodeWorkspace : Attribute
    { }

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class IsInteractiveAttribute : Attribute
    {
        public IsInteractiveAttribute(bool isInteractive)
        {
            IsInteractive = isInteractive;
        }

        public bool IsInteractive { get; set; }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class NodeDescriptionAttribute : Attribute
    {
        public NodeDescriptionAttribute(string description)
        {
            ElementDescription = description;
        }

        public string ElementDescription { get; set; }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchableAttribute : Attribute
    {
        public NodeSearchableAttribute(bool isSearchable)
        {
            IsSearchable = isSearchable;
        }

        public bool IsSearchable { get; set; }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class NodeTypeIdAttribute : Attribute
    {
        public NodeTypeIdAttribute(string description)
        {
            Id = description;
        }

        public string Id { get; set; }
    }


    /// <summary>
    ///     The DoNotLoadOnPlatforms attribute allows the node implementor
    ///     to define an array of contexts in which the node will not
    ///     be loaded.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DoNotLoadOnPlatformsAttribute : Attribute
    {
        public DoNotLoadOnPlatformsAttribute(params string[] values)
        {
            Values = values;
        }

        public string[] Values { get; set; }
    }


    /// <summary>
    ///     Flag to hide deprecated nodes in search, but allow in workflows
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class NodeDeprecatedAttribute : Attribute { }

    /// <summary>
    ///     The AlsoKnownAs attribute allows the node implementor to
    ///     define an array of names that this node might have had
    ///     in the past.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AlsoKnownAsAttribute : Attribute
    {
        public AlsoKnownAsAttribute(params string[] values)
        {
            Values = values;
        }

        public string[] Values { get; set; }
    }


    /// <summary>
    ///     The MetaNode attribute means this node shouldn't be added to the category,
    ///     only its instances are allowed
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class IsMetaNodeAttribute : Attribute { }


    /// <summary>
    ///     The IsDesignScriptCompatibleAttribute indicates if the node is able
    ///     to work with DesignScript evaluation engine.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class IsDesignScriptCompatibleAttribute : Attribute { }

    #endregion
    
    public class UIDispatcherEventArgs : EventArgs
    {
        public UIDispatcherEventArgs(Action a)
        {
            ActionToDispatch = a;
        }

        public Action ActionToDispatch { get; set; }
        public List<object> Parameters { get; set; }
    }
}

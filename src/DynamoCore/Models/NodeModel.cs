using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.FSchemeInterop;
using Dynamo.Nodes;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using String = System.String;
using StringNode = ProtoCore.AST.AssociativeAST.StringNode;
using Utils = Dynamo.FSchemeInterop.Utils;

namespace Dynamo.Models
{
    public abstract class NodeModel : ModelBase
    {
        #region abstract members

        /// <summary>
        ///     The dynElement's Evaluation Logic.
        /// </summary>
        /// <param name="args">
        ///     Parameters to the node. You are guaranteed to have as many arguments as you have InPorts at the time
        ///     it is run.
        /// </param>
        /// <param name="outPuts"></param>
        /// <returns>
        ///     An expression that is the result of the Node's evaluation. It will be passed along to whatever the OutPort is
        ///     connected to.
        /// </returns>
        public virtual void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private members

        private bool overrideNameWithNickName;
        private LacingStrategy argumentLacing = LacingStrategy.First;
        private bool displayLabels;
        private ObservableCollection<PortModel> inPorts = new ObservableCollection<PortModel>();
        private bool interactionEnabled = true;
        private bool isUpstreamVisible;
        private bool isVisible;
        private string nickName;
        private ObservableCollection<PortModel> outPorts = new ObservableCollection<PortModel>();
        private ElementState state;
        private string toolTipText = "";
        private IdentifierNode identifier;
        private bool saveResult;
        private bool isUpdated;
        private string description;
        private const string FailureString = "Node evaluation failed";
        private readonly Dictionary<PortModel, PortData> portDataDict = new Dictionary<PortModel, PortData>();
        private int errorCount;

        private List<IRenderPackage> _renderPackages = new List<IRenderPackage>();

        #endregion

        #region public members

        // TODO(Ben): Move this up to ModelBase (it makes sense for connector as well).
        public WorkspaceModel WorkSpace;

        public Dictionary<int, Tuple<int, NodeModel>> Inputs = new Dictionary<int, Tuple<int, NodeModel>>();

        public Dictionary<int, HashSet<Tuple<int, NodeModel>>> Outputs =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

        private Object mutex = new object();
        public List<IRenderPackage> RenderPackages
        {
            get { return _renderPackages; }
            set
            {
                lock (mutex)
                {
                    _renderPackages = value;
                    RaisePropertyChanged("RenderPackages");
                } 
            }
        }

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
                if (value != ElementState.Error)
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

        /// <summary>
        ///     The value which was produced for this node during the previous evaluation.
        /// </summary>
        public virtual MirrorData OldValue
        {
            get
            {
                var mirrorData = dynSettings.Controller.EngineController.GetMirror(AstIdentifierForPreview.Value);
                return mirrorData == null ? null : mirrorData.GetData();
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
                RaisePropertyChanged("IsUpdated");
            }
        }

        /// <summary>
        ///     Return a variable whose value will be displayed in preview window.
        ///     Derived nodes may overwrite this function to display default value
        ///     of this node. E.g., code block node may want to display the value
        ///     of the left hand side variable of last statement.
        /// </summary>
        /// <returns></returns>
        public virtual string VariableToPreview
        {
            get
            {
                IdentifierNode ident = AstIdentifierForPreview;
                return (ident == null) ? null : ident.Name;
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
        internal IdentifierNode AstIdentifierForPreview
        {
            get
            {
                if (identifier == null)
                {
                    string id = AstIdentifierBase;
                    identifier = new IdentifierNode { Name = id, Value = id };
                }
                return identifier;
            }
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
        ///     Base name for ProtoAST Identifiers corresponding to this node's output.
        /// </summary>
        protected string AstIdentifierBase
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
        ///     Flags this node as dirty.
        /// </summary>
        [Obsolete("Use RequiresRecalc = true")]
        public void ResetOldValue()
        {
            RequiresRecalc = true;
        }

        /// <summary>
        ///     Get the description from type information
        /// </summary>
        /// <returns>The value or "No description provided"</returns>
        public string GetDescriptionStringFromAttributes()
        {
            Type t = GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
            if (rtAttribs.Length > 0)
                return ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;

            return "No description provided";
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

            if (OutPortData.Count == 1)
                return AstIdentifierForPreview;

            string nameAndValue = AstIdentifierBase + "[" + outputIndex + "]";

            return new IdentifierNode { Name = nameAndValue, Value = nameAndValue };
        }

        #endregion

        protected NodeModel()
        {
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
        ///     Called when this node is being removed from the workspace.
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        ///     Implement on derived classes to cleanup resources when
        /// </summary>
        public virtual void Cleanup() { }

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
            if (IsReportingModifications && WorkSpace != null)
                WorkSpace.Modified();
        }

        #endregion

        #region Load/Save

        /// <summary>
        ///     Override this to implement custom save data for your Element. If overridden, you should also override
        ///     LoadNode() in order to read the data back when loaded.
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole workspace containing this Element.</param>
        /// <param name="nodeElement">The XmlElement representing this Element.</param>
        /// <param name="context">Why is this being called?</param>
        protected virtual void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context) { }

        /// <summary>
        ///     Saves this node into an XML Document.
        /// </summary>
        /// <param name="xmlDoc">Overall XmlDocument representing the entire workspace being saved.</param>
        /// <param name="dynEl">The XmlElement representing this node in the workspace.</param>
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
        ///     Called when the node's workspace has been saved.
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

            var result = BuildOutputAst(inputAstNodes);

            /*
            var functionDef = new FunctionDefinitionNode
            {
                Name = AstBuilder.StringConstants.FunctionPrefix + GUID.ToString().Replace("-", string.Empty),
                Signature = new ArgumentSignatureNode { Arguments = InPortData.Select(x => AstFactory.BuildParamNode(x.NickName)).ToList() },
                FunctionBody = 
            };
            */

            if (OutPortData.Count == 1)
            {
                return
                    result.Concat(
                        new[]
                        {
                            AstFactory.BuildAssignment(
                                AstIdentifierForPreview,
                                GetAstIdentifierForOutputIndex(0))
                        });
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

        /// <summary>
        ///     Called back from the view to enable users to setup their own view elements
        /// </summary>
        /// <param name="view"></param>
        internal void InitializeUI(dynamic view)
        {
            //Runtime dispatch
            (this as dynamic).SetupCustomUIElements(view);
        }

        /// <summary>
        /// Used as a catch-all if runtime dispatch for UI initialization is unimplemented.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void SetupCustomUIElements(object view) { }

        private void ClearTooltipText()
        {
            ToolTipText = "";
        }

        protected void ClearError()
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

        internal void DisableInteraction()
        {
            State = ElementState.Dead;
            InteractionEnabled = false;
        }

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
                        State = ElementState.Dead;
                    else if (State == ElementState.Dead)
                        State = ElementState.Active;
                });

            if (dynSettings.Controller != null &&
                dynSettings.Controller.UIDispatcher != null &&
                dynSettings.Controller.UIDispatcher.CheckAccess() == false)
            {
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
                dynSettings.Controller.UIDispatcher.BeginInvoke(setState);
            }
            else
                setState();
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

            if (portType == PortType.INPUT)
            {
                for (int i = 0; i < index; i++)
                    verticalOffset += inPorts[i].MarginThickness.Top + 20;
                verticalOffset += inPorts[index].MarginThickness.Top;
            }
            else if (portType == PortType.OUTPUT)
            {
                for (int i = 0; i < index; i++)
                    verticalOffset += outPorts[i].MarginThickness.Top + 20;
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

                    p = new PortModel(portType, this, data.NickName)
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

                    p = new PortModel(portType, this, data.NickName)
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

        public static string PrintValue(
            string variableName,
            int currentListIndex,
            int maxListIndex,
            int currentDepth,
            int maxDepth,
            int maxStringLength = 20)
        {
            string previewValue = "<null>";
            if (!string.IsNullOrEmpty(variableName))
            {
                try
                {
                    previewValue = dynSettings.Controller.EngineController.GetStringValue(variableName);
                }
                catch (Exception ex)
                {
                    DynamoLogger.Instance.Log(ex.Message);
                }
            }
            return previewValue;
        }

        public static string PrintValue(
            FScheme.Value eIn,
            int currentListIndex,
            int maxListIndex,
            int currentDepth,
            int maxDepth,
            int maxStringLength = 20)
        {
            if (eIn == null)
                return "<null>";

            string accString = String.Concat(Enumerable.Repeat("  ", currentDepth));

            if (maxDepth == currentDepth || currentListIndex == maxListIndex)
            {
                accString += "...";
                return accString;
            }

            if (eIn.IsContainer)
            {
                string str = (eIn as FScheme.Value.Container).Item != null
                                 ? (eIn as FScheme.Value.Container).Item.ToString()
                                 : "<empty>";

                accString += str;
            }
            else if (eIn.IsFunction)
                accString += "<function>";
            else if (eIn.IsList)
            {
                accString += "List";

                FSharpList<FScheme.Value> list = (eIn as FScheme.Value.List).Item;

                if (!list.Any())
                    accString += " (empty)";

                // when children will be at maxDepth, just do 1
                if (currentDepth + 1 == maxDepth)
                    maxListIndex = 0;

                // build all elements of sub list
                accString =
                    list.Select((x, i) => new { Element = x, Index = i })
                        .TakeWhile(e => e.Index <= maxListIndex)
                        .Aggregate(
                            accString,
                            (current, e) =>
                            current + "\n"
                            + PrintValue(e.Element, e.Index, maxListIndex, currentDepth + 1, maxDepth, maxStringLength));
            }
            else if (eIn.IsNumber)
            {
                double num = (eIn as FScheme.Value.Number).Item;
                var numFloat = (float)num;
                accString += numFloat.ToString();
            }
            else if (eIn.IsString)
            {
                string str = (eIn as FScheme.Value.String).Item;
                if (str.Length > maxStringLength)
                    str = str.Substring(0, maxStringLength) + "...";

                accString += "\"" + str + "\"";
            }
            else if (eIn.IsSymbol)
                accString += "<" + (eIn as FScheme.Value.Symbol).Item + ">";

            return accString;
        }
        

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

        #region FScheme Compilation

        /// <summary>
        ///     Compiles this Element into a ProcedureCallNode. Override this instead of Build() if you don't want to set up all
        ///     of the inputs for the ProcedureCallNode.
        /// </summary>
        /// <param name="portNames">The names of the inputs to the node.</param>
        /// <returns>A ProcedureCallNode which will then be processed recursively to be connected to its inputs.</returns>
        protected virtual InputNode Compile(IEnumerable<string> portNames)
        {
            //Debug.WriteLine(string.Format("Compiling InputNode with ports {0}.", string.Join(",", portNames)));

            //Return a Function that calls eval.
            return new ExternalFunctionNode(evalIfDirty, portNames);
        }

        internal virtual INode BuildExpression(Dictionary<NodeModel, Dictionary<int, INode>> buildDict)
        {
            //Debug.WriteLine("Building expression...");

            if (OutPortData.Count > 1)
            {
                List<string> names =
                    OutPortData.Select(x => x.NickName)
                               .Zip(Enumerable.Range(0, OutPortData.Count), (x, i) => x + i)
                               .ToList();
                var listNode = new FunctionNode("list", names);
                foreach (
                    var data in
                        names.Zip(
                            Enumerable.Range(0, OutPortData.Count),
                            (name, index) => new { Name = name, Index = index }))
                    listNode.ConnectInput(data.Name, Build(buildDict, data.Index));
                return listNode;
            }
            return Build(buildDict, 0);
        }

        /// <summary>
        ///     Builds an INode out of this Element. Override this or Compile() if you want complete control over this Element's
        ///     execution.
        /// </summary>
        /// <returns>The INode representation of this Element.</returns>
        protected internal virtual INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            //Debug.WriteLine("Building node...");

            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result))
                return result[outPort];

            //Fetch the names of input ports.
            List<string> portNames =
                InPortData.Zip(Enumerable.Range(0, InPortData.Count), (x, i) => x.NickName + i).ToList();

            //Is this a partial application?
            bool partial = false;

            var connections = new List<Tuple<string, INode>>();
            var partialSymList = new List<string>();

            //For each index in InPortData
            foreach (
                var data in
                    Enumerable.Range(0, InPortData.Count)
                              .Zip(portNames, (data, name) => new { Index = data, Name = name }))
            {
                Tuple<int, NodeModel> input;

                //If this port has connectors...
                //if (port.Connectors.Any())
                if (TryGetInput(data.Index, out input))
                {
                    //Compile input and connect it
                    connections.Add(Tuple.Create(data.Name, input.Item2.Build(preBuilt, input.Item1)));
                }
                else if (InPorts[data.Index].UsingDefaultValue)
                {
                    connections.Add(
                        Tuple.Create(
                            data.Name,
                            new ValueNode(InPortData[data.Index].DefaultValue as FScheme.Value) as
                                INode));
                }
                else //othwise, remember that this is a partial application
                {
                    partial = true;
                    partialSymList.Add(data.Name);
                }
            }

            Dictionary<int, INode> nodes = OutPortData.Count == 1
                                               ? (partial
                                                      ? buildPartialSingleOut(portNames, connections, partialSymList)
                                                      : buildSingleOut(portNames, connections))
                                               : (partial
                                                      ? buildPartialMultiOut(portNames, connections, partialSymList)
                                                      : buildMultiOut(portNames, connections));

            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                //OldValue = FScheme.Value.NewFunction(null); // cache an old value for display to the user
                RequiresRecalc = false;
            }

            preBuilt[this] = nodes;

            //And we're done
            return nodes[outPort];
        }

        private Dictionary<int, INode> buildSingleOut(
            IEnumerable<string> portNames,
            IEnumerable<Tuple<string, INode>> connections)
        {
            InputNode node = Compile(portNames);

            foreach (var connection in connections)
                node.ConnectInput(connection.Item1, connection.Item2);

            return new Dictionary<int, INode> { { 0, node } };
        }

        private Dictionary<int, INode> buildMultiOut(
            IEnumerable<string> portNames,
            IEnumerable<Tuple<string, INode>> connections)
        {
            InputNode node = Compile(portNames);

            foreach (var connection in connections)
                node.ConnectInput(connection.Item1, connection.Item2);

            InputNode prev = node;

            return OutPortData.Select((d, i) => new { Index = i, Data = d }).ToDictionary(
                data => data.Index,
                data =>
                {
                    if (data.Index > 0)
                    {
                        var rest = new ExternalFunctionNode(FScheme.Cdr, new[] { "list" });
                        rest.ConnectInput("list", prev);
                        prev = rest;
                    }

                    var firstNode = new ExternalFunctionNode(FScheme.Car, new[] { "list" });
                    firstNode.ConnectInput("list", prev);
                    return firstNode as INode;
                });
        }

        private Dictionary<int, INode> buildPartialSingleOut(
            IEnumerable<string> portNames,
            List<Tuple<string, INode>> connections,
            List<string> partials)
        {
            InputNode node = Compile(portNames);

            foreach (string partial in partials)
                node.ConnectInput(partial, new SymbolNode(partial));

            var outerNode = new AnonymousFunctionNode(partials, node);
            if (connections.Any())
            {
                outerNode = new AnonymousFunctionNode(connections.Select(x => x.Item1), outerNode);
                foreach (var connection in connections)
                {
                    node.ConnectInput(connection.Item1, new SymbolNode(connection.Item1));
                    outerNode.ConnectInput(connection.Item1, connection.Item2);
                }
            }

            return new Dictionary<int, INode> { { 0, outerNode } };
        }

        private Dictionary<int, INode> buildPartialMultiOut(
            IEnumerable<string> portNames,
            List<Tuple<string, INode>> connections,
            List<string> partials)
        {
            return OutPortData.Select((d, i) => new { Index = i, Data = d }).ToDictionary(
                data => data.Index,
                data =>
                {
                    InputNode node = Compile(portNames);

                    foreach (string partial in partials)
                        node.ConnectInput(partial, new SymbolNode(partial));

                    var accessor = new ExternalFunctionNode(FScheme.Get, new[] { "idx", "list" });
                    accessor.ConnectInput("list", node);
                    accessor.ConnectInput("idx", new NumberNode(data.Index));

                    var outerNode = new AnonymousFunctionNode(partials, accessor);
                    if (connections.Any())
                    {
                        outerNode = new AnonymousFunctionNode(connections.Select(x => x.Item1), outerNode);
                        foreach (var connection in connections)
                        {
                            node.ConnectInput(connection.Item1, new SymbolNode(connection.Item1));
                            outerNode.ConnectInput(connection.Item1, connection.Item2);
                        }
                    }

                    return outerNode as INode;
                });
        }

        #endregion

        #region FScheme Evaluation

        /// <summary>
        ///     Wraps node evaluation logic so that it can be called in different threads.
        /// </summary>
        /// <returns>Some(Value) -> Result | None -> Run was cancelled</returns>
        private delegate FSharpOption<FScheme.Value> InnerEvaluationDelegate();

        private FScheme.Value evalIfDirty(FSharpList<FScheme.Value> args)
        {
            //// should I re-evaluate?
            //if (OldValue == null || !SaveResult || RequiresRecalc)
            //{
            //    // re-evaluate
            //    FScheme.Value result = evaluateNode(args);

            //    // if it was a failure, the old value is null
            //    if (result.IsString && (result as FScheme.Value.String).Item == FailureString)
            //        OldValue = null;
            //    else // cache the old value
            //        OldValue = result;
            //}
            ////else
            ////    OnEvaluate();

            return OldValue.Data as FScheme.Value;
        }

        public MirrorData GetValue(int outPortIndex)
        {
            return dynSettings.Controller.EngineController.GetMirror(
                GetAstIdentifierForOutputIndex(outPortIndex).Value).GetData();
        }

        protected internal virtual FScheme.Value evaluateNode(FSharpList<FScheme.Value> args)
        {
            //Debug.WriteLine("Evaluating node...");

            var evalDict = new Dictionary<PortData, FScheme.Value>();

            object[] iaAttribs = GetType().GetCustomAttributes(typeof(IsInteractiveAttribute), false);
            bool isInteractive = iaAttribs.Length > 0 && ((IsInteractiveAttribute)iaAttribs[0]).IsInteractive;

            InnerEvaluationDelegate evaluation = delegate
            {
                FScheme.Value expr = null;

                try
                {
                    if (dynSettings.Controller.RunCancelled)
                        throw new CancelEvaluationException(false);


                    __eval_internal(args, evalDict);

                    expr = OutPortData.Count == 1
                               ? evalDict[OutPortData[0]]
                               : FScheme.Value.NewList(
                                   Utils.ToFSharpList(
                                       evalDict.OrderBy(pair => OutPortData.IndexOf(pair.Key))
                                               .Select(pair => pair.Value)));

                    ValidateConnections();
                }
                catch (CancelEvaluationException)
                {
                    OnRunCancelled();
                    return FSharpOption<FScheme.Value>.None;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                    DynamoLogger.Instance.Log(ex);

                    if (dynSettings.Controller.DynamoModel.CanWriteToLog(null))
                    {
                        dynSettings.Controller.DynamoModel.WriteToLog(ex.Message);
                        dynSettings.Controller.DynamoModel.WriteToLog(ex.StackTrace);
                    }

                    //Controller.DynamoViewModel.ShowElement(this); // not good if multiple nodes are in error state

                    Error(ex.Message);

                    if (DynamoController.IsTestMode)
                        throw new Exception(ex.Message);

                    errorCount++;
                }


                RequiresRecalc = false;

                return FSharpOption<FScheme.Value>.Some(expr);
            };

            //C# doesn't have a Option type, so we'll just borrow F#'s instead.
            FSharpOption<FScheme.Value> result = isInteractive && dynSettings.Controller.UIDispatcher != null
                                                     ? (FSharpOption<FScheme.Value>)
                                                       dynSettings.Controller.UIDispatcher.Invoke(evaluation)
                                                     : evaluation();

            if (result == FSharpOption<FScheme.Value>.None)
                throw new CancelEvaluationException(false);

            return result.Value ?? FScheme.Value.NewString(FailureString);
        }

        protected virtual void OnRunCancelled() { }

        protected virtual void __eval_internal(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            //HACK: This allows us to apply a host units conversion for all 
            //types in the DynamoRevit library
            if (GetType().Assembly.GetName().Name == "DynamoRevit")
            {
                args = args.Select(Utils.UnwrapToDoubleWithHostUnitConversion).ToFSharpList();
            }

            errorCount = 0;
            __eval_internal_recursive(args, outPuts);
            if (errorCount > 1)
                Error(string.Format("{0} runs generated errors.\n\n{1}", errorCount, ToolTipText));
        }

        protected virtual void __eval_internal_recursive(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts, int level = 0)
        {
            try
            {
                var argSets = new List<FSharpList<FScheme.Value>>();

                //create a zip of the incoming args and the port data
                //to be used for type comparison
                List<Tuple<Type, Type>> portComparison =
                    args.Zip(InPortData, (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType))
                        .ToList();
                IEnumerable<Tuple<bool, Type>> listOfListComparison = args.Zip(
                    InPortData,
                    (first, second) => new Tuple<bool, Type>(Utils.IsListOfLists(first), second.PortType));

                //there are more than zero arguments
                //and there is either an argument which does not match its expections 
                //OR an argument which requires a list and gets a list of lists
                //AND argument lacing is not disabled
                if (ArgumentLacing != LacingStrategy.Disabled && args.Any()
                    && (portComparison.Any(
                        x => x.Item1 == typeof(FScheme.Value.List) && x.Item2 != typeof(FScheme.Value.List))
                        || listOfListComparison.Any(x => x.Item1 && x.Item2 == typeof(FScheme.Value.List))))
                {
                    //if the argument is of the expected type, then
                    //leave it alone otherwise, wrap it in a list
                    int j = 0;
                    foreach (FScheme.Value arg in args)
                    {
                        //incoming value is list and expecting single
                        if (portComparison.ElementAt(j).Item1 == typeof(FScheme.Value.List)
                            && portComparison.ElementAt(j).Item2 != typeof(FScheme.Value.List))
                        {
                            //leave as list
                            argSets.Add(((FScheme.Value.List)arg).Item);
                        }
                        //incoming value is list and expecting list
                        else
                        {
                            //check if we have a list of lists, if so, then don't wrap
                            argSets.Add(
                                Utils.IsListOfLists(arg) && !AcceptsListOfLists(arg)
                                    ? ((FScheme.Value.List)arg).Item
                                    : Utils.MakeFSharpList(arg));
                        }
                        j++;
                    }

                    IEnumerable<IEnumerable<FScheme.Value>> lacedArgs = null;
                    switch (ArgumentLacing)
                    {
                        case LacingStrategy.First:
                            lacedArgs = argSets.SingleSet();
                            break;
                        case LacingStrategy.Shortest:
                            lacedArgs = argSets.ShortestSet();
                            break;
                        case LacingStrategy.Longest:
                            lacedArgs = argSets.LongestSet();
                            break;
                        case LacingStrategy.CrossProduct: default:
                            lacedArgs = argSets.CartesianProduct();
                            break;
                    }

                    Dictionary<PortData, FSharpList<FScheme.Value>> evalResult = OutPortData.ToDictionary(
                        x => x,
                        _ => FSharpList<FScheme.Value>.Empty);

                    var evalDict = new Dictionary<PortData, FScheme.Value>();

                    //run the evaluate method for each set of 
                    //arguments in the lace result.
                    foreach (var argList in lacedArgs)
                    {
                        evalDict.Clear();

                        FSharpList<FScheme.Value> thisArgsAsFSharpList = Utils.ToFSharpList(argList);

                        List<Tuple<Type, Type>> portComparisonLaced =
                            thisArgsAsFSharpList.Zip(
                                InPortData,
                                (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType)).ToList();

                        int jj = 0;
                        bool bHasListNotExpecting = false;
                        foreach (FScheme.Value argLaced in argList)
                        {
                            //incoming value is list and expecting single
                            if (ArgumentLacing != LacingStrategy.Disabled && thisArgsAsFSharpList.Any()
                                && portComparisonLaced.ElementAt(jj).Item1 == typeof(FScheme.Value.List)
                                && portComparison.ElementAt(jj).Item2 != typeof(FScheme.Value.List)
                                && (!AcceptsListOfLists(argLaced) || !Utils.IsListOfLists(argLaced)))
                            {
                                bHasListNotExpecting = true;
                                break;
                            }
                            jj++;
                        }
                        if (bHasListNotExpecting)
                        {
                            if (level > 20)
                                throw new Exception("Too deep recursive list containment by lists, only 21 are allowed");
                            var outPutsLevelPlusOne = new Dictionary<PortData, FScheme.Value>();

                            __eval_internal_recursive(Utils.ToFSharpList(argList), outPutsLevelPlusOne, level + 1);
                            //pack result back

                            foreach (var dataLaced in outPutsLevelPlusOne)
                            {
                                PortData dataL = dataLaced.Key;
                                FScheme.Value valueL = outPutsLevelPlusOne[dataL];
                                evalResult[dataL] = FSharpList<FScheme.Value>.Cons(valueL, evalResult[dataL]);
                            }
                            continue;
                        }
                        Evaluate(Utils.ToFSharpList(argList), evalDict);

                        OnEvaluate();

                        foreach (PortData data in OutPortData)
                            evalResult[data] = FSharpList<FScheme.Value>.Cons(evalDict[data], evalResult[data]);
                    }

                    //the result of evaluation will be a list. we split that result
                    //and send the results to the outputs
                    foreach (PortData data in OutPortData)
                    {
                        FSharpList<FScheme.Value> portResults = evalResult[data];

                        //if the lacing is cross product, the results
                        //need to be split back out into a set of lists
                        //equal in dimension to the first list argument
                        if (args[0].IsList && ArgumentLacing == LacingStrategy.CrossProduct)
                        {
                            int length = portResults.Count();
                            int innerLength = length / ((FScheme.Value.List)args[0]).Item.Count();
                            int subCount = 0;
                            FSharpList<FScheme.Value> listOfLists = FSharpList<FScheme.Value>.Empty;
                            FSharpList<FScheme.Value> innerList = FSharpList<FScheme.Value>.Empty;
                            for (int i = 0; i < length; i++)
                            {
                                innerList = FSharpList<FScheme.Value>.Cons(portResults.ElementAt(i), innerList);
                                subCount++;

                                if (subCount == innerLength)
                                {
                                    subCount = 0;
                                    listOfLists = FSharpList<FScheme.Value>.Cons(
                                        FScheme.Value.NewList(innerList),
                                        listOfLists);
                                    innerList = FSharpList<FScheme.Value>.Empty;
                                }
                            }

                            evalResult[data] = Utils.ToFSharpList(listOfLists);
                        }
                        else
                        {
                            //Reverse the evaluation results so they come out right way around
                            evalResult[data] = Utils.ToFSharpList(evalResult[data].Reverse());
                        }

                        outPuts[data] = FScheme.Value.NewList(evalResult[data]);
                    }
                }
                else
                {
                    Evaluate(args, outPuts);
                    OnEvaluate();
                }
            }
            catch (NullReferenceException ex)
            {
                throw new Exception("One of the inputs was not satisfied.", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new Exception("One of your inputs was not of the correct type. See the console for more details.", ex);
            }
        }

        /// <summary>
        ///     Called right before Evaluate() is called. Useful for processing side-effects without touching Evaluate()
        /// </summary>
        protected virtual void OnEvaluate() { }

        protected virtual bool AcceptsListOfLists(FScheme.Value value)
        {
            return false;
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

        #endregion

        /// <summary>
        /// Updates the render package for this node by
        /// getting the MirrorData objects corresponding to
        /// each of the node's ports and processing the underlying
        /// CLR data as IGraphicItems.
        /// </summary>
        public virtual void UpdateRenderPackage()
        {
            //Avoid attempting an update after the controller 
            //has shut down.
            if (dynSettings.Controller == null)
                return;

            //dispose of the current render package
            RenderPackages.Clear();

            if (State == ElementState.Error || !IsVisible)
            {
                return;
            }

            IEnumerable<string> drawableIds = GetDrawableIds();

            int count = 0;
            var labelMap = new List<string>();

            var ident = AstIdentifierForPreview.Name;

            foreach (var varName in drawableIds)
            {
                var mirror = dynSettings.Controller.EngineController.GetMirror(varName);
                if (mirror != null)
                {
                    var mirrorData = mirror.GetData();
                    AddToLabelMap(mirrorData, labelMap, ident);
                    count++;
                }
            } 

            count = 0;
            foreach (var varName in drawableIds)
            {
                var graphItems = dynSettings.Controller.EngineController.GetGraphicItems(varName);
                if (graphItems == null)
                    continue;

                foreach (var gItem in graphItems)
                {
                    var package = new RenderPackage(IsSelected, DisplayLabels);

                    PushGraphicItemIntoPackage(gItem, package, labelMap.Count > count ? labelMap[count] : "?");

                    package.ItemsCount++;
                    RenderPackages.Add(package);
                    count++;
                }
            }
        }

        private void PushGraphicItemIntoPackage(IGraphicItem graphicItem, IRenderPackage package, string tag)
        {
            graphicItem.Tessellate(package);
            package.Tag = tag;
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
        private void AddToLabelMap(IEnumerable list, List<string> map, string tag)
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
        private string
            GetDrawableId(int outPortIndex)
        {
            var output = GetAstIdentifierForOutputIndex(outPortIndex);
            return output.ToString();
        }

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

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
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

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        #endregion

    }

    public enum ElementState
    {
        Dead,
        Active,
        Warning,
        Error
    };


    public enum LacingStrategy
    {
        Disabled,
        First,
        Shortest,
        Longest,
        CrossProduct
    };


    public delegate void PortsChangedHandler(object sender, EventArgs e);


    public delegate void DispatchedToUIThreadHandler(object sender, UIDispatcherEventArgs e);


    public abstract class NodeWithOneOutput : NodeModel
    {
        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract FScheme.Value Evaluate(FSharpList<FScheme.Value> args);
    }


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

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class NodeHiddenInBrowserAttribute : Attribute { }

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


    public class PredicateTraverser
    {
        private readonly Predicate<NodeModel> _predicate;

        private readonly Dictionary<NodeModel, bool> _resultDict = new Dictionary<NodeModel, bool>();

        private bool _inProgress;

        public PredicateTraverser(Predicate<NodeModel> p)
        {
            _predicate = p;
        }

        public bool TraverseUntilAny(NodeModel entry)
        {
            _inProgress = true;
            bool result = TraverseAny(entry);
            _resultDict.Clear();
            _inProgress = false;
            return result;
        }

        public bool ContinueTraversalUntilAny(NodeModel entry)
        {
            if (_inProgress)
                return TraverseAny(entry);
            throw new Exception("ContinueTraversalUntilAny cannot be used except in a traversal predicate.");
        }

        private bool TraverseAny(NodeModel entry)
        {
            bool result;
            if (_resultDict.TryGetValue(entry, out result))
                return result;

            result = _predicate(entry);
            _resultDict[entry] = result;
            if (result)
                return true;

            if (entry is Function)
            {
                Guid symbol = Guid.Parse((entry as Function).Symbol);
                if (!dynSettings.Controller.CustomNodeManager.Contains(symbol))
                {
                    DynamoLogger.Instance.Log("WARNING -- No implementation found for node: " + symbol);
                    entry.Error("Could not find .dyf definition file for this node.");
                    return false;
                }

                result =
                    dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(symbol)
                               .WorkspaceModel.GetTopMostNodes()
                               .Any(ContinueTraversalUntilAny);
            }
            _resultDict[entry] = result;
            return result || entry.Inputs.Values.Any(x => x != null && TraverseAny(x.Item2));
        }
    }


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

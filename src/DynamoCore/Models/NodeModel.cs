using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Dynamo.Nodes;
using System.Xml;
using Dynamo.Selection;
using Microsoft.FSharp.Collections;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Core;
using String = System.String;
using Value = Dynamo.FScheme.Value;
using ProtoCore.AST.AssociativeAST;
using Dynamo.DSEngine;

namespace Dynamo.Models
{
    public enum ElementState { DEAD, ACTIVE, ERROR };

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

    public abstract class NodeModel : ModelBase
    {
        /* TODO:
         * Incorporate INode in here somewhere
         */

        #region abstract members

        /// <summary>
        /// The dynElement's Evaluation Logic.
        /// </summary>
        /// <param name="args">Arguments to the node. You are guaranteed to have as many arguments as you have InPorts at the time it is run.</param>
        /// <returns>An expression that is the result of the Node's evaluation. It will be passed along to whatever the OutPort is connected to.</returns>
        public virtual void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private members

        private string _description;
        private bool interactionEnabled = true;
        internal bool isVisible;
        internal bool isUpstreamVisible;
        private readonly Dictionary<int, Tuple<int, NodeModel>> previousInputPortMappings =
            new Dictionary<int, Tuple<int, NodeModel>>();
        private readonly Dictionary<int, HashSet<Tuple<int, NodeModel>>> previousOutputPortMappings =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();
        readonly Dictionary<PortModel, PortData> portDataDict = new Dictionary<PortModel, PortData>();
        ObservableCollection<PortModel> inPorts = new ObservableCollection<PortModel>();
        ObservableCollection<PortModel> outPorts = new ObservableCollection<PortModel>();
        private LacingStrategy argumentLacing = LacingStrategy.First;
        private string nickName;
        ElementState state;
        string toolTipText = "";
        private IdentifierNode identifier = null;
        // protected AssociativeNode defaultAstExpression = null;
        private bool _overrideNameWithNickName = false;

        /// <summary>
        /// Should changes be reported to the containing workspace?
        /// </summary>
        private bool _report = true;

        /// <summary>
        /// Get the last computed value from the node.
        /// </summary>
        private FScheme.Value _oldValue = null;

        protected internal ExecutionEnvironment macroEnvironment = null;
        private bool _isDirty = true;
        private const string FailureString = "Node evaluation failed";
        private Dictionary<PortData, FScheme.Value> _evaluationDict;
        private bool displayLabels = false;

        #endregion

        #region public members

        // TODO(Ben): Move this up to ModelBase (it makes sense for connector as well).
        public WorkspaceModel WorkSpace;

        #endregion

        #region events
        public event DispatchedToUIThreadHandler DispatchedToUI;
        #endregion

        #region public properties

        public ObservableCollection<PortData> InPortData { get; private set; }
        public ObservableCollection<PortData> OutPortData { get; private set; }
        
        public Dictionary<int, Tuple<int, NodeModel>> Inputs = 
            new Dictionary<int, Tuple<int, NodeModel>>();
        public Dictionary<int, HashSet<Tuple<int, NodeModel>>> Outputs =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

        /// <summary>
        /// Returns whether this node represents a built-in or custom function.
        /// </summary>
        public bool IsCustomFunction
        {
            get { return this is Function; }
        }

        /// <summary>
        /// Returns whether the node is to be included in visualizations.
        /// </summary>
        public bool IsVisible
        {
            get 
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                isDirty = true;
                RaisePropertyChanged("IsVisible");
            }
        }

        /// <summary>
        /// Returns whether the node aggregates its upstream connections
        /// for visualizations.
        /// </summary>
        public bool IsUpstreamVisible
        {
            get 
            {
                return isUpstreamVisible;
            }
            set
            {
                isUpstreamVisible = value;
                isDirty = true;
                RaisePropertyChanged("IsUpstreamVisible");
            }
        }

        public ElementState State
        {
            get
            {
                return state;
            }
            set
            {
                //don't bother changing the state
                //when we are not reporting modifications
                //used when clearing the workbench
                //to avoid nodes recoloring when connectors
                //are deleted
                if (IsReportingModifications)
                {
                    if (value != ElementState.ERROR)
                    {
                        SetTooltip();
                    }

                    state = value;
                    RaisePropertyChanged("State");
                }
            }
        }

        public string ToolTipText
        {
            get
            {
                return toolTipText;
            }
            set
            {
                toolTipText = value;
                RaisePropertyChanged("ToolTipText");
            }
        }

        public bool OverrideNameWithNickName { get { return _overrideNameWithNickName; } set { this._overrideNameWithNickName = value; RaisePropertyChanged("OverrideNameWithNickName"); } }

        public string NickName
        {
            //get { return OverrideNameWithNickName ? _nickName : this.Name; }
            get { return nickName; }
            set
            {
                nickName = value;
                RaisePropertyChanged("NickName");
            }
        }

        public ObservableCollection<PortModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

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
        /// Control how arguments lists of various sizes are laced.
        /// </summary>
        public LacingStrategy ArgumentLacing
        {
            get { return argumentLacing; }
            set
            {
                if (argumentLacing != value)
                {
                    argumentLacing = value;
                    isDirty = true;
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
                var type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
                if (type.Namespace == "Dynamo.Nodes" &&
                    !type.IsAbstract &&
                    attribs.Length > 0 &&
                    type.IsSubclassOf(typeof(NodeModel)))
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
        public string Category { 
            get
            {
                var type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(NodeCategoryAttribute), false);
                if (type.Namespace == "Dynamo.Nodes" &&
                    !type.IsAbstract &&
                    attribs.Length > 0 &&
                    type.IsSubclassOf(typeof (NodeModel)))
                {
                    NodeCategoryAttribute elCatAttrib = attribs[0] as NodeCategoryAttribute;
                    return elCatAttrib.ElementCategory;
                }                    
                return "";
            }
        }

        public virtual FScheme.Value OldValue
        {
            get { return _oldValue; }
            protected set
            {
                _oldValue = value;
                RaisePropertyChanged("OldValue");
            }
        }

        public void ResetOldValue()
        {
            OldValue = null;
            RequiresRecalc = true;
        }

        protected DynamoController Controller
        {
            get { return dynSettings.Controller; }
        }

        ///<summary>
        ///Does this Element need to be regenerated? Setting this to true will trigger a modification event
        ///for the dynWorkspace containing it. If Automatic Running is enabled, setting this to true will
        ///trigger an evaluation.
        ///</summary>
        public virtual bool RequiresRecalc
        {
            get
            {
                //TODO: When marked as clean, remember so we don't have to re-traverse
                if (_isDirty)
                    return true;
                
                bool dirty = Inputs.Values.Where(x => x != null).Any(x => x.Item2.RequiresRecalc);
                _isDirty = dirty;

                return dirty;
            }
            set
            {
                _isDirty = value;
                if (value)
                    ReportModification();
            }
        }

        /// <summary>
        /// Returns if this node requires a recalculation without checking input nodes.
        /// </summary>
        protected internal bool isDirty
        {
            get { return _isDirty; }
            set { RequiresRecalc = value; }
        }

        private bool _saveResult = false;
        /// <summary>
        /// Determines whether or not the output of this Element will be saved. If true, Evaluate() will not be called
        /// unless IsDirty is true. Otherwise, Evaluate will be called regardless of the IsDirty value.
        /// </summary>
        internal bool SaveResult
        {
            get
            {
                return _saveResult
                   && Enumerable.Range(0, InPortData.Count).All(HasInput);
            }
            set
            {
                _saveResult = value;
            }
        }

        /// <summary>
        /// Is this node an entry point to the program?
        /// </summary>
        public bool IsTopmost
        {
            get
            {
                return OutPorts == null
                    || OutPorts.All(x => !x.Connectors.Any());
            }
        }

        public List<string> Tags
        {
            get
            {
                Type t = GetType();
                object[] rtAttribs = t.GetCustomAttributes(typeof(NodeSearchTagsAttribute), true);

                if (rtAttribs.Length > 0)
                    return ((NodeSearchTagsAttribute)rtAttribs[0]).Tags;
                else
                    return new List<string>();

            }
        }

        public virtual string Description
        {
            get
            {
                _description = _description ?? GetDescriptionStringFromAttributes();
                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        /// <summary>
        ///     Get the description from type information
        /// </summary>
        /// <returns>The value or "No description provided"</returns>
        public string GetDescriptionStringFromAttributes()
        {
            var t = GetType();
            object[] rtAttribs = t.GetCustomAttributes(typeof(NodeDescriptionAttribute), true);
            if (rtAttribs.Length > 0)
                return ((NodeDescriptionAttribute)rtAttribs[0]).ElementDescription;
            
            return "No description provided";
        }

        public bool InteractionEnabled
        {
            get { return interactionEnabled; }
            set 
            { 
                interactionEnabled = value;
                RaisePropertyChanged("InteractionEnabled");
            }
        }

        public virtual AssociativeNode AstIdentifier
        {
            get
            {
                if (identifier == null)
                {
                    identifier = new IdentifierNode();
                    identifier.Name = identifier.Value = AstBuilder.StringConstants.kVarPrefix + GUID.ToString().Replace("-", string.Empty);
                }
                return identifier;
            }
        }

        /// <summary>
        /// Enable or disable label display. Default is false.
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
        
        #endregion

        protected NodeModel()
        {
            InPortData = new ObservableCollection<PortData>();
            OutPortData = new ObservableCollection<PortData>();

            IsVisible = true;
            IsUpstreamVisible = true;

            this.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { if(args.PropertyName == "OverrideName") this.RaisePropertyChanged("NickName"); };

            //Fetch the element name from the custom attribute.
            var nameArray = GetType().GetCustomAttributes(typeof(NodeNameAttribute), true);

            if (nameArray.Length > 0)
            {
                var elNameAttrib = nameArray[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    NickName = elNameAttrib.Name;
                }
            }
            else
                NickName = "";

            this.IsSelected = false;
            State = ElementState.DEAD;
            ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// Check current ports against ports used for previous mappings.
        /// </summary>
        void CheckPortsForRecalc()
        {
            RequiresRecalc = Enumerable.Range(0, InPortData.Count).Any(
               delegate(int input)
               {
                   Tuple<int, NodeModel> oldInput;
                   Tuple<int, NodeModel> currentInput;

                   //this is dirty if there wasn't anything set last time (implying it was never run)...
                   return !previousInputPortMappings.TryGetValue(input, out oldInput)
                       || oldInput == null
                       || !TryGetInput(input, out currentInput)
                       //or If what's set doesn't match
                       || (oldInput.Item2 != currentInput.Item2 && oldInput.Item1 != currentInput.Item1);
               })
            || Enumerable.Range(0, OutPortData.Count).Any(
               delegate(int output)
               {
                   HashSet<Tuple<int, NodeModel>> oldOutputs;
                   HashSet<Tuple<int, NodeModel>> newOutputs;

                   return !previousOutputPortMappings.TryGetValue(output, out oldOutputs)
                       || !TryGetOutput(output, out newOutputs)
                       || oldOutputs.SetEquals(newOutputs);
               });
        }

        /// <summary>
        /// Override this to implement custom save data for your Element. If overridden, you should also override
        /// LoadNode() in order to read the data back when loaded.
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole workspace containing this Element.</param>
        /// <param name="nodeElement">The XmlElement representing this Element.</param>
        /// <param name="context">Why is this being called?</param>
        protected virtual void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {

        }

        public void Save(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            SaveNode(xmlDoc, dynEl, context);

            //write port information
            foreach (var port in inPorts.Select((port, index) => new { port, index }).Where(x => x.port.UsingDefaultValue))
            {
                var portInfo = xmlDoc.CreateElement("PortInfo");
                portInfo.SetAttribute("index", port.index.ToString(CultureInfo.InvariantCulture));
                portInfo.SetAttribute("default", true.ToString());
                dynEl.AppendChild(portInfo);
            }
        }

        /// <summary>
        /// Override this to implement loading of custom data for your Element. If overridden, you should also override
        /// SaveNode() in order to write the data when saved.
        /// </summary>
        /// <param name="nodeElement">The XmlNode representing this Element.</param>
        protected virtual void LoadNode(XmlNode nodeElement)
        {

        }

        public void Load(XmlNode elNode, Version workspaceVersion)
        {
            #region Process Migrations

            var migrations =
                (from method in GetType().GetMethods()
                 let attribute =
                     method.GetCustomAttributes(false)
                           .OfType<NodeMigrationAttribute>()
                           .FirstOrDefault()
                 where attribute != null
                 let result = new { method, attribute.From, attribute.To }
                 orderby result.From
                 select result).ToList();

            var currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;

            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                nextMigration.method.Invoke(this, new object[] { elNode });
                workspaceVersion = nextMigration.To;
            }

            #endregion

            LoadNode(elNode);

            var portInfoProcessed = new HashSet<int>();

            //read port information
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "PortInfo")
                {
                    var index = int.Parse(subNode.Attributes["index"].Value);
                    portInfoProcessed.Add(index);
                    var def = bool.Parse(subNode.Attributes["default"].Value);
                    inPorts[index].UsingDefaultValue = def;
                }
            }
            
            //set defaults
            foreach (var port in inPorts.Select((x, i) => new { x, i })
                                        .Where(x => !portInfoProcessed.Contains(x.i)))
            {
                port.x.UsingDefaultValue = false;
            }
        }

        /// <summary>
        /// Forces the node to refresh it's dirty state by checking all inputs.
        /// </summary>
        public void MarkDirty()
        {
            bool dirty = false;
            foreach (var input in Inputs.Values.Where(x => x != null))
            {
                input.Item2.MarkDirty();
                if (input.Item2.RequiresRecalc)
                {
                    dirty = true;
                }
            }
            if (!_isDirty)
                _isDirty = dirty;
        }

        internal virtual INode BuildExpression(Dictionary<NodeModel, Dictionary<int, INode>> buildDict)
        {
            //Debug.WriteLine("Building expression...");

            if (OutPortData.Count > 1)
            {
                var names = OutPortData.Select(x => x.NickName).Zip(Enumerable.Range(0, OutPortData.Count), (x, i) => x+i).ToList();
                var listNode = new FunctionNode("list", names);
                foreach (var data in names.Zip(Enumerable.Range(0, OutPortData.Count), (name, index) => new { Name=name, Index=index }))
                {
                    listNode.ConnectInput(data.Name, Build(buildDict, data.Index));
                }
                return listNode;
            }
            else
                return Build(buildDict, 0);
        }

        //TODO: do all of this as the Ui is modified, simply return this?
        /// <summary>
        /// Builds an INode out of this Element. Override this or Compile() if you want complete control over this Element's
        /// execution.
        /// </summary>
        /// <returns>The INode representation of this Element.</returns>
        protected internal virtual INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            //Debug.WriteLine("Building node...");

            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result))
                return result[outPort];

            //Fetch the names of input ports.
            var portNames = InPortData.Zip(Enumerable.Range(0, InPortData.Count), (x, i) => x.NickName + i).ToList();

            //Is this a partial application?
            var partial = false;

            var connections = new List<Tuple<string, INode>>();
            var partialSymList = new List<string>();

            //For each index in InPortData
            foreach (var data in Enumerable.Range(0, InPortData.Count).Zip(portNames, (data, name) => new { Index = data, Name = name }))
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
                    connections.Add(Tuple.Create(data.Name, new ValueNode(InPortData[data.Index].DefaultValue) as INode));
                }
                else //othwise, remember that this is a partial application
                {
                    partial = true;
                    partialSymList.Add(data.Name);
                }
            }

            Dictionary<int, INode> nodes = 
                OutPortData.Count == 1
                    ? (partial
                        ? buildPartialSingleOut(portNames, connections, partialSymList)
                        : buildSingleOut(portNames, connections))
                    : (partial
                        ? buildPartialMultiOut(portNames, connections, partialSymList)
                        : buildMultiOut(portNames, connections));
            
            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                OldValue = Value.NewFunction(null); // cache an old value for display to the user
                RequiresRecalc = false;
            }

            preBuilt[this] = nodes;

            //And we're done
            return nodes[outPort];
        }

        private Dictionary<int, INode> buildSingleOut(IEnumerable<string> portNames, IEnumerable<Tuple<string, INode>> connections)
        {
            InputNode node = Compile(portNames);

            foreach (var connection in connections)
                node.ConnectInput(connection.Item1, connection.Item2);

            return new Dictionary<int, INode> { { 0, node } };
        }

        private Dictionary<int, INode> buildMultiOut(IEnumerable<string> portNames, IEnumerable<Tuple<string, INode>> connections)
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

        private Dictionary<int, INode> buildPartialSingleOut(IEnumerable<string> portNames, List<Tuple<string, INode>> connections, List<string> partials)
        {
            InputNode node = Compile(portNames);

            foreach (var partial in partials)
            {
                node.ConnectInput(partial, new SymbolNode(partial));
            }

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

        private Dictionary<int, INode> buildPartialMultiOut(IEnumerable<string> portNames, List<Tuple<string, INode>> connections, List<string> partials)
        {
            return OutPortData.Select((d, i) => new { Index = i, Data = d }).ToDictionary(
                data => data.Index,
                data =>
                {
                    var node = Compile(portNames);

                    foreach (var partial in partials)
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

        protected virtual AssociativeNode BuildAstNode(IAstBuilder builder, List<AssociativeNode> inputAstNodes)
        {
            return builder.Build(this, inputAstNodes);
        }

        public AssociativeNode CompileToAstNode(AstBuilder builder)
        {
            if (!RequiresRecalc)
            {
                return this.AstIdentifier; 
            }

            builder.ClearAstNodes(GUID);
            bool isPartiallyApplied = false;

            // Recursively compile its inputs to ast nodes and add intermediate
            // nodes to builder
            var inputAstNodes = new List<AssociativeNode>();
            for (int index = 0; index < InPortData.Count; ++index)
            {
                Tuple<int, NodeModel> input;
                if (!TryGetInput(index, out input))
                {
                    isPartiallyApplied = true;
                    inputAstNodes.Add(null);
                }
                else
                {
                    inputAstNodes.Add(input.Item2.CompileToAstNode(builder));
                }
            }

            // Build evaluatiion for this node. If the rhs is a partially
            // applied function, then a function defintion node will be created.
            // But in the end there is always an assignment:
            //
            //     AstIdentifier = ...;
            var rhs = BuildAstNode(builder, inputAstNodes)
                      ?? builder.BuildEvaluator(this, inputAstNodes);
            builder.BuildEvaluation(this, rhs, isPartiallyApplied);

            return AstIdentifier;
        }

        /// <summary>
        /// Compiles this Element into a ProcedureCallNode. Override this instead of Build() if you don't want to set up all
        /// of the inputs for the ProcedureCallNode.
        /// </summary>
        /// <param name="portNames">The names of the inputs to the node.</param>
        /// <returns>A ProcedureCallNode which will then be processed recursively to be connected to its inputs.</returns>
        protected virtual InputNode Compile(IEnumerable<string> portNames)
        {
            //Debug.WriteLine(string.Format("Compiling InputNode with ports {0}.", string.Join(",", portNames)));

            //Return a Function that calls eval.
            return new ExternalFunctionNode(evalIfDirty, portNames);
        }

        /// <summary>
        /// Called right before Evaluate() is called. Useful for processing side-effects without touching Evaluate()
        /// </summary>
        protected virtual void OnEvaluate() { }

        /// <summary>
        /// Called when the node's workspace has been saved.
        /// </summary>
        protected internal virtual void OnSave() { }

        internal void onSave()
        {
            savePortMappings();
            OnSave();
        }

        private void savePortMappings()
        {
            //Save all of the connection states, so we can check if this is dirty
            foreach (var data in Enumerable.Range(0, InPortData.Count))
            {
                Tuple<int, NodeModel> input;

                previousInputPortMappings[data] = TryGetInput(data, out input)
                   ? input
                   : null;
            }

            foreach (var data in Enumerable.Range(0, OutPortData.Count))
            {
                HashSet<Tuple<int, NodeModel>> outputs;

                previousOutputPortMappings[data] = TryGetOutput(data, out outputs)
                    ? outputs
                    : new HashSet<Tuple<int, NodeModel>>();
            }
        }

        private Value evalIfDirty(FSharpList<Value> args)
        {
            // should I re-evaluate?
            if (OldValue == null || !SaveResult || RequiresRecalc)
            {
                // re-evaluate
                var result = evaluateNode(args);

                // if it was a failure, the old value is null
                if (result.IsString && (result as FScheme.Value.String).Item == FailureString)
                {
                    OldValue = null;
                }
                else // cache the old value
                {
                    OldValue = result;
                }               
            }
            //else
            //    OnEvaluate();

            return OldValue;
        }

        /// <summary>
        /// Wraps node evaluation logic so that it can be called in different threads.
        /// </summary>
        /// <returns>Some(Value) -> Result | None -> Run was cancelled</returns>
        private delegate FSharpOption<FScheme.Value> InnerEvaluationDelegate();

        public FScheme.Value GetValue(int outPortIndex)
        {
            return _evaluationDict.Values.ElementAt(outPortIndex);
        }

        protected internal virtual FScheme.Value evaluateNode(FSharpList<FScheme.Value> args)
        {
            //Debug.WriteLine("Evaluating node...");

            if (SaveResult)
            {
                savePortMappings();
            }

            var evalDict = new Dictionary<PortData, FScheme.Value>();
            _evaluationDict = evalDict;

            object[] iaAttribs = GetType().GetCustomAttributes(typeof(IsInteractiveAttribute), false);
            bool isInteractive = iaAttribs.Length > 0 && ((IsInteractiveAttribute)iaAttribs[0]).IsInteractive;

            InnerEvaluationDelegate evaluation = delegate
            {
                FScheme.Value expr = null;

                try
                {
                    if (Controller.RunCancelled)
                        throw new CancelEvaluationException(false);
                    

                    __eval_internal(args, evalDict);

                    expr = OutPortData.Count == 1
                        ? evalDict[OutPortData[0]]
                        : Value.NewList(
                            Utils.SequenceToFSharpList(
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

                    if (dynSettings.Controller.Testing)
                        throw new Exception(ex.Message);
                }
                

                RequiresRecalc = false;

                return FSharpOption<FScheme.Value>.Some(expr);
            };

            //C# doesn't have a Option type, so we'll just borrow F#'s instead.
            FSharpOption<FScheme.Value> result = isInteractive && dynSettings.Controller.UIDispatcher != null
                ? (FSharpOption<FScheme.Value>)dynSettings.Controller.UIDispatcher.Invoke(evaluation)
                : evaluation();

            if (result == FSharpOption<FScheme.Value>.None)
            {
                throw new CancelEvaluationException(false);
            }
            
            return result.Value ?? Value.NewString(FailureString);
        }

        protected virtual void OnRunCancelled()
        {

        }

        protected virtual void __eval_internal(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            __eval_internal_recursive(args, outPuts);
        }
        
        protected virtual void __eval_internal_recursive(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts, int level = 0)
        {
            var argSets = new List<FSharpList<FScheme.Value>>();

            //create a zip of the incoming args and the port data
            //to be used for type comparison
            var portComparison =
                args.Zip(InPortData, (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType))
                    .ToList();
            var listOfListComparison = args.Zip(InPortData,
                (first, second) => new Tuple<bool, Type>(Utils.IsListOfLists(first), second.PortType));

            //there are more than zero arguments
            //and there is either an argument which does not match its expections 
            //OR an argument which requires a list and gets a list of lists
            //AND argument lacing is not disabled
            if (ArgumentLacing != LacingStrategy.Disabled && args.Any() &&
                (portComparison.Any(x => x.Item1 == typeof (Value.List) && x.Item2 != typeof (Value.List)) ||
                    listOfListComparison.Any(x => x.Item1 && x.Item2 == typeof (Value.List))))
            {
                //if the argument is of the expected type, then
                //leave it alone otherwise, wrap it in a list
                int j = 0;
                foreach (var arg in args)
                {
                    //incoming value is list and expecting single
                    if (portComparison.ElementAt(j).Item1 == typeof (Value.List) &&
                        portComparison.ElementAt(j).Item2 != typeof (Value.List))
                    {
                        //leave as list
                        argSets.Add(((Value.List) arg).Item);
                    }
                        //incoming value is list and expecting list
                    else
                    {
                        //check if we have a list of lists, if so, then don't wrap
                        argSets.Add(
                            Utils.IsListOfLists(arg) && !AcceptsListOfLists(arg)
                                ? ((Value.List) arg).Item
                                : Utils.MakeFSharpList(arg));
                    }
                    j++;
                }

                IEnumerable<IEnumerable<Value>> lacedArgs = null;
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
                    case LacingStrategy.CrossProduct:
                        lacedArgs = argSets.CartesianProduct();
                        break;
                }

                var evalResult = OutPortData.ToDictionary(
                    x => x,
                    _ => FSharpList<Value>.Empty);

                var evalDict = new Dictionary<PortData, Value>();

                //run the evaluate method for each set of 
                //arguments in the lace result.
                foreach (var argList in lacedArgs)
                {
                    evalDict.Clear();

                    var thisArgsAsFSharpList = Utils.SequenceToFSharpList(argList);

                    var portComparisonLaced =
                        thisArgsAsFSharpList.Zip(InPortData,
                            (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType)).ToList();

                    int jj = 0;
                    bool bHasListNotExpecting = false;
                    foreach (var argLaced in argList)
                    {
                        //incoming value is list and expecting single
                        if (ArgumentLacing != LacingStrategy.Disabled && thisArgsAsFSharpList.Any() &&
                            portComparisonLaced.ElementAt(jj).Item1 == typeof (Value.List) &&
                            portComparison.ElementAt(jj).Item2 != typeof (Value.List) &&
                            (!AcceptsListOfLists(argLaced) || !Utils.IsListOfLists(argLaced))
                            )
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
                        Dictionary<PortData, FScheme.Value> outPutsLevelPlusOne =
                            new Dictionary<PortData, FScheme.Value>();

                        __eval_internal_recursive(Utils.SequenceToFSharpList(argList), outPutsLevelPlusOne,
                            level + 1);
                        //pack result back

                        foreach (var dataLaced in outPutsLevelPlusOne)
                        {
                            var dataL = dataLaced.Key;
                            var valueL = outPutsLevelPlusOne[dataL];
                            evalResult[dataL] = FSharpList<Value>.Cons(valueL, evalResult[dataL]);
                        }
                        continue;
                    }
                    else
                        Evaluate(Utils.SequenceToFSharpList(argList), evalDict);

                    OnEvaluate();

                    foreach (var data in OutPortData)
                    {
                        evalResult[data] = FSharpList<Value>.Cons(evalDict[data], evalResult[data]);
                    }
                }

                //the result of evaluation will be a list. we split that result
                //and send the results to the outputs
                foreach (var data in OutPortData)
                {
                    var portResults = evalResult[data];

                    //if the lacing is cross product, the results
                    //need to be split back out into a set of lists
                    //equal in dimension to the first list argument
                    if (args[0].IsList && ArgumentLacing == LacingStrategy.CrossProduct)
                    {
                        var length = portResults.Count();
                        var innerLength = length/((Value.List) args[0]).Item.Count();
                        int subCount = 0;
                        var listOfLists = FSharpList<Value>.Empty;
                        var innerList = FSharpList<Value>.Empty;
                        for (int i = 0; i < length; i++)
                        {
                            innerList = FSharpList<Value>.Cons(portResults.ElementAt(i), innerList);
                            subCount++;

                            if (subCount == innerLength)
                            {
                                subCount = 0;
                                listOfLists = FSharpList<Value>.Cons(Value.NewList(innerList), listOfLists);
                                innerList = FSharpList<Value>.Empty;
                            }
                        }

                        evalResult[data] = Utils.SequenceToFSharpList(listOfLists);
                    }
                    else
                    {
                        //Reverse the evaluation results so they come out right way around
                        evalResult[data] = Utils.SequenceToFSharpList(evalResult[data].Reverse());
                    }

                    outPuts[data] = Value.NewList(evalResult[data]);
                }

            }
            else
            {
                Evaluate(args, outPuts);
                OnEvaluate();
            }
        }

        protected virtual bool AcceptsListOfLists(Value value)
        {
            return false;
        }
        
        /// <summary>
        /// Destroy this dynElement
        /// </summary>
        public virtual void Destroy() { }

        protected internal void DisableReporting()
        {
            _report = false;
        }

        protected internal void EnableReporting()
        {
            _report = true;
            ValidateConnections();
        }

        protected internal bool IsReportingModifications { get { return _report; } }

        protected internal void ReportModification()
        {
            if (IsReportingModifications && WorkSpace != null)
                WorkSpace.Modified();
        }

        /// <summary>
        /// Creates a Scheme representation of this dynNode and all connected dynNodes.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            var nick = NickName.Replace(' ', '_');

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
                s += "(lambda ("
                   + string.Join(" ", InPortData.Where((_, i) => !HasInput(i)).Select(x => x.NickName))
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

        internal void ConnectInput(int inputData, int outputData, NodeModel node)
        {
            Inputs[inputData] = Tuple.Create(outputData, node);
            CheckPortsForRecalc();
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
            CheckPortsForRecalc();
        }

        internal int GetPortIndex(PortModel portModel, out PortType portType)
        {
            int index = this.inPorts.IndexOf(portModel);
            if (-1 != index)
            {
                portType = PortType.INPUT;
                return index;
            }

            index = this.outPorts.IndexOf(portModel);
            if (-1 != index)
            {
                portType = PortType.OUTPUT;
                return index;
            }

            portType = PortType.INPUT;
            return -1; // No port found.
        }

        /// <summary>
        /// Attempts to get the input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <param name="input">If an input is found, it will be assigned.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool TryGetInput(int data, out Tuple<int, NodeModel> input)
        {
            return Inputs.TryGetValue(data, out input) && input != null;
        }

        /// <summary>
        /// Attempts to get the output for a certain port.
        /// </summary>
        /// <param name="output">Index to look for an output for.</param>
        /// <param name="newOutputs">If an output is found, it will be assigned.</param>
        /// <returns>True if there is an output, false otherwise.</returns>
        public bool TryGetOutput(int output, out HashSet<Tuple<int, NodeModel>> newOutputs)
        {
            return Outputs.TryGetValue(output, out newOutputs);
        }

        /// <summary>
        /// Checks if there is an input for a certain port.
        /// </summary>
        /// <param name="data">Index of the port to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasInput(int data)
        {
            return HasConnectedInput(data) || (InPorts.Count > data && InPorts[data].UsingDefaultValue);
        }

        /// <summary>
        /// Checks if there is a connected input for a certain port. This does
        /// not count default values as an input.
        /// </summary>
        /// <param name="data">Index of the port to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasConnectedInput(int data)
        {
            return Inputs.ContainsKey(data) && Inputs[data] != null;
        }

        /// <summary>
        /// Checks if there is an output for a certain port.
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
            CheckPortsForRecalc();
        }

        /// <summary>
        /// Implement on derived classes to cleanup resources when 
        /// </summary>
        public virtual void Cleanup()
        {
        }

        /// <summary>
        /// Updates UI so that all ports reflect current state of InPortData and OutPortData.
        /// </summary>
        public void RegisterAllPorts()
        {
            RegisterInputs();
            RegisterOutputs();
            ValidateConnections();
        }

        /// <summary>
        /// Add a port to this node. If the port already exists, return that port.
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

                    p = new PortModel(index, portType, this, data.NickName)
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
                        return p;
                    }

                    p = new PortModel(index, portType, this, data.NickName)
                    {
                        UsingDefaultValue = false
                    };

                    OutPorts.Add(p);

                    //register listeners on the port
                    p.PortConnected += p_PortConnected;
                    p.PortDisconnected += p_PortDisconnected;

                    return p;
            }

            return null;
        }

        //TODO: call connect and disconnect for dynNode

        /// <summary>
        /// When a port is connected, register a listener for the dynElementUpdated event
        /// and tell the object to build
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void p_PortConnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (PortModel)sender;
            if (port.PortType == PortType.INPUT)
            {
                var data = InPorts.IndexOf(port);
                var startPort = port.Connectors[0].Start;
                var outData = startPort.Owner.OutPorts.IndexOf(startPort);
                ConnectInput(data, outData, startPort.Owner);
                startPort.Owner.ConnectOutput(outData, data, this);
            }
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            ValidateConnections();

            var port = (PortModel)sender;
            if (port.PortType == PortType.INPUT)
            {
                var data = InPorts.IndexOf(port);
                var startPort = port.Connectors[0].Start;
                DisconnectInput(data);
                startPort.Owner.DisconnectOutput(
                    startPort.Owner.OutPorts.IndexOf(startPort),
                    data,
                    this);
            }
        }

        private void DestroyConnectors(PortModel port)
        {
            while (port.Connectors.Any())
            {
                var connector = port.Connectors[0];
                WorkSpace.Connectors.Remove(connector);
                connector.NotifyConnectedPortsOfDeletion();
            }
        }

        /// <summary>
        /// Reads inputs list and adds ports for each input.
        /// </summary>
        public void RegisterInputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in InPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                var port = AddPort(PortType.INPUT, pd, count);

                //MVVM: AddPort now returns a port model. You can't set the data context here.
                //port.DataContext = this;

                portDataDict[port] = pd;
                count++;
            }

            if (inPorts.Count > count)
            {
                foreach (var inport in inPorts.Skip(count))
                {
                    DestroyConnectors(inport);
                    portDataDict.Remove(inport);
                }

                for (int i = inPorts.Count - 1; i >= count; i--)
                    inPorts.RemoveAt(i);
            }
        }

        /// <summary>
        /// Reads outputs list and adds ports for each output
        /// </summary>
        public void RegisterOutputs()
        {
            //read the inputs list and create a number of
            //input ports
            int count = 0;
            foreach (PortData pd in OutPortData)
            {
                //add a port for each input
                //distribute the ports along the 
                //edges of the icon
                var port = AddPort(PortType.OUTPUT, pd, count);

//MVVM : don't set the data context in the model
                //port.DataContext = this;

                portDataDict[port] = pd;
                count++;
            }

            if (outPorts.Count > count)
            {
                foreach (var outport in outPorts.Skip(count))
                    DestroyConnectors(outport);

                for (int i = outPorts.Count - 1; i >= count; i--)
                    outPorts.RemoveAt(i);

                //OutPorts.RemoveRange(count, outPorts.Count - count);
            }
        }

        void SetTooltip()
        {
            ToolTipText = "";
        }

        public IEnumerable<ConnectorModel> AllConnectors()
        {
            return inPorts.Concat(outPorts).SelectMany(port => port.Connectors);
        }

        /// <summary>
        /// Color the connection according to it's port connectivity
        /// if all ports are connected, color green, else color orange
        /// </summary>
        public void ValidateConnections()
        {

            Action setState = (() =>
                {

                    // if there are inputs without connections
                    // mark as dead
                    State = inPorts.Any(x => !x.Connectors.Any() && !(x.UsingDefaultValue && x.DefaultValueEnabled))
                                ? ElementState.DEAD
                                : ElementState.ACTIVE;
                });

            if (dynSettings.Controller != null &&
                dynSettings.Controller.UIDispatcher != null &&
                dynSettings.Controller.UIDispatcher.Thread != Thread.CurrentThread)
            {
                //Force this onto the UI thread
                dynSettings.Controller.UIDispatcher.Invoke(setState);
            }
            else
                setState();
        }

        public void Error(string p)
        {
            State = ElementState.ERROR;
            ToolTipText = p;
        }

        public void SelectNeighbors()
        {
            var outConnectors = outPorts.SelectMany(x => x.Connectors);
            var inConnectors = inPorts.SelectMany(x => x.Connectors);

            foreach (var c in outConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.End.Owner)))
                DynamoSelection.Instance.Selection.Add(c.End.Owner);

            foreach (var c in inConnectors.Where(c => !DynamoSelection.Instance.Selection.Contains(c.Start.Owner)))
                DynamoSelection.Instance.Selection.Add(c.Start.Owner);
        }

        //private Dictionary<UIElement, bool> enabledDict
        //    = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            State = ElementState.DEAD;
            InteractionEnabled = false;
        }

        internal void EnableInteraction()
        {
            ValidateConnections();
            InteractionEnabled = true;
        }

        /// <summary>
        /// Called back from the view to enable users to setup their own view elements
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void SetupCustomUIElements(object nodeUI)
        {
            
        }

        /// <summary>
        /// Called by nodes for behavior that they want to dispatch on the UI thread
        /// Triggers event to be received by the UI. If no UI exists, behavior will not be executed.
        /// </summary>
        /// <param name="a"></param>
        public void DispatchOnUIThread(Action a)
        {
            OnDispatchedToUI(this, new UIDispatcherEventArgs(a));
        }

        public static string PrintValue(Value eIn, int currentListIndex, int maxListIndex, int currentDepth, int maxDepth, int maxStringLength = 20)
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
                var str = (eIn as Value.Container).Item != null
                    ? (eIn as Value.Container).Item.ToString()
                    : "<empty>";

                accString += str;
            }
            else if (eIn.IsFunction)
            {
                accString += "<function>";
            }
            else if (eIn.IsList)
            {
                accString += "List";
                
                var list = (eIn as Value.List).Item;

                if (!list.Any())
                {
                    accString += " (empty)";
                }

                // when children will be at maxDepth, just do 1
                if (currentDepth + 1 == maxDepth)
                {
                    maxListIndex = 0;
                }

                // build all elements of sub list
                accString =
                   list.Select((x, i) => new { Element = x, Index = i })
                       .TakeWhile(e => e.Index <= maxListIndex)
                       .Aggregate(
                           accString,
                           (current, e) => current + "\n" + PrintValue(e.Element, e.Index, maxListIndex, currentDepth + 1, maxDepth, maxStringLength));

               
            }
            else if (eIn.IsNumber)
            {
                var num = (eIn as Value.Number).Item;
                var numFloat = (float) num;
                accString += numFloat.ToString();
            }
            else if (eIn.IsString)
            {
                var str = (eIn as Value.String).Item;

                if (str.Length > maxStringLength)
                {
                    str = str.Substring(0, maxStringLength) + "...";
                }

                accString += "\"" + str + "\"";
            }
            else if (eIn.IsSymbol)
            {
                accString += "<" + (eIn as Value.Symbol).Item + ">";
            }

            return accString;
        }

        public void OnDispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            if (DispatchedToUI != null)
                DispatchedToUI(this, e);
        }

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
                this.NickName = value;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);

            // Set the type attribute
            helper.SetAttribute("type", this.GetType().ToString());
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("nickname", this.NickName);
            helper.SetAttribute("x", this.X);
            helper.SetAttribute("y", this.Y);
            helper.SetAttribute("isVisible", this.IsVisible);
            helper.SetAttribute("isUpstreamVisible", this.IsUpstreamVisible);
            helper.SetAttribute("lacing", this.ArgumentLacing.ToString());

            if (context == SaveContext.Undo)
            {
                // Fix: MAGN-159 (nodes are not editable after undo/redo).
                helper.SetAttribute("interactionEnabled", this.interactionEnabled);
                helper.SetAttribute("nodeState", this.state.ToString());
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("guid", Guid.NewGuid());

            // Resolve node nick name.
            string nickName = helper.ReadString("nickname", string.Empty);
            if (!string.IsNullOrEmpty(nickName))
                this.nickName = nickName;
            else
            {
                System.Type type = this.GetType();
                var attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), true);
                NodeNameAttribute attrib = attribs[0] as NodeNameAttribute;
                if (null != attrib)
                    this.nickName = attrib.Name;
            }

            this.X = helper.ReadDouble("x", 0.0);
            this.Y = helper.ReadDouble("y", 0.0);
            this.isVisible = helper.ReadBoolean("isVisible", true);
            this.isUpstreamVisible = helper.ReadBoolean("isUpstreamVisible", true);
            this.argumentLacing = helper.ReadEnum("lacing", LacingStrategy.Disabled);

            if (context == SaveContext.Undo)
            {
                // Fix: MAGN-159 (nodes are not editable after undo/redo).
                interactionEnabled = helper.ReadBoolean("interactionEnabled", true);
                this.state = helper.ReadEnum("nodeState", ElementState.ACTIVE);

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
                this.ReportPosition();
            }
        }

        #endregion

    }

    public abstract class NodeWithOneOutput : NodeModel
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract Value Evaluate(FSharpList<Value> args);
    }

    #region class attributes
    [AttributeUsage(AttributeTargets.All)]
    public class NodeNameAttribute : System.Attribute
    {
        public string Name { get; set; }

        public NodeNameAttribute(string elementName)
        {
            Name = elementName;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : System.Attribute
    {
        public string ElementCategory { get; set; }

        public NodeCategoryAttribute(string category)
        {
            ElementCategory = category;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchTagsAttribute : System.Attribute
    {
        public List<string> Tags { get; set; }

        public NodeSearchTagsAttribute(params string[] tags)
        {
            Tags = tags.ToList();
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class IsInteractiveAttribute : System.Attribute
    {
        public bool IsInteractive { get; set; }

        public IsInteractiveAttribute(bool isInteractive)
        {
            IsInteractive = isInteractive;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeDescriptionAttribute : System.Attribute
    {
        public string ElementDescription
        {
            get;
            set;
        }

        public NodeDescriptionAttribute(string description)
        {
            ElementDescription = description;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchableAttribute : System.Attribute
    {
        public bool IsSearchable
        {
            get;
            set;
        }

        public NodeSearchableAttribute(bool isSearchable)
        {
            IsSearchable = isSearchable;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeTypeIdAttribute : System.Attribute
    {
        public string Id
        {
            get;
            set;
        }

        public NodeTypeIdAttribute(string description)
        {
            Id = description;
        }
    }

    /// <summary>
    /// The DoNotLoadOnPlatforms attribute allows the node implementor
    /// to define an array of contexts in which the node will not
    /// be loaded.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DoNotLoadOnPlatformsAttribute : Attribute
    {
        public string[] Values { get; set; }

        public DoNotLoadOnPlatformsAttribute(params string[] values)
        {
            this.Values = values;
        }
    }

    /// <summary>
    /// Flag to hide deprecated nodes in search, but allow in workflows
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class NodeHiddenInBrowserAttribute : System.Attribute
    {
    }

    /// <summary>
    /// The AlsoKnownAs attribute allows the node implementor to
    /// define an array of names that this node might have had
    /// in the past.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AlsoKnownAsAttribute : Attribute
    {
        public string[] Values { get; set; }

        public AlsoKnownAsAttribute(params string[] values)
        {
            this.Values = values;
        }
    }

    #endregion

    public class PredicateTraverser
    {
        readonly Predicate<NodeModel> _predicate;

        readonly Dictionary<NodeModel, bool> _resultDict = new Dictionary<NodeModel, bool>();

        bool _inProgress;

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
            else
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
                return result;

            if (entry is Function)
            {
                var symbol = Guid.Parse((entry as Function).Symbol);
                if (!dynSettings.Controller.CustomNodeManager.Contains(symbol))
                {
                    DynamoLogger.Instance.Log("WARNING -- No implementation found for node: " + symbol);
                    entry.Error("Could not find .dyf definition file for this node.");
                    return false;
                }

                result = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(symbol)
                    .WorkspaceModel.GetTopMostNodes().Any(ContinueTraversalUntilAny);
            }
            _resultDict[entry] = result;
            if (result)
                return result;

            return entry.Inputs.Values.Any(x => x != null && TraverseAny(x.Item2));
        }
    }

    public class UIDispatcherEventArgs:EventArgs
    {
        public Action ActionToDispatch { get; set; }
        public List<object> Parameters { get; set; }
        public UIDispatcherEventArgs(Action a)
        {
            ActionToDispatch = a;
        }
    }
}

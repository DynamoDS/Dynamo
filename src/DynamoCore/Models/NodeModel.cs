using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
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

    public enum SaveContext { File, Copy };

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

        #region Abstract Members

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

        #region Properties

        public event DispatchedToUIThreadHandler DispatchedToUI;
        public void OnDispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            if (DispatchedToUI != null)
                DispatchedToUI(this, e);
        }

        public WorkspaceModel WorkSpace;
        public ObservableCollection<PortData> InPortData { get; private set; }
        public ObservableCollection<PortData> OutPortData { get; private set; }
        readonly Dictionary<PortModel, PortData> portDataDict = new Dictionary<PortModel, PortData>();
        
//MVVM : node should not reference its view directly
        //public dynNodeView NodeUI;
        
        public Dictionary<int, Tuple<int, NodeModel>> Inputs = 
            new Dictionary<int, Tuple<int, NodeModel>>();
        public Dictionary<int, HashSet<Tuple<int, NodeModel>>> Outputs =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();

        private readonly Dictionary<int, Tuple<int, NodeModel>> previousInputPortMappings = 
            new Dictionary<int, Tuple<int, NodeModel>>();
        private readonly Dictionary<int, HashSet<Tuple<int, NodeModel>>> previousOutputPortMappings =
            new Dictionary<int, HashSet<Tuple<int, NodeModel>>>();
        ObservableCollection<PortModel> inPorts = new ObservableCollection<PortModel>();
        ObservableCollection<PortModel> outPorts = new ObservableCollection<PortModel>();
        private LacingStrategy argumentLacing  = LacingStrategy.First;
        private string nickName;
        ElementState state;
        string toolTipText = "";
        //bool isSelected = false;

        private bool interactionEnabled = true;
        private bool isVisible;
        private bool isUpstreamVisible;

        private IdentifierNode identifier = null;
        protected AssociativeNode defaultAstExpression = null;

        /// <summary>
        /// Returns whether this node represents a built-in or custom function.
        /// </summary>
        public bool IsCustomFunction
        {
            get { return this is Function; }
        }

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
                if (value != ElementState.ERROR)
                {
                    SetTooltip();
                }

                state = value;
                RaisePropertyChanged("State");
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

        private bool _overrideNameWithNickName = false;
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
                argumentLacing = value;
                isDirty = true;
                RaisePropertyChanged("ArgumentLacing");
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

        /// <summary>
        /// Should changes be reported to the containing workspace?
        /// </summary>
        private bool _report = true;

        /// <summary>
        /// Get the last computed value from the node.
        /// </summary>
        private FScheme.Value _oldValue = null;
        public FScheme.Value OldValue
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

        protected internal ExecutionEnvironment macroEnvironment = null;

        //TODO: don't make this static (maybe)
        //protected DynamoView Bench
        //{
        //    get { return dynSettings.Bench; }
        //}

        protected DynamoController Controller
        {
            get { return dynSettings.Controller; }
        }

        private bool _isDirty = true;

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

        private string _description;
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

        public AssociativeNode AstIdentifier
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
        /// Some node is constant value like Pi or E, no need to create a
        /// variable for this kind of node.
        /// </summary>
        protected virtual AssociativeNode DefaultAstExpression
        {
            get
            {
                return defaultAstExpression;
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
        protected virtual void SaveNode(System.Xml.XmlDocument xmlDoc, System.Xml.XmlElement nodeElement, SaveContext context)
        {

        }

        public void Save(System.Xml.XmlDocument xmlDoc, System.Xml.XmlElement dynEl, SaveContext context)
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
        protected virtual void LoadNode(System.Xml.XmlNode nodeElement)
        {

        }

        public void Load(System.Xml.XmlNode elNode)
        {
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
            foreach (var port in inPorts.Select((x, i) => new { x, i }).Where(x => !portInfoProcessed.Contains(x.i)))
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
                var names = OutPortData.Select(x => x.NickName).Zip(Enumerable.Range(0, OutPortData.Count), (x, i) => x+i);
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

            //Compile the procedure for this node.
            InputNode node = Compile(portNames);

            //Is this a partial application?
            var partial = false;

            var connections = new List<Tuple<string, INode>>();
            var partialSymList = new List<string>();

            //For each index in InPortData
            //for (int i = 0; i < InPortData.Count; i++)
            foreach (var data in Enumerable.Range(0, InPortData.Count).Zip(portNames, (data, name) => new { Index = data, Name = name }))
            {
                //Fetch the corresponding port
                //var port = InPorts[i];

                Tuple<int, NodeModel> input;

                //If this port has connectors...
                //if (port.Connectors.Any())
                if (TryGetInput(data.Index, out input))
                {
                    //Debug.WriteLine(string.Format("Connecting input {0}", data.Name));

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
                    node.ConnectInput(data.Name, new SymbolNode(data.Name));
                    partialSymList.Add(data.Name);
                }
            }

            var nodes = new Dictionary<int, INode>();

            if (OutPortData.Count > 1)
            {
                if (partial)
                {
                    foreach (var connection in connections)
                        node.ConnectInput(connection.Item1, new SymbolNode(connection.Item1));
                }
                else
                {
                    foreach (var connection in connections)
                        node.ConnectInput(connection.Item1, connection.Item2);
                }

                InputNode prev = node;
                int prevIndex = 0;

                foreach (var data in OutPortData.Select((d, i) => new { Index = i, Data = d }))
                {
                    if (HasOutput(data.Index))
                    {
                        if (data.Index > 0)
                        {
                            var diff = data.Index - prevIndex;
                            InputNode restNode;
                            if (diff > 1)
                            {
                                restNode = new ExternalFunctionNode(FScheme.Drop, new[] { "amt", "list" });
                                restNode.ConnectInput("amt", new NumberNode(diff));
                                restNode.ConnectInput("list", prev);
                            }
                            else
                            {
                                restNode = new ExternalFunctionNode(FScheme.Cdr, new[] { "list" });
                                restNode.ConnectInput("list", prev);
                            }
                            prev = restNode;
                            prevIndex = data.Index;
                        }

                        var firstNode = new ExternalFunctionNode(FScheme.Car, new[] { "list" }) as InputNode;
                        firstNode.ConnectInput("list", prev);

                        if (partial)
                        {
                            var outerNode = new AnonymousFunctionNode(partialSymList, firstNode);
                            if (connections.Any())
                            {
                                outerNode = new AnonymousFunctionNode(
                                    connections.Select(x => x.Item1),
                                    outerNode);
                                foreach (var connection in connections)
                                {
                                    outerNode.ConnectInput(connection.Item1, connection.Item2);
                                }
                            }
                            firstNode = outerNode;
                        }

                        nodes[data.Index] = firstNode;
                    }
                    else
                        nodes[data.Index] = new NumberNode(0);
                }
            }
            else
            {
                if (partial)
                {
                    var outerNode = new AnonymousFunctionNode(partialSymList, node);
                    if (connections.Any())
                    {
                        outerNode = new AnonymousFunctionNode(
                            connections.Select(x => x.Item1),
                            outerNode);
                        foreach (var connection in connections)
                        {
                            node.ConnectInput(connection.Item1, new SymbolNode(connection.Item1));
                            outerNode.ConnectInput(connection.Item1, connection.Item2);
                        }
                    }
                    node = outerNode;
                }
                else
                {
                    foreach (var connection in connections)
                    {
                        node.ConnectInput(connection.Item1, connection.Item2);
                    }
                }
                nodes[outPort] = node;
            }

            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                OldValue = FScheme.Value.NewFunction(null); // cache an old value for display to the user
                RequiresRecalc = false;
            }

            preBuilt[this] = nodes;

            //And we're done
            return nodes[outPort];
        }

        protected virtual AssociativeNode CompileToAstNodeInternal(List<AssociativeNode> inputAstNodes)
        {
            return null;
        }

        public AssociativeNode CompileToAstNode(AstBuilder builder)
        {
            if (!RequiresRecalc || builder.ContainsAstNodes(GUID))
            {
                return DefaultAstExpression == null ? this.AstIdentifier : DefaultAstExpression;
            }

            bool isPartiallyApplied = false;

            // Recursively compile its inputs to ast nodes and add intermediate
            // nodes to builder
            List<AssociativeNode> inputAstNodes = new List<AssociativeNode>();
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
            var rhs = CompileToAstNodeInternal(inputAstNodes);
            if (rhs == null)
            {
                // For any dyn node which doesn't override this function, we treat
                // them as custom nodes, therefore their evaluation is based on f#
                // evaluation engine. This is done through evalutor.
                rhs = AstBuilder.BuildEvaluator(builder, this, inputAstNodes);
            }
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

        private const string FailureString = "Node evaluation failed";
        private Dictionary<PortData, FScheme.Value> _evaluationDict;

        protected virtual void OnRunCancelled()
        {

        }
        
        protected virtual void __eval_internal(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            //if this element maintains a collcection of references
            //then clear the collection
            if (this is IClearable)
                (this as IClearable).ClearReferences();

            var argSets = new List<FSharpList<FScheme.Value>>();

            //create a zip of the incoming args and the port data
            //to be used for type comparison
            var portComparison = args.Zip(InPortData, (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType)).ToList();
            var listOfListComparison = args.Zip(InPortData, (first, second) => new Tuple<bool, Type>(Utils.IsListOfLists(first), second.PortType));

            //there are more than zero arguments
            //and there is either an argument which does not match its expections 
            //OR an argument which requires a list and gets a list of lists
            //AND argument lacing is not disabled
            if (ArgumentLacing != LacingStrategy.Disabled && args.Any() &&
                (portComparison.Any(x => x.Item1 == typeof(Value.List) && x.Item2 != typeof(Value.List)) ||
                listOfListComparison.Any(x => x.Item1 && x.Item2 == typeof(Value.List))))
            {
                //if the argument is of the expected type, then
                //leave it alone otherwise, wrap it in a list
                int j = 0;
                foreach (var arg in args)
                {
                    //incoming value is list and expecting single
                    if (portComparison.ElementAt(j).Item1 == typeof(Value.List) &&
                        portComparison.ElementAt(j).Item2 != typeof(Value.List))
                    {
                        //leave as list
                        argSets.Add(((Value.List)arg).Item);
                    }
                    //incoming value is list and expecting list
                    else
                    {
                        //check if we have a list of lists, if so, then don't wrap
                        argSets.Add(
                            Utils.IsListOfLists(arg) && !AcceptsListOfLists(arg)
                                ? ((Value.List)arg).Item
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
                //arguments in the lace result. do these
                //in reverse order so our cons comes out the right
                //way around
                foreach (var argList in lacedArgs.Reverse())
                {
                    evalDict.Clear();

                    Evaluate(Utils.SequenceToFSharpList(argList), evalDict);
                    OnEvaluate();

                    foreach (var data in OutPortData)
                        evalResult[data] = FSharpList<Value>.Cons(evalDict[data], evalResult[data]);
                }

                //the result of evaluation will be a list. we split that result
                //and send the results to the outputs
                foreach (var data in OutPortData)
                    outPuts[data] = Value.NewList(evalResult[data]);
            }
            else
            {
                Evaluate(args, outPuts);
                OnEvaluate();
            }

            if (dynSettings.Controller.UIDispatcher != null && this is IDrawable)
            {
                dynSettings.Controller.UIDispatcher.Invoke(new Action(() => (this as IDrawable).Draw()));
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

        public bool TryGetOutput(int output, out HashSet<Tuple<int, NodeModel>> newOutputs)
        {
            return Outputs.TryGetValue(output, out newOutputs);
        }

        /// <summary>
        /// Checks if there is an input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasInput(int data)
        {
            return Inputs.ContainsKey(data) && Inputs[data] != null;
        }

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

        private void RemovePort(PortModel inport)
        {
            while (inport.Connectors.Any())
            {
                var connector = inport.Connectors[0];
                dynSettings.Controller.DynamoModel.CurrentWorkspace.Connectors.Remove(connector);
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
                    RemovePort(inport);

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
                    RemovePort(outport);

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
            // if there are inputs without connections
            // mark as dead
            State = inPorts.Any(x => !x.Connectors.Any() && !(x.UsingDefaultValue && x.DefaultValueEnabled))
                ? ElementState.DEAD 
                : ElementState.ACTIVE;
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

        public static string BuildValueString(Value eIn, int currentListIndex, int maxListIndex, int currentDepth, int maxDepth)
        {
            if (eIn == null)
                return "<null>";

            string accString = String.Concat(Enumerable.Repeat("  ", currentDepth));

            if (maxDepth == currentDepth || currentListIndex == maxListIndex)
            {
                accString += "...\n";
                return accString;
            }

            if (eIn.IsContainer)
            {
                var str = (eIn as Value.Container).Item != null
                    ? (eIn as Value.Container).Item.ToString()
                    : "null";

                accString += str;
            }
            else if (eIn.IsFunction)
            {
                accString += "<function>";
            }
            else if (eIn.IsList)
            {
                accString += "List\n";

                var list = (eIn as Value.List).Item;

                // build all elements of sub list
                accString = 
                    list.Select((x, i) => new {Element = x, Index = i})
                        .TakeWhile(e => e.Index <= maxListIndex)
                        .Aggregate(
                            accString, 
                            (current, e) => current + BuildValueString(e.Element, e.Index, maxListIndex, currentDepth + 1, maxDepth));
            }
            else if (eIn.IsNumber)
            {
                accString += (eIn as Value.Number).Item.ToString();
            }
            else if (eIn.IsString)
            {
                accString += "\"" + (eIn as Value.String).Item + "\"";
            }
            else if (eIn.IsSymbol)
            {
                accString += "<" + (eIn as Value.Symbol).Item + ">";
            }

            accString += "\n";

            return accString;
        }

        #region ISelectable Interface

        public override void Deselect()
        {
            ValidateConnections();
            IsSelected = false;
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
                    .Workspace.GetTopMostNodes().Any(ContinueTraversalUntilAny);
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

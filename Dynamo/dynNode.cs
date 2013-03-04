using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Dynamo.Nodes
{
    public abstract class dynNode
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
        public virtual void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            throw new NotImplementedException();
        }

        #endregion

        public dynWorkspace WorkSpace;

        public List<PortData> InPortData { get; private set; }
        public List<PortData> OutPortData { get; private set; }
        public dynNodeUI NodeUI;
        public Dictionary<PortData, Tuple<PortData, dynNode>> Inputs = 
            new Dictionary<PortData, Tuple<PortData, dynNode>>();
        public Dictionary<PortData, HashSet<dynNode>> Outputs = 
            new Dictionary<PortData, HashSet<dynNode>>();

        private Dictionary<PortData, Tuple<PortData, dynNode>> previousInputPortMappings = 
            new Dictionary<PortData, Tuple<PortData, dynNode>>();
        private Dictionary<PortData, HashSet<dynNode>> previousOutputPortMappings = 
            new Dictionary<PortData, HashSet<dynNode>>();
        
        /// <summary>
        /// Should changes be reported to the containing workspace?
        /// </summary>
        private bool _report = true;

        protected Value oldValue;
        protected internal ExecutionEnvironment macroEnvironment = null;

        //TODO: don't make this static (maybe)
        protected dynBench Bench
        {
            get { return dynSettings.Bench; }
        }

        protected DynamoController Controller
        {
            get { return dynSettings.Controller; }
        }

        protected internal static HashSet<string> _taggedSymbols = new HashSet<string>();
        protected internal static bool _startTag = false;

        //private bool __isDirty = true;
        private bool _isDirty = true;
        //{
        //    get { return __isDirty; }
        //    set
        //    {
        //        __isDirty = value;
        //        Dispatcher.BeginInvoke(new Action(() => dirtyEllipse.Fill = __isDirty ? Brushes.Red : Brushes.Green));
        //    }
        //}
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
                else
                {
                    //TODO: move this entirely to dynFunction?
                    bool start = _startTag;
                    _startTag = true;

                    bool dirty = Inputs.Values.Any(x => x.Item2.RequiresRecalc);
                    _isDirty = dirty;

                    if (!start)
                    {
                        _startTag = false;
                        _taggedSymbols.Clear();
                    }

                    return dirty;
                }
            }
            set
            {
                _isDirty = value;
                if (value && _report && WorkSpace != null)
                    WorkSpace.Modified();
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
                   && InPortData.All(HasInput);
            }
            set
            {
                _saveResult = value;
            }
        }

        public dynNode()
        {
            InPortData = new List<PortData>();
            OutPortData = new List<PortData>();
            NodeUI = new dynNodeUI(this);
        }

        /// <summary>
        /// Check current ports against ports used for previous mappings.
        /// </summary>
        void CheckPortsForRecalc()
        {
            RequiresRecalc = InPortData.Any(
               delegate(PortData input)
               {
                   Tuple<PortData, dynNode> oldInput;
                   Tuple<PortData, dynNode> currentInput;

                   //this is dirty if there wasn't anything set last time (implying it was never run)...
                   return !previousInputPortMappings.TryGetValue(input, out oldInput)
                       || oldInput == null
                       || !TryGetInput(input, out currentInput)
                       //or If what's set doesn't match
                       || (oldInput.Item2 != currentInput.Item2 && oldInput.Item1 != currentInput.Item1);
               })
            || OutPortData.Any(
               delegate(PortData output)
               {
                   HashSet<dynNode> oldOutputs;
                   HashSet<dynNode> newOutputs;

                   return !previousOutputPortMappings.TryGetValue(output, out oldOutputs)
                       || !TryGetOutput(output, out newOutputs)
                       || oldOutputs.SetEquals(newOutputs);
               });
        }

        /// <summary>
        /// Override this to implement custom save data for your Element. If overridden, you should also override
        /// LoadElement() in order to read the data back when loaded.
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole workspace containing this Element.</param>
        /// <param name="dynEl">The XmlElement representing this Element.</param>
        public virtual void SaveElement(System.Xml.XmlDocument xmlDoc, System.Xml.XmlElement dynEl)
        {

        }

        /// <summary>
        /// Override this to implement loading of custom data for your Element. If overridden, you should also override
        /// SaveElement() in order to write the data when saved.
        /// </summary>
        /// <param name="elNode">The XmlNode representing this Element.</param>
        public virtual void LoadElement(System.Xml.XmlNode elNode)
        {

        }

        /// <summary>
        /// Forces the node to refresh it's dirty state by checking all inputs.
        /// </summary>
        public void MarkDirty()
        {
            bool dirty = false;
            foreach (var input in Inputs.Values)
            {
                input.Item2.MarkDirty();
                if (input.Item2.RequiresRecalc)
                    dirty = true;
            }
            if (!_isDirty)
                _isDirty = dirty;
            return;
        }

        internal virtual INode BuildExpression(Dictionary<dynNode, Dictionary<PortData, INode>> buildDict)
        {
            if (OutPortData.Count > 1)
            {
                var names = OutPortData.Select(x => x.NickName);
                var listNode = new FunctionNode("list", names);
                foreach (var data in OutPortData)
                {
                    listNode.ConnectInput(data.NickName, Build(buildDict, data));
                }
                return listNode;
            }
            else
                return Build(buildDict, OutPortData[0]);
        }

        //TODO: do all of this as the Ui is modified, simply return this?
        /// <summary>
        /// Builds an INode out of this Element. Override this or Compile() if you want complete control over this Element's
        /// execution.
        /// </summary>
        /// <returns>The INode representation of this Element.</returns>
        protected internal virtual INode Build(Dictionary<dynNode, Dictionary<PortData, INode>> preBuilt, PortData outPort)
        {
            Dictionary<PortData, INode> result;
            if (preBuilt.TryGetValue(this, out result))
                return result[outPort];

            //Fetch the names of input ports.
            var portNames = InPortData.Select(x => x.NickName);

            //Compile the procedure for this node.
            InputNode node = Compile(portNames);

            //Is this a partial application?
            var partial = false;

            var partialSymList = new List<PortData>();

            //For each index in InPortData
            //for (int i = 0; i < InPortData.Count; i++)
            foreach (PortData data in InPortData)
            {
                //Fetch the corresponding port
                //var port = InPorts[i];

                Tuple<PortData, dynNode> input;

                //If this port has connectors...
                //if (port.Connectors.Any())
                if (TryGetInput(data, out input))
                {
                    //Compile input and connect it
                    node.ConnectInput(data.NickName, input.Item2.Build(preBuilt, input.Item1));
                }
                else //othwise, remember that this is a partial application
                {
                    partial = true;
                    partialSymList.Add(data);
                }
            }

            var nodes = new Dictionary<PortData, INode>();

            if (OutPortData.Count > 1)
            {
                foreach (var data in partialSymList)
                    node.ConnectInput(data.NickName, new SymbolNode(data.NickName));

                InputNode prev = node;
                int prevIndex = 0;

                foreach (var data in Enumerable.Range(0, OutPortData.Count).Zip(OutPortData, (i, d) => new { Index = i, Data = d }))
                {
                    if (HasOutput(data.Data))
                    {
                        if (data.Index > 0)
                        {
                            var diff = data.Index - prevIndex;
                            InputNode restNode;
                            if (diff > 1)
                            {
                                restNode = new ExternalFunctionNode(FScheme.Drop, new List<string>() { "amt", "list" });
                                restNode.ConnectInput("amt", new NumberNode(diff));
                                restNode.ConnectInput("list", prev);
                            }
                            else
                            {
                                restNode = new ExternalFunctionNode(FScheme.Cdr, new List<string>() { "list" });
                                restNode.ConnectInput("list", prev);
                            }
                            prev = restNode;
                            prevIndex = data.Index;
                        }

                        var firstNode = new ExternalFunctionNode(FScheme.Car, new List<string>() { "list" });
                        firstNode.ConnectInput("list", prev);

                        if (partial)
                            nodes[data.Data] = new AnonymousFunctionNode(partialSymList.Select(x => x.NickName), firstNode);
                        else
                            nodes[data.Data] = firstNode;
                    }
                }
            }
            else
            {
                nodes[OutPortData[0]] = node;
            }

            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                RequiresRecalc = false;
            }
            
            preBuilt[this] = nodes;

            //And we're done
            return nodes[outPort];
        }

        /// <summary>
        /// Compiles this Element into a ProcedureCallNode. Override this instead of Build() if you don't want to set up all
        /// of the inputs for the ProcedureCallNode.
        /// </summary>
        /// <param name="portNames">The names of the inputs to the node.</param>
        /// <returns>A ProcedureCallNode which will then be processed recursively to be connected to its inputs.</returns>
        protected virtual InputNode Compile(IEnumerable<string> portNames)
        {
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
            foreach (PortData data in InPortData)
            {
                Tuple<PortData, dynNode> input;

                previousInputPortMappings[data] = TryGetInput(data, out input)
                   ? input
                   : null;
            }

            foreach (var data in OutPortData)
            {
                HashSet<dynNode> outputs;

                previousOutputPortMappings[data] = TryGetOutput(data, out outputs)
                    ? outputs
                    : new HashSet<dynNode>();
            }
        }

        private Value evalIfDirty(FSharpList<Value> args)
        {
            if (!SaveResult || RequiresRecalc || oldValue == null)
            {
                //Evaluate arguments, then evaluate 
                oldValue = evaluateNode(args);
            }
            else
                OnEvaluate();

            return oldValue;
        }

        private delegate Value innerEvaluationDelegate();

        private static Dictionary<PortData, Value> evaluationDict = new Dictionary<PortData, Value>();

        protected internal virtual Value evaluateNode(FSharpList<Value> args)
        {
            if (SaveResult)
            {
                savePortMappings();
            }

            evaluationDict.Clear();

            object[] iaAttribs = GetType().GetCustomAttributes(typeof(IsInteractiveAttribute), false);
            bool isInteractive = iaAttribs.Length > 0 && ((IsInteractiveAttribute)iaAttribs[0]).IsInteractive;

            innerEvaluationDelegate evaluation = delegate
            {
                Value expr = null;

                if (Controller.RunCancelled)
                    throw new CancelEvaluationException(false);

                try
                {
                    __eval_internal(args, evaluationDict);

                    expr = OutPortData.Count == 1
                        ? evaluationDict[OutPortData[0]]
                        : Value.NewList(
                            Utils.SequenceToFSharpList(
                                evaluationDict.OrderBy(
                                    pair => OutPortData.IndexOf(pair.Key))
                                .Select(
                                    pair => pair.Value)));

                    NodeUI.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            NodeUI.UpdateLayout();
                            NodeUI.ValidateConnections();
                        }
                    ));
                }
                catch (CancelEvaluationException ex)
                {
                    OnRunCancelled();
                    throw ex;
                }
                catch (Exception ex)
                {
                    Bench.Dispatcher.Invoke(new Action(
                       delegate
                       {
                           Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                           Bench.Log(ex);

                           dynSettings.Writer.WriteLine(ex.Message);
                           dynSettings.Writer.WriteLine(ex.StackTrace);

                           Controller.ShowElement(this);
                       }
                    ));

                    NodeUI.Error(ex.Message);
                }

                OnEvaluate();

                RequiresRecalc = false;

                return expr;
            };

            Value result = isInteractive
                ? (Value)NodeUI.Dispatcher.Invoke(evaluation)
                : evaluation();

            if (result != null)
                return result;
            else
                throw new Exception("");
        }

        protected virtual void OnRunCancelled()
        {

        }
        
        protected internal virtual void __eval_internal(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            Evaluate(args, outPuts);
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

        protected internal bool ReportingEnabled { get { return _report; } }

        /// <summary>
        /// Creates a Scheme representation of this dynNode and all connected dynNodes.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            var nick = NodeUI.NickName.Replace(' ', '_');

            if (!InPortData.Any(HasInput))
                return nick;

            string s = "";

            if (InPortData.All(HasInput))
            {
                s += "(" + nick;
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (PortData data in InPortData)
                {
                    Tuple<PortData, dynNode> input;
                    TryGetInput(data, out input);
                    s += " " + input.Item2.PrintExpression();
                }
                s += ")";
            }
            else
            {
                s += "(lambda ("
                   + string.Join(" ", InPortData.Where(x => !HasInput(x)).Select(x => x.NickName))
                   + ") (" + nick;
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (PortData data in InPortData)
                {
                    s += " ";
                    Tuple<PortData, dynNode> input;
                    if (TryGetInput(data, out input))
                        s += input.Item2.PrintExpression();
                    else
                        s += data.NickName;
                }
                s += "))";
            }

            return s;
        }

        internal void ConnectInput(PortData inputData, PortData outputData, dynNode node)
        {
            Inputs[inputData] = Tuple.Create(outputData, node);
            CheckPortsForRecalc();
        }
        
        internal void ConnectOutput(PortData portData, dynNode nodeLogic)
        {
            if (!Outputs.ContainsKey(portData))
                Outputs[portData] = new HashSet<dynNode>();
            Outputs[portData].Add(nodeLogic);
        }

        internal void DisconnectInput(PortData data)
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
        public bool TryGetInput(PortData data, out Tuple<PortData, dynNode> input)
        {
            return Inputs.TryGetValue(data, out input) && input != null;
        }

        public bool TryGetOutput(PortData output, out HashSet<dynNode> newOutputs)
        {
            return Outputs.TryGetValue(output, out newOutputs);
        }

        /// <summary>
        /// Checks if there is an input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        public bool HasInput(PortData data)
        {
            return Inputs.ContainsKey(data) && Inputs[data] != null;
        }

        public bool HasOutput(PortData portData)
        {
            return Outputs.ContainsKey(portData) && Outputs[portData].Any();
        }

        internal void DisconnectOutput(PortData portData, dynNode nodeLogic)
        {
            Outputs[portData].Remove(nodeLogic);
        }
    }

    public abstract class dynNodeWithOneOutput : dynNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
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
    #endregion

    public class PredicateTraverser
    {
        Predicate<dynNode> predicate;

        Dictionary<dynNode, bool> resultDict = new Dictionary<dynNode, bool>();

        bool inProgress;

        public PredicateTraverser(Predicate<dynNode> p)
        {
            predicate = p;
        }

        public bool TraverseUntilAny(dynNode entry)
        {
            inProgress = true;
            bool result = traverseAny(entry);
            resultDict.Clear();
            inProgress = false;
            return result;
        }

        public bool ContinueTraversalUntilAny(dynNode entry)
        {
            if (inProgress)
                return traverseAny(entry);
            else
                throw new Exception("ContinueTraversalUntilAny cannot be used except in a traversal predicate.");
        }

        private bool traverseAny(dynNode entry)
        {
            bool result;
            if (resultDict.TryGetValue(entry, out result))
                return result;

            result = predicate(entry);
            resultDict[entry] = result;
            if (result)
                return result;

            if (entry is dynFunction)
            {
                var symbol = (entry as dynFunction).Symbol;
                if (!dynSettings.Controller.dynFunctionDict.ContainsKey(symbol))
                {
                    dynSettings.Bench.Log("WARNING -- No implementation found for node: " + symbol);
                    entry.NodeUI.Error("Could not find .dyf definition file for this node.");
                    return false;
                }

                result = dynSettings.Controller.dynFunctionDict[symbol]
                    .GetTopMostElements()
                    .Any(ContinueTraversalUntilAny);
            }
            resultDict[entry] = result;
            if (result)
                return result;

            return entry.Inputs.Values.Any(x => x != null && traverseAny(x.Item2));
        }
    }
}

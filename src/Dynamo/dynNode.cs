using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Dynamo.Search;
using Microsoft.FSharp.Collections;

using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.Commands;

using Value = Dynamo.FScheme.Value;


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
        public Dictionary<int, Tuple<int, dynNode>> Inputs = 
            new Dictionary<int, Tuple<int, dynNode>>();
        public Dictionary<int, HashSet<Tuple<int, dynNode>>> Outputs =
            new Dictionary<int, HashSet<Tuple<int, dynNode>>>();

        private Dictionary<int, Tuple<int, dynNode>> previousInputPortMappings = 
            new Dictionary<int, Tuple<int, dynNode>>();
        private Dictionary<int, HashSet<Tuple<int, dynNode>>> previousOutputPortMappings =
            new Dictionary<int, HashSet<Tuple<int, dynNode>>>();

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
                    type.IsSubclassOf(typeof (dynNode)))
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
                else
                {
                    bool dirty = Inputs.Values.Where(x => x != null).Any(x => x.Item2.RequiresRecalc);
                    _isDirty = dirty;

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
                   && Enumerable.Range(0, InPortData.Count).All(HasInput);
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
            RequiresRecalc = Enumerable.Range(0, InPortData.Count).Any(
               delegate(int input)
               {
                   Tuple<int, dynNode> oldInput;
                   Tuple<int, dynNode> currentInput;

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
                   HashSet<Tuple<int, dynNode>> oldOutputs;
                   HashSet<Tuple<int, dynNode>> newOutputs;

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
            foreach (var input in Inputs.Values.Where(x => x != null))
            {
                input.Item2.MarkDirty();
                if (input.Item2.RequiresRecalc)
                    dirty = true;
            }
            if (!_isDirty)
                _isDirty = dirty;
            return;
        }

        internal virtual INode BuildExpression(Dictionary<dynNode, Dictionary<int, INode>> buildDict)
        {
            Debug.WriteLine("Building expression...");

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
        protected internal virtual INode Build(Dictionary<dynNode, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Debug.WriteLine("Building node...");

            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result))
                return result[outPort];

            //Fetch the names of input ports.
            var portNames = InPortData.Zip(Enumerable.Range(0, InPortData.Count), (x, i) => x.NickName + i);

            //Compile the procedure for this node.
            InputNode node = Compile(portNames);

            //Is this a partial application?
            var partial = false;

            var partialSymList = new List<string>();

            //For each index in InPortData
            //for (int i = 0; i < InPortData.Count; i++)
            foreach (var data in Enumerable.Range(0, InPortData.Count).Zip(portNames, (data, name) => new { Index=data, Name=name }))
            {
                //Fetch the corresponding port
                //var port = InPorts[i];

                Tuple<int, dynNode> input;

                //If this port has connectors...
                //if (port.Connectors.Any())
                if (TryGetInput(data.Index, out input))
                {
                    Debug.WriteLine(string.Format("Connecting input {0}", data.Name));

                    //Compile input and connect it
                    node.ConnectInput(data.Name, input.Item2.Build(preBuilt, input.Item1));
                }
                else //othwise, remember that this is a partial application
                {
                    partial = true;
                    partialSymList.Add(data.Name);
                }
            }

            var nodes = new Dictionary<int, INode>();

            if (OutPortData.Count > 1)
            {
                foreach (var data in partialSymList)
                    node.ConnectInput(data, new SymbolNode(data));

                InputNode prev = node;
                int prevIndex = 0;

                foreach (var data in Enumerable.Range(0, OutPortData.Count).Zip(OutPortData, (i, d) => new { Index = i, Data = d }))
                {
                    if (HasOutput(data.Index))
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
                            nodes[data.Index] = new AnonymousFunctionNode(partialSymList, firstNode);
                        else
                            nodes[data.Index] = firstNode;
                    }
                }
            }
            else
            {
                nodes[outPort] = node;
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
            Debug.WriteLine(string.Format("Compiling InputNode with ports {0}.", string.Join(",", portNames)));

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
                Tuple<int, dynNode> input;

                previousInputPortMappings[data] = TryGetInput(data, out input)
                   ? input
                   : null;
            }

            foreach (var data in Enumerable.Range(0, OutPortData.Count))
            {
                HashSet<Tuple<int, dynNode>> outputs;

                previousOutputPortMappings[data] = TryGetOutput(data, out outputs)
                    ? outputs
                    : new HashSet<Tuple<int, dynNode>>();
            }
        }

        private Value evalIfDirty(FSharpList<Value> args)
        {
            if (oldValue == null || !SaveResult || RequiresRecalc)
            {
                //Evaluate arguments, then evaluate 
                oldValue = evaluateNode(args);
            }
            else
                OnEvaluate();

            return oldValue;
        }

        private delegate Value innerEvaluationDelegate();

        private Dictionary<PortData, Value> evaluationDict = new Dictionary<PortData, Value>();

        protected internal virtual Value evaluateNode(FSharpList<Value> args)
        {
            Debug.WriteLine("Evaluating node...");

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

                           //dynSettings.Writer.WriteLine(ex.Message);
                           //dynSettings.Writer.WriteLine(ex.StackTrace);

                           if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                           {
                               DynamoCommands.WriteToLogCmd.Execute(ex.Message);
                               DynamoCommands.WriteToLogCmd.Execute(ex.StackTrace);
                           }

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
            var argList = new List<string>();
            if (args.Count() > 0)
            {
                argList = args.Select(x => x.ToString()).ToList<string>();
            }
            var outPutsList = new List<string>();
            if(outPuts.Count() > 0)
            {
                outPutsList = outPuts.Keys.Select(x=>x.NickName).ToList<string>();
            }

            Debug.WriteLine(string.Format("__eval_internal : {0} : {1}", 
                string.Join(",", argList), 
                string.Join(",", outPutsList)));

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

            if (!Enumerable.Range(0, InPortData.Count).Any(HasInput))
                return nick;

            string s = "";

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                s += "(" + nick;
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (int data in Enumerable.Range(0, InPortData.Count))
                {
                    Tuple<int, dynNode> input;
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
                    Tuple<int, dynNode> input;
                    if (TryGetInput(data, out input))
                        s += input.Item2.PrintExpression();
                    else
                        s += InPortData[data].NickName;
                }
                s += "))";
            }

            return s;
        }

        internal void ConnectInput(int inputData, int outputData, dynNode node)
        {
            Inputs[inputData] = Tuple.Create(outputData, node);
            CheckPortsForRecalc();
        }
        
        internal void ConnectOutput(int portData, int inputData, dynNode nodeLogic)
        {
            if (!Outputs.ContainsKey(portData))
                Outputs[portData] = new HashSet<Tuple<int, dynNode>>();
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
        public bool TryGetInput(int data, out Tuple<int, dynNode> input)
        {
            return Inputs.TryGetValue(data, out input) && input != null;
        }

        public bool TryGetOutput(int output, out HashSet<Tuple<int, dynNode>> newOutputs)
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

        internal void DisconnectOutput(int portData, int inPortData)
        {
            HashSet<Tuple<int, dynNode>> output;
            if (Outputs.TryGetValue(portData, out output))
                output.RemoveWhere(x => x.Item1 == inPortData);
            CheckPortsForRecalc();
        }

        /// <summary>
        /// Implement on derived classes to cleanup resources when 
        /// </summary>
        public virtual void Cleanup()
        {
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
                var symbol = Guid.Parse((entry as dynFunction).Symbol);
                if (!dynSettings.FunctionDict.ContainsKey(symbol))
                {
                    dynSettings.Bench.Log("WARNING -- No implementation found for node: " + symbol);
                    entry.NodeUI.Error("Could not find .dyf definition file for this node.");
                    return false;
                }

                result = dynSettings.FunctionDict[symbol]
                    .Workspace.GetTopMostNodes().Any(ContinueTraversalUntilAny);
            }
            resultDict[entry] = result;
            if (result)
                return result;

            return entry.Inputs.Values.Any(x => x != null && traverseAny(x.Item2));
        }
    }
}

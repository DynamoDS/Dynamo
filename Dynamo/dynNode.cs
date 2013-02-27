using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Utilities;
using Autodesk.Revit.DB;
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

        //TODO: Make it so by default the outport is registered with the UI, because it's
        //      mandatory to have one.
        public abstract PortData OutPortData { get; }

        /// <summary>
        /// The dynElement's Evaluation Logic.
        /// </summary>
        /// <param name="args">Arguments to the node. You are guaranteed to have as many arguments as you have InPorts at the time it is run.</param>
        /// <returns>An expression that is the result of the Node's evaluation. It will be passed along to whatever the OutPort is connected to.</returns>
        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }

        #endregion

        public dynWorkspace WorkSpace;

        public List<PortData> InPortData { get; private set; }
        public dynNodeUI NodeUI;
        public Dictionary<PortData, dynNode> Inputs = new Dictionary<PortData, dynNode>();

        private Dictionary<PortData, dynNode> previousEvalPortMappings = new Dictionary<PortData, dynNode>();
        
        /// <summary>
        /// Should changes be reported to the containing workspace?
        /// </summary>
        private bool _report = true;

        protected Value oldValue;
        protected internal ExecutionEnvironment macroEnvironment = null;

        //TODO: don't make this static (maybe)
        protected dynBench Bench
        {
            get { return dynSettings.Instance.Bench; }
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
        //        this.Dispatcher.BeginInvoke(new Action(() => this.dirtyEllipse.Fill = __isDirty ? Brushes.Red : Brushes.Green));
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
                if (this._isDirty)
                    return true;
                else
                {
                    //TODO: move this entirely to dynFunction?
                    bool start = _startTag;
                    _startTag = true;

                    bool dirty = this.Inputs.Values.Any(x => x.RequiresRecalc);
                    this._isDirty = dirty;

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
                this._isDirty = value;
                if (value && this._report && this.WorkSpace != null)
                    this.WorkSpace.Modified();
            }
        }

        /// <summary>
        /// Returns if this node requires a recalculation without checking input nodes.
        /// </summary>
        protected internal bool isDirty
        {
            get { return this._isDirty; }
            set { this.RequiresRecalc = value; }
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
                return this._saveResult
                   && this.InPortData.All(this.HasInput);
            }
            set
            {
                this._saveResult = value;
            }
        }

        public dynNode()
        {
            this.InPortData = new List<PortData>();
            this.NodeUI = new dynNodeUI(this);
        }

        /// <summary>
        /// Check current ports against ports used for previous mappings.
        /// </summary>
        void CheckPortsForRecalc()
        {
            this.RequiresRecalc = this.InPortData.Any(
               delegate(PortData input)
               {
                   dynNode oldInput;
                   dynNode currentInput;

                   //this is dirty if there wasn't anything set last time (implying it was never run)...
                   return !this.previousEvalPortMappings.TryGetValue(input, out oldInput)
                       || !this.TryGetInput(input, out currentInput)
                       //or If what's set doesn't match
                       || (oldInput != currentInput);
               }
            );
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
            foreach (var input in this.Inputs.Values)
            {
                input.MarkDirty();
                if (input.RequiresRecalc)
                    dirty = true;
            }
            if (!this._isDirty)
                this._isDirty = dirty;
            return;
        }

        internal virtual INode BuildExpression()
        {
            return Build(new Dictionary<dynNode, INode>());
        }

        //TODO: do all of this as the Ui is modified, simply return this?
        /// <summary>
        /// Builds an INode out of this Element. Override this or Compile() if you want complete control over this Element's
        /// execution.
        /// </summary>
        /// <returns>The INode representation of this Element.</returns>
        protected internal virtual INode Build(Dictionary<dynNode, INode> preBuilt)
        {
            INode result;
            if (preBuilt.TryGetValue(this, out result))
                return result;

            //Fetch the names of input ports.
            var portNames = this.InPortData.Select(x => x.NickName);

            //Compile the procedure for this node.
            InputNode node = this.Compile(portNames);

            //Is this a partial application?
            var partial = false;

            //For each index in InPortData
            //for (int i = 0; i < this.InPortData.Count; i++)
            foreach (PortData data in this.InPortData)
            {
                //Fetch the corresponding port
                //var port = this.InPorts[i];

                dynNode input;

                //If this port has connectors...
                //if (port.Connectors.Any())
                if (this.TryGetInput(data, out input))
                {
                    //Compile input and connect it
                    node.ConnectInput(data.NickName, input.Build(preBuilt));
                }
                else //othwise, remember that this is a partial application
                    partial = true;
            }

            //If this is a partial application, then remember not to re-eval.
            if (partial)
            {
                this.RequiresRecalc = false;
                OnEvaluate();
            }

            result = node;

            preBuilt[this] = result;

            //And we're done
            return result;
        }

        /// <summary>
        /// Compiles this Element into a ProcedureCallNode. Override this instead of Build() if you don't want to set up all
        /// of the inputs for the ProcedureCallNode.
        /// </summary>
        /// <param name="portNames">The names of the inputs to the node.</param>
        /// <returns>A ProcedureCallNode which will then be processed recursively to be connected to its inputs.</returns>
        protected internal virtual InputNode Compile(IEnumerable<string> portNames)
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
            this.savePortMappings();
            this.OnSave();
        }

        private void savePortMappings()
        {
            //Save all of the connection states, so we can check if this is dirty
            foreach (PortData data in this.InPortData)
            {
                dynNode input;

                this.previousEvalPortMappings[data] = this.TryGetInput(data, out input)
                   ? input
                   : null;
            }
        }

        private Value evalIfDirty(FSharpList<Value> args)
        {
            if (SaveResult && (RequiresRecalc || oldValue == null))
            {
                //Evaluate arguments, then evaluate 
                oldValue = evaluateNode(args);
            }
            else
                OnEvaluate();

            return oldValue;
        }

        private delegate Value innerEvaluationDelegate();

        protected internal virtual Value evaluateNode(FSharpList<Value> args)
        {
            if (this.SaveResult)
            {
                this.savePortMappings();
            }

            object[] iaAttribs = this.GetType().GetCustomAttributes(typeof(IsInteractiveAttribute), false);
            bool isInteractive = iaAttribs.Length > 0 && ((IsInteractiveAttribute)iaAttribs[0]).IsInteractive;

            innerEvaluationDelegate evaluation = delegate
            {
                Value expr = null;

                if (Bench.RunCancelled)
                    throw new CancelEvaluationException(false);

                try
                {
                    expr = this.__eval_internal(args);

                    this.NodeUI.Dispatcher.BeginInvoke(new Action(
                        delegate
                        {
                            this.NodeUI.UpdateLayout();
                            this.NodeUI.ValidateConnections();
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
                           Bench.Log(ex.Message);

                           dynSettings.Instance.Writer.WriteLine(ex.Message);
                           dynSettings.Instance.Writer.WriteLine(ex.StackTrace);

                           Bench.ShowElement(this);
                       }
                    ));

                    this.NodeUI.Error(ex.Message);
                }

                this.OnEvaluate();

                this.RequiresRecalc = false;

                return expr;
            };

            Value result = isInteractive
                ? (Value)this.NodeUI.Dispatcher.Invoke(evaluation)
                : evaluation();

            if (result != null)
                return result;
            else
                throw new Exception("");
        }

        protected virtual void OnRunCancelled()
        {

        }
        
        protected internal virtual Value __eval_internal(FSharpList<Value> args)
        {
            return this.Evaluate(args);
        }
        

        /// <summary>
        /// Destroy this dynElement
        /// </summary>
        public virtual void Destroy() { }

        internal void DisableReporting()
        {
            this._report = false;
        }

        internal void EnableReporting()
        {
            this._report = true;
        }

        internal bool ReportingEnabled { get { return _report; } }

        /// <summary>
        /// Creates a Scheme representation of this dynNode and all connected dynNodes.
        /// </summary>
        /// <returns>S-Expression</returns>
        public virtual string PrintExpression()
        {
            var nick = this.NodeUI.NickName.Replace(' ', '_');

            if (!this.InPortData.Any(this.HasInput))
                return nick;

            string s = "";

            if (this.InPortData.All(this.HasInput))
            {
                s += "(" + nick;
                //for (int i = 0; i < this.InPortData.Count; i++)
                foreach (PortData data in this.InPortData)
                {
                    dynNode input;
                    this.TryGetInput(data, out input);
                    s += " " + input.PrintExpression();
                }
                s += ")";
            }
            else
            {
                s += "(lambda ("
                   + string.Join(" ", this.InPortData.Where(x => !this.HasInput(x)).Select(x => x.NickName))
                   + ") (" + nick;
                //for (int i = 0; i < this.InPortData.Count; i++)
                foreach (PortData data in this.InPortData)
                {
                    s += " ";
                    dynNode input;
                    if (this.TryGetInput(data, out input))
                        s += input.PrintExpression();
                    else
                        s += data.NickName;
                }
                s += "))";
            }

            return s;
        }

        internal void Connect(PortData data, dynNode node)
        {
            this.Inputs[data] = node;
            this.CheckPortsForRecalc();
        }

        internal void Disconnect(PortData data)
        {
            this.Inputs[data] = null;
            this.CheckPortsForRecalc();
        }

        /// <summary>
        /// Attempts to get the input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <param name="input">If an input is found, it will be assigned.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        protected bool TryGetInput(PortData data, out dynNode input)
        {
            return this.Inputs.TryGetValue(data, out input) && input != null;
        }

        /// <summary>
        /// Checks if there is an input for a certain port.
        /// </summary>
        /// <param name="data">PortData to look for an input for.</param>
        /// <returns>True if there is an input, false otherwise.</returns>
        protected bool HasInput(PortData data)
        {
            dynNode input;
            return this.TryGetInput(data, out input);
        }
    }

    public abstract class dynRevitNode : dynNode
    {
        //TODO: Move from dynElementSettings to another static area in DynamoRevit
        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return dynSettings.Instance.Doc; }
        }

        private List<List<ElementId>> elements;
        public List<ElementId> Elements
        {
            get
            {
                while (this.elements.Count <= this.runCount)
                    this.elements.Add(new List<ElementId>());
                return this.elements[this.runCount];
            }
            private set
            {
                this.elements[this.runCount] = value;
            }
        }

        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int runCount;

        public dynRevitNode() : base()
        {
            elements = new List<List<ElementId>>() { new List<ElementId>() };
        }

        internal void ResetRuns()
        {
            if (this.runCount > 0)
            {
                PruneRuns(this.runCount);
                this.runCount = 0;
            }
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            this.runCount++;
        }

        //protected internal virtual bool RequiresManualTransaction()
        //{
        //    return this.InPorts.Any(
        //       x =>
        //          x.Connectors.Any() && x.Connectors[0].Start.Owner.RequiresManualTransaction()
        //    );
        //}

        //TODO: return true? is this even necessary?
        //protected internal virtual bool RequiresTransaction()
        //{
        //    object[] attribs = this.GetType().GetCustomAttributes(typeof(RequiresTransactionAttribute), false);

        //    return (attribs.Length > 0 && (attribs[0] as RequiresTransactionAttribute).RequiresTransaction)
        //       || this.InPorts.Any(
        //             x =>
        //                x.Connectors.Any() && x.Connectors[0].Start.Owner.RequiresTransaction()
        //          );
        //}

        internal void PruneRuns(int runCount)
        {
            for (int i = this.elements.Count - 1; i >= runCount; i--)
            {
                var elems = this.elements[i];
                foreach (var e in elems)
                {
                    this.UIDocument.Document.Delete(e);
                }
                elems.Clear();
            }

            if (this.elements.Count > runCount)
            {
                this.elements.RemoveRange(
                   runCount,
                   this.elements.Count - runCount
                );
            }
        }

        protected internal override Value __eval_internal(FSharpList<Value> args)
        {
            Value result = null;

            bool debug = Bench.RunInDebug;

            if (!debug)
            {
                #region no debug

                if (Bench.TransMode == TransactionMode.Manual && !Bench.IsTransactionActive())
                {
                    throw new Exception("A Revit transaction is required in order evaluate this element.");
                }

                Bench.InitTransaction();

                result = this.Evaluate(args);

                foreach (ElementId eid in this.deletedIds)
                {
                    Bench.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete
                    );
                }
                this.deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                Bench.Dispatcher.Invoke(new Action(
                   () =>
                      Bench.Log("Starting a debug transaction for element: " + this.NodeUI.NickName)
                ));

                result = IdlePromise<Value>.ExecuteOnIdle(
                   delegate
                   {
                       Bench.InitTransaction();

                       try
                       {
                           var exp = this.Evaluate(args);

                           foreach (ElementId eid in this.deletedIds)
                           {
                               this.Bench.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete
                               );
                           }
                           this.deletedIds.Clear();

                           Bench.EndTransaction();

                           this.NodeUI.Dispatcher.BeginInvoke(new Action(
                               delegate
                               {
                                   this.NodeUI.UpdateLayout();
                                   this.NodeUI.ValidateConnections();
                               }
                           ));

                           return exp;
                       }
                       catch (Exception ex)
                       {
                           Bench.CancelTransaction();
                           throw ex;
                       }
                   }
                );

                #endregion
            }

            #region Register Elements w/ DMU

            var del = new DynElementUpdateDelegate(this.onDeleted);

            foreach (ElementId id in this.Elements)
                this.Bench.RegisterDeleteHook(id, del);

            #endregion

            return result;
        }

        private List<ElementId> deletedIds = new List<ElementId>();
        protected void DeleteElement(ElementId id, bool hookOnly=false)
        {
            if (!hookOnly)
                this.UIDocument.Document.Delete(id);
            deletedIds.Add(id);
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public override void Destroy()
        {
            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   Bench.InitTransaction();
                   try
                   {
                       this.runCount = 0;
                       foreach (var els in this.elements)
                       {
                           foreach (ElementId e in els)
                           {
                               try
                               {
                                   dynSettings.Instance.Doc.Document.Delete(e);
                               }
                               catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                               {
                                   //TODO: Flesh out?
                               }
                           }
                           //els.Clear();
                       }

                       //clear out the array to avoid object initialization errors
                       elements.Clear();

                       //clear the data tree
                       //dataTree.Clear();
                   }
                   catch (Exception ex)
                   {
                       Bench.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message
                       );
                   }
                   Bench.EndTransaction();
                   this.WorkSpace.Modified();
               },
               true
            );
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = 0;
            foreach (var els in this.elements)
            {
                count += els.RemoveAll(deleted.Contains);
            }

            if (!this.isDirty)
                this.isDirty = count > 0;
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in this.elements)
                els.RemoveAll(x => deleted.Contains(x));
        }
    }

    namespace SyncedNodeExtensions
    {
        public static class ElementSync
        {
            /// <summary>
            /// Registers the given element id with the DMU such that any change in the element will
            /// trigger a workspace modification event (dynamic running and saving).
            /// </summary>
            /// <param name="id">ElementId of the element to watch.</param>
            public static void RegisterEvalOnModified(this dynNode node, ElementId id, Action modAction = null, Action delAction = null)
            {
                var u = dynSettings.Instance.Bench.Updater;
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Modify,
                   ReEvalOnModified(node, modAction)
                );
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Delete,
                   UnRegOnDelete(delAction)
                );
            }

            /// <summary>
            /// Unregisters the given element id with the DMU. Should not be called unless it has already
            /// been registered with RegisterEvalOnModified
            /// </summary>
            /// <param name="id">ElementId of the element to stop watching.</param>
            public static void UnregisterEvalOnModified(this dynNode node, ElementId id)
            {
                var u = dynSettings.Instance.Bench.Updater;
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Modify
                );
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Delete
                );
            }

            static DynElementUpdateDelegate UnRegOnDelete(Action deleteAction)
            {
                return delegate(List<ElementId> deleted)
                {
                    foreach (var d in deleted)
                    {
                        var u = dynSettings.Instance.Bench.Updater;
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Delete);
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Modify);
                    }
                    if (deleteAction != null)
                        deleteAction();
                };
            }

            static DynElementUpdateDelegate ReEvalOnModified(dynNode node, Action modifiedAction)
            {
                return delegate(List<ElementId> modified)
                {
                    if (!node.RequiresRecalc && !dynSettings.Instance.Bench.Running)
                    {
                        if (modifiedAction != null)
                            modifiedAction();
                        node.RequiresRecalc = true;
                    }
                };
            }
        }
    }

    #region class attributes
    [AttributeUsage(AttributeTargets.All)]
    public class NodeNameAttribute : System.Attribute
    {
        public string Name { get; set; }

        public NodeNameAttribute(string elementName)
        {
            this.Name = elementName;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : System.Attribute
    {
        public string ElementCategory { get; set; }

        public NodeCategoryAttribute(string category)
        {
            this.ElementCategory = category;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeSearchTagsAttribute : System.Attribute
    {
        public List<string> Tags { get; set; }

        public NodeSearchTagsAttribute(params string[] tags)
        {
            this.Tags = tags.ToList();
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class IsInteractiveAttribute : System.Attribute
    {
        public bool IsInteractive { get; set; }

        public IsInteractiveAttribute(bool isInteractive)
        {
            this.IsInteractive = isInteractive;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class NodeDescriptionAttribute : System.Attribute
    {
        private string description;

        public string ElementDescription
        {
            get { return description; }
            set { description = value; }
        }

        public NodeDescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
    #endregion

    public class PredicateTraverser
    {
        Predicate<dynNode> predicate;

        Dictionary<dynNode, bool> resultDict = new Dictionary<dynNode, bool>();

        public PredicateTraverser(Predicate<dynNode> p)
        {
            this.predicate = p;
        }

        public bool TraverseUntilAny(dynNode entry)
        {
            bool result = traverseAny(entry);
            resultDict.Clear();
            return result;
        }

        private bool traverseAny(dynNode entry)
        {
            bool result;
            if (resultDict.TryGetValue(entry, out result))
                return result;

            result = predicate(entry);
            if (result)
            {
                return result;
            }
            resultDict[entry] = result;

            return entry.Inputs.Values.Any(x => x != null && traverseAny(x));
        }

        public bool TraverseForAll(dynNode entry)
        {
            bool result = traverseAll(entry);
            resultDict.Clear();
            return result;
        }

        private bool traverseAll(dynNode entry)
        {
            bool result;
            if (resultDict.TryGetValue(entry, out result))
                return result;

            result = predicate(entry);
            if (!result)
                return result;
            resultDict[entry] = result;

            return entry.Inputs.Values.Any(x => x != null && traverseAll(x));
        }
    }
}

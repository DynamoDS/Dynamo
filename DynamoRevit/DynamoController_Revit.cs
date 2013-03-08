using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Nodes;
using Dynamo.Revit;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Nodes.PythonNode;
using Binding = Dynamo.Nodes.PythonNode.Binding;
using Value = Dynamo.FScheme.Value;

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        public DynamoUpdater Updater { get; private set; }

        PredicateTraverser checkManualTransaction;
        PredicateTraverser checkRequiresTransaction;

        public DynamoController_Revit(DynamoUpdater updater, SplashScreen splash)
            : base(splash)
        {
            Updater = updater;

            dynRevitSettings.Controller = this;

            Predicate<dynNode> manualTransactionPredicate = delegate(dynNode node)
            {
                return node is dynTransaction;
            };
            checkManualTransaction = new PredicateTraverser(manualTransactionPredicate);

            Predicate<dynNode> requiresTransactionPredicate = delegate(dynNode node)
            {
                return node is dynRevitTransactionNode;
            };
            checkRequiresTransaction = new PredicateTraverser(requiresTransactionPredicate);

            AddPythonBindings();
            AddWatchNodeHandler();
        }

        #region Python Nodes Revit Hooks
        private delegate void LogDelegate(string msg);
        private delegate void SaveElementDelegate(Autodesk.Revit.DB.Element e);

        void AddPythonBindings()
        {
            try
            {
                var pyBindings = PythonBindings.Bindings;

                pyBindings.Add(new Binding("DynLog", new LogDelegate(Bench.Log))); //Logging

                pyBindings.Add(new Binding(
                   "DynTransaction",
                   new Func<Autodesk.Revit.DB.SubTransaction>(
                      delegate
                      {
                          if (!dynRevitSettings.Controller.IsTransactionActive())
                          {
                              dynRevitSettings.Controller.InitTransaction();
                          }
                          return new Autodesk.Revit.DB.SubTransaction(dynRevitSettings.Doc.Document);
                      }
                   )
                ));
                pyBindings.Add(new Binding("__revit__", dynRevitSettings.Doc.Application));
                pyBindings.Add(new Binding("__doc__", dynRevitSettings.Doc.Application.ActiveUIDocument.Document));

                var oldPyEval = PythonEngine.Evaluator;

                PythonEngine.Evaluator = delegate(bool dirty, string script, IEnumerable<Binding> bindings)
                {
                    bool transactionRunning = Transaction != null && Transaction.GetStatus() == TransactionStatus.Started;

                    Value result = null;

                    if (dynRevitSettings.Controller.InIdleThread)
                        result = oldPyEval(dirty, script, bindings);
                    else
                    {
                        result = IdlePromise<Value>.ExecuteOnIdle(
                           () => oldPyEval(dirty, script, bindings));
                    }

                    if (transactionRunning)
                    {
                        if (!IsTransactionActive())
                        {
                            InitTransaction();
                        }
                        else
                        {
                            var ts = Transaction.GetStatus();
                            if (ts != TransactionStatus.Started)
                            {
                                if (ts != TransactionStatus.RolledBack)
                                    CancelTransaction();
                                InitTransaction();
                            }
                        }
                    }
                    else if (RunInDebug)
                    {
                        if (IsTransactionActive())
                            EndTransaction();
                    }

                    return result;
                };
                // use this to pass into the python script a list of previously created elements from dynamo
                //TODO: ADD BACK IN
                //bindings.Add(new Binding("DynStoredElements", this.Elements));
            }
            catch { }
        }
        #endregion
        #region Watch Node Revit Hooks
        void AddWatchNodeHandler()
        {
            dynWatch.AddWatchHandler(new RevitElementWatchHandler());
        }

        private class RevitElementWatchHandler : WatchHandler
        {
            #region WatchHandler Members

            public bool AcceptsValue(object o)
            {
                return o is Element;
            }

            public void ProcessNode(object value, WatchNode node)
            {
                var element = value as Element;
                var id = element.Id;

                node.Clicked += delegate
                {
                    dynRevitSettings.Doc.ShowElements(element);
                };

                node.Link = id.ToString();
            }

            #endregion
        }
        #endregion

        public override bool RunInDebug
        {
            get
            {
                return this.TransMode == TransactionMode.Debug;
            }
        }

        public bool InIdleThread;

        public enum TransactionMode
        {
            Automatic,
            Manual,
            Debug
        }
        public TransactionMode TransMode;

        public override bool DynamicRunEnabled
        {
            get
            {
                var result = base.DynamicRunEnabled;

                bool manTran = ExecutionRequiresManualTransaction();

                Bench.dynamicCheckBox.IsEnabled = !manTran && Bench.debugCheckBox.IsChecked == false;
                if (manTran)
                    Bench.dynamicCheckBox.IsChecked = false;

                return !manTran && result;
            }
        }

        bool ExecutionRequiresManualTransaction()
        {
            return homeSpace.GetTopMostNodes().Any(
                checkManualTransaction.TraverseUntilAny
            );
        }

        private List<Autodesk.Revit.DB.ElementId> _transElements = new List<Autodesk.Revit.DB.ElementId>();

        private Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate> _transDelElements
           = new Dictionary<Autodesk.Revit.DB.ElementId, DynElementUpdateDelegate>();

        internal void RegisterSuccessfulDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            this._transDelElements[id] = d;
        }

        private void CommitDeletions()
        {
            var delDict = new Dictionary<DynElementUpdateDelegate, List<Autodesk.Revit.DB.ElementId>>();
            foreach (var kvp in this._transDelElements)
            {
                if (!delDict.ContainsKey(kvp.Value))
                {
                    delDict[kvp.Value] = new List<Autodesk.Revit.DB.ElementId>();
                }
                delDict[kvp.Value].Add(kvp.Key);
            }

            foreach (var kvp in delDict)
                kvp.Key(kvp.Value);
        }

        internal void RegisterDeleteHook(Autodesk.Revit.DB.ElementId id, DynElementUpdateDelegate d)
        {
            DynElementUpdateDelegate del = delegate(List<Autodesk.Revit.DB.ElementId> deleted)
            {
                var valid = new List<Autodesk.Revit.DB.ElementId>();
                var invalid = new List<Autodesk.Revit.DB.ElementId>();
                foreach (var delId in deleted)
                {
                    try
                    {
                        Autodesk.Revit.DB.Element e = dynRevitSettings.Doc.Document.GetElement(delId);
                        if (e != null)
                        {
                            valid.Add(e.Id);
                        }
                        else
                            invalid.Add(delId);
                    }
                    catch
                    {
                        invalid.Add(delId);
                    }
                }
                valid.Clear();
                d(invalid);
                foreach (var invId in invalid)
                {
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Add);
                    this.Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Delete);
                }
            };

            DynElementUpdateDelegate mod = delegate(List<Autodesk.Revit.DB.ElementId> modded)
            {
                _transElements.RemoveAll(modded.Contains);

                foreach (var mid in modded)
                {
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Modify);
                    this.Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Add);
                }
            };

            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Delete, del
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Modify, mod
            );
            this.Updater.RegisterChangeHook(
               id, ChangeTypeEnum.Add, mod
            );
            this._transElements.Add(id);
        }

        private Transaction _trans;
        public void InitTransaction()
        {
            if (_trans == null || _trans.GetStatus() != TransactionStatus.Started)
            {
                _trans = new Transaction(
                   dynRevitSettings.Doc.Document,
                   "Dynamo Script"
                );
                _trans.Start();

                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new DynamoWarningPrinter(this.Bench));
                _trans.SetFailureHandlingOptions(failOpt);
            }
        }

        public Transaction Transaction { get { return this._trans; } }

        public void EndTransaction()
        {
            if (_trans != null)
            {
                if (_trans.GetStatus() == TransactionStatus.Started)
                {
                    _trans.Commit();
                    _transElements.Clear();
                    CommitDeletions();
                    _transDelElements.Clear();
                }
                _trans = null;
            }
        }

        public void CancelTransaction()
        {
            if (_trans != null)
            {
                _trans.RollBack();
                _trans = null;
                this.Updater.RollBack(this._transElements);
                this._transElements.Clear();
                this._transDelElements.Clear();
            }
        }

        public bool IsTransactionActive()
        {
            return _trans != null;
        }

        protected override void OnRunCancelled(bool error)
        {
            this.CancelTransaction();
        }

        protected override void OnEvaluationCompleted()
        {
            //Cleanup Delegate
            Action cleanup = delegate
            {
                this.InitTransaction(); //Initialize a transaction (if one hasn't been aleady)

                //Reset all elements
                foreach (var element in this.AllNodes)
                {
                    if (element is dynRevitTransactionNode)
                        (element as dynRevitTransactionNode).ResetRuns();
                }

                //////
                /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                //////

                this.EndTransaction(); //Close global transaction.
            };

            //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
            //from the idle thread. Otherwise, just run it in this thread.
            if (RunInDebug || !InIdleThread)
                IdlePromise.ExecuteOnIdle(cleanup, false);
            else
                cleanup();
        }

        protected override void Run(IEnumerable<dynNode> topElements, FScheme.Expression runningExpression)
        {
            //If we are not running in debug...
            if (!_debug)
            {
                //Do we need manual transaction control?
                bool manualTrans = topElements.Any(checkManualTransaction.TraverseUntilAny);

                //Can we avoid running everything in the Revit Idle thread?
                bool noIdleThread = manualTrans || !topElements.Any(checkRequiresTransaction.TraverseUntilAny);

                //If we don't need to be in the idle thread...
                if (noIdleThread)
                {
                    this.TransMode = TransactionMode.Manual; //Manual transaction control
                    this.InIdleThread = false; //Not in idle thread at the moment
                    base.Run(topElements, runningExpression); //Just run the Run Delegate
                }
                else //otherwise...
                {
                    this.TransMode = TransactionMode.Automatic; //Automatic transaction control
                    this.InIdleThread = true; //Now in the idle thread.
                    IdlePromise.ExecuteOnIdle(new Action(
                        () => base.Run(topElements, runningExpression)),
                        false); //Execute the Run Delegate in the Idle thread.
                }
            }
            else //If we are in debug mode...
            {
                this.TransMode = TransactionMode.Debug; //Debug transaction control
                this.InIdleThread = true; //Everything will be evaluated in the idle thread.

                Bench.Dispatcher.Invoke(new Action(
                   () => Bench.Log("Running expression in debug.")
                ));

                //Execute the Run Delegate.
                base.Run(topElements, runningExpression);
            }
        }
    }


    public class DynamoWarningPrinter : Autodesk.Revit.DB.IFailuresPreprocessor
    {
        dynBench bench;

        public DynamoWarningPrinter(dynBench b)
        {
            this.bench = b;
        }

        public Autodesk.Revit.DB.FailureProcessingResult PreprocessFailures(Autodesk.Revit.DB.FailuresAccessor failuresAccessor)
        {
            var failList = failuresAccessor.GetFailureMessages();
            foreach (var fail in failList)
            {
                var severity = fail.GetSeverity();
                if (severity == Autodesk.Revit.DB.FailureSeverity.Warning)
                {
                    bench.Log(
                       "!! Warning: " + fail.GetDescriptionText()
                    );
                    failuresAccessor.DeleteWarning(fail);
                }
            }

            return Autodesk.Revit.DB.FailureProcessingResult.Continue;
        }
    }
}

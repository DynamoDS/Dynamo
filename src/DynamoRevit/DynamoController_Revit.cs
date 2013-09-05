using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Greg;
using Transaction = Dynamo.Nodes.Transaction;
using Value = Dynamo.FScheme.Value;

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        public DynamoUpdater Updater { get; private set; }

        dynamic _oldPyEval;

        public PredicateTraverser CheckManualTransaction { get; private set; }
        public PredicateTraverser CheckRequiresTransaction { get; private set; }

        public DynamoController_Revit(FSchemeInterop.ExecutionEnvironment env, DynamoUpdater updater, Type viewModelType, string context)
            : base(env, viewModelType, context)
        {
            Updater = updater;
            
            dynRevitSettings.Controller = this;

            Predicate<NodeModel> requiresTransactionPredicate = node => node is RevitTransactionNode;
            CheckRequiresTransaction = new PredicateTraverser(requiresTransactionPredicate);

            Predicate<NodeModel> manualTransactionPredicate = node => node is Transaction;
            CheckManualTransaction = new PredicateTraverser(manualTransactionPredicate);

            dynSettings.Controller.DynamoViewModel.RequestAuthentication += RegisterSingleSignOn;

            AddPythonBindings();
            AddWatchNodeHandler();

            dynRevitSettings.Revit.Application.DocumentClosed += Application_DocumentClosed;
            dynRevitSettings.Revit.Application.DocumentOpened += Application_DocumentOpened;

            //allow the showing of elements in context
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CanFindNodesFromElements = true;
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.FindNodesFromElements = FindNodesFromSelection;
        }

        /// <summary>
        /// A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private Assembly _singleSignOnAssembly;

        /// <summary>
        /// Callback for registering an authentication provider with the package manager
        /// </summary>
        /// <param name="client">The client, to which the provider will be attached</param>
        void RegisterSingleSignOn(PackageManagerClient client)
        {
            if (_singleSignOnAssembly == null)
                _singleSignOnAssembly = LoadSSONet();
            client.Client.Provider = new RevitOxygenProvider();
        }

        /// <summary>
        /// Delay loading of the SSONet.dll, which is used by the package manager for 
        /// get authentication information.  Internally uses Assembly.LoadFrom so the DLL
        /// will be loaded into the Load From context or extracted from the Load context
        /// if already present there.
        /// </summary>
        /// <returns>The SSONet assembly</returns>
        public Assembly LoadSSONet()
        {
            // get the location of RevitAPI assembly.  SSONet is in the same directory.
            var revitAPIAss = Assembly.GetAssembly(typeof(Autodesk.Revit.DB.XYZ)); // any type loaded from RevitAPI
            var revitAPIDir = Path.GetDirectoryName(revitAPIAss.Location);

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = Path.Combine(revitAPIDir, "SSONET.dll");

            //Load the assembly from the specified path. 					
            return Assembly.LoadFrom(strTempAssmbPath);
            
        }

        void FindNodesFromSelection()
        {
            var selectedIds = dynRevitSettings.Doc.Selection.Elements.Cast<Element>().Select(x => x.Id);
            var transNodes = dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes.OfType<RevitTransactionNode>();
            var foundNodes = transNodes.Where(x => x.AllElements.Intersect(selectedIds).Any()).ToList();

            if (foundNodes.Any())
            {
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(
                    this, 
                    new ModelEventArgs(foundNodes.First(), null));
                DynamoSelection.Instance.ClearSelection();
                foundNodes.ForEach(DynamoSelection.Instance.Selection.Add);
            }
        }

        void Application_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            //when a document is opened 
            if (dynRevitSettings.Doc == null)
            {
                dynRevitSettings.Doc = dynRevitSettings.Revit.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;
            }
        }

        void Application_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
        {
            //Disable running against revit without a document
            if (dynRevitSettings.Revit.ActiveUIDocument == null)
            {
                dynRevitSettings.Doc = null;
                DynamoViewModel.RunEnabled = false;
            }
            else
            {
                dynRevitSettings.Doc = dynRevitSettings.Revit.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;
            }
        }

        #region Python Nodes Revit Hooks
        private delegate void LogDelegate(string msg);
        private delegate void SaveElementDelegate(Element e);

        void AddPythonBindings()
        {
            try
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Assembly ironPythonAssembly = null;

                string path;

                if (File.Exists(path = Path.Combine(assemblyPath, "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }
                else if (File.Exists(path = Path.Combine(assemblyPath, "Packages", "IronPython", "DynamoPython.dll")))
                {
                    ironPythonAssembly = Assembly.LoadFrom(path);
                }

                if (ironPythonAssembly == null)
                    throw new Exception();

                var pythonBindings = ironPythonAssembly.GetType("DynamoPython.PythonBindings");

                var pyBindingsProperty = pythonBindings.GetProperty("Bindings");
                var pyBindings = pyBindingsProperty.GetValue(null, null);

                var binding = ironPythonAssembly.GetType("DynamoPython.Binding");

                Func<string, object, object> createBinding =
                    (name, boundObject) =>
                        Activator.CreateInstance(binding, new[] { name, boundObject });

                Action<string, object> addToBindings =
                    (name, boundObject) =>
                        pyBindings.GetType()
                                  .InvokeMember(
                                      "Add", BindingFlags.InvokeMethod, null, pyBindings,
                                      new[] { createBinding(name, boundObject) });

                addToBindings("DynLog", new LogDelegate(DynamoLogger.Instance.Log)); //Logging

                addToBindings(
                   "DynTransaction",
                   new Func<SubTransaction>(
                      delegate
                      {
                          if (!dynRevitSettings.Controller.IsTransactionActive())
                          {
                              dynRevitSettings.Controller.InitTransaction();
                          }
                          return new SubTransaction(dynRevitSettings.Doc.Document);
                      }));

                addToBindings("__revit__", dynRevitSettings.Doc.Application);
                addToBindings("__doc__", dynRevitSettings.Doc.Application.ActiveUIDocument.Document);

                var pythonEngine = ironPythonAssembly.GetType("DynamoPython.PythonEngine");
                var evaluatorField = pythonEngine.GetField("Evaluator");

                _oldPyEval = evaluatorField.GetValue(null);

                //var x = PythonEngine.GetMembers();
                //foreach (var y in x)
                //    Console.WriteLine(y);

                var evalDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+EvaluationDelegate");

                Delegate d = Delegate.CreateDelegate(
                    evalDelegateType,
                    this,
                    typeof(DynamoController_Revit)
                        .GetMethod("newEval", BindingFlags.NonPublic | BindingFlags.Instance));

                evaluatorField.SetValue(
                    null,
                    d);

                var drawingField = pythonEngine.GetField("Drawing");
                var drawDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+DrawDelegate");
                Delegate draw = Delegate.CreateDelegate(
                    drawDelegateType,
                    this,
                    typeof(DynamoController_Revit)
                        .GetMethod("DrawPython", BindingFlags.NonPublic | BindingFlags.Instance));

                drawingField.SetValue(null, draw);

                // use this to pass into the python script a list of previously created elements from dynamo
                //TODO: ADD BACK IN
                //bindings.Add(new Binding("DynStoredElements", this.Elements));

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        void DrawPython(Value val, RenderDescription rd)
        {
            DrawContainers(val, rd);
        }

        private void DrawContainers(Value val, RenderDescription rd)
        {
            if (val.IsList)
            {
                foreach (Value v in ((Value.List)val).Item)
                {
                    DrawContainers(v, rd);
                }
            }
            if (val.IsContainer)
            {
                var drawable = ((Value.Container)val).Item;

                if(drawable is XYZ)
                {
                    RevitTransactionNode.DrawXYZ(rd, drawable);
                }
                else if (drawable is GeometryObject)
                {
                    RevitTransactionNode.DrawGeometryObject(rd, drawable);
                }
            }
        }

        Value newEval(bool dirty, string script, dynamic bindings)
        {
            bool transactionRunning = Transaction != null && Transaction.GetStatus() == TransactionStatus.Started;

            Value result = null;

            if (dynRevitSettings.Controller.InIdleThread)
                result = _oldPyEval(dirty, script, bindings);
            else
            {
                result = IdlePromise<Value>.ExecuteOnIdle(
                   () => _oldPyEval(dirty, script, bindings));
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
            else if (DynamoViewModel.RunInDebug)
            {
                if (IsTransactionActive())
                    EndTransaction();
            }

            return result;
        }
        #endregion

        #region Watch Node Revit Hooks
        void AddWatchNodeHandler()
        {
            Watch.AddWatchHandler(new RevitElementWatchHandler());
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

        public bool InIdleThread;

        private readonly List<ElementId> _transElements = new List<ElementId>();

        private readonly Dictionary<DynElementUpdateDelegate, HashSet<ElementId>> _transDelElements
           = new Dictionary<DynElementUpdateDelegate, HashSet<ElementId>>();

        internal void RegisterSuccessfulDeleteHook(ElementId id, DynElementUpdateDelegate updateDelegate)
        {
            HashSet<ElementId> elements;
            if (!_transDelElements.TryGetValue(updateDelegate, out elements))
            {
                elements = new HashSet<ElementId>();
                _transDelElements[updateDelegate] = elements;
            }
            elements.Add(id);
        }

        private void CommitDeletions()
        {
            foreach (var kvp in _transDelElements)
                kvp.Key(kvp.Value);
        }

        internal void RegisterDMUHooks(ElementId id, DynElementUpdateDelegate updateDelegate)
        {
            // Redundancies? Leaving commented out for now. -SJE

            DynElementUpdateDelegate del = delegate(HashSet<ElementId> deleted)
            {
                //var invalid = new HashSet<ElementId>();
                //foreach (var delId in deleted)
                //{
                //    try
                //    {
                //        Element e = dynRevitSettings.Doc.Document.GetElement(delId);
                //        if (e == null)
                //            invalid.Add(delId);
                //    }
                //    catch
                //    {
                //        invalid.Add(delId);
                //    }
                //}
                foreach (var invId in deleted)//invalid)
                {
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Modify);
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Add);
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Delete);
                }
                updateDelegate(deleted);//invalid);
            };

            //DynElementUpdateDelegate mod = delegate(HashSet<ElementId> modded)
            //{
            //    _transElements.RemoveAll(modded.Contains);

            //    foreach (var mid in modded)
            //    {
            //        Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Modify);
            //        Updater.UnRegisterChangeHook(mid, ChangeTypeEnum.Add);
            //    }
            //};

            Updater.RegisterChangeHook(id, ChangeTypeEnum.Delete, del);
            //Updater.RegisterChangeHook(id, ChangeTypeEnum.Modify, mod);
            //Updater.RegisterChangeHook(id, ChangeTypeEnum.Add, mod);
            _transElements.Add(id);
        }

        private Autodesk.Revit.DB.Transaction _trans;
        public void InitTransaction()
        {
            if (_trans == null || _trans.GetStatus() != TransactionStatus.Started)
            {
                _trans = new Autodesk.Revit.DB.Transaction(dynRevitSettings.Doc.Document, "Dynamo Script");
                _trans.Start();

                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new DynamoWarningPrinter());
                _trans.SetFailureHandlingOptions(failOpt);
            }
        }

        public Autodesk.Revit.DB.Transaction Transaction { get { return _trans; } }

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
                Updater.RollBack(_transElements);
                _transElements.Clear();
                _transDelElements.Clear();
            }
        }

        public bool IsTransactionActive()
        {
            return _trans != null;
        }

        private TransactionMode _transMode;
        public TransactionMode TransMode
        {
            get { return _transMode; }
            set
            {
                _transMode = value;
                if (_transMode == TransactionMode.Debug)
                {
                    DynamoViewModel.RunInDebug = true;
                }
            }
        }

        protected override void OnRunCancelled(bool error)
        {
            base.OnRunCancelled(error);

            CancelTransaction();
        }

        protected override void OnEvaluationCompleted()
        {
            base.OnEvaluationCompleted();

            //Cleanup Delegate
            Action cleanup = delegate
            {
                //TODO: perhaps this should occur inside of ResetRuns in the event that
                //      there is nothing to be deleted?
                InitTransaction(); //Initialize a transaction (if one hasn't been aleady)

                //Reset all elements
                var query = dynSettings.Controller.DynamoModel.AllNodes
                    .OfType<RevitTransactionNode>();

                foreach (RevitTransactionNode element in query)
                    element.ResetRuns();

                //////
                /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                //////

                EndTransaction(); //Close global transaction.
            };

            //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
            //from the idle thread. Otherwise, just run it in this thread.
            if (dynSettings.Controller.DynamoViewModel.RunInDebug || !InIdleThread && !Testing)
            {
                IdlePromise.ExecuteOnIdle(cleanup, false);
            }
            else
                cleanup();
        }

        public override void ShutDown()
        {
            base.ShutDown();
            Updater.UnRegisterAllChangeHooks();
        }

        protected override void Run(List<NodeModel> topElements, FScheme.Expression runningExpression)
        {
            var model = (DynamoRevitViewModel)DynamoViewModel;

            //If we are not running in debug...
            if (!DynamoViewModel.RunInDebug)
            {
                //Do we need manual transaction control?
                bool manualTrans = topElements.Any(CheckManualTransaction.TraverseUntilAny);

                //Can we avoid running everything in the Revit Idle thread?
                bool noIdleThread = manualTrans || 
                    !topElements.Any(CheckRequiresTransaction.TraverseUntilAny);

                //If we don't need to be in the idle thread...
                if (noIdleThread || Testing)
                {
                    DynamoLogger.Instance.Log("Running expression in evaluation thread...");
                    TransMode = TransactionMode.Manual; //Manual transaction control

                    if (Testing)
                        TransMode = TransactionMode.Automatic;

                    InIdleThread = false; //Not in idle thread at the moment
                    base.Run(topElements, runningExpression); //Just run the Run Delegate
                }
                else //otherwise...
                {
                    DynamoLogger.Instance.Log("Running expression in Revit's Idle thread...");
                    TransMode = TransactionMode.Automatic; //Automatic transaction control

                    Debug.WriteLine("Adding a run to the idle stack.");
                    InIdleThread = true; //Now in the idle thread.
                    IdlePromise.ExecuteOnIdle(
                        () => base.Run(topElements, runningExpression),
                        false); //Execute the Run Delegate in the Idle thread.
                    
                }
            }
            else //If we are in debug mode...
            {
                TransMode = TransactionMode.Debug; //Debug transaction control
                InIdleThread = true; //Everything will be evaluated in the idle thread.

                DynamoLogger.Instance.Log("Running expression in debug.");

                //Execute the Run Delegate.
                base.Run(topElements, runningExpression);
            }
        }
    }

    public enum TransactionMode
    {
        Debug,
        Manual,
        Automatic
    }

    public class DynamoWarningPrinter : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var failList = failuresAccessor.GetFailureMessages();

            var query = from fail in failList
                        let severity = fail.GetSeverity()
                        where severity == FailureSeverity.Warning
                        select fail;

            foreach (var fail in query)
            {
                DynamoLogger.Instance.Log(
                    "!! Warning: " + fail.GetDescriptionText());
                failuresAccessor.DeleteWarning(fail);
            }

            return FailureProcessingResult.Continue;
        }
    }
}

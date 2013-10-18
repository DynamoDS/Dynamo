using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Greg;
using ProtoScript.Runners;
using RevitPersistenceManager;
using RevitServices;
using RevitServices.Elements;
using RevitServices.Threading;
using RevitServices.Transactions;
using CurveLoop = Autodesk.Revit.DB.CurveLoop;
using Transaction = Dynamo.Nodes.Transaction;
using Value = Dynamo.FScheme.Value;

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        public RevitServicesUpdater Updater { get; private set; }

        private dynamic _oldPyEval;

        public PredicateTraverser CheckManualTransaction { get; private set; }
        public PredicateTraverser CheckRequiresTransaction { get; private set; }

        private ElementId keeperId = ElementId.InvalidElementId;

        /// <summary>
        /// A visualization manager responsible for generating geometry for rendering.
        /// </summary>
        public override VisualizationManager VisualizationManager
        {
            get
            {
                if (visualizationManager == null)
                {
                    visualizationManager = new VisualizationManagerRevit();
                    visualizationManager.VisualizationUpdateComplete +=
                        visualizationManager_VisualizationUpdateComplete;
                    visualizationManager.RequestAlternateContextClear += CleanupVisualizations;
                    dynSettings.Controller.DynamoModel.CleaningUp += CleanupVisualizations;
                }
                return visualizationManager;
            }
        }

        public DynamoController_Revit(ExecutionEnvironment env, RevitServicesUpdater updater, Type viewModelType,
            string context)
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

            TransactionManager = new TransactionManager();
            TransactionManager.TransactionStarted += TransactionManager_TransactionCommitted;
            TransactionManager.TransactionCancelled += TransactionManager_TransactionCancelled;
            TransactionManager.FailuresRaised += TransactionManager_FailuresRaised;
        }


        private void CleanupVisualizations(object sender, EventArgs e)
        {
            IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    dynRevitSettings.Controller.InitTransaction();

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        dynRevitSettings.Doc.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    dynRevitSettings.Controller.EndTransaction();
                });
        }

        /// <summary>
        /// Handler for the visualization manager's VisualizationUpdateComplete event.
        /// Sends goemetry to the GeomKeeper, if available, for preview in Revit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visualizationManager_VisualizationUpdateComplete(object sender, VisualizationEventArgs e)
        {
            //do not draw to geom keeper if the user has selected
            //not to draw to the alternate context or if it is not available
            if (!VisualizationManager.AlternateDrawingContextAvailable ||
                !VisualizationManager.DrawToAlternateContext)
                return;

            var values = dynSettings.Controller.DynamoModel.Nodes
                .Where(x => !(x is SelectionBase))
                .Where(x => x.IsVisible)
                .Where(x => x.OldValue != null)
                .Where(x => x.OldValue is Value.Container || x.OldValue is Value.List)
                .Select(x => x.OldValue);

            var geoms = values.ToList().SelectMany(RevitGeometryFromNodes).ToList();

            DrawToAlternateContext(geoms);
        }

        /// <summary>
        /// Utility method to get the Revit geometry associated with nodes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static List<GeometryObject> RevitGeometryFromNodes(Value value)
        {
            var geoms = new List<GeometryObject>();

            if (value.IsList)
            {
                foreach (var valInner in ((Value.List) value).Item)
                {
                    geoms.AddRange(RevitGeometryFromNodes(valInner));
                }
                return geoms;
            }

            var container = value as Value.Container;
            if (container == null)
                return geoms;

            var geom = ((Value.Container)value).Item as GeometryObject;
            if (geom != null && !(geom is Face))
                geoms.Add(geom);

            var ps = ((Value.Container) value).Item as ParticleSystem;
            if (ps != null)
            {
                geoms.AddRange(
                    ps.Springs.Select(
                        spring =>
                            Line.CreateBound(spring.getOneEnd().getPosition(), spring.getTheOtherEnd().getPosition())));
            }

            var cl = ((Value.Container) value).Item as CurveLoop;
            if (cl != null)
            {
                geoms.AddRange(cl);
            }

            //draw xyzs as Point objects
            var pt = ((Value.Container) value).Item as XYZ;
            if (pt != null)
            {
                Type pointType = typeof (Point);
                MethodInfo[] pointTypeMethods = pointType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                var method = pointTypeMethods.FirstOrDefault(x => x.Name == "CreatePoint");

                if (method != null)
                {
                    var args = new object[3];
                    args[0] = pt.X;
                    args[1] = pt.Y;
                    args[2] = pt.Z;
                    geoms.Add((Point) method.Invoke(null, args));
                }
            }

            return geoms;
        }

        private void DrawToAlternateContext(List<GeometryObject> geoms)
        {
            Type geometryElementType = typeof (GeometryElement);
            MethodInfo[] geometryElementTypeMethods =
                geometryElementType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var method = geometryElementTypeMethods.FirstOrDefault(x => x.Name == "SetForTransientDisplay");

            if (method == null)
            {
                return;
            }

            var styles = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            styles.OfClass(typeof (GraphicsStyle));

            var gStyle = styles.ToElements().FirstOrDefault(x => x.Name == "Dynamo");

            IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    dynRevitSettings.Controller.InitTransaction();

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        dynRevitSettings.Doc.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    var argsM = new object[4];
                    argsM[0] = dynRevitSettings.Doc.Document;
                    argsM[1] = ElementId.InvalidElementId;
                    argsM[2] = geoms;
                    if (gStyle != null)
                        argsM[3] = gStyle.Id;
                    else
                        argsM[3] = ElementId.InvalidElementId;

                    keeperId = (ElementId) method.Invoke(null, argsM);

                    //keeperId = GeometryElement.SetForTransientDisplay(dynRevitSettings.Doc.Document, ElementId.InvalidElementId, geoms,
                    //                                       ElementId.InvalidElementId);

                    dynRevitSettings.Controller.EndTransaction();
                });
        }

        /// <summary>
        /// A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private Assembly _singleSignOnAssembly;

        /// <summary>
        /// Callback for registering an authentication provider with the package manager
        /// </summary>
        /// <param name="client">The client, to which the provider will be attached</param>
        private void RegisterSingleSignOn(PackageManagerClient client)
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
            var revitAPIAss = Assembly.GetAssembly(typeof (XYZ)); // any type loaded from RevitAPI
            var revitAPIDir = Path.GetDirectoryName(revitAPIAss.Location);

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = Path.Combine(revitAPIDir, "SSONET.dll");

            //Load the assembly from the specified path. 					
            return Assembly.LoadFrom(strTempAssmbPath);
        }

        private void FindNodesFromSelection()
        {
            var selectedIds = dynRevitSettings.Doc.Selection.Elements.Cast<Element>().Select(x => x.Id);
            var transNodes = dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes.OfType<RevitTransactionNode>();
            var foundNodes = transNodes.Where(x => x.AllElements.Intersect(selectedIds).Any()).ToList();

            if (foundNodes.Any())
            {
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(
                    this, new ModelEventArgs(foundNodes.First()));

                DynamoSelection.Instance.ClearSelection();
                foundNodes.ForEach(DynamoSelection.Instance.Selection.Add);
            }
        }

        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            //when a document is opened 
            if (dynRevitSettings.Doc == null)
            {
                dynRevitSettings.Doc = dynRevitSettings.Revit.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;
            }
        }

        private void Application_DocumentClosed(object sender, DocumentClosedEventArgs e)
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

        private void AddPythonBindings()
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
                        Activator.CreateInstance(binding, new[] {name, boundObject});

                Action<string, object> addToBindings =
                    (name, boundObject) =>
                        pyBindings.GetType()
                            .InvokeMember(
                                "Add", BindingFlags.InvokeMethod, null, pyBindings,
                                new[] {createBinding(name, boundObject)});

                addToBindings("DynLog", new LogDelegate(DynamoLogger.Instance.Log)); //Logging

                addToBindings(
                    "DynTransaction",
                    new Func<SubTransaction>(
                        delegate
                        {
                            if (!TransactionManager.TransactionActive)
                            {
                                TransactionManager.StartTransaction(dynRevitSettings.Doc.Document);
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
                    typeof (DynamoController_Revit)
                        .GetMethod("newEval", BindingFlags.NonPublic | BindingFlags.Instance));

                evaluatorField.SetValue(
                    null,
                    d);

                var drawingField = pythonEngine.GetField("Drawing");
                var drawDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+DrawDelegate");
                Delegate draw = Delegate.CreateDelegate(
                    drawDelegateType,
                    this,
                    typeof (DynamoController_Revit)
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

        private void DrawPython(Value val, string id)
        {
            //DrawContainers(val, id);
        }

        //private void DrawContainers(Value val, string id)
        //{
        //    if (val.IsList)
        //    {
        //        foreach (Value v in ((Value.List)val).Item)
        //        {
        //            DrawContainers(v, id);
        //        }
        //    }
        //    if (val.IsContainer)
        //    {
        //        var drawable = ((Value.Container)val).Item;

        //        //support drawing XYZs geometry objects or LibG graphic items
        //        if(drawable is XYZ || drawable is GeometryObject || drawable is GraphicItem )
        //        {
        //            VisualizationManager.Visualizations[id].Geometry.Add(drawable);
        //        }
        //    }
        //}

        private Value newEval(bool dirty, string script, dynamic bindings)
        {
            return InIdleThread
                ? _oldPyEval(dirty, script, bindings)
                : IdlePromise<Value>.ExecuteOnIdle(() => _oldPyEval(dirty, script, bindings));
        }

        #endregion

        #region Watch Node Revit Hooks

        private void AddWatchNodeHandler()
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

                node.Link = id.IntegerValue.ToString(CultureInfo.InvariantCulture);
            }

            #endregion
        }

        #endregion

        #region Element Persistence Management

        private readonly List<ElementId> _transElements = new List<ElementId>();

        private readonly Dictionary<ElementUpdateDelegate, HashSet<ElementId>> _transDelElements
            = new Dictionary<ElementUpdateDelegate, HashSet<ElementId>>();

        internal void RegisterSuccessfulDeleteHook(ElementId id, ElementUpdateDelegate updateDelegate)
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

        internal void RegisterDMUHooks(ElementId id, ElementUpdateDelegate updateDelegate)
        {
            ElementUpdateDelegate del = delegate(HashSet<ElementId> deleted)
            {
                foreach (var invId in deleted) //invalid)
                {
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Modify);
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Add);
                    Updater.UnRegisterChangeHook(invId, ChangeTypeEnum.Delete);
                }
                updateDelegate(deleted); //invalid);
            };

            Updater.RegisterChangeHook(id, ChangeTypeEnum.Delete, del);
            _transElements.Add(id);
        }

        #endregion

        #region Revit Transaction Management

        private TransactionManager.TransactionHandle _transaction;

        public TransactionManager TransactionManager { get; private set; }

        private void TransactionManager_TransactionCancelled()
        {
            Updater.RollBack(_transElements);
            _transElements.Clear();
            _transDelElements.Clear();
        }

        private void TransactionManager_TransactionCommitted()
        {
            _transElements.Clear();
            CommitDeletions();
            _transDelElements.Clear();
        }

        private void TransactionManager_FailuresRaised(FailuresAccessor failuresAccessor)
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
        }

        #endregion

        public bool InIdleThread;

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

            if (_transaction != null && _transaction.Status == TransactionStatus.Started)
                _transaction.CancelTransaction();
        }

        protected override void OnEvaluationCompleted(object sender, EventArgs e)
        {
            base.OnEvaluationCompleted(sender, e);

            //Cleanup Delegate
            Action cleanup = delegate
            {
                //TODO: perhaps this should occur inside of ResetRuns in the event that
                //      there is nothing to be deleted?

                //Initialize a transaction (if one hasn't been aleady)
                _transaction = TransactionManager.StartTransaction(dynRevitSettings.Doc.Document);

                //Reset all elements
                var query = dynSettings.Controller.DynamoModel.AllNodes
                    .OfType<RevitTransactionNode>();

                foreach (RevitTransactionNode element in query)
                    element.ResetRuns();

                //////
                /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                //////

                _transaction.CommitTransaction(); //Close global transaction.
            };

            //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
            //from the idle thread. Otherwise, just run it in this thread.
            if (dynSettings.Controller.DynamoViewModel.RunInDebug || !InIdleThread && !Testing)
            {
                IdlePromise.ExecuteOnIdleSync(cleanup);
            }
            else
                cleanup();

        }

        public override void ShutDown()
        {
            IdlePromise.ExecuteOnShutdown(
                delegate
                {
                    var transaction = new Autodesk.Revit.DB.Transaction(dynRevitSettings.Doc.Document, "Dynamo Script");
                    transaction.Start();

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        dynRevitSettings.Doc.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    transaction.Commit();
                }
                );

            base.ShutDown();
            Updater.UnRegisterAllChangeHooks();
        }

        protected override void RunDS()
        {
            DocumentManager.CurrentDoc = dynRevitSettings.Doc.Document;


            //This is prototype grade only
            ProtoRunner runner = new ProtoRunner();
            
            string demoScript = 
                "import(\"DSRevitNodes.dll\");" +
                "y = 40..50..2;" +
                "z = 0..10..5;" +
                "pt = Point.ByCoordinates(10, y<1>, z<2>);" +
                "pt2 = Point.ByCoordinates(10, 10, 10);";



            //IdlePromise.ExecuteOnIdleAsync(() => DSRevitNodes.Point.ByCoordinates(10, 10, 10));
            IdlePromise.ExecuteOnIdleAsync(() =>
                {
                    ProtoRunner.ProtoVMState state = runner.PreStart(demoScript);
                    runner.RunToNextBreakpoint(state);
                });

        }


        protected override void Run(List<NodeModel> topElements, FScheme.Expression runningExpression)
        {
            var model = (DynamoRevitViewModel) DynamoViewModel;

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
                    //DynamoLogger.Instance.Log("Running expression in evaluation thread...");
                    TransMode = TransactionMode.Manual; //Manual transaction control

                    if (Testing)
                        TransMode = TransactionMode.Automatic;

                    InIdleThread = false; //Not in idle thread at the moment
                    base.Run(topElements, runningExpression); //Just run the Run Delegate
                }
                else //otherwise...
                {
                    //DynamoLogger.Instance.Log("Running expression in Revit's Idle thread...");
                    TransMode = TransactionMode.Automatic; //Automatic transaction control

                    Debug.WriteLine("Adding a run to the idle stack.");
                    InIdleThread = true; //Now in the idle thread.
                    IdlePromise.ExecuteOnIdleSync(() => base.Run(topElements, runningExpression)); //Execute the Run Delegate in the Idle thread.

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

        public void InitTransaction()
        {
            _transaction = TransactionManager.StartTransaction(dynRevitSettings.Doc.Document);
        }

        public void EndTransaction()
        {
            _transaction.CommitTransaction();
        }

        public void CancelTransaction()
        {
            _transaction.CancelTransaction();
        }
    }

    public enum TransactionMode
    {
        Debug,
        Manual,
        Automatic
    }
}

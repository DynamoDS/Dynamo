#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;
using DSNodeServices;
using Dynamo.Applications;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.Utilities;
using Greg;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using ChangeType = RevitServices.Elements.ChangeType;
using CurveLoop = Autodesk.Revit.DB.CurveLoop;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;
using RevThread = RevitServices.Threading;
using Transaction = Dynamo.Nodes.Transaction;

#endregion

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        private ElementId keeperId = ElementId.InvalidElementId;

        /// <summary>
        ///     A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private Assembly singleSignOnAssembly;

        public DynamoController_Revit(RevitServicesUpdater updater, Type viewModelType, string context)
            : base(
                viewModelType,
                context,
                new UpdateManager.UpdateManager(),
                new RevitWatchHandler(),
                Dynamo.PreferenceSettings.Load())
        {
            Updater = updater;

            dynRevitSettings.Controller = this;

            Predicate<NodeModel> requiresTransactionPredicate = node => node is RevitTransactionNode;
            CheckRequiresTransaction = new PredicateTraverser(requiresTransactionPredicate);

            Predicate<NodeModel> manualTransactionPredicate = node => node is Transaction;
            CheckManualTransaction = new PredicateTraverser(manualTransactionPredicate);

            dynSettings.Controller.DynamoViewModel.RequestAuthentication += RegisterSingleSignOn;

            AddPythonBindings();

            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosed +=
                Application_DocumentClosed;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentOpened +=
                Application_DocumentOpened;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated += Revit_ViewActivated;

            //allow the showing of elements in context
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.CanFindNodesFromElements = true;
            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.FindNodesFromElements =
                FindNodesFromSelection;

            TransactionWrapper = TransactionManager.Instance.TransactionWrapper;
            TransactionWrapper.TransactionStarted += TransactionManager_TransactionCommitted;
            TransactionWrapper.TransactionCancelled += TransactionManager_TransactionCancelled;
            TransactionWrapper.FailuresRaised += TransactionManager_FailuresRaised;

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));
            ElementNameStore = new Dictionary<ElementId, string>();

            EngineController.ImportLibrary("RevitNodes.dll");
        }

        public RevitServicesUpdater Updater { get; private set; }
        public PredicateTraverser CheckManualTransaction { get; private set; }
        public PredicateTraverser CheckRequiresTransaction { get; private set; }

        /// <summary>
        ///     A dictionary which temporarily stores element names for setting after element deletion.
        /// </summary>
        public Dictionary<ElementId, string> ElementNameStore { get; set; }

        /// <summary>
        ///     A visualization manager responsible for generating geometry for rendering.
        /// </summary>
        public override VisualizationManager VisualizationManager
        {
            get
            {
                if (visualizationManager == null)
                {
                    visualizationManager = new VisualizationManagerRevit(this);

                    visualizationManager.VisualizationUpdateComplete +=
                        visualizationManager_VisualizationUpdateComplete;

                    visualizationManager.RequestAlternateContextClear += CleanupVisualizations;
                    dynSettings.Controller.DynamoModel.CleaningUp += CleanupVisualizations;
                }
                return visualizationManager;
            }
        }

        public bool InIdleThread
        {
            get { return RevThread.IdlePromise.InIdleThread; }
        }

        //TODO: probably don't need this property anymore --SJE
        public TransactionMode TransMode
        {
            get
            {
                if (TransactionManager.Instance.Strategy is AutomaticTransactionStrategy)
                    return TransactionMode.Automatic;

                return TransactionMode.Debug;
            }
            set
            {
                switch (value)
                {
                    case TransactionMode.Automatic:
                        TransactionManager.Instance.Strategy = new AutomaticTransactionStrategy();
                        break;
                    default:
                        TransactionManager.Instance.Strategy = new DebugTransactionStrategy();
                        break;
                }

                DynamoViewModel.RunInDebug = value == TransactionMode.Debug;
            }
        }

        /// <summary>
        ///     The Synchronication Context from the current thread.  This is expected to be the
        ///     Revit UI thread SynchronizationContext
        /// </summary>
        public Dispatcher RevitSyncContext { get; set; }

        private void CleanupVisualizations(object sender, EventArgs e)
        {
            RevThread.IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        /// <summary>
        ///     Handler for the visualization manager's VisualizationUpdateComplete event.
        ///     Sends goemetry to the GeomKeeper, if available, for preview in Revit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visualizationManager_VisualizationUpdateComplete(object sender, EventArgs e)
        {
            //do not draw to geom keeper if the user has selected
            //not to draw to the alternate context or if it is not available
            if (!VisualizationManager.AlternateDrawingContextAvailable
                || !VisualizationManager.DrawToAlternateContext)
                return;

            IEnumerable<FScheme.Value> values = dynSettings.Controller.DynamoModel.Nodes.Where(
                x => !(x is SelectionBase)).Where(x => x.IsVisible).Where(x => x.OldValue != null)
                //.Where(x => x.OldValue is Value.Container || x.OldValue is Value.List)
                .Select(x => x.OldValue.Data as FScheme.Value);

            List<GeometryObject> geoms = values.ToList().SelectMany(RevitGeometryFromNodes).ToList();

            DrawToAlternateContext(geoms);
        }

        /// <summary>
        ///     Utility method to get the Revit geometry associated with nodes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static List<GeometryObject> RevitGeometryFromNodes(FScheme.Value value)
        {
            var geoms = new List<GeometryObject>();

            if (value == null)
                return geoms;

            if (value.IsList)
            {
                foreach (FScheme.Value valInner in ((FScheme.Value.List)value).Item)
                    geoms.AddRange(RevitGeometryFromNodes(valInner));
                return geoms;
            }

            var container = value as FScheme.Value.Container;
            if (container == null)
                return geoms;

            var geom = ((FScheme.Value.Container)value).Item as GeometryObject;
            if (geom != null && !(geom is Face))
                geoms.Add(geom);

            var ps = ((FScheme.Value.Container)value).Item as ParticleSystem;
            if (ps != null)
            {
                geoms.AddRange(
                    ps.Springs.Select(
                        spring =>
                            Line.CreateBound(
                                spring.getOneEnd().getPosition(),
                                spring.getTheOtherEnd().getPosition())));
            }

            var cl = ((FScheme.Value.Container)value).Item as CurveLoop;
            if (cl != null)
                geoms.AddRange(cl);

            //draw xyzs as Point objects
            var pt = ((FScheme.Value.Container)value).Item as XYZ;
            if (pt != null)
            {
                Type pointType = typeof(Point);
                MethodInfo[] pointTypeMethods = pointType.GetMethods(
                    BindingFlags.Static | BindingFlags.Public);
                MethodInfo method = pointTypeMethods.FirstOrDefault(x => x.Name == "CreatePoint");

                if (method != null)
                {
                    var args = new object[3];
                    args[0] = pt.X;
                    args[1] = pt.Y;
                    args[2] = pt.Z;
                    geoms.Add((Point)method.Invoke(null, args));
                }
            }

            return geoms;
        }

        private void DrawToAlternateContext(List<GeometryObject> geoms)
        {
            Type geometryElementType = typeof(GeometryElement);
            MethodInfo[] geometryElementTypeMethods =
                geometryElementType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            MethodInfo method =
                geometryElementTypeMethods.FirstOrDefault(x => x.Name == "SetForTransientDisplay");

            if (method == null)
                return;

            var styles = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            styles.OfClass(typeof(GraphicsStyle));

            Element gStyle = styles.ToElements().FirstOrDefault(x => x.Name == "Dynamo");

            RevThread.IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    var argsM = new object[4];
                    argsM[0] = DocumentManager.Instance.CurrentUIDocument.Document;
                    argsM[1] = ElementId.InvalidElementId;
                    argsM[2] = geoms;
                    if (gStyle != null)
                        argsM[3] = gStyle.Id;
                    else
                        argsM[3] = ElementId.InvalidElementId;

                    keeperId = (ElementId)method.Invoke(null, argsM);

                    //keeperId = GeometryElement.SetForTransientDisplay(dynRevitSettings.Doc.Document, ElementId.InvalidElementId, geoms,
                    //                                       ElementId.InvalidElementId);

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        /// <summary>
        ///     Callback for registering an authentication provider with the package manager
        /// </summary>
        /// <param name="client">The client, to which the provider will be attached</param>
        private void RegisterSingleSignOn(PackageManagerClient client)
        {
            singleSignOnAssembly = singleSignOnAssembly ?? LoadSSONet();
            client.Client.Provider = client.Client.Provider
                ?? new RevitOxygenProvider(new DispatcherSynchronizationContext(UIDispatcher));
        }

        /// <summary>
        ///     Delay loading of the SSONet.dll, which is used by the package manager for
        ///     authentication information.
        /// </summary>
        /// <returns>The SSONet assembly</returns>
        private Assembly LoadSSONet()
        {
            // get the location of RevitAPI assembly.  SSONet is in the same directory.
            Assembly revitAPIAss = Assembly.GetAssembly(typeof(XYZ)); // any type loaded from RevitAPI
            string revitAPIDir = Path.GetDirectoryName(revitAPIAss.Location);

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = Path.Combine(revitAPIDir, "SSONET.dll");

            //Load the assembly from the specified path. 					
            return Assembly.LoadFrom(strTempAssmbPath);
        }

        private void FindNodesFromSelection()
        {
            IEnumerable<ElementId> selectedIds =
                DocumentManager.Instance.CurrentUIDocument.Selection.Elements.Cast<Element>()
                    .Select(x => x.Id);
            IEnumerable<RevitTransactionNode> transNodes =
                dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes.OfType<RevitTransactionNode>();
            List<RevitTransactionNode> foundNodes =
                transNodes.Where(x => x.AllElements.Intersect(selectedIds).Any()).ToList();

            if (foundNodes.Any())
            {
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(
                    this,
                    new ModelEventArgs(foundNodes.First()));

                DynamoSelection.Instance.ClearSelection();
                foundNodes.ForEach(DynamoSelection.Instance.Selection.Add);
            }
        }

        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            //when a document is opened 
            if (DocumentManager.Instance.CurrentUIDocument != null)
            {
                //DocumentManager.Instance.CurrentUIDocument =
                //    DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;

                ResetForNewDocument();
            }
        }

        private void Application_DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            //Disable running against revit without a document
            if (DocumentManager.Instance.CurrentDBDocument == null)
            {
                //DocumentManager.Instance.CurrentUIDocument = null;
                DynamoViewModel.RunEnabled = false;
                DynamoLogger.Instance.LogWarning(
                    "Dynamo no longer has an active document.",
                    WarningLevel.Moderate);
            }
            else
            {
                //DocumentManager.Instance.CurrentUIDocument =
                //    DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                DynamoViewModel.RunEnabled = true;
                DynamoLogger.Instance.LogWarning(
                    string.Format(
                        "Dynamo is now pointing at document: {0}",
                        DocumentManager.Instance.CurrentUIDocument.Document.PathName),
                    WarningLevel.Moderate);
            }

            ResetForNewDocument();
        }

        private void Revit_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            //if Dynamo doesn't have a view, then latch onto this one
            if (DocumentManager.Instance.CurrentUIDocument != null)
            {
                //DocumentManager.Instance.CurrentUIDocument =
                //    DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                DynamoLogger.Instance.LogWarning(
                    string.Format(
                        "Dynamo is now pointing at document: {0}",
                        DocumentManager.Instance.CurrentUIDocument.Document.PathName),
                    WarningLevel.Moderate);

                ResetForNewDocument();
            }
        }

        public event EventHandler RevitDocumentChanged;

        public virtual void OnRevitDocumentChanged()
        {
            if (RevitDocumentChanged != null)
                RevitDocumentChanged(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Clears all element collections on nodes and resets the visualization manager and the old value.
        /// </summary>
        private void ResetForNewDocument()
        {
            if (dynSettings.Controller != null)
            {
                dynSettings.Controller.DynamoModel.Nodes.ToList().ForEach(x => x.ResetOldValue());

                foreach (var node in dynSettings.Controller.DynamoModel.Nodes)
                {
                    lock (node.RenderPackagesMutex)
                    {
                        node.RenderPackages.Clear();
                    }
                }
            }

            OnRevitDocumentChanged();
        }

        protected override void OnRunCancelled(bool error)
        {
            base.OnRunCancelled(error);

            if (transaction != null && transaction.Status == TransactionStatus.Started)
                transaction.CancelTransaction();
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
                TransactionManager.Instance.EnsureInTransaction(
                    DocumentManager.Instance.CurrentUIDocument.Document);

                //Reset all elements
                IEnumerable<RevitTransactionNode> query =
                    dynSettings.Controller.DynamoModel.AllNodes.OfType<RevitTransactionNode>();

                foreach (RevitTransactionNode element in query)
                    element.ResetRuns();

                //////
                /* FOR NON-DEBUG RUNS, THIS IS THE ACTUAL END POINT FOR DYNAMO TRANSACTION */
                //////

                //Close global transaction.
                TransactionManager.Instance.TransactionTaskDone();
            };

            //Rename Delegate
            Action rename = delegate
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

                foreach (var kvp in ElementNameStore)
                {
                    //find the element and rename it
                    Element el;
                    if (dynUtils.TryGetElement(kvp.Key, out el))
                    {
                        //if the element is not stored with a unique name
                        //add a unique suffix to it
                        try
                        {
                            if (el is ReferencePlane)
                            {
                                var rp = el as ReferencePlane;
                                rp.Name = kvp.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (el is ReferencePlane)
                            {
                                var rp = el as ReferencePlane;
                                rp.Name = kvp.Value + "_" + Guid.NewGuid();
                            }
                        }
                    }
                }

                ElementNameStore.Clear();

                TransactionManager.Instance.TransactionTaskDone();
            };

            //If we're in a debug run or not already in the idle thread, then run the Cleanup Delegate
            //from the idle thread. Otherwise, just run it in this thread.
            //if (dynSettings.Controller.DynamoViewModel.RunInDebug || !InIdleThread && !IsTestMode)
            //{
            //    RevThread.IdlePromise.ExecuteOnIdleSync(cleanup);
            //    RevThread.IdlePromise.ExecuteOnIdleSync(rename);
            //    RevThread.IdlePromise.ExecuteOnIdleAsync(TransactionManager.Instance.ForceCloseTransaction);
            //}
            //else
            //{
            cleanup();
            rename();
            TransactionManager.Instance.ForceCloseTransaction();
            //}
        }

        public override void ShutDown(bool shutDownHost)
        {
            DisposeLogic.IsShuttingDown = true;

            RevThread.IdlePromise.ExecuteOnShutdown(
                delegate
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    TransactionManager.Instance.ForceCloseTransaction();
                });

            base.ShutDown(shutDownHost);
            Updater.UnRegisterAllChangeHooks();
            RevertPythonBindings();

            // PB: killed this block as the LookupPostableCommandId method is not available in revit 2013
            //     dynamo will crash consistently on shutdown without this commented out.  
            //     TODO: fix with proper reflection call

            //if (shutDownHost)
            //{
            //    // this method cannot be called without Revit 2014
            //    var exitCommand = RevitCommandId.LookupPostableCommandId(PostableCommand.ExitRevit);

            //    UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
            //    if (uiapp.CanPostCommand(exitCommand))
            //        uiapp.PostCommand(exitCommand);
            //    else
            //    {
            //        MessageBox.Show(
            //            "A command in progress prevented Dynamo from closing revit. Dynamo update will be cancelled.");
            //    }
            //}
        }

        protected override void Run()
        {
            //DocumentManager.Instance.CurrentDBDocument = DocumentManager.Instance.CurrentUIDocument.Document;

            if (DynamoViewModel.RunInDebug)
            {
                TransMode = TransactionMode.Debug; //Debug transaction control
                DynamoLogger.Instance.Log("Running expression in debug.");
            }
            else
            {
                // As we use a generic function node to represent all functions,
                // we don't know if a node has something to do with Revit or 
                // not, neither we know that the re-execution of a dirty node
                // will trigger the update for a Revit related node. Now just
                // run the execution in the idle thread until we find out a 
                // way to control the execution.
                TransMode = TransactionMode.Automatic; //Automatic transaction control
                Debug.WriteLine("Adding a run to the idle stack.");
            }

            //Run in idle thread no matter what
            RevThread.IdlePromise.ExecuteOnIdleSync(base.Run);
        }

        //protected override void Run(List<NodeModel> topElements, FScheme.Expression runningExpression)
        //{
        //    var model = (DynamoRevitViewModel) DynamoViewModel;

        //    //If we are not running in debug...
        //    if (!DynamoViewModel.RunInDebug)
        //    {
        //        //Do we need manual transaction control?
        //        bool manualTrans = topElements.Any(CheckManualTransaction.TraverseUntilAny);

        //        //Can we avoid running everything in the Revit Idle thread?
        //        bool noIdleThread = manualTrans ||
        //                            !topElements.Any(CheckRequiresTransaction.TraverseUntilAny);

        //        //If we don't need to be in the idle thread...
        //        if (noIdleThread || Testing)
        //        {
        //            //DynamoLogger.Instance.Log("Running expression in evaluation thread...");
        //            TransMode = TransactionMode.Manual; //Manual transaction control

        //            if (Testing)
        //                TransMode = TransactionMode.Automatic;

        //            InIdleThread = false; //Not in idle thread at the moment
        //            base.Run(topElements, runningExpression); //Just run the Run Delegate
        //        }
        //        else //otherwise...
        //        {
        //            //DynamoLogger.Instance.Log("Running expression in Revit's Idle thread...");
        //            TransMode = TransactionMode.Automatic; //Automatic transaction control

        //            Debug.WriteLine("Adding a run to the idle stack.");
        //            InIdleThread = true; //Now in the idle thread.
        //            RevThread.IdlePromise.ExecuteOnIdleSync(() => base.Run(topElements, runningExpression)); //Execute the Run Delegate in the Idle thread.

        //        }
        //    }
        //    else //If we are in debug mode...
        //    {
        //        TransMode = TransactionMode.Debug; //Debug transaction control
        //        InIdleThread = true; //Everything will be evaluated in the idle thread.

        //        DynamoLogger.Instance.Log("Running expression in debug.");

        //        //Execute the Run Delegate.
        //        base.Run(topElements, runningExpression);
        //    }
        //}

        public override void ResetEngine()
        {
            RevThread.IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    if (EngineController != null)
                        EngineController.Dispose();

                    EngineController = new EngineController(this, true);
                });
        }

        #region Python Nodes Revit Hooks

        private FieldInfo evaluatorField;
        private dynamic oldPyEval;

        private void AddPythonBindings()
        {
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Assembly ironPythonAssembly = null;

                string path;

                if (File.Exists(path = Path.Combine(assemblyPath, "DynamoPython.dll")))
                    ironPythonAssembly = Assembly.LoadFrom(path);
                else if (
                    File.Exists(
                        path = Path.Combine(assemblyPath, "Packages", "IronPython", "DynamoPython.dll")))
                    ironPythonAssembly = Assembly.LoadFrom(path);

                if (ironPythonAssembly == null)
                    return;

                Type pythonBindings = ironPythonAssembly.GetType("DynamoPython.PythonBindings");

                PropertyInfo pyBindingsProperty = pythonBindings.GetProperty("Bindings");
                object pyBindings = pyBindingsProperty.GetValue(null, null);

                Action<string, object> addToBindings =
                    (name, boundObject) =>
                        pyBindings.GetType()
                            .InvokeMember(
                                "Add",
                                BindingFlags.InvokeMethod,
                                null,
                                pyBindings,
                                new[] { name, boundObject });

                addToBindings("DynLog", new LogDelegate(DynamoLogger.Instance.Log)); //Logging

                addToBindings(
                    "DynTransaction",
                    new Func<SubTransaction>(
                        delegate
                        {
                            TransactionManager.Instance.EnsureInTransaction(
                                DocumentManager.Instance.CurrentUIDocument.Document);
                            return new SubTransaction(DocumentManager.Instance.CurrentUIDocument.Document);
                        }));

                addToBindings("__revit__", DocumentManager.Instance.CurrentUIDocument.Application);
                addToBindings(
                    "__doc__",
                    DocumentManager.Instance.CurrentUIDocument.Application.ActiveUIDocument.Document);

                Type pythonEngine = ironPythonAssembly.GetType("DynamoPython.PythonEngine");
                evaluatorField = pythonEngine.GetField("Evaluator");

                oldPyEval = evaluatorField.GetValue(null);

                //var x = PythonEngine.GetMembers();
                //foreach (var y in x)
                //    Console.WriteLine(y);

                Type evalDelegateType =
                    ironPythonAssembly.GetType("DynamoPython.PythonEngine+EvaluationDelegate");

                Delegate d = Delegate.CreateDelegate(
                    evalDelegateType,
                    this,
                    typeof(DynamoController_Revit).GetMethod(
                        "newEval",
                        BindingFlags.NonPublic | BindingFlags.Instance));

                evaluatorField.SetValue(null, d);

                FieldInfo drawingField = pythonEngine.GetField("Drawing");
                Type drawDelegateType = ironPythonAssembly.GetType("DynamoPython.PythonEngine+DrawDelegate");
                Delegate draw = Delegate.CreateDelegate(
                    drawDelegateType,
                    this,
                    typeof(DynamoController_Revit).GetMethod(
                        "DrawPython",
                        BindingFlags.NonPublic | BindingFlags.Instance));

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

        private void RevertPythonBindings()
        {
            try
            {
                if (evaluatorField != null && oldPyEval != null)
                    evaluatorField.SetValue(null, oldPyEval);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

// ReSharper disable once UnusedMember.Local
        private void DrawPython(FScheme.Value val, string id)
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

// ReSharper disable once UnusedMember.Local
        private FScheme.Value newEval(bool dirty, string script, dynamic bindings)
        {
            return InIdleThread
                ? oldPyEval(dirty, script, bindings)
                : RevThread.IdlePromise.ExecuteOnIdleSync(() => oldPyEval(dirty, script, bindings));
        }

        private delegate void LogDelegate(string msg);

        private delegate void SaveElementDelegate(Element e);

        #endregion

        #region Element Persistence Management

        private readonly Dictionary<ElementUpdateDelegate, HashSet<ElementId>> transDelElements =
            new Dictionary<ElementUpdateDelegate, HashSet<ElementId>>();

        private readonly List<ElementId> transElements = new List<ElementId>();

        internal void RegisterSuccessfulDeleteHook(ElementId id, ElementUpdateDelegate updateDelegate)
        {
            HashSet<ElementId> elements;
            if (!transDelElements.TryGetValue(updateDelegate, out elements))
            {
                elements = new HashSet<ElementId>();
                transDelElements[updateDelegate] = elements;
            }
            elements.Add(id);
        }

        private void CommitDeletions()
        {
            foreach (var kvp in transDelElements)
                kvp.Key(kvp.Value);
        }

        internal void RegisterDMUHooks(ElementId id, ElementUpdateDelegate updateDelegate)
        {
            ElementUpdateDelegate del = delegate(HashSet<ElementId> deleted)
            {
                foreach (ElementId invId in deleted) //invalid)
                {
                    Updater.UnRegisterChangeHook(invId, ChangeType.Modify);
                    Updater.UnRegisterChangeHook(invId, ChangeType.Delete);
                }
                updateDelegate(deleted); //invalid);
            };

            Updater.RegisterChangeHook(id, ChangeType.Delete, del);
            transElements.Add(id);
        }

        #endregion

        #region Revit Transaction Management

        private TransactionHandle transaction;

        public TransactionWrapper TransactionWrapper { get; private set; }

        private void TransactionManager_TransactionCancelled()
        {
            Updater.RollBack(transElements);
            transElements.Clear();
            transDelElements.Clear();
        }

        private void TransactionManager_TransactionCommitted()
        {
            transElements.Clear();
            CommitDeletions();
            transDelElements.Clear();
        }

        private void TransactionManager_FailuresRaised(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();

            IEnumerable<FailureMessageAccessor> query = 
                from fail in failList
                where fail.GetSeverity() == FailureSeverity.Warning
                select fail;

            foreach (FailureMessageAccessor fail in query)
            {
                DynamoLogger.Instance.Log("!! Warning: " + fail.GetDescriptionText());
                failuresAccessor.DeleteWarning(fail);
            }
        }

        #endregion
    }

    public enum TransactionMode
    {
        Debug,
        Manual,
        Automatic
    }
}

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using DSIronPython;
using DSNodeServices;
using Dynamo.Applications;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Revit;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.UpdateManager;
using DynamoUtilities;
using Greg;
using ProtoCore;
using Revit.Elements;
using RevitServices.Elements;
using RevitServices.Materials;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Autodesk.Revit.DB.Element;
using WrappedElement = Revit.Elements.Element;
using RevThread = RevitServices.Threading;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;

#endregion

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        public class Reactor
        {
            private static Reactor reactor = null;

            public static Reactor GetInstance()
            {
                lock (typeof(Reactor))
                {
                    if (reactor == null)
                    {
                        reactor = new Reactor();
                    }
                }

                return reactor;
            }

            internal Reactor()
            {
                var u = dynRevitSettings.Controller.Updater;

                u.ElementsDeleted += OnElementsDeleted;
            }


            private void OnElementsDeleted(Document document, IEnumerable<ElementId> deleted)
            {
                if (!deleted.Any())
                    return;

                var nodes = dynRevitUtils.GetNodesFromElementIds(deleted);
                foreach (var node in nodes)
                {
                    node.RequiresRecalc = true;
                    node.ForceReExecuteOfNode = true;
                }
            }
        }

        /// <summary>
        ///     A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private Assembly singleSignOnAssembly;

        /// <summary>
        ///     Flag for syncing up document switches between Application.DocumentClosing and
        ///     Application.DocumentClosed events.
        /// </summary>
        private bool updateCurrentUIDoc;

        private Reactor reactor;

        public DynamoController_Revit(RevitServicesUpdater updater, string context, IUpdateManager updateManager, string corePath)
            : base(
                context,
                updateManager,
                new RevitWatchHandler(),
                Dynamo.PreferenceSettings.Load(),
                corePath)
        {
            Updater = updater;

            dynRevitSettings.Controller = this;

            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosing +=
                Application_DocumentClosing;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosed +=
                Application_DocumentClosed;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentOpened +=
                Application_DocumentOpened;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated += Revit_ViewActivated;

            // Set the intitial document.
            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument != null)
            {
                DocumentManager.Instance.CurrentUIDocument =
                       DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                dynSettings.DynamoLogger.LogWarning(GetDocumentPointerMessage(), WarningLevel.Moderate);
            }

            // Reset the materials manager.
            MaterialsManager.Reset();

            TransactionWrapper = TransactionManager.Instance.TransactionWrapper;
            TransactionWrapper.TransactionStarted += TransactionManager_TransactionCommitted;
            TransactionWrapper.TransactionCancelled += TransactionManager_TransactionCancelled;
            TransactionWrapper.FailuresRaised += TransactionManager_FailuresRaised;

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));
            ElementNameStore = new Dictionary<ElementId, string>();

            SetupPython();

            Runner = new DynamoRunner_Revit(this);
        }

        private void SetupPython()
        {
            //IronPythonEvaluator.InputMarshaler.RegisterMarshaler((WrappedElement element) => element.InternalElement);
            IronPythonEvaluator.OutputMarshaler.RegisterMarshaler((Element element) => element.ToDSType(true));
            //IronPythonEvaluator.OutputMarshaler.RegisterMarshaler((IList<Element> elements) => elements.Select(e=>e.ToDSType(true)));

            // Turn off element binding during iron python script execution
            IronPythonEvaluator.EvaluationBegin += (a, b, c, d, e) => ElementBinder.IsEnabled = false;
            IronPythonEvaluator.EvaluationEnd += (a, b, c, d, e) => ElementBinder.IsEnabled = true;

            // register UnwrapElement method in ironpython
            IronPythonEvaluator.EvaluationBegin += (a, b, scope, d, e) =>
            {
                var marshaler = new DataMarshaler();
                marshaler.RegisterMarshaler((WrappedElement element) => element.InternalElement);

                Func<object, object> unwrap = marshaler.Marshal;
                scope.SetVariable("UnwrapElement", unwrap);
            };
        }

        public RevitServicesUpdater Updater { get; private set; }

        /// <summary>
        ///     A dictionary which temporarily stores element names for setting after element deletion.
        /// </summary>
        public Dictionary<ElementId, string> ElementNameStore { get; set; }

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

        /// <summary>
        ///     Callback for registering an authentication provider with the package manager
        /// </summary>
        /// <param name="client">The client, to which the provider will be attached</param>
        internal void RegisterSingleSignOn(PackageManagerClient client)
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
        private static Assembly LoadSSONet()
        {
            // get the location of RevitAPI assembly.  SSONet is in the same directory.
            Assembly revitAPIAss = Assembly.GetAssembly(typeof(XYZ)); // any type loaded from RevitAPI
            string revitAPIDir = Path.GetDirectoryName(revitAPIAss.Location);
            Debug.Assert(revitAPIDir != null, "revitAPIDir != null");

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            string strTempAssmbPath = Path.Combine(revitAPIDir, "SSONET.dll");

            //Load the assembly from the specified path. 					
            return Assembly.LoadFrom(strTempAssmbPath);
        }

        internal void FindNodesFromSelection()
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

        /// <summary>
        /// Handler for Revit's DocumentOpened event.
        /// This handler is called when a document is opened, but NOT when
        /// a document is created from a template.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            // If the current document is null, for instance if there are
            // no documents open, then set the current document, and 
            // present a message telling us where Dynamo is pointing.
            if (DocumentManager.Instance.CurrentUIDocument == null)
            {
                DocumentManager.Instance.CurrentUIDocument = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                dynSettings.DynamoLogger.LogWarning(GetDocumentPointerMessage(), WarningLevel.Moderate);
                DynamoViewModel.RunEnabled = true;
                ResetForNewDocument();
            }
        }

        private void Application_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (DocumentManager.Instance.CurrentDBDocument.Equals(e.Document))
            {
                updateCurrentUIDoc = true;
            }
        }

        /// <summary>
        /// Handler for Revit's DocumentClosed event.
        /// This handler is called when a document is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            // If the active UI document is null, it means that all views have been 
            // closed from all document. Clear our reference, present a warning,
            // and disable running.
            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument == null)
            {
                DocumentManager.Instance.CurrentUIDocument = null;
                DynamoViewModel.RunEnabled = false;
                dynSettings.DynamoLogger.LogWarning(
                    "Dynamo no longer has an active document. Please open a document.",
                    WarningLevel.Error);
            }
            else
            {
                // If Dynamo's active UI document's document is the one that was just closed
                // then set Dynamo's active UI document to whatever revit says is active.
                if (updateCurrentUIDoc)
                {
                    updateCurrentUIDoc = false;
                    DocumentManager.Instance.CurrentUIDocument = 
                        DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                    
                    MaterialsManager.Reset();
                }
            }

            var uiDoc = DocumentManager.Instance.CurrentUIDocument;
            if (uiDoc != null)
            {
                DynamoRevit.SetRunEnabledBasedOnContext(uiDoc.ActiveView); 
            }
        }

        /// <summary>
        /// Handler for Revit's ViewActivated event.
        /// This handler is called when a view is activated. It is called
        /// after the ViewActivating event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Revit_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            // If there is no active document, then set it to whatever
            // document has just been activated
            if (DocumentManager.Instance.CurrentUIDocument == null)
            {
                DocumentManager.Instance.CurrentUIDocument =
                DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                
                MaterialsManager.Reset();

                DynamoViewModel.RunEnabled = true;

                //In the case that the current document is null, we also need to do
                //a reset for the current document.
                ResetForNewDocument();
            }
        }

        internal static string GetDocumentPointerMessage()
        {
            var docPath = DocumentManager.Instance.CurrentUIDocument.Document.PathName;
            var message = string.IsNullOrEmpty(docPath)
                ? "a new document."
                : string.Format("document: {0}", docPath);
            return string.Format("Dynamo is now running on {0}", message);
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
                foreach (var node in DynamoModel.Nodes)
                    node.ResetOldValue();

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

        public override void OnEvaluationCompleted(object sender, EventArgs e)
        {
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
                        catch (Exception)
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

            cleanup();
            rename();
            TransactionManager.Instance.ForceCloseTransaction();

            base.OnEvaluationCompleted(sender, e);
        }

        public override void ShutDown(bool shutDownHost, EventArgs args = null)
        {
            DisposeLogic.IsShuttingDown = true;

            RevThread.IdlePromise.ExecuteOnShutdown(
                delegate
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    var keeperId = ((VisualizationManagerRevit) VisualizationManager).KeeperId;

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    TransactionManager.Instance.ForceCloseTransaction();
                });

            base.ShutDown(shutDownHost, args);
            Updater.UnRegisterAllChangeHooks();

            if (shutDownHost)
            {
                // this method cannot be called without Revit 2014
                var exitCommand = RevitCommandId.LookupPostableCommandId(PostableCommand.ExitRevit);

                UIApplication uiapp = DocumentManager.Instance.CurrentUIApplication;
                if (uiapp.CanPostCommand(exitCommand))
                    uiapp.PostCommand(exitCommand);
                else
                {
                    MessageBox.Show(
                        "A command in progress prevented Dynamo from closing revit. Dynamo update will be cancelled.");
                }
            }
        }

        public override void ResetEngine()
        {
            RevThread.IdlePromise.ExecuteOnIdleAsync(base.ResetEngine);
        }

        #region Element Persistence Management

        private readonly Dictionary<ElementUpdateDelegate, HashSet<string>> transDelElements =
            new Dictionary<ElementUpdateDelegate, HashSet<string>>();

        private readonly List<ElementId> transElements = new List<ElementId>();

        internal void RegisterSuccessfulDeleteHook(string id, ElementUpdateDelegate updateDelegate)
        {
            HashSet<string> elements;
            if (!transDelElements.TryGetValue(updateDelegate, out elements))
            {
                elements = new HashSet<string>();
                transDelElements[updateDelegate] = elements;
            }
            elements.Add(id);
        }

        private void CommitDeletions()
        {
            foreach (var kvp in transDelElements)
                kvp.Key(kvp.Value);
        }

        #endregion

        #region Revit Transaction Management

        internal TransactionHandle transaction;

        public TransactionWrapper TransactionWrapper { get; private set; }

        private void TransactionManager_TransactionCancelled()
        {
            Updater.RollBack(DocumentManager.Instance.CurrentDBDocument, transElements);
            transElements.Clear();
            transDelElements.Clear();
        }

        private void TransactionManager_TransactionCommitted()
        {
            transElements.Clear();
            CommitDeletions();
            transDelElements.Clear();
        }

        private static void TransactionManager_FailuresRaised(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();

            IEnumerable<FailureMessageAccessor> query = 
                from fail in failList
                where fail.GetSeverity() == FailureSeverity.Warning
                select fail;

            foreach (FailureMessageAccessor fail in query)
            {
                dynSettings.DynamoLogger.Log("!! Warning: " + fail.GetDescriptionText());
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

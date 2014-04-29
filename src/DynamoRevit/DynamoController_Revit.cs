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
using CurveLoop = Autodesk.Revit.DB.CurveLoop;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;
using RevThread = RevitServices.Threading;

#endregion

namespace Dynamo
{
    public class DynamoController_Revit : DynamoController
    {
        /// <summary>
        ///     A reference to the the SSONET assembly to prevent reloading.
        /// </summary>
        private Assembly singleSignOnAssembly;

        public DynamoController_Revit(RevitServicesUpdater updater, string context)
            : base(
                context,
                new UpdateManager.UpdateManager(),
                new RevitWatchHandler(),
                Dynamo.PreferenceSettings.Load())
        {
            Updater = updater;

            dynRevitSettings.Controller = this;

            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosed +=
                Application_DocumentClosed;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentOpened +=
                Application_DocumentOpened;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated += Revit_ViewActivated;

            TransactionWrapper = TransactionManager.Instance.TransactionWrapper;
            TransactionWrapper.TransactionStarted += TransactionManager_TransactionCommitted;
            TransactionWrapper.TransactionCancelled += TransactionManager_TransactionCancelled;
            TransactionWrapper.FailuresRaised += TransactionManager_FailuresRaised;

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));
            ElementNameStore = new Dictionary<ElementId, string>();

            EngineController.ImportLibrary("RevitNodes.dll");
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

        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            //when a document is opened 
            if (DocumentManager.Instance.CurrentUIDocument != null)
            {
                DynamoViewModel.RunEnabled = true;
                ResetForNewDocument();
            }
        }

        private void Application_DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            //Disable running against revit without a document
            if (DocumentManager.Instance.CurrentDBDocument == null)
            {
                DynamoViewModel.RunEnabled = false;
                DynamoLogger.Instance.LogWarning(
                    "Dynamo no longer has an active document.",
                    WarningLevel.Moderate);
            }
            else
            {
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

        private TransactionHandle transaction;

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

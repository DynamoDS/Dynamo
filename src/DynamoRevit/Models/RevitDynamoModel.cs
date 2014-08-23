﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using DSIronPython;

using DSNodeServices;

using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UpdateManager;
using Dynamo.Utilities;

using ProtoCore;

using Revit.Elements;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

using Element = Autodesk.Revit.DB.Element;

namespace Dynamo.Applications.Models
{
    public class RevitDynamoModel : DynamoModel
    {
        #region Events

        public event EventHandler RevitDocumentChanged;
        public virtual void OnRevitDocumentChanged()
        {
            if (RevitDocumentChanged != null)
                RevitDocumentChanged(this, EventArgs.Empty);
        }

        public delegate void DynamoRevitModelHandler(RevitDynamoModel model);
        public event DynamoRevitModelHandler ShuttingDown;
        private void OnShuttingDown()
        {
            if (ShuttingDown != null)
            {
                ShuttingDown(this);
            }
        }

        #endregion

        #region Properties/Fields
        public RevitServicesUpdater RevitServicesUpdater { get; private set; }
     
        #endregion

        #region Constructors

        public new static RevitDynamoModel Start()
        {
            return RevitDynamoModel.Start(new StartConfiguration());
        }

        public new static RevitDynamoModel Start(StartConfiguration configuration)
        {
            // where necessary, assign defaults
            if (string.IsNullOrEmpty(configuration.Context))
                configuration.Context = Core.Context.REVIT_2014;
            if (string.IsNullOrEmpty(configuration.DynamoCorePath))
            {
                var asmLocation = Assembly.GetExecutingAssembly().Location;
                configuration.DynamoCorePath = Path.GetDirectoryName(asmLocation);
            }

            if (configuration.Preferences == null)
                configuration.Preferences = new PreferenceSettings();
            if (configuration.Runner == null)
                configuration.Runner = new RevitDynamoRunner();

            return new RevitDynamoModel(configuration);
        }

        private RevitDynamoModel(StartConfiguration configuration) :
            base(configuration)
        {
            string context = configuration.Context;
            IPreferences preferences = configuration.Preferences;
            string corePath = configuration.DynamoCorePath;
            IUpdateManager updateManager = configuration.UpdateManager;
            bool isTestMode = configuration.StartInTestMode;

            RevitServicesUpdater = new RevitServicesUpdater(DynamoRevitApp.ControlledApplication, DynamoRevitApp.Updaters);
            SubscribeRevitServicesUpdaterEvents();

            InitializeDocumentManager();
            SubscribeDocumentManagerEvents();
            SubscribeTransactionManagerEvents();

            SetupPython();
        }

        #endregion

        #region Initialization

        private bool setupPython;
        private void SetupPython()
        {
            if (setupPython) return;

            IronPythonEvaluator.OutputMarshaler.RegisterMarshaler((Element element) => ElementWrapper.ToDSType(element, (bool)true));

            // Turn off element binding during iron python script execution
            IronPythonEvaluator.EvaluationBegin += (a, b, c, d, e) => ElementBinder.IsEnabled = false;
            IronPythonEvaluator.EvaluationEnd += (a, b, c, d, e) => ElementBinder.IsEnabled = true;

            // register UnwrapElement method in ironpython
            IronPythonEvaluator.EvaluationBegin += (a, b, scope, d, e) =>
            {
                var marshaler = new DataMarshaler();
                marshaler.RegisterMarshaler((Revit.Elements.Element element) => element.InternalElement);

                Func<object, object> unwrap = marshaler.Marshal;
                scope.SetVariable("UnwrapElement", unwrap);
            };

            setupPython = true;
        }

        private void InitializeDocumentManager()
        {
            // Set the intitial document.
            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument != null)
            {
                DocumentManager.Instance.CurrentUIDocument =
                    DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                this.Logger.LogWarning(GetDocumentPointerMessage(), WarningLevel.Moderate);
            }
        }

        #endregion

        #region Event subscribe/unsubscribe

        private void SubscribeRevitServicesUpdaterEvents()
        {
            RevitServicesUpdater.ElementAddedForID += ElementMappingCache.GetInstance().WatcherMethodForAdd;
            RevitServicesUpdater.ElementsDeleted += ElementMappingCache.GetInstance().WatcherMethodForDelete;
            RevitServicesUpdater.ElementsDeleted += RevitServicesUpdater_ElementsDeleted;
        }

        private void UnsubscribeRevitServicesUpdaterEvents()
        {
            RevitServicesUpdater.ElementAddedForID -= ElementMappingCache.GetInstance().WatcherMethodForAdd;
            RevitServicesUpdater.ElementsDeleted -= ElementMappingCache.GetInstance().WatcherMethodForDelete;
            RevitServicesUpdater.ElementsDeleted -= RevitServicesUpdater_ElementsDeleted;
        }

        private void SubscribeTransactionManagerEvents()
        {
            TransactionManager.Instance.TransactionWrapper.FailuresRaised += TransactionManager_FailuresRaised;
        }

        private void UnsubscribeTransactionManagerEvents()
        {
            TransactionManager.Instance.TransactionWrapper.FailuresRaised -= TransactionManager_FailuresRaised;
        }

        private void SubscribeDocumentManagerEvents()
        {
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosed +=
                Application_DocumentClosed;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentOpened +=
                Application_DocumentOpened;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated += Revit_ViewActivated;

            DocumentManager.OnLogError += this.Logger.Log;
        }

        private void UnsubscribeDocumentManagerEvents()
        {
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentClosed -=
                Application_DocumentClosed;
            DocumentManager.Instance.CurrentUIApplication.Application.DocumentOpened -=
                Application_DocumentOpened;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated -= Revit_ViewActivated;

            DocumentManager.OnLogError -= this.Logger.Log;
        }

        #endregion

        #region Public methods

        public override void OnEvaluationCompleted(object sender, EventArgs e)
        {
            // finally close the transaction!
            TransactionManager.Instance.ForceCloseTransaction();

            base.OnEvaluationCompleted(sender, e);
        }

        public override void ShutDown(bool shutDownHost, EventArgs args = null)
        {
            DisposeLogic.IsShuttingDown = true;

            OnShuttingDown();

            base.ShutDown(shutDownHost, args);

            // unsubscribe events
            RevitServicesUpdater.UnRegisterAllChangeHooks();

            UnsubscribeDocumentManagerEvents();
            UnsubscribeRevitServicesUpdaterEvents();
            UnsubscribeTransactionManagerEvents();

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
            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(base.ResetEngine);
        }

        public void SetRunEnabledBasedOnContext(Autodesk.Revit.DB.View newView)
        {
            var view = newView as View3D;

            if (view != null && view.IsPerspective
                && this.Context != Core.Context.VASARI_2014)
            {
                this.Logger.LogWarning(
                    "Dynamo is not available in a perspective view. Please switch to another view to Run.",
                    WarningLevel.Moderate);
                this.RunEnabled = false;
            }
            else
            {
                this.Logger.Log(
                    string.Format("Active view is now {0}", newView.Name));

                // If there is a current document, then set the run enabled
                // state based on whether the view just activated is 
                // the same document.
                if (DocumentManager.Instance.CurrentUIDocument != null)
                {
                    this.RunEnabled =
                        newView.Document.Equals(DocumentManager.Instance.CurrentDBDocument);

                    if (this.RunEnabled == false)
                    {
                        this.Logger.LogWarning(
                            "Dynamo is not pointing at this document. Run will be disabled.",
                            WarningLevel.Error);
                    }
                }
            }
        }

        #endregion

        #region Event handlers 

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
                this.Logger.LogWarning(GetDocumentPointerMessage(), WarningLevel.Moderate);
                this.RunEnabled = true;
                ResetForNewDocument();
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
                this.RunEnabled = false;
                this.Logger.LogWarning(
                    "Dynamo no longer has an active document. Please open a document.",
                    WarningLevel.Error);
            }
            else
            {
                // If Dynamo's active UI document's document is the one that was just closed
                // then set Dynamo's active UI document to whatever revit says is active.
                if (DocumentManager.Instance.CurrentUIDocument.Document == null)
                {
                    DocumentManager.Instance.CurrentUIDocument =
                        DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
                }
            }

            var uiDoc = DocumentManager.Instance.CurrentUIDocument;
            if (uiDoc != null)
            {
                this.SetRunEnabledBasedOnContext(uiDoc.ActiveView);
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

                this.RunEnabled = true;
            }
        }

        private static string GetDocumentPointerMessage()
        {
            var docPath = DocumentManager.Instance.CurrentUIDocument.Document.PathName;
            var message = string.IsNullOrEmpty(docPath)
                ? "a new document."
                : string.Format("document: {0}", docPath);
            return string.Format("Dynamo is now running on {0}", message);
        }

        /// <summary>
        ///     Clears all element collections on nodes and resets the visualization manager and the old value.
        /// </summary>
        private void ResetForNewDocument()
        {
            foreach (var node in this.Nodes)
                node.ResetOldValue();

            foreach (var node in this.Nodes)
            {
                lock (node.RenderPackagesMutex)
                {
                    node.RenderPackages.Clear();
                }
            }

            OnRevitDocumentChanged();
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
                this.Logger.Log("!! Warning: " + fail.GetDescriptionText());
                failuresAccessor.DeleteWarning(fail);
            }
        }

        private void RevitServicesUpdater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (!deleted.Any())
                return;

            var workspace = this.CurrentWorkspace;

            ProtoCore.Core core = null;
            var engine = this.EngineController;
            if (engine != null && (engine.LiveRunnerCore != null))
                core = engine.LiveRunnerCore;

            if (core == null) // No execution yet as of this point.
                return;

            // Selecting all nodes that are either a DSFunction,
            // a DSVarArgFunction or a CodeBlockNodeModel into a list.
            var nodeGuids = workspace.Nodes.Where((n) =>
            {
                return (n is DSFunction
                    || (n is DSVarArgFunction)
                    || (n is CodeBlockNodeModel));
            }).Select((n) => n.GUID);

            var nodeTraceDataList = core.GetCallsitesForNodes(nodeGuids);// core.GetTraceDataForNodes(nodeGuids);

            foreach (Guid guid in nodeTraceDataList.Keys)
            {
                foreach (CallSite cs in nodeTraceDataList[guid])
                {
                    foreach (CallSite.SingleRunTraceData srtd in cs.TraceData)
                    {
                        List<ISerializable> traceData = srtd.RecursiveGetNestedData();

                        foreach (ISerializable thingy in traceData)
                        {
                            SerializableId sid = thingy as SerializableId;

                            foreach (ElementId eid in deleted)
                            {

                                if (sid != null)
                                {
                                    if (sid.IntID == eid.IntegerValue)
                                    {
                                        NodeModel inm =
                                            workspace.Nodes.Where((n) => n.GUID == guid).FirstOrDefault();

                                        Validity.Assert(inm != null, "The bound node has disappeared");

                                        inm.RequiresRecalc = true;
                                        inm.ForceReExecuteOfNode = true;

                                        //FOUND IT!
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}

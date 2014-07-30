using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using DSIronPython;

using DSNodeServices;

using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;

using Revit.Elements;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

using Element = Autodesk.Revit.DB.Element;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;

namespace Dynamo.Applications
{
    internal class DynamoRevitModel : DynamoModel
    {
        public enum TransactionMode
        {
            Debug,
            Manual,
            Automatic
        }

        #region Events

        public event EventHandler RevitDocumentChanged;

        public virtual void OnRevitDocumentChanged()
        {
            if (RevitDocumentChanged != null)
                RevitDocumentChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Properties/Fields

        public RevitServicesUpdater RevitUpdater { get; private set; }

        public bool InIdleThread
        {
            get { return RevitServices.Threading.IdlePromise.InIdleThread; }
        }
                
        #endregion

        public DynamoRevitModel(string context, IPreferences preferences, bool isTestMode = false) :
            base(context, preferences, isTestMode)
        {
            RevitUpdater = updater;

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
                this.Logger.LogWarning(GetDocumentPointerMessage(), WarningLevel.Moderate);
            }

            TransactionWrapper = TransactionManager.Instance.TransactionWrapper;
            TransactionWrapper.TransactionStarted += TransactionManager_TransactionCommitted;
            TransactionWrapper.TransactionCancelled += TransactionManager_TransactionCancelled;
            TransactionWrapper.FailuresRaised += TransactionManager_FailuresRaised;

            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));

            SetupPython();

            Runner = new DynamoRunner_Revit(this);
        }

        private void SetupPython()
        {
            IronPythonEvaluator.OutputMarshaler.RegisterMarshaler((Element element) => ElementWrapper.ToDSType(element, (bool)true));

            // Turn off element binding during iron python script execution
            IronPythonEvaluator.EvaluationBegin += (a, b, c, d, e) => ElementBinder.IsEnabled = false;
            IronPythonEvaluator.EvaluationEnd += (a, b, c, d, e) => ElementBinder.IsEnabled = true;

            // register UnwrapElement method in ironpython
            IronPythonEvaluator.EvaluationBegin += (a, b, scope, d, e) =>
            {
                var marshaler = new DataMarshaler();
                marshaler.RegisterMarshaler((global::Revit.Elements.Element element) => element.InternalElement);

                Func<object, object> unwrap = marshaler.Marshal;
                scope.SetVariable("UnwrapElement", unwrap);
            };
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

                this.RunEnabled = true;
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

        /// <summary>
        ///     Utility function to determine if an Element of the given ID exists in the document.
        /// </summary>
        /// <returns>True if exists, false otherwise.</returns>
        private static bool TryGetElement<T>(ElementId id, out T e) where T : Element
        {
            try
            {
                e = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id) as T;
                return e != null && e.Id != null;
            }
            catch
            {
                e = null;
                return false;
            }
        }

        public override void ShutDown(bool shutDownHost, EventArgs args = null)
        {
            DisposeLogic.IsShuttingDown = true;

            RevitServices.Threading.IdlePromise.ExecuteOnShutdown(
                delegate
                {
                    TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

                    var keeperId = ((VisualizationManagerRevit) VisualizationManager).KeeperId;

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                    }

                    TransactionManager.Instance.ForceCloseTransaction();
                });

            base.ShutDown(shutDownHost, args);
            RevitUpdater.UnRegisterAllChangeHooks();

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
            RevitUpdater.RollBack(DocumentManager.Instance.CurrentDBDocument, transElements);
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
                this.Logger.Log("!! Warning: " + fail.GetDescriptionText());
                failuresAccessor.DeleteWarning(fail);
            }
        }

        #endregion

    }
}

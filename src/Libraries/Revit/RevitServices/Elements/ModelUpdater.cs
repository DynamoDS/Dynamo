//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using RevitServices.Persistence;

namespace RevitServices.Elements
{
    public class RevitServicesUpdater : IDisposable
    {
        //TODO: To handle multiple documents, should store unique ids as opposed to ElementIds.

        private readonly ControlledApplication application;

        public event ElementAddDelegate ElementsAdded;
        public event ElementUpdateElementIdDelegate ElementAddedForID;
        public event ElementModifyDelegate ElementsModified;
        public event ElementDeleteDelegate ElementsDeleted;

        #region Event Invokers

        protected virtual void OnElementsAdded(Document doc, IEnumerable<string> updated)
        {
            var handler = ElementsAdded;
            if (handler != null) handler(doc, updated);
        }

        protected virtual void OnElementsAdded(Document doc, IEnumerable<ElementId> updated)
        {
            var handler = ElementAddedForID;
            if (handler != null) handler(doc, updated);
        }


        protected virtual void OnElementsModified(Document doc, IEnumerable<string> modified)
        {
            var handler = ElementsModified;
            if (handler != null) handler(doc, modified);
        }

        protected virtual void OnElementsDeleted(Document doc, IEnumerable<ElementId> deleted)
        {
            var handler = ElementsDeleted;
            if (handler != null) handler(doc, deleted);
        }

        #endregion

        // constructor takes the AddInId for the add-in associated with this updater
        public RevitServicesUpdater(/*AddInId id, */ControlledApplication app, IEnumerable<IUpdater> updaters)
        {
            application = app;
            application.DocumentChanged += ApplicationDocumentChanged;

            foreach (var updater in updaters)
            {
                ((ElementTypeSpecificUpdater)updater).Updated += RevitServicesUpdater_Updated;
            }
        }

        /// <summary>
        /// Handler for the ElementTypeSpecificUpdater's Updated event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void RevitServicesUpdater_Updated(object sender, UpdaterArgs args)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var added = args.Added.Select(x => doc.GetElement(x).UniqueId);
            var addedIds = args.Added;
            var modified = args.Modified.Select(x => doc.GetElement(x).UniqueId).ToList();
            var deleted = args.Deleted;
            ProcessUpdates(doc, modified, deleted, added, addedIds);
        }

        public void Dispose()
        {
            application.DocumentChanged -= ApplicationDocumentChanged;
        }

        //TODO: remove once we are using unique ids
        /// <summary>
        /// Document that is being watched for changes.
        /// </summary>
        public Document DocumentToWatch
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        /// <summary>
        /// Forces all deletion callbacks to be called for given sequence of elements.
        /// </summary>
        /// <param name="doc">Document to perform the Rollback on.</param>
        /// <param name="deleted">Sequence of elements to have registered deletion callbacks invoked.</param>
        public void RollBack(Document document, ICollection<ElementId> deleted)
        {
            var empty = new List<string>();
            ProcessUpdates(document, empty, deleted, empty, new List<ElementId>());
        }

        private void ProcessUpdates(Document document, IEnumerable<string> modified, 
            IEnumerable<ElementId> deleted, IEnumerable<string> added, 
            IEnumerable<ElementId> addedIds )
        {
            OnElementsModified(document, modified.Distinct());
            OnElementsDeleted(document, deleted.Distinct());
            OnElementsAdded(document, added.Distinct());
            OnElementsAdded(document, addedIds);
        }

        void ApplicationDocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            var document = args.GetDocument();
            var added = args.GetAddedElementIds().Select(x => document.GetElement(x).UniqueId);
            var addedIds = args.GetAddedElementIds();
            var modified = args.GetModifiedElementIds().Select(x => document.GetElement(x).UniqueId).ToList();
            var deleted = args.GetDeletedElementIds();

            ProcessUpdates(document, modified, deleted, added, addedIds);
        }

        /// <summary>
        /// Clears all registered callbacks. This includes Modified and Deleted callbacks registered
        /// with RegisterChangeHook, and all delegates registered with the ElementsAdded event.
        /// </summary>
        public void UnRegisterAllChangeHooks()
        {
            ElementsAdded = null;
            ElementsModified = null;
            ElementsDeleted = null;
        }
    }

    /// <summary>
    /// Callback for when Elements have been added.
    /// </summary>
    /// <param name="document">Document from which added elements originated.</param>
    /// <param name="updated">All added elements that have been registered with this callback.</param>
    public delegate void ElementAddDelegate(Document document, IEnumerable<string> updated);


    /// <summary>
    /// Callback for when Elements have been updated.
    /// Recoemnt using the UUID version instead
    /// </summary>
    /// <param name="document">Document from which updated elements originated.</param>
    /// <param name="updated">All updated elements that have been registered with this callback.</param>
    public delegate void ElementUpdateElementIdDelegate(Document document, IEnumerable<ElementId> updated);
    
    /// <summary>
    /// Callback for when Elements have been modified.
    /// </summary>
    /// <param name="document">Document from which modified elements originated.</param>
    /// <param name="modified">All modified elements that have been registered with this callback.</param>
    public delegate void ElementModifyDelegate(Document document, IEnumerable<string> modified);

    /// <summary>
    ///     Callback for when Elements have been deleted.
    /// </summary>
    /// <param name="document">Document from which deleted elements originated.</param>
    /// <param name="deleted">The deleted ElementIds.</param>
    public delegate void ElementDeleteDelegate(Document document, IEnumerable<ElementId> deleted);
}

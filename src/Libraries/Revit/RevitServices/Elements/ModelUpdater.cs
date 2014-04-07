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

        private readonly Dictionary<string, ElementUpdateDelegate> deletedCallbacks = new Dictionary<string, ElementUpdateDelegate>();
        private readonly Dictionary<string, ElementUpdateDelegate> modifiedCallbacks = new Dictionary<string, ElementUpdateDelegate>();

        private readonly ControlledApplication application;

        public event ElementUpdateDelegate ElementsAdded;
        public event ElementUpdateDelegate ElementsModified;
        public event ElementUpdateDelegate ElementsDeleted;

        #region Event Invokers

        protected virtual void OnElementsModified(IEnumerable<string> updated)
        {
            var handler = ElementsModified;
            if (handler != null) handler(updated);
        }

        protected virtual void OnElementsDeleted(IEnumerable<string> updated)
        {
            var handler = ElementsDeleted;
            if (handler != null) handler(updated);
        }

        protected virtual void OnElementsAdded(IEnumerable<string> updated)
        {
            var handler = ElementsAdded;
            if (handler != null) handler(updated);
        }

        #endregion

        // constructor takes the AddInId for the add-in associated with this updater
        public RevitServicesUpdater(/*AddInId id, */ControlledApplication app)
        {
            application = app;
            application.DocumentChanged += Application_DocumentChanged;
        }

        public void Dispose()
        {
            application.DocumentChanged -= Application_DocumentChanged;
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
        /// <param name="deleted">Sequence of elements to have registered deletion callbacks invoked.</param>
        public void RollBack(ICollection<string> deleted)
        {
            var empty = new List<string>();
            ProcessUpdates(empty, deleted, empty);
        }

        private void ProcessUpdates(ICollection<string> modified, ICollection<string> deleted, IEnumerable<string> added)
        {
            #region Modified

            var dict = new Dictionary<ElementUpdateDelegate, HashSet<string>>();
            foreach (var modifiedElementID in modified)
            {
                if (!modifiedCallbacks.ContainsKey(modifiedElementID))
                    continue;

                var k = modifiedCallbacks[modifiedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<string>();
                dict[k].Add(modifiedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            OnElementsModified(modified.Distinct());

            #endregion

            #region Deleted

            dict.Clear();
            foreach (var deletedElementID in deleted)
            {
                if (!deletedCallbacks.ContainsKey(deletedElementID))
                    continue;

                var k = deletedCallbacks[deletedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<string>();
                dict[k].Add(deletedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            OnElementsDeleted(deleted.Distinct());

            #endregion

            #region Added

            OnElementsAdded(added.Distinct());

            #endregion
        }

        void Application_DocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            var doc = args.GetDocument();
            ProcessUpdates(
                args.GetModifiedElementIds().Select(x => doc.GetElement(x).UniqueId).ToList(),
                args.GetDeletedElementIds().Select(x => doc.GetElement(x).UniqueId).ToList(),
                args.GetAddedElementIds().Select(x => doc.GetElement(x).UniqueId).ToList());
        }

        /// <summary>
        /// Watches for changes of the given type to the Element with the given ID. When changed, executes
        /// the given Delegate.
        /// </summary>
        /// <param name="e">ID of the Element being watched.</param>
        /// <param name="type">Type of change to watch for.</param>
        /// <param name="d">Delegate to be called when changed.</param>
        public void RegisterChangeHook(string e, ChangeType type, ElementUpdateDelegate d)
        {
            switch (type)
            {
                case ChangeType.Delete:
                    deletedCallbacks[e] = d;
                    break;
                case ChangeType.Modify:
                    modifiedCallbacks[e] = d;
                    break;
            }
        }

        /// <summary>
        /// Unregisters an element that has been registered via RegisterChangeHook()
        /// </summary>
        /// <param name="e">ID of the Element to unregister.</param>
        /// <param name="type">Type of change to unsubscribe from.</param>
        public void UnRegisterChangeHook(string e, ChangeType type)
        {
            switch (type)
            {
                case ChangeType.Delete:
                    deletedCallbacks.Remove(e);
                    break;
                case ChangeType.Modify:
                    modifiedCallbacks.Remove(e);
                    break;
            }
        }

        /// <summary>
        /// Clears all registered callbacks. This includes Modified and Deleted callbacks registered
        /// with RegisterChangeHook, and all delegates registered with the ElementsAdded event.
        /// </summary>
        public void UnRegisterAllChangeHooks()
        {
            deletedCallbacks.Clear();
            modifiedCallbacks.Clear();
            ElementsAdded = null;
            ElementsModified = null;
            ElementsDeleted = null;
        }
    }

    /// <summary>
    /// Callback for when Elements have been updated.
    /// </summary>
    /// <param name="updated">All modified elements that have been registered with this callback.</param>
    public delegate void ElementUpdateDelegate(IEnumerable<string> updated);

    public enum ChangeType
    {
        Delete,
        Modify
    };
}

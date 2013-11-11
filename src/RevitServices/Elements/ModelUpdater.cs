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
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace RevitServices.Elements
{
    public class RevitServicesUpdater // : IUpdater
    {
        //static UpdaterId _mUpdaterId;

        //TODO: To handle multiple documents, should store unique ids as opposed to ElementIds.

        private readonly Dictionary<ElementId, ElementUpdateDelegate> _deletedCallbacks = new Dictionary<ElementId, ElementUpdateDelegate>();
        private readonly Dictionary<ElementId, ElementUpdateDelegate> _modifiedCallbacks = new Dictionary<ElementId, ElementUpdateDelegate>();

        public event ElementUpdateDelegate ElementsAdded;

        protected virtual void OnElementsAdded(HashSet<ElementId> updated)
        {
            var handler = ElementsAdded;
            if (handler != null) handler(updated);
        }

        // constructor takes the AddInId for the add-in associated with this updater
        public RevitServicesUpdater(/*AddInId id, */ControlledApplication app)
        {
            //_mUpdaterId = new UpdaterId(id, new Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")); //[Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")]
            
            app.DocumentChanged += Application_DocumentChanged;
        }

        //TODO: remove once we are using unique ids
        /// <summary>
        /// Document that is being watched for changes.
        /// </summary>
        public Document DocumentToWatch { get; set; }

        /// <summary>
        /// Forces all deletion callbacks to be called for given sequence of elements.
        /// </summary>
        /// <param name="deleted">Sequence of elements to have registered deletion callbacks invoked.</param>
        public void RollBack(IEnumerable<ElementId> deleted)
        {
            var empty = new List<ElementId>();
            ProcessUpdates(empty, deleted, empty);
        }

        private void ProcessUpdates(IEnumerable<ElementId> modified, IEnumerable<ElementId> deleted, IEnumerable<ElementId> added)
        {
            #region Modified

            var dict = new Dictionary<ElementUpdateDelegate, HashSet<ElementId>>();
            foreach (ElementId modifiedElementID in modified)
            {
                if (!_modifiedCallbacks.ContainsKey(modifiedElementID))
                    continue;

                var k = _modifiedCallbacks[modifiedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<ElementId>();
                dict[k].Add(modifiedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            #endregion

            #region Deleted

            dict.Clear();
            foreach (ElementId deletedElementID in deleted)
            {
                if (!_deletedCallbacks.ContainsKey(deletedElementID))
                    continue;

                var k = _deletedCallbacks[deletedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<ElementId>();
                dict[k].Add(deletedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            #endregion

            #region Added

            OnElementsAdded(new HashSet<ElementId>(added));

            #endregion
        }

        void Application_DocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            if (args.GetDocument().Equals(DocumentToWatch))
            {
                ProcessUpdates(
                   args.GetModifiedElementIds(),
                   args.GetDeletedElementIds(),
                   args.GetAddedElementIds());
            }
        }

        /// <summary>
        /// Watches for changes of the given type to the Element with the given ID. When changed, executes
        /// the given Delegate.
        /// </summary>
        /// <param name="e">ID of the Element being watched.</param>
        /// <param name="type">Type of change to watch for.</param>
        /// <param name="d">Delegate to be called when changed.</param>
        public void RegisterChangeHook(ElementId e, ChangeType type, ElementUpdateDelegate d)
        {
            switch (type)
            {
                case ChangeType.Delete:
                    _deletedCallbacks[e] = d;
                    break;
                case ChangeType.Modify:
                    _modifiedCallbacks[e] = d;
                    break;
            }
        }

        /// <summary>
        /// Unregisters an element that has been registered via RegisterChangeHook()
        /// </summary>
        /// <param name="e">ID of the Element to unregister.</param>
        /// <param name="type">Type of change to unsubscribe from.</param>
        public void UnRegisterChangeHook(ElementId e, ChangeType type)
        {
            switch (type)
            {
                case ChangeType.Delete:
                    _deletedCallbacks.Remove(e);
                    break;
                case ChangeType.Modify:
                    _modifiedCallbacks.Remove(e);
                    break;
            }
        }

        /// <summary>
        /// Clears all registered callbacks. This includes Modified and Deleted callbacks registered
        /// with RegisterChangeHook, and all delegates registered with the ElementsAdded event.
        /// </summary>
        public void UnRegisterAllChangeHooks()
        {
            _deletedCallbacks.Clear();
            _modifiedCallbacks.Clear();
            ElementsAdded = null;
        }

        /* Disabled IUpdater Methods
        public void Execute(UpdaterData data)
        {
            ProcessUpdates(
               data.GetModifiedElementIds(),
               data.GetDeletedElementIds(),
               data.GetAddedElementIds());
        }

        public string GetAdditionalInformation()
        {
            return "Watch for user-selected elements that have been changed or deleted and use this info to update Dynnamo";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return _mUpdaterId;
        }

        public string GetUpdaterName()
        {
            return "Dyanmo Element Watcher";
        }*/
    }

    /// <summary>
    /// Callback for when Elements have been updated.
    /// </summary>
    /// <param name="updated">All modified elements that have been registered with this callback.</param>
    public delegate void ElementUpdateDelegate(HashSet<ElementId> updated);

    public enum ChangeType
    {
        Delete,
        Modify
    };
}

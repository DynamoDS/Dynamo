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

using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace RevitServices.Elements
{
    public class RevitServicesUpdater // : IUpdater
    {
        //static UpdaterId _mUpdaterId;

        readonly Dictionary<ChangeTypeEnum, Dictionary<ElementId, ElementUpdateDelegate>> _updateDict
           = new Dictionary<ChangeTypeEnum, Dictionary<ElementId, ElementUpdateDelegate>>();

        // constructor takes the AddInId for the add-in associated with this updater
        public RevitServicesUpdater(/*AddInId id, */ControlledApplication app)
        {
            //_mUpdaterId = new UpdaterId(id, new Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")); //[Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")]

            _updateDict[ChangeTypeEnum.Delete] = new Dictionary<ElementId, ElementUpdateDelegate>();
            _updateDict[ChangeTypeEnum.Modify] = new Dictionary<ElementId, ElementUpdateDelegate>();
            _updateDict[ChangeTypeEnum.Add] = new Dictionary<ElementId, ElementUpdateDelegate>();

            app.DocumentChanged += Application_DocumentChanged;
        }

        public Document DocumentToWatch { get; set; }

        public void RollBack(IEnumerable<ElementId> deleted)
        {
            ProcessUpdates(new List<ElementId>(), deleted, new List<ElementId>());
        }

        private void ProcessUpdates(IEnumerable<ElementId> modified, IEnumerable<ElementId> deleted, IEnumerable<ElementId> added)
        {
            #region Modified

            var modDict = _updateDict[ChangeTypeEnum.Modify];
            var dict = new Dictionary<ElementUpdateDelegate, HashSet<ElementId>>();
            foreach (ElementId modifiedElementID in modified)
            {
                if (!modDict.ContainsKey(modifiedElementID))
                    continue;

                var k = modDict[modifiedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<ElementId>();
                dict[k].Add(modifiedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            #endregion

            #region Added

            modDict = _updateDict[ChangeTypeEnum.Add];
            dict.Clear();
            foreach (ElementId addedElementID in added)
            {
                if (!modDict.ContainsKey(addedElementID))
                    continue;

                var k = modDict[addedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<ElementId>();
                dict[k].Add(addedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

            #endregion

            #region Deleted

            modDict = _updateDict[ChangeTypeEnum.Delete];
            dict.Clear();
            foreach (ElementId deletedElementID in deleted)
            {
                if (!modDict.ContainsKey(deletedElementID))
                    continue;

                var k = modDict[deletedElementID];
                if (!dict.ContainsKey(k))
                    dict[k] = new HashSet<ElementId>();
                dict[k].Add(deletedElementID);
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);

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
        public void RegisterChangeHook(ElementId e, ChangeTypeEnum type, ElementUpdateDelegate d)
        {
            Dictionary<ElementId, ElementUpdateDelegate> dict;
            
            if (!_updateDict.TryGetValue(type, out dict))
            {
                dict = new Dictionary<ElementId, ElementUpdateDelegate>();
                _updateDict[type] = dict;
            }

            dict[e] = d;
        }

        /// <summary>
        /// Unregisters an element that has been registered via RegisterChangeHook()
        /// </summary>
        /// <param name="e">ID of the Element to unregister.</param>
        /// <param name="type">Type of change to unsubscribe from.</param>
        public void UnRegisterChangeHook(ElementId e, ChangeTypeEnum type)
        {
            _updateDict[type].Remove(e);
        }

        public void UnRegisterAllChangeHooks()
        {
            foreach (var hookDict in _updateDict.Values)
            {
                hookDict.Clear();
            }
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

    public delegate void ElementUpdateDelegate(HashSet<ElementId> updated);

    public enum ChangeTypeEnum
    {
        Delete,
        Modify,
        Add
    };

}

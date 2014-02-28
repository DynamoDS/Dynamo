using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;
using RevitServices.Persistence;

namespace Dynamo
{
    public delegate void DynElementUpdateDelegate(HashSet<ElementId> updated);

    public enum ChangeTypeEnum
    {
        Delete,
        Modify,
        Add
    };

    public class DynamoUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;

        readonly Dictionary<ChangeTypeEnum, Dictionary<ElementId, DynElementUpdateDelegate>> _updateDict
           = new Dictionary<ChangeTypeEnum, Dictionary<ElementId, DynElementUpdateDelegate>>();

        // constructor takes the AddInId for the add-in associated with this updater
        public DynamoUpdater(AddInId id, ControlledApplication app)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")); //[Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")]

            _updateDict[ChangeTypeEnum.Delete] = new Dictionary<ElementId, DynElementUpdateDelegate>();
            _updateDict[ChangeTypeEnum.Modify] = new Dictionary<ElementId, DynElementUpdateDelegate>();
            _updateDict[ChangeTypeEnum.Add] = new Dictionary<ElementId, DynElementUpdateDelegate>();

            app.DocumentChanged += Application_DocumentChanged;
        }

        public void RollBack(IEnumerable<ElementId> deleted)
        {
            processUpdates(new List<ElementId>(), deleted, new List<ElementId>());
        }

        private void processUpdates(IEnumerable<ElementId> modified, IEnumerable<ElementId> deleted, IEnumerable<ElementId> added)
        {

            #region Modified
            var modDict = _updateDict[ChangeTypeEnum.Modify];
            var dict = new Dictionary<DynElementUpdateDelegate, HashSet<ElementId>>();
            foreach (ElementId modifiedElementID in modified)
            {
                try
                {
                    if (!modDict.ContainsKey(modifiedElementID))
                        continue;

                    var k = modDict[modifiedElementID];
                    if (!dict.ContainsKey(k))
                        dict[k] = new HashSet<ElementId>();
                    dict[k].Add(modifiedElementID);
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log("Dynamic Model Update error while parsing modified elements.");
                    DynamoLogger.Instance.Log(e);
                }
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);
            #endregion

            #region Added
            modDict = _updateDict[ChangeTypeEnum.Add];
            dict.Clear();
            foreach (ElementId addedElementID in added)
            {
                try
                {
                    if (!modDict.ContainsKey(addedElementID))
                        continue;

                    var k = modDict[addedElementID];
                    if (!dict.ContainsKey(k))
                        dict[k] = new HashSet<ElementId>();
                    dict[k].Add(addedElementID);
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log("Dynamic Model Update error while parsing added elements.");
                    DynamoLogger.Instance.Log(e);
                }
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);
            #endregion

            #region Deleted
            modDict = _updateDict[ChangeTypeEnum.Delete];
            dict.Clear();
            foreach (ElementId deletedElementID in deleted)
            {
                try
                {
                    if (!modDict.ContainsKey(deletedElementID))
                        continue;

                    var k = modDict[deletedElementID];
                    if (!dict.ContainsKey(k))
                        dict[k] = new HashSet<ElementId>();
                    dict[k].Add(deletedElementID);
                }
                catch (Exception e)
                {
                    DynamoLogger.Instance.Log("Dynamic Model Update error while parsing deleted elements.");
                    DynamoLogger.Instance.Log(e);
                }
            }

            foreach (var pair in dict)
                pair.Key(pair.Value);
            #endregion
        }

        void Application_DocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            if (args.GetDocument().Equals(DocumentManager.GetInstance().CurrentUIDocument.Document))
            {
                processUpdates(
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
        public void RegisterChangeHook(ElementId e, ChangeTypeEnum type, DynElementUpdateDelegate d)
        {
            Dictionary<ElementId, DynElementUpdateDelegate> dict;
            if (!_updateDict.ContainsKey(type))
            {
                dict = new Dictionary<ElementId, DynElementUpdateDelegate>();
                _updateDict[type] = dict;
            }
            else
                dict = _updateDict[type];

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

        public void Execute(UpdaterData data)
        {
            processUpdates(
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
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Dyanmo Element Watcher";
        }
    }
}

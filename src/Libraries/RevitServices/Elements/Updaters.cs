using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitServices.Elements
{
    public delegate void UpdaterHandler(object sender, UpdaterArgs args);

    /// <summary>
    /// EventArgs structure designed to pass added, modified, and deleted 
    /// element ids during the Updated event.
    /// </summary>
    public class UpdaterArgs : EventArgs
    {
        public ICollection<ElementId> Added { get; set; }
        public ICollection<ElementId> Modified { get; set; }
        public ICollection<ElementId> Deleted { get; set; }

        public UpdaterArgs(ICollection<ElementId> added, ICollection<ElementId> modified, ICollection<ElementId> deleted)
        {
            Added = added;
            Modified = modified;
            Deleted = deleted;
        }
    }

    /// <summary>
    /// Base class for element updaters.
    /// </summary>
    public abstract class ElementTypeSpecificUpdater : IUpdater
    {
        private readonly AddInId _id;
        private readonly UpdaterId _updaterId;

        public event UpdaterHandler Updated;

        protected virtual void OnUpdated(UpdaterArgs args)
        {
            if (Updated != null)
            {
                Updated(this, args);
            }
        }

        protected ElementTypeSpecificUpdater(AddInId id)
        {
            _id = id;
            _updaterId = new UpdaterId(_id, Guid.Parse("51c0be9b-07a8-443a-be8c-21db815b17e7"));
        }

        public virtual void Execute(UpdaterData data)
        {
            var added = data.GetAddedElementIds();
            var modded = data.GetModifiedElementIds();
            var deleted = data.GetDeletedElementIds();

            if (added.Any() || modded.Any() || deleted.Any())
            {
                OnUpdated(new UpdaterArgs(added, modded, deleted));
            }
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public virtual ChangePriority GetChangePriority()
        {
            return ChangePriority.GridsLevelsReferencePlanes;
        }

        public virtual string GetUpdaterName()
        {
            return "Updater Base";
        }

        public virtual string GetAdditionalInformation()
        {
            return "Updater base.";
        }
    }

    /// <summary>
    /// Updater which is designed to be used for Sun and Shadow Settings document modifications.
    /// </summary>
    public class SunPathUpdater : ElementTypeSpecificUpdater
    {
        public SunPathUpdater(AddInId id):base(id){}

        public override void Execute(UpdaterData data)
        {
            var modded = data.GetModifiedElementIds();

            if (modded.Any())
            {
                OnUpdated(new UpdaterArgs(new List<ElementId>(), modded, new List<ElementId>()));
            }
        }

        public override string GetUpdaterName()
        {
            return "Sun and Shadow Settings Updater";
        }

        public override string GetAdditionalInformation()
        {
            return "The Sun and Shadow Settings updater tracks changes to the Sun and Shadow Settings for a document.";
        }
    }
}

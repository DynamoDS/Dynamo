using System;
using System.ComponentModel;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Graph.Notes
{
    /// <summary>
    /// NoteModel represents notes in Dynamo.
    /// </summary>
    public class NoteModel : ModelBase
    {
        public enum UndoAction
        {
            Pin, Unpin
        }

        /// <summary>
        /// This action is triggered when undo command is pressed and a node is pinned
        /// </summary>
        internal event Action<ModelBase> UndoRequest;

        private string text;

        /// <summary>
        /// Returns the text inside the note.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged("Text");
            }
        }

        private NodeModel pinnedNode;

        /// <summary>
        /// NodeModel which this Note is pinned to
        /// When using the pin to node command  
        /// note and node become entangled so that 
        /// if you select and move one the other one 
        /// moves as well. 
        /// </summary>
        public NodeModel PinnedNode
        {
            get { return pinnedNode; }
            set
            {
                pinnedNode = value;
                RaisePropertyChanged(nameof(PinnedNode));
            }
        }

        private Guid pinnedNodeGuid;

        public Guid PinnedNodeGuid
        {
            get { return pinnedNodeGuid; }
            set
            {
                pinnedNodeGuid = value;
                RaisePropertyChanged(nameof(PinnedNodeGuid));
            }
        }

        private UndoAction undoAction;
        public UndoAction UndoRedoAction
        {
            get { return undoAction; }
            set
            {
                undoAction = value;
                RaisePropertyChanged(nameof(UndoRedoAction));
            }
        }

        /// <summary>
        /// Creates NoteModel.
        /// </summary>
        /// <param name="x">X coordinate of note.</param>
        /// <param name="y">Y coordinate of note.</param>
        /// <param name="text">Text of note</param>
        /// <param name="guid">Unique id of note</param>
        public NoteModel(double x, double y, string text, Guid guid)
        {
            X = x;
            Y = y;
            Text = text;
            GUID = guid;
            PinnedNode = pinnedNode;
        }

        /// <summary>
        /// Creates NoteModel with a reference to a pinned node.
        /// </summary>
        /// <param name="x">X coordinate of note.</param>
        /// <param name="y">Y coordinate of note.</param>
        /// <param name="text">Text of note</param>
        /// <param name="guid">Unique id of note</param>
        /// <param name="pinnedNode">Pinned NodeModel</param>
        public NoteModel(double x, double y, string text, Guid guid, NodeModel pinnedNode)
        {
            X = x;
            Y = y;
            Text = text;
            GUID = guid;
            PinnedNode = pinnedNode;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name != "Text")
                return base.UpdateValueCore(updateValueParams);

            Text = value;

            return true;
        }
        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("text", Text);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
            helper.SetAttribute("pinnedNode", pinnedNode == null ? Guid.Empty : pinnedNode.GUID);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);
            GUID = helper.ReadGuid("guid", GUID);
            Text = helper.ReadString("text", "New Note");
            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);

            try
            {
                PinnedNodeGuid = helper.ReadGuid("pinnedNode");
            }
            catch (Exception) { }

            if (pinnedNode != null && helper.ReadGuid("pinnedNode") != Guid.Empty)
                pinnedNode.GUID = helper.ReadGuid("pinnedNode");

            // Notify listeners that the position of the note has changed, 
            // then parent group will also redraw itself.
            ReportPosition();
            TryToSubscribeUndoNote();
        }

        /// <summary>
        /// Verify if the current user action is to pin a node so the 'unpin' method can be called to undo the action
        /// </summary>
        internal void TryToSubscribeUndoNote()
        {
            if (pinnedNode != null && PinnedNodeGuid == Guid.Empty && UndoRequest != null)
            {
                UndoRedoAction = UndoAction.Unpin;
                UndoRequest(this);
                return;
            }
            else if (pinnedNode == null && PinnedNodeGuid != Guid.Empty && UndoRequest != null)
            {
                UndoRedoAction = UndoAction.Pin;
                UndoRequest(this);
            }
        }

        #endregion
    }
}

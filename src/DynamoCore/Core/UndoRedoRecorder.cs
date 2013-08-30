using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;

namespace Dynamo.Core
{
    class UndoRedoRecorder
    {
        internal UndoRedoRecorder(dynWorkspaceModel workspace)
        {
            // An UndoRedoRecorder is a singleton with respect to its owning 
            // workspace. The recorder needs access to the workspace because 
            // in an undo/redo operation, it may choose to modify the workspace 
            // model data. For example, undoing a "deletion of a node" would 
            // require the recorder to "re-insert the node" back into workspace.
            // 
            this.owningWorkspace = workspace;
        }

        internal void BeginGroup()
        {
            if (null != currentGroup)
                throw new InvalidOperationException("BeginGroup called twice");

            currentGroup = document.CreateElement("UndoGroup");
        }

        internal void EndGroup()
        {
            if (null == currentGroup)
                throw new InvalidOperationException("No open group to end");

            if (currentGroup.HasChildNodes)
                undoStack.Add(currentGroup);

            currentGroup = null;
        }

        private static string CreationAction = "Creation";
        private static string ModificationAction = "Modification";
        private static string DeletionAction = "Deletion";

        // Three primary methods for the recorder to record a model before 
        // it is deleted or modified; or right after the model is created.
        // 
        internal void RecordCreationForUndo(dynModelBase model) { }
        internal void RecordDeletionForUndo(dynModelBase model) { }
        internal void RecordModificationForUndo(dynModelBase model)
        {
            // Omitted: Ensure this model has not been recorded in the 
            // current group so far (we don't want to double record it).

            // Get the model to serialize itself into XmlNode form, with the 
            // same set of data that is used during DYN file loading/saving.
            // Then mark this XmlNode with "modification" as its action, so 
            // we know what to do with it during undo operation (for example,
            // if this action is "creation", then during undo, the model will
            // be deleted).
            // 
            XmlNode childNode = null; // model.Serialize();
            XmlAttribute actionAttribute = document.CreateAttribute("UserAction");
            actionAttribute.Value = UndoRedoRecorder.ModificationAction;

            // Note that there may be more than one affected node in a single 
            // user action. The simplest example of this would be a bunch of 
            // nodes getting dragged around by user, so in a single undo group 
            // there will be all the nodes being dragged around.
            // 
            childNode.Attributes.Append(actionAttribute);
            currentGroup.AppendChild(childNode);
        }

        internal void Undo() { }
        internal void Redo() { }

        private dynWorkspaceModel owningWorkspace = null;
        private XmlDocument document = new XmlDocument();
        private XmlElement currentGroup = null;
        private List<XmlElement> undoStack = null;
        private List<XmlElement> redoStack = null;
    }
}

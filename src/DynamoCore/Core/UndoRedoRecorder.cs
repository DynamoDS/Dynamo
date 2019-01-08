using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;

namespace Dynamo.Core
{
    /// <summary>
    /// An instance of UndoRedoRecorder is owned by an "undo client" object. In 
    /// the context of Dynamo, the undo client is "Workspace". The undo recorder 
    /// calls into the owning undo client in an undo/redo operation, causing the 
    /// client to delete, reload or create the corresponding model. To qualify
    /// as an undo client, a class must implement this interface.
    /// </summary>
    internal interface IUndoRedoRecorderClient
    {
        /// <summary>
        /// UndoRedoRecorder calls this method to delete a model in the client.
        /// </summary>
        /// <param name="modelData">The data representing the model to be 
        /// deleted. It is important that this element contains identifiable 
        /// information so that the corresponding model can be located in the 
        /// client for deletion.
        /// </param>
        void DeleteModel(XmlElement modelData);

        /// <summary>
        /// UndoRedoRecorder calls this method to request the client to reload 
        /// a given model by giving its data.
        /// </summary>
        /// <param name="modelData">The xml data from which the corresponding 
        /// model can be reloaded from.</param>
        void ReloadModel(XmlElement modelData);

        /// <summary>
        /// UndoRedoRecorder calls this method to request a model to be created.
        /// </summary>
        /// <param name="modelData">The xml data from which the corresponding 
        ///     model can be re-created from.</param>
        void CreateModel(XmlElement modelData);

        /// <summary>
        /// UndoRedoRecorder calls this method to retrieve the up-to-date 
        /// instance of the model before any undo/redo operation modifies the 
        /// model. The up-to-date information of the model is important so that
        /// an undo operation can be redone (repopulated with the up-to-date 
        /// data before the undo operation happens).
        /// </summary>
        /// <param name="modelData">The xml data representing the model which 
        /// UndoRedoRecorder requires for serialization purposes.</param>
        /// <returns>Returns the model that modelData corresponds to.</returns>
        ModelBase GetModelForElement(XmlElement modelData);
    }

    internal class UndoRedoRecorder
    {
        #region Private Class Data Members

        public enum UserAction
        {
            Creation,
            Modification,
            Deletion
        }

        private const string UserActionAttrib = "UserAction";
        private const string ActionGroup = "ActionGroup";

        private readonly IUndoRedoRecorderClient undoClient;
        private readonly XmlDocument document = new XmlDocument();
        private XmlElement currentActionGroup;
        private readonly Stack<XmlElement> undoStack;
        private readonly Stack<XmlElement> redoStack;
        private HashSet<Guid> offTrackModels;

        #endregion

        #region Public Class Operational Methods

        public UndoRedoRecorder(IUndoRedoRecorderClient undoClient)
        {
            if (null == undoClient)
                throw new ArgumentNullException("undoClient");

            // A single instance of UndoRedoRecorder serves a single undo client
            // object, which implements IUndoRedoRecorderClient interface. See 
            // the definition of IUndoRedoRecorderClient for more information.
            // 
            this.undoClient = undoClient;

            undoStack = new Stack<XmlElement>();
            redoStack = new Stack<XmlElement>();
        }

        /// <summary>
        /// <para>For a series of actions to be recorded for undo, an "action 
        /// group" needs to be opened. An action group is the smallest unit in 
        /// which undo or redo can be done. For example, dragging a set of nodes 
        /// on the UI results in those nodes getting recorded in a single action 
        /// group, when undo is done, the set of recorded nodes get reverted in 
        /// one undo command.</para>
        /// <para>It is mandatory for the caller of this method to call 
        /// EndActionGroup when the undo recording is done for the current 
        /// action group. Failing to do so will result in subsequent calls to 
        /// BeginActionGroup to throw an exception.</para>
        /// </summary>
        public IDisposable BeginActionGroup()
        {
            EnsureValidRecorderStates();
            currentActionGroup = document.CreateElement(ActionGroup);
            return new ActionGroupDisposable(this);
        }

        /// <summary>
        /// Call this method to close the currently opened action group, wrapping 
        /// all recorded actions as part of the group. Actions in an action group
        /// get undone/redone at one go with a single undo/redo command.
        /// </summary>
        private void EndActionGroup()
        {
            if (null == currentActionGroup)
                throw new InvalidOperationException("No open group to end");

            // If there wasn't anything recorded between BeginActionGroup and 
            // the corresponding EndActionGroup method, then the action group 
            // is discarded.
            if (currentActionGroup.HasChildNodes)
                undoStack.Push(currentActionGroup);

            currentActionGroup = null;
        }

        public void Undo()
        {
            EnsureValidRecorderStates();

            if (CanUndo == false)
                return; // Nothing to be undone.

            // Before undo operation, ensure the top-most item on the undo
            // stack is pushed onto the redo stack so that the same action can 
            // be carried out when user performs a redo action.
            // 
            XmlElement topMostAction = PopActionGroupFromUndoStack();
            UndoActionGroup(topMostAction); // Perform the actual undo activities.
        }

        public void Redo()
        {
            EnsureValidRecorderStates();

            if (CanRedo == false)
                return; // Nothing to be redone.

            // Top-most group gets moved from redo stack to undo stack.
            XmlElement topMostAction = PopActionGroupFromRedoStack();
            RedoActionGroup(topMostAction); // Perform the actual redo activities.
        }

        /// <summary>
        /// Call this method to clear the internal undo/redo stacks, effectively
        /// destroying all the recorded actions. This is desirable for example,
        /// when the workspace is cleared to be used as a clean slate for future
        /// works, causing recorded actions become irrelevant.
        /// </summary>
        public void Clear()
        {
            EnsureValidRecorderStates();
            undoStack.Clear();
            redoStack.Clear();
        }

        #endregion

        #region Public Undo Recording Methods

        /// <summary>
        /// Record the given model right after it has been created. This results
        /// in a creation action to be recorded under the current action group.
        /// Undoing this action will result in the model being deleted.
        /// </summary>
        /// <param name="model">The model to be recorded.</param>
        public void RecordCreationForUndo(ModelBase model)
        {
            RecordActionInternal(currentActionGroup,
                model, UserAction.Creation);

            redoStack.Clear(); // Wipe out the redo-stack.
        }

        /// <summary>
        /// Record the given model right before it has been deleted. This 
        /// results in a deletion action to be recorded under the current action 
        /// group. Undoing this action will result in the model being created 
        /// and re-inserted into the current workspace.
        /// </summary>
        /// <param name="model">The model to be recorded.</param>
        public void RecordDeletionForUndo(ModelBase model)
        {
            RecordActionInternal(currentActionGroup,
                model, UserAction.Deletion);

            redoStack.Clear(); // Wipe out the redo-stack.
        }

        /// <summary>
        /// Record the given model right before it is modified. This results
        /// in a modification action to be recorded under the current action 
        /// group. Undoing this action will result in the model being reverted
        /// to the states that it was in before the modification took place.
        /// </summary>
        /// <param name="model">The model to be recorded.</param>
        public void RecordModificationForUndo(ModelBase model)
        {
            RecordActionInternal(currentActionGroup,
                model, UserAction.Modification);

            redoStack.Clear(); // Wipe out the redo-stack.
        }

        /// <summary>
        /// This function removes the top most item from the UndoStack. In 
        /// order to preserve continuity of both undo and redo stacks, a pop 
        /// action cannot be done when undo has unwinded some user actions, 
        /// leaving them on the redo stack. This partially helps making sure 
        /// the method is only called when the caller recognizes the most 
        /// recent item that was pushed onto the undo stack (and that it does 
        /// not accidentally pop an action that is irrelevant).
        /// </summary>
        /// <returns>Returns the XmlElement representing the action group that 
        /// is on top of the stack at the time pop is requested.</returns>
        public XmlElement PopFromUndoGroup()
        {
            if (redoStack.Count > 0)
            {
                throw new InvalidOperationException(
                    "UndoStack cannot be popped with non-empty RedoStack");
            }

            return PopActionGroupFromUndoStack();
        }
        #endregion

        #region Public Class Properties

        public bool CanUndo { get { return undoStack.Count > 0; } }
        public bool CanRedo { get { return redoStack.Count > 0; } }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// This method is called at the beginning of any method that requires 
        /// any existing undo group to be closed before proceeding. For example,
        /// UndoRedoRecorder.OpenGroup cannot be called again while there is an
        /// existing undo group that is left open.
        /// </summary>
        private void EnsureValidRecorderStates()
        {
            if (null != currentActionGroup)
            {
                const string message = "An existing open undo group detected";
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// The recorder calls this method to determine if a given model has 
        /// already been recorded in the active action group. For an example,
        /// if there is a connection between NodeA and NodeB, selecting both 
        /// the nodes and deleting them will cause the connection model to be 
        /// recorded twice (when a node is deleted, its connections are being 
        /// recorded for undo).
        /// </summary>
        /// <param name="group">The action group to check against.</param>
        /// <param name="model">The model to check against.</param>
        /// <returns>Returns true if the model has already been recorded in the
        /// current action group, or false otherwise.</returns>
        private bool IsRecordedInActionGroup(XmlElement group, ModelBase model)
        {
            if (null == group)
                throw new ArgumentNullException("group");
            if (null == model)
                throw new ArgumentNullException("model");

            Guid guid = model.GUID;
            foreach (XmlNode childNode in group.ChildNodes)
            {
                // See if the model supports Guid identification, in unit test cases 
                // those sample models do not support this so in such cases identity 
                // check will not be performed.
                // 
                XmlAttribute guidAttribute = childNode.Attributes["guid"];
                if (null != guidAttribute && (guid == Guid.Parse(guidAttribute.Value)))
                    return true; // This model was found to be recorded.
            }

            return false;
        }

        private void SetNodeAction(XmlNode childNode, string action)
        {
            XmlAttribute actionAttribute = document.CreateAttribute(UserActionAttrib);
            actionAttribute.Value = action;
            childNode.Attributes.Append(actionAttribute);
        }

        private XmlElement PopActionGroupFromUndoStack()
        {
            if (CanUndo == false)
            {
                throw new InvalidOperationException("Invalid call to " +
                    "'PopActionGroupFromUndoStack' when the undo stack is empty");
            }

            return undoStack.Pop();
        }

        private XmlElement PopActionGroupFromRedoStack()
        {
            if (CanRedo == false)
            {
                throw new InvalidOperationException("Invalid call to " +
                    "'PopActionGroupFromRedoStack' when the undo stack is empty");
            }

            return redoStack.Pop();
        }

        private void RecordActionInternal(XmlElement group, ModelBase model, UserAction action)
        {
            if (IsRecordedInActionGroup(group, model))
                return;

            // Serialize the affected model into xml representation
            // and store it under the current action group.
            XmlNode childNode = model.Serialize(document, SaveContext.Undo);
            SetNodeAction(childNode, action.ToString());
            group.AppendChild(childNode);
        }

        private void UndoActionGroup(XmlElement actionGroup)
        {
            // This is the action group where all the undone actions are added.
            XmlElement newGroup = document.CreateElement(ActionGroup);

            // As we iterate through each child node under "actionGroup", 
            // occassionally we may want to appand that child node under 
            // "newGroup". This action causes the child node to be removed, and 
            // "actionGroup.ChildNodes" to drop by one. This means that 
            // "actionGroup.ChildNodes" to be non-constant, something the loop 
            // cannot iterate over correctly. So here we make a duplicated copy
            // instead.
            // 
            var actions = actionGroup.ChildNodes.Cast<XmlNode>().ToList();

            // In undo scenario, user actions are undone in the reversed order 
            // that they were done (due to inter-dependencies among components).
            // 
            for (int index = actions.Count - 1; index >= 0; index--)
            {
                var element = actions[index] as XmlElement;

                XmlAttribute actionAttribute = element.Attributes[UserActionAttrib];
                var modelActionType = (UserAction)Enum.Parse(typeof(UserAction), actionAttribute.Value);

                switch (modelActionType)
                {
                    // Before undo takes place (to delete the model), the most 
                    // up-to-date model is retrieved and serialized into the 
                    // redo action group so that it can properly be redone later.
                    case UserAction.Creation:

                        ModelBase toBeDeleted = undoClient.GetModelForElement(element);
                        if (toBeDeleted != null)
                        {
                            RecordActionInternal(newGroup, toBeDeleted, modelActionType);
                            undoClient.DeleteModel(element);
                        }
                        break;

                    case UserAction.Modification:
                        ModelBase toBeUpdated = undoClient.GetModelForElement(element);
                        if (toBeUpdated != null)
                        {

                            RecordActionInternal(newGroup, toBeUpdated, modelActionType);
                            undoClient.ReloadModel(element);
                        }
                        break;

                    case UserAction.Deletion:
                        newGroup.AppendChild(element);
                        undoClient.CreateModel(element);
                        break;
                }
            }

            redoStack.Push(newGroup); // Place the states on the redo-stack.
        }

        private void RedoActionGroup(XmlElement actionGroup)
        {
            // This is the action group where all the redone actions are added.
            XmlElement newGroup = document.CreateElement(ActionGroup);

            // See "UndoActionGroup" above for details why this duplicate.
            var actions = actionGroup.ChildNodes.Cast<XmlNode>().ToList();

            // Redo operation is the reversed of undo operation, naturally.
            for (int index = actions.Count - 1; index >= 0; index--)
            {
                var element = actions[index] as XmlElement;
                XmlAttribute actionAttribute = element.Attributes[UserActionAttrib];
                var modelActionType = (UserAction)Enum.Parse(typeof(UserAction), actionAttribute.Value);
                switch (modelActionType)
                {
                    case UserAction.Creation:
                        newGroup.AppendChild(element);
                        undoClient.CreateModel(element);
                        break;

                    case UserAction.Modification:
                        ModelBase toBeUpdated = undoClient.GetModelForElement(element);
                        if (toBeUpdated != null)
                        {

                            RecordActionInternal(newGroup, toBeUpdated, modelActionType);
                            undoClient.ReloadModel(element);
                        }
                        break;

                    case UserAction.Deletion:
                        ModelBase toBeDeleted = undoClient.GetModelForElement(element);
                        if (toBeDeleted != null)
                        {
                            RecordActionInternal(newGroup, toBeDeleted, modelActionType);
                            undoClient.DeleteModel(element);
                        }
                        break;
                }
            }

            undoStack.Push(newGroup);
        }

        #endregion

        #region Nested Undo Helper Classes

        private sealed class ActionGroupDisposable : IDisposable
        {
            private readonly UndoRedoRecorder recorder;
            public ActionGroupDisposable(UndoRedoRecorder recorder)
            {
                this.recorder = recorder;
            }

            public void Dispose()
            {
                recorder.EndActionGroup();
            }
        }

        internal class ModelModificationUndoHelper : IDisposable
        {
            private readonly List<ModelBase> models;
            private readonly UndoRedoRecorder recorder;
            private readonly Dictionary<Guid, XmlElement> existingConnectors;
            private readonly Dictionary<Guid, ConnectorModel> remainingConnectors;

            public ModelModificationUndoHelper(UndoRedoRecorder recorder, ModelBase model)
                : this(recorder, new [] { model })
            {
            }

            public ModelModificationUndoHelper(UndoRedoRecorder recorder, IEnumerable<ModelBase> models)
            {
                this.recorder = recorder;

                this.models = new List<ModelBase>(models);
                existingConnectors = new Dictionary<Guid, XmlElement>();
                remainingConnectors = new Dictionary<Guid, ConnectorModel>();

                var allConnectors = new List<ConnectorModel>();
                using (this.recorder.BeginActionGroup())
                {
                    // Assuming no connectors will be modified as part of this, we 
                    // record the node prior to it being modified. If for some 
                    // reason connectors are dropped/created along the way, then 
                    // this particular action group will be pop off the undo stack.
                    // 
                    foreach (var model in this.models)
                    {
                        var nodeModel = model as NodeModel;
                        if (nodeModel != null)
                            allConnectors.AddRange(nodeModel.AllConnectors);

                        this.recorder.RecordModificationForUndo(model);
                    }
                }

                // Record the existing connectors...
                foreach (var connectorModel in allConnectors)
                {
                    var element = connectorModel.Serialize(
                        recorder.document, SaveContext.Undo);

                    existingConnectors[connectorModel.GUID] = element;
                }
            }

            public void Dispose()
            {
                foreach (var modelBase in models)
                {
                    if (!(modelBase is NodeModel)) // Only nodes have connectors.
                        continue;

                    var nodeModel = modelBase as NodeModel;
                    foreach (var connectorModel in nodeModel.AllConnectors)
                    {
                        // Connectors after node is modified.
                        remainingConnectors[connectorModel.GUID] = connectorModel;
                    }
                }

                var removed = new List<XmlElement>();
                var added = new List<ConnectorModel>();
                if (!ComputeDifference(removed, added))
                    return; // No difference in connectors.

                // If there are differences in connector count, some connectors must 
                // have been dropped or created. In this case the originally recorded
                // entry on the undo stack must be discarded, and recreated in the 
                // same action group as these connectors.
                // 
                var previousGroup = recorder.PopFromUndoGroup();

                // Create a new action group to record changes 
                // affecting the node and its connectors.
                using (recorder.BeginActionGroup())
                {
                    // For each of the deleted connectors, record its respective
                    // XmlElement that was serialized before they were deleted.
                    var deletionString = UserAction.Deletion.ToString();
                    foreach (var connector in removed)
                    {
                        recorder.SetNodeAction(connector, deletionString);
                        recorder.currentActionGroup.AppendChild(connector);
                    }

                    foreach (XmlNode childNode in previousGroup.ChildNodes)
                    {
                        // Record the model modification itself.
                        recorder.currentActionGroup.AppendChild(childNode);
                    }

                    foreach (var connector in added)
                    {
                        recorder.RecordCreationForUndo(connector);
                    }

                    // When a new action group is recorded for undo,
                    // the redo stack should always be cleared.
                    if (recorder.redoStack.Count > 0)
                    {
                        throw new InvalidOperationException(
                            "Redo stack should be empty after recording!");
                    }
                }
            }

            private bool ComputeDifference(List<XmlElement> removed, List<ConnectorModel> added)
            {
                // Whatever that was in the existing set but no longer exist...
                var deletedKeys = existingConnectors.Keys.Except(remainingConnectors.Keys);
                removed.AddRange(deletedKeys.Select(key => existingConnectors[key]));

                // Whatever that did not originally exist but got created...
                var addedConnectors = remainingConnectors.Keys.Except(
                    existingConnectors.Keys);

                added.AddRange(addedConnectors.Select(
                    connectorKey => remainingConnectors[connectorKey]));

                return removed.Any() || added.Any();
            }
        }

        #endregion
    }
}

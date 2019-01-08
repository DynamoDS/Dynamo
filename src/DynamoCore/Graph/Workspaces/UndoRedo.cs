using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Utilities;

namespace Dynamo.Graph.Workspaces
{
    public partial class WorkspaceModel
    {
        /// <summary>
        /// Returns the current UndoRedoRecorder that is associated with the current
        /// WorkspaceModel. Note that external parties should not have the needs
        /// to access the recorder directly, so this property is exposed just as
        /// a "temporary solution". Before using this property, consider using
        /// WorkspaceModel.RecordModelsForUndo method which allows for multiple
        /// modifications in a single action group.
        /// </summary>
        internal UndoRedoRecorder UndoRecorder
        {
            get { return undoRecorder; }
        }

        /// <summary>
        ///     Determine if undo operation is currently possible.
        /// </summary>
        public bool CanUndo
        {
            get { return ((null != undoRecorder) && undoRecorder.CanUndo); }
        }

        /// <summary>
        ///     Determine if redo operation is currently possible.
        /// </summary>
        public bool CanRedo
        {
            get { return ((null != undoRecorder) && undoRecorder.CanRedo); }
        }

        internal void Undo()
        {
            if (null != undoRecorder)
            {
                undoRecorder.Undo();

                // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7883
                // Request run for every undo action
                RequestRun();
            }
        }

        internal void Redo()
        {
            if (null != undoRecorder)
            {
                undoRecorder.Redo();

                // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7883
                // Request run for every redo action
                RequestRun();
            }
        }

        internal void ClearUndoRecorder()
        {
            if (null != undoRecorder)
                undoRecorder.Clear();
        }

        // See RecordModelsForModification below for more details.
        internal static void RecordModelForModification(ModelBase model, UndoRedoRecorder recorder)
        {
            if (null != model)
            {
                var models = new List<ModelBase> { model };
                RecordModelsForModification(models, recorder);
            }
        }

        /// <summary>
        /// TODO(Ben): This method is exposed this way for external codes (e.g.
        /// the DragCanvas) to record models before they are modified. This is
        /// by no means ideal. The ideal case of course is for ALL codes that
        /// end up modifying models to be folded back into WorkspaceViewModel in
        /// the form of commands. These commands then internally record those
        /// affected models before updating them. We need this method to be gone
        /// sooner than later.
        /// </summary>
        /// <param name="models">The models to be recorded for undo.</param>
        /// <param name="recorder"></param>
        internal static void RecordModelsForModification(List<ModelBase> models, UndoRedoRecorder recorder)
        {
            if (null == recorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            using (recorder.BeginActionGroup())
            {
                foreach (var model in models)
                    recorder.RecordModificationForUndo(model);
            }
        }

        internal static void RecordModelsForUndo(Dictionary<ModelBase, UndoRedoRecorder.UserAction> models, UndoRedoRecorder recorder)
        {
            if (null == recorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            if (null != savedModels)
            {
                // Before an existing connector is reconnected, we have one action group
                // which records the deletion of the connector. Pop that out so that we can
                // record the deletion and reconnection in one action group.
                recorder.PopFromUndoGroup();
            }

            using (recorder.BeginActionGroup())
            {
                if (null != savedModels)
                {
                    foreach (var modelPair in savedModels)
                    {
                        recorder.RecordDeletionForUndo(modelPair);
                    }
                    savedModels = null;
                }
                foreach (var modelPair in models)
                {
                    switch (modelPair.Value)
                    {
                        case UndoRedoRecorder.UserAction.Creation:
                            recorder.RecordCreationForUndo(modelPair.Key);
                            break;
                        case UndoRedoRecorder.UserAction.Deletion:
                            recorder.RecordDeletionForUndo(modelPair.Key);
                            break;
                        case UndoRedoRecorder.UserAction.Modification:
                            recorder.RecordModificationForUndo(modelPair.Key);
                            break;
                    }
                }
            }
        }

        internal void RecordCreatedModel(ModelBase model)
        {
            if (null == model) return;

            using (undoRecorder.BeginActionGroup())
            {
                undoRecorder.RecordCreationForUndo(model);
            }
        }

        internal void RecordCreatedModels(List<ModelBase> models)
        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing created.

            using (undoRecorder.BeginActionGroup())
            {
                foreach (ModelBase model in models)
                    undoRecorder.RecordCreationForUndo(model);
            }
        }

        internal void RecordAndDeleteModels(List<ModelBase> models)

        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing for deletion.

            // Gather a list of connectors first before the nodes they connect
            // to are deleted. We will have to delete the connectors first
            // before

            using (undoRecorder.BeginActionGroup()) // Start a new action group.
            {
                foreach (var model in models)
                {
                    if (model is NoteModel)
                    {
                        // Take a snapshot of the note before it goes away.
                        undoRecorder.RecordDeletionForUndo(model);
                        RemoveNote(model as NoteModel);
                    }
                    else if (model is AnnotationModel)
                    {
                        undoRecorder.RecordDeletionForUndo(model);
                        RemoveAnnotation(model as AnnotationModel);
                    }

                    else if (model is PresetModel)
                    {
                        undoRecorder.RecordDeletionForUndo(model);
                        this.RemovePreset(model as PresetModel);
                    }

                    else if (model is NodeModel)
                    {
                        // Just to make sure we don't end up deleting nodes from
                        // another workspace (potentially two issues: the node was
                        // having its "Workspace" pointing to another workspace,
                        // or the selection set was not quite set up properly.
                        //
                        var node = model as NodeModel;
                        Debug.Assert(Nodes.Contains(node));

                        bool silentFlag = node.RaisesModificationEvents;
                        node.RaisesModificationEvents = false;

                        // Note that AllConnectors is duplicated as a separate list
                        // by calling its "ToList" method. This is the because the
                        // "Connectors.Remove" will modify "AllConnectors", causing
                        // the Enumerator in this "foreach" to become invalid.
                        foreach (var conn in node.AllConnectors.ToList())
                        {
                            conn.Delete();
                            undoRecorder.RecordDeletionForUndo(conn);
                        }

                        node.RaisesModificationEvents = silentFlag;

                        // Take a snapshot of the node before it goes away.
                        undoRecorder.RecordDeletionForUndo(node);

                        RemoveAndDisposeNode(node);
                    }
                    else if (model is ConnectorModel)
                    {
                        undoRecorder.RecordDeletionForUndo(model);
                    }
                }

                RequestRun();

            } // Conclude the deletion.
        }

        internal void DeleteSavedModels()
        {
            savedModels = null;
        }

        internal void SaveAndDeleteModels(List<ModelBase> models)
        {
            if (null != models)
            {
                // If an existing connector is grabbed (to be reconnected), save the 
                // models for deletion later in one action group.
                savedModels = models;

                // After saving the models, delete them from the workspace
                // in one action group.
                RecordAndDeleteModels(models);
            }
        }

        internal void RecordGroupModelBeforeUngroup(AnnotationModel annotation)
        {
            using (undoRecorder.BeginActionGroup()) // Start a new action group.
            {
                undoRecorder.RecordModificationForUndo(annotation);
            }
        }

        private static bool ShouldProceedWithRecording(List<ModelBase> models)
        {
            if (null == models)
                return false;

            models.RemoveAll(x => x == null);
            return models.Count > 0;
        }

        private static bool ShouldProceedWithRecording(
            Dictionary<ModelBase, UndoRedoRecorder.UserAction> models)
        {
            return (null != models && (models.Count > 0));
        }

        /// <summary>
        /// Deletes <see cref="ModelBase"/> object given by <see cref="XmlElement"/>
        /// from a corresponding collection of the workspace.
        /// </summary>
        /// <param name="modelData"><see cref="ModelBase"/> object given by <see cref="XmlElement"/></param>
        public void DeleteModel(XmlElement modelData)
        {
            //When there is a Redo operation, model is removed from
            //the workspace but the model is "not disposed" from memory.
            //Identified this when redo operation is performed on groups
            ModelBase model = GetModelForElement(modelData);

            if (model is NoteModel)
            {
                var note = model as NoteModel;
                RemoveNote(note);
                note.Dispose();
            }
            else if (model is AnnotationModel)
            {
                RemoveGroup(model);
            }
            else if (model is PresetModel)
            {
                this.RemovePreset(model as PresetModel);
            }
            else if (model is ConnectorModel)
            {
                var connector = model as ConnectorModel;
                connector.Delete();
            }
            else if (model is NodeModel)
            {
                RemoveAndDisposeNode(model as NodeModel);
            }
            else if(model == null)
            {
                return;
            }
            //some unknown type
            else
            {
                // If it gets here we obviously need to handle it.
                throw new InvalidOperationException(string.Format(
                    "Unhandled type: {0}", model.GetType()));
            }
        }

        /// <summary>
        /// Deletes <see cref="AnnotationModel"/> object from annotation collection of the workspace.
        /// </summary>
        /// <param name="model"><see cref="AnnotationModel"/> object to remove.</param>
        public void RemoveGroup(ModelBase model)
        {
            var annotation = model as AnnotationModel;
            RemoveAnnotation(annotation);
            annotation.Dispose();
        }

        /// <summary>
        /// Updates <see cref="ModelBase"/> object with given xml data
        /// </summary>
        /// <param name="modelData">Xml data to update model</param>
        public void ReloadModel(XmlElement modelData)
        {
            ModelBase model = GetModelForElement(modelData);
            if(model != null)
            {
                model.Deserialize(modelData, SaveContext.Undo);
            }
        }

        /// <summary>
        /// Creates <see cref="ModelBase"/> object by given xml data and
        /// adds it to corresponding collection of the workspace.
        /// </summary>
        /// <param name="modelData">Xml data to create model</param>
        public void CreateModel(XmlElement modelData)
        {
            var helper = new XmlElementHelper(modelData);
            string typeName = helper.ReadString("type", String.Empty);
            if (string.IsNullOrEmpty(typeName))
            {
                // If there wasn't a "type" attribute, then we fall-back onto
                // the name of the XmlElement itself, which is usually the type
                // name.
                typeName = modelData.Name;
                if (string.IsNullOrEmpty(typeName))
                {
                    string guid = helper.ReadString("guid");
                    throw new InvalidOperationException(
                        string.Format("No type information: {0}", guid));
                }
            }

            if (typeName.Contains("ConnectorModel"))
            {
                var connector = NodeGraph.LoadConnectorFromXml(modelData,
                    Nodes.ToDictionary(node => node.GUID));

                // It is possible that in some cases connector can't be created,
                // for example, connector connects to a custom node instance
                // whose input ports have been changed, so connector can't find
                // its end port owner.
                if (connector == null)
                {
                    var guidAttribute = modelData.Attributes["guid"];
                    if (guidAttribute == null)
                    {
                        throw new InvalidOperationException("'guid' field missing from recorded model");
                    }
                }
                else
                {
                    OnConnectorAdded(connector); // Update view-model and view.
                }
            }
            else if (typeName.Contains("NoteModel"))
            {
                var noteModel = NodeGraph.LoadNoteFromXml(modelData);
                AddNote(noteModel);

                //check whether this note belongs to a group
                foreach (var annotation in Annotations)
                {
                    //this note "was" in a group
                    if (annotation.DeletedModelBases.Any(m => m.GUID == noteModel.GUID))
                    {
                        annotation.AddToSelectedModels(noteModel);
                    }
                }
            }
            else if (typeName.Contains("AnnotationModel"))
            {
                var selectedNodes = this.Nodes == null ? null : this.Nodes.Where(s => s.IsSelected);
                var selectedNotes = this.Notes == null ? null : this.Notes.Where(s => s.IsSelected);

                var annotationModel = new AnnotationModel(selectedNodes, selectedNotes);
                annotationModel.ModelBaseRequested += annotationModel_GetModelBase;
                annotationModel.Disposed += (_) => annotationModel.ModelBaseRequested -= annotationModel_GetModelBase;
                annotationModel.Deserialize(modelData, SaveContext.Undo);
                AddNewAnnotation(annotationModel);
            }

            else if (typeName.Contains("PresetModel"))
            {
                var preset = new PresetModel(this.Nodes);
                preset.Deserialize(modelData, SaveContext.Undo);
                presets.Add(preset);
                //we raise this property change here so that this event bubbles up through
                //the model and to the DynamoViewModel so that presets show in the UI menu if our undo/redo
                //created the first preset
                RaisePropertyChanged("EnablePresetOptions");

            }
            else // Other node types.
            {
                NodeModel nodeModel = NodeFactory.CreateNodeFromXml(modelData, SaveContext.Undo, ElementResolver);

                AddAndRegisterNode(nodeModel);

                //check whether this node belongs to a group
                foreach (var annotation in Annotations)
                {
                    //this node "was" in a group
                    if (annotation.DeletedModelBases.Any(m => m.GUID == nodeModel.GUID))
                    {
                        annotation.AddToSelectedModels(nodeModel);
                    }
                }
            }
        }

        /// <summary>
        /// Gets model by GUID which is contained in given Xml data.
        /// </summary>
        /// <param name="modelData">Xml data to find model.</param>
        /// <returns>Found <see cref="ModelBase"/> object.</returns>
        public ModelBase GetModelForElement(XmlElement modelData)
        {
            // TODO(Ben): This may or may not be true, but I guess we should be
            // using "System.Type" (given the "type" information in "modelData"),
            // and determine the matching category (e.g. is this a Node, or a
            // Connector?) instead of checking in each and every collections we
            // have in the workspace.
            //
            // System.Type type = System.Type.GetType(helper.ReadString("type"));
            // if (typeof(Dynamo.Models.NodeModel).IsAssignableFrom(type))
            //     return Nodes.First((x) => (x.GUID == modelGuid));

            var helper = new XmlElementHelper(modelData);
            Guid modelGuid = helper.ReadGuid("guid");

            ModelBase foundModel = GetModelInternal(modelGuid);
            if (null != foundModel)
                return foundModel;
            
            //if we could not find a matching model
            this.Log(string.Format("Please Report: Unhandled model type: {0}, could not find a matching model with given id", helper.ReadString("type", modelData.Name)), Logging.WarningLevel.Error);
            return null;
        }

        /// <summary>
        /// Returns model by GUID
        /// </summary>
        /// <param name="modelGuid">Identifier of the requested model.</param>
        /// <returns>Found <see cref="ModelBase"/> object.</returns>
        public ModelBase GetModelInternal(Guid modelGuid)
        {
            ModelBase foundModel = (Connectors.FirstOrDefault(c => c.GUID == modelGuid)
                ?? Nodes.FirstOrDefault(node => node.GUID == modelGuid) as ModelBase)
                ?? (Notes.FirstOrDefault(note => note.GUID == modelGuid)
                ?? Annotations.FirstOrDefault(annotation => annotation.GUID == modelGuid) as ModelBase
                ?? Presets.FirstOrDefault(preset => preset.GUID == modelGuid) as ModelBase);

            return foundModel;
        }

        /// <summary>
        /// Gets model list by their GUIDs
        /// </summary>
        /// <param name="modelGuids">Identifiers of the requested models.</param>
        /// <returns>All found <see cref="ModelBase"/> objects.</returns>
        private IEnumerable<ModelBase> GetModelsInternal(IEnumerable<Guid> modelGuids)
        {
            var foundModels = new List<ModelBase>();

            foreach (var modelGuid in modelGuids)
            {
                var foundModel = GetModelInternal(modelGuid);
                if (foundModel != null)
                    foundModels.Add(foundModel);
            }

            return foundModels;
        }
    }
}

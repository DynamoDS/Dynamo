﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Properties;
using Dynamo.Selection;
using Dynamo.Utilities;
using ProtoCore.AST;
using ProtoCore.Namespace;

using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Models
{
    public abstract class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient, ILogSource, IDisposable, IWorkspaceModel
    {

        public const double ZOOM_MAXIMUM = 4.0;
        public const double ZOOM_MINIMUM = 0.01;

        #region private/internal members

        /// <summary>
        ///     The offset of the elements in the current paste operation
        /// </summary>
        private int currentPasteOffset = 0;
        internal int CurrentPasteOffset
        {
            get { return currentPasteOffset; }
        }

        /// <summary>
        ///     The step to offset elements between subsequent paste operations
        /// </summary>
        internal static readonly int PASTE_OFFSET_STEP = 10;

        /// <summary>
        ///     The maximum paste offset before reset
        /// </summary>
        internal static readonly int PASTE_OFFSET_MAX = 60;

        private string fileName;
        private string name;
        private double height = 100;
        private double width = 100;
        private double x;
        private double y;
        private double zoom = 1.0;
        private DateTime lastSaved;
        private string author = "None provided";
        private string description;
        private bool hasUnsavedChanges;
        private readonly List<NodeModel> nodes;
        private readonly List<NoteModel> notes;
        private readonly List<AnnotationModel> annotations;
        private readonly List<PresetModel> presets;
        private readonly UndoRedoRecorder undoRecorder;
        private Guid guid;

        #endregion

        #region events

        /// <summary>
        ///     Function that can be used to repsond to a saved workspace.
        /// </summary>
        /// <param name="model"></param>
        public delegate void WorkspaceSavedEvent(WorkspaceModel model);

        /// <summary>
        ///     Event that is fired when a workspace requests that a Node or Note model is
        ///     centered.
        /// </summary>

        public event NodeEventHandler RequestNodeCentered;

        /// <summary>
        ///     Requests that a Node or Note model should be centered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestNodeCentered(object sender, ModelEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }


        /// <summary>
        ///     Function that can be used to respond to a changed workspace Zoom amount.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ZoomEventHandler(object sender, EventArgs e);
        
        /// <summary>
        ///     Event that is fired every time the zoom factor of a workspace changes.
        /// </summary>
        public event ZoomEventHandler ZoomChanged;

        /// <summary>
        /// Used during open and workspace changes to set the zoom of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnZoomChanged(object sender, ZoomEventArgs e)
        {
            if (ZoomChanged != null)
            {
                //Debug.WriteLine(string.Format("Setting zoom to {0}", e.Zoom));
                ZoomChanged(this, e); 
            }
        }

        /// <summary>
        ///     Function that can be used to respond to a "point event"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PointEventHandler(object sender, EventArgs e);

        /// <summary>
        ///     Event that is fired every time the position offset of a workspace changes.
        /// </summary>
        public event PointEventHandler CurrentOffsetChanged;

        /// <summary>
        ///     Used during open and workspace changes to set the location of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
            {
                Debug.WriteLine("Setting current offset to {0}", e.Point);
                CurrentOffsetChanged(this, e);
            }
        }

        /// <summary>
        ///     Event that is fired when the workspace is saved.
        /// </summary>
        public event Action WorkspaceSaved;
        protected virtual void OnWorkspaceSaved()
        {
            LastSaved = DateTime.Now;
            HasUnsavedChanges = false;

            if (WorkspaceSaved != null)
                WorkspaceSaved();
        }

        /// <summary>
        ///     Event that is fired when a node is added to the workspace.
        /// </summary>
        public event Action<NodeModel> NodeAdded;
        protected virtual void OnNodeAdded(NodeModel node)
        {
            var handler = NodeAdded;
            if (handler != null) handler(node);
        }

        /// <summary>
        ///     Event that is fired when a node is removed from the workspace.
        /// </summary>
        public event Action<NodeModel> NodeRemoved;
        protected virtual void OnNodeRemoved(NodeModel node)
        {
            var handler = NodeRemoved;
            if (handler != null) handler(node);
        }

        /// <summary>
        ///     Event that is fired when nodes are cleared from the workspace.
        /// </summary>
        public event Action NodesCleared;
        protected virtual void OnNodesCleared()
        {
            var handler = NodesCleared;
            if (handler != null) handler();
        }

        /// <summary>
        ///     Event that is fired when a note is added to the workspace.
        /// </summary>
        public event Action<NoteModel> NoteAdded;
        protected virtual void OnNoteAdded(NoteModel note)
        {
            var handler = NoteAdded;
            if (handler != null) handler(note);
        }

        /// <summary>
        ///     Event that is fired when a note is removed from the workspace.
        /// </summary>
        public event Action<NoteModel> NoteRemoved;
        protected virtual void OnNoteRemoved(NoteModel note)
        {
            var handler = NoteRemoved;
            if (handler != null) handler(note);
        }

        /// <summary>
        ///     Event that is fired when notes are cleared from the workspace.
        /// </summary>
        public event Action NotesCleared;
        protected virtual void OnNotesCleared()
        {
            var handler = NotesCleared;
            if (handler != null) handler();
        }

        /// <summary>
        ///     Event that is fired when an annotation is added to the workspace.
        /// </summary>
        public event Action<AnnotationModel> AnnotationAdded;
        protected virtual void OnAnnotationAdded(AnnotationModel annotation)
        {
            var handler = AnnotationAdded;
            if (handler != null) handler(annotation);
        }

        /// <summary>
        ///     Event that is fired when an annotation is removed from the workspace.
        /// </summary>
        public event Action<AnnotationModel> AnnotationRemoved;
        protected virtual void OnAnnotationRemoved(AnnotationModel annotation)
        {
            var handler = AnnotationRemoved;
            if (handler != null) handler(annotation);
        }

        /// <summary>
        ///     Event that is fired when annotations are cleared from the workspace.
        /// </summary>
        public event Action AnnotationsCleared;
        protected virtual void OnAnnotationsCleared()
        {
            var handler = AnnotationsCleared;
            if (handler != null) handler();
        }

        /// <summary>
        ///     Event that is fired when a connector is added to the workspace.
        /// </summary>
        public event Action<ConnectorModel> ConnectorAdded;
        protected virtual void OnConnectorAdded(ConnectorModel obj)
        {
            RegisterConnector(obj);
            var handler = ConnectorAdded;
            if (handler != null) handler(obj);
        }

        private void RegisterConnector(ConnectorModel connector)
        {
            connector.Deleted += () => OnConnectorDeleted(connector);
        }

        /// <summary>
        ///     Event that is fired when a connector is deleted from a workspace.
        /// </summary>
        public event Action<ConnectorModel> ConnectorDeleted;
        protected virtual void OnConnectorDeleted(ConnectorModel obj)
        {
            var handler = ConnectorDeleted;
            if (handler != null) handler(obj);
        }

        /// <summary>
        ///     Event that is fired when this workspace is disposed of.
        /// </summary>
        public event Action Disposed;


        /// <summary>
        /// Event that is fired during the saving of the workspace.
        /// 
        /// Add additional XmlNode objects to the XmlDocument provided,
        /// in order to save data to the file.
        /// </summary>
        public event Action<XmlDocument> Saving;
        protected virtual void OnSaving(XmlDocument obj)
        {
            var handler = Saving;
            if (handler != null) handler(obj);
        }

        #endregion

        #region public properties

        /// <summary>
        ///     A NodeFactory used by this workspace to create Nodes.
        /// </summary>
        //TODO(Steve): This should only live on DynamoModel, not here. It's currently used to instantiate NodeModels during UndoRedo. -- MAGN-5713
        public readonly NodeFactory NodeFactory;

        /// <summary>
        ///     A set of input parameter states, this can be used to set the graph to a serialized state.       
        /// </summary>
        public IEnumerable<PresetModel> Presets { get { return presets;} }

        /// <summary>
        ///     The date of the last save.
        /// </summary>
        public DateTime LastSaved
        {
            get { return lastSaved; }
            set
            {
                lastSaved = value;
                RaisePropertyChanged("LastSaved");
            }
        }

        /// <summary>
        ///     An author of the workspace
        /// </summary>
        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                RaisePropertyChanged("Author");
            }
        }

        /// <summary>
        ///     A description of the workspace
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        /// <summary>
        ///     Are there unsaved changes in the workspace?
        /// </summary>
        public bool HasUnsavedChanges
        {
            get { return hasUnsavedChanges; }
            set
            {
                hasUnsavedChanges = value;
                RaisePropertyChanged("HasUnsavedChanges");
            }
        }

        /// <summary>
        ///     All of the nodes currently in the workspace.
        /// </summary>
        public IEnumerable<NodeModel> Nodes { 
            get 
            {
                IEnumerable<NodeModel> nodesClone;
                lock (nodes)
                {
                    nodesClone = nodes.ToList();                
                }

                return nodesClone;
            } 
        }


        private void AddNode(NodeModel node)
        {
            lock (nodes)
            {
                nodes.Add(node);
            }            

            OnNodeAdded(node);
        }

        private void ClearNodes()
        {
            lock (nodes)
            {
                nodes.Clear();
            }

            OnNodesCleared();
        }


        /// <summary>
        ///     All of the connectors currently in the workspace.
        /// </summary>
        public IEnumerable<ConnectorModel> Connectors
        {
            get
            {
                return nodes.SelectMany(
                    node => node.OutPorts.SelectMany(port => port.Connectors))
                    .Distinct();
            }
        }

        /// <summary>
        ///     All of the notes currently in the workspace.
        /// </summary>
        public IEnumerable<NoteModel> Notes
        {
            get
            {
                IEnumerable<NoteModel> notesClone;
                lock (notes)
                {
                    notesClone = notes.ToList();
                }

                return notesClone;
            }
        }

        public IEnumerable<AnnotationModel> Annotations
        {
            get
            {
                IEnumerable<AnnotationModel> annotationsClone;
                lock (annotations)
                {
                    annotationsClone = annotations.ToList();
                }

                return annotationsClone;
            }
        }

        /// <summary>
        ///     Path to the file this workspace is associated with. If null or empty, this workspace has never been saved.
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        /// <summary>
        ///     The name of this workspace.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        ///     Get or set the X position of the workspace.
        /// </summary>
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        ///     Get or set the Y position of the workspace
        /// </summary>
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                RaisePropertyChanged("Y");
            }
        }

        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        /// <summary>
        ///     Get the height of the workspace's bounds.
        /// </summary>
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        ///     Get the width of the workspace's bounds.
        /// </summary>
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        ///     Get the bounds of the workspace.
        /// </summary>
        public Rect2D Rect
        {
            get { return new Rect2D(x, y, width, height); }
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

        //TODO(Steve): This probably isn't needed inside of WorkspaceModel -- MAGN-5714
        internal Version WorkspaceVersion { get; set; }

        public double CenterX
        {
            get { return 0; }
            set { }
        }

        public double CenterY
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Get the current UndoRedoRecorder that is associated with the current 
        /// WorkspaceModel. Note that external parties should not have the needs 
        /// to access the recorder directly, so this property is exposed just as 
        /// a "temporary solution". Before using this property, consider using 
        /// WorkspaceModel.RecordModelsForUndo method which allows for multiple 
        /// modifications in a single action group.
        /// </summary>
        public UndoRedoRecorder UndoRecorder
        {
            get { return undoRecorder; }
        }

        public ElementResolver ElementResolver { get; protected set; }
        /// <summary>
        /// A unique identifier for the workspace.
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
        }

        #endregion

        #region constructors

        protected WorkspaceModel(
            IEnumerable<NodeModel> e, 
            IEnumerable<NoteModel> n,
            IEnumerable<AnnotationModel> a,
            WorkspaceInfo info, 
            NodeFactory factory,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver)
        {
            guid = Guid.NewGuid();

            nodes = new List<NodeModel>(e);
            notes = new List<NoteModel>(n);

            annotations = new List<AnnotationModel>(a);         

            // Set workspace info from WorkspaceInfo object
            Name = info.Name;
            Description = info.Description;
            X = info.X;
            Y = info.Y;
            FileName = info.FileName;
            Zoom = info.Zoom;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);

            NodeFactory = factory;

            this.presets = new List<PresetModel>(presets);
            ElementResolver = resolver;

            foreach (var node in nodes)
                RegisterNode(node);

            foreach (var connector in Connectors)
                RegisterConnector(connector);

            SetModelEventOnAnnotation();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            foreach (var node in Nodes)
                DisposeNode(node);

            foreach (var connector in Connectors)
                OnConnectorDeleted(connector);

            var handler = Disposed;
            if (handler != null) 
                handler();
            Disposed = null;
        }

     
        #endregion

        #region public methods

        /// <summary>
        ///     Clears this workspace of nodes, notes, and connectors.
        /// </summary>
        public virtual void Clear()
        {
            Log(Resources.ClearingWorkSpace);

            DynamoSelection.Instance.ClearSelection();

            // The deletion of connectors in the following step will trigger a
            // lot of graph executions. As connectors are deleted, nodes will 
            // have invalid inputs, so these executions are meaningless and may
            // cause invalid GC. See comments in MAGN-7229.
            foreach (NodeModel node in Nodes)
                node.RaisesModificationEvents = false;

            foreach (NodeModel el in Nodes)
            {
                el.Dispose();

                foreach (PortModel p in el.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].Delete();
                }
                foreach (PortModel port in el.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].Delete();
                }
            }

            ClearNodes();
            ClearNotes();
            ClearAnnotations();
            presets.Clear();

            ClearUndoRecorder();
            ResetWorkspace();

            X = 0.0;
            Y = 0.0;
            Zoom = 1.0;
        }

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="newPath">The path to save to</param>
        /// <param name="core"></param>
        /// <param name="isBackup">Indicates whether saved workspace is backup or not. If it's not backup,
        /// we should add it to recent files. Otherwise leave it.</param>
        public virtual bool SaveAs(string newPath, ProtoCore.RuntimeCore runtimeCore, bool isBackup = false)
        {
            if (String.IsNullOrEmpty(newPath)) return false;

            Log(String.Format(Resources.SavingInProgress, newPath));
            try
            {
                if (SaveInternal(newPath, runtimeCore) && !isBackup)
                    OnWorkspaceSaved();
            }
            catch (Exception ex)
            {
                //Log(ex);
                Log(ex.Message);
                Log(ex.StackTrace);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Adds a node to this workspace.
        /// </summary>
        public void AddAndRegisterNode(NodeModel node, bool centered = false)
        {
            if (nodes.Contains(node))
                return;

            RegisterNode(node);

            if (centered)
            {
                var args = new ModelEventArgs(node, true);
                OnRequestNodeCentered(this, args);
            }

            AddNode(node);

            HasUnsavedChanges = true;

            RequestRun();
        }

        private void RegisterNode(NodeModel node)
        {
            node.Modified += NodeModified;
            node.ConnectorAdded += OnConnectorAdded;
        }

        protected virtual void RequestRun()
        {
            
        }

        /// <summary>
        ///     Indicates that the AST for a node in this workspace requires recompilation
        /// </summary>
        protected virtual void NodeModified(NodeModel node)
        {
            HasUnsavedChanges = true;
        }

        /// <summary>
        /// Removes a node from this workspace. 
        /// This method does not raise a NodesModified event. (LC notes this is clearly not true)
        /// </summary>
        /// <param name="model"></param>
        public void RemoveNode(NodeModel model)
        {
            lock (nodes)
            {
                if (!nodes.Remove(model)) return;                
            }

            OnNodeRemoved(model);
            DisposeNode(model);
        }

        protected void DisposeNode(NodeModel model)
        {
            model.ConnectorAdded -= OnConnectorAdded;
            model.Modified -= NodeModified;
            model.Dispose();
        }

        private void AddNote(NoteModel note)
        {
            lock (notes)
            {
                notes.Add(note);
            }

            OnNoteAdded(note);
        }

        public void AddNote(NoteModel note, bool centered)
        {
            if (centered)
            {
                var args = new ModelEventArgs(note, true);
                OnRequestNodeCentered(this, args);
            }
            AddNote(note);
        }

        public NoteModel AddNote(bool centerNote, double xPos, double yPos, string text, Guid id)
        {
            var noteModel = new NoteModel(xPos, yPos, string.IsNullOrEmpty(text) ? Resources.NewNoteString : text, id);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            AddNote(noteModel, centerNote);
            return noteModel;
        }

        public void ClearNotes()
        {
            lock (notes)
            {
                notes.Clear();
            }

            OnNotesCleared();
        }

        private void RemoveNote(NoteModel note)
        {
            lock (notes)
            {
                if (!notes.Remove(note)) return;
            }
            OnNoteRemoved(note);
        }

        private void AddNewAnnotation(AnnotationModel annotation)
        {
            lock (annotations)
            {
                annotations.Add(annotation);
            }

            OnAnnotationAdded(annotation);
        }

        public void ClearAnnotations()
        {
            lock (annotations)
            {
                annotations.Clear();
            }

            OnAnnotationsCleared();
        }

        private void RemoveAnnotation(AnnotationModel annotation)
        {
            lock (annotations)
            {
                if (!annotations.Remove(annotation)) return;
            }
            OnAnnotationRemoved(annotation);
        }

        public void AddAnnotation(AnnotationModel annotationModel)
        {
            annotationModel.ModelBaseRequested += annotationModel_GetModelBase;
            annotationModel.Disposed += (_) => annotationModel.ModelBaseRequested -= annotationModel_GetModelBase;
            AddNewAnnotation(annotationModel);
        }

        public AnnotationModel AddAnnotation(string text, Guid id)
        {
            var selectedNodes = this.Nodes == null ? null:this.Nodes.Where(s => s.IsSelected);
            var selectedNotes = this.Notes == null ? null: this.Notes.Where(s => s.IsSelected);
           
            if (!CheckIfModelExistsInSameGroup(selectedNodes, selectedNotes))
            {
                var annotationModel = new AnnotationModel(selectedNodes, selectedNotes)
                {
                    GUID = id,
                    AnnotationText = text                   
                };
                annotationModel.ModelBaseRequested += annotationModel_GetModelBase;
                annotationModel.Disposed += (_) => annotationModel.ModelBaseRequested -= annotationModel_GetModelBase;
                AddNewAnnotation(annotationModel);
                HasUnsavedChanges = true;
                return annotationModel;
            }
            return null;
        }

        /// <summary>
        //this sets the event on Annotation. This event return the model from the workspace.
        //When a model is ungrouped from a group, that model will be deleted from that group.
        //So, when UNDO execution, cannot get that model from that group, it has to get from the workspace.
        //The below method will set the event on every annotation model, that will return the specific model
        //from workspace.
        /// </summary>
        /// <param name="model">The model.</param>
        private void SetModelEventOnAnnotation()
        {           
            foreach (var model in this.Annotations)
            {
                model.ModelBaseRequested += annotationModel_GetModelBase;
                model.Disposed += (_) => model.ModelBaseRequested -= annotationModel_GetModelBase;
            }
        }

        /// <summary>
        /// Get the model from Workspace
        /// </summary>
        /// <param name="modelGuid">The model unique identifier.</param>
        /// <returns></returns>
        private ModelBase annotationModel_GetModelBase(Guid modelGuid)
        {
            ModelBase model = null;
            model = this.Nodes.FirstOrDefault(x => x.GUID == modelGuid);
            if (model == null) //Check if GUID is a Note instead.
            {
                model = this.Notes.FirstOrDefault(x => x.GUID == modelGuid);
            }

            return model;
        }

        /// <summary>
        /// Checks if model exists in same group.
        /// </summary>
        /// <param name="selectNodes">The select nodes.</param>
        /// <param name="selectNotes">The select notes.</param>
        /// <returns>true if the models are already in the same group</returns>
        private bool CheckIfModelExistsInSameGroup(IEnumerable<NodeModel> selectNodes, IEnumerable<NoteModel> selectNotes)
        {
            var selectedModels = selectNodes.Concat(selectNotes.Cast<ModelBase>()).ToList();
            bool nodesInSameGroup = false;
            foreach (var group in this.Annotations)
            {
                var groupModels = group.SelectedModels;
                nodesInSameGroup = !selectedModels.Except(groupModels).Any();
                if (nodesInSameGroup)
                    break;
            }

            return nodesInSameGroup;
        }

        /// <summary>
        /// Save assuming that the Filepath attribute is set.
        /// </summary>
        public virtual bool Save(ProtoCore.RuntimeCore runtimeCore)
        {
            return SaveAs(FileName, runtimeCore);
        }

        internal void ResetWorkspace()
        {
            ElementResolver = new ElementResolver();
            ResetWorkspaceCore();
        }

        /// <summary>
        /// Derived workspace classes can choose to override 
        /// this method to perform clean-up specific to them.
        /// </summary>
        /// 
        protected virtual void ResetWorkspaceCore()
        {
        }
        
        public IEnumerable<NodeModel> GetHangingNodes()
        {
            return
                Nodes.Where(
                    node =>
                        node.OutPortData.Any() && node.OutPorts.Any(port => !port.Connectors.Any()));
        }

        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        /// <summary>
        ///     Increment the current paste offset to prevent overlapping pasted elements
        /// </summary>
        internal void IncrementPasteOffset()
        {
            this.currentPasteOffset = (this.currentPasteOffset + PASTE_OFFSET_STEP) % PASTE_OFFSET_MAX;
        }
        
        #endregion

        #region Presets
        /// <summary>
        ///  this method creates a new preset state from a set of NodeModels and adds this new state to this presets collection
        /// </summary>
        /// <param name="name">the name of preset state</param>
        /// <param name="description">a description of what the state does</param>
        /// <param name="currentSelection">a set of NodeModels that are to be serialized in this state</param>
        /// <param name="id">a GUID id for the state, if not supplied, a new GUID will be generated, cannot be a duplicate</param>
        private PresetModel AddPresetCore(string name, string description, IEnumerable<NodeModel> currentSelection)
        {
            if (currentSelection == null || currentSelection.Count() < 1)
            {
                throw new ArgumentException("currentSelection is empty or null");
            }
            var inputs = currentSelection;

            var newstate = new PresetModel(name, description, inputs);
            if (Presets.Any(x => x.GUID == newstate.GUID))
            {
                throw new ArgumentException("duplicate id in collection");
            }

            presets.Add(newstate);
            return newstate;
        }

        public void RemovePreset(PresetModel state)
        {
            if (Presets.Contains(state))
            {
                presets.Remove(state);
            }
        }

        internal void ApplyPreset(PresetModel state)
        {
            if (state == null)
            {
                Log("Attempted to apply a PresetState that was null");
                return;
            }
            //start an undoBeginGroup
            using (var undoGroup = this.undoRecorder.BeginActionGroup())
            {
               //reload each node, and record each each modification in the undogroup
                foreach (var node in state.Nodes)
                {
                    //check that node still exists in this workspace, 
                    //otherwise bail on this node, check by GUID instead of nodemodel
                    if (nodes.Select(x=>x.GUID).Contains(node.GUID))
                    {
                        var originalpos = node.Position;
                        var serializedNode = state.SerializedNodes.ToList().Find(x => Guid.Parse(x.GetAttribute("guid")) == node.GUID);
                        //overwrite the xy coords of the serialized node with the current position, so the node is not moved
                        serializedNode.SetAttribute("x", originalpos.X.ToString(CultureInfo.InvariantCulture));
                        serializedNode.SetAttribute("y", originalpos.Y.ToString(CultureInfo.InvariantCulture));

                        this.undoRecorder.RecordModificationForUndo(node);
                        this.ReloadModel(serializedNode);
                    }
                }
                //select all the modified nodes in the UI
                DynamoSelection.Instance.ClearSelection();
                foreach(var node in state.Nodes)
                {
                    DynamoSelection.Instance.Selection.Add(node);
                }
            }
        }
        internal PresetModel AddPreset(string name, string description, IEnumerable<Guid> IDSToSave)
        {
                //lookup the nodes by their ID, can also check that we find all of them....
                var nodesFromIDs = this.Nodes.Where(node => IDSToSave.Contains(node.GUID));
                //access the presetsCollection and add a new state based on the current selection
                var newpreset = this.AddPresetCore(name, description, nodesFromIDs);
                HasUnsavedChanges = true;
                return newpreset;
        }

        public void ImportPresets(IEnumerable<PresetModel> presetCollection)
        {
            presets.AddRange(presetCollection);
        }

        #endregion

        #region private/internal methods

        private bool SaveInternal(string targetFilePath, ProtoCore.RuntimeCore runtimeCore)
        {
            // Create the xml document to write to.
            var document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", null, null);
            document.AppendChild(document.CreateElement("Workspace"));

            Utils.SetDocumentXmlPath(document, targetFilePath);

            if (!PopulateXmlDocument(document))
                return false;

            SerializeSessionData(document, runtimeCore);

            try
            {
                Utils.SetDocumentXmlPath(document, string.Empty);
                document.Save(targetFilePath);
            }
            catch (IOException)
            {
                return false;
            }

            FileName = targetFilePath;
            return true;
        }

        private void SerializeElementResolver(XmlDocument xmlDoc)
        {
            Debug.Assert(xmlDoc != null);

            var root = xmlDoc.DocumentElement;

            var mapElement = xmlDoc.CreateElement("NamespaceResolutionMap");

            foreach (var element in ElementResolver.ResolutionMap)
            {
                var resolverElement = xmlDoc.CreateElement("ClassMap");
                
                resolverElement.SetAttribute("partialName", element.Key);
                resolverElement.SetAttribute("resolvedName", element.Value.Key);
                resolverElement.SetAttribute("assemblyName", element.Value.Value);

                mapElement.AppendChild(resolverElement);
            }
            root.AppendChild(mapElement);
        }

        protected virtual bool PopulateXmlDocument(XmlDocument xmlDoc)
        {
            try
            {
                var root = xmlDoc.DocumentElement;
                root.SetAttribute("Version", WorkspaceVersion.ToString());
                root.SetAttribute("X", X.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Y", Y.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("zoom", Zoom.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Name", Name);
                root.SetAttribute("Description", Description);

                SerializeElementResolver(xmlDoc);

                var elementList = xmlDoc.CreateElement("Elements");
                //write the root element
                root.AppendChild(elementList);

                foreach (var dynEl in Nodes.Select(el => el.Serialize(xmlDoc, SaveContext.File)))
                    elementList.AppendChild(dynEl);

                //write only the output connectors
                var connectorList = xmlDoc.CreateElement("Connectors");
                //write the root element
                root.AppendChild(connectorList);

                foreach (var el in Nodes)
                {
                    foreach (var port in el.OutPorts)
                    {
                        foreach (
                            var c in
                                port.Connectors.Where(c => c.Start != null && c.End != null))
                        {
                            var connector = xmlDoc.CreateElement(c.GetType().ToString());
                            connectorList.AppendChild(connector);
                            connector.SetAttribute("start", c.Start.Owner.GUID.ToString());
                            connector.SetAttribute("start_index", c.Start.Index.ToString());
                            connector.SetAttribute("end", c.End.Owner.GUID.ToString());
                            connector.SetAttribute("end_index", c.End.Index.ToString());

                            if (c.End.PortType == PortType.Input)
                                connector.SetAttribute("portType", "0");
                        }
                    }
                }

                //save the notes
                var noteList = xmlDoc.CreateElement("Notes"); //write the root element
                root.AppendChild(noteList);
                foreach (var n in Notes)
                {
                    var note = n.Serialize(xmlDoc, SaveContext.File);
                    noteList.AppendChild(note);                         
                }

                //save the annotation
                var annotationList = xmlDoc.CreateElement("Annotations");
                root.AppendChild(annotationList);
                foreach (var n in annotations)
                {
                    var annotation = n.Serialize(xmlDoc, SaveContext.File);
                    annotationList.AppendChild(annotation);                   
                }

                //save the presets into the dyn file as a seperate element on the root
                var presetsElement = xmlDoc.CreateElement("Presets");
                root.AppendChild(presetsElement);
                foreach (var preset in Presets)
                {
                    var presetState = preset.Serialize(xmlDoc, SaveContext.File);
                    presetsElement.AppendChild(presetState);
                }

                OnSaving(xmlDoc);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                return false;
            }
        }

        // TODO(Ben): Documentation to come before pull request.
        // TODO(Steve): This probably only belongs on HomeWorkspaceModel. -- MAGN-5715
        protected virtual void SerializeSessionData(XmlDocument document, ProtoCore.RuntimeCore runtimeCore)
        {
            if (document.DocumentElement == null)
            {
                const string message = "Workspace should have been saved before this";
                throw new InvalidOperationException(message);
            }

            try
            {
                if (runtimeCore == null) // No execution yet as of this point.
                    return;

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids =
                    Nodes.Where(
                        n => n is DSFunction || n is DSVarArgFunction || n is CodeBlockNodeModel)
                        .Select(n => n.GUID);

                var nodeTraceDataList = runtimeCore.RuntimeData.GetTraceDataForNodes(nodeGuids, runtimeCore.DSExecutable);

                if (nodeTraceDataList.Any())
                    Utils.SaveTraceDataToXmlDocument(document, nodeTraceDataList);
            }
            catch (Exception exception)
            {
                // We'd prefer file saving process to not crash Dynamo,
                // otherwise user will lose the last hope in retaining data.
                Log(exception.Message);
                Log(exception.StackTrace);
            }
        }

        internal void SendModelEvent(Guid modelGuid, string eventName)
        {
            var retrievedModel = GetModelInternal(modelGuid);
            if (retrievedModel == null)
                throw new InvalidOperationException("SendModelEvent: Model not found");

            var handled = false;
            var nodeModel = retrievedModel as NodeModel;
            if (nodeModel != null)
            {
                using (new UndoRedoRecorder.ModelModificationUndoHelper(undoRecorder, nodeModel))
                {
                    handled = nodeModel.HandleModelEvent(eventName, undoRecorder);
                }
            }
            else
            {
                // Perform generic undo recording for models other than node.
                RecordModelForModification(retrievedModel, UndoRecorder);
                handled = retrievedModel.HandleModelEvent(eventName, undoRecorder);
            }

            if (!handled) // Method call was not handled by any derived class.
            {
                string type = retrievedModel.GetType().FullName;
                string message = string.Format(
                    "ModelBase.HandleModelEvent call not handled.\n\n" +
                    "Model type: {0}\n" +
                    "Model GUID: {1}\n" +
                    "Event name: {2}",
                    type, modelGuid, eventName);

                // All 'HandleModelEvent' calls must be handled by one of 
                // the ModelBase derived classes that the 'SendModelEvent'
                // is intended for.
                throw new InvalidOperationException(message);
            }

            HasUnsavedChanges = true;
        }

        internal void UpdateModelValue(IEnumerable<Guid> modelGuids, string propertyName, string value)
        {
            if (modelGuids == null || (!modelGuids.Any()))
                throw new ArgumentNullException("modelGuids");

            var retrievedModels = GetModelsInternal(modelGuids);
            if (!retrievedModels.Any())
                throw new InvalidOperationException("UpdateModelValue: Model not found");

            var updateValueParams = new UpdateValueParams(propertyName, value, ElementResolver);
            using (new UndoRedoRecorder.ModelModificationUndoHelper(undoRecorder, retrievedModels))
            {
                foreach (var retrievedModel in retrievedModels)
                {
                    retrievedModel.UpdateValue(updateValueParams);
                }
            }

            HasUnsavedChanges = true;
        }

        internal void ConvertNodesToCodeInternal(EngineController engineController)
        {
            var selectedNodes = DynamoSelection.Instance
                                               .Selection
                                               .OfType<NodeModel>()
                                               .Where(n => n.IsConvertible);
            if (!selectedNodes.Any())
                return;

            var cliques = NodeToCodeUtils.GetCliques(selectedNodes).Where(c => !(c.Count == 1 && c.First() is CodeBlockNodeModel));
            var codeBlockNodes = new List<CodeBlockNodeModel>();

            //UndoRedo Action Group----------------------------------------------
            NodeToCodeUndoHelper undoHelper = new NodeToCodeUndoHelper();

            // using (UndoRecorder.BeginActionGroup())
            {
                foreach (var nodeList in cliques)
                {
                    //Create two dictionarys to store the details of the external connections that have to 
                    //be recreated after the conversion
                    var externalInputConnections = new Dictionary<ConnectorModel, string>();
                    var externalOutputConnections = new Dictionary<ConnectorModel, string>();

                    //Also collect the average X and Y co-ordinates of the different nodes
                    int nodeCount = nodeList.Count;

                    var nodeToCodeResult = engineController.ConvertNodesToCode(this.nodes, nodeList);

                    #region Step I. Delete all nodes and their connections

                    double totalX = 0, totalY = 0;

                    foreach (var node in nodeList)
                    {
                        #region Step I.A. Delete the connections for the node

                        foreach (var connector in node.AllConnectors.ToList())
                        {
                            if (!IsInternalNodeToCodeConnection(nodeList, connector))
                            {
                                //If the connector is an external connector, the save its details
                                //for recreation later
                                var startNode = connector.Start.Owner;
                                int index = startNode.OutPorts.IndexOf(connector.Start);
                                //We use the varibleName as the connection between the port of the old Node
                                //to the port of the new node.
                                var variableName = startNode.GetAstIdentifierForOutputIndex(index).Value;

                                //Store the data in the corresponding dictionary
                                if (startNode == node)
                                {
                                    if (nodeToCodeResult.OutputMap.ContainsKey(variableName))
                                        variableName = nodeToCodeResult.OutputMap[variableName];
                                    externalOutputConnections.Add(connector, variableName);
                                }
                                else
                                {
                                    if (nodeToCodeResult.InputMap.ContainsKey(variableName))
                                        variableName = nodeToCodeResult.InputMap[variableName];
                                    externalInputConnections.Add(connector, variableName);
                                }
                            }

                            //Delete the connector
                            undoHelper.RecordDeletion(connector);
                            connector.Delete();
                        }
                        #endregion

                        #region Step I.B. Delete the node
                        totalX += node.X;
                        totalY += node.Y;
                        undoHelper.RecordDeletion(node);
                        RemoveNode(node);
                        #endregion
                    }
                    #endregion

                    #region Step II. Create the new code block node
                    var outputVariables = externalOutputConnections.Values;
                    var newResult = NodeToCodeUtils.ConstantPropagationForTemp(nodeToCodeResult, outputVariables);

                    // Rewrite the AST using the shortest unique name in case of namespace conflicts
                    NodeToCodeUtils.ReplaceWithShortestQualifiedName(
                        engineController.LibraryServices.LibraryManagementCore.ClassTable, newResult.AstNodes, ElementResolver);
                    var codegen = new ProtoCore.CodeGenDS(newResult.AstNodes);
                    var code = codegen.GenerateCode();

                    var codeBlockNode = new CodeBlockNodeModel(
                        code,
                        System.Guid.NewGuid(), 
                        totalX / nodeCount,
                        totalY / nodeCount, engineController.LibraryServices, ElementResolver);
                    undoHelper.RecordCreation(codeBlockNode);
                   
                    AddAndRegisterNode(codeBlockNode, false);
                    codeBlockNodes.Add(codeBlockNode);
                    #endregion

                    #region Step III. Recreate the necessary connections
                    var newInputConnectors = ReConnectInputConnections(externalInputConnections, codeBlockNode);
                    foreach (var connector in newInputConnectors)
                    {
                        undoHelper.RecordCreation(connector);
                    }

                    var newOutputConnectors = ReConnectOutputConnections(externalOutputConnections, codeBlockNode);
                    foreach (var connector in newOutputConnectors)
                    {
                        undoHelper.RecordCreation(connector);
                    }
                    #endregion
                }
            }

            undoHelper.ApplyActions(UndoRecorder);

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(codeBlockNodes);

            RequestRun();
        }

        #endregion
        
        #region Undo/Redo Supporting Methods

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
        public static void RecordModelForModification(ModelBase model, UndoRedoRecorder recorder)
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

        public static void RecordModelsForUndo(Dictionary<ModelBase, UndoRedoRecorder.UserAction> models, UndoRedoRecorder recorder)
        {
            if (null == recorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            using (recorder.BeginActionGroup())
            {
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
                        RemovePreset(model as PresetModel);
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

                        RemoveNode(node);
                    }
                    else if (model is ConnectorModel)
                    {
                        undoRecorder.RecordDeletionForUndo(model);
                    }
                }

                RequestRun();

            } // Conclude the deletion.
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

        #endregion

        #region IUndoRedoRecorderClient Members

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
                RemovePreset(model as PresetModel);
            }
            else if (model is ConnectorModel)
            {
                var connector = model as ConnectorModel;
                connector.Delete();
            }
            else if (model is NodeModel)
            {
                RemoveNode(model as NodeModel);
            }
            else
            {
                // If it gets here we obviously need to handle it.
                throw new InvalidOperationException(string.Format(
                    "Unhandled type: {0}", model.GetType()));
            }
        }

        public void RemoveGroup(ModelBase model)
        {
            var annotation = model as AnnotationModel;
            RemoveAnnotation(annotation);
            annotation.Dispose();
        }

        public void ReloadModel(XmlElement modelData)
        {
            ModelBase model = GetModelForElement(modelData);
            model.Deserialize(modelData, SaveContext.Undo);
        }

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

            /*
            if (typeName.Equals("Dynamo.Nodes.DSFunction") ||
                typeName.Equals("Dynamo.Nodes.DSVarArgFunction"))
            {
                // For DSFunction and DSVarArgFunction node types, the type name
                // is actually embedded within "name" attribute (for an example,
                // "UV.ByCoordinates@double,double").
                // 
                typeName = modelData.Attributes["name"].Value;
            }
            */

            if (typeName.StartsWith("Dynamo.Models.ConnectorModel"))
            {
                var connector = NodeGraph.LoadConnectorFromXml(modelData,
                    Nodes.ToDictionary(node => node.GUID));

                OnConnectorAdded(connector); // Update view-model and view.
            }
            else if (typeName.StartsWith("Dynamo.Models.NoteModel"))
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
            else if (typeName.StartsWith("Dynamo.Models.AnnotationModel"))
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
                    if (annotation.DeletedModelBases.Any(m=>m.GUID == nodeModel.GUID))
                    {
                        annotation.AddToSelectedModels(nodeModel);
                    }
                }
            }
        }

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

            throw new ArgumentException(
                string.Format("Unhandled model type: {0}", helper.ReadString("type", modelData.Name)));
        }

        public ModelBase GetModelInternal(Guid modelGuid)
        {
            ModelBase foundModel = (Connectors.FirstOrDefault(c => c.GUID == modelGuid)
                ?? Nodes.FirstOrDefault(node => node.GUID == modelGuid) as ModelBase)
                ?? (Notes.FirstOrDefault(note => note.GUID == modelGuid)
                ?? Annotations.FirstOrDefault(annotation => annotation.GUID == modelGuid) as ModelBase
                ?? Presets.FirstOrDefault(preset => preset.GUID == modelGuid) as ModelBase);

            return foundModel;
        }

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

        #endregion

        #region Node To Code Reconnection

        /// <summary>
        /// Checks whether the given connection is inside the node to code set or outside it. 
        /// This determines if it should be redrawn(if it is external) or if it should be 
        /// deleted (if it is internal)
        /// </summary>
        private static bool IsInternalNodeToCodeConnection(IEnumerable<NodeModel> nodes, ConnectorModel connector)
        {
            return nodes.Contains(connector.Start.Owner) && nodes.Contains(connector.End.Owner);
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalOutputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="cbn">The new Node To Code created Code Block Node</param>
        private List<ConnectorModel> ReConnectOutputConnections(Dictionary<ConnectorModel, string> externalOutputConnections, CodeBlockNodeModel cbn)
        {
            List<ConnectorModel> newConnectors = new List<ConnectorModel>();
            foreach (var kvp in externalOutputConnections)
            {
                var connector = kvp.Key;
                var variableName = kvp.Value;

                //Get the start and end idex for the ports for the connection
                var portModel = cbn.OutPorts.FirstOrDefault(
                    port => cbn.GetRawAstIdentifierForOutputIndex(port.Index).Value.Equals(variableName));

                if (portModel == null)
                    continue;

                //Make the new connection and then record and add it
                var newConnector = ConnectorModel.Make(
                    cbn,
                    connector.End.Owner,
                    portModel.Index,
                    connector.End.Index);

                newConnectors.Add(newConnector);
            }
            return newConnectors;
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalInputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="cbn">The new Node To Code created Code Block Node</param>
        private List<ConnectorModel> ReConnectInputConnections(
            Dictionary<ConnectorModel, string> externalInputConnections, CodeBlockNodeModel cbn)
        {
            List<ConnectorModel> newConnectors = new List<ConnectorModel>();

            foreach (var kvp in externalInputConnections)
            {
                var connector = kvp.Key;
                var variableName = kvp.Value;

                var endPortIndex = CodeBlockNodeModel.GetInportIndex(cbn, variableName);
                if (endPortIndex < 0)
                    continue;

                if (Connectors.Any(c => c.End == cbn.InPorts[endPortIndex]))
                    continue;

                var newConnector = ConnectorModel.Make(
                    connector.Start.Owner,
                    cbn,
                    connector.Start.Index,
                    endPortIndex);

                newConnectors.Add(newConnector);
            }

            return newConnectors;
        }

        #endregion

        internal string GetStringRepOfWorkspace()
        {
            // Create the xml document to write to.
            var document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", null, null);
            document.AppendChild(document.CreateElement("Workspace"));

            //This is only used for computing relative offsets, it's not actually created
            string virtualFileName = Path.Combine(Path.GetTempPath(), "DynamoTemp.dyn");
            Utils.SetDocumentXmlPath(document, virtualFileName);

            if (!PopulateXmlDocument(document))
                return String.Empty;

            //Now unset the temp file name again
            Utils.SetDocumentXmlPath(document, null);
            
            return document.OuterXml;
        }
        
        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }

        #endregion       
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public abstract class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient, ILogSource, IDisposable
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
        private bool hasUnsavedChanges;
        private readonly ObservableCollection<NodeModel> nodes;
        private readonly ObservableCollection<NoteModel> notes;
        private readonly ObservableCollection<AnnotationModel> annotations;
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

        #endregion

        #region public properties

        /// <summary>
        ///     A NodeFactory used by this workspace to create Nodes.
        /// </summary>
        //TODO(Steve): This should only live on DynamoModel, not here. It's currently used to instantiate NodeModels during UndoRedo. -- MAGN-5713
        public readonly NodeFactory NodeFactory;

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
        ///     A description of the workspace
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
        /// 
        ///     TODO(Peter): This should be an IEnumerable of nodes to prevent modification from the outside - MAGN-6580
        /// </summary>
        public ObservableCollection<NodeModel> Nodes { get { return nodes; } }

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
        /// 
        ///     TODO(Peter): This should be an IEnumerable of notes to prevent modification from the outside - MAGN-6580
        /// </summary>
        public ObservableCollection<NoteModel> Notes { get { return notes; } }

        public ObservableCollection<AnnotationModel> Annotations { get { return annotations; } }

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
            NodeFactory factory)
        {
            guid = Guid.NewGuid();

            nodes = new ObservableCollection<NodeModel>(e);
            notes = new ObservableCollection<NoteModel>(n);

            annotations = new ObservableCollection<AnnotationModel>(a);         

            // Set workspace info from WorkspaceInfo object
            Name = info.Name;
            X = info.X;
            Y = info.Y;
            FileName = info.FileName;
            Zoom = info.Zoom;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);

            NodeFactory = factory;

            // Update ElementResolver from nodeGraph.Nodes (where node is CBN)
            ElementResolver = new ElementResolver();
            foreach (var node in nodes)
            {
                RegisterNode(node);

                var cbn = node as CodeBlockNodeModel;
                if (cbn != null && cbn.ElementResolver != null)
                {
                    ElementResolver.CopyResolutionMap(cbn.ElementResolver);
                }
            }

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

            Nodes.Clear();
            Notes.Clear();
            Annotations.Clear();

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
        public virtual bool SaveAs(string newPath, ProtoCore.RuntimeCore runtimeCore)
        {
            if (String.IsNullOrEmpty(newPath)) return false;

            Log(String.Format(Resources.SavingInProgress, newPath));
            try
            {
                if (SaveInternal(newPath, runtimeCore))
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
        public void AddNode(NodeModel node, bool centered = false)
        {
            if (nodes.Contains(node))
                return;

            RegisterNode(node);

            if (centered)
            {
                var args = new ModelEventArgs(node, true);
                OnRequestNodeCentered(this, args);
            }

            nodes.Add(node);
            OnNodeAdded(node);
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

        }

        /// <summary>
        /// Removes a node from this workspace. 
        /// This method does not raise a NodesModified event.
        /// </summary>
        /// <param name="model"></param>
        public void RemoveNode(NodeModel model)
        {
            if (!nodes.Remove(model)) return;

            DisposeNode(model);
        }

        protected void DisposeNode(NodeModel model)
        {
            model.ConnectorAdded -= OnConnectorAdded;
            model.Modified -= NodeModified;
            OnNodeRemoved(model);
            model.Dispose();
        }

        public void AddNote(NoteModel note, bool centered)
        {
            if (centered)
            {
                var args = new ModelEventArgs(note, true);
                OnRequestNodeCentered(this, args);
            }
            Notes.Add(note);
        }

        public NoteModel AddNote(bool centerNote, double xPos, double yPos, string text, Guid id)
        {
            var noteModel = new NoteModel(xPos, yPos, string.IsNullOrEmpty(text) ? Resources.NewNoteString : text, id);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            AddNote(noteModel, centerNote);
            return noteModel;
        }

        public void AddAnnotation(AnnotationModel annotationModel)
        {
            annotationModel.ModelBaseRequested += annotationModel_GetModelBase;
            annotationModel.Disposed += (_) => annotationModel.ModelBaseRequested -= annotationModel_GetModelBase;
            Annotations.Add(annotationModel);
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
                Annotations.Add(annotationModel);
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
                        Nodes.Remove(node);
                        #endregion
                    }
                    #endregion

                    #region Step II. Create the new code block node
                    var outputVariables = externalOutputConnections.Values;
                    var newResult = NodeToCodeUtils.ConstantPropagationForTemp(nodeToCodeResult, outputVariables);
                    NodeToCodeUtils.ReplaceWithUnqualifiedName(engineController.LibraryServices.LibraryManagementCore, newResult.AstNodes);
                    var codegen = new ProtoCore.CodeGenDS(newResult.AstNodes);
                    var code = codegen.GenerateCode();

                    var codeBlockNode = new CodeBlockNodeModel(
                        code,
                        System.Guid.NewGuid(), 
                        totalX / nodeCount,
                        totalY / nodeCount, engineController.LibraryServices);
                    undoHelper.RecordCreation(codeBlockNode);
                    Nodes.Add(codeBlockNode);
                    this.RegisterNode(codeBlockNode);

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
                undoRecorder.Undo();
        }

        internal void Redo()
        {
            if (null != undoRecorder)
                undoRecorder.Redo();
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
                        Notes.Remove(model as NoteModel);
                    }
                    else if (model is AnnotationModel)
                    {
                        undoRecorder.RecordDeletionForUndo(model);
                        Annotations.Remove(model as AnnotationModel);
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
                Notes.Remove(note);                
                note.Dispose();
            }
            else if (model is AnnotationModel)
            {
                RemoveGroup(model);
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
            Annotations.Remove(annotation);
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
                Notes.Add(noteModel);

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
                Annotations.Add(annotationModel);
            }
            else // Other node types.
            {
                NodeModel nodeModel = NodeFactory.CreateNodeFromXml(modelData, SaveContext.Undo);
                Nodes.Add(nodeModel);
                RegisterNode(nodeModel);
                
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
                ??  Nodes.FirstOrDefault(node => node.GUID == modelGuid) as ModelBase)
                ?? (Notes.FirstOrDefault(note => note.GUID == modelGuid) 
                ??  Annotations.FirstOrDefault(annotation => annotation.GUID == modelGuid) as ModelBase) ;

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

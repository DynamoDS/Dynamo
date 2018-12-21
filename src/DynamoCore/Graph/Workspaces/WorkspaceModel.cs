using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Events;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Properties;
using Dynamo.Selection;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.Namespace;
using Utils = Dynamo.Graph.Nodes.Utilities;
using Dynamo.Engine;
using Dynamo.Scheduler;
using Newtonsoft.Json.Linq;
using Dynamo.Library;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Non view-specific container for additional view information required to
    /// fully construct a WorkspaceModel from JSON
    /// </summary>
    public class ExtraWorkspaceViewInfo
    {
        public object Camera;
        public IEnumerable<ExtraNodeViewInfo> NodeViews;
        public IEnumerable<ExtraNoteViewInfo> Notes;
        public IEnumerable<ExtraAnnotationViewInfo> Annotations;
        public double X;
        public double Y;
        public double Zoom;        
    }

    /// <summary>
    /// Non view-specific container for additional node view information 
    /// required to fully construct a WorkspaceModel from JSON
    /// </summary>
    public class ExtraNodeViewInfo
    {
        public string Id;
        public string Name;
        public double X;
        public double Y;
        public bool ShowGeometry;
        public bool Excluded;
        public bool IsSetAsInput;
        public bool IsSetAsOutput;
    }

    /// <summary>
    /// Non view-specific container for additional note view information 
    /// required to fully construct a WorkspaceModel from JSON
    /// </summary>
    public class ExtraNoteViewInfo
    {
        public string Id;
        public double X;
        public double Y;
        public string Text;

        // TODO, QNTM-1099: Figure out if this is necessary
        // public int ZIndex;
    }

    /// <summary>
    /// Non view-specific container for additional annotation view information 
    /// required to fully construct a WorkspaceModel from JSON
    /// </summary>
    public class ExtraAnnotationViewInfo
    {
        public string Title;
        public IEnumerable<string> Nodes;
        public double FontSize;
        public string Background;
        public string Id;

        // TODO, Determine if these are required
        public double Left;
        public double Top;
        public double Width;
        public double Height;
        public double InitialTop;
        public double InitialHeight;
        public double TextBlockHeight;

        private bool tolerantDoubleCompare(double a, double b)
        {
            return Math.Abs(a - b) < .0001;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExtraAnnotationViewInfo;
            return other != null &&
                this.Id == other.Id &&
                this.Title == other.Title &&
                this.Nodes.SequenceEqual(other.Nodes) &&
                this.FontSize == other.FontSize &&
                this.Background == other.Background;

                //TODO try to get rid of these if possible
                //needs investigation if we are okay letting them get 
                //calculated at runtime. currently checking them will fail as we do
                //not deserialize them.

                //tolerantDoubleCompare(this.Left, other.Left) &&
                //tolerantDoubleCompare(this.Top, other.Top) &&
                //tolerantDoubleCompare(this.InitialTop, other.InitialTop);
                //this.Width == other.Width &&
                //this.Height == other.Height &&
                //this.TextBlockHeight == other.TextBlockHeight;
        }
    }

    /// <summary>
    /// Represents base class for all kind of workspaces which contains general data
    /// such as Name, collections of nodes, notes, annotations, etc.
    /// </summary>
    public abstract partial class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient, ILogSource, IDisposable, IWorkspaceModel
    {
        #region private/internal members

        /// <summary>
        ///     The offset of the elements in the current paste operation
        /// </summary>
        private int currentPasteOffset = 0;
        internal int CurrentPasteOffset
        {
            get
            {
                return currentPasteOffset + PasteOffsetStep;
            }
        }

        internal bool ScaleFactorChanged = false;

        /// <summary>
        ///     The step to offset elements between subsequent paste operations
        /// </summary>
        internal static readonly int PasteOffsetStep = 10;

        /// <summary>
        ///     The maximum paste offset before reset
        /// </summary>
        internal static readonly int PasteOffsetMax = 60;

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
        private bool isReadOnly;
        private readonly List<NodeModel> nodes;
        private readonly List<NoteModel> notes;
        private readonly List<AnnotationModel> annotations;
        internal readonly List<PresetModel> presets;
        private readonly UndoRedoRecorder undoRecorder;
        private static List<ModelBase> savedModels = null;
        private double scaleFactor = 1.0;
        private bool hasNodeInSyncWithDefinition;
        protected Guid guid;
        private HashSet<Guid> dependencies = new HashSet<Guid>();

        /// <summary>
        /// This is set to true after a workspace is added.
        /// This is set to false, if the workspace is cleared or disposed.
        /// </summary>
        private bool workspaceLoaded;

        /// <summary>
        /// sets the name property of the model based on filename,backup state and model type.
        /// </summary>
        /// <param name="filePath">Full filepath to file to save.</param>
        /// <param name="isBackup">Indicates if this save represents a backup save.</param>
        internal void setNameBasedOnFileName(string filePath, bool isBackup)
        {
            string fileName = string.Empty;
            try
            {
                fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath);
                if (extension == ".dyn" || extension == ".dyf")
                {
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                }
            }
            catch (ArgumentException)
            {
            }
            // Don't change name property if backup save or this is a customnode
            if (fileName != string.Empty && isBackup == false && this is HomeWorkspaceModel)
            {
                this.Name = fileName;
            }
        }

        #endregion

        #region events

        /// <summary>
        ///     Function that can be used to respond on a saved workspace.
        /// </summary>
        /// <param name="model">The <see cref="WorkspaceModel"/> object which has been saved.</param>
        public delegate void WorkspaceSavedEvent(WorkspaceModel model);

        /// <summary>
        ///     Event that is fired when a workspace requests that a Node or Note model is
        ///     centered.
        /// </summary>
        public event NodeEventHandler RequestNodeCentered;

        /// <summary>
        ///     Requests that a Node or Note model should be centered.
        /// </summary>
        /// <param name="sender">The workspace object where the event handler is attached.</param>
        /// <param name="e">The event data containing sufficient information about node.</param>
        internal virtual void OnRequestNodeCentered(object sender, ModelEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }
        /// <summary>
        ///     Function that can be used to respond to a "point event"
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        public delegate void PointEventHandler(object sender, EventArgs e);

        /// <summary>
        ///     Event that is fired every time the position offset of a workspace changes.
        /// </summary>
        public event PointEventHandler CurrentOffsetChanged;

        /// <summary>
        ///     Used during open and workspace changes to set the location of the workspace
        /// </summary>
        /// <param name="sender">The object which triggers the event</param>
        /// <param name="e">The offset event data.</param>
        internal virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
            {
                Debug.WriteLine("Setting current offset to {0}", e.Point);
                CurrentOffsetChanged(this, e);
            }
        }

        /// <summary>
        /// Event that is fired when the workspace is saved.
        /// </summary>
        public event Action Saved;
        internal virtual void OnSaved()
        {
            LastSaved = DateTime.Now;
            HasUnsavedChanges = false;

            if (Saved != null)
                Saved();
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
            //Check if the workspace is loaded, i.e all the nodes are
            //added to the workspace. In that case, compute the Upstream cache for the
            //given node.
            if (workspaceLoaded)
            {
                obj.End.Owner.ComputeUpstreamOnDownstreamNodes();
            }
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
            //Check if the workspace is loaded, i.e all the nodes are
            //added to the workspace. In that case, compute the Upstream cache for the
            //given node.
            if (workspaceLoaded)
            {
                obj.End.Owner.ComputeUpstreamOnDownstreamNodes();
            }
        }

        /// <summary>
        /// Implement recording node modification for undo/redo.
        /// </summary>
        /// <param name="models">Collection of <see cref="ModelBase"/> objects to record.</param>
        public void RecordModelsForModification(IEnumerable<ModelBase> models)
        {
            RecordModelsForModification(models.ToList(), undoRecorder);
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

        /// <summary>
        /// This handler handles the workspaceModel's request to populate a JSON with view data.
        /// This is used to construct a full workspace for instrumentation.
        /// </summary>
        internal delegate string PopulateJSONWorkspaceHandler(JObject modelData);
        internal event PopulateJSONWorkspaceHandler PopulateJSONWorkspace;
        protected virtual void OnPopulateJSONWorkspace(JObject modelData)
        {
            var handler = PopulateJSONWorkspace;
            if (handler != null) handler(modelData);
        }

        private void OnSyncWithDefinitionStart(NodeModel nodeModel)
        {
            hasNodeInSyncWithDefinition = true;
        }

        private void OnSyncWithDefinitionEnd(NodeModel nodeModel)
        {
            hasNodeInSyncWithDefinition = false;
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
        /// gathers the direct workspace dependencies of this workspace.
        /// </summary>
        /// <returns> a list of workspace IDs in GUID form</returns>
        public HashSet<Guid> Dependencies
        {
            get {
                dependencies.Clear();
                //if the workspace is a main workspace then find all functions and their dependencies
                if (this is HomeWorkspaceModel)
                {
                    foreach (var node in this.Nodes.OfType<Function>())
                    {
                        dependencies.Add(node.FunctionSignature);
                    }
                }
                //else the workspace is a customnode - and we can add the dependencies directly
                else
                {
                    var customNodeDirectDependencies = new HashSet<Guid>((this as CustomNodeWorkspaceModel).
                        CustomNodeDefinition.DirectDependencies.Select(x => x.FunctionId));
                    dependencies = customNodeDirectDependencies;
                }
                return dependencies;
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
            get 
            {
                if(!string.IsNullOrEmpty(this.FileName)) // if there is a filename
                {
                    if (!File.Exists(this.FileName)) // but the filename is invalid
                    {
                        this.fileName = string.Empty;
                        hasUnsavedChanges = true;
                    }
                }

                return hasUnsavedChanges;
            }
            set
            {
                hasUnsavedChanges = value;
                RaisePropertyChanged("HasUnsavedChanges");
            }
        }

        /// <summary>
        /// Returns if current workspace is readonly.
        /// </summary>
        public bool IsReadOnly
        {   
            //if the workspace contains xmlDummyNodes it's effectively a readonly graph.
            get { return isReadOnly || this.containsXmlDummyNodes(); }
            set
            {
                isReadOnly = value;
            }
        }

        /// <summary>
        ///     All of the nodes currently in the workspace.
        /// </summary>
        public IEnumerable<NodeModel> Nodes
        {
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

        public IEnumerable<NodeModel> CurrentSelection
        {
            get
            {
                return DynamoSelection.Instance.Selection.OfType<NodeModel>();
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
                    .Distinct().ToList();
            }
        }

        /// <summary>
        ///     Returns the notes <see cref="NoteModel"/> collection.
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

        /// <summary>
        ///     Returns all of the annotations currently present in the workspace.
        /// </summary>
        [JsonIgnore]
        [Obsolete("This property will be removed from the model, please use Annotations on the WorkspaceViewModel in the DynamoCoreWpf assembly")]
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
        ///     Returns or set the X position of the workspace.
        /// </summary>
        [Obsolete("This property will be removed from the model, please use the X property on the WorkspaceViewModel in the DynamoCoreWpf assembly.")]
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
        ///     Returns or set the Y position of the workspace
        /// </summary>
        [Obsolete("This property will be removed from the model, please use the Y property on the WorkspaceViewModel in the DynamoCoreWpf assembly.")]
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                RaisePropertyChanged("Y");
            }
        }
        
        /// <summary>
        ///     Get or set the zoom value of the workspace.
        /// </summary>
        [JsonIgnore]
        [Obsolete("This property will be removed from the model, please use the Zoom property on the WorkspaceViewModel in the DynamoCoreWpf assembly.")]
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
        ///     Returns the height of the workspace's bounds.
        /// </summary>
        [JsonIgnore]
        [Obsolete("This property will be removed from the model, please use the Zoom property on the WorkspaceViewModel in the DynamoCoreWpf assembly.")]
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
        ///     Returns the width of the workspace's bounds.
        /// </summary>
        [JsonIgnore]
        [Obsolete("This property will be removed from the model, please use the Zoom property on the WorkspaceViewModel in the DynamoCoreWpf assembly.")]
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
        ///     Returns the bounds of the workspace.
        /// </summary>
        public Rect2D Rect
        {
            get { return new Rect2D(x, y, width, height); }
        }

        //TODO(Steve): This probably isn't needed inside of WorkspaceModel -- MAGN-5714
        internal Version WorkspaceVersion { get; set; }

        /// <summary>
        /// Implements <see cref="ILocatable.CenterX"/> property.
        /// </summary>
        // TODO: make a better implementation of this property
        public double CenterX
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Implements <see cref="ILocatable.CenterY"/> property.
        /// </summary>
        // TODO: make a better implementation of this property
        public double CenterY
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Returns <see cref="ElementResolver"/>. This property resolves partial class name to fully resolved name.
        /// </summary>
        public ElementResolver ElementResolver { get; protected set; }

        /// <summary>
        /// A unique identifier for the workspace.
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
            internal set { guid = value; }
        }

        /// <summary>
        /// The geometry scale factor specific to the workspace obtained from user input
        /// when selecting the scale of the model with which he/she is working. 
        /// This is used by ProtoGeometry to scale geometric values appropriately before passing them to ASM.
        /// This property is set either when reading the setting from a DYN file or when the setting is updated from the UI.
        /// </summary>
        public double ScaleFactor
        {
            get { return scaleFactor; }
            internal set
            {
                scaleFactor = value;
                WorkspaceEvents.OnWorkspaceSettingsChanged(scaleFactor);
            }
        }

        #endregion

        #region constructors

        protected WorkspaceModel(
            IEnumerable<NodeModel> nodes,
            IEnumerable<NoteModel> notes,
            IEnumerable<AnnotationModel> annotations,
            WorkspaceInfo info,
            NodeFactory factory,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver)
        {
            guid = Guid.NewGuid();

            this.nodes = new List<NodeModel>(nodes);
            this.notes = new List<NoteModel>(notes);

            this.annotations = new List<AnnotationModel>(annotations);

            // Set workspace info from WorkspaceInfo object
            Name = info.Name;
            Description = info.Description;
            X = info.X;
            Y = info.Y;
            FileName = info.FileName;
            Zoom = info.Zoom; 


            HasUnsavedChanges = false;
            IsReadOnly = DynamoUtilities.PathHelper.IsReadOnlyPath(fileName);
            LastSaved = DateTime.Now;

            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);

            NodeFactory = factory;

            this.presets = new List<PresetModel>(presets);
            ElementResolver = resolver;

            foreach (var node in this.nodes)
                RegisterNode(node);

            foreach (var connector in Connectors)
                RegisterConnector(connector);

            SetModelEventOnAnnotation();
            WorkspaceEvents.WorkspaceAdded += computeUpstreamNodesWhenWorkspaceAdded;
        }

        /// <summary>
        /// Computes the upstream nodes when workspace is added. when a workspace is added (assuming that
        /// all the nodes and its connectors were added successfully) compute the upstream cache for all
        /// the frozen nodes.
        /// </summary>
        /// <param name="args">The <see cref="WorkspacesModificationEventArgs"/> instance containing the event data.</param>
        private void computeUpstreamNodesWhenWorkspaceAdded(WorkspacesModificationEventArgs args)
        {
            if (args.Id == this.Guid)
            {
                this.workspaceLoaded = true;
                this.ComputeUpstreamCacheForEntireGraph();

                // If the entire graph is frozen then set silenceModification
                // to false on the workspace. This is required
                // becuase if all the nodes are frozen, then updategraphsyncdata task
                // has nothing to process and the graph will not run. setting silenceModification here
                // ensure graph runs immediately when any of the node is set to unfreeze.
                lock (nodes)
                {
                    if (nodes != null && nodes.Any() && nodes.All(z => z.IsFrozen))
                    {
                        var firstnode = nodes.First();
                        firstnode.OnRequestSilenceModifiedEvents(false);
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            this.workspaceLoaded = false;
            foreach (var node in Nodes)
            {
                DisposeNode(node);
            }

            foreach (var connector in Connectors)
            {
                OnConnectorDeleted(connector);
            }

            WorkspaceEvents.WorkspaceAdded -= computeUpstreamNodesWhenWorkspaceAdded;

            var handler = Disposed;
            if (handler != null) handler();
            Disposed = null;

            WorkspaceEvents.WorkspaceAdded -= computeUpstreamNodesWhenWorkspaceAdded;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Returns appropriate name of workspace for sharing.
        /// </summary>
        public virtual string GetSharedName()
        {
            return this.Name;
        }

        /// <summary>
        ///     Clears this workspace of nodes, notes, and connectors.
        /// </summary>
        public virtual void Clear()
        {
            workspaceLoaded = false;
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
            ScaleFactor = 1.0;
            // Reset the workspace offset
            OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(X, Y)));
            workspaceLoaded = true;
        }

        /// <summary>
        /// Workspace's Save method serializes the Workspace to JSON and writes it to the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="isBackup">A flag indicating whether this save operation represents a backup. If it's not backup,
        /// we should add it to recent files. Otherwise leave it.</param>
        /// <param name="engine">An EngineController instance to be used to serialize node bindings.</param>
        /// <exception cref="ArgumentNullException">Thrown when the file path is null.</exception>
        public virtual void Save(string filePath, bool isBackup = false, EngineController engine = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            try
            {
                //set the name before serializing model.
                setNameBasedOnFileName(filePath, isBackup);

                // Stage 1: Serialize the workspace.
                var json = this.ToJson(engine);

                // Stage 2: Save
                File.WriteAllText(filePath, json);

                // Handle Workspace or CustomNodeWorkspace related non-serialization internal logic
                // Only for actual save, update file path and recent file list
                if (!isBackup)
                {
                    FileName = filePath;
                    OnSaved();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                throw (ex);
            }
        }

        /// <summary>
        ///     Adds a node to this workspace.
        /// </summary>
        /// <param name="node">The node which is being added to the workspace.</param>
        /// <param name="centered">Indicates if the node should be placed at the center of workspace.</param>
        internal void AddAndRegisterNode(NodeModel node, bool centered = false)
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

        protected virtual void RegisterNode(NodeModel node)
        {
            node.Modified += NodeModified;
            node.ConnectorAdded += OnConnectorAdded;
            node.UpdateASTCollection +=OnToggleNodeFreeze;

            var functionNode = node as Function;
            if (functionNode != null)
            {
                functionNode.Controller.SyncWithDefinitionStart += OnSyncWithDefinitionStart;
                functionNode.Controller.SyncWithDefinitionEnd += OnSyncWithDefinitionEnd;
            }
        }

        protected virtual void OnToggleNodeFreeze(NodeModel obj)
        {

        }

        internal virtual void RequestRun()
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
        /// <param name="model">The node which is being removed from the worksapce.</param>
        internal void RemoveAndDisposeNode(NodeModel model, bool dispose = true)
        {
            lock (nodes)
            {
                if (!nodes.Remove(model)) return;
            }

            OnNodeRemoved(model);

            if (dispose)
            {
                DisposeNode(model);
            }
        }

        protected virtual void DisposeNode(NodeModel node)
        {
            var functionNode = node as Function;
            if (functionNode != null)
            {
                functionNode.Controller.SyncWithDefinitionStart -= OnSyncWithDefinitionStart;
                functionNode.Controller.SyncWithDefinitionEnd -= OnSyncWithDefinitionEnd;
            }
            node.ConnectorAdded -= OnConnectorAdded;
            node.UpdateASTCollection -= OnToggleNodeFreeze;
            node.Modified -= NodeModified;
            node.Dispose();
        }

        private void AddNote(NoteModel note)
        {
            lock (notes)
            {
                notes.Add(note);
            }

            OnNoteAdded(note);
        }

        internal void AddNote(NoteModel note, bool centered)
        {
            if (centered)
            {
                var args = new ModelEventArgs(note, true);
                OnRequestNodeCentered(this, args);
            }
            AddNote(note);
        }

        internal NoteModel AddNote(bool centerNote, double xPos, double yPos, string text, Guid id)
        {
            var noteModel = new NoteModel(xPos, yPos, string.IsNullOrEmpty(text) ? Resources.NewNoteString : text, id);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            AddNote(noteModel, centerNote);
            return noteModel;
        }

        internal void ClearNotes()
        {
            lock (notes)
            {
                notes.Clear();
            }

            OnNotesCleared();
        }

        internal void RemoveNote(NoteModel note)
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

        internal void ClearAnnotations()
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

        internal void AddAnnotation(AnnotationModel annotationModel)
        {
            annotationModel.ModelBaseRequested += annotationModel_GetModelBase;
            annotationModel.Disposed += (_) => annotationModel.ModelBaseRequested -= annotationModel_GetModelBase;
            AddNewAnnotation(annotationModel);
        }

        internal AnnotationModel AddAnnotation(string text, Guid id)
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
        /// this sets the event on Annotation. This event return the model from the workspace.
        /// When a model is ungrouped from a group, that model will be deleted from that group.
        /// So, when UNDO execution, cannot get that model from that group, it has to get from the workspace.
        /// The below method will set the event on every annotation model, that will return the specific model
        /// from workspace.
        /// </summary>
        private void SetModelEventOnAnnotation()
        {
            foreach (var model in this.Annotations)
            {
                model.ModelBaseRequested += annotationModel_GetModelBase;
                model.Disposed += (_) => model.ModelBaseRequested -= annotationModel_GetModelBase;
            }
        }

        /// <summary>
        /// Returns the model from Workspace
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
                var groupModels = group.Nodes;
                nodesInSameGroup = !selectedModels.Except(groupModels).Any();
                if (nodesInSameGroup)
                    break;
            }

            return nodesInSameGroup;
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

        internal IEnumerable<NodeModel> GetHangingNodes()
        {
            return
                Nodes.Where(
                    node =>
                        node.OutPorts.Any() && node.OutPorts.Any(port => !port.Connectors.Any()));
        }

        /// <summary>
        /// Returns the nodes in the graph that have no inputs or have none of their inputs filled
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<NodeModel> GetSourceNodes()
        {
            return
                Nodes.Where(
                    node =>
                       !node.InPorts.Any()||node.InPorts.All(port => !port.Connectors.Any()));
        }

        /// <summary>
        /// This method ensures that all upstream node caches are correct by calling
        /// ComputeUpstreamOnDownstream on all source nodes in the graph,
        /// this is done in such a way that each node is only computed once.
        /// </summary>
        private void ComputeUpstreamCacheForEntireGraph()
        {
            var sortedNodes = AstBuilder.TopologicalSort(this.nodes);

            foreach (var sortedNode in sortedNodes)
            {
                //call ComputeUpstreamCache to propogate the upstream Cache down to all nodes
                sortedNode.ComputeUpstreamCache();
            }
        }

        /// <summary>
        ///     Called when workspace position is changed. This method notifyies all the listeners when a workspace is changed.
        /// </summary>
        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        /// <summary>
        ///     Increment the current paste offset to prevent overlapping pasted elements
        /// </summary>
        internal void IncrementPasteOffset()
        {
            this.currentPasteOffset = (this.currentPasteOffset + PasteOffsetStep) % PasteOffsetMax;
        }

        #endregion

        #region private/internal methods
        /// <summary>
        /// Returns true if the graph currently contains dummy node which point to XML content.
        /// These nodes cannot be serialized to json correctly.
        /// </summary>
        /// <returns></returns>
        internal bool containsXmlDummyNodes()
        {
            return this.Nodes.OfType<DummyNode>().Where(node => node.OriginalNodeContent is XmlElement).Count()> 0;
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
                root.SetAttribute("ScaleFactor", ScaleFactor.ToString(CultureInfo.InvariantCulture));
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

        internal void SendModelEvent(Guid modelGuid, string eventName, int value)
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
                    handled = nodeModel.HandleModelEvent(eventName, value, undoRecorder);
                }
            }
            else
            {
                // Perform generic undo recording for models other than node.
                RecordModelForModification(retrievedModel, UndoRecorder);
                handled = retrievedModel.HandleModelEvent(eventName, value, undoRecorder);
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

        #endregion

        internal string GetStringRepOfWorkspace()
        {
            try
            {
                //Send the data in JSON format
                //step 1: convert the model to json
                var json = this.ToJson(null);

                //step2 : parse it as JObject
                var jo = JObject.Parse(json);

                //step 3: raise the event to populate the view block
                if (PopulateJSONWorkspace != null)
                {
                    json = PopulateJSONWorkspace(jo);
                }

                return json;
            }
            catch (JsonReaderException ex)
            {
                JArray array = new JArray();
                array.Add(ex.Message);

                JObject jo = new JObject();
                jo["exception"] = array;

                return jo.ToString();
            }
            catch (Exception ex)
            {
                JArray array = new JArray();
                array.Add(ex.InnerException.ToString());

                JObject jo = new JObject();
                jo["exception"] = array;

                return jo.ToString();
            }

        }

        #region ILogSource implementation

        /// <summary>
        /// Triggers when something needs to be logged
        /// </summary>
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


        /// <summary>
        /// Load a WorkspaceModel from json. If the WorkspaceModel is a HomeWorkspaceModel
        /// it will be set as the current workspace.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="libraryServices"></param>
        /// <param name="engineController"></param>
        /// <param name="scheduler"></param>
        /// <param name="factory"></param>
        /// <param name="isTestMode"></param>
        /// <param name="verboseLogging"></param>
        /// <param name="manager"></param>
        public static WorkspaceModel FromJson(string json, LibraryServices libraryServices,
            EngineController engineController, DynamoScheduler scheduler, NodeFactory factory,
            bool isTestMode, bool verboseLogging, CustomNodeManager manager)
        {
            var logger = engineController != null ? engineController.AsLogger() : null;

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                Culture = CultureInfo.InvariantCulture,
                Converters = new List<JsonConverter>{
                        new ConnectorConverter(logger),
                        new WorkspaceReadConverter(engineController, scheduler, factory, isTestMode, verboseLogging),
                        new NodeReadConverter(manager, libraryServices, factory, isTestMode),
                        new TypedParameterConverter()
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var result = SerializationExtensions.ReplaceTypeDeclarations(json, true);
            var ws = JsonConvert.DeserializeObject<WorkspaceModel>(result, settings);

            return ws;
        }

        /// <summary>
        /// Updates a workspace model with extra view information. When loading a workspace from JSON,
        /// the data is split into two parts, model and view. This method sets the view information.
        /// </summary>
        /// <param name="workspaceViewInfo">The extra view information from the workspace to update the model with.</param>
        public void UpdateWithExtraWorkspaceViewInfo(ExtraWorkspaceViewInfo workspaceViewInfo)
        {
            if (workspaceViewInfo == null)
              return;

            X = workspaceViewInfo.X;
            Y = workspaceViewInfo.Y;
            Zoom = workspaceViewInfo.Zoom; 

            OnCurrentOffsetChanged(
                this,
                new PointEventArgs(new Point2D(X, Y)));

            // This function loads standard nodes
            LoadNodes(workspaceViewInfo.NodeViews);

            // This function loads notes from the Notes array in the JSON format
            // NOTE: This is here to support early JSON graphs
            // IMPORTANT: All notes must be loaded before annotations are loaded to
            //            ensure that any contained notes are contained properly
            LoadLegacyNotes(workspaceViewInfo.Notes);

            // This function loads notes from the Annotations array in the JSON format
            // that have an empty nodes collection
            // IMPORTANT: All notes must be loaded before annotations are loaded to
            //            ensure that any contained notes are contained properly
            LoadNotesFromAnnotations(workspaceViewInfo.Annotations);

            // This function loads annotations from the Annotations array in the JSON format
            // that have a non-empty nodes collection
            LoadAnnotations(workspaceViewInfo.Annotations);

            // TODO, QNTM-1099: These items are not in the extra view info
            // Name = info.Name;
            // Description = info.Description;
            // FileName = info.FileName;
        }

        private void LoadNodes(IEnumerable<ExtraNodeViewInfo> nodeViews)
        {
            if (nodeViews == null)
              return;

            foreach (ExtraNodeViewInfo nodeViewInfo in nodeViews)
            {
                var guidValue = IdToGuidConverter(nodeViewInfo.Id);
                var nodeModel = Nodes.FirstOrDefault(node => node.GUID == guidValue);
                if (nodeModel != null)
                {
                    nodeModel.X = nodeViewInfo.X;
                    nodeModel.Y = nodeViewInfo.Y;
                    nodeModel.IsFrozen = nodeViewInfo.Excluded;
                    nodeModel.IsSetAsInput = nodeViewInfo.IsSetAsInput;
                    nodeModel.IsSetAsOutput = nodeViewInfo.IsSetAsOutput;

                    // NOTE: The name needs to be set using UpdateValue to cause the view to update
                    nodeModel.UpdateValue(new UpdateValueParams("Name", nodeViewInfo.Name));

                    // NOTE: These parameters are not directly accessible due to undo/redo considerations
                    //       which should not be used during deserialization (see "ArgumentLacing" for details)
                    nodeModel.UpdateValue(new UpdateValueParams("IsVisible", nodeViewInfo.ShowGeometry.ToString()));
                }
                else
                {   
                    this.Log(string.Format("This graph has a nodeview with id:{0} and name:{1}, but does not contain a matching nodeModel", 
                        guidValue.ToString(),nodeViewInfo.Name)
                        , WarningLevel.Moderate);
                }
            }
        }

        private void LoadLegacyNotes(IEnumerable<ExtraNoteViewInfo> noteViews)
        {
            if (noteViews == null)
              return;

            foreach (ExtraNoteViewInfo noteViewInfo in noteViews)
            {
                var guidValue = IdToGuidConverter(noteViewInfo.Id);

                // TODO, QNTM-1099: Figure out if ZIndex needs to be set here as well
                var noteModel = new NoteModel(noteViewInfo.X, noteViewInfo.Y, noteViewInfo.Text, guidValue);

                //if this note does not exist, add it to the workspace.
                var matchingNote = this.Notes.FirstOrDefault(x => x.GUID == noteModel.GUID);
                if (matchingNote == null)
                {
                    this.AddNote(noteModel);
                }
            }
        }

        private void LoadNotesFromAnnotations(IEnumerable<ExtraAnnotationViewInfo> annotationViews)
        {
            if (annotationViews == null)
              return;

            foreach (ExtraAnnotationViewInfo annotationViewInfo in annotationViews)
            {
                if (annotationViewInfo.Nodes == null)
                    continue;

                // If count is not zero, this is an annotation, not a note
                if (annotationViewInfo.Nodes.Count() != 0)
                    continue;

                var annotationGuidValue = IdToGuidConverter(annotationViewInfo.Id);
                var text = annotationViewInfo.Title;

                var noteModel = new NoteModel(
                    annotationViewInfo.Left, 
                    annotationViewInfo.Top, 
                    text, 
                    annotationGuidValue);

                //if this note does not exist, add it to the workspace.
                var matchingNote = this.Notes.FirstOrDefault(x => x.GUID == noteModel.GUID);
                if (matchingNote == null)
                {
                    this.AddNote(noteModel);
                }
           
            }
        }

        private void LoadAnnotations(IEnumerable<ExtraAnnotationViewInfo> annotationViews)
        {
            if (annotationViews == null)
              return;

            foreach (ExtraAnnotationViewInfo annotationViewInfo in annotationViews)
            {
                if (annotationViewInfo.Nodes == null)
                    continue;

                // If count is zero, this is a note, not an annotation
                if (annotationViewInfo.Nodes.Count() == 0)
                    continue;

                var annotationGuidValue = IdToGuidConverter(annotationViewInfo.Id);
                var text = annotationViewInfo.Title;

                // Create a collection of nodes in the given annotation
                var nodes = new List<NodeModel>();
                foreach (string nodeId in annotationViewInfo.Nodes)
                {
                    var guidValue = IdToGuidConverter(nodeId);
                    if (guidValue == null)
                      continue;

                    // NOTE: Some nodes may be annotations and not be found here
                    var nodeModel = Nodes.FirstOrDefault(node => node.GUID == guidValue);
                    if (nodeModel == null)
                      continue;

                    nodes.Add(nodeModel);
                }

                // Create a collection of notes in the given annotation
                var notes = new List<NoteModel>();
                foreach (string noteId in annotationViewInfo.Nodes)
                {
                    var guidValue = IdToGuidConverter(noteId);
                    if (guidValue == null)
                      continue;

                    // NOTE: Some nodes may not be annotations and not be found here
                    var noteModel = Notes.FirstOrDefault(note => note.GUID == guidValue);
                    if (noteModel == null)
                      continue;

                    notes.Add(noteModel);
                }

                var annotationModel = new AnnotationModel(nodes, notes);
                annotationModel.AnnotationText = text;
                annotationModel.FontSize = annotationViewInfo.FontSize;
                annotationModel.Background = annotationViewInfo.Background;
                annotationModel.GUID = annotationGuidValue;

                //if this group/annotation does not exist, add it to the workspace.
                var matchingAnnotation = this.Annotations.FirstOrDefault(x => x.GUID == annotationModel.GUID);
                if (matchingAnnotation == null)
                {
                    this.AddNewAnnotation(annotationModel);
                }
            }
        }

        private Guid IdToGuidConverter(string id)
        {
            Guid deterministicGuid;
            if (!Guid.TryParse(id, out deterministicGuid))
            {
                Console.WriteLine("The id was not a guid, converting to a guid");
                deterministicGuid = GuidUtility.Create(GuidUtility.UrlNamespace, id);
                Console.WriteLine(id + " becomes " + deterministicGuid);
            }

            return deterministicGuid;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Globalization;
using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;

using String = System.String;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Models
{
    public abstract class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient, ILogSource, IDisposable
    {
        public const double ZOOM_MAXIMUM = 4.0;
        public const double ZOOM_MINIMUM = 0.01;

        #region private members
        
        private string fileName;
        private string name;
        private double height = 100;
        private double width = 100;
        private double x;
        private double y;
        private double zoom = 1.0;
        private DateTime lastSaved;
        private string author = "None provided";
        private string description = "";
        private string category = "";
        private bool hasUnsavedChanges;
        private readonly ObservableCollection<NodeModel> nodes;
        private readonly ObservableCollection<ConnectorModel> connectors;
        private readonly ObservableCollection<NoteModel> notes;
        private readonly UndoRedoRecorder undoRecorder;
        private IEnumerable<KeyValuePair<Guid, List<string>>> preloadedTraceData;

        #endregion

        #region events

        public delegate void WorkspaceSavedEvent(WorkspaceModel model);

        public event DynamoModel.NodeEventHandler RequestNodeCentered;
        protected virtual void OnRequestNodeCentered(object sender, ModelEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }

        public event DynamoModel.ZoomEventHandler ZoomChanged;
        /// <summary>
        /// Used during open and workspace changes to set the zoom of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnZoomChanged(object sender, DynamoModel.ZoomEventArgs e)
        {
            if (ZoomChanged != null)
            {
                //Debug.WriteLine(string.Format("Setting zoom to {0}", e.Zoom));
                ZoomChanged(this, e);
            }
        }

        public event DynamoModel.PointEventHandler CurrentOffsetChanged;
        /// <summary>
        /// Used during open and workspace changes to set the location of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
            {
                Debug.WriteLine("Setting current offset to {0}", e.Point);
                CurrentOffsetChanged(this, e);
            }
        }

        public event Action Modified;
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnModified()
        {
            if (Modified != null)
                Modified();
        }

        public event Action WorkspaceSaved;
        /// <summary>
        ///     If there are observers for the save, notifies them
        /// </summary>
        protected virtual void OnWorkspaceSaved()
        {
            LastSaved = DateTime.Now;
            HasUnsavedChanges = false;

            if (WorkspaceSaved != null)
                WorkspaceSaved();
        }

        //TODO(Steve): Since we now expose a
        public event Action<NodeModel> NodeAdded;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnNodeAdded(NodeModel obj)
        {
            var handler = NodeAdded;
            if (handler != null) handler(obj);
        }

        public event Action<ConnectorModel> ConnectorAdded;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnConnectorAdded(ConnectorModel obj)
        {
            var handler = ConnectorAdded;
            if (handler != null) handler(obj);
        }

        #endregion

        #region public properties

        [Obsolete("Use Modified event", true)]
        public bool WatchChanges
        {
            set
            {
                if (value)
                {
                    nodes.CollectionChanged += MarkUnsavedAndModified;
                    notes.CollectionChanged += MarkUnsaved;
                    connectors.CollectionChanged += MarkUnsavedAndModified;
                }
                else
                {
                    nodes.CollectionChanged -= MarkUnsavedAndModified;
                    notes.CollectionChanged -= MarkUnsaved;
                    connectors.CollectionChanged -= MarkUnsavedAndModified;
                }
            }
        }

        /// <summary>
        ///     Search category for this workspace, if it is a Custom Node.
        /// </summary>
        //TODO(Steve): Move to CustomNodeWorkspace, potentially in the Definition
        public string Category
        {
            get { return category; }
            set
            {
                category = value;
                RaisePropertyChanged("Category");
            }
        }

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

        //TODO(Steve): split these into IEnumerable and INotifyCollectionChanged, so that external components can't mutate

        /// <summary>
        ///     All of the nodes currently in the workspace.
        /// </summary>
        public ObservableCollection<NodeModel> Nodes { get { return nodes; } }

        /// <summary>
        ///     All of the connectors currently in the workspace.
        /// </summary>
        public ObservableCollection<ConnectorModel> Connectors { get { return connectors; } }

        /// <summary>
        ///     All of the notes currently in the workspace.
        /// </summary>
        public ObservableCollection<NoteModel> Notes { get { return notes; } }

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
        //TODO(Steve): Investigate whether this is necessary
        public String Name
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
        public Rect Rect
        {
            get { return new Rect(x, y, width, height); }
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

        //TODO(Steve): This probably isn't needed inside of WorkspaceModel
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

        /// <summary>
        /// This does not belong here, period. It is here simply because there is 
        /// currently no better place to put it. A DYN file is loaded by DynamoModel,
        /// subsequently populating WorkspaceModel, along the way, the trace data 
        /// gets preloaded with the file. The best place for this cached data is in 
        /// the EngineController (or even LiveRunner), but the engine gets reset in 
        /// a rather nondeterministic way (for example, when Revit idle thread 
        /// decides it is time to execute a pre-scheduled engine reset). And it gets 
        /// done more than once during file open. So that's out. The second best 
        /// place to store this information is then the WorkspaceModel, where file 
        /// loading is SUPPOSED TO BE done. As of now we let DynamoModel sets the 
        /// loaded data (since it deals with loading DYN file), but in near future,
        /// the file loading mechanism will be completely moved into WorkspaceModel,
        /// that's the time we removed this property setter below.
        /// </summary>
        internal IEnumerable<KeyValuePair<Guid, List<string>>> PreloadedTraceData
        {
            get
            {
                return preloadedTraceData;
            }

            set
            {
                if (value != null && (preloadedTraceData != null))
                {
                    const string message = "PreloadedTraceData cannot be set twice";
                    throw new InvalidOperationException(message);
                }

                preloadedTraceData = value;
            }
        }

        #endregion

        #region constructors

        protected WorkspaceModel(string name, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
        {
            Name = name;

            nodes = new ObservableCollection<NodeModel>(e);
            connectors = new ObservableCollection<ConnectorModel>(c);
            notes = new ObservableCollection<NoteModel>();
            X = x;
            Y = y;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);
        }

        public event Action Disposed;
        public virtual void Dispose()
        {
            var handler = Disposed;
            if (handler != null) 
                handler();
            Disposed = null;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="newPath">The path to save to</param>
        /// <param name="core"></param>
        public virtual bool SaveAs(string newPath, ProtoCore.Core core)
        {
            if (String.IsNullOrEmpty(newPath)) return false;

            Log("Saving " + newPath + "...");
            try
            {
                if (SaveInternal(newPath, core))
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

        [Obsolete("Use AddNode(NodeModel)", true)]
        public NodeModel AddNode(double xPos, double yPos, string nodeName, NodeFactory factory, EngineController engine, CustomNodeManager manager)
        {
            var id = Guid.NewGuid();
            return AddNode(id, nodeName, xPos, yPos, false, false, factory, engine, manager);
        }

        [Obsolete("Use AddNode(NodeModel)", true)]
        public T AddNode<T>(NodeFactory factory) where T : NodeModel
        {
            var node = (T)factory.CreateNodeInstance(null, null, null); 
            //var node = factory.CreateNodeInstance<T>();
            if (node == null) throw new Exception("The supplied node Type was invalid!");

            nodes.Add(node);

            return node;
        }

        /// <summary>
        ///     Adds a node to this workspace.
        /// </summary>
        /// <param name="model"></param>
        public void AddNode(NodeModel model)
        {
            nodes.Add(model);
            model.Modified += OnModified;
            OnNodeAdded(model);
        }

        /// <summary>
        ///     Removes a node from this workspace.
        /// </summary>
        /// <param name="model"></param>
        public void RemoveNode(NodeModel model)
        {
            if (nodes.Remove(model))
            {
                model.Modified -= OnModified;
                model.Dispose();
            }
        }

        /// <summary>
        /// Create a node with the given parameters. Since this method is called
        /// on an instance of DynamoModel, it also raises node added event to any
        /// event handlers, typically useful for real user scenario (listeners 
        /// may include package manager and other UI components).
        /// </summary>
        /// <param name="nodeId">The Guid to be used for the new node, it cannot
        ///     be Guid.Empty since this method does not attempt to internally generate 
        ///     a new Guid. An ArgumentException will be thrown if this argument is 
        ///     Guid.Empty.</param>
        /// <param name="nodeName">The name of the node type to be created.</param>
        /// <param name="xPos">The x coordinates where the newly created node should 
        ///     be placed. This value is ignored if useDefaultPos is true.</param>
        /// <param name="yPos">The y coordinates where the newly created node should 
        ///     be placed. This value is ignored if useDefaultPos is true.</param>
        /// <param name="useDefaultPos">This parameter indicates if the node 
        ///     should be created at the default position. If this parameter is true,
        ///     the node is created at the center of view, and both x and y parameters
        ///     are ignored. If this is false, the values for both x and y parameters 
        ///     will be used as the initial position of the new node.</param>
        /// <param name="transformCoordinates">If this parameter is true, then the
        ///     position of new node will be transformed from outerCanvas space into 
        ///     zoomCanvas space.</param>
        /// <param name="factory"></param>
        /// <param name="dynamoLogger"></param>
        /// <param name="engine"></param>
        /// <param name="customNodeManager"></param>
        /// <param name="xmlNode">This argument carries information that a node 
        ///     may require for its creation. The new node loads itself from this 
        ///     parameter if one is specified. This parameter is optional.</param>
        /// <returns>Returns the created NodeModel, or null if the operation has 
        /// failed.</returns>
        [Obsolete("Use AddNode(NodeModel)", true)]
        public NodeModel AddNode(Guid nodeId, string nodeName, double xPos, double yPos, bool useDefaultPos, bool transformCoordinates, NodeFactory factory, EngineController engine, CustomNodeManager customNodeManager, XmlElement xmlNode = null)
        {
            if (nodeId == Guid.Empty)
                throw new ArgumentException(@"Node ID must be specified", "nodeId");

            NodeModel node = factory.CreateNodeInstance(nodeName, engine, customNodeManager);
            if (node == null)
            {
                const string format = "Failed to create node '{0}' (GUID: {1})";
                Log(string.Format(format, nodeName, nodeId));
                return null;
            }

            AddNode(node, nodeId, x, y, useDefaultPos, transformCoordinates, xmlNode);
            return node;
        }

        [Obsolete("Use AddNode(NodeModel)", true)]
        public void AddNode(
            NodeModel node, Guid nodeId, double xPos, double yPos,
            bool useDefaultPos, bool transformCoordinates, XmlElement xmlNode = null)
        {
            // Fix for: 
            //  http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4024
            //  http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5045
            //  http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4767
            //  http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4946
            // 
            // Various derived classes of NodeModel like CodeBlockNode, build 
            // their internal variables based on the node's GUID. In cases like 
            // this, a node's GUID must be finalized before variable generation 
            // logic kicks in.
            // 
            node.GUID = nodeId; // Set the node's GUID before anything else.

            if (useDefaultPos == false) // Position was specified.
            {
                node.X = xPos;
                node.Y = yPos;
            }

            Nodes.Add(node);

            if (null != xmlNode)
                node.Load(xmlNode);

            ModelEventArgs args = null;
            if (!useDefaultPos)
                args = new ModelEventArgs(node, x, y, transformCoordinates);
            else
            {
                // The position of the new node has not been specified.
                args = new ModelEventArgs(node, transformCoordinates);
            }

            OnRequestNodeCentered(this, args);

            node.EnableInteraction();

            OnNodeAdded(node);
        }

        [Obsolete("Use AddConnection(ConnectorModel) instead", true)]
        public ConnectorModel AddConnection(NodeModel start, NodeModel end, int startIndex, int endIndex, PortType portType = PortType.Input)
        {
            try
            {
                var c = ConnectorModel.Make(start, end, startIndex, endIndex, portType );

                if (c != null)
                    Connectors.Add(c);

                OnConnectorAdded(c);

                return c;
            }
            catch (Exception e)
            {
                Log(LogMessage.Error(e));
            }

            return null;
        }

        public void AddConnection(ConnectorModel model)
        {
            Connectors.Add(model);
            OnConnectorAdded(model);
        }

        public NoteModel AddNote(bool centerNote, double xPos, double yPos, string text, Guid id)
        {
            var noteModel = new NoteModel(xPos, yPos, string.IsNullOrEmpty(text) ? "New Note" : text, id);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note

            if (centerNote)
            {
                var args = new ModelEventArgs(noteModel, true);
                OnRequestNodeCentered(this, args);
            }

            Notes.Add(noteModel);
            return noteModel;
        }

        /// <summary>
        /// Save assuming that the Filepath attribute is set.
        /// </summary>
        public virtual bool Save(ProtoCore.Core core)
        {
            return SaveAs(FileName, core);
        }

        internal void ResetWorkspace()
        {
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

        //TODO: Replace all RequestSync calls with RaisePropertyChanged-style system, that way observable collections can catch any changes
        [Obsolete("Use OnModified Event", true)]
        public void DisableReporting()
        {
            foreach (var node in Nodes)
                node.DisableReporting();
        }

        [Obsolete("Use OnModified Event", true)]
        public void EnableReporting()
        {
            foreach (var node in Nodes)
                node.EnableReporting();
        }

        public IEnumerable<NodeModel> GetHangingNodes()
        {
            return
                Nodes.Where(
                    node =>
                        node.OutPortData.Any() && node.OutPorts.Any(port => !port.Connectors.Any()));
        }

        public IEnumerable<NodeModel> GetTopMostNodes()
        {
            return Nodes.Where(IsTopMostNode);
        }

        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        #endregion

        #region private/internal methods

        //If node is connected to some other node(other than Output) then it is not a 'top' node
        private static bool IsTopMostNode(NodeModel node)
        {
            if (node.OutPortData.Count < 1)
                return false;

            foreach (var port in node.OutPorts.Where(port => port.Connectors.Count != 0))
            {
                return port.Connectors.Any(connector => connector.End.Owner is Output);
            }

            return true;
        }

        public event EventHandler Updated;
        public void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }

        private bool SaveInternal(string targetFilePath, ProtoCore.Core core)
        {
            // Create the xml document to write to.
            var document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", null, null);
            document.AppendChild(document.CreateElement("Workspace"));

            Utils.SetDocumentXmlPath(document, targetFilePath);

            if (!PopulateXmlDocument(document))
                return false;

            SerializeSessionData(document, core);

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

        protected virtual bool PopulateXmlDocument(XmlDocument xmlDoc)
        {
            try
            {
                var root = xmlDoc.DocumentElement;
                root.SetAttribute("Version", WorkspaceVersion.ToString());
                root.SetAttribute("X", X.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Y", Y.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("zoom", Zoom.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Description", Description);
                root.SetAttribute("Category", Category);
                root.SetAttribute("Name", Name);

                var elementList = xmlDoc.CreateElement("Elements");
                //write the root element
                root.AppendChild(elementList);

                foreach (var el in Nodes)
                {
                    var dynEl = el.Serialize(xmlDoc, SaveContext.File);
                    elementList.AppendChild(dynEl);
                }

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
                    var note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.Text);
                    note.SetAttribute("x", n.X.ToString(CultureInfo.InvariantCulture));
                    note.SetAttribute("y", n.Y.ToString(CultureInfo.InvariantCulture));
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }
        }

        // TODO(Ben): Documentation to come before pull request.
        protected virtual void SerializeSessionData(XmlDocument document, ProtoCore.Core core)
        {
            if (document.DocumentElement == null)
            {
                const string message = "Workspace should have been saved before this";
                throw new InvalidOperationException(message);
            }

            try
            {
                if (core == null) // No execution yet as of this point.
                    return;

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids =
                    Nodes.Where(
                        n => n is DSFunction || n is DSVarArgFunction || n is CodeBlockNodeModel)
                        .Select(n => n.GUID);

                var nodeTraceDataList = core.GetTraceDataForNodes(nodeGuids);

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
            ModelBase model = GetModelInternal(modelGuid);
            if (null != model)
            {
                RecordModelForModification(model);
                if (!model.HandleModelEvent(eventName))
                {
                    string type = model.GetType().FullName;
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
        }

        internal void UpdateModelValue(Guid modelGuid, string propertyName, string value)
        {
            ModelBase model = GetModelInternal(modelGuid);
            if (null != model)
            {
                RecordModelForModification(model);
                if (!model.UpdateValue(propertyName, value))
                {
                    string type = model.GetType().FullName;
                    string message = string.Format(
                        "ModelBase.UpdateValue call not handled.\n\n" +
                        "Model type: {0}\n" +
                        "Model GUID: {1}\n" +
                        "Property name: {2}\n" +
                        "Property value: {3}",
                        type, modelGuid, propertyName, value);

                    // All 'UpdateValue' calls must be handled by one of the 
                    // ModelBase derived classes that the 'UpdateModelValue'
                    // is intended for.
                    throw new InvalidOperationException(message);
                }

                HasUnsavedChanges = true;
            }
        }

        [Obsolete("Node to Code not enabled, API subject to change.")]
        internal void ConvertNodesToCodeInternal(Guid nodeId, EngineController engineController)
        {
            IEnumerable<NodeModel> selectedNodes =
                DynamoSelection.Instance.Selection.OfType<NodeModel>().Where(n => n.IsConvertible);
            
            if (!selectedNodes.Any())
                return;

            Dictionary<string, string> variableNameMap;
            string code = engineController.ConvertNodesToCode(selectedNodes, out variableNameMap);

            CodeBlockNodeModel codeBlockNode;

            //UndoRedo Action Group----------------------------------------------
            using (UndoRecorder.BeginActionGroup())
            {
                #region Step I. Delete all nodes and their connections
                //Create two dictionarys to store the details of the external connections that have to 
                //be recreated after the conversion
                var externalInputConnections = new Dictionary<ConnectorModel, string>();
                var externalOutputConnections = new Dictionary<ConnectorModel, string>();

                //Also collect the average X and Y co-ordinates of the different nodes
                var nodeList = selectedNodes.ToList();
                int nodeCount = nodeList.Count;
                double totalX = 0, totalY = 0;


                foreach (var node in nodeList) 
                {
                    #region Step I.A. Delete the connections for the node

                    foreach (var connector in node.AllConnectors.ToList())
                    {
                        if (!IsInternalNodeToCodeConnection(connector))
                        {
                            //If the connector is an external connector, the save its details
                            //for recreation later
                            var startNode = connector.Start.Owner;
                            int index = startNode.OutPorts.IndexOf(connector.Start);
                            //We use the varibleName as the connection between the port of the old Node
                            //to the port of the new node.
                            var variableName = startNode.GetAstIdentifierForOutputIndex(index).Value;
                            if (variableNameMap.ContainsKey(variableName))
                                variableName = variableNameMap[variableName];

                            //Store the data in the corresponding dictionary
                            if (startNode == node)
                                externalOutputConnections.Add(connector, variableName);
                            else
                                externalInputConnections.Add(connector, variableName);
                        }

                        //Delete the connector
                        UndoRecorder.RecordDeletionForUndo(connector);
                        connector.NotifyConnectedPortsOfDeletion();
                        Connectors.Remove(connector);
                    }
                    #endregion

                    #region Step I.B. Delete the node
                    totalX += node.X;
                    totalY += node.Y;
                    UndoRecorder.RecordDeletionForUndo(node);
                    Nodes.Remove(node);
                    #endregion
                }
                #endregion

                #region Step II. Create the new code block node
                codeBlockNode = new CodeBlockNodeModel(
                    code,
                    nodeId,
                    this,
                    totalX/nodeCount,
                    totalY/nodeCount);
                UndoRecorder.RecordCreationForUndo(codeBlockNode);
                Nodes.Add(codeBlockNode);
                #endregion

                #region Step III. Recreate the necessary connections
                ReConnectInputConnections(externalInputConnections, codeBlockNode);
                ReConnectOutputConnections(externalOutputConnections, codeBlockNode);
                #endregion
            }
            //End UndoRedo Action Group------------------------------------------

            // select node
            
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(codeBlockNode);

            OnModified();
        }
        
        private void MarkUnsaved(
            object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            HasUnsavedChanges = true;
        }

        private void MarkUnsavedAndModified(
            object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            HasUnsavedChanges = true;
            OnModified();
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
        public void RecordModelForModification(ModelBase model)
        {
            if (null != model)
            {
                var models = new List<ModelBase> { model };
                RecordModelsForModification(models);
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
        /// 
        internal void RecordModelsForModification(List<ModelBase> models)
        {
            if (null == undoRecorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            using (undoRecorder.BeginActionGroup())
            {
                foreach (ModelBase model in models)
                    undoRecorder.RecordModificationForUndo(model);
            }
        }

        public void RecordModelsForUndo(Dictionary<ModelBase, UndoRedoRecorder.UserAction> models)
        {
            if (null == undoRecorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            using (undoRecorder.BeginActionGroup())
            {
                foreach (var modelPair in models)
                {
                    switch (modelPair.Value)
                    {
                        case UndoRedoRecorder.UserAction.Creation:
                            undoRecorder.RecordCreationForUndo(modelPair.Key);
                            break;
                        case UndoRedoRecorder.UserAction.Deletion:
                            undoRecorder.RecordDeletionForUndo(modelPair.Key);
                            break;
                        case UndoRedoRecorder.UserAction.Modification:
                            undoRecorder.RecordModificationForUndo(modelPair.Key);
                            break;
                    }
                }
            }
        }

        internal void RecordCreatedModel(ModelBase model)
        {
            if (null != model)
            {
                using (undoRecorder.BeginActionGroup())
                {
                    undoRecorder.RecordCreationForUndo(model);
                }
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
                foreach (ModelBase model in models)
                {
                    if (model is NoteModel)
                    {
                        // Take a snapshot of the note before it goes away.
                        undoRecorder.RecordDeletionForUndo(model);
                        Notes.Remove(model as NoteModel);
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

                        // Note that AllConnectors is duplicated as a separate list 
                        // by calling its "ToList" method. This is the because the 
                        // "Connectors.Remove" will modify "AllConnectors", causing 
                        // the Enumerator in this "foreach" to become invalid.
                        foreach (var conn in node.AllConnectors.ToList())
                        {
                            conn.NotifyConnectedPortsOfDeletion();
                            Connectors.Remove(conn);
                            undoRecorder.RecordDeletionForUndo(conn);
                        }

                        // Take a snapshot of the node before it goes away.
                        undoRecorder.RecordDeletionForUndo(model);

                        RemoveNode(node);
                    }
                    else if (model is ConnectorModel)
                    {
                        var connector = model as ConnectorModel;
                        Connectors.Remove(connector);
                        undoRecorder.RecordDeletionForUndo(model);
                    }
                }

            } // Conclude the deletion.
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
            ModelBase model = GetModelForElement(modelData);

            if (model is NoteModel)
                Notes.Remove(model as NoteModel);
            else if (model is ConnectorModel)
            {
                var connector = model as ConnectorModel;
                Connectors.Remove(connector);
                connector.NotifyConnectedPortsOfDeletion();
            }
            else if (model is NodeModel)
                Nodes.Remove(model as NodeModel);
            else
            {
                // If it gets here we obviously need to handle it.
                throw new InvalidOperationException(string.Format(
                    "Unhandled type: {0}", model.GetType()));
            }
        }

        public void ReloadModel(XmlElement modelData)
        {
            ModelBase model = GetModelForElement(modelData);
            model.Deserialize(modelData, SaveContext.Undo);
        }

        public void CreateModel(XmlElement modelData, EngineController engine, NodeFactory factory, CustomNodeManager manager)
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
                ConnectorModel connector;
                var loaded = NodeGraph.LoadConnectorFromXml(
                    modelData,
                    Nodes.ToDictionary(node => node.GUID),
                    out connector);
                
                if (loaded)
                    Connectors.Add(connector);
            }
            else if (typeName.StartsWith("Dynamo.Models.NoteModel"))
            {
                var noteModel = NodeGraph.LoadNoteFromXml(modelData);
                Notes.Add(noteModel);
            }
            else // Other node types.
            {
                NodeModel nodeModel = factory.CreateNodeFromXml(modelData, SaveContext.Undo);
                Nodes.Add(nodeModel);
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

            throw new ArgumentException(string.Format(
                "Unhandled model type: {0}", helper.ReadString("type")));
        }

        internal ModelBase GetModelInternal(Guid modelGuid)
        {
            ModelBase foundModel = null;
            if (Connectors.Count > 0)
                foundModel = Connectors.FirstOrDefault(c => c.GUID == modelGuid);

            if (null == foundModel && (Nodes.Count > 0))
                foundModel = Nodes.FirstOrDefault(node => node.GUID == modelGuid);

            if (null == foundModel && (Notes.Count > 0))
                foundModel = Notes.FirstOrDefault(note => note.GUID == modelGuid);

            return foundModel;
        }

        #endregion

        #region Node To Code Reconnection

        /// <summary>
        /// Checks whether the given connection is inside the node to code set or outside it. 
        /// This determines if it should be redrawn(if it is external) or if it should be 
        /// deleted (if it is internal)
        /// </summary>
        [Obsolete("Node to Code not enabled, API subject to change.")]
        private static bool IsInternalNodeToCodeConnection(ConnectorModel connector)
        {
            return DynamoSelection.Instance.Selection.Contains(connector.Start.Owner) && DynamoSelection.Instance.Selection.Contains(connector.End.Owner);
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalOutputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="codeBlockNode">The new Node To Code created Code Block Node</param>
        /// <param name="logger"></param>
        [Obsolete("Node to Code not enabled, API subject to change.")]
        private void ReConnectOutputConnections(Dictionary<ConnectorModel, string> externalOutputConnections, CodeBlockNodeModel codeBlockNode)
        {
            foreach (var kvp in externalOutputConnections)
            {
                var connector = kvp.Key;
                string variableName = kvp.Value;

                //Get the start and end idex for the ports for the connection
                int endIndex = connector.End.Owner.InPorts.IndexOf(connector.End);
                int i;
                for (i = 0; i < codeBlockNode.OutPorts.Count; i++)
                {
                    if (codeBlockNode.GetAstIdentifierForOutputIndex(i).Value == variableName)
                        break;
                }
                var portModel = codeBlockNode.OutPorts[i];
                int startIndex = codeBlockNode.OutPorts.IndexOf(portModel);

                //Make the new connection and then record and add it
                var newConnector = AddConnection(codeBlockNode, connector.End.Owner,
                    startIndex, endIndex);

                UndoRecorder.RecordCreationForUndo(newConnector);
            }
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalInputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="codeBlockNode">The new Node To Code created Code Block Node</param>
        /// <param name="logger"></param>
        [Obsolete("Node to Code not enabled, API subject to change.")]
        private void ReConnectInputConnections(
            Dictionary<ConnectorModel, string> externalInputConnections,
            CodeBlockNodeModel codeBlockNode)
        {
            foreach (var kvp in externalInputConnections)
            {
                var connector = kvp.Key;
                string variableName = kvp.Value;

                //Find the start and end index of the ports for the connection
                int startIndex = connector.Start.Owner.OutPorts.IndexOf(connector.Start);
                int endIndex = CodeBlockNodeModel.GetInportIndex(codeBlockNode, variableName);

                //For inputs, a single node can be an input to multiple nodes in the code block node selection
                //After conversion, all these connecetions should become only 1 connection and not many
                //Hence for inputs, it is required to make sure that a certain type of connection has not
                //been created already.
                if (Connectors.All(c => c.End != codeBlockNode.InPorts[endIndex]))
                {
                    //Make the new connection and then record and add it
                    var newConnector = AddConnection(
                        connector.Start.Owner,
                        codeBlockNode,
                        startIndex,
                        endIndex);
                    UndoRecorder.RecordCreationForUndo(newConnector);
                }
            }
        }
        
        #endregion

        /// <summary>
        ///     Syncronously get a string representation of the workspace
        /// </summary>
        [Obsolete("Dispatch GetStringRepOfWorkspace() manually", true)]
        internal string GetStringRepOfWorkspaceSync()
        {
            string outData = String.Empty;

            Action getString = (() => { outData = GetStringRepOfWorkspace(); });

            //dynamoModel.OnRequestDispatcherInvoke(getString);
            
            return outData;
        }

        internal string GetStringRepOfWorkspace()
        {
            // Create the xml document to write to.
            var document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", null, null);
            document.AppendChild(document.CreateElement("Workspace"));

            //This is only used for computing relative offsets, it's not actually created
            string virtualFileName = String.Join(Path.GetTempPath(), "DynamoTemp.dyn");
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

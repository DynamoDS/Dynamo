using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private bool hasUnsavedChanges;
        private readonly ObservableCollection<NodeModel> nodes;
        private readonly ObservableCollection<NoteModel> notes;
        private readonly UndoRedoRecorder undoRecorder;

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
        protected virtual void OnNodeAdded(NodeModel obj)
        {
            var handler = NodeAdded;
            if (handler != null) handler(obj);
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

        #endregion

        #region constructors

        protected WorkspaceModel(
            string name, IEnumerable<NodeModel> e, IEnumerable<NoteModel> n,
            double x, double y, NodeFactory factory)
        {
            Name = name;

            nodes = new ObservableCollection<NodeModel>(e);
            notes = new ObservableCollection<NoteModel>(n);
            X = x;
            Y = y;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);

            NodeFactory = factory;

            foreach (var node in nodes)
                RegisterNode(node);

            foreach (var connector in Connectors)
                RegisterConnector(connector);
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
            Log("Clearing workspace...");

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

            ClearUndoRecorder();
            ResetWorkspace();
        }

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

        /// <summary>
        ///     Adds a node to this workspace.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="centered"></param>
        public void AddNode(NodeModel node, bool centered)
        {
            if (nodes.Contains(node))
                return;

            RegisterNode(node);

            if (centered)
            {
                var args = new ModelEventArgs(node, true);
                OnRequestNodeCentered(this, args);
            }

            //var cbn = node as CodeBlockNodeModel;
            //if (cbn != null)
            //{
            //    var firstChange = true;
            //    PropertyChangedEventHandler codeChangedHandler = (sender, args) =>
            //    {
            //        if (args.PropertyName != "Code") return;
                    
            //        if (string.IsNullOrWhiteSpace(cbn.Code))
            //        {
            //            if (firstChange)
            //                RemoveNode(cbn);
            //            else
            //                RecordAndDeleteModels(new List<ModelBase> { cbn });
            //        }
            //        firstChange = false;
            //    };
            //    cbn.PropertyChanged += codeChangedHandler;
            //    cbn.Disposed += () => { cbn.PropertyChanged -= codeChangedHandler; };
            //}

            nodes.Add(node);
            OnNodeAdded(node);
            HasUnsavedChanges = true;
        }

        private void RegisterNode(NodeModel node)
        {
            node.AstUpdated += OnAstUpdated;
            node.ConnectorAdded += OnConnectorAdded;
        }

        /// <summary>
        ///     Indicates that this workspace's DesignScript AST has been updated.
        /// </summary>
        public virtual void OnAstUpdated()
        {

        }

        /// <summary>
        ///     Removes a node from this workspace.
        /// </summary>
        /// <param name="model"></param>
        public void RemoveNode(NodeModel model)
        {
            if (nodes.Remove(model))
            {
                DisposeNode(model);
                OnAstUpdated();
            }
        }

        protected void DisposeNode(NodeModel model)
        {
            model.ConnectorAdded -= OnConnectorAdded;
            model.AstUpdated -= OnAstUpdated;
            OnNodeRemoved(model);
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

        #endregion

        #region private/internal methods
        
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
                root.SetAttribute("Name", Name);

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
        // TODO(Steve): This probably only belongs on HomeWorkspaceModel. -- MAGN-5715
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
                RecordModelForModification(model, UndoRecorder);
                if (!model.HandleModelEvent(eventName, undoRecorder))
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
                RecordModelForModification(model, UndoRecorder);
                if (!model.UpdateValue(propertyName, value, undoRecorder))
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
        internal void ConvertNodesToCodeInternal(Guid nodeId, EngineController engineController, bool verboseLogging)
        {
            IEnumerable<NodeModel> selectedNodes =
                DynamoSelection.Instance.Selection.OfType<NodeModel>().Where(n => n.IsConvertible);
            
            if (!selectedNodes.Any())
                return;

            Dictionary<string, string> variableNameMap;
            string code = engineController.ConvertNodesToCode(selectedNodes, out variableNameMap, verboseLogging);

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
                        connector.Delete();
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
                    totalX/nodeCount,
                    totalY/nodeCount, engineController.LibraryServices);
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

            OnAstUpdated();
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
                            conn.Delete();
                            undoRecorder.RecordDeletionForUndo(conn);
                        }

                        // Take a snapshot of the node before it goes away.
                        undoRecorder.RecordDeletionForUndo(node);

                        RemoveNode(node);
                    }
                    else if (model is ConnectorModel)
                    {
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
                connector.Delete();
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
                ConnectorModel connector;
                NodeGraph.LoadConnectorFromXml(
                    modelData,
                    Nodes.ToDictionary(node => node.GUID),
                    out connector);
            }
            else if (typeName.StartsWith("Dynamo.Models.NoteModel"))
            {
                var noteModel = NodeGraph.LoadNoteFromXml(modelData);
                Notes.Add(noteModel);
            }
            else // Other node types.
            {
                NodeModel nodeModel = NodeFactory.CreateNodeFromXml(modelData, SaveContext.Undo);
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

            throw new ArgumentException(
                string.Format("Unhandled model type: {0}", helper.ReadString("type", modelData.Name)));
        }

        internal ModelBase GetModelInternal(Guid modelGuid)
        {
            ModelBase foundModel = (Connectors.FirstOrDefault(c => c.GUID == modelGuid)
                ?? (ModelBase)Nodes.FirstOrDefault(node => node.GUID == modelGuid))
                ?? Notes.FirstOrDefault(note => note.GUID == modelGuid);

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
                var newConnector = ConnectorModel.Make(
                    codeBlockNode,
                    connector.End.Owner,
                    startIndex,
                    endIndex);

                UndoRecorder.RecordCreationForUndo(newConnector);
            }
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalInputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="codeBlockNode">The new Node To Code created Code Block Node</param>
        [Obsolete("Node to Code not enabled, API subject to change.")]
        private void ReConnectInputConnections(
            Dictionary<ConnectorModel, string> externalInputConnections, CodeBlockNodeModel codeBlockNode)
        {
            var connections = from kvp in externalInputConnections
                              let connector = kvp.Key
                              let variableName = kvp.Value
                              let startIndex = connector.Start.Owner.OutPorts.IndexOf(connector.Start)
                              let endIndex = CodeBlockNodeModel.GetInportIndex(codeBlockNode, variableName)
                              where Connectors.All(c => c.End != codeBlockNode.InPorts[endIndex])
                              select
                                  new
                                  {
                                      Start = connector.Start.Owner,
                                      End = codeBlockNode,
                                      StartIdx = startIndex,
                                      EndIdx = endIndex
                                  };

            foreach (var newConnector in connections)
            {
                var connector = ConnectorModel.Make(
                    newConnector.Start,
                    newConnector.End,
                    newConnector.StartIdx,
                    newConnector.EndIdx);
                UndoRecorder.RecordCreationForUndo(connector);
            }
        }

        #endregion

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

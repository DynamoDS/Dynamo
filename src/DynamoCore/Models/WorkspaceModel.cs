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
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Utils = Dynamo.Nodes.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Models
{
    public abstract class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient
    {
        public static readonly double ZOOM_MAXIMUM = 4.0;
        public static readonly double ZOOM_MINIMUM = 0.01;

        public delegate void WorkspaceSavedEvent(WorkspaceModel model);

        #region internal members

        private string _fileName;
        private string _name;
        private double _height = 100;
        private double _width = 100;
        private double _x;
        private double _y;
        private double _zoom = 1.0;
        private DateTime _lastSaved;
        private string _author = "None provided";
        private string _description = "";
        private string _category = "";
        private bool _hasUnsavedChanges;
        private bool _isCurrentSpace;
        private ObservableCollection<NodeModel> _nodes;
        private ObservableCollection<ConnectorModel> _connectors;

        #endregion

        #region events

        public event Action OnModified;
        public event WorkspaceSavedEvent WorkspaceSaved;

        #endregion

        #region Properties

        public bool WatchChanges
        {
            set
            {
                if (value)
                {

                    Nodes.CollectionChanged += MarkUnsavedAndModified;
                    Notes.CollectionChanged += MarkUnsaved;
                    Connectors.CollectionChanged += MarkUnsavedAndModified;
                }
                else
                {
                    Nodes.CollectionChanged -= MarkUnsavedAndModified;
                    Notes.CollectionChanged -= MarkUnsaved;
                    Connectors.CollectionChanged -= MarkUnsavedAndModified;
                }
            }
        }

        /// <summary>
        ///     Defines whether this is the current space in Dynamo
        /// </summary>
        public bool IsCurrentSpace
        {
            get { return _isCurrentSpace; }
            set
            {
                _isCurrentSpace = value;
                RaisePropertyChanged("IsCurrentSpace");
            }
        }

        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                RaisePropertyChanged("Category");
            }
        }

        /// <summary>
        ///     The date of the last save.
        /// </summary>
        public DateTime LastSaved
        {
            get { return _lastSaved; }
            set
            {
                _lastSaved = value;
                RaisePropertyChanged("LastSaved");
            }
        }

        /// <summary>
        ///     A description of the workspace
        /// </summary>
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                RaisePropertyChanged("Author");
            }
        }

        /// <summary>
        ///     A description of the workspace
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        /// <summary>
        ///     Are there unsaved changes in the workspace?
        /// </summary>
        public bool HasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
            set
            {
                _hasUnsavedChanges = value;
                RaisePropertyChanged("HasUnsavedChanges");
            }
        }

        public ObservableCollection<NodeModel> Nodes
        {
            get { return _nodes; }
            internal set
            {
                if (Equals(value, _nodes)) return;
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }

        public ObservableCollection<ConnectorModel> Connectors
        {
            get { return _connectors; }
            internal set
            {
                if (Equals(value, _connectors)) return;
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }

        public ObservableCollection<NoteModel> Notes { get; internal set; }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        ///     Get or set the X position of the workspace.
        /// </summary>
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        ///     Get or set the Y position of the workspace
        /// </summary>
        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        /// <summary>
        ///     Get the height of the workspace's bounds.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        ///     Get the width of the workspace's bounds.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        ///     Get the bounds of the workspace.
        /// </summary>
        public Rect Rect
        {
            get { return new Rect(_x, _y, _width, _height); }
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

        internal Version WorkspaceVersion { get; set; }

        public double CenterX
        {
            get { return 0; }
            set
            {

            }
        }

        public double CenterY
        {
            get { return 0; }
            set
            {

            }
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
        private IEnumerable<KeyValuePair<Guid, List<string>>> preloadedTraceData = null;

        internal IEnumerable<KeyValuePair<Guid, List<string>>> PreloadedTraceData
        {
            get
            {
                return this.preloadedTraceData;
            }

            set
            {
                if (value != null && (this.preloadedTraceData != null))
                {
                    var message = "PreloadedTraceData cannot be set twice";
                    throw new InvalidOperationException(message);
                }

                this.preloadedTraceData = value;
            }
        }

        #endregion

        #region constructors

        protected WorkspaceModel(
            String name, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
        {
            Name = name;

            Nodes = new TrulyObservableCollection<NodeModel>(e);
            Connectors = new TrulyObservableCollection<ConnectorModel>(c);
            Notes = new ObservableCollection<NoteModel>();
            X = x;
            Y = y;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceSaved += OnWorkspaceSaved;
            WorkspaceVersion = AssemblyHelper.GetDynamoVersion();
            undoRecorder = new UndoRedoRecorder(this);
        }

        #endregion

        #region public methods

        public abstract void OnDisplayed();

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="newPath">The path to save to</param>
        public virtual bool SaveAs(string newPath)
        {
            if (String.IsNullOrEmpty(newPath)) return false;

            dynSettings.DynamoLogger.Log("Saving " + newPath + "...");
            try
            {
                if (SaveInternal(newPath))
                    OnWorkspaceSaved();
            }
            catch (Exception ex)
            {
                //Log(ex);
                dynSettings.DynamoLogger.Log(ex.Message);
                dynSettings.DynamoLogger.Log(ex.StackTrace);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save assuming that the Filepath attribute is set.
        /// </summary>
        public virtual bool Save()
        {
            return SaveAs(FileName);
        }

        //TODO: Replace all Modified calls with RaisePropertyChanged-style system, that way observable collections can catch any changes
        public void DisableReporting()
        {
            Nodes.ToList().ForEach(x => x.DisableReporting());
        }

        public void EnableReporting()
        {
            Nodes.ToList().ForEach(x => x.EnableReporting());
        }

        public virtual void Modified()
        {
            //dynSettings.DynamoLogger.Log("Workspace modified.");
            if (OnModified != null)
                OnModified();
        }

        public IEnumerable<NodeModel> GetHangingNodes()
        {
            return Nodes.Where(x => x.OutPortData.Any() && x.OutPorts.Any(y => !y.Connectors.Any()));
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

        private bool SaveInternal(string targetFilePath)
        {
            // Create the xml document to write to.
            var document = new XmlDocument();
            document.CreateXmlDeclaration("1.0", null, null);
            document.AppendChild(document.CreateElement("Workspace"));

            Dynamo.Nodes.Utilities.SetDocumentXmlPath(document, targetFilePath);

            if (!this.PopulateXmlDocument(document))
                return false;

            this.SerializeSessionData(document);

            try
            {
                Dynamo.Nodes.Utilities.SetDocumentXmlPath(document, string.Empty);
                document.Save(targetFilePath);
            }
            catch (System.IO.IOException)
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
                    var typeName = el.GetType().ToString();

                    var dynEl = xmlDoc.CreateElement(typeName);
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    dynEl.SetAttribute("x", el.X.ToString(CultureInfo.InvariantCulture));
                    dynEl.SetAttribute("y", el.Y.ToString(CultureInfo.InvariantCulture));
                    dynEl.SetAttribute("isVisible", el.IsVisible.ToString().ToLower());
                    dynEl.SetAttribute("isUpstreamVisible", el.IsUpstreamVisible.ToString().ToLower());
                    dynEl.SetAttribute("lacing", el.ArgumentLacing.ToString());

                    el.Save(xmlDoc, dynEl, SaveContext.File);
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

                            if (c.End.PortType == PortType.INPUT)
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
        protected virtual void SerializeSessionData(XmlDocument document)
        {
            if (document.DocumentElement == null)
            {
                var message = "Workspace should have been saved before this";
                throw new InvalidOperationException(message);
            }

            try
            {
                ProtoCore.Core core = null;
                if (dynSettings.Controller != null)
                {
                    var engine = dynSettings.Controller.EngineController;
                    if (engine != null && (engine.LiveRunnerCore != null))
                        core = engine.LiveRunnerCore;
                }

                if (core == null) // No execution yet as of this point.
                    return;

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids = this.Nodes.Where((n) =>
                {
                    return (n is DSFunction
                        || (n is DSVarArgFunction)
                        || (n is CodeBlockNodeModel));

                }).Select((n) => n.GUID);

                var nodeTraceDataList = core.GetTraceDataForNodes(nodeGuids);

                if (nodeTraceDataList.Count() > 0)
                    Utils.SaveTraceDataToXmlDocument(document, nodeTraceDataList);
            }
            catch (Exception exception)
            {
                // We'd prefer file saving process to not crash Dynamo,
                // otherwise user will lose the last hope in retaining data.
                dynSettings.DynamoLogger.Log(exception.Message);
                dynSettings.DynamoLogger.Log(exception.StackTrace);
            }
        }

        internal void SendModelEvent(Guid modelGuid, string eventName)
        {
            ModelBase model = this.GetModelInternal(modelGuid);
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
                        type, modelGuid.ToString(), eventName);

                    // All 'HandleModelEvent' calls must be handled by one of 
                    // the ModelBase derived classes that the 'SendModelEvent'
                    // is intended for.
                    throw new InvalidOperationException(message);
                }

                this.HasUnsavedChanges = true;
            }
        }

        internal void UpdateModelValue(Guid modelGuid, string name, string value)
        {
            ModelBase model = GetModelInternal(modelGuid);
            if (null != model)
            {
                RecordModelForModification(model);
                if (!model.UpdateValue(name, value))
                {
                    string type = model.GetType().FullName;
                    string message = string.Format(
                        "ModelBase.UpdateValue call not handled.\n\n" +
                        "Model type: {0}\n" +
                        "Model GUID: {1}\n" +
                        "Property name: {2}\n" +
                        "Property value: {3}",
                        type, modelGuid.ToString(), name, value);

                    // All 'UpdateValue' calls must be handled by one of the 
                    // ModelBase derived classes that the 'UpdateModelValue'
                    // is intended for.
                    throw new InvalidOperationException(message);
                }

                this.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// After command framework is implemented, this method should now be only 
        /// called from a menu item (i.e. Ctrl + W). It should not be used as a 
        /// way for any other code paths to convert nodes to code programmatically. 
        /// For that we now have ConvertNodesToCodeInternal which takes in more 
        /// configurable arguments.
        /// </summary>
        /// <param name="parameters">This is not used and should always be null,
        /// otherwise an ArgumentException will be thrown.</param>
        /// 
        internal void NodeToCode(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                const string message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            Guid nodeID = Guid.NewGuid();
            var command = new DynCmd.ConvertNodesToCodeCommand(nodeID);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanNodeToCode(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        internal void ConvertNodesToCodeInternal(Guid nodeId)
        {
            IEnumerable<NodeModel> nodes = DynamoSelection.Instance.Selection.OfType<NodeModel>().Where(n => n.IsConvertible);
            if (!nodes.Any())
                return;

            Dictionary<string, string> variableNameMap;
            string code = dynSettings.Controller.EngineController.ConvertNodesToCode(nodes, out variableNameMap);

            //UndoRedo Action Group----------------------------------------------
            UndoRecorder.BeginActionGroup();

            #region Step I. Delete all nodes and their connections
            //Create two dictionarys to store the details of the external connections that have to 
            //be recreated after the conversion
            var externalInputConnections = new Dictionary<ConnectorModel, string>();
            var externalOutputConnections = new Dictionary<ConnectorModel, string>();

            //Also collect the average X and Y co-ordinates of the different nodes
            var nodeList = nodes.ToList();
            int nodeCount = nodeList.Count;
            double totalX = 0, totalY = 0;


            for (int i = 0; i < nodeList.Count; ++i)
            {
                var node = nodeList[i];
                #region Step I.A. Delete the connections for the node
                var connectors = node.AllConnectors as IList<ConnectorModel>;
                if (null == connectors)
                {
                    connectors = node.AllConnectors.ToList();
                }

                for (int n = 0; n < connectors.Count(); ++n)
                {
                    var connector = connectors[n];
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
            var codeBlockNode = new CodeBlockNodeModel(code, nodeId, this, totalX / nodeCount, totalY / nodeCount);
            UndoRecorder.RecordCreationForUndo(codeBlockNode);
            Nodes.Add(codeBlockNode);
            #endregion

            #region Step III. Recreate the necessary connections
            ReConnectInputConnections(externalInputConnections, codeBlockNode);
            ReConnectOutputConnections(externalOutputConnections, codeBlockNode);
            #endregion

            UndoRecorder.EndActionGroup();
            //End UndoRedo Action Group------------------------------------------

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == nodeId);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }

            Modified();
        }

        /// <summary>
        ///     If there are observers for the save, notifies them
        /// </summary>
        private void OnWorkspaceSaved()
        {
            if (WorkspaceSaved != null)
                WorkspaceSaved(this);
        }

        /// <summary>
        ///     Updates relevant parameters on save
        /// </summary>
        /// <param name="model">The workspace that was just saved</param>
        private static void OnWorkspaceSaved(WorkspaceModel model)
        {
            model.LastSaved = DateTime.Now;
            model.HasUnsavedChanges = false;
            dynSettings.Controller.DynamoViewModel.AddToRecentFiles(model.FileName);
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
            Modified();
        }

        #endregion
        
        #region Undo/Redo Supporting Methods

        private UndoRedoRecorder undoRecorder = null;

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

            undoRecorder.BeginActionGroup();
            {
                foreach (ModelBase model in models)
                    undoRecorder.RecordModificationForUndo(model);
            }
            undoRecorder.EndActionGroup();
        }

        public void RecordModelsForUndo(Dictionary<ModelBase, UndoRedoRecorder.UserAction> models)
        {
            if (null == undoRecorder)
                return;
            if (!ShouldProceedWithRecording(models))
                return;

            undoRecorder.BeginActionGroup();
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
            undoRecorder.EndActionGroup();
        }

        internal void RecordCreatedModel(ModelBase model)
        {
            if (null != model)
            {
                undoRecorder.BeginActionGroup();
                undoRecorder.RecordCreationForUndo(model);
                undoRecorder.EndActionGroup();
            }
        }

        internal void RecordCreatedModels(List<ModelBase> models)
        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing created.

            undoRecorder.BeginActionGroup();
            foreach (ModelBase model in models)
                undoRecorder.RecordCreationForUndo(model);
            undoRecorder.EndActionGroup();
        }

        internal void RecordAndDeleteModels(List<ModelBase> models)
        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing for deletion.

            // Gather a list of connectors first before the nodes they connect
            // to are deleted. We will have to delete the connectors first 
            // before 

            undoRecorder.BeginActionGroup(); // Start a new action group.

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
                    Debug.Assert(this == node.WorkSpace);

                    // Note that AllConnectors is duplicated as a separate list 
                    // by calling its "ToList" method. This is the because the 
                    // "Connectors.Remove" will modify "AllConnectors", causing 
                    // the Enumerator in this "foreach" to become invalid.
                    // 
                    List<ConnectorModel> connectors = node.AllConnectors.ToList();
                    foreach (var conn in connectors)
                    {
                        conn.NotifyConnectedPortsOfDeletion();
                        Connectors.Remove(conn);
                        undoRecorder.RecordDeletionForUndo(conn);
                    }

                    // Take a snapshot of the node before it goes away.
                    undoRecorder.RecordDeletionForUndo(model);

                    node.DisableReporting();
                    node.Destroy();
                    node.Cleanup();
                    node.WorkSpace.Nodes.Remove(node);
                }
                else if (model is ConnectorModel)
                {
                    var connector = model as ConnectorModel;
                    Connectors.Remove(connector);
                    undoRecorder.RecordDeletionForUndo(model);
                }
            }

            undoRecorder.EndActionGroup(); // Conclude the deletion.
        }

        private static bool ShouldProceedWithRecording(List<ModelBase> models)
        {
            if (null != models)
                models.RemoveAll((x) => (x == null));

            return (null != models && (models.Count > 0));
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
                ConnectorModel connector = model as ConnectorModel;
                Connectors.Remove(connector);
                connector.NotifyConnectedPortsOfDeletion();
            }
            else if (model is NodeModel)
                Nodes.Remove(model as NodeModel);
            else
            {
                // If it gets here we obviously need to handle it.
                throw new InvalidOperationException(string.Format(
                    "Unhandled type: {0}", model.GetType().ToString()));
            }
        }

        public void ReloadModel(XmlElement modelData)
        {
            ModelBase model = GetModelForElement(modelData);
            model.Deserialize(modelData, SaveContext.Undo);
        }

        public void CreateModel(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
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

#if USE_DSENGINE
            if (typeName.Equals("Dynamo.Nodes.DSFunction") ||
                typeName.Equals("Dynamo.Nodes.DSVarArgFunction"))
            {
                // For DSFunction and DSVarArgFunction node types, the type name
                // is actually embedded within "name" attribute (for an example,
                // "UV.ByCoordinates@double,double").
                // 
                typeName = modelData.Attributes["name"].Value;
            }
#endif

            if (typeName.StartsWith("Dynamo.Models.ConnectorModel"))
            {
                ConnectorModel connector = ConnectorModel.Make();
                connector.Deserialize(modelData, SaveContext.Undo);
                Connectors.Add(connector);
            }
            else if (typeName.StartsWith("Dynamo.Models.NoteModel"))
            {
                NoteModel noteModel = new NoteModel(0.0, 0.0);
                noteModel.Deserialize(modelData, SaveContext.Undo);
                Notes.Add(noteModel);
            }
            else // Other node types.
            {
                NodeModel nodeModel = DynamoModel.CreateNodeInstance(typeName);
                nodeModel.WorkSpace = this;
                nodeModel.Deserialize(modelData, SaveContext.Undo);
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

            XmlElementHelper helper = new XmlElementHelper(modelData);
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
            if (null == foundModel && (Connectors.Count > 0))
                foundModel = Connectors.FirstOrDefault((x) => x.GUID == modelGuid);

            if (null == foundModel && (Nodes.Count > 0))
                foundModel = Nodes.FirstOrDefault((x) => (x.GUID == modelGuid));

            if (null == foundModel && (Notes.Count > 0))
                foundModel = Notes.FirstOrDefault((x) => (x.GUID == modelGuid));

            return foundModel;
        }

        #endregion

        #region Node To Code Reconnection

        /// <summary>
        /// Checks whether the given connection is inside the node to code set or outside it. 
        /// This determines if it should be redrawn(if it is external) or if it should be 
        /// deleted (if it is internal)
        /// </summary>
        private bool IsInternalNodeToCodeConnection(ConnectorModel connector)
        {
            return DynamoSelection.Instance.Selection.Contains(connector.Start.Owner) && DynamoSelection.Instance.Selection.Contains(connector.End.Owner);
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalOutputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="codeBlockNode">The new Node To Code created Code Block Node</param>
        private void ReConnectOutputConnections(Dictionary<ConnectorModel, string> externalOutputConnections, CodeBlockNodeModel codeBlockNode)
        {
            foreach (var kvp in externalOutputConnections)
            {
                var connector = kvp.Key;
                string variableName = kvp.Value;
                int startIndex = 0, endIndex = 0;

                //Get the start and end idex for the ports for the connection
                endIndex = connector.End.Owner.InPorts.IndexOf(connector.End);
                int i = 0;
                for (i = 0; i < codeBlockNode.OutPorts.Count; i++)
                {
                    if (codeBlockNode.GetAstIdentifierForOutputIndex(i).Value == variableName)
                        break;
                }
                var portModel = codeBlockNode.OutPorts[i];
                startIndex = codeBlockNode.OutPorts.IndexOf(portModel);

                //Make the new connection and then record and add it
                var newConnector = ConnectorModel.Make(codeBlockNode, connector.End.Owner,
                    startIndex, endIndex, PortType.INPUT);

                this.Connectors.Add(newConnector);
                UndoRecorder.RecordCreationForUndo(newConnector);
            }
        }

        /// <summary>
        /// Forms new connections from the external nodes to the Node To Code Node,
        /// based on the connectors passed as inputs.
        /// </summary>
        /// <param name="externalInputConnections">List of connectors to remake, along with the port names of the new port</param>
        /// <param name="codeBlockNode">The new Node To Code created Code Block Node</param>
        private void ReConnectInputConnections(Dictionary<ConnectorModel, string> externalInputConnections, CodeBlockNodeModel codeBlockNode)
        {
            foreach (var kvp in externalInputConnections)
            {
                var connector = kvp.Key;
                string variableName = kvp.Value;
                int startIndex = 0, endIndex = 0;

                //Find the start and end index of the ports for the connection
                startIndex = connector.Start.Owner.OutPorts.IndexOf(connector.Start);
                endIndex = CodeBlockNodeModel.GetInportIndex(codeBlockNode, variableName);

                //For inputs, a single node can be an input to multiple nodes in the code block node selection
                //After conversion, all these connecetions should become only 1 connection and not many
                //Hence for inputs, it is required to make sure that a certain type of connection has not
                //been created already.
                if (Connectors.Where(x => (x.Start == connector.Start &&
                    x.End == codeBlockNode.InPorts[endIndex])).FirstOrDefault() == null)
                {
                    //Make the new connection and then record and add it
                    var newConnector = ConnectorModel.Make(connector.Start.Owner, codeBlockNode,
                        startIndex, endIndex, PortType.INPUT);

                    this.Connectors.Add(newConnector);
                    UndoRecorder.RecordCreationForUndo(newConnector);
                }
            }
        }
        #endregion

        /// <summary>
        /// Syncronously get a string representation of the workspace
        /// </summary>
        /// <returns></returns>
        internal string GetStringRepOfWorkspaceSync()
        {
            string outData = String.Empty;

            Action getString = (() =>
                {
                    // Create the xml document to write to.
                    var document = new XmlDocument();
                    document.CreateXmlDeclaration("1.0", null, null);
                    document.AppendChild(document.CreateElement("Workspace"));

                    //This is only used for computing relative offsets, it's not actually created
                    string virtualFileName = String.Join(Path.GetTempPath(), "DynamoTemp.dyn");
                    Dynamo.Nodes.Utilities.SetDocumentXmlPath(document, virtualFileName);

                    if (!this.PopulateXmlDocument(document))
                        return;

                    //Now unset the temp file name again
                    Dynamo.Nodes.Utilities.SetDocumentXmlPath(document, null);


                    outData = document.OuterXml;

                });

            if (dynSettings.Controller != null &&
                dynSettings.Controller.UIDispatcher != null &&
                dynSettings.Controller.UIDispatcher.CheckAccess() == false)
            {
                dynSettings.Controller.UIDispatcher.Invoke(getString);
                return outData;
            }
            else
                return String.Empty;

        }


    }
}

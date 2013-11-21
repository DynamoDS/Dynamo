using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Globalization;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using String = System.String;
using Dynamo.Core;

namespace Dynamo.Models
{
    public abstract class WorkspaceModel : NotificationObject, ILocatable, IUndoRedoRecorderClient
    {
        public static readonly double ZOOM_MAXIMUM = 4.0;
        public static readonly double ZOOM_MINIMUM = 0.1;

        #region Properties

        private string _fileName;
        private string _name;
        private double _height = 100;
        private double _width = 100;
        private double _x;
        private double _y;
        private double _zoom = 1.0;

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

        public bool IsCurrentSpace
        {
            get { return _isCurrentSpace; }
            set
            {
                _isCurrentSpace = value;
                RaisePropertyChanged("IsCurrentSpace");
            }
        }

        public event Action OnModified;


        private string _category = "";
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
        private DateTime _lastSaved;
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
        private string _author = "None provided";
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
        private string _description = "";
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
        private bool _hasUnsavedChanges;

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
            get { return ((null == undoRecorder) ? false : undoRecorder.CanUndo); }
        }

        /// <summary>
        ///     Determine if redo operation is currently possible.
        /// </summary>
        public bool CanRedo
        {
            get { return ((null == undoRecorder) ? false : undoRecorder.CanRedo); }
        }

        internal Version WorkspaceVersion { get; set; }

        
        
        public delegate void WorkspaceSavedEvent(WorkspaceModel model);
        public event WorkspaceSavedEvent WorkspaceSaved;

        /// <summary>
        ///     Defines whether this is the current space in Dynamo
        /// </summary>
        private bool _isCurrentSpace;

        private ObservableCollection<NodeModel> _nodes;
        private ObservableCollection<ConnectorModel> _connectors;

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
        internal UndoRedoRecorder UndoRecorder
        {
            get { return undoRecorder; }
        }

        #endregion

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


        public abstract void OnDisplayed();

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentWorkspace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="newPath">The path to save to</param>
        public virtual bool SaveAs(string newPath)
        {
            if (String.IsNullOrEmpty(newPath)) return false;

            DynamoLogger.Instance.Log("Saving " + newPath + "...");
            try
            {
                var xmlDoc = this.GetXml();
                xmlDoc.Save(newPath);
                this.FileName = newPath;

                this.OnWorkspaceSaved();
            }
            catch (Exception ex)
            {
                //Log(ex);
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
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
            return this.SaveAs(this.FileName);
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

        //TODO: Replace all Modified calls with RaisePropertyChanged-style system, that way observable collections can catch any changes
        public void DisableReporting()
        {
            Nodes.ToList().ForEach(x => x.DisableReporting());
        }

        public void EnableReporting()
        {
            Nodes.ToList().ForEach(x => x.EnableReporting());
        }

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
        internal void RecordModelForModification(ModelBase model)
        {
            if (null != model)
            {
                List<ModelBase> models = new List<ModelBase>();
                models.Add(model);
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
                this.undoRecorder.BeginActionGroup();
                this.undoRecorder.RecordCreationForUndo(model);
                this.undoRecorder.EndActionGroup();
            }
        }

        internal void RecordCreatedModels(List<ModelBase> models)
        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing created.

            this.undoRecorder.BeginActionGroup();
            foreach (ModelBase model in models)
                this.undoRecorder.RecordCreationForUndo(model);
            this.undoRecorder.EndActionGroup();
        }

        internal void RecordAndDeleteModels(List<ModelBase> models)
        {
            if (!ShouldProceedWithRecording(models))
                return; // There's nothing for deletion.

            // Gather a list of connectors first before the nodes they connect
            // to are deleted. We will have to delete the connectors first 
            // before 

            this.undoRecorder.BeginActionGroup(); // Start a new action group.

            foreach (ModelBase model in models)
            {
                if (model is NoteModel)
                {
                    // Take a snapshot of the note before it goes away.
                    this.undoRecorder.RecordDeletionForUndo(model);
                    this.Notes.Remove(model as NoteModel);
                }
                else if (model is NodeModel)
                {
                    // Just to make sure we don't end up deleting nodes from 
                    // another workspace (potentially two issues: the node was 
                    // having its "Workspace" pointing to another workspace, 
                    // or the selection set was not quite set up properly.
                    // 
                    NodeModel node = model as NodeModel;
                    System.Diagnostics.Debug.Assert(this == node.WorkSpace);

                    foreach (var conn in node.AllConnectors().ToList())
                    {
                        conn.NotifyConnectedPortsOfDeletion();
                        this.Connectors.Remove(conn);
                        this.undoRecorder.RecordDeletionForUndo(conn);
                    }

                    // Take a snapshot of the node before it goes away.
                    this.undoRecorder.RecordDeletionForUndo(model);

                    node.DisableReporting();
                    node.Destroy();
                    node.Cleanup();
                    node.WorkSpace.Nodes.Remove(node);
                }
                else if (model is ConnectorModel)
                {
                    ConnectorModel connector = model as ConnectorModel;
                    this.Connectors.Remove(connector);
                    this.undoRecorder.RecordDeletionForUndo(model);
                }
            }

            this.undoRecorder.EndActionGroup(); // Conclude the deletion.
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
                this.Notes.Remove(model as NoteModel);
            else if (model is ConnectorModel)
            {
                ConnectorModel connector = model as ConnectorModel;
                this.Connectors.Remove(connector);
                connector.NotifyConnectedPortsOfDeletion();
            }
            else if (model is NodeModel)
                this.Nodes.Remove(model as NodeModel);
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

            if (typeName.StartsWith("Dynamo.Nodes"))
            {
                NodeModel nodeModel = DynamoModel.CreateNodeInstance(typeName);
                nodeModel.WorkSpace = this;
                nodeModel.Deserialize(modelData, SaveContext.Undo);
                Nodes.Add(nodeModel);
            }
            else if (typeName.StartsWith("Dynamo.Models.ConnectorModel"))
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

        public virtual void Modified()
        {
            //DynamoLogger.Instance.Log("Workspace modified.");
            if (OnModified != null)
                OnModified();
        }

        public IEnumerable<NodeModel> GetHangingNodes()
        {
            return Nodes.Where(x => x.OutPortData.Any() && x.OutPorts.Any(y => !y.Connectors.Any()));
        }

        public IEnumerable<NodeModel> GetTopMostNodes()
        {
            return Nodes.Where(
                x =>
                    x.OutPortData.Any()
                    && x.OutPorts.Any(y => !y.Connectors.Any() || y.Connectors.Any(c => c.End.Owner is Output)));
        }

        public event EventHandler Updated;
        public void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
        }

        public virtual XmlDocument GetXml()
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);
                var root = xmlDoc.CreateElement("Workspace"); //write the root element
                root.SetAttribute("Version", this.WorkspaceVersion.ToString());
                root.SetAttribute("X", this.X.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Y", this.Y.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("zoom", this.Zoom.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Description", this.Description);
                root.SetAttribute("Category", this.Category);
                root.SetAttribute("Name", this.Name);

                xmlDoc.AppendChild(root);

                var elementList = xmlDoc.CreateElement("Elements");
                //write the root element
                root.AppendChild(elementList);

                foreach (var el in this.Nodes)
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

                foreach (var el in this.Nodes)
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
                foreach (var n in this.Notes)
                {
                    var note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.Text);
                    note.SetAttribute("x", n.X.ToString(CultureInfo.InvariantCulture));
                    note.SetAttribute("y", n.Y.ToString(CultureInfo.InvariantCulture));
                }

                return xmlDoc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        public void ReportPosition()
        {
            RaisePropertyChanged("Position");
        }

        internal void UpdateModelValue(Guid modelGuid, string name, string value)
        {
            ModelBase model = this.GetModelInternal(modelGuid);
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
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ViewModels.Core;
using Function = Dynamo.Graph.Nodes.CustomNodes.Function;

namespace Dynamo.ViewModels
{
    public delegate void NoteEventHandler(object sender, EventArgs e);
    public delegate void ViewEventHandler(object sender, EventArgs e);
    public delegate void SelectionEventHandler(object sender, SelectionBoxUpdateArgs e);
    public delegate void ViewModelAdditionEventHandler(object sender, ViewModelEventArgs e);
    public delegate void WorkspacePropertyEditHandler(WorkspaceModel workspace);

    public enum ShowHideFlags { Hide, Show };

    public partial class WorkspaceViewModel : ViewModelBase
    {
        #region constants
        /// <summary>
        /// Represents maximum value of workspace zoom
        /// </summary>
        [JsonIgnore]
        public const double ZOOM_MAXIMUM = 4.0;

        /// <summary>
        /// Represents minimum value of workspace zoom
        /// </summary>
        [JsonIgnore]
        public const double ZOOM_MINIMUM = 0.01;
        #endregion

        #region events 

        /// <summary>
        ///     Function that can be used to respond to a changed workspace Zoom amount.
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        public delegate void ZoomEventHandler(object sender, EventArgs e);

        /// <summary>
        ///     Event that is fired every time the zoom factor of a workspace changes.
        /// </summary>
        public event ZoomEventHandler ZoomChanged;

        /// <summary>
        /// Used during open and workspace changes to set the zoom of the workspace
        /// </summary>
        /// <param name="sender">The object which triggers the event</param>
        /// <param name="e">The zoom event data.</param>
        internal virtual void OnZoomChanged(object sender, ZoomEventArgs e)
        {
            if (ZoomChanged != null)
            {
                //Debug.WriteLine(string.Format("Setting zoom to {0}", e.Zoom));
                ZoomChanged(this, e);
            }
        }

        public event ZoomEventHandler RequestZoomToViewportCenter;
        /// <summary>
        /// For requesting registered workspace to zoom in center
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRequestZoomToViewportCenter(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportCenter != null)
            {
                RequestZoomToViewportCenter(this, e);
            }
        }

        public event ZoomEventHandler RequestZoomToViewportPoint;
        /// <summary>
        /// For requesting registered workspace to zoom in out from a point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnRequestZoomToViewportPoint(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportPoint != null)
            {
                RequestZoomToViewportPoint(this, e);
            }
        }

        public event ZoomEventHandler RequestZoomToFitView;
        /// <summary>
        /// For requesting registered workspace to zoom in or out to fitview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRequestZoomToFitView(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToFitView != null)
            {
                RequestZoomToFitView(this, e);
            }
        }

        public event NodeEventHandler RequestCenterViewOnElement;
        internal virtual void OnRequestCenterViewOnElement(object sender, ModelEventArgs e)
        {
            if (RequestCenterViewOnElement != null)
                RequestCenterViewOnElement(this, e);
        }

        public event ViewEventHandler RequestAddViewToOuterCanvas;
        public virtual void OnRequestAddViewToOuterCanvas(object sender, ViewEventArgs e)
        {
            if (RequestAddViewToOuterCanvas != null)
                RequestAddViewToOuterCanvas(this, e);
        }

        public event SelectionEventHandler RequestSelectionBoxUpdate;
        public virtual void OnRequestSelectionBoxUpdate(object sender, SelectionBoxUpdateArgs e)
        {
            if (RequestSelectionBoxUpdate != null)
                RequestSelectionBoxUpdate(this, e);
        }

        public event WorkspacePropertyEditHandler WorkspacePropertyEditRequested;
        public virtual void OnWorkspacePropertyEditRequested()
        {
            // extend this for all workspaces
            if (WorkspacePropertyEditRequested != null)
                WorkspacePropertyEditRequested(Model);
        }

        internal event Action<ShowHideFlags> RequestShowInCanvasSearch;

        private void OnRequestShowInCanvasSearch(object param)
        {
            var flag = (ShowHideFlags)param;
            RequestShowInCanvasSearch?.Invoke(flag);
        }

        internal event Action<object> RequestHideAllPopup;
        private void OnRequestHideAllPopup(object param)
        {
            RequestHideAllPopup?.Invoke(param);
        }

        internal event Action<ShowHideFlags> RequestNodeAutoCompleteSearch;
        internal event Action<ShowHideFlags, PortViewModel> RequestPortContextMenu;

        internal void OnRequestNodeAutoCompleteSearch(ShowHideFlags flag)
        {
            RequestNodeAutoCompleteSearch?.Invoke(flag);
        }

        internal void OnRequestPortContextMenu(ShowHideFlags flag, PortViewModel viewModel)
        {
            RequestPortContextMenu?.Invoke(flag, viewModel);
        }

        #endregion

        #region Properties and Fields

        [JsonIgnore]
        public DynamoViewModel DynamoViewModel { get; private set; }

        [JsonIgnore]
        public readonly WorkspaceModel Model;

        private bool _canFindNodesFromElements = false;

        [JsonIgnore]
        public PortViewModel portViewModel { get; set; }

        [JsonIgnore]
        public bool IsSnapping { get; set; }

        /// <summary>
        /// Gets the collection of Dynamo-specific preferences.
        /// This is used when serializing Dynamo preferences in the View block of Graph.Json.
        /// </summary>
        [JsonProperty("Dynamo")]
        public DynamoPreferencesData DynamoPreferences
        {
            get
            {
              bool hasRunWithoutCrash = false;
              string runType = RunType.Manual.ToString();
              string runPeriod = RunSettings.DefaultRunPeriod.ToString();
              HomeWorkspaceModel homeWorkspace = Model as HomeWorkspaceModel;
              if (homeWorkspace != null)
              {
                hasRunWithoutCrash = homeWorkspace.HasRunWithoutCrash;
                runType = homeWorkspace.RunSettings.RunType.ToString();
                runPeriod = homeWorkspace.RunSettings.RunPeriod.ToString();
              }

              bool isVisibleInDynamoLibrary = true;
              CustomNodeWorkspaceModel customNodeWorkspace = Model as CustomNodeWorkspaceModel;
              if (customNodeWorkspace != null)
                isVisibleInDynamoLibrary = customNodeWorkspace.IsVisibleInDynamoLibrary;

              return new DynamoPreferencesData(
                Model.ScaleFactor,
                hasRunWithoutCrash,
                isVisibleInDynamoLibrary,
                AssemblyHelper.GetDynamoVersion().ToString(),
                runType,
                runPeriod);
            }
        }

        /// <summary>
        /// Gets the Camera Data. This is used when serializing Camera Data in the View block
        /// of Graph.Json.
        /// </summary>
        [JsonProperty("Camera")]
        public CameraData Camera => DynamoViewModel.BackgroundPreviewViewModel?.GetCameraInformation() ?? new CameraData();

        /// <summary>
        /// ViewModel that is used in InCanvasSearch in context menu and called by Shift+DoubleClick.
        /// </summary>
        [JsonIgnore]
        public SearchViewModel InCanvasSearchViewModel { get; private set; }

        /// <summary>
        /// ViewModel that is used in NodeAutoComplete feature in context menu and called by Shift+DoubleClick.
        /// </summary>
        [JsonIgnore]
        public NodeAutoCompleteSearchViewModel NodeAutoCompleteSearchViewModel { get; private set; }

        /// <summary>
        /// Cursor Property Binding for WorkspaceView
        /// </summary>
        private Cursor currentCursor = null;
        [JsonIgnore]
        public Cursor CurrentCursor
        {
            get { return currentCursor; }
            set { currentCursor = value; RaisePropertyChanged("CurrentCursor"); }
        }

        /// <summary>
        /// Force Cursor Property Binding for WorkspaceView
        /// </summary>
        private bool isCursorForced = false;
        [JsonIgnore]
        public bool IsCursorForced
        {
            get { return isCursorForced; }
            set { isCursorForced = value; RaisePropertyChanged("IsCursorForced"); }
        }

        [JsonIgnore]
        public CompositeCollection WorkspaceElements { get; } = new CompositeCollection();
        [JsonIgnore]
        public ObservableCollection<ConnectorViewModel> Connectors { get; } = new ObservableCollection<ConnectorViewModel>();

        [JsonProperty("NodeViews")]
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();
        // Do not serialize notes, they will be converted to annotations during serialization
        [JsonIgnore]
        public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

        [JsonIgnore]
        public ObservableCollection<ConnectorPinViewModel> Pins { get; } = new ObservableCollection<ConnectorPinViewModel>();

        [JsonIgnore]
        public ObservableCollection<InfoBubbleViewModel> Errors { get; } = new ObservableCollection<InfoBubbleViewModel>();
        public ObservableCollection<AnnotationViewModel> Annotations { get; } = new ObservableCollection<AnnotationViewModel>();

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Model == DynamoViewModel.HomeSpace)
                    return "Home";
                return Model.Name;
            }
        }

        /// <summary>
        ///     Returns or set the X position of the workspace.
        /// </summary>
        public double X
        {
            get { return Model.X; }
            set
            {
                Model.X = value;
            }
        }

        /// <summary>
        ///     Returns or set the Y position of the workspace
        /// </summary>
        public double Y
        {
            get { return Model.Y; }
            set
            {
                Model.Y = value;
            }
        }

        [JsonIgnore]
        public string FileName
        {
            get { return Model.FileName; }
        }

        [JsonIgnore]
        public bool CanEditName
        {
            get { return Model != DynamoViewModel.HomeSpace; }
        }

        [JsonIgnore]
        public bool IsCurrentSpace
        {
            get { return Model == DynamoViewModel.CurrentSpace; }
        }

        /// <summary>
        /// Boolean indicating if the target workspace is home workspace (true), or custom node workspace (false)
        /// </summary>
        [JsonIgnore]
        public bool IsHomeSpace
        {
            get { return Model == DynamoViewModel.HomeSpace; }
        }

        [JsonIgnore]
        public bool HasUnsavedChanges
        {
            get { return Model.HasUnsavedChanges; }
            set { Model.HasUnsavedChanges = value; }
        }

        private ObservableCollection<Watch3DFullscreenViewModel> _watches = new ObservableCollection<Watch3DFullscreenViewModel>();
        [JsonIgnore]
        public ObservableCollection<Watch3DFullscreenViewModel> Watch3DViewModels
        {
            get { return _watches; }
            set
            {
                _watches = value;
                RaisePropertyChanged("Watch3DViewModels");
            }
        }

        /// <summary>
        ///     Get or set the zoom value of the workspace.
        /// </summary>
        public double Zoom
        {
            get { return this.Model.Zoom; }
            set
            {
                this.Model.Zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        [JsonIgnore]
        public bool CanZoomIn
        {
            get { return CanZoom(Configurations.ZoomIncrement); }
        }

        [JsonIgnore]
        public bool CanZoomOut
        {
            get { return CanZoom(-Configurations.ZoomIncrement); }
        }

        [JsonIgnore]
        public bool CanFindNodesFromElements
        {
            get { return _canFindNodesFromElements; }
            set
            {
                _canFindNodesFromElements = value;
                RaisePropertyChanged("CanFindNodesFromElements");
            }
        }

        [JsonIgnore]
        public bool CanShowInfoBubble
        {
            get { return stateMachine.IsInIdleState; }
        }

        [JsonIgnore]
        public bool CanRunNodeToCode
        {
            get
            {
                return true;
            }
        }

        [JsonIgnore]
        public Action FindNodesFromElements { get; set; }

        [JsonIgnore]
        public RunSettingsViewModel RunSettingsViewModel { get; protected set; }


        private GeometryScalingViewModel geoScalingViewModel;
        internal GeometryScalingViewModel GeoScalingViewModel
        {
            get
            {
                return geoScalingViewModel;
            }
        }

        #endregion

        public WorkspaceViewModel(WorkspaceModel model, DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;
            Model = model;
            stateMachine = new StateMachine(this);

            var nodesColl = new CollectionContainer { Collection = Nodes };
            WorkspaceElements.Add(nodesColl);

            var connColl = new CollectionContainer { Collection = Connectors };
            WorkspaceElements.Add(connColl);

            var notesColl = new CollectionContainer { Collection = Notes };
            WorkspaceElements.Add(notesColl);

            var pinsColl = new CollectionContainer { Collection = Pins };
            WorkspaceElements.Add(pinsColl);

            var errorsColl = new CollectionContainer { Collection = Errors };
            WorkspaceElements.Add(errorsColl);

            var annotationsColl = new CollectionContainer {Collection = Annotations};
            WorkspaceElements.Add(annotationsColl);

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection

            Model.NodeAdded += Model_NodeAdded;
            Model.NodeRemoved += Model_NodeRemoved;
            Model.NodesCleared += Model_NodesCleared;

            Model.NoteAdded += Model_NoteAdded;
            Model.NoteRemoved += Model_NoteRemoved;
            Model.NotesCleared += Model_NotesCleared;

            Model.AnnotationAdded += Model_AnnotationAdded;
            Model.AnnotationRemoved += Model_AnnotationRemoved;
            Model.AnnotationsCleared += Model_AnnotationsCleared;

            Model.ConnectorAdded += Connectors_ConnectorAdded;
            Model.ConnectorDeleted += Connectors_ConnectorDeleted;
            Model.PropertyChanged += ModelPropertyChanged;
            Model.PopulateJSONWorkspace += Model_PopulateJSONWorkspace;
            
            DynamoSelection.Instance.Selection.CollectionChanged += RefreshViewOnSelectionChange;

            DynamoViewModel.CopyCommand.CanExecuteChanged += CopyPasteChanged;
            DynamoViewModel.PasteCommand.CanExecuteChanged += CopyPasteChanged;



            // InCanvasSearchViewModel needs to happen before the nodes are created
            // as we rely upon it to retrieve node icon images
            if (!dynamoViewModel.Model.IsServiceMode)
            {
                InCanvasSearchViewModel = new SearchViewModel(DynamoViewModel)
                {
                    Visible = true
                };
                NodeAutoCompleteSearchViewModel = new NodeAutoCompleteSearchViewModel(DynamoViewModel)
                {
                    Visible = true
                };
            }

            // sync collections
            foreach (NodeModel node in Model.Nodes) Model_NodeAdded(node);
            foreach (NoteModel note in Model.Notes) Model_NoteAdded(note);
            foreach (AnnotationModel annotation in Model.Annotations) Model_AnnotationAdded(annotation);
            foreach (ConnectorModel connector in Model.Connectors) Connectors_ConnectorAdded(connector);
            
            NodeAutoCompleteSearchViewModel = new NodeAutoCompleteSearchViewModel(DynamoViewModel)
            {
                Visible = true
            };

            geoScalingViewModel = new GeometryScalingViewModel(this.DynamoViewModel);
            geoScalingViewModel.ScaleValue = this.DynamoViewModel.ScaleFactorLog;
        }
        /// <summary>
        /// This event is triggered from Workspace Model. Used in instrumentation
        /// </summary>
        /// <param name="modelData"> Workspace model data as JSON </param>
        /// <returns>workspace model with view block in string format</returns>
        private string Model_PopulateJSONWorkspace(JObject modelData)
        {
             var jsonData = AddViewBlockToJSON(modelData);
             return jsonData.ToString();
        }

        public override void Dispose()
        {
            Model.NodeAdded -= Model_NodeAdded;
            Model.NodeRemoved -= Model_NodeRemoved;
            Model.NodesCleared -= Model_NodesCleared;
            Model.NoteAdded -= Model_NoteAdded;
            Model.NoteRemoved -= Model_NoteRemoved;
            Model.NotesCleared -= Model_NotesCleared;

            Model.AnnotationAdded -= Model_AnnotationAdded;
            Model.AnnotationRemoved -= Model_AnnotationRemoved;
            Model.AnnotationsCleared -= Model_AnnotationsCleared;

            Model.ConnectorAdded -= Connectors_ConnectorAdded;
            Model.ConnectorDeleted -= Connectors_ConnectorDeleted;
            Model.PropertyChanged -= ModelPropertyChanged;
            Model.PopulateJSONWorkspace -= Model_PopulateJSONWorkspace;

            DynamoSelection.Instance.Selection.CollectionChanged -= RefreshViewOnSelectionChange;

            DynamoViewModel.CopyCommand.CanExecuteChanged -= CopyPasteChanged;
            DynamoViewModel.PasteCommand.CanExecuteChanged -= CopyPasteChanged;

            var nodeViewModels = Nodes.ToList();
            nodeViewModels.ForEach(nodeViewModel => nodeViewModel.Dispose());
            nodeViewModels.ForEach(nodeViewModel => this.unsubscribeNodeEvents(nodeViewModel));

            Notes.ToList().ForEach(noteViewModel => noteViewModel.Dispose());
            Connectors.ToList().ForEach(connectorViewmModel => connectorViewmModel.Dispose());
            Annotations.ToList().ForEach(AnnotationViewModel => AnnotationViewModel.Dispose());
            Nodes.Clear();
            Notes.Clear();
            Pins.Clear();
            Connectors.Clear();
            Errors.Clear();
            Annotations.Clear();
            InCanvasSearchViewModel?.Dispose();
            NodeAutoCompleteSearchViewModel?.Dispose();
        }

        internal void ZoomInInternal()
        {
            var args = new ZoomEventArgs(Configurations.ZoomIncrement);
            OnRequestZoomToViewportCenter(this, args);
            ResetFitViewToggle(null);
        }

        internal void ZoomOutInternal()
        {
            var args = new ZoomEventArgs(-Configurations.ZoomIncrement);
            OnRequestZoomToViewportCenter(this, args);
            ResetFitViewToggle(null);
        }

        /// <summary>
        /// WorkspaceViewModel's Save method does a two-part serialization. First, it serializes the Workspace,
        /// then adds a View property to serialized Workspace, and sets its value to the serialized ViewModel.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isBackup"></param>
        /// <param name="engine"></param>
        /// <param name="saveContext"></param>
        /// <exception cref="ArgumentNullException">Thrown when the file path is null.</exception>
        internal void Save(string filePath, bool isBackup = false, EngineController engine = null, SaveContext saveContext = SaveContext.None)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            try
            {
                if (!isBackup)
                {
                    Model.OnSaving(saveContext);
                }

                //set the name before serializing model.
                this.Model.setNameBasedOnFileName(filePath, isBackup);
                // Stage 1: Serialize the workspace.
                var json = Model.ToJson(engine);
                var json_parsed = JObject.Parse(json);

                // Stage 2: Add the View.
                var jo = AddViewBlockToJSON(json_parsed);

                // Stage 3: Save
                string saveContent;
                if(saveContext == SaveContext.SaveAs && !isBackup)
                {
                    // For intentional SaveAs either through UI or API calls, replace workspace elements' Guids and workspace Id
                    jo["Uuid"] = Guid.NewGuid().ToString();
                    saveContent = GuidUtility.UpdateWorkspaceGUIDs(jo.ToString());
                }
                else
                {
                    saveContent = jo.ToString();
                }
                File.WriteAllText(filePath, saveContent);

                // Handle Workspace or CustomNodeWorkspace related non-serialization internal logic
                // Only for actual save, update file path and recent file list
                if (!isBackup)
                {
                    Model.FileName = filePath;
                    Model.OnSaved();
                }

                // If a new CustomNodeWorkspaceModel is created, store that info in CustomNodeManager without creating an instance of the custom node.
                if (this.Model is CustomNodeWorkspaceModel customNodeWorkspaceModel)
                {
                    customNodeWorkspaceModel.SetInfo(Path.GetFileNameWithoutExtension(filePath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                throw (ex);
            }
        }
        /// <summary>
        /// This function appends view block to the model json
        /// </summary>
        /// <param name="modelData">Workspace Model data in JSON format</param>
        private JObject AddViewBlockToJSON(JObject modelData)
        {
            var token = JToken.Parse(this.ToJson());
            modelData.Add("View", token);

            return modelData;
        }

        /// <summary>
        /// Load the extra view information required to fully construct a WorkspaceModel object 
        /// </summary>
        /// <param name="json"></param>
        static public ExtraWorkspaceViewInfo ExtraWorkspaceViewInfoFromJson(string json)
        {
            JsonReader reader = new JsonTextReader(new StringReader(json));
            var obj = JObject.Load(reader);
            var viewBlock = obj["View"];
            if (viewBlock == null)
              return null;

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
               Culture = CultureInfo.InvariantCulture
           };

            return JsonConvert.DeserializeObject<ExtraWorkspaceViewInfo>(viewBlock.ToString(), settings);
        }

        void CopyPasteChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("CanPaste", "CanCopy", "CanCopyOrPaste");
            PasteCommand.RaiseCanExecuteChanged();
        }

        void Connectors_ConnectorAdded(ConnectorModel c)
        {
            var viewModel = new ConnectorViewModel(this, c);
            if (Connectors.All(x => x.ConnectorModel != c))
                Connectors.Add(viewModel);
        }

        void Connectors_ConnectorDeleted(ConnectorModel c)
        {
            var connector = Connectors.FirstOrDefault(x => x.ConnectorModel == c);
            if (connector != null)
            {
                Connectors.Remove(connector);
                connector.Dispose();
            }
        }

        private void Model_NoteAdded(NoteModel note)
        {
            var viewModel = new NoteViewModel(this, note);
            Notes.Add(viewModel);
        }

        private void Model_NoteRemoved(NoteModel note)
        {
            var matchingNoteViewModel = Notes.First(x => x.Model == note);
            Notes.Remove(matchingNoteViewModel);
            matchingNoteViewModel.Dispose();
        }

        private void Model_NotesCleared()
        {
            foreach (var noteViewModel in Notes)
            {
                noteViewModel.Dispose();
            }
            Notes.Clear();
        }

        private void Model_AnnotationAdded(AnnotationModel annotation)
        {
            var viewModel = new AnnotationViewModel(this, annotation);
            Annotations.Add(viewModel);
        }

        private void Model_AnnotationRemoved(AnnotationModel annotation)
        {
            var matchingAnnotation = Annotations.First(x => x.AnnotationModel == annotation);
            Annotations.Remove(matchingAnnotation);
            matchingAnnotation.Dispose();
           
        }

        private void Model_AnnotationsCleared()
        {
            foreach (var annotationViewModel in Annotations)
            {
                annotationViewModel.Dispose();
            }
            Annotations.Clear();
        }

        void Model_NodesCleared()
        {
            lock (Nodes)
            {
                foreach (var nodeViewModel in Nodes)
                {
                    this.unsubscribeNodeEvents(nodeViewModel);
                    nodeViewModel.Dispose();
                }
                Nodes.Clear();
            }
            Errors.Clear();

            PostNodeChangeActions();
        }

        private void unsubscribeNodeEvents(NodeViewModel nodeViewModel)
        {
            nodeViewModel.SnapInputEvent -= nodeViewModel_SnapInputEvent;
            nodeViewModel.NodeLogic.Modified -= OnNodeModified;
        }

        void Model_NodeRemoved(NodeModel node)
        {
            NodeViewModel nodeViewModel;
            lock (Nodes)
            {
                nodeViewModel = Nodes.First(x => x.NodeLogic == node);
                Errors.Remove(nodeViewModel.ErrorBubble);
                Nodes.Remove(nodeViewModel);
            }
            //unsub the events we attached below in NodeAdded.
            this.unsubscribeNodeEvents(nodeViewModel);
            nodeViewModel.Dispose();

            PostNodeChangeActions();
        }

        void Model_NodeAdded(NodeModel node)
        {
            var nodeViewModel = new NodeViewModel(this, node);
            nodeViewModel.SnapInputEvent += nodeViewModel_SnapInputEvent;
            nodeViewModel.NodeLogic.Modified += OnNodeModified;
            lock (Nodes)
            {
                Nodes.Add(nodeViewModel);
            }
            Errors.Add(nodeViewModel.ErrorBubble);
            
            PostNodeChangeActions();
        }

        void PostNodeChangeActions()
        {
            if (RunSettingsViewModel == null) return;
            CheckAndSetPeriodicRunCapability();
        }

        /// <summary>
        /// This is required here to compute the nodes delta state.
        /// This is overriden in HomeWorkspaceViewModel
        /// </summary>
        /// <param name="obj">The object.</param>
        public virtual void OnNodeModified(NodeModel obj)
        {
            
        }

        internal void CheckAndSetPeriodicRunCapability()
        {
            var periodUpdateAvailable = Model.Nodes.Any(n => n.CanUpdatePeriodically);
            RunSettingsViewModel.ToggleRunTypeEnabled(RunType.Periodic, periodUpdateAvailable);
        }

        /// <summary>
        /// Handles the port snapping on Mouse Enter.
        /// </summary>
        /// <param name="portViewModel">The port view model.</param>
        private void nodeViewModel_SnapInputEvent(PortViewModel portViewModel)
        {
            switch (portViewModel.EventType)
            {
                case PortEventType.MouseEnter:                    
                    IsSnapping = this.CheckActiveConnectorCompatibility(portViewModel);
                    this.portViewModel = portViewModel;
                    break;
                case PortEventType.MouseLeave:
                    IsSnapping = this.CheckActiveConnectorCompatibility(portViewModel, false);
                    this.portViewModel = portViewModel;
                    break;
                case PortEventType.MouseLeftButtonDown:
                    //If the connector is not active, then the state is changed to None. otherwise, the connector state is connection and 
                    //is not deleted from the view.
                    this.portViewModel = portViewModel;
                    if (this.CheckActiveConnectorCompatibility(portViewModel))
                    {
                        this.HandlePortClicked(portViewModel);
                    }
                    else
                    {
                        this.CancelActiveState();

                    }
                    break;
                default:
                    IsSnapping = this.CheckActiveConnectorCompatibility(portViewModel);
                    this.portViewModel = portViewModel;
                    break;

            }
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "X":
                    break;
                case "Y":
                    break;
                case "Zoom":
                    this.OnZoomChanged(this, new ZoomEventArgs(this.Zoom));
                    RaisePropertyChanged("Zoom");
                    break;
                case "IsCurrentSpace":
                    RaisePropertyChanged("IsCurrentSpace");
                    RaisePropertyChanged("IsHomeSpace");
                    break;
                case "HasUnsavedChanges":
                    RaisePropertyChanged("HasUnsavedChanges");
                    break;
                case "FileName":
                    RaisePropertyChanged("FileName");
                    break;
            }
        }

        internal void SelectAll(object parameter)
        {
            DynamoSelection.Instance.ClearSelection();
            Nodes.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele.NodeModel));
            Notes.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele.Model));
            Annotations.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele.AnnotationModel));
        }

        internal bool CanSelectAll(object parameter)
        {
            return true;
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

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            this.DynamoViewModel.ExecuteCommand(command);
        }

        internal bool CanNodeToCode(object parameters)
        {
            var nodeModels = DynamoSelection.Instance.Selection.OfType<NodeModel>();
            if (!nodeModels.Any() || nodeModels.Any(x => x.IsInErrorState))
            {
                return false;
            }
            return true;
        }

        internal void SelectInRegion(Rect2D region, bool isCrossSelect)
        {
            var fullyEnclosed = !isCrossSelect;
            var selection = DynamoSelection.Instance.Selection;
            var childlessModels = Model.Nodes
                .Concat<ModelBase>(Model.Notes)
                .Concat<ModelBase>(Pins.Select(c=>c.Model));

            foreach (var n in childlessModels)
            {
                // if target is within selection area but does not belong to a collapsed group
                if (IsInRegion(region, n, fullyEnclosed) && !IsCollapsed && !IsModelInCollapsedGroup(n))
                {
                    selection.AddUnique(n);
                }
                else if (n.IsSelected && !DynamoSelection.Instance.ClearSelectionDisabled) // only remove current selection if ClearSelectionDisabled flag is false
                {
                    selection.Remove(n);
                }
            }

            foreach (var n in Model.Annotations)
            {
                // if target is within selection area but does not belong to a collapsed group
                if (IsInRegion(region, n, fullyEnclosed) && !IsCollapsed && !IsModelInCollapsedGroup(n))
                {
                    selection.AddUnique(n);
                    // if annotation is selected its children should be added to selection too
                    foreach (var m in n.Nodes)
                    {
                        if (m is AnnotationModel nestedGroup)
                        {
                            foreach (var model in nestedGroup.Nodes)
                            {
                                selection.AddUnique(model);
                            }
                        }
                        selection.AddUnique(m);
                    }
                }
                // only remove current selection if ClearSelectionDisabled flag is false
                // This prevents group getting removed when user press shift to add more groups
                else if (n.IsSelected && !Model.Annotations.ContainsModel(n)
                    && !DynamoSelection.Instance.ClearSelectionDisabled)
                {
                    selection.Remove(n);
                }
            }
        }

        /// <summary>
        /// Determine if a Dynamo element belongs to a collapsed group or sub group of a collapsed group
        /// </summary>
        /// <param name="model">Target node, note, annotation</param>
        /// <returns></returns>
        private bool IsModelInCollapsedGroup(ModelBase model)
        {
            bool IsInCollapsedGroup = false;
            // Check all the collapsed groups and their sub groups
            foreach (var group in Model.Annotations.Where(x => !x.IsExpanded))
            {
                if (group.Nodes.Contains(model))
                {
                    IsInCollapsedGroup = true;
                    break;
                }
                foreach (var nestGroup in group.Nodes.OfType<AnnotationModel>())
                {
                    if (nestGroup.Nodes.Contains(model))
                    {
                        IsInCollapsedGroup = true;
                        break;
                    }
                }
            }
            return IsInCollapsedGroup;
        }

        private static bool IsInRegion(Rect2D region, ILocatable locatable, bool fullyEnclosed)
        {
            double x0 = locatable.X;
            double y0 = locatable.Y;

            if (false == fullyEnclosed) // Cross selection.
            {
                var test = new Rect2D(x0, y0, locatable.Width, locatable.Height);
                return region.IntersectsWith(test);
            }

            double x1 = x0 + locatable.Width;
            double y1 = y0 + locatable.Height;
            return (region.Contains(x0, y0) && region.Contains(x1, y1));
        }

        public double GetSelectionAverageX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterX)
                           .Average();
        }

        public double GetSelectionAverageY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterY)
                           .Average();
        }

        public double GetSelectionMinX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Min();
        }

        public double GetSelectionMinY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Min();
        }

        public double GetSelectionMaxX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X + x.Width)
                           .Max();
        }

        public double GetSelectionMaxLeftX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Max();
        }

        public double GetSelectionMaxY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y + x.Height)
                           .Max();
        }

        public double GetSelectionMaxTopY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => !(x is AnnotationModel) && x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Max();
        }

        public void AlignSelected(object parameter)
        {
            string alignType = parameter.ToString();

            if (DynamoSelection.Instance.Selection.Count <= 1) return;

            // All the models in the selection will be modified, 
            // record their current states before anything gets changed.
            SmartObservableCollection<ISelectable> selection = DynamoSelection.Instance.Selection;
            IEnumerable<ModelBase> models = selection.OfType<ModelBase>();
            WorkspaceModel.RecordModelsForModification(models.ToList(), Model.UndoRecorder);

            var toAlign = DynamoSelection.Instance.Selection.OfType<ILocatable>().Where(node => !(node is AnnotationModel)).ToList();

            switch (alignType)
            {
                case "HorizontalCenter":
                {
                    var xAll = GetSelectionAverageX();
                    toAlign.ForEach((x) => { x.CenterX = xAll; });
                }
                    break;
                case "HorizontalLeft":
                    {
                        var xAll = GetSelectionMinX();
                        toAlign.ForEach((x) =>
                        {
                            if (x is ConnectorPinModel pin)
                            {
                                x.X = xAll - ConnectorPinViewModel.OneThirdWidth;
                            }
                            else
                            {
                                x.X = xAll;
                            }
                        });
                    }
                    break;
                case "HorizontalRight":
                    {
                        var xAll = GetSelectionMaxX();
                        toAlign.ForEach((x) =>
                        {
                            if (x is ConnectorPinModel pin)
                            {
                                x.X = xAll - ConnectorPinViewModel.OneThirdWidth * 4;
                            }
                            else
                            {
                                x.X = xAll - x.Width;
                            }
                        });
                    }
                    break;
                case "VerticalCenter":
                {
                    var yAll = GetSelectionAverageY();
                    toAlign.ForEach((x) => { x.CenterY = yAll; });
                }
                    break;
                case "VerticalTop":
                    {
                        var yAll = GetSelectionMinY();
                        toAlign.ForEach((x) =>
                        {
                            if (x is ConnectorPinModel pin)
                            {
                                x.Y = yAll + ConnectorPinViewModel.OneThirdWidth;
                            }
                            else
                            {
                                x.Y = yAll;
                            }
                        });
                    }
                    break;
                case "VerticalBottom":
                    {
                        var yAll = GetSelectionMaxY();
                        toAlign.ForEach((x) =>
                        {
                            if (x is ConnectorPinModel pin)
                            {
                                x.Y = yAll - ConnectorPinViewModel.OneThirdWidth*2;
                            }
                            else
                            {
                                x.Y = yAll - x.Height;
                            }
                        });
                    }
                    break;
                case "VerticalDistribute":
                {
                    var nodesSelected = DynamoSelection.Instance.Selection.Where(node => !(node is AnnotationModel) && node is ILocatable);
                    if (nodesSelected.Count() <= 2) return;

                    var yMin = GetSelectionMinY();
                    var yMax = GetSelectionMaxY();

                    var spacing = 0.0;
                    var span = yMax - yMin;

                    var nodeHeightSum =
                        nodesSelected.Where(y => y is ILocatable)
                            .Cast<ILocatable>()
                            .Sum((y) => y.Height);

                    if (span > nodeHeightSum)
                    {
                        spacing = (span - nodeHeightSum)
                            /(nodesSelected.Count() - 1);
                    }

                    var cursor = yMin;
                    foreach (var node in toAlign.OrderBy(y => y.Y))
                    {
                        node.Y = cursor;
                        cursor += node.Height + spacing;
                    }
                }
                    break;
                case "HorizontalDistribute":
                {
                    var nodesSelected = DynamoSelection.Instance.Selection.Where(node => !(node is AnnotationModel) && node is ILocatable);
                    if (nodesSelected.Count() <= 2) return;

                    var xMin = GetSelectionMinX();
                    var xMax = GetSelectionMaxX();

                    var spacing = 0.0;
                    var span = xMax - xMin;
                    var nodeWidthSum =
                        nodesSelected.Where((x) => x is ILocatable)
                            .Cast<ILocatable>()
                            .Sum((x) => x.Width);

                    // If there is more span than total node width,
                    // distribute the nodes with a gap. If not, leave
                    // the spacing at 0 and the nodes will distribute
                    // up against each other.
                    if (span > nodeWidthSum)
                    {
                        spacing = (span - nodeWidthSum)
                            /(nodesSelected.Count() - 1);
                    }

                    var cursor = xMin;
                    foreach (var node in toAlign.OrderBy(x => x.X))
                    {
                        node.X = cursor;
                        cursor += node.Width + spacing;
                    }
                }
                    break;
            }

            toAlign.ForEach(x => x.ReportPosition());
        }

        private static bool CanAlignSelected(object parameter)
        {
            return DynamoSelection.Instance.Selection.Count > 1;
        }
        
        private void Paste(object param)
        {
            var point = InCanvasSearchViewModel.InCanvasSearchPosition;
            DynamoViewModel.Model.Paste(new Point2D(point.X, point.Y), false);
            DynamoViewModel.RaiseCanExecuteUndoRedo();
        }

        private void ShowHideAllGeometryPreview(object parameter)
        {
            var modelGuids = DynamoSelection.Instance.Selection.
                OfType<NodeModel>().Select(n => n.GUID);

            if (!modelGuids.Any())
                return;

            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty,
                modelGuids, "IsVisible", (string) parameter);

            DynamoViewModel.Model.ExecuteCommand(command);
            RefreshViewOnSelectionChange(this,null);
        }

        private void SetArgumentLacing(object parameter)
        {
            var modelGuids = DynamoSelection.Instance.Selection
                .OfType<NodeModel>()
                .Where(n => n.ArgumentLacing != LacingStrategy.Disabled)
                .Select(n => n.GUID);

            if (!modelGuids.Any())
                return;

            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty,
                modelGuids, "ArgumentLacing", (string) parameter);

            DynamoViewModel.Model.ExecuteCommand(command);
            RaisePropertyChanged("SelectionArgumentLacing");
        }

        private void Hide(object parameters)
        {
            // Closing of custom workspaces will simply close those workspaces,
            // but closing Home workspace has a different meaning. First off, 
            // Home workspace cannot be closed or hidden, it can only be cleared.
            // As of this revision, pressing the "X" button on Home workspace 
            // tab simply clears the Home workspace, and bring up the Start Page
            // if there are no other custom workspace that is opened.
            // 

            if (IsHomeSpace)
            {
                if (DynamoViewModel.CloseHomeWorkspaceCommand.CanExecute(null))
                    DynamoViewModel.CloseHomeWorkspaceCommand.Execute(null);
            }
            else
            {
                if (!Model.HasUnsavedChanges || DynamoViewModel.AskUserToSaveWorkspaceOrCancel(Model))
                    DynamoViewModel.Model.RemoveWorkspace(Model);
            }
        }

        private static bool CanHide(object parameters)
        {
            // Workspaces other than HOME can be hidden (i.e. closed), but we 
            // are enabling it also for the HOME workspace. When clicked, the 
            // HOME workspace is cleared (i.e. equivalent of pressing the New 
            // button), and if there is no other workspaces opened, then the 
            // Start Page is displayed.
            // 
            return true;
        }

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            if (Model.X != p.X || Model.Y != p.Y)
            {
                Model.X = p.X;
                Model.Y = p.Y;
            }
        }

        private static bool CanSetCurrentOffset(object parameter)
        {
            return true;
        }

        private void CreateNodeFromSelection(object parameter)
        {
            CollapseSelectedNodes();
        }

        private void AlignSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AlignSelectedCommand.RaiseCanExecuteChanged();

        }

        private static bool CanCreateNodeFromSelection(object parameter)
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>().Any();
        }

        private bool CanZoom(double zoom)
        {
            return (!(zoom < 0) || !(this.Zoom <= WorkspaceViewModel.ZOOM_MINIMUM)) && (!(zoom > 0) 
                || !(this.Zoom >= WorkspaceViewModel.ZOOM_MAXIMUM));
        }

        private void SetZoom(object zoom)
        {
            this.Zoom = Convert.ToDouble(zoom);
        }

        private static bool CanSetZoom(object zoom)
        {
            double setZoom = Convert.ToDouble(zoom);
            return setZoom >= WorkspaceViewModel.ZOOM_MINIMUM && setZoom <= WorkspaceViewModel.ZOOM_MAXIMUM;
        }

        private bool _fitViewActualZoomToggle = false;

        /// <summary>
        ///     Zoom around current selection
        ///     _fitViewActualZoomToggle is used internally to toggle
        /// between the default 1.0 zoom level and the intended zoom around selection
        ///     The optional toggle boolean is introduced to avoid this behavior and only zoom around the selection
        /// no matter how many times the operation is performed
        /// </summary>
        /// <param name="toggle"></param>
        internal void FitViewInternal(bool toggle = true)
        {
            // Get the offset and focus width & height (zoom if 100%)
            double minX, maxX, minY, maxY;

            // Get the width and height of area to fit
            if (DynamoSelection.Instance.Selection.Count > 0)
            {   // has selection
                minX = GetSelectionMinX();
                maxX = GetSelectionMaxX();
                minY = GetSelectionMinY();
                maxY = GetSelectionMaxY();
            }
            else
            {   
                // no selection, fitview all nodes and notes
                var nodes = Nodes.Select(x => x.NodeModel);
                var notes = Notes.Select(x => x.Model);
                var models = nodes.Concat<ModelBase>(notes);

                if (!models.Any()) return;

                // initialize to the first model (either note or node) on the list 

                var firstModel = models.First();
                minX = firstModel.X;
                maxX = firstModel.X;
                minY = firstModel.Y;
                maxY = firstModel.Y;

                foreach (var model in models)
                {
                    //calculates the min and max positions of both x and y coords of all nodes and notes
                    minX = Math.Min(model.X, minX);
                    maxX = Math.Max(model.X + model.Width, maxX);
                    minY = Math.Min(model.Y, minY);
                    maxY = Math.Max(model.Y + model.Height, maxY);
                }
                
            }

            var offset = new Point2D(minX, minY);
            double focusWidth = maxX - minX;
            double focusHeight = maxY - minY;

            ZoomEventArgs zoomArgs;

            if (toggle)
            {
                _fitViewActualZoomToggle = !_fitViewActualZoomToggle;
                zoomArgs = _fitViewActualZoomToggle && toggle
                    ? new ZoomEventArgs(offset, focusWidth, focusHeight)
                    : new ZoomEventArgs(offset, focusWidth, focusHeight, 1.0);
            }
            else
            {
                zoomArgs = new ZoomEventArgs(offset, focusWidth, focusHeight);
            }

            OnRequestZoomToFitView(this, zoomArgs);
        }

        internal void ResetFitViewToggle(object o)
        {
            _fitViewActualZoomToggle = false;
        }

        private static bool CanResetFitViewToggle(object o)
        {
            return true;
        }

        /// <summary>
        /// Selects an Element by ID and focuses the view around it
        /// </summary>
        /// <param name="id"></param>
        private void FocusNode(object id)
        {
            try
            {
                var node = DynamoViewModel.Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == id.ToString());
              
                //select the element
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(node);

                DynamoViewModel.ShowElement(node, false);
            }
            catch
            {
                DynamoViewModel.Model.Logger.Log(Wpf.Properties.Resources.MessageFailedToFindNodeById);
            }
        }

        private static bool CanFocusNode(object id)
        {
            return !string.IsNullOrEmpty(id.ToString());
        }

        private void FindById(object id)
        {
            try
            {
                var node = DynamoViewModel.Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == id.ToString());

                if (node != null)
                {
                    //select the element
                    DynamoSelection.Instance.ClearSelection();
                    DynamoSelection.Instance.Selection.Add(node);

                    //focus on the element
                    DynamoViewModel.ShowElement(node);

                    return;
                }
            }
            catch
            {
                DynamoViewModel.Model.Logger.Log(Wpf.Properties.Resources.MessageFailedToFindNodeById);
            }

            try
            {
                var function =
                    (Function)DynamoViewModel.Model.CurrentWorkspace.Nodes.First(x => x is Function && ((Function)x).Definition.FunctionId.ToString() == id.ToString());

                if (function == null) return;

                //select the element
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(function);

                //focus on the element
                DynamoViewModel.ShowElement(function);
            }
            catch
            {
                DynamoViewModel.Model.Logger.Log(Wpf.Properties.Resources.MessageFailedToFindNodeById);
            }
        }

        private static bool CanFindById(object id)
        {
            return !string.IsNullOrEmpty(id.ToString());
        }

        private void FindNodesFromSelection(object parameter)
        {
            FindNodesFromElements();
        }

        private bool CanFindNodesFromSelection(object parameter)
        {
            return FindNodesFromElements != null;
        }

        private void DoGraphAutoLayout(object o)
        {
            Model.DoGraphAutoLayout();
            DynamoViewModel.RaiseCanExecuteUndoRedo();

            Dynamo.Logging.Analytics.TrackCommandEvent("GraphLayout");
        }

        private static bool CanDoGraphAutoLayout(object o)
        {
            return true;
        }

        private void ShowAllWires(object o)
        {
            var nodeModels = DynamoSelection.Instance.Selection.OfType<NodeModel>().Where(n => n.AllConnectors.Any(x => x.IsHidden)).ToList();
            ShowHideAllWires(nodeModels, false);
        }

        private bool CanShowAllWires(object o)
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>()
                .Any(n => n.AllConnectors.Any(x => x.IsHidden));
        }

        private void HideAllWires(object o)
        {
            var nodeModels = DynamoSelection.Instance.Selection.OfType<NodeModel>().Where(n => n.AllConnectors.Any(x => !x.IsHidden)).ToList();
            ShowHideAllWires(nodeModels, true);

        }

        private bool CanHideAllWires(object o)
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>()
                .Any(n => n.AllConnectors.Any(x => !x.IsHidden));
        }

        /// <summary>
        /// Shows or Hides all wires of a list of nodeModels
        /// </summary>
        /// <param name="nodeModels"></param>
        /// <param name="isHidden"></param>
        private void ShowHideAllWires(List<NodeModel> nodeModels, bool isHidden)
        {
            if (!nodeModels.Any()) return;

            foreach (var nodeModel in nodeModels)
            {
                var connectors = nodeModel.AllConnectors;
                foreach (var connector in connectors)
                {
                    if (connector != null)
                        connector.IsHidden = isHidden;

                }
            }
        }

        /// <summary>
        /// Collapse a set of nodes and notes currently selected in workspace
        /// </summary>
        internal void CollapseSelectedNodes()
        {
            var args = new FunctionNamePromptEventArgs();
            DynamoViewModel.Model.OnRequestsFunctionNamePrompt(null, args);

            if (!args.Success)
                return;

            var selectedNodes = DynamoSelection.Instance.Selection.OfType<NodeModel>();
            var selectedNotes = DynamoSelection.Instance.Selection.OfType<NoteModel>();

            DynamoViewModel.Model.AddCustomNodeWorkspace(
                DynamoViewModel.Model.CustomNodeManager.Collapse(selectedNodes,
                selectedNotes, Model, DynamoModel.IsTestMode, args));

            Dynamo.Logging.Analytics.TrackCommandEvent("NewCustomNode",
                "NodeCount", selectedNodes.Count());
        }

        internal void Loaded()
        {
            RaisePropertyChanged("IsHomeSpace");

            // New workspace or swapped workspace to follow it offset and zoom
            this.Model.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(this.X, this.Y)));
            this.OnZoomChanged(this, new ZoomEventArgs(this.Zoom));
        }

        private void RefreshViewOnSelectionChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            AlignSelectedCommand.RaiseCanExecuteChanged();
            ShowHideAllGeometryPreviewCommand.RaiseCanExecuteChanged();
            SetArgumentLacingCommand.RaiseCanExecuteChanged();     
            ShowAllWiresCommand.RaiseCanExecuteChanged();
            HideAllWiresCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("HasSelection");
            RaisePropertyChanged("IsGeometryOperationEnabled");
            RaisePropertyChanged("AnyNodeVisible");
            RaisePropertyChanged("SelectionArgumentLacing");            
        }

        /// <summary>
        /// Returns ViewModelBase by GUID
        /// </summary>
        /// <param name="modelGuid">Identifier of the requested model.</param>
        /// <returns>Found <see cref="ViewModelBase"/> object.</returns>
        internal ViewModelBase GetViewModelInternal(Guid modelGuid)
        {
            ViewModelBase foundModel = (Connectors.FirstOrDefault(c => c.ConnectorModel.GUID == modelGuid)
                ?? Nodes.FirstOrDefault(node => node.NodeModel.GUID == modelGuid) as ViewModelBase)
                ?? (Notes.FirstOrDefault(note => note.Model.GUID == modelGuid)
                ?? Annotations.FirstOrDefault(annotation => annotation.AnnotationModel.GUID == modelGuid) as ViewModelBase);

            return foundModel;
        }

        /// <summary>
        /// Gets viewModels by their GUIDs
        /// </summary>
        /// <param name="modelGuids">Identifiers of the requested models.</param>
        /// <returns>All found <see cref="ViewModelBase"/> objects.</returns>
        internal IEnumerable<ViewModelBase> GetViewModelsInternal(IEnumerable<Guid> modelGuids)
        {
            var foundModels = new List<ViewModelBase>();

            foreach (var modelGuid in modelGuids)
            {
                var foundModel = GetViewModelInternal(modelGuid);
                if (foundModel != null)
                    foundModels.Add(foundModel);
            }

            return foundModels;
        }

        
    }

    public class ViewModelEventArgs : EventArgs
    {
        public NodeViewModel ViewModel { get; set; }
        public ViewModelEventArgs(NodeViewModel vm)
        {
            ViewModel = vm;
        }
    }
}

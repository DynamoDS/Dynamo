using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using Dynamo.Core;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Wpf.Properties;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class Watch3DViewModelStartupParams
    {
        public IDynamoModel Model { get; set; }
        public IScheduler Scheduler { get; set; }
        public ILogger Logger { get; set; }
        public IPreferences Preferences { get; set; }
        public IEngineControllerManager EngineControllerManager { get; set; }

        public Watch3DViewModelStartupParams()
        {
            
        }

        public Watch3DViewModelStartupParams(DynamoModel model)
        {
            Model = model;
            Scheduler = model.Scheduler;
            Logger = model.Logger;
            Preferences = model.PreferenceSettings;
            EngineControllerManager = model;
        }
    }

    /// <summary>
    /// The DefaultWatch3DViewModel is the base class for all 3D previews in Dynamo.
    /// Classes which derive from this base are used to prepare geometry for 
    /// rendering by various render targets. The base class handles the registration
    /// of all necessary event handlers on models, workspaces, and nodes.
    /// </summary>
    public class DefaultWatch3DViewModel : NotificationObject, IWatch3DViewModel, IDisposable
    {
        protected readonly NodeModel watchModel;

        protected readonly IDynamoModel dynamoModel;
        protected readonly IScheduler scheduler;
        protected readonly IPreferences preferences;
        protected readonly ILogger logger;
        protected readonly IEngineControllerManager engineManager;
        protected IRenderPackageFactory renderPackageFactory;
        protected IDynamoViewModel viewModel;

        protected List<NodeModel> recentlyAddedNodes = new List<NodeModel>();
        protected bool active;
        protected bool isGridVisible;

        /// <summary>
        /// Represents the name of current Watch3DViewModel which will be saved in preference settings
        /// </summary>
        public virtual string PreferenceWatchName { get { return "IsBackgroundPreviewActive"; } }

        /// <summary>
        /// A flag which indicates whether this Watch3DView should process
        /// geometry updates. When set to False, the Watch3DView corresponding
        /// to this view model is not displayed.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (active == value)
                {
                    return;
                }

                active = value;
                preferences.SetIsBackgroundPreviewActive(PreferenceWatchName, value);               
                RaisePropertyChanged("Active");

                OnActiveStateChanged();

                if (active)
                {
                    RegenerateAllPackages();
                }
            }
        }

        /// <summary>
        /// A flag indicating whether the grid is visible in 3D.
        /// </summary>
        public virtual bool IsGridVisible
        {
            get { return isGridVisible; }
            set
            {
                if (isGridVisible == value) return;

                isGridVisible = value;
                preferences.IsBackgroundGridVisible = value;
                RaisePropertyChanged("IsGridVisible");
            }
        }

        /// <summary>
        /// A name which identifies this view model when multiple
        /// Watch3DViewModel objects exist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A flag which indicates whether this view model is used for a background preview.
        /// </summary>
        public virtual bool IsBackgroundPreview 
        {
            get { return true; }
        }

        private bool canNavigateBackground = false;
        public bool CanNavigateBackground
        {
            get
            {
                return canNavigateBackground || navigationKeyIsDown;
            }
            set
            {
                canNavigateBackground = value;
                Dynamo.Logging.Analytics.TrackScreenView(canNavigateBackground ? "Geometry" : "Nodes");
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

        /// <summary>
        /// A flag which indicates whether Isolate Selected Geometry mode is activated.
        /// </summary>
        private bool isolationMode;
        public bool IsolationMode
        {
            get
            {
                return isolationMode;
            }
            set
            {
                isolationMode = value;
                RaisePropertyChanged("IsolationMode");
            }
        }

        /// <summary>
        /// A flag which indicates whether the user is holding the
        /// navigation override key (ESC).
        /// </summary>
        private bool navigationKeyIsDown = false;
        public bool NavigationKeyIsDown
        {
            get { return navigationKeyIsDown; }
            set
            {
                if (navigationKeyIsDown == value) return;

                navigationKeyIsDown = value;
                RaisePropertyChanged("NavigationKeyIsDown");
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

        public DelegateCommand TogglePanCommand { get; set; }

        public DelegateCommand ToggleOrbitCommand { get; set; }

        public DelegateCommand ToggleCanNavigateBackgroundCommand { get; set; }

        public DelegateCommand ToggleIsolationModeCommand { get; set; }

        public DelegateCommand ZoomToFitCommand { get; set; }

        internal WorkspaceViewModel CurrentSpaceViewModel
        {
            get
            {
                return viewModel.Workspaces.FirstOrDefault(vm => vm.Model == dynamoModel.CurrentWorkspace);
            }
        }

        /// <summary>
        /// A flag which indicates whether the Watch3DViewModel
        /// is currently in pan mode.
        /// </summary>
        public bool IsPanning
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsPanning;
            }
        }

        /// <summary>
        /// A flag which indicates whether the Watch3DViewModel
        /// is currently in orbit mode.
        /// </summary>
        public bool IsOrbiting
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsOrbiting;
            }
        }

        public bool CanBeActivated { get; internal set; }

        /// <summary>
        /// The DefaultWatch3DViewModel is used in contexts where a complete rendering environment
        /// cannot be established. Typically, this is machines that do not have GPUs, or do not
        /// support DirectX 10 feature levels. For most purposes, you will want to use a <see cref="HelixWatch3DViewModel"/>
        /// </summary>
        /// <param name="model">The NodeModel that this watch is displaying.</param>
        /// <param name="parameters">A Watch3DViewModelStartupParams object.</param>
        public DefaultWatch3DViewModel(NodeModel model, Watch3DViewModelStartupParams parameters)
        {
            watchModel = model;
            dynamoModel = parameters.Model;
            scheduler = parameters.Scheduler;
            preferences = parameters.Preferences;
            logger = parameters.Logger;
            engineManager = parameters.EngineControllerManager;

            Name = Resources.BackgroundPreviewDefaultName;
            isGridVisible = parameters.Preferences.IsBackgroundGridVisible;
            active = parameters.Preferences.IsBackgroundPreviewActive;
            logger = parameters.Logger;

            RegisterEventHandlers();

            TogglePanCommand = new DelegateCommand(TogglePan, CanTogglePan);
            ToggleOrbitCommand = new DelegateCommand(ToggleOrbit, CanToggleOrbit);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
            ToggleIsolationModeCommand = new DelegateCommand(ToggleIsolationMode, CanToggleIsolationMode);
            ZoomToFitCommand = new DelegateCommand(ZoomToFit, CanZoomToFit);
            CanBeActivated = true;
        }

        /// <summary>
        /// Call setup to establish the visualization context for the
        /// Watch3DViewModel. Because the Watch3DViewModel is passed into the DynamoViewModel,
        /// Setup is required to fully establish the rendering context. 
        /// </summary>
        /// <param name="viewModel">An IDynamoViewModel object.</param>
        /// <param name="renderPackageFactory">An IRenderPackageFactory object.</param>
        public void Setup(IDynamoViewModel viewModel, 
            IRenderPackageFactory renderPackageFactory)
        {
            this.viewModel = viewModel;
            this.renderPackageFactory = renderPackageFactory;
        }

        protected virtual void OnShutdown()
        {
            // Override in inherited classes.
        }

        
        protected virtual void OnClear()
        {
            // Override in inherited classes.
        }

        protected virtual void OnActiveStateChanged()
        {
            UnregisterEventHandlers();
            OnClear();
        }

        private void RegisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionChangedHandler;
            PropertyChanged += OnPropertyChanged;

            LogVisualizationCapabilities();

            RegisterModelEventhandlers(dynamoModel);

            RegisterWorkspaceEventHandlers(dynamoModel);

            AttachedProperties.RequestResetColorsForDynamoGeometryModel += AttachedProperties_RequestResetColorsForDynamoGeometryModel;
        }

        /// <summary>
        /// Handle requests to reset colors on geometry graphics objects given an id.
        /// </summary>
        /// <param name="objId">id of the object</param>
        protected virtual void AttachedProperties_RequestResetColorsForDynamoGeometryModel(string objId)
        {
            //override in derived classes
        }

        /// <summary>
        /// Event to be handled when the background preview is toggled on or off
        /// On/off state is passed using the bool parameter
        /// </summary>
        public event Action<bool> CanNavigateBackgroundPropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "CanNavigateBackground":
                    var handler = CanNavigateBackgroundPropertyChanged;
                    if (handler != null)
                    {
                        handler(CanNavigateBackground);
                    }
                    break;
                case "IsolationMode":
                    OnIsolationModeRequestUpdate();
                    break;
            }
        }

        protected virtual void OnIsolationModeRequestUpdate()
        {
            // Override in inherited classes.
        }

        protected void UnregisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionChangedHandler;

            UnregisterModelEventHandlers(dynamoModel);

            UnregisterWorkspaceEventHandlers(dynamoModel);
        }

        private void OnModelShutdownStarted(IDynamoModel dynamoModel)
        {
            UnregisterEventHandlers();
            OnShutdown();
        }

        private void LogVisualizationCapabilities()
        {
            var renderingTier = (RenderCapability.Tier >> 16);
            var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
            var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
            var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
            var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

            logger.Log(string.Format("RENDER : Rendering Tier: {0}", renderingTier));
            logger.LogError(string.Format("RENDER : Pixel Shader 3 Supported: {0}", pixelShader3Supported));
            logger.Log(string.Format("RENDER : Pixel Shader 4 Supported: {0}", pixelShader4Supported));
            logger.Log(string.Format("RENDER : Software Effect Rendering Supported: {0}", softwareEffectSupported));
            logger.Log(string.Format("RENDER : Maximum hardware texture size: {0}", maxTextureSize));
        }

        private void RegisterModelEventhandlers(IDynamoModel dynamoModel)
        {
            dynamoModel.WorkspaceCleared += OnWorkspaceCleared;
            dynamoModel.ShutdownStarted += OnModelShutdownStarted;
            dynamoModel.EvaluationCompleted += OnEvaluationCompleted;
            dynamoModel.PropertyChanged += OnModelPropertyChanged;
        }

        protected virtual void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void OnEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            // Override in derived classes
        }

        private void UnregisterModelEventHandlers(IDynamoModel model)
        {
            model.WorkspaceCleared -= OnWorkspaceCleared;
            model.ShutdownStarted -= OnModelShutdownStarted;
            model.EvaluationCompleted -= OnEvaluationCompleted;
            model.PropertyChanged -= OnModelPropertyChanged;
        }

        private void UnregisterWorkspaceEventHandlers(IDynamoModel model)
        {
            model.WorkspaceAdded -= OnWorkspaceAdded;
            model.WorkspaceRemoved -= OnWorkspaceRemoved;
            model.WorkspaceOpening -= OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving -= OnWorkspaceSaving;
            }
        }

        private void RegisterWorkspaceEventHandlers(IDynamoModel model)
        {
            model.WorkspaceAdded += OnWorkspaceAdded;
            model.WorkspaceRemoved += OnWorkspaceRemoved;
            model.WorkspaceOpening += OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving += OnWorkspaceSaving;
                ws.NodeAdded += OnNodeAddedToWorkspace;
                ws.NodeRemoved += OnNodeRemovedFromWorkspace;

                foreach (var node in ws.Nodes)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                    node.RenderPackagesUpdated += OnRenderPackagesUpdated;
                }
            }
        }

        /// <summary>
        /// Forces a regeneration of the render packages for all nodes.
        /// </summary>
        public void RegenerateAllPackages()
        {
            foreach (var node in
                dynamoModel.CurrentWorkspace.Nodes)
            {
                node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController,
                        renderPackageFactory, true);
            }
        }

        protected virtual void OnWorkspaceCleared(WorkspaceModel workspace)
        {
            OnClear();
        }

        protected virtual void OnWorkspaceOpening(object obj)
        {
            // Override in derived classes.
        }

        private void OnWorkspaceAdded(WorkspaceModel workspace)
        {
            workspace.Saving += OnWorkspaceSaving;
            workspace.NodeAdded += OnNodeAddedToWorkspace;
            workspace.NodeRemoved += OnNodeRemovedFromWorkspace;

            foreach (var node in workspace.Nodes)
            {
                RegisterNodeEventHandlers(node);
            }
        }

        protected void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            workspace.Saving -= OnWorkspaceSaving;
            workspace.NodeAdded -= OnNodeAddedToWorkspace;
            workspace.NodeRemoved -= OnNodeRemovedFromWorkspace;

            foreach (var node in workspace.Nodes)
            {
                UnregisterNodeEventHandlers(node);
            }

            OnClear();
        }

        protected virtual void OnWorkspaceSaving(XmlDocument doc)
        {
            // Override in derived classes
        }

        private void OnNodeAddedToWorkspace(NodeModel node)
        {
            RegisterNodeEventHandlers(node);
        }

        private void OnNodeRemovedFromWorkspace(NodeModel node)
        {
            UnregisterNodeEventHandlers(node);
            DeleteGeometryForNode(node);
        }

        public virtual CameraData GetCameraInformation()
        {
            // Override in derived classes.
            return null;
        }

        /// <summary>
        /// Call this method to remove render pakcages that created by node.
        /// </summary>
        /// <param name="node"></param>
        public virtual void RemoveGeometryForNode(NodeModel node)
        {
            // Override in inherited classes.
        }

        /// <summary>
        /// Call this method to add the render package. 
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="forceAsyncCall"></param>
        public virtual void AddGeometryForRenderPackages(RenderPackageCache packages, bool forceAsyncCall = false)
        {
            // Override in inherited classes.
        }

        public virtual void AddLabelForPath(string path)
        {
            // Override in inherited classes.
        }

        /// <summary>
        /// Remove the labels (in Watch3D View) for geometry once the Watch node is disconnected
        /// </summary>
        /// <param name="path"></param>
        public virtual void ClearPathLabel(string path)
        {
            // Override in inherited classes.
        }

        public void Invoke(Action action)
        {
            var dynamoViewModel = viewModel as DynamoViewModel;
            if (dynamoViewModel != null)
            {
                dynamoViewModel.UIDispatcher.Invoke(action);
            }
        }

        public virtual void DeleteGeometryForNode(NodeModel node, bool requestUpdate = true)
        {
            // Override in derived classes.
        }

        public virtual void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            // Override in derived classes.
        }

        public virtual void HighlightNodeGraphics(IEnumerable<NodeModel> nodes)
        {
            // Override in derived classes.
        }

        public virtual void UnHighlightNodeGraphics(IEnumerable<NodeModel> nodes)
        {
            // Override in derived classes.
        }

        private void RegisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged += OnNodePropertyChanged;
            node.RenderPackagesUpdated += OnRenderPackagesUpdated;

            RegisterPortEventHandlers(node);
        }

        protected internal void UnregisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.RenderPackagesUpdated -= OnRenderPackagesUpdated;

            UnregisterPortEventHandlers(node);
        }

        protected void RegisterPortEventHandlers(NodeModel node)
        {
            node.PortConnected += PortConnectedHandler;
            node.PortDisconnected += PortDisconnectedHandler;
        }

        private void UnregisterPortEventHandlers(NodeModel node)
        {
            node.PortConnected -= PortConnectedHandler;
            node.PortDisconnected -= PortDisconnectedHandler;
        }

        protected virtual void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Do nothing for a standard node.
        }

        protected virtual void PortDisconnectedHandler(PortModel port)
        {
            if (port.PortType == PortType.Input)
            {
                DeleteGeometryForIdentifier(port.Owner.AstIdentifierBase);
            }
        }

        protected virtual void SelectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void OnRenderPackagesUpdated(NodeModel node, RenderPackageCache packages)
        {
            RemoveGeometryForNode(node);
            if (packages.IsEmpty())
                return;

            // If there is no attached model update for all render packages
            if (watchModel == null)
            {
                AddGeometryForRenderPackages(packages);
                return;
            }

            // If there are no input ports update for all render packages
            var inPorts = watchModel.InPorts;
            if (inPorts == null || inPorts.Count() < 1)
            {
                AddGeometryForRenderPackages(packages);
                return;
            }

            // If there are no connectors connected to the first (only) input port update for all render packages
            var inConnectors = inPorts[0].Connectors;
            if (inConnectors == null || inConnectors.Count() < 1 || inConnectors[0].Start == null)
            {
                AddGeometryForRenderPackages(packages);
                return;
            }

            // Only update for render packages from the connected output port
            var inputId = inConnectors[0].Start.GUID;
            foreach (var port in node.OutPorts)
            {
                if (port.GUID != inputId)
                {
                    continue;
                }

                RenderPackageCache portPackages = packages.GetPortPackages(inputId);
                if (portPackages == null)
                {
                    continue;
                }

                AddGeometryForRenderPackages(portPackages);
            }
        }

        /// <summary>
        /// Called from derived classes when a collection of render packages
        /// are available to be processed as render geometry.
        /// </summary>
        /// <param name="taskPackages">A collection of packages from which to 
        /// create render geometry.</param>
        public virtual void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(
            RenderPackageCache taskPackages)
        {
            // Override in derived classes
        }

        internal event Func<MouseEventArgs, IRay> RequestClickRay;

        /// <summary>
        /// Returns a 3D ray from the camera to the given mouse location
        /// in world coordinates that can be used to perform a hit-test 
        /// on objects in the view
        /// </summary>
        /// <param name="args">mouse click location in screen coordinates</param>
        /// <returns></returns>
        public IRay GetClickRay(MouseEventArgs args)
        {
            return RequestClickRay != null ? RequestClickRay(args) : null;
        }

        internal event Func<Point3D> RequestCameraPosition;

        public Point3D? GetCameraPosition()
        {
            var handler = RequestCameraPosition;
            if (handler != null) return handler();
            return null;
        }

        public event Action<object, MouseButtonEventArgs> ViewMouseDown;
        internal void OnViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HandleViewClick(sender, e);
            var handler = ViewMouseDown;
            if (handler != null) handler(sender, e);
        }

        protected virtual void HandleViewClick(object sender, MouseButtonEventArgs e)
        { }

        public event Action<object, MouseButtonEventArgs> ViewMouseUp;
        internal void OnViewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var handler = ViewMouseUp;
            if (handler != null) handler(sender, e);
        }

        public event Action<object, MouseEventArgs> ViewMouseMove;
        internal void OnViewMouseMove(object sender, MouseEventArgs e)
        {
            var handler = ViewMouseMove;
            if (handler != null) handler(sender, e);
        }

        public event Action<object, RoutedEventArgs> ViewCameraChanged;

        internal void OnViewCameraChanged(object sender, RoutedEventArgs args)
        {
            var handler = ViewCameraChanged;
            if (handler != null) handler(sender, args);
        }

        protected virtual void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Override in derived classes.
        }

        internal virtual void ExportToSTL(string path, string modelName)
        {
            // Override in derived classes
        }

        internal void CancelNavigationState()
        {
            if(IsPanning) TogglePan(null);
            if(IsOrbiting) ToggleOrbit(null);
        }

        #region command methods

        internal void TogglePan(object parameter)
        {
            CurrentSpaceViewModel.RequestTogglePanMode();

            // Since panning and orbiting modes are exclusive from one another,
            // turning one on may turn the other off. This is the reason we must
            // raise property change for both at the same time to update visual.
            RaisePropertyChanged("IsPanning");
            RaisePropertyChanged("IsOrbiting");
            RaisePropertyChanged("LeftClickCommand");
        }

        private static bool CanTogglePan(object parameter)
        {
            return true;
        }

        private void ToggleOrbit(object parameter)
        {
            CurrentSpaceViewModel.RequestToggleOrbitMode();

            // Since panning and orbiting modes are exclusive from one another,
            // turning one on may turn the other off. This is the reason we must
            // raise property change for both at the same time to update visual.
            RaisePropertyChanged("IsPanning");
            RaisePropertyChanged("IsOrbiting");
            RaisePropertyChanged("LeftClickCommand");
        }

        private static bool CanToggleOrbit(object parameter)
        {
            return true;
        }

        private void ToggleCanNavigateBackground(object parameter)
        {
            if (!Active)
                return;

            CanNavigateBackground = !CanNavigateBackground;
        }

        protected virtual bool CanToggleCanNavigateBackground(object parameter)
        {
            return false;
        }

        private void ToggleIsolationMode(object parameter)
        {
            IsolationMode = !IsolationMode;
        }

        private bool CanToggleIsolationMode(object parameter)
        {
            return true;
        }

        private static bool CanZoomToFit(object parameter)
        {
            return true;
        }

        protected virtual void ZoomToFit(object parameter)
        {
            // Override in derived classes to specify zoom to fit behavior.
        } 

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            AttachedProperties.RequestResetColorsForDynamoGeometryModel -= AttachedProperties_RequestResetColorsForDynamoGeometryModel;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

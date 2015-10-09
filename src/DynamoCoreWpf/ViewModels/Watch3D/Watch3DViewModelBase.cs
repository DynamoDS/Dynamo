using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public class Watch3DViewModelStartupParams
    {
        public IDynamoModel Model { get; set; }
        public IScheduler Scheduler { get; set; }
        public ILogger Logger { get; set; }
        public IPreferences Preferences { get; set; }
        public IEngineControllerManager EngineControllerManager { get; set; }
        public IRenderPackageFactory RenderPackageFactory { get; set; }
        public IDynamoViewModel ViewModel { get; set; }
        public INotifyPropertyChanged RenderPackageFactoryViewModel { get;set; }
        public string Name { get; set; }

        public Watch3DViewModelStartupParams()
        {
            
        }

        public Watch3DViewModelStartupParams(DynamoModel model, DynamoViewModel viewModel, string name)
        {
            Model = model;
            Scheduler = model.Scheduler;
            Logger = model.Logger;
            Preferences = model.PreferenceSettings;
            EngineControllerManager = model;
            RenderPackageFactory = viewModel.RenderPackageFactoryViewModel.Factory;
            ViewModel = viewModel;
            RenderPackageFactoryViewModel = viewModel.RenderPackageFactoryViewModel;
            Name = name;
        }
    }

    /// <summary>
    /// The Watch3DViewModelBase is the base class for all 3D previews in Dynamo.
    /// Classes which derive from this base are used to prepare geometry for 
    /// rendering by various render targets. The base class handles the registration
    /// of all necessary event handlers on models, workspaces, and nodes.
    /// </summary>
    public class Watch3DViewModelBase : NotificationObject, IWatch3DViewModel
    {
        protected readonly IDynamoModel model;
        protected readonly IScheduler scheduler;
        protected readonly IPreferences preferences;
        protected readonly ILogger logger;
        protected readonly IEngineControllerManager engineManager;
        protected readonly IRenderPackageFactory renderPackageFactory;
        protected readonly IDynamoViewModel viewModel;
        protected readonly INotifyPropertyChanged renderPackageFactoryViewModel;

        protected List<NodeModel> recentlyAddedNodes = new List<NodeModel>();
        protected bool active;
        private readonly List<IRenderPackage> currentTaggedPackages = new List<IRenderPackage>();

        /// <summary>
        /// A flag which indicates whether geometry should be processed.
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
                preferences.IsBackgroundPreviewActive = active;

                RaisePropertyChanged("Active");

                OnActiveStateChanged();
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
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

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

        internal WorkspaceViewModel CurrentSpaceViewModel
        {
            get
            {
                return viewModel.Workspaces.FirstOrDefault(vm => vm.Model == model.CurrentWorkspace);
            }
        }

        public bool IsPanning
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsPanning;
            }
        }

        public bool IsOrbiting
        {
            get
            {
                return CurrentSpaceViewModel != null && CurrentSpaceViewModel.IsOrbiting;
            }
        }

        protected Watch3DViewModelBase(Watch3DViewModelStartupParams parameters)
        {
            model = parameters.Model;
            scheduler = parameters.Scheduler;
            preferences = parameters.Preferences;
            logger = parameters.Logger;
            engineManager = parameters.EngineControllerManager;
            renderPackageFactory = parameters.RenderPackageFactory;
            viewModel = parameters.ViewModel;
            renderPackageFactoryViewModel = parameters.RenderPackageFactoryViewModel;

            Active = parameters.Preferences.IsBackgroundPreviewActive;
            Name = parameters.Name;
            logger = parameters.Logger;

            RegisterEventHandlers();

            TogglePanCommand = new DelegateCommand(TogglePan, CanTogglePan);
            ToggleOrbitCommand = new DelegateCommand(ToggleOrbit, CanToggleOrbit);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
        }

        public static Watch3DViewModelBase Start(Watch3DViewModelStartupParams parameters)
        {
            var vm = new Watch3DViewModelBase(parameters);
            vm.OnStartup();
            return vm;
        }

        protected virtual void OnStartup()
        {
            // Override in inherited classes.
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
            preferences.IsBackgroundPreviewActive = active;

            UnregisterEventHandlers();
            OnClear();
        }

        private void RegisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionChangedHandler;

            LogVisualizationCapabilities();

            renderPackageFactoryViewModel.PropertyChanged += OnRenderPackageFactoryViewModelPropertyChanged;

            RegisterModelEventhandlers(model);

            RegisterWorkspaceEventHandlers(model);

        }

        private void UnregisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionChangedHandler;

            renderPackageFactoryViewModel.PropertyChanged -= OnRenderPackageFactoryViewModelPropertyChanged;

            UnregisterModelEventHandlers(model);

            UnregisterWorkspaceEventHandlers(model);
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

        private void OnRenderPackageFactoryViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var node in
                model.CurrentWorkspace.Nodes)
            {
                node.RequestVisualUpdateAsync(scheduler, engineManager.EngineController,
                        renderPackageFactory, true);
            }
        }

        protected virtual void OnWorkspaceCleared(WorkspaceModel workspace)
        {
            OnClear();
        }

        protected virtual void OnWorkspaceOpening(XmlDocument doc)
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

        private void OnWorkspaceRemoved(WorkspaceModel workspace)
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
            DeleteGeometryForIdentifier(node.AstIdentifierBase);
        }

        public virtual void AddGeometryForRenderPackages(IEnumerable<IRenderPackage> packages)
        {
            // Override in inherited classes.
        }

        public virtual void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            // Override in derived classes.
        }

        private void RegisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged += OnNodePropertyChanged;
            node.RenderPackagesUpdated += OnRenderPackagesUpdated;

            RegisterPortEventHandlers(node);
        }

        private void UnregisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.RenderPackagesUpdated -= OnRenderPackagesUpdated;

            UnregisterPortEventHandlers(node);
        }

        protected void RegisterPortEventHandlers(NodeModel node)
        {
            foreach (var p in node.InPorts)
            {
                p.PortDisconnected += PortDisconnectedHandler;
                p.PortConnected += PortConnectedHandler;
            }
        }

        private void UnregisterPortEventHandlers(NodeModel node)
        {
            foreach (var p in node.InPorts)
            {
                p.PortDisconnected -= PortDisconnectedHandler;
                p.PortConnected -= PortConnectedHandler;
            }
        }

        protected virtual void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Do nothing for a standard node.
        }

        protected virtual void PortDisconnectedHandler(PortModel port)
        {
            DeleteGeometryForIdentifier(port.Owner.AstIdentifierBase);
        }

        protected virtual void SelectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void OnRenderPackagesUpdated(NodeModel node, IEnumerable<IRenderPackage> packages)
        {
            AddGeometryForRenderPackages(packages);
        }

        public virtual void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(
            IEnumerable<IRenderPackage> taskPackages)
        {
            // Override in derived classes
        }

        internal event Func<MouseEventArgs, Ray3D> RequestClickRay;
        public Ray3D GetClickRay(MouseEventArgs args)
        {
            return RequestClickRay != null ? RequestClickRay(args) : null;
        }

        public event Action<object, MouseButtonEventArgs> ViewMouseDown;
        internal void OnViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var handler = ViewMouseDown;
            if (handler != null) handler(sender, e);
        }

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

            InstrumentationLogger.LogAnonymousScreen(CanNavigateBackground ? "Geometry" : "Nodes");
        }

        private bool CanToggleCanNavigateBackground(object parameter)
        {
            return true;
        }

        #endregion
    }
}

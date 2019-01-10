using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Exceptions;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI;
using Dynamo.Updates;
using Dynamo.Utilities;
using Dynamo.Visualization;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoUtilities;
using ISelectable = Dynamo.Selection.ISelectable;

namespace Dynamo.ViewModels
{
    public interface IDynamoViewModel : INotifyPropertyChanged
    {
        ObservableCollection<WorkspaceViewModel> Workspaces { get; set; } 
    }

    public partial class DynamoViewModel : ViewModelBase, IDynamoViewModel
    {
        public int ScaleFactorLog
        {
            get
            {
                return (CurrentSpace == null) ? 0 :
                    Convert.ToInt32(Math.Log10(CurrentSpace.ScaleFactor));
            }
            set
            {
                CurrentSpace.ScaleFactor = Math.Pow(10, value);
                CurrentSpace.ScaleFactorChanged = true;
            }
        }

        #region properties

        private readonly DynamoModel model;
        private Point transformOrigin;
        private bool showStartPage = false;
        
        private List<DefaultWatch3DViewModel> watch3DViewModels = new List<DefaultWatch3DViewModel>();

        /// <summary>
        /// An observable collection of workspace view models which tracks the model
        /// </summary>
        private ObservableCollection<WorkspaceViewModel> workspaces = new ObservableCollection<WorkspaceViewModel>();
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get { return workspaces; }
            set
            {
                workspaces = value;
                RaisePropertyChanged("Workspaces");
            }
        }

        public DynamoModel Model
        {
            get { return model; }
        }

        public PreferenceSettings PreferenceSettings
        {
            get { return Model.PreferenceSettings; }
        }

        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set
            {
                transformOrigin = value;
                RaisePropertyChanged("TransformOrigin");
            }
        }

        public bool ViewingHomespace
        {
            get { return model.CurrentWorkspace == HomeSpace; }
        }

        public bool IsAbleToGoHome
        {
            get { return !(model.CurrentWorkspace is HomeWorkspaceModel); }
        }

        public HomeWorkspaceModel HomeSpace
        {
            get
            {
                return model.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            }
        }

        public WorkspaceViewModel HomeSpaceViewModel
        {
            get { return Workspaces.FirstOrDefault(w => w.Model is HomeWorkspaceModel); }
        }

        public EngineController EngineController { get { return Model.EngineController; } }

        public WorkspaceModel CurrentSpace
        {
            get { return model.CurrentWorkspace; }
        }

        public double WorkspaceActualHeight { get; set; }
        public double WorkspaceActualWidth { get; set; }

        public void WorkspaceActualSize(double width, double height)
        {
            WorkspaceActualWidth = width;
            WorkspaceActualHeight = height;
            RaisePropertyChanged("WorkspaceActualHeight");
            RaisePropertyChanged("WorkspaceActualWidth");
        }

        private WorkspaceViewModel currentWorkspaceViewModel;
        private string filePath;
        /// <summary>
        /// The index in the collection of workspaces of the current workspace.
        /// This property is bound to the SelectedIndex property in the workspaces tab control
        /// </summary>
        public int CurrentWorkspaceIndex
        {
            get
            {
                // It is safe to assume that DynamoModel.CurrentWorkspace is
                // update-to-date.
                var viewModel = workspaces.FirstOrDefault(vm => vm.Model == model.CurrentWorkspace);
                var index = workspaces.IndexOf(viewModel);

                // As the getter could aslo be triggered by the change of model,
                // we need to update currentWorkspaceViewModel here. 
                if (currentWorkspaceViewModel != viewModel)
                    currentWorkspaceViewModel = viewModel;

                return index;
            }
            set
            {
                // It happens when current workspace is home workspace, and we 
                // open a new home workspace. At this moment, the old homework 
                // space is removed, before new home workspace is added, Dynamo
                // has no idea about what is selected tab index.
                if (value < 0)
                    return;

                var viewModel = workspaces.ElementAt(value);
                if (currentWorkspaceViewModel != viewModel)
                {
                    currentWorkspaceViewModel = viewModel;

                    // Keep DynamoModel.CurrentWorkspace update-to-date
                    int modelIndex = model.Workspaces.IndexOf(currentWorkspaceViewModel.Model);
                    this.ExecuteCommand(new DynamoModel.SwitchTabCommand(modelIndex));
                }
            }
        }

        /// <summary>
        /// Returns the workspace view model whose workspace model is the model's current workspace
        /// </summary>
        public WorkspaceViewModel CurrentSpaceViewModel
        {
            get
            {
                if (currentWorkspaceViewModel == null)
                    currentWorkspaceViewModel = workspaces.FirstOrDefault(vm => vm.Model == model.CurrentWorkspace);

                return currentWorkspaceViewModel;
            }
        }

        internal AutomationSettings Automation { get { return this.automationSettings; } }

        internal string editName = "";
        public string EditName
        {
            get { return editName; }
            set
            {
                editName = value;
                RaisePropertyChanged("EditName");
            }
        }

        public bool ShowStartPage
        {
            get { return this.showStartPage; }

            set
            {
                // If the caller attempts to show the start page, but we are 
                // currently in playback mode, then this will not be allowed
                // (i.e. the start page will never be shown during a playback).
                // 
                if ((value == true) && (null != automationSettings))
                {
                    if (automationSettings.IsInPlaybackMode)
                        return;
                }

                showStartPage = value;
                if (showStartPage) Logging.Analytics.TrackScreenView("StartPage");

                RaisePropertyChanged("ShowStartPage");
                if (DisplayStartPageCommand != null)
                    DisplayStartPageCommand.RaiseCanExecuteChanged();
            }
        }

        public string LogText
        {
            get { return model.Logger.LogText; }
        }

        public int ConsoleHeight
        {
            get
            {
                return model.PreferenceSettings.ConsoleHeight;
            }
            set
            {
                model.PreferenceSettings.ConsoleHeight = value;

                RaisePropertyChanged("ConsoleHeight");
            }
        }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        public bool ShowPreviewBubbles
        {
            get
            {
                return model.PreferenceSettings.ShowPreviewBubbles;
            }
            set
            {
                model.PreferenceSettings.ShowPreviewBubbles = value;
                RaisePropertyChanged("ShowPreviewBubbles");
            }
        }

        /// <summary>
        /// Indicates whether to make T-Spline nodes (under ProtoGeometry.dll) discoverable
        /// in the node search library.
        /// </summary>
        public bool EnableTSpline
        {
            get
            {
                return !PreferenceSettings.NamespacesToExcludeFromLibrary.Contains(
                    "ProtoGeometry.dll:Autodesk.DesignScript.Geometry.TSpline");
            }
            set
            {
                model.HideUnhideNamespace(!value,
                    "ProtoGeometry.dll", "Autodesk.DesignScript.Geometry.TSpline");
            }
        }

        public int LibraryWidth
        {
            get
            {
                return model.PreferenceSettings.LibraryWidth;
            }
            set
            {
                model.PreferenceSettings.LibraryWidth = value;
                RaisePropertyChanged("LibraryWidth");
            }
        }

        public bool IsShowingConnectors
        {
            get
            {
                return model.IsShowingConnectors;
            }
            set
            {
                model.IsShowingConnectors = value;

                RaisePropertyChanged("IsShowingConnectors");
            }
        }

        public bool IsMouseDown { get; set; }

        public ConnectorType ConnectorType
        {
            get
            {
                return model.ConnectorType;
            }
            set
            {
                model.ConnectorType = value;

                RaisePropertyChanged("ConnectorType");
            }
        }

        public bool IsUsageReportingApproved
        {
            get
            {
                return UsageReportingManager.Instance.IsUsageReportingApproved;
            }
        }

        private ObservableCollection<string> recentFiles =
            new ObservableCollection<string>();
        public ObservableCollection<string> RecentFiles
        {
            get { return recentFiles; }
            set
            {
                recentFiles = value;
                RaisePropertyChanged("RecentFiles");
            }
        }

        public bool WatchIsResizable { get; set; }

        public string Version
        {
            get { return model.Version; }
        }

        public string HostVersion
        {
            get { return model.HostVersion; }
        }

        public string HostName
        {
            get { return model.HostName; }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                var um = model.UpdateManager;
                return um.IsUpdateAvailable;
            }
        }

        public string LicenseFile
        {
            get
            {
                string executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
                string rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
                return Path.Combine(rootModuleDirectory, "License.rtf");
            }
        }

        public bool VerboseLogging
        {
            get { return model.DebugSettings.VerboseLogging; }
            set
            {
                model.DebugSettings.VerboseLogging = value;
                RaisePropertyChanged("VerboseLogging");
           }
        }

        public bool ShowDebugASTs
        {
            get { return IsDebugBuild && model.DebugSettings.ShowDebugASTs; }
            set
            {
                model.DebugSettings.ShowDebugASTs = value;
                RaisePropertyChanged("ShowDebugASTs");
            }
        }

        internal Dispatcher UIDispatcher { get; set; }

        public IWatchHandler WatchHandler { get; private set; }

        public SearchViewModel SearchViewModel { get; private set; }

        public PackageManagerClientViewModel PackageManagerClientViewModel { get; private set; }

        /// <summary>
        ///     Whether sign in should be shown in Dynamo.  In instances where Dynamo obtains
        ///     authentication capabilities from a host, Dynamo's sign in should generally be 
        ///     hidden to avoid inconsistencies in state.
        /// </summary>
        public bool ShowLogin { get; private set; }

        private bool showRunPreview;
        public bool ShowRunPreview
        {
            get { return showRunPreview; }
            set
            {
                showRunPreview = value;
                HomeSpace.GetExecutingNodes(showRunPreview);
                RaisePropertyChanged("ShowRunPreview");              
            }
        }

        public RenderPackageFactoryViewModel RenderPackageFactoryViewModel { get; set; }

        public bool EnablePresetOptions
        {
            get { return this.Model.CurrentWorkspace.Presets.Any(); }            
        }

        /// <summary>
        /// A collection of <see cref="DefaultWatch3DViewModel"/> objects. 
        /// 
        /// Each DefaultWatch3DViewModel object is responsible for converting
        /// data for visualization in a different context. For example, the 
        /// <see cref="HelixWatch3DViewModel"/> provides the geometry for the
        /// background preview.
        /// </summary>
        public IEnumerable<DefaultWatch3DViewModel> Watch3DViewModels
        {
            get { return watch3DViewModels; }
        }

        /// <summary>
        /// A <see cref="DefaultWatch3DViewModel"/> which provides the
        /// geometry for the primary background 3d preview.
        /// </summary>
        public DefaultWatch3DViewModel BackgroundPreviewViewModel { get; private set; }

        public bool BackgroundPreviewActive
        {
            get { return BackgroundPreviewViewModel.Active; }
        }
        
        #endregion

        public struct StartConfiguration
        {
            public string CommandFilePath { get; set; }
            public IWatchHandler WatchHandler { get; set; }
            public DynamoModel DynamoModel { get; set; }
            public bool ShowLogin { get; set; }
            public DefaultWatch3DViewModel Watch3DViewModel { get; set; }

            /// <summary>
            /// This property is initialized if there is an external host application
            /// at startup in order to be used to pass in host specific resources to DynamoViewModel
            /// </summary>
            public IBrandingResourceProvider BrandingResourceProvider { get; set; }
        }

        public static DynamoViewModel Start(StartConfiguration startConfiguration = new StartConfiguration())
        {
            if(startConfiguration.DynamoModel == null) 
                startConfiguration.DynamoModel = DynamoModel.Start();

            if(startConfiguration.WatchHandler == null)
                startConfiguration.WatchHandler = new DefaultWatchHandler(startConfiguration.DynamoModel.PreferenceSettings);

            if (startConfiguration.Watch3DViewModel == null)
            {
                startConfiguration.Watch3DViewModel = 
                    HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                        null,
                        new Watch3DViewModelStartupParams(startConfiguration.DynamoModel), 
                        startConfiguration.DynamoModel.Logger);
            }

            return new DynamoViewModel(startConfiguration);
        }

        protected DynamoViewModel(StartConfiguration startConfiguration)
        {

            // This can be removed after this bug is fixed in .net 4.7
            // https://developercommunity.visualstudio.com/content/problem/244615/setfinalsizemaxdiscrepancy-getting-stuck-in-an-inf.html
            // if the key "Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace" has a value true in the 
            // dynamoCoreWpf.config file we will set the switch here before the view is created.
            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            var gridSwitchKey = "Switch.System.Windows.Controls.Grid.StarDefinitionsCanExceedAvailableSpace";
            var gridSwitchKeyValue = config.AppSettings.Settings[gridSwitchKey];
            bool gridSwitch = false;
            if(gridSwitchKeyValue != null)
            {
                bool.TryParse(gridSwitchKeyValue.Value, out gridSwitch);
                AppContext.SetSwitch(gridSwitchKey, gridSwitch);
            }

            this.ShowLogin = startConfiguration.ShowLogin;

            // initialize core data structures
            this.model = startConfiguration.DynamoModel;
            this.model.CommandStarting += OnModelCommandStarting;
            this.model.CommandCompleted += OnModelCommandCompleted;

            UsageReportingManager.Instance.InitializeCore(this);
            this.WatchHandler = startConfiguration.WatchHandler;
            var pmExtension = model.GetPackageManagerExtension();
            this.PackageManagerClientViewModel = new PackageManagerClientViewModel(this, pmExtension.PackageManagerClient);
            this.SearchViewModel = new SearchViewModel(this);

            // Start page should not show up during test mode.
            this.ShowStartPage = !DynamoModel.IsTestMode;

            this.BrandingResourceProvider = startConfiguration.BrandingResourceProvider ?? new DefaultBrandingResourceProvider();

            // commands should be initialized before adding any WorkspaceViewModel
            InitializeDelegateCommands();

            //add the initial workspace and register for future 
            //updates to the workspaces collection
            var homespaceViewModel = new HomeWorkspaceViewModel(model.CurrentWorkspace as HomeWorkspaceModel, this);
            workspaces.Add(homespaceViewModel);
            currentWorkspaceViewModel = homespaceViewModel;
           
            model.WorkspaceAdded += WorkspaceAdded;
            model.WorkspaceRemoved += WorkspaceRemoved;
             
            SubscribeModelCleaningUpEvent();
            SubscribeModelUiEvents();
            SubscribeModelChangedHandlers();
            SubscribeModelBackupFileSaveEvent();
            SubscribeUpdateManagerHandlers();

            InitializeAutomationSettings(startConfiguration.CommandFilePath);

            SubscribeLoggerHandlers();

            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;

            InitializeRecentFiles();

            UsageReportingManager.Instance.PropertyChanged += CollectInfoManager_PropertyChanged;

            WatchIsResizable = false;

            SubscribeDispatcherHandlers();

            RenderPackageFactoryViewModel = new RenderPackageFactoryViewModel(Model.PreferenceSettings);
            RenderPackageFactoryViewModel.PropertyChanged += RenderPackageFactoryViewModel_PropertyChanged;

            BackgroundPreviewViewModel = startConfiguration.Watch3DViewModel;
            BackgroundPreviewViewModel.PropertyChanged += Watch3DViewModelPropertyChanged;
            WatchHandler.RequestSelectGeometry += BackgroundPreviewViewModel.AddLabelForPath;
            RegisterWatch3DViewModel(BackgroundPreviewViewModel, RenderPackageFactoryViewModel.Factory);
            model.ComputeModelDeserialized += model_ComputeModelDeserialized;
        }

        /// <summary>
        /// Sets up the provided <see cref="DefaultWatch3DViewModel"/> object and 
        /// adds it to the Watch3DViewModels collection.
        /// </summary>
        /// <param name="watch3DViewModel"></param>
        /// <param name="factory"></param>
        protected void RegisterWatch3DViewModel(DefaultWatch3DViewModel watch3DViewModel, IRenderPackageFactory factory)
        {
            watch3DViewModel.Setup(this, factory);
            watch3DViewModels.Add(watch3DViewModel);
            watch3DViewModel.Active = PreferenceSettings
                .GetIsBackgroundPreviewActive(watch3DViewModel.PreferenceWatchName);
            RaisePropertyChanged("Watch3DViewModels");
        }

        private void RenderPackageFactoryViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowEdges":
                    var factoryVm = (RenderPackageFactoryViewModel)sender;
                    model.PreferenceSettings.ShowEdges = factoryVm.Factory.TessellationParameters.ShowEdges;
                    // A full regeneration is required to get the edge geometry.
                    foreach (var vm in Watch3DViewModels)
                    {
                        vm.RegenerateAllPackages();
                    }
                    break;
                default:
                    return;
            }
        }

        void Watch3DViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Active":
                    RaisePropertyChanged("BackgroundPreviewActive");                 
                    break;
                case "CanNavigateBackground":
                    if (!BackgroundPreviewViewModel.CanNavigateBackground)
                    {
                        // Return focus back to Dynamo View
                        OnRequestReturnFocusToView();
                    }
                    break;
            }
        }

        internal event EventHandler NodeViewReady;
        internal void OnNodeViewReady(object nodeView)
        {
            if (NodeViewReady != null)
            {
                this.NodeViewReady(nodeView, new EventArgs());
            }
        }

        #region Event handler destroy/create

        protected virtual void UnsubscribeAllEvents()
        {
            UnsubscribeDispatcherEvents();
            UnsubscribeModelUiEvents();
            UnsubscribeModelChangedEvents();
            UnsubscribeUpdateManagerEvents();
            UnsubscribeLoggerEvents();
            UnsubscribeModelCleaningUpEvent();
            UnsubscribeModelBackupFileSaveEvent();

            model.WorkspaceAdded -= WorkspaceAdded;
            model.WorkspaceRemoved -= WorkspaceRemoved;
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
            UsageReportingManager.Instance.PropertyChanged -= CollectInfoManager_PropertyChanged;
        }

        private void InitializeRecentFiles()
        {
            this.RecentFiles = new ObservableCollection<string>(model.PreferenceSettings.RecentFiles);
            this.RecentFiles.CollectionChanged += (sender, args) =>
            {
                model.PreferenceSettings.RecentFiles = this.RecentFiles.ToList();
            };
        }

        private void SubscribeLoggerHandlers()
        {
            model.Logger.PropertyChanged += Instance_PropertyChanged;
        }

        private void UnsubscribeLoggerEvents()
        {
            model.Logger.PropertyChanged -= Instance_PropertyChanged;
        }

        private void SubscribeUpdateManagerHandlers()
        {
            model.UpdateManager.UpdateDownloaded += Instance_UpdateDownloaded;
            model.UpdateManager.ShutdownRequested += UpdateManager_ShutdownRequested;
        }

        private void UnsubscribeUpdateManagerEvents()
        {
            model.UpdateManager.UpdateDownloaded -= Instance_UpdateDownloaded;
            model.UpdateManager.ShutdownRequested -= UpdateManager_ShutdownRequested;
        }

        private void SubscribeModelUiEvents()
        {
            model.RequestBugReport += ReportABug;
            model.RequestDownloadDynamo += DownloadDynamo;
        }

        private void UnsubscribeModelUiEvents()
        {
            model.RequestBugReport -= ReportABug;
            model.RequestDownloadDynamo -= DownloadDynamo;
        }

        private void SubscribeModelCleaningUpEvent()
        {
            model.CleaningUp += CleanUp;
        }

        private void UnsubscribeModelCleaningUpEvent()
        {
            model.CleaningUp -= CleanUp;
        }

        private void SubscribeModelBackupFileSaveEvent()
        {
            model.RequestWorkspaceBackUpSave += SaveAs;
        }

        private void UnsubscribeModelBackupFileSaveEvent()
        {
            model.RequestWorkspaceBackUpSave -= SaveAs;
        }

        private void SubscribeModelChangedHandlers()
        {
            model.PropertyChanged += _model_PropertyChanged;
            model.WorkspaceCleared += ModelWorkspaceCleared;
            model.RequestCancelActiveStateForNode += this.CancelActiveState;
        }

        private void UnsubscribeModelChangedEvents()
        {
            model.PropertyChanged -= _model_PropertyChanged;
            model.WorkspaceCleared -= ModelWorkspaceCleared;
            model.RequestCancelActiveStateForNode -= this.CancelActiveState;
        }

        private void SubscribeDispatcherHandlers()
        {
            DynamoModel.RequestDispatcherBeginInvoke += TryDispatcherBeginInvoke;
            DynamoModel.RequestDispatcherInvoke += TryDispatcherInvoke;
        }

        private void UnsubscribeDispatcherEvents()
        {
            DynamoModel.RequestDispatcherBeginInvoke -= TryDispatcherBeginInvoke;
            DynamoModel.RequestDispatcherInvoke -= TryDispatcherInvoke;
        }
        #endregion

        private void InitializeAutomationSettings(string commandFilePath)
        {
            if (String.IsNullOrEmpty(commandFilePath) || !File.Exists(commandFilePath))
                commandFilePath = null;

            // Instantiate an AutomationSettings to handle record/playback.
            automationSettings = new AutomationSettings(this.Model, commandFilePath);
        }

        private void TryDispatcherBeginInvoke(Action action)
        {
            if (this.UIDispatcher != null)
            {
                UIDispatcher.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void TryDispatcherInvoke(Action action)
        {
            if (this.UIDispatcher != null)
            {
                UIDispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void ModelWorkspaceCleared(WorkspaceModel workspace)
        {
            RaiseCanExecuteUndoRedo();

            // Reset workspace state
            CurrentSpaceViewModel.CancelActiveState();
        }

        internal void ForceRunExprCmd(object parameters)
        {
            bool displayErrors = Convert.ToBoolean(parameters);
            var command = new DynamoModel.ForceRunCancelCommand(displayErrors, false);
            this.ExecuteCommand(command);
        }

        internal void MutateTestCmd(object parameters)
        {
            var command = new DynamoModel.MutateTestCommand();
            this.ExecuteCommand(command);
        }

        public void DisplayFunction(object parameters)
        {
            Model.OpenCustomNodeWorkspace((Guid)parameters);
        }

        internal bool CanDisplayFunction(object parameters)
        {
            return Model.CustomNodeManager.Contains((Guid)parameters);
        }

        /// <summary>
        /// Opens a new browser window pointing to the Dynamo repo new issue page, pre-filling issue 
        /// title and content based on crash details. Uses system default browser.
        /// </summary>
        /// <param name="bodyContent">Crash details body. If null, nothing will be filled-in.</param>
        public static void ReportABug(object bodyContent)
        {
            var urlWithParameters = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(bodyContent);

            // launching the process using explorer.exe will format the URL incorrectly
            // and Github will not recognise the query parameters in the URL
            // so launch with default operating system web browser
            Process.Start(new ProcessStartInfo(urlWithParameters));
        }

        public static void ReportABug()
        {
            ReportABug(null);
        }

        internal static void DownloadDynamo()
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoDownloadLink));
        }

        internal bool CanReportABug(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Clear the UI log.
        /// </summary>
        public void ClearLog(object parameter)
        {
            Model.Logger.ClearLog();
        }

        internal bool CanClearLog(object parameter)
        {
            return true;
        }

        void Instance_UpdateDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            RaisePropertyChanged("Version");
            RaisePropertyChanged("IsUpdateAvailable");
        }

        void UpdateManager_ShutdownRequested(IUpdateManager updateManager)
        {
            PerformShutdownSequence(new ShutdownParams(
                shutdownHost: true, allowCancellation: true));
        }

        void CollectInfoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CollectInfoOption":
                    RaisePropertyChanged("CollectInfoOption");
                    break;
            }
        }

        private void SelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            PublishSelectedNodesCommand.RaiseCanExecuteChanged();
            AlignSelectedCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            UngroupModelCommand.RaiseCanExecuteChanged();
            AddModelsToGroupModelCommand.RaiseCanExecuteChanged();
            ShowNewPresetsDialogCommand.RaiseCanExecuteChanged();
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "LogText":
                    RaisePropertyChanged("LogText");
                    RaisePropertyChanged("WarningText");
                    break;
            }

        }

        void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    RaisePropertyChanged("IsAbleToGoHome");
                    RaisePropertyChanged("CurrentSpace");
                    RaisePropertyChanged("BackgroundColor");
                    RaisePropertyChanged("CurrentWorkspaceIndex");
                    RaisePropertyChanged("ViewingHomespace");
                    if (this.PublishCurrentWorkspaceCommand != null)
                        this.PublishCurrentWorkspaceCommand.RaiseCanExecuteChanged();
                    RaisePropertyChanged("IsPanning");
                    RaisePropertyChanged("IsOrbiting");
                    if (ChangeScaleFactorCommand != null)
                    {
                        ChangeScaleFactorCommand.RaiseCanExecuteChanged();
                    }
                    //RaisePropertyChanged("RunEnabled");
                    break;

                case "EnablePresetOptions":
                    RaisePropertyChanged("EnablePresetOptions");
                    break;
            }
        }

        // TODO(Sriram): This method is currently not used, but it should really 
        // be. It watches property change notifications coming from the current 
        // WorkspaceModel, and then enables/disables 'set timer' button on the UI.
        // 
        //void CurrentWorkspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "RunEnabled":
        //            RaisePropertyChanged(e.PropertyName);
        //            break;
        //    }
        //}

        private void CleanUp()
        {
            UnsubscribeAllEvents();
        }

        internal bool CanWriteToLog(object parameters)
        {
            if (model.Logger != null)
            {
                return true;
            }

            return false;
        }

        internal bool CanCopy(object parameters)
        {
            if (DynamoSelection.Instance.Selection.Count == 0)
            {
                return false;
            }
            return true;
        }

        internal bool CanPaste(object parameters)
        {
            if (model.ClipBoard.Count == 0)
            {
                return false;
            }

            return true;
        }
       
        private void Paste(object parameter)
        {
            OnRequestPaste();
            RaiseCanExecuteUndoRedo();
        }

        /// <summary>
        /// After command framework is implemented, this method should now be only 
        /// called from a menu item (i.e. Ctrl + W). It should not be used as a way
        /// for any other code paths to create a note programmatically. For that we
        /// now have AddNoteInternal which takes in more configurable arguments.
        /// </summary>
        /// <param name="parameters">This is not used and should always be null,
        /// otherwise an ArgumentException will be thrown.</param>
        /// 
        public void AddNote(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            var command = new DynamoModel.CreateNoteCommand(Guid.NewGuid(), null, 0, 0, true);
            this.ExecuteCommand(command);
        }

        internal bool CanAddNote(object parameters)
        {
            return true;
        }

        internal void AddAnnotation(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            var command = new DynamoModel.CreateAnnotationCommand(Guid.NewGuid(), null, 0, 0, true);
            this.ExecuteCommand(command);
        }

        internal bool CanAddAnnotation(object parameter)
        {
            var groups = Model.CurrentWorkspace.Annotations;
            //Create Group should be disabled when a group is selected
            if (groups.Any(x => x.IsSelected))
            {
                return false;
            }

            //Create Group should be disabled when a node selected is already in a group
            if (!groups.Any(x => x.IsSelected))
            {
                var modelSelected = DynamoSelection.Instance.Selection.OfType<ModelBase>().Where(x => x.IsSelected);
                //If there are no nodes selected then return false
                if (!modelSelected.Any())
                {
                    return false;
                }

                foreach (var model in modelSelected)
                {
                    if (groups.ContainsModel(model.GUID))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal void UngroupAnnotation(object parameters)
        {
            if (null != parameters)
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }
            //Check for multiple groups - Delete the group and not the nodes.
            foreach (var group in DynamoSelection.Instance.Selection.OfType<AnnotationModel>().ToList())
            {
                var command = new DynamoModel.DeleteModelCommand(group.GUID);
                this.ExecuteCommand(command);
            }            
        }

        internal bool CanUngroupAnnotation(object parameter)
        {
            return DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Any();
        }

        internal void UngroupModel(object parameters)
        {
            if (null != parameters)
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }
            //Check for multiple groups - Delete the group and not the nodes.
            foreach (var modelb in DynamoSelection.Instance.Selection.OfType<ModelBase>().ToList())
            {
                if (!(modelb is AnnotationModel))
                {
                    var command = new DynamoModel.UngroupModelCommand(modelb.GUID);
                    this.ExecuteCommand(command);
                }
            }  
        }

        internal bool CanUngroupModel(object parameter)
        {
            var tt = DynamoSelection.Instance.Selection.OfType<ModelBase>().Any();
            return DynamoSelection.Instance.Selection.OfType<ModelBase>().Any();
        }

        internal bool CanAddModelsToGroup(object obj)
        {          
            return DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Any();
        }

        internal void AddModelsToGroup(object parameters)
        {
            if (null != parameters)
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }
            //Check for multiple groups - Delete the group and not the nodes.
            foreach (var modelb in DynamoSelection.Instance.Selection.OfType<ModelBase>())
            {
                if (!(modelb is AnnotationModel))
                {
                    var command = new DynamoModel.AddModelToGroupCommand(modelb.GUID);
                    this.ExecuteCommand(command);
                }
            }  
        }

        private void WorkspaceAdded(WorkspaceModel item)
        {
            if (item is HomeWorkspaceModel)
            {
                var newVm = new HomeWorkspaceViewModel(item as HomeWorkspaceModel, this);
                workspaces.Insert(0, newVm);

                // The RunSettings control is a child of the DynamoView, 
                // but has its DataContext set to the RunSettingsViewModel 
                // on the HomeWorkspaceViewModel. When the home workspace is changed,
                // we need to raise a property change notification for the 
                // homespace view model, so the RunSettingsControl's bindings
                // get updated.
                RaisePropertyChanged("HomeSpaceViewModel");
            }
            else
            {
                var newVm = new WorkspaceViewModel(item, this);

                // For Json Workspaces, workspace view info need to be read again from file
                string fileContents;
                Exception ex;
                if (DynamoUtilities.PathHelper.isValidJson(newVm.Model.FileName, out fileContents, out ex))
                {
                    ExtraWorkspaceViewInfo viewInfo = WorkspaceViewModel.ExtraWorkspaceViewInfoFromJson(fileContents);
                    newVm.Model.UpdateWithExtraWorkspaceViewInfo(viewInfo);
                }
                workspaces.Add(newVm);
            }
        }

        private void WorkspaceRemoved(WorkspaceModel item)
        {
            var viewModel = workspaces.First(x => x.Model == item);
            if (currentWorkspaceViewModel == viewModel)
                if(currentWorkspaceViewModel != null)
                {
                    currentWorkspaceViewModel.Dispose();
                }
                currentWorkspaceViewModel = null;
            workspaces.Remove(viewModel);
        }

        internal void AddToRecentFiles(string path)
        {
            if (path == null) return;

            if (RecentFiles.Contains(path))
            {
                RecentFiles.Remove(path);
            }

            RecentFiles.Insert(0, path);

            int maxNumRecentFiles = Model.PreferenceSettings.MaxNumRecentFiles;
            if (RecentFiles.Count > maxNumRecentFiles)
            {
                RecentFiles.RemoveRange(maxNumRecentFiles, RecentFiles.Count - maxNumRecentFiles);
            }
        }

        /// <summary>
        /// Returns the file-save dialog with customized file types of Dynamo.
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns>A customized file-save dialog</returns>
        public FileDialog GetSaveDialog(WorkspaceModel workspace)
        {
            FileDialog fileDialog = new SaveFileDialog
            {
                AddExtension = true,
            };

            string ext, fltr;
            if (workspace == HomeSpace)
            {
                ext = ".dyn";
                fltr = string.Format(Resources.FileDialogDynamoWorkspace,BrandingResourceProvider.ProductName,"*.dyn");
            }
            else
            {
                ext = ".dyf";
                fltr = string.Format(Resources.FileDialogDynamoCustomNode,BrandingResourceProvider.ProductName,"*.dyf");
            }
            fltr += "|" + string.Format(Resources.FileDialogAllFiles, "*.*");

            fileDialog.FileName = workspace.Name + ext;
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = ext;
            fileDialog.Filter = fltr;

            return fileDialog;
        }

        /// <summary>
        /// Attempts to open a file using the passed open command, but wraps the call
        /// with a check to make sure no unsaved changes to the HomeWorkspace are lost.
        /// </summary>
        /// <param name="openCommand"> <see cref="DynamoModel.OpenFileCommand"/> </param>
        private void OpenIfSaved(object openCommand)
        {
            var command = openCommand as DynamoModel.OpenFileCommand;
            if (command == null)
            {
                return;
            }

            if(HomeSpace != null && HomeSpace.HasUnsavedChanges)
            {
                if (AskUserToSaveWorkspaceOrCancel(HomeSpace))
                {
                    this.ExecuteCommand(command);
                    this.ShowStartPage = false;
                }
            }
            else
            {
                this.OpenCommand.Execute(new Tuple<string,bool>(command.FilePath, command.ForceManualExecutionMode));
                this.ShowStartPage = false;
            }
        }

        /// <summary>
        /// Open a definition or workspace.
        /// </summary>
        /// <param name="parameters"></param>
        /// For most cases, parameters variable refers to the file path to open
        /// However, when this command is used in OpenFileDialog, the variable is
        /// a Tuple<string, bool> instead. The boolean flag is used to override the
        /// RunSetting of the workspace.
        private void Open(object parameters)
        {
            // try catch for exceptions thrown while opening files, say from a future version, 
            // that can't be handled reliably
            filePath = string.Empty;
            bool forceManualMode = false; 
            try
            {
                var packedParams = parameters as Tuple<string, bool>;
                if (packedParams != null)
                {
                    filePath = packedParams.Item1;
                    forceManualMode = packedParams.Item2;
                }
                else
                {
                    filePath = parameters as string;
                }
                ExecuteCommand(new DynamoModel.OpenFileCommand(filePath, forceManualMode));
            }
            catch (Exception e)
            {
                if (!DynamoModel.IsTestMode)
                {
                    string commandString = String.Format(Resources.MessageErrorOpeningFileGeneral);
                    string errorMsgString;
                    // Catch all the IO exceptions and file access here. The message provided by .Net is clear enough to indicate the problem in this case.
                    if (e is IOException || e is UnauthorizedAccessException)
                    {
                        errorMsgString = String.Format(e.Message, filePath);
                    }
                    else if (e is System.Xml.XmlException || e is Newtonsoft.Json.JsonReaderException)
                    {
                        errorMsgString = String.Format(Resources.MessageFailedToOpenCorruptedFile, filePath);
                    }
                    else
                    {
                        errorMsgString = String.Format(Resources.MessageUnkownErrorOpeningFile, filePath);
                    }
                    model.Logger.LogNotification("Dynamo", commandString, errorMsgString, e.ToString());
                    System.Windows.MessageBox.Show(errorMsgString);
                }
                else
                {
                    throw (e);
                }
                return;
            }
            this.ShowStartPage = false; // Hide start page if there's one.
        }

        private bool CanOpen(object parameters)
        {
            var filePath = parameters as string;
            return PathHelper.IsValidPath(filePath);
        }

        /// <summary>
        /// Read the contents of the file and set the view parameters for that current workspace
        /// </summary>
        private void model_ComputeModelDeserialized()
        {
            if (filePath == String.Empty) return;
            string fileContents = File.ReadAllText(filePath);
            try
            {
                // This call will fail in the case of an XML file
                ExtraWorkspaceViewInfo viewInfo = WorkspaceViewModel.ExtraWorkspaceViewInfoFromJson(fileContents);

                Model.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(viewInfo);
                Model.OnWorkspaceOpening(viewInfo);
            }
            catch (Exception e)
            {
                this.ShowStartPage = false; // Hide start page if there's one.
                return;
            }

            // TODO: Finish initialization of the WorkspaceViewModel
        }

        /// <summary>
        /// Present the open dialog and open the workspace that is selected.
        /// </summary>
        /// <param name="parameter"></param>
        private void ShowOpenDialogAndOpenResult(object parameter)
        {
            if (HomeSpace.HasUnsavedChanges)
            {
                if (!AskUserToSaveWorkspaceOrCancel(HomeSpace))
                    return;
            }

            DynamoOpenFileDialog _fileDialog = new DynamoOpenFileDialog(this)
            {
                Filter = string.Format(Resources.FileDialogDynamoDefinitions,
                         BrandingResourceProvider.ProductName, "*.dyn;*.dyf") + "|" +
                         string.Format(Resources.FileDialogAllFiles, "*.*"),
                Title = string.Format(Resources.OpenDynamoDefinitionDialogTitle,BrandingResourceProvider.ProductName)
            };

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(Model.CurrentWorkspace.FileName))
            {
                string path = Model.CurrentWorkspace.FileName;
                if (File.Exists(path))
                {
                    var fi = new FileInfo(Model.CurrentWorkspace.FileName);
                    _fileDialog.InitialDirectory = fi.DirectoryName;
                }
                else
                {
                    _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                }
            }
            else // use the samples directory, if it exists
            {
                Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
                string location = Path.GetDirectoryName(dynamoAssembly.Location);
                string UICulture = CultureInfo.CurrentUICulture.ToString();
                string path = Path.Combine(location, "samples", UICulture);

                if (Directory.Exists(path))
                    _fileDialog.InitialDirectory = path;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (CanOpen(_fileDialog.FileName))
                {
                    Open(new Tuple<string,bool>(_fileDialog.FileName, _fileDialog.RunManualMode));
                }
            }
        }

        private bool CanShowOpenDialogAndOpenResultCommand(object parameter)
        {
            return HomeSpace.RunSettings.RunEnabled;
        }

        private void OpenRecent(object path)
        {
            // Make sure user get chance to save unsaved changes first
            if (CurrentSpaceViewModel.HasUnsavedChanges)
            {
                if (!AskUserToSaveWorkspaceOrCancel(HomeSpace))
                    return;
            }
            this.Open(path as string);
        }

        private bool CanOpenRecent(object path)
        {
            return HomeSpace.RunSettings.RunEnabled;
        }

        /// <summary>
        /// Attempts to save an the current workspace.
        /// Assumes that workspace has already been saved.
        /// </summary>
        private void Save(object parameter)
        {
            if (!String.IsNullOrEmpty(Model.CurrentWorkspace.FileName))
            {
                // For read-only file, re-direct save to save-as
                if (this.CurrentSpace.IsReadOnly)
                    ShowSaveDialogAndSaveResult(parameter);
                else
                    SaveAs(Model.CurrentWorkspace.FileName);      
            }
                
        }

        private bool CanSave(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Save the current workspace.
        /// </summary>
        /// <param name="parameters">The file path.</param>
        private void SaveAs(object parameters)
        {
            var filePath = parameters as string;
            if (string.IsNullOrEmpty(filePath))
                return;

            var fi = new FileInfo(filePath);
            SaveAs(fi.FullName);
        }

        internal bool CanSaveAs(object parameters)
        {
            return (parameters != null);
        }

        /// <summary>
        /// Save the current workspace to a specific file path. If the file path is null or empty, an
        /// exception is written to the console.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="isBackup">Indicates if an automated backup save has called this function.</param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void SaveAs(string path, bool isBackup = false)
        {
            try
            {
                Model.Logger.Log(String.Format(Properties.Resources.SavingInProgress, path));

                CurrentSpaceViewModel.Save(path, isBackup, Model.EngineController);

                if(!isBackup) AddToRecentFiles(path);
            }
            catch (Exception ex)
            {
                Model.Logger.Log(ex.Message);
                Model.Logger.Log(ex.StackTrace);

                if (ex is IOException || ex is UnauthorizedAccessException)
                    System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        }

        
        /// <summary>
        ///     Attempts to save a given workspace.  Shows a save as dialog if the 
        ///     workspace does not already have a path associated with it
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        /// <returns>true if save was successful, false otherwise</returns>
        internal bool ShowSaveDialogIfNeededAndSave(WorkspaceModel workspace)
        {
            // crash should always allow save as
            if (!String.IsNullOrEmpty(workspace.FileName) && !DynamoModel.IsCrashing)
            {
                SaveAs(workspace.FileName);
                return true;
            }
            else
            {
                //TODO(ben): We still add a cancel button to the save dialog if we're crashing
                // sadly it's not usually possible to cancel a crash

                var fd = this.GetSaveDialog(workspace);
                // since the workspace file directory is null, we set the initial directory
                // for the file to be MyDocument folder in the local computer. 
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    SaveAs(fd.FileName);
                    return true;
                }
            }

            return false;
        }

        internal bool CanVisibilityBeToggled(object parameters)
        {
            return true;
        }

        internal bool CanUpstreamVisibilityBeToggled(object parameters)
        {
            return true;
        }

        internal void ShowPackageManagerSearch(object parameters)
        {
            OnRequestPackageManagerSearchDialog(this, EventArgs.Empty);
        }

        internal bool CanShowPackageManagerSearch(object parameters)
        {
            return true;
        }

        private void ShowInstalledPackages(object parameters)
        {
            OnRequestManagePackagesDialog(this, EventArgs.Empty);
        }

        private bool CanShowInstalledPackages(object parameters)
        {
            return true;
        }

        private void ManagePackagePaths(object parameters)
        {
            OnRequestPackagePathsDialog(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Change the currently visible workspace to a custom node's workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void FocusCustomNodeWorkspace(Guid symbol)
        {
            if (symbol == null)
            {
                throw new Exception(Resources.MessageNodeWithNullFunction);
            }

            if (model.OpenCustomNodeWorkspace(symbol))
            {
                //set the zoom and offsets events
                CurrentSpace.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(CurrentSpace.X, CurrentSpace.Y)));
                CurrentSpaceViewModel.OnZoomChanged(this, new ZoomEventArgs(CurrentSpaceViewModel.Zoom));
            }
        }

        internal void ShowElement(NodeModel e)
        {
            if (HomeSpace.RunSettings.RunType == RunType.Automatic)
                return;

            if (!model.CurrentWorkspace.Nodes.Contains(e))
            {
                if (HomeSpace != null && HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    model.CurrentWorkspace = HomeSpace;
                }
                else
                {
                    foreach (
                        var customNodeWorkspace in
                            model.CustomNodeManager.LoadedWorkspaces.Where(
                                customNodeWorkspace => customNodeWorkspace.Nodes.Contains(e)))
                    {
                        FocusCustomNodeWorkspace(customNodeWorkspace.CustomNodeId);
                        break;
                    }
                }
            }

            this.CurrentSpaceViewModel.OnRequestCenterViewOnElement(this, new ModelEventArgs(e));
        }

        private void CancelActiveState(NodeModel node)
        {
            WorkspaceViewModel wvm = this.CurrentSpaceViewModel;

            if (wvm.IsConnecting && (node == wvm.FirstActiveConnector.ActiveStartPort.Owner))
                wvm.CancelActiveState();
        }

        /// <summary>
        /// Present the new function dialogue and create a custom function.
        /// </summary>
        /// <param name="parameter"></param>
        private void ShowNewFunctionDialogAndMakeFunction(object parameter)
        {           
            var args = new FunctionNamePromptEventArgs();
            this.Model.OnRequestsFunctionNamePrompt(this, args);

            if (args.Success)
            {
                this.ExecuteCommand(new DynamoModel.CreateCustomNodeCommand(Guid.NewGuid(),
                    args.Name, args.Category, args.Description, true));
                this.ShowStartPage = false;
            }
        }

        private bool CanShowNewFunctionDialogCommand(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Present the new preset dialogue and add a new presetModel 
        /// to the preset set on this graph
        /// </summary>
        private void ShowNewPresetStateDialogAndMakePreset(object parameter)
        {
            var selectedNodes = GetInputNodesFromSelectionForPresets().ToList();

            //If there are NO input nodes then show the error message
            if (!selectedNodes.Any())
            {
                this.OnRequestPresetWarningPrompt();
            }
            else
            {
                //trigger the event to request the display
                //of the preset name dialogue
                var args = new PresetsNamePromptEventArgs();
                this.Model.OnRequestPresetNamePrompt(args);

                //Select only Input nodes for preset
                var ids = selectedNodes.Select(x => x.GUID).ToList();
                if (args.Success)
                {
                    this.ExecuteCommand(new DynamoModel.AddPresetCommand(args.Name, args.Description, ids));
                }
                
                //Presets created - this will enable the Restore / Delete presets
                RaisePropertyChanged("EnablePresetOptions");     
            }
          
        }
        private bool CanShowNewPresetStateDialog(object parameter)
        {
            RaisePropertyChanged("EnablePresetOptions");
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        private void CreateNodeFromSelection(object parameter)
        {
            CurrentSpaceViewModel.CollapseSelectedNodes();
        }


        private static bool CanCreateNodeFromSelection(object parameter)
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>().Any();
        }

        /// <summary>
        /// Returns the selected nodes that are "input" nodes, and makes an 
        /// exception for CodeBlockNodes and Filename nodes as these are marked 
        /// false so they do not expose a IsInput checkbox
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<NodeModel> GetInputNodesFromSelectionForPresets()
        {
            return DynamoSelection.Instance.Selection.OfType<NodeModel>()
                .Where(
                    x => x.IsInputNode ||
                    x is CodeBlockNodeModel ||

                    // NOTE: The Filename node is being matched by name due to the node definition
                    //       being in the CoreNodeModels project instead of the DynamoCore project.
                    //       After some discussions it was decided that this was the least bad way to 
                    //       make this check (versus either adding a new, overridable property to 
                    //       NodeModel, or moving Filename and the associated classes into DynamoCore).
                    x.GetType().Name == "Filename");
        }

        public void ShowSaveDialogIfNeededAndSaveResult(object parameter)
        {
            var vm = this;

            if (string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FileName))
            {
                if (CanShowSaveDialogAndSaveResult(parameter))
                    ShowSaveDialogAndSaveResult(parameter);
            }
            else
            {
                if (CanSave(parameter))
                    Save(parameter);
            }
        }

        internal bool CanShowSaveDialogIfNeededAndSaveResultCommand(object parameter)
        {
            return true;
        }

        public void ShowSaveDialogAndSaveResult(object parameter)
        {
            var vm = this;

            FileDialog _fileDialog = vm.GetSaveDialog(vm.Model.CurrentWorkspace);

            // If the filePath is not empty set the default directory
            if (!string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(vm.Model.CurrentWorkspace.FileName);
                _fileDialog.InitialDirectory = fi.DirectoryName;
                _fileDialog.FileName = fi.Name;
            }
            else if (vm.Model.CurrentWorkspace is CustomNodeWorkspaceModel)
            {
                var pathManager = vm.model.PathManager;
                _fileDialog.InitialDirectory = pathManager.DefaultUserDefinitions;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveAs(_fileDialog.FileName);
            }
        }

        internal bool CanShowSaveDialogAndSaveResult(object parameter)
        {
            return true;
        }

        public void ToggleFullscreenWatchShowing(object parameter)
        {
            if (BackgroundPreviewViewModel == null) return;
            BackgroundPreviewViewModel.Active = !BackgroundPreviewViewModel.Active;
        }

        internal bool CanToggleFullscreenWatchShowing(object parameter)
        {
            return true;
        }

        public void ToggleBackgroundGridVisibility(object parameter)
        {
            if (BackgroundPreviewViewModel == null || !BackgroundPreviewViewModel.Active) return;

            BackgroundPreviewViewModel.IsGridVisible = !BackgroundPreviewViewModel.IsGridVisible;
        }

        internal bool CanToggleBackgroundGridVisibility(object parameter)
        {
            return true;
        }

        public void GoToWorkspace(object parameter)
        {
            if (parameter is Guid && model.CustomNodeManager.Contains((Guid)parameter))
            {
                FocusCustomNodeWorkspace((Guid)parameter);
            }
        }

        internal bool CanGoToWorkspace(object parameter)
        {
            return true;
        }

        public void AlignSelected(object param)
        {
            //this.CurrentSpaceViewModel.AlignSelectedCommand.Execute(param);
            this.CurrentSpaceViewModel.AlignSelectedCommand.Execute(param.ToString());
        }

        internal bool CanAlignSelected(object param)
        {
            return this.CurrentSpaceViewModel.AlignSelectedCommand.CanExecute(param);
        }

        public void DoGraphAutoLayout(object parameter)
        {
            this.CurrentSpaceViewModel.GraphAutoLayoutCommand.Execute(parameter);
        }

        internal bool CanDoGraphAutoLayout(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Resets the offset and the zoom for a view
        /// </summary>
        public void GoHomeView(object parameter)
        {
            CurrentSpaceViewModel.Zoom = 1.0;

            var ws = this.Model.Workspaces.First(x => x == model.CurrentWorkspace);
            ws.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(0, 0)));
        }

        internal bool CanGoHomeView(object parameter)
        {
            return true;
        }

        public void GoHome(object _)
        {
            model.CurrentWorkspace = HomeSpace;
        }

        public bool CanGoHome(object _)
        {
            return ViewingHomespace;
        }

        public void SelectAll(object parameter)
        {
            this.CurrentSpaceViewModel.SelectAll(null);
        }

        internal bool CanSelectAll(object parameter)
        {
            return this.CurrentSpaceViewModel.CanSelectAll(null);
        }

        public void MakeNewHomeWorkspace(object parameter)
        {
            if (ClearHomeWorkspaceInternal())
                this.ShowStartPage = false; // Hide start page if there's one.
        }

        internal bool CanMakeNewHomeWorkspace(object parameter)
        {
            return HomeSpace.RunSettings.RunEnabled;
        }

        private void CloseHomeWorkspace(object parameter)
        {
            if (ClearHomeWorkspaceInternal())
            {
                // If after closing the HOME workspace, and there are no other custom 
                // workspaces opened at the time, then we should show the start page.
                this.ShowStartPage = (Model.Workspaces.Count() <= 1);
            }
        }

        private bool CanCloseHomeWorkspace(object parameter)
        {
            return HomeSpace.RunSettings.RunEnabled;
        }

        /// <summary>
        /// TODO(Ben): Both "CloseHomeWorkspace" and "MakeNewHomeWorkspace" are 
        /// quite close in terms of functionality, but because their callers 
        /// have different expectations in different scenarios, they remain 
        /// separate now. A new task has been scheduled for them to be unified 
        /// into one consistent way of handling.
        /// 
        ///     http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3813
        /// 
        /// </summary>
        /// <returns>Returns true if the home workspace has been saved and 
        /// cleared, or false otherwise.</returns>
        /// 
        private bool ClearHomeWorkspaceInternal()
        {
            // if the workspace is unsaved, prompt to save
            // otherwise overwrite the home workspace with new workspace
            if (!HomeSpace.HasUnsavedChanges || AskUserToSaveWorkspaceOrCancel(HomeSpace))
            {
                Model.CurrentWorkspace = HomeSpace;

                model.ClearCurrentWorkspace();

                return true;
            }

            return false;
        }

        public void Exit(object allowCancel)
        {
            var allowCancellation = true;
            if (allowCancel != null)
                allowCancellation = ((bool)allowCancel);

            PerformShutdownSequence(new ShutdownParams(
                shutdownHost: false, allowCancellation: allowCancellation));
        }

        internal bool CanExit(object allowCancel)
        {
            return !model.ShutdownRequested;
        }

        /// <summary>
        /// Requests a message box asking the user to save the workspace and allows saving.
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        /// <returns>False if the user cancels, otherwise true</returns>
        public bool AskUserToSaveWorkspaceOrCancel(WorkspaceModel workspace, bool allowCancel = true)
        {
            var args = new WorkspaceSaveEventArgs(workspace, allowCancel);
            OnRequestUserSaveWorkflow(this, args);
            if (!args.Success)
                return false;
            return true;
        }

        internal bool CanAddToSelection(object parameters)
        {
            var node = parameters as NodeModel;
            if (node == null)
            {
                return false;
            }

            return true;
        }

        internal bool CanClear(object parameter)
        {
            return true;
        }

        internal void Delete(object parameters)
        {
            if (null != parameters) // See above for details of this exception.
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }

            var command = new DynamoModel.DeleteModelCommand(Guid.Empty);
            this.ExecuteCommand(command);
        }

        internal bool CanDelete(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }


        public void SaveImage(object parameters)
        {
            OnRequestSaveImage(this, new ImageSaveEventArgs(parameters.ToString()));

            Dynamo.Logging.Analytics.TrackCommandEvent("ImageCapture",
                "NodeCount", CurrentSpace.Nodes.Count());
        }

        private void Save3DImage(object parameters)
        {
            // Save the parameters
            OnRequestSave3DImage(this, new ImageSaveEventArgs(parameters.ToString()));
        }

        internal bool CanSaveImage(object parameters)
        {
            return true;
        }

        public void ShowSaveImageDialogAndSaveResult(object parameter)
        {
            FileDialog _fileDialog = null;

            if (_fileDialog == null)
            {
                _fileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = ".png",
                    FileName = Resources.FileDialogDefaultPNGName,
                    Filter = string.Format(Resources.FileDialogPNGFiles, "*.png"),
                    Title = Resources.SaveWorkbenToImageDialogTitle
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(model.CurrentWorkspace.FileName);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() != DialogResult.OK) return;
            if (!CanSaveImage(_fileDialog.FileName)) return;

            if (parameter == null)
            {
                SaveImage(_fileDialog.FileName);
                return;
            }

            if (parameter.ToString() == Resources.ScreenShotFrom3DParameter)
            {
                Save3DImage(_fileDialog.FileName);
            }
            else if (parameter.ToString() == Resources.ScreenShotFrom3DShortcutParameter)
            {
                if (BackgroundPreviewViewModel.CanNavigateBackground)
                {
                    Save3DImage(_fileDialog.FileName);
                }
                else
                {
                    SaveImage(_fileDialog.FileName);
                }
            }
            else
            {
                SaveImage(_fileDialog.FileName);
            }
        }

        internal bool CanShowSaveImageDialogAndSaveResult(object parameter)
        {
            return true;
        }

        private void Undo(object parameter)
        {
            var command = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            this.ExecuteCommand(command);
        }

        private bool CanUndo(object parameter)
        {
            var workspace = model.CurrentWorkspace;
            return ((null == workspace) ? false : workspace.CanUndo);
        }

        private void Redo(object parameter)
        {
            var command = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Redo);
            this.ExecuteCommand(command);
        }

        private bool CanRedo(object parameter)
        {
            var workspace = model.CurrentWorkspace;
            return ((null == workspace) ? false : workspace.CanRedo);
        }

        internal void RaiseCanExecuteUndoRedo()
        {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        public void ToggleConsoleShowing(object parameter)
        {
            if (ConsoleHeight == 0)
            {
                ConsoleHeight = 100;
            }
            else
            {
                ConsoleHeight = 0;
            }
        }

        internal bool CanToggleConsoleShowing(object parameter)
        {
            return true;
        }

        public void SelectNeighbors(object parameters)
        {
            List<ISelectable> sels = DynamoSelection.Instance.Selection.ToList<ISelectable>();

            foreach (ISelectable sel in sels)
            {
                if (sel is NodeModel)
                    ((NodeModel)sel).SelectNeighbors();
            }
        }

        internal bool CanSelectNeighbors(object parameters)
        {
            return true;
        }

        public void ShowConnectors(object parameter)
        {
        }

        internal bool CanShowConnectors(object parameter)
        {
            return true;
        }

        public void SetConnectorType(object parameters)
        {
            if (parameters.ToString() == "BEZIER")
            {
                ConnectorType = ConnectorType.BEZIER;
            }
            else
            {
                ConnectorType = ConnectorType.POLYLINE;
            }
        }

        internal bool CanSetConnectorType(object parameters)
        {
            //parameter object will be BEZIER or POLYLINE
            if (string.IsNullOrEmpty(parameters.ToString()))
            {
                return false;
            }
            return true;
        }

        public void GoToWiki(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoWikiLink));
        }

        internal bool CanGoToWiki(object parameter)
        {
            return true;
        }

        public void GoToSourceCode(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.GitHubDynamoLink));
        }

        internal bool CanGoToSourceCode(object parameter)
        {
            return true;
        }

        public void GoToDictionary(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoDictionary));
        }

        internal bool CanGoToDictionary(object parameter)
        {
            return true;
        }

        private void DisplayStartPage(object parameter)
        {
            this.ShowStartPage = true;
        }

        private bool CanDisplayStartPage(object parameter)
        {
            return !this.ShowStartPage;
        }

        internal void Pan(object parameter)
        {
            Debug.WriteLine(string.Format("Offset: {0},{1}, Zoom: {2}", CurrentSpaceViewModel.X, CurrentSpaceViewModel.Y, currentWorkspaceViewModel.Zoom));
            var panType = parameter.ToString();
            double pan = 10;
            var pt = new Point2D(CurrentSpaceViewModel.X, CurrentSpaceViewModel.Y);

            switch (panType)
            {
                case "Left":
                    pt.X += pan;
                    break;
                case "Right":
                    pt.X -= pan;
                    break;
                case "Up":
                    pt.Y += pan;
                    break;
                case "Down":
                    pt.Y -= pan;
                    break;
            }
            model.CurrentWorkspace.X = pt.X;
            model.CurrentWorkspace.Y = pt.Y;

            CurrentSpaceViewModel.Model.OnCurrentOffsetChanged(this, new PointEventArgs(pt));
            CurrentSpaceViewModel.ResetFitViewToggleCommand.Execute(parameter);
        }

        private bool CanPan(object parameter)
        {
            return true;
        }

        internal void ZoomIn(object parameter)
        {
            if (BackgroundPreviewViewModel != null && 
                BackgroundPreviewViewModel.CanNavigateBackground)
            {
                var op = ViewOperationEventArgs.Operation.ZoomIn;
                OnRequestViewOperation(new ViewOperationEventArgs(op));
                return;
            }

            CurrentSpaceViewModel.ZoomInInternal();
            ZoomInCommand.RaiseCanExecuteChanged();
        }

        private bool CanZoomIn(object parameter)
        {
            return CurrentSpaceViewModel.CanZoomIn;
        }

        private void ZoomOut(object parameter)
        {
            if (BackgroundPreviewViewModel != null && 
                BackgroundPreviewViewModel.CanNavigateBackground)
            {
                var op = ViewOperationEventArgs.Operation.ZoomOut;
                OnRequestViewOperation(new ViewOperationEventArgs(op));
                return;
            }

            CurrentSpaceViewModel.ZoomOutInternal();
            ZoomOutCommand.RaiseCanExecuteChanged();
        }

        private bool CanZoomOut(object parameter)
        {
            return CurrentSpaceViewModel.CanZoomOut;
        }

        private void FitView(object parameter)
        {
            if (BackgroundPreviewViewModel.CanNavigateBackground)
            {
                var op = ViewOperationEventArgs.Operation.FitView;
                OnRequestViewOperation(new ViewOperationEventArgs(op));
                return;
            }

            CurrentSpaceViewModel.FitViewInternal();
        }

        private bool CanFitView(object parameter)
        {
            return true;
        }

        public void ImportLibrary(object parameter)
        {
            string[] fileFilter = {string.Format(Resources.FileDialogLibraryFiles, "*.dll; *.ds" ), string.Format(Resources.FileDialogAssemblyFiles, "*.dll"), 
                                   string.Format(Resources.FileDialogDesignScriptFiles, "*.ds"), string.Format(Resources.FileDialogAllFiles,"*.*")};
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = String.Join("|", fileFilter);
            openFileDialog.Title = Resources.ImportLibraryDialogTitle;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        EngineController.ImportLibrary(file);

                        FileInfo info = new FileInfo(file);
                        if (this.Model.AddPackagePath(info.Directory.FullName, info.Name))
                        {
                            string title = Resources.PackagePathAutoAddNotificationTitle;
                            string shortMessage = Resources.PackagePathAutoAddNotificationShortDescription;
                            string detailedMessage = Resources.PackagePathAutoAddNotificationDetailedDescription;
                            this.Model.Logger.LogNotification(
                                "Dynamo", 
                                title,
                                shortMessage, 
                                string.Format(detailedMessage, file));
                        }
                    }
                    SearchViewModel.SearchAndUpdateResults();
                }
                catch(LibraryLoadFailedException ex)
                {
                    System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }
        }

        internal bool CanImportLibrary(object parameter)
        {
            return true;
        }

        public void Escape(object parameter)
        {
            CurrentSpaceViewModel.CancelActiveState();

            // Since panning and orbiting modes are exclusive from one another,
            // turning one on may turn the other off. This is the reason we must
            // raise property change for both at the same time to update visual.
            RaisePropertyChanged("IsPanning");
            RaisePropertyChanged("IsOrbiting");
        }

        internal bool CanEscape(object parameter)
        {
            return true;
        }

        internal bool CanShowInfoBubble(object parameter)
        {
            return true;
        }

        private void ExportToSTL(object parameter)
        {
            FileDialog _fileDialog = null ?? new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".stl",
                FileName = Resources.FileDialogDefaultSTLModelName,
                Filter = string.Format(Resources.FileDialogSTLModels,"*.stl"),
                Title = Resources.SaveModelToSTLDialogTitle,
            };

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(model.CurrentWorkspace.FileName);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                BackgroundPreviewViewModel.ExportToSTL(_fileDialog.FileName, HomeSpace.Name);

                Dynamo.Logging.Analytics.TrackCommandEvent("ExportToSTL");
            }
        }

        internal bool CanExportToSTL(object parameter)
        {
            return true;
        }

        private bool CanShowAboutWindow(object obj)
        {
            return true;
        }

        private void ShowAboutWindow(object obj)
        {
            OnRequestAboutWindow(this);
        }

        private void SetNumberFormat(object parameter)
        {
            model.PreferenceSettings.NumberFormat = parameter.ToString();
        }

        private bool CanSetNumberFormat(object parameter)
        {
            return true;
        }

        #region Shutdown related methods

        /// <summary>
        /// This struct represents parameters for PerformShutdownSequence call.
        /// It exposes several properties to control the way shutdown process goes.
        /// </summary>
        /// 
        public struct ShutdownParams
        {
            public ShutdownParams(
                bool shutdownHost,
                bool allowCancellation)
                : this(shutdownHost, allowCancellation, true) { }

            public ShutdownParams(
                bool shutdownHost,
                bool allowCancellation,
                bool closeDynamoView)
                : this()
            {
                ShutdownHost = shutdownHost;
                AllowCancellation = allowCancellation;
                CloseDynamoView = closeDynamoView;
            }

            /// <summary>
            /// The call to PerformShutdownSequence results in the host 
            /// application being shutdown if this property is set to true.
            /// </summary>
            internal bool ShutdownHost { get; private set; }

            /// <summary>
            /// If this property is set to true, the user is given
            /// an option to cancel the shutdown process.
            /// </summary>
            internal bool AllowCancellation { get; private set; }

            /// <summary>
            /// Set this to true to close down DynamoView as part of shutdown 
            /// process. This is typically desirable for calls originated from 
            /// within the DynamoViewModel layer to shutdown Dynamo. If the 
            /// shutdown is initiated by DynamoView when it is being closed, 
            /// then this should be set to false since DynamoView is already 
            /// being closed.
            /// </summary>
            internal bool CloseDynamoView { get; private set; }
        }

        private bool shutdownSequenceInitiated = false;

        /// <summary>
        /// Call this method to initiate DynamoModel shutdown sequence.
        /// See the definition of ShutdownParams structure for more details.
        /// </summary>
        /// <param name="shutdownParams">A set of parameters that control the 
        /// way in which shutdown sequence is to be performed. See ShutdownParams
        /// for more details.</param>
        /// <returns>Returns true if the shutdown sequence is started, or false 
        /// otherwise (i.e. when user chooses not to proceed with shutting down 
        /// Dynamo).</returns>
        /// 
        public bool PerformShutdownSequence(ShutdownParams shutdownParams)
        {
            if (shutdownSequenceInitiated)
            {
                // There was a prior call to shutdown. This could happen for example
                // when user presses 'ALT + F4' to close the DynamoView, the 'Exit' 
                // handler calls this method to close Dynamo, which in turn closes 
                // the DynamoView ('OnRequestClose' below). When DynamoView closes,
                // its "Window.Closing" event fires and "DynamoView.WindowClosing" 
                // gets called before 'PerformShutdownSequence' is called again.
                // 
                return true;
            }

            if (!AskUserToSaveWorkspacesOrCancel(shutdownParams.AllowCancellation))
                return false;

            // 'shutdownSequenceInitiated' is marked as true here indicating 
            // that the shutdown may not be stopped.
            shutdownSequenceInitiated = true;

            // Request the View layer to close its window (see 
            // ShutdownParams.CloseDynamoView member for details).
            if (shutdownParams.CloseDynamoView)
                OnRequestClose(this, EventArgs.Empty);

            BackgroundPreviewViewModel.Dispose();

            model.ShutDown(shutdownParams.ShutdownHost);
            if (shutdownParams.ShutdownHost)
            {
                model.UpdateManager.HostApplicationBeginQuit();
            }

            UsageReportingManager.DestroyInstance();

            return true;
        }

        /// <summary>
        /// Ask the user if they want to save any unsaved changes.
        /// </summary>
        /// <param name="allowCancel">Whether to show cancel button to user. </param>
        /// <returns>Returns true if the cleanup is completed and that the shutdown 
        /// can proceed, or false if the user chooses to cancel the operation.</returns>
        /// 
        private bool AskUserToSaveWorkspacesOrCancel(bool allowCancel = true)
        {
            if (automationSettings != null)
            {
                // In an automation run, Dynamo should not be asking user to save 
                // the modified file. Instead it should be shutting down, leaving 
                // behind unsaved changes (if saving is desired, then the save command 
                // should have been recorded for the test case to it can be replayed).
                // 
                if (automationSettings.IsInPlaybackMode)
                    return true; // In playback mode, just exit without saving.
            }

            foreach (var wvm in Workspaces.Where((wvm) => wvm.Model.HasUnsavedChanges))
            {
                //if (!AskUserToSaveWorkspaceOrCancel(wvm.Model, allowCancel))
                //    return false;

                var args = new WorkspaceSaveEventArgs(wvm.Model, allowCancel);
                OnRequestUserSaveWorkflow(this, args);
                if (!args.Success)
                    return false;
            }
            return true;
        }

        #endregion
    }
}

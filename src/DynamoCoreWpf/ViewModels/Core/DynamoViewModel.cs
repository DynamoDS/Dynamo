using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Exceptions;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Scheduler;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.Visualization;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Core.Converters;
using Dynamo.Wpf.ViewModels.FileTrust;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoMLDataPipeline;
using DynamoServices;
using DynamoUtilities;
using ICSharpCode.AvalonEdit;
using J2N.Text;
using Newtonsoft.Json;
using PythonNodeModels;
using ISelectable = Dynamo.Selection.ISelectable;
using WpfResources = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.ViewModels
{
    public interface IDynamoViewModel : INotifyPropertyChanged
    {
        ObservableCollection<WorkspaceViewModel> Workspaces { get; set; }
    }

    public partial class DynamoViewModel : ViewModelBase, IDynamoViewModel
    {
        #region properties
        public Window Owner { get; set; }
        private readonly DynamoModel model;
        private Point transformOrigin;
        private bool showStartPage = false;
        private PreferencesViewModel preferencesViewModel;
        private string dynamoMLDataPath = string.Empty;
        private const string dynamoMLDataFileName = "DynamoMLDataPipeline.json";

        // Can the user run the graph
        private bool CanRunGraph => HomeSpace.RunSettings.RunEnabled && !HomeSpace.GraphRunInProgress;
        private ObservableCollection<DefaultWatch3DViewModel> watch3DViewModels = new ObservableCollection<DefaultWatch3DViewModel>();
        private ObservableCollection<TabItem> sideBarTabItems = new ObservableCollection<TabItem>();

        /// <summary>
        /// An observable collection of workspace view models which tracks the model.
        /// </summary>
        private ObservableCollection<WorkspaceViewModel> workspaces = new ObservableCollection<WorkspaceViewModel>();

        /// <summary>
        /// Set of node window id's that are currently docked in right side sidebar.
        /// </summary>
        internal HashSet<string> DockedNodeWindows { get; set; } = new HashSet<string>();

        /// <summary>
        ///  Node window's state, either DockRight or FloatingWindow.
        /// </summary>
        internal Dictionary<string, ViewExtensionDisplayMode> NodeWindowsState { get; set; } = new Dictionary<string, ViewExtensionDisplayMode>();

        internal DynamoMLDataPipelineExtension MLDataPipelineExtension { get; set; }

        internal Dictionary<string, NodeSearchElementViewModel> DefaultAutocompleteCandidates;

        /// <summary>
        /// Collection of Right SideBar tab items: view extensions and docked windows.
        /// </summary>
        public ObservableCollection<TabItem> SideBarTabItems
        {
            get
            {
                return sideBarTabItems;
            }
            set
            {
                sideBarTabItems = value;
                RaisePropertyChanged(nameof(SideBarTabItems));
            }
        }

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

        internal PreferencesViewModel PreferencesViewModel
        {
            get
            {
                return preferencesViewModel;
            }
        }
        /// <summary>
        /// Denotes the last location used to open or close a workspace.
        /// </summary>
        internal string LastSavedLocation { get; set; }


        /// <summary>
        /// Guided Tour Manager
        /// </summary>
        public GuidesManager MainGuideManager { get; set; }

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

        /// <summary>
        /// Controls if the the ML data ingestion pipeline is beta from feature flag
        /// </summary>
        internal bool IsDNADataIngestionPipelineinBeta
        {
            get
            {
                return DynamoModel.FeatureFlags?.CheckFeatureFlag("IsDNADataIngestionPipelineinBeta", true) ?? true;
            }
        }

        /// <summary>
        /// Controls if the cluster node autocomplete placement feature is enabled from feature flag
        /// </summary>
        internal bool IsDNAClusterPlacementEnabled
        {
            get
            {
                return DynamoModel.FeatureFlags?.CheckFeatureFlag("IsDNAClusterPlacementEnabled", false) ?? false;
            }
        }

        /// <summary>
        /// Controls if the new DNA Flyout is enabled from preference settings.
        /// </summary>
        internal bool IsNewDNAUIEnabled
        {
            get
            {
                return model.PreferenceSettings.EnableNewNodeAutoCompleteUI;
            }
        }

        /// <summary>
        /// Count of unresolved issues on the linter manager.
        /// This is used for binding in the NotificationsControl
        /// </summary>
        public int LinterIssuesCount
        {
            get => Model.LinterManager?.RuleEvaluationResults.Count ?? 0;
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

        /// <summary>
        /// This property is the ViewModel that will be passed to the File Trust Warning popup when is created.
        /// </summary>
        internal FileTrustWarningViewModel FileTrustViewModel { get; set; }

        private WorkspaceViewModel currentWorkspaceViewModel;
        private string filePath;
        private string fileContents;
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
                    ExecuteCommand(new DynamoModel.SwitchTabCommand(modelIndex));
                    (HomeSpaceViewModel as HomeWorkspaceViewModel)?.UpdateRunStatusMsgBasedOnStates();
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

                if (DisplayInteractiveGuideCommand != null)
                    DisplayInteractiveGuideCommand.RaiseCanExecuteChanged();

                if(ShowInsertDialogAndInsertResultCommand != null)
                    ShowInsertDialogAndInsertResultCommand.RaiseCanExecuteChanged();
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

        private double minLeftMarignOffset;
        /// <summary>
        /// The
        /// </summary>
        public double MinLeftMarginOffset
        {
            get => minLeftMarignOffset;
            set
            {
                if(minLeftMarignOffset != value)
                {
                    minLeftMarignOffset = value;
                    RaisePropertyChanged(nameof(MinLeftMarginOffset));
                }
            }
        }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        [Obsolete("This was moved to PreferencesViewModel.cs")]
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
        /// Indicates if line numbers should be displayed on code block nodes.
        /// </summary>
        [Obsolete("This was moved to PreferencesViewModel.cs")]
        public bool ShowCodeBlockLineNumber
        {
            get
            {
                return model.PreferenceSettings.ShowCodeBlockLineNumber;
            }
            set
            {
                model.PreferenceSettings.ShowCodeBlockLineNumber = value;
                RaisePropertyChanged(nameof(ShowCodeBlockLineNumber));
            }
        }

        /// <summary>
        /// Indicates whether to make T-Spline nodes (under ProtoGeometry.dll) discoverable
        /// in the node search library.
        /// </summary>
        [Obsolete("This was moved to PreferencesViewModel.cs")]
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

        /// <summary>
        /// Indicates whether to enabled node Auto Complete feature for port interaction.
        /// </summary>
        public bool EnableNodeAutoComplete
        {
            get
            {
                return PreferenceSettings.EnableNodeAutoComplete;
            }
            set
            {
                PreferenceSettings.EnableNodeAutoComplete = value;
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
                RaisePropertyChanged(nameof(IsShowingConnectors));
            }
        }
        /// <summary>
        /// Relaying the flag `IsShowingConnectorTooltip' coming from
        /// the Dynamo model.
        /// </summary>
        public bool IsShowingConnectorTooltip
        {
            get
            {
                return model.IsShowingConnectorTooltip;
            }
            set
            {
                model.IsShowingConnectorTooltip = value;
                RaisePropertyChanged(nameof(IsShowingConnectorTooltip));
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
            get { return DynamoModel.Version; }
        }

        public string HostVersion
        {
            get { return model.HostVersion; }
        }

        public string HostName
        {
            get { return model.HostName; }
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

        [Obsolete("This Property will be obsoleted in a future version of Dynamo")]
        internal SearchViewModel SearchViewModel { get; private set; }

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

        public bool HideReportOptions { get; internal set; }

        private DynamoPythonScriptEditorTextOptions editTextOptions = new DynamoPythonScriptEditorTextOptions();
        /// <summary>
        /// Gets/Sets the text editor options for python script editor.
        /// </summary>
        internal DynamoPythonScriptEditorTextOptions PythonScriptEditorTextOptions
        {
            get
            {
                return editTextOptions;
            }
        }


        /// <summary>
        /// Indicates if the whitespaces and tabs should be visible in the python script editor.
        /// This property is for the global whitespace toggle option in settings menu.
        /// </summary>
        public bool ShowTabsAndSpacesInScriptEditor
        {
            get
            {
                return model.PreferenceSettings.ShowTabsAndSpacesInScriptEditor;
            }
            set
            {
                PythonScriptEditorTextOptions.ShowWhiteSpaceCharacters(value);
                model.PreferenceSettings.ShowTabsAndSpacesInScriptEditor = value;
                RaisePropertyChanged(nameof(ShowTabsAndSpacesInScriptEditor));
            }
        }

        /// <summary>
        /// Engine used by default for new Python script and string nodes. If not empty, this takes precedence over any system settings.
        /// </summary>
        [Obsolete ("This was moved to PreferencesViewModel.cs")]
        public string DefaultPythonEngine
        {
            get { return model.PreferenceSettings.DefaultPythonEngine; }
            set
            {
                if (value != model.PreferenceSettings.DefaultPythonEngine)
                {
                    model.PreferenceSettings.DefaultPythonEngine = value;
                    RaisePropertyChanged(nameof(DefaultPythonEngine));
                }
            }
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

            /// <summary>
            /// If true, Analytics and Usage options are hidden from UI
            /// </summary>
            public bool HideReportOptions { get; set; }
        }

        public static DynamoViewModel Start(StartConfiguration startConfiguration = new StartConfiguration())
        {
            if (startConfiguration.DynamoModel == null)
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

        private void SearchDefaultNodeAutocompleteCandidates()
        {
            var tempSearchViewModel = new SearchViewModel(this)
            {
                Visible = true
            };
            DefaultAutocompleteCandidates = new Dictionary<string, NodeSearchElementViewModel>();

            // TODO: These are basic input types in Dynamo
            // This should be only served as a temporary default case.
            var queries = new List<string>() { "String", "Number Slider", "Integer Slider", "Number", "Boolean", "Watch", "Watch 3D", "Python Script" };
            var categories = new List<(string, SearchElementGroup)> { (".List", SearchElementGroup.Create), (".List", SearchElementGroup.Query) };

            var addNodeIfValid = (NodeSearchElement nse) =>
            {
                var node = nse != null ? tempSearchViewModel.MakeNodeSearchElementVM(nse) : null;
                if (node != null)
                    DefaultAutocompleteCandidates.Add(node.Name, node);
            };

            foreach (var query in queries)
            {
                addNodeIfValid(tempSearchViewModel.Model.Entries.FirstOrDefault(n => n.Name == query));
            }

            foreach(var query in categories)
            {
                var categoryNse = tempSearchViewModel.Model.Entries.Where(n => n.FullCategoryName.EndsWith(query.Item1) && n.Group == query.Item2);
                foreach (var item in categoryNse)
                {
                    addNodeIfValid(item);
                }
            }

            tempSearchViewModel.Dispose();
        }

        protected DynamoViewModel(StartConfiguration startConfiguration)
        {
            // CurrentDomain_UnhandledException - catches unhandled exceptions that are fatal to the current process. These exceptions cannot be handled and process termination is guaranteed
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Dispatcher.CurrentDispatcher.UnhandledException - catches unhandled exceptions from the UI thread. Can mark exceptions as handled (and close Dynamo) so that host apps can continue running normally even though Dynamo crashed
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;
            // TaskScheduler.UnobservedTaskException - catches unobserved Task exceptions from all threads. Does not crash Dynamo, we only log the exceptions and do not call CER or close Dynamo
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            this.ShowLogin = startConfiguration.ShowLogin;

            // initialize core data structures
            this.model = startConfiguration.DynamoModel;
            this.model.CommandStarting += OnModelCommandStarting;
            this.model.CommandCompleted += OnModelCommandCompleted;
            this.model.RequestsCrashPrompt += CrashReportTool.ShowCrashWindow;

            this.HideReportOptions = startConfiguration.HideReportOptions;
            UsageReportingManager.Instance.InitializeCore(this);
            this.WatchHandler = startConfiguration.WatchHandler;
            var pmExtension = model.GetPackageManagerExtension();

            if (pmExtension != null)
            {
                this.PackageManagerClientViewModel = new PackageManagerClientViewModel(this, pmExtension.PackageManagerClient);
            }

            this.SearchViewModel = null;

            // Start page should not show up during test mode.
            this.ShowStartPage = !DynamoModel.IsTestMode;

            this.BrandingResourceProvider = startConfiguration.BrandingResourceProvider ?? new DefaultBrandingResourceProvider();

            // commands should be initialized before adding any WorkspaceViewModel
            InitializeDelegateCommands();

            //add the initial workspace and register for future
            //updates to the workspaces collection
            if(!Model.IsServiceMode)
            {
                SearchDefaultNodeAutocompleteCandidates();
            }

            var homespaceViewModel = new HomeWorkspaceViewModel(model.CurrentWorkspace as HomeWorkspaceModel, this);
            workspaces.Add(homespaceViewModel);
            currentWorkspaceViewModel = homespaceViewModel;

            model.WorkspaceAdded += WorkspaceAdded;
            model.WorkspaceRemoved += WorkspaceRemoved;
            if (model.LinterManager != null)
            {
                model.LinterManager.RuleEvaluationResults.CollectionChanged += OnRuleEvaluationResultsCollectionChanged;
            }

            SubscribeModelCleaningUpEvent();
            SubscribeModelUiEvents();
            SubscribeModelChangedHandlers();
            SubscribeModelBackupFileSaveEvent();

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
            model.RequestNotification += model_RequestNotification;

            preferencesViewModel = new PreferencesViewModel(this);

            dynamoMLDataPath = Path.Combine(Model.PathManager.UserDataDirectory, dynamoMLDataFileName);

            if (!DynamoModel.IsTestMode && !DynamoModel.IsHeadless)
            {
                model.State = DynamoModel.DynamoModelState.StartedUI;

                // deserialize workspace checksum hashes that is used for Dynamo ML data pipeline.
                if (File.Exists(dynamoMLDataPath))
                {
                    try
                    {
                        Model.GraphChecksumDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(dynamoMLDataPath));
                    }
                    catch (Exception ex)
                    {
                        Model.Logger.Log($"Failed to deserialize {dynamoMLDataFileName} : {ex.Message}", LogLevel.File);
                    }
                }
            }

            FileTrustViewModel = new FileTrustWarningViewModel();
            MLDataPipelineExtension = model.ExtensionManager.Extensions.OfType<DynamoMLDataPipelineExtension>().FirstOrDefault();
            IsIDSDKInitialized();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var crashData = new CrashErrorReportArgs(e.Exception);
                Model?.Logger?.LogError($"Unobserved task exception: {crashData.Details}");
                Analytics.TrackException(e.Exception, true);
            }
            catch
            { }
        }

        private void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Handled || DynamoModel.IsCrashing)
            {
                return;
            }

            if (DynamoModel.IsTestMode)
            {
                // Do not handle the exception in test mode.
                // Let the test host handle it.
            }
            else
            {
                // Try to handle the exception so that the host app can continue (in most cases).
                // In some cases Dynamo code might still crash after this handler kicks in. In these edge cases
                // we might see 2 CER windows (the extra one from the host app) - CER tool might handle this in the future.
                e.Handled = true;
            }

            CrashGracefully(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!DynamoModel.IsCrashing)//Avoid duplicate CER reports
            {
                CrashGracefully(e.ExceptionObject as Exception, fatal: true);
            }
        }

        // CrashGracefully should only be used in the DynamoViewModel class or within tests.
        internal void CrashGracefully(Exception ex, bool fatal = false)
        {
            try
            {
                var exceptionAssembly = ex.TargetSite?.Module?.Assembly;
                // TargetInvocationException is the exception that is thrown by methods invoked through reflection
                // The inner exception contains the originating exception.
                // https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/7.0/reflection-invoke-exceptions
                if (ex is TargetInvocationException && ex.InnerException != null)
                {
                    exceptionAssembly = ex.InnerException?.TargetSite?.Module?.Assembly;
                }

                // Do not crash if the exception is coming from a 3d party package;
                if (!fatal && exceptionAssembly != null)
                {
                    // Check if the exception might be coming from a loaded package assembly.
                    var faultyPkg = Model.GetPackageManagerExtension()?.PackageLoader?.LocalPackages?.FirstOrDefault(p => exceptionAssembly.Location.StartsWith(p.RootDirectory, StringComparison.OrdinalIgnoreCase));
                    if (faultyPkg != null)
                    {
                        var crashDetails = new CrashErrorReportArgs(ex);
                        DynamoConsoleLogger.OnLogErrorToDynamoConsole($"Unhandled exception coming from package {faultyPkg.Name} was handled: {crashDetails.Details}");
                        Analytics.TrackException(ex, false);
                        return;
                    }
                }

                DynamoModel.IsCrashing = true;
                var crashData = new CrashErrorReportArgs(ex);
                DynamoConsoleLogger.OnLogErrorToDynamoConsole($"Unhandled exception: {crashData.Details} ");
                Analytics.TrackException(ex, true);

                if (DynamoModel.IsTestMode)
                {
                    // Do not show the crash UI during tests.
                    return;
                }

                Model?.OnRequestsCrashPrompt(crashData);

                if (fatal)
                {
                    // Fatal exception. Close Dynamo but do not terminate the process.

                    // Run the Dynamo exit code in the UI thread since CrashGracefully could be called in other threads too.
                    TryDispatcherInvoke(() => {
                        try
                        {
                            Exit(false);
                        }
                        catch { }
                    });

                    // Do not terminate the process in the plugin, because other AppDomain.UnhandledException events will not get a chance to get called
                    // ex. A host app (like Revit) could have an AppDomain.UnhandledException too.
                    // If we terminate the process here, the host app will not get a chance to gracefully shut down.
                    // Environment.Exit(1);
                }
                else
                {
                    // Non fatal exception.

                    // We run the Dynamo exit call asyncronously in the dispatcher to ensure that any continuation of code
                    // manages to run to completion before we start shutting down Dynamo.
                    TryDispatcherBeginInvoke(() => {
                        try
                        {
                            Exit(false);
                        }
                        catch
                        { }
                    });
                }
            }
            catch
            { }
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
            var factoryVm = (RenderPackageFactoryViewModel)sender;
            switch (e.PropertyName)
            {
                case "ShowEdges":
                    model.PreferenceSettings.ShowEdges = factoryVm.Factory.TessellationParameters.ShowEdges;
                    // A full regeneration is required to get the edge geometry.
                    foreach (var vm in Watch3DViewModels)
                    {
                        if (vm is HelixWatch3DViewModel) // just need a full regeneration when vm is HelixWatch3DViewModel
                        {
                            vm.RegenerateAllPackages();
                        }
                    }
                    break;
                case "UseRenderInstancing":
                    model.PreferenceSettings.UseRenderInstancing = factoryVm.Factory.TessellationParameters.UseRenderInstancing;
                    // A full regeneration is required to use instancing.
                    foreach (var vm in Watch3DViewModels)
                    {
                        if (vm is HelixWatch3DViewModel) // just need a full regeneration when vm is HelixWatch3DViewModel
                        {
                            vm.RegenerateAllPackages();
                        }
                    }
                    break;
                case "MaxTessellationDivisions":
                    model.PreferenceSettings.RenderPrecision = factoryVm.Factory.TessellationParameters.MaxTessellationDivisions;
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
                        // Reset state machine
                        CurrentSpaceViewModel.CancelActiveState();
                    }
                    break;
            }
        }

        internal event EventHandler WindowRezised;
        internal void OnWindowResized(object underThreshold)
        {
            if(WindowRezised != null)
            {
                WindowRezised(underThreshold, new EventArgs());
            }
        }

        internal event EventHandler PreferencesWindowChanged;
        internal void OnPreferencesWindowChanged(object preferencesView)
        {
            if (PreferencesWindowChanged != null)
            {
                PreferencesWindowChanged(preferencesView, new EventArgs());
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

        /// <summary>
        /// Returns whether the IDSDK is initialized or not for Dynamo Sandbox, in host environment defaults to true.
        /// If showWarning is true, a warning message will be shown to the user if the IDSDK is not initialized.
        /// If owner is not null, the warning message will be shown as a dialog with the owner as the parent.
        /// </summary>
        internal bool IsIDSDKInitialized(bool showWarning = true, Window owner = null)
        {
            if (!Model.AuthenticationManager.IsIDSDKInitialized())
            {
                if (showWarning)
                {
                    var ownerWindow = owner ?? Owner ?? System.Windows.Application.Current.MainWindow;
                    DynamoMessageBox.Show(ownerWindow, WpfResources.IDSDKErrorMessage, WpfResources.IDSDKErrorMessageTitle, true, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return false;
            }
            return true;
        }

        #region Event handler destroy/create

        protected virtual void UnsubscribeAllEvents()
        {
            preferencesViewModel.UnsubscribeAllEvents();
            UnsubscribeDispatcherEvents();
            UnsubscribeModelUiEvents();
            UnsubscribeModelChangedEvents();
            UnsubscribeLoggerEvents();
            UnsubscribeModelCleaningUpEvent();
            UnsubscribeModelBackupFileSaveEvent();

            model.WorkspaceAdded -= WorkspaceAdded;
            model.WorkspaceRemoved -= WorkspaceRemoved;
            if (model.LinterManager != null)
            {
                model.LinterManager.RuleEvaluationResults.CollectionChanged -= OnRuleEvaluationResultsCollectionChanged;
            }

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

        private void SubscribeModelUiEvents()
        {
            model.RequestBugReport += ReportABug;
            model.RequestDownloadDynamo += DownloadDynamo;
            model.Preview3DOutage += Disable3DPreview;

            DynamoServices.LoadLibraryEvents.LoadLibraryFailure += LoadLibraryEvents_LoadLibraryFailure;
        }

        private void UnsubscribeModelUiEvents()
        {
            model.RequestBugReport -= ReportABug;
            model.RequestDownloadDynamo -= DownloadDynamo;
            model.Preview3DOutage -= Disable3DPreview;

            DynamoServices.LoadLibraryEvents.LoadLibraryFailure -= LoadLibraryEvents_LoadLibraryFailure;
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

        // InitializeAutomationSettings initializes the Dynamo automationSettings object.
        // This method is meant to be accessed only within this class and within tests.
        internal void InitializeAutomationSettings(string commandFilePath)
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
            BackgroundPreviewViewModel.RefreshState();
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
            var urlWithParameters = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(bodyContent, CrashUtilities.ReportType.Bug);

            // launching the process using explorer.exe will format the URL incorrectly
            // and Github will not recognise the query parameters in the URL
            // so launch with default operating system web browser
            Process.Start(new ProcessStartInfo(urlWithParameters) { UseShellExecute = true });
        }

        public static void ReportABug()
        {
            ReportABug(null);
        }

        internal static void DownloadDynamo()
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoDownloadLink) { UseShellExecute = true });
        }

        private void Disable3DPreview()
        {
            foreach (var item in Watch3DViewModels)
            {
                item.Active = false;
            }
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
            PublishSelectedNodesCommand?.RaiseCanExecuteChanged();
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
                    //RaisePropertyChanged("RunEnabled");
                    if (!DynamoModel.IsTestMode)
                    {
                        ExitGuidedTourIfOpened();
                    }
                    break;

                case "EnablePresetOptions":
                    RaisePropertyChanged("EnablePresetOptions");
                    break;
            }
        }

        private void ExitGuidedTourIfOpened()
        {
            if (GuideFlowEvents.IsAnyGuideActive)
                MainGuideManager.ExitTour();
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

        internal bool CanUpdatePythonNodeEngine(object parameter)
        {
            if (DynamoSelection.Instance.Selection.Count > 0 && SelectionHasPythonNodes())
            {
                return true;
            }
            return false;
        }
        private bool SelectionHasPythonNodes()
        {
            if (GetSelectedPythonNodes().Any())
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Updates the engine for the Python nodes,
        /// if the nodes belong to another workspace (like custom nodes), they will be opened silently.
        /// </summary>
        /// <param name="pythonNode"></param>
        /// <param name="engine"></param>
        internal void UpdatePythonNodeEngine(PythonNodeBase pythonNode, string engine)
        {
            try
            {
                var workspaceGUID = Guid.Empty;
                var cnWorkspace = GetCustomNodeWorkspace(pythonNode);
                if (cnWorkspace != null)
                {
                    workspaceGUID = cnWorkspace.Guid;
                    FocusCustomNodeWorkspace(cnWorkspace.CustomNodeId, true);
                }
                this.ExecuteCommand(
                    new DynamoModel.UpdateModelValueCommand(
                        workspaceGUID, pythonNode.GUID, nameof(pythonNode.EngineName), engine));
                pythonNode.OnNodeModified();
                Analytics.TrackEvent(Actions.Switch, Categories.PythonOperations, engine);
                Model.Logger.Log("Updated python node(" + pythonNode.GUID.ToString() + ") engine to use " + engine, LogLevel.Console);
            }
            catch(Exception ex)
            {
                Model.Logger.Log("Failed to update Python node engine: " + ex.Message, LogLevel.Console);
            }

        }
        internal void UpdateAllPythonEngine(object param)
        {
            var pNodes = GetSelectedPythonNodes(Model.CurrentWorkspace.Nodes);
            if (pNodes.Count == 0) return;
            var result = MessageBoxService.Show(
                        Owner,
                        string.Format(Resources.UpdateAllPythonEngineWarning, pNodes.Count, param.ToString()),
                        Resources.UpdateAllPythonEngineWarningTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                pNodes.ForEach(x => UpdatePythonNodeEngine(x, param.ToString()));
            }
        }
        internal bool CanUpdateAllPythonEngine(object param)
        {
            return true;
        }

        /// <summary>
        /// Adds the python engine to the menu items and subscribes to their click event for updating the engine.
        /// </summary>
        /// <param name="pythonNodeModel">List of python nodes</param>
        /// <param name="pythonEngineVersionMenu">context menu item to which the engines will be added to</param>
        /// <param name="updateEngineDelegate">Update event handler, to trigger engine update for the node</param>
        /// <param name="engineName">Python engine to be added</param>
        /// <param name="isBinding">Should be set to true, if you require to bind the passed
        /// NodeModel engine value with the menu item, works only when a single node is passed in the list.</param>
        internal void AddPythonEngineToMenuItems(List<PythonNodeBase> pythonNodeModel,
           MenuItem pythonEngineVersionMenu,
           RoutedEventHandler updateEngineDelegate,
           string engineName, bool isBinding = false)
        {
            //if all nodes in the selection are set to a specific engine, then that engine will be checked in the list.
            bool hasCommonEngine = pythonNodeModel.All(x => x.EngineName == engineName);
            var currentItem = pythonEngineVersionMenu.Items.Cast<MenuItem>().FirstOrDefault(x => x.Header as string == engineName);
            if (currentItem != null)
            {
                if (pythonNodeModel.Count == 1) return;
                currentItem.IsChecked = hasCommonEngine;
                return;
            }
            MenuItem pythonEngineItem = null;
            //if single node, then checked property is bound to the engine value, as python node context menu is not recreated
            if (pythonNodeModel.Count == 1 && isBinding)
            {
                var pythonNode = pythonNodeModel.FirstOrDefault(); ;
                pythonEngineItem = new MenuItem { Header = engineName, IsCheckable = false };
                pythonEngineItem.SetBinding(MenuItem.IsCheckedProperty, new System.Windows.Data.Binding(nameof(pythonNode.EngineName))
                {
                    Source = pythonNode,
                    Converter = new CompareToParameterConverter(),
                    ConverterParameter = engineName
                });
            }
            else
            {
                //when updating multiple nodes checked value is not bound to any specific node,
                //rather takes into account all the selected nodes
                pythonEngineItem = new MenuItem { Header = engineName, IsCheckable = true };
                pythonEngineItem.IsChecked = hasCommonEngine;
            }
            pythonEngineItem.Click += updateEngineDelegate;
            pythonEngineVersionMenu.Items.Add(pythonEngineItem);
        }
        /// <summary>
        /// Gets the Python nodes from the provided list, including python nodes inside custom nodes as well.
        /// If no list is provided then the current selection will be considered.
        /// </summary>
        /// <returns></returns>
        internal List<PythonNodeBase> GetSelectedPythonNodes(IEnumerable<NodeModel> nodes = null)
        {
            if (nodes == null)
            {
                nodes = DynamoSelection.Instance.Selection.OfType<NodeModel>();
            }
            var selectedPythonNodes = nodes.OfType<PythonNodeBase>().ToList();
            var customNodes = nodes.Where(x => x.IsCustomFunction).ToList();
            if (customNodes.Count > 0)
            {
                foreach (var cNode in customNodes)
                {
                    var customNodeFunction = cNode as Function;
                    var pythonNodesInCN = customNodeFunction?.Definition.FunctionBody.OfType<PythonNodeBase>().ToList();
                    if (pythonNodesInCN.Count > 0)
                    {
                        selectedPythonNodes.AddRange(pythonNodesInCN);
                    }
                }
            }
            return selectedPythonNodes;
        }
        private CustomNodeWorkspaceModel GetCustomNodeWorkspace(NodeModel node)
        {
            var wg = model.CustomNodeManager.LoadedWorkspaces.Where(x => x.Nodes.Contains(node)).FirstOrDefault();
            return wg ?? null;
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

            var command = new DynamoModel.CreateAnnotationCommand(Guid.NewGuid(), null, null, 0, 0, true);
            this.ExecuteCommand(command);
        }

        internal bool CanAddAnnotation(object parameter)
        {
            var groups = Model.CurrentWorkspace.Annotations;
            // Create Group should be disabled when a group is among selection and come with nested group inside
            if (groups.Any(x => x.IsSelected && x.HasNestedGroups))
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
                if (!group.IsExpanded)
                {
                    // If the group is not expanded we expanded
                    // and show the group content before deleting
                    // the group.
                    var viewModel = this.CurrentSpaceViewModel.Annotations
                        .FirstOrDefault(x => x.AnnotationModel == group);

                    if (viewModel is null) continue;

                    viewModel.IsExpanded = true;
                    viewModel.ShowGroupContents();
                }

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

            // If only an AnnotationModel is selected that means
            // we are trying to remove a group from another group
            if (DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Count() == DynamoSelection.Instance.Selection.Count())
            {
                foreach (var modelb in DynamoSelection.Instance.Selection.OfType<ModelBase>().ToList())
                {
                    var command = new DynamoModel.UngroupModelCommand(modelb.GUID);
                    this.ExecuteCommand(command);
                    return;
                }
            }

            //Check for multiple groups - Delete the group and not the nodes.
            foreach (var modelb in DynamoSelection.Instance.Selection.OfType<ModelBase>().ToList())
            {
                if (!(modelb is AnnotationModel))
                {
                    var guids = new List<Guid> { modelb.GUID };
                    if (modelb is NodeModel node)
                    {
                        var pinnedNotes = CurrentSpaceViewModel.Notes
                            .Where(x => x.PinnedNode?.NodeModel == node)
                            .Select(x => x.Model.GUID);

                        if (pinnedNotes.Any())
                        {
                            guids.AddRange(pinnedNotes);
                        }
                    }
                    var command = new DynamoModel.UngroupModelCommand(guids);
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
                if (!(modelb is AnnotationModel) &&
                    !CurrentSpace.Annotations.ContainsModel(modelb))
                {
                    var command = new DynamoModel.AddModelToGroupCommand(modelb.GUID);
                    this.ExecuteCommand(command);
                }
            }
        }

        internal void AddGroupToGroup(object hostGroupGuid)
        {
            if (!(hostGroupGuid is Guid hostGroupId))
            {
                var message = "Internal error, argument must be null";
                throw new ArgumentException(message, "parameters");
            }
            //Check for multiple groups - Delete the group and not the nodes.
            var command = new DynamoModel.AddGroupToGroupCommand(
                DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Where(x => x.GUID != hostGroupId).Select(x => x.GUID),
                hostGroupId);
            this.ExecuteCommand(command);
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
                    ExtraWorkspaceViewInfo viewInfo = ExtraWorkspaceViewInfo.ExtraWorkspaceViewInfoFromJson(fileContents);
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

        private void OnRuleEvaluationResultsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(LinterIssuesCount));
        }

        /// <summary>
        /// Adds the path to the list of recent files.
        /// We don't do anything if the file is already added and is at the first place,
        /// we move the file to the start of the list if it is already present,
        /// or add it to the start of the list if it is not present.
        /// Every other event, except Move will refresh all the recent files.
        /// </summary>
        /// <param name="path"></param>
        internal void AddToRecentFiles(string path)
        {
            if (path == null) return;

            var currIdx = RecentFiles.IndexOf(path);
            if (currIdx == 0) return;
            else if (currIdx > 0)
            {
                RecentFiles.Move(currIdx, 0);
                return;
            }

            RecentFiles.Insert(0, path);
            UpdateRecentFiles();
        }

        /// <summary>
        /// Update recent files list and limits the number of recent files to the maximum number of recent files as set in the preferences.
        /// </summary>
        internal void UpdateRecentFiles()
        {
            int maxNumRecentFiles = Model.PreferenceSettings.MaxNumRecentFiles;
            if (RecentFiles.Count > maxNumRecentFiles)
            {
                RecentFiles.RemoveRange(maxNumRecentFiles, RecentFiles.Count - maxNumRecentFiles);
            }
        }

        // Get the nodemodel if a node is present in any open workspace.
        internal NodeModel GetDockedWindowNodeModel(string tabId)
        {
            var workspaces = Model.Workspaces;
            NodeModel nodeModel = null;

            foreach (WorkspaceModel workspace in workspaces)
            {
                nodeModel = workspace.Nodes.FirstOrDefault(x => x.GUID.ToString() == tabId);
                if (nodeModel != null)
                {
                    return nodeModel;
                }
            }
            return nodeModel;
        }

        /// <summary>
        /// Closes any docked python script windows if there are no unsaved changes.
        /// Show warning message on the script editor if it has unsaved changes.
        /// </summary>
        /// <param name="nodes">Nodes in the workspace</param>
        internal bool CanCloseDockedNodeWindows(ObservableCollection<NodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                var id = node.NodeModel.GUID.ToString();
                if (DockedNodeWindows.Contains(id))
                {
                    var tabItem = SideBarTabItems.OfType<TabItem>().SingleOrDefault(n => n.Uid.ToString() == id);

                    if (tabItem != null)
                    {
                        NodeModel nodeModel = GetDockedWindowNodeModel(tabItem.Uid);

                        if (nodeModel is PythonNode pythonNode)
                        {
                            var editor = (tabItem.Content as Grid).ChildOfType<TextEditor>();
                            if (editor != null && editor.IsModified)
                            {
                                pythonNode.OnWarnUserScript();
                                tabItem.Focus();
                                return false;
                            }
                            pythonNode.Dispose();
                        }

                        SideBarTabItems.Remove(tabItem);
                        DockedNodeWindows.Remove(id);
                    }
                }
            }
            return true;
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
        /// Attempts to open a file using the Json content passed to OpenFromJsonCommand, but wraps
        /// the call with a check to make sure no unsaved changes to the HomeWorkspace are lost.
        /// </summary>
        /// <param name="openCommand"> <see cref="DynamoModel.OpenFileFromJsonCommand"/> </param>
        private void OpenFromJsonIfSaved(object openCommand)
        {
            filePath = string.Empty;
            fileContents = string.Empty;

            var command = openCommand as DynamoModel.OpenFileFromJsonCommand;
            if (command == null)
            {
                return;
            }

            if (HomeSpace != null && HomeSpace.HasUnsavedChanges)
            {
                if (AskUserToSaveWorkspaceOrCancel(HomeSpace))
                {
                    fileContents = command.FileContents;
                    ExecuteCommand(command);
                    ShowStartPage = false;
                }
            }
            else
            {
                OpenFromJsonCommand.Execute(new Tuple<string, bool>(command.FileContents, command.ForceManualExecutionMode));
                ShowStartPage = false;
            }
        }

        /// <summary>
        /// Attempts to open a file using the passed open command, but wraps the call
        /// with a check to make sure no unsaved changes to the HomeWorkspace are lost.
        /// </summary>
        /// <param name="openCommand"> <see cref="DynamoModel.OpenFileCommand"/> </param>
        private void OpenIfSaved(object openCommand)
        {
            fileContents = string.Empty;
            filePath = string.Empty;

            var command = openCommand as DynamoModel.OpenFileCommand;
            if (command == null)
            {
                return;
            }

            if(HomeSpace != null && HomeSpace.HasUnsavedChanges)
            {
                if (AskUserToSaveWorkspaceOrCancel(HomeSpace))
                {
                    filePath = command.FilePath;
                    ExecuteCommand(command);
                    ShowStartPage = false;
                }
            }
            else
            {
                OpenCommand.Execute(new Tuple<string,bool>(command.FilePath, command.ForceManualExecutionMode));
                ShowStartPage = false;
            }
        }

        /// <summary>
        /// Open a definition or workspace.
        /// For most cases, parameters variable refers to the Json content file to open
        /// However, when this command is used in OpenFileDialog, the variable is
        /// a Tuple{string,bool} instead. The boolean flag is used to override the
        /// RunSetting of the workspace.
        /// </summary>
        /// <param name="parameters"></param>
        private void OpenFromJson(object parameters)
        {
            // try catch for exceptions thrown while opening files, say from a future version,
            // that can't be handled reliably
            filePath = string.Empty;
            fileContents = string.Empty;
            bool forceManualMode = false;
            try
            {
                var packedParams = parameters as Tuple<string, bool>;
                if (packedParams != null)
                {
                    fileContents = packedParams.Item1;
                    forceManualMode = packedParams.Item2;
                }
                else
                {
                    fileContents = parameters as string;
                }
                ExecuteCommand(new DynamoModel.OpenFileFromJsonCommand(fileContents, forceManualMode));
            }
            catch (Exception e)
            {
                if (!DynamoModel.IsTestMode)
                {
                    string commandString = String.Format(Resources.MessageErrorOpeningFileGeneral);
                    string errorMsgString;
                    if (e is Newtonsoft.Json.JsonReaderException)
                    {
                        errorMsgString = String.Format(Resources.MessageFailedToOpenCorruptedFile, "Json file content");
                    }
                    else
                    {
                        errorMsgString = String.Format(Resources.MessageUnkownErrorOpeningFile, "Json file content");
                    }
                    model.Logger.LogNotification(Configurations.DynamoAsString, commandString, errorMsgString, e.ToString());
                    MessageBoxService.Show(
                        Owner,
                        errorMsgString,
                        Properties.Resources.FileLoadFailureMessageBoxTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
                else
                {
#pragma warning disable CA2200 // Rethrow to preserve stack details
                    throw e;
#pragma warning restore CA2200 // Rethrow to preserve stack details
                }
                return;
            }
            this.ShowStartPage = false; // Hide start page if there's one.
        }

        /// <summary>
        /// Open a definition or workspace.
        /// For most cases, parameters variable refers to the file path to open
        /// However, when this command is used in OpenFileDialog, the variable is
        /// a Tuple{string, bool} instead. When this command is used in OpenTemplateDialog,
        /// the variable is a Tuple{string, bool} instead. The second boolean flag is
        /// used to override the RunSetting of the workspace.
        /// </summary>
        /// <param name="parameters"></param>
        private void Open(object parameters)
        {
            if (CurrentSpaceViewModel != null && CurrentSpaceViewModel.HasUnsavedChanges)
            {
                if (!AskUserToSaveWorkspaceOrCancel(HomeSpace))
                    return;
            }

            // try catch for exceptions thrown while opening files, say from a future version,
            // that can't be handled reliably
            filePath = string.Empty;
            fileContents = string.Empty;
            bool forceManualMode = false;
            bool isTemplate = false;
            try
            {
                if (parameters is Tuple<string, bool> packedParams)
                {
                    filePath = packedParams.Item1;
                    forceManualMode = packedParams.Item2;
                }
                else if (parameters is Tuple<string, bool, bool> tupleParams)
                {
                    filePath = tupleParams.Item1;
                    forceManualMode = tupleParams.Item2;
                    isTemplate = tupleParams.Item3;
                }
                else
                {
                    filePath = parameters as string;
                }

                var directoryName = Path.GetDirectoryName(filePath);

                // Display trust warning when file is not among trust location and warning feature is on
                bool displayTrustWarning = !PreferenceSettings.IsTrustedLocation(directoryName)
                    && !filePath.EndsWith("dyf")
                    && !DynamoModel.IsTestMode
                    && !PreferenceSettings.DisableTrustWarnings
                    && FileTrustViewModel != null;
                RunSettings.ForceBlockRun = displayTrustWarning;
                // Execute graph open command
                ExecuteCommand(new DynamoModel.OpenFileCommand(filePath, forceManualMode, isTemplate));

                // Apply annotation updates based on the preference setting
                RefreshAnnotationDescriptions();

                // Only show trust warning popop when current opened workspace is homeworkspace and not custom node workspace
                if (displayTrustWarning && (currentWorkspaceViewModel?.IsHomeSpace ?? false))
                {
                    // Skip these when opening dyf
                    FileTrustViewModel.DynFileDirectoryName = directoryName;
                    FileTrustViewModel.ShowWarningPopup = true;
                    (HomeSpaceViewModel as HomeWorkspaceViewModel).UpdateRunStatusMsgBasedOnStates();
                    FileTrustViewModel.AllowOneTimeTrust = false;
                }
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
                    model.Logger.LogNotification(Configurations.DynamoAsString, commandString, errorMsgString, e.ToString());
                    MessageBoxService.Show(
                        Owner,
                        errorMsgString,
                        commandString,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    this.ShowStartPage = true;  // Triggers start page change to notify the frontend
                }
                else
                {
#pragma warning disable CA2200 // Rethrow to preserve stack details
                    throw e;
#pragma warning restore CA2200 // Rethrow to preserve stack details
                }
                return;
            }
            this.ShowStartPage = false; // Hide start page if there's one.
        }

        /// <summary>
        /// Insert a definition or a custom node.
        /// For most cases, parameters variable refers to the file path to open
        /// However, when this command is used in InsertFileDialog, the variable is
        /// a Tuple&lt;string, bool&gt; instead. The boolean flag is used to override the
        /// RunSetting of the workspace.
        /// </summary>
        /// <param name="parameters"></param>
        private void Insert(object parameters)
        {
            // try catch for exceptions thrown while opening files, say from a future version,
            // that can't be handled reliably
            filePath = string.Empty;
            fileContents = string.Empty;
            bool forceManualMode = true;
            try
            {
                if (parameters is Tuple<string, bool> packedParams)
                {
                    filePath = packedParams.Item1;
                    forceManualMode = packedParams.Item2;
                }
                else
                {
                    filePath = parameters as string;
                }

                var directoryName = Path.GetDirectoryName(filePath);

                // Display trust warning when file is not among trust location and warning feature is on
                bool displayTrustWarning = !PreferenceSettings.IsTrustedLocation(directoryName)
                    && !filePath.EndsWith("dyf")
                    && !DynamoModel.IsTestMode
                    && !PreferenceSettings.DisableTrustWarnings
                    && FileTrustViewModel != null;
                RunSettings.ForceBlockRun = displayTrustWarning;
                // Execute graph open command
                ExecuteCommand(new DynamoModel.InsertFileCommand(filePath, forceManualMode));

                this.FitViewCommand.Execute(null);

                // Only show trust warning popup when current opened workspace is homeworkspace and not custom node workspace
                if (displayTrustWarning && (currentWorkspaceViewModel?.IsHomeSpace ?? false))
                {
                    // Skip these when opening dyf
                    FileTrustViewModel.DynFileDirectoryName = directoryName;
                    FileTrustViewModel.ShowWarningPopup = true;
                    (HomeSpaceViewModel as HomeWorkspaceViewModel).UpdateRunStatusMsgBasedOnStates();
                    FileTrustViewModel.AllowOneTimeTrust = false;
                }
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
                    model.Logger.LogNotification(Configurations.DynamoAsString, commandString, errorMsgString, e.ToString());
                    MessageBoxService.Show(errorMsgString, commandString, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
#pragma warning disable CA2200 // Rethrow to preserve stack details
                    throw e;
#pragma warning restore CA2200 // Rethrow to preserve stack details
                }
                return;
            }
            this.ShowStartPage = false; // Hide start page if there's one.
        }

        internal void OpenOnboardingGuideFile()
        {
            var jsonDynFile = ResourceUtilities.LoadContentFromResources(GuidesManager.OnboardingGuideWorkspaceEmbeededResource, Assembly.GetExecutingAssembly(), false, false);
            OpenFromJson(new Tuple<string, bool>(jsonDynFile, true));
        }

        private bool CanOpen(object parameters)
        {

            var filePath = parameters as string;

            if (!PathHelper.IsValidPath(filePath))
            {
                DynamoMessageBox.Show(Owner, string.Format(WpfResources.MessageErrorOpeningInvalidPath.Replace("\\n", Environment.NewLine), filePath), WpfResources.MessageErrorOpeningFileGeneral,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool CanOpenFromJson(object parameters)
        {
            string fileContents = parameters as string;
            return PathHelper.isFileContentsValidJson(fileContents, out _);
        }

        /// <summary>
        /// Forces all annotations in the workspace to refresh their description text,
        /// ensuring that UI updates when the ShowDefaultGroupDescription setting changes.
        /// </summary>
        public void RefreshAnnotationDescriptions()
        {
            foreach (var workspace in Model.Workspaces)
            {
                if (workspace.Annotations.Any())
                {
                    foreach (var group in workspace.Annotations)
                    {
                        var temp = group.AnnotationDescriptionText;
                        group.AnnotationDescriptionText = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Read the contents of the file and set the view parameters for that current workspace
        /// </summary>
        private void model_ComputeModelDeserialized()
        {
            try
            {
                filePath = Model.CurrentWorkspace.FileName;
                string fileContentsInUse = String.IsNullOrEmpty(filePath) ? fileContents : File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(fileContentsInUse))
                {
                    return;
                }

                // This call will fail in the case of an XML file
                ExtraWorkspaceViewInfo viewInfo = ExtraWorkspaceViewInfo.ExtraWorkspaceViewInfoFromJson(fileContentsInUse);

                Model.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(viewInfo);
                Model.OnWorkspaceOpening(viewInfo);
            }
            catch (Exception)
            {
                this.ShowStartPage = false; // Hide start page if there's one.
                return;
            }

            // TODO: Finish initialization of the WorkspaceViewModel
        }

        /// <summary>
        /// Create a toast notification with a notification sent by the DynamoModel
        /// </summary>
        /// <param name="notification"></param>
        private void model_RequestNotification(string notification, bool stayOpen = false)
        {
            this.MainGuideManager?.CreateRealTimeInfoWindow(notification, stayOpen);
            model?.Logger?.Log(notification);
        }

        /// <summary>
        /// Present the open dialog and open the workspace that is selected.
        /// - If template is selected, opens the template folder
        /// - else, if current file is saved , opens the file dialog at the current file's directory
        /// - else, opens the samples directory, if available
        /// - else , opens the last accessed path (or desktop, if it was the templates directory)
        /// </summary>
        /// <param name="parameter"></param>
        private void ShowOpenDialogAndOpenResult(object parameter)
        {
            if (HomeSpace.HasUnsavedChanges)
            {
                if (!AskUserToSaveWorkspaceOrCancel(HomeSpace))
                    return;
            }

            bool isTemplate = parameter != null && (parameter as string).Equals("Template");

            DynamoOpenFileDialog _fileDialog = new DynamoOpenFileDialog(this)
            {
                Filter = string.Format(Resources.FileDialogDynamoDefinitions,
                         BrandingResourceProvider.ProductName, "*.dyn;*.dyf") + "|" +
                         string.Format(Resources.FileDialogAllFiles, "*.*"),
                Title = string.Format(Resources.OpenDynamoDefinitionDialogTitle,BrandingResourceProvider.ProductName)
            };

            // If opening a template, use templates dir as the initial dir
            if (isTemplate && !string.IsNullOrEmpty(Model.PathManager.TemplatesDirectory))
            {
                string path = Model.PathManager.TemplatesDirectory;
                if (Directory.Exists(path))
                {
                    var di = new DirectoryInfo(Model.PathManager.TemplatesDirectory);
                    _fileDialog.InitialDirectory = di.FullName;
                }
            }
            // otherwise, if you've got the current space path, use it as the initial dir
            else if (!string.IsNullOrEmpty(Model.CurrentWorkspace.FileName))
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
            else if (string.IsNullOrEmpty(LastSavedLocation)) //If there is no last saved location, use the samples directory(if it exists)
            {
                Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
                string location = Path.GetDirectoryName(dynamoAssembly.Location);
                string UICulture = CultureInfo.CurrentUICulture.Name;
                string path = Path.Combine(location, "samples", UICulture);

                if (Directory.Exists(path))
                {
                    _fileDialog.InitialDirectory = path;
                }
                else
                {
                    SetDefaultInitialDirectory(_fileDialog);
                }
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (CanOpen(_fileDialog.FileName))
                {
                    if (isTemplate)
                    {
                        // File opening API which does not modify the original template file
                        Open(new Tuple<string, bool, bool>(_fileDialog.FileName, _fileDialog.RunManualMode, true));
                    }
                    else
                    {
                        Open(new Tuple<string, bool>(_fileDialog.FileName, _fileDialog.RunManualMode));
                        LastSavedLocation = Path.GetDirectoryName(_fileDialog.FileName);
                    }
                }
            }
        }

        private async void ShowOpenDialogAndOpenResultAsync(object parameter)
        {
            UIDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => ShowOpenDialogAndOpenResult(parameter));
        }

        private void SetDefaultInitialDirectory(DynamoOpenFileDialog _fileDialog)
        {
            try
            {
                //check if the last accessed path was the templates directory, if yes, change it to default
                var lastPath = _fileDialog.GetLastAccessedPath();
                if (!string.IsNullOrEmpty(lastPath))
                {
                    if (Path.GetFullPath(lastPath).Equals(Path.GetFullPath(Model.PathManager.TemplatesDirectory), StringComparison.OrdinalIgnoreCase))
                    {
                        //use the last saved location
                        if (!string.IsNullOrEmpty(LastSavedLocation) && !Path.GetFullPath(LastSavedLocation).Equals(Path.GetFullPath(Model.PathManager.TemplatesDirectory), StringComparison.OrdinalIgnoreCase))
                        {
                            _fileDialog.InitialDirectory = LastSavedLocation;
                        }
                        else
                        {
                            _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        private bool CanShowOpenDialogAndOpenResultCommand(object parameter) => CanRunGraph;

        /// <summary>
        /// Present the open dialog and open the workspace that is selected.
        /// </summary>
        /// <param name="parameter"></param>
        private void ShowInsertDialogAndInsertResult(object parameter)
        {
            var currentWorkspaceModelType = Model.CurrentWorkspace.GetType();
            var fileExtensions = currentWorkspaceModelType == typeof(CustomNodeWorkspaceModel) ? "*.dyf" : "*.dyn";

            DynamoOpenFileDialog _fileDialog = new DynamoOpenFileDialog(this, false)
            {
                Filter = string.Format(Resources.FileDialogDynamoDefinitions,
                         BrandingResourceProvider.ProductName, fileExtensions) + "|" +
                         string.Format(Resources.FileDialogAllFiles, "*.*"),
                Title = string.Format(Properties.Resources.InsertDialogBoxText, BrandingResourceProvider.ProductName)
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
                string UICulture = CultureInfo.CurrentUICulture.Name;
                string path = Path.Combine(location, "samples", UICulture);

                if (Directory.Exists(path))
                    _fileDialog.InitialDirectory = path;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (CanOpen(_fileDialog.FileName))
                {
                    Insert(new Tuple<string, bool>(_fileDialog.FileName, _fileDialog.RunManualMode));

                    if (HomeSpace.RunSettings.RunType != RunType.Manual)
                    {
                        MainGuideManager.CreateRealTimeInfoWindow(Properties.Resources.InsertGraphRunModeNotificationText);
                        HomeSpace.RunSettings.RunType = RunType.Manual;
                    }
                }
            }
        }

        private bool CanShowInsertDialogAndInsertResultCommand(object parameter)
        {
            return CanRunGraph && !this.showStartPage;
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
            return CanRunGraph;
        }

        /// <summary>
        /// Attempts to save an the current workspace.
        /// Assumes that workspace has already been saved.
        /// </summary>
        private void Save(object parameter)
        {
            if (!ShowWarningDialogOnSaveWithUnresolvedIssues()) return;

            if (!String.IsNullOrEmpty(Model.CurrentWorkspace.FileName))
            {
                // For read-only file, re-direct save to save-as
                if (this.CurrentSpace.IsReadOnly)
                    ShowSaveDialogAndSaveResult(parameter);
                else
                    InternalSaveAs(Model.CurrentWorkspace.FileName, SaveContext.Save);
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
            InternalSaveAs(fi.FullName, SaveContext.SaveAs);
        }

        internal bool CanSaveAs(object parameters)
        {
            return (parameters != null);
        }

        /// <summary>
        /// Indicates if the workspace has been changed based on node connections and store the checksum value of the graph.
        /// </summary>
        /// <returns></returns>
        private bool HasDifferentialCheckSum()
        {
            bool differentialChecksum = false;
            string graphId = Model.CurrentWorkspace.Guid.ToString();

            Model.GraphChecksumDictionary.TryGetValue(graphId, out List<string> checksums);

            // compare the current checksum with previous hash values.
            if (checksums != null)
            {
                if (!checksums.Contains(currentWorkspaceViewModel.CurrentCheckSum))
                {
                    checksums.Add(currentWorkspaceViewModel.CurrentCheckSum);
                    Model.GraphChecksumDictionary.Remove(graphId);
                    Model.GraphChecksumDictionary.Add(graphId, checksums);
                    differentialChecksum = true;
                }
            }
            else
            {
                Model.GraphChecksumDictionary.Add(graphId, new List<string>() { currentWorkspaceViewModel.CurrentCheckSum });
                differentialChecksum = true;
            }

            // if the checksum is different from previous hashes, serialize this new info.
            if (differentialChecksum)
            {
                try
                {
                    using (StreamWriter file = File.CreateText(dynamoMLDataPath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, Model.GraphChecksumDictionary);
                    }
                }
                catch (Exception ex)
                {
                    Model.Logger.Log("Failed to serialize " + dynamoMLDataFileName + "::" + ex.Message, LogLevel.File);
                }
            }

            return differentialChecksum;
        }

        private void InternalSaveAs(string path, SaveContext saveContext, bool isBackup = false)
        {
            try
            {
                Model.Logger.Log(string.Format(Properties.Resources.SavingInProgress, path));
                var hasSaved = false;
                if (path.Contains(Model.PathManager.TemplatesDirectory))
                {
                    // Give user notifications
                    DynamoMessageBox.Show(Owner, WpfResources.WorkspaceSaveTemplateDirectoryBlockMsg, WpfResources.WorkspaceSaveTemplateDirectoryBlockTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    hasSaved = CurrentSpaceViewModel.Save(path, isBackup, Model.EngineController, saveContext);
                }

                if (!isBackup && hasSaved)
                {
                    AddToRecentFiles(path);

                    // Track save and save-as operations on workspace, excluding the backup files.
                    if (saveContext.Equals(SaveContext.Save))
                    {
                        Analytics.TrackTaskFileOperationEvent(
                                  Path.GetFileName(path),
                                  Logging.Actions.Save,
                                  Model.CurrentWorkspace.Nodes.Count());
                    }
                    else if (saveContext.Equals(SaveContext.SaveAs))
                    {
                        Analytics.TrackTaskFileOperationEvent(
                                  Path.GetFileName(path),
                                  Logging.Actions.SaveAs,
                                  Model.CurrentWorkspace.Nodes.Count());
                    }
                }
            }
            catch (Exception ex)
            {
                Model.Logger.Log(ex.Message);
                Model.Logger.Log(ex.StackTrace);

                if (ex is IOException || ex is UnauthorizedAccessException)
                    MessageBoxService.Show(
                        Owner,
                        ex.Message,
                        Resources.UnsavedChangesMessageBoxTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Save the current workspace to a specific file path. If the file path is null or empty, an
        /// exception is written to the console.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="isBackup">Indicates if an automated backup save has called this function.</param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        [Obsolete("Please use the SaveAs method with the saveContext argument.")]
        public void SaveAs(string path, bool isBackup = false)
        {
            InternalSaveAs(path, SaveContext.SaveAs, isBackup: isBackup);
        }

        /// <summary>
        /// Save the current workspace to a specific file path. If the file path is null or empty, an
        /// exception is written to the console.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="saveContext">The context of the save operation.</param>
        /// <param name="isBackup">Indicates if an automated backup save has called this function.</param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void SaveAs(string path, SaveContext saveContext, bool isBackup = false)
        {
            InternalSaveAs(path, saveContext, isBackup);
        }

        /// <summary>
        /// Save the current workspace to a specific file path. If the file path is null or empty, an
        /// exception is written to the console.
        /// </summary>
        /// <param name="id">Indicate the id of target workspace view model instead of defaulting to
        /// current workspace view model. This is critical in crash cases.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="isBackup">Indicates if an automated backup save has called this function.</param>
        /// <param name="saveContext">The context of the save operation.</param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        internal void SaveAs(Guid id, string path, bool isBackup = false, SaveContext saveContext = SaveContext.None)
        {
            try
            {
                Model.Logger.Log(String.Format(Properties.Resources.SavingInProgress, path));
                var hasSaved = Workspaces.FirstOrDefault(w => w.Model.Guid == id).Save(
                    path, isBackup, Model.EngineController, saveContext);

                if (!isBackup && hasSaved) AddToRecentFiles(path);
            }
            catch (Exception ex)
            {
                Model.Logger.Log(ex.Message);
                Model.Logger.Log(ex.StackTrace);

                if (ex is IOException || ex is UnauthorizedAccessException)
                    MessageBoxService.Show(
                        Owner,
                        ex.Message,
                        string.Empty,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
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
                SaveAs(workspace.Guid, workspace.FileName);
                return true;
            }
            else
            {
                var fd = this.GetSaveDialog(workspace);
                // Since the workspace file directory is null, we set the initial directory
                // for the file to be MyDocument folder in the local computer.
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    SaveAs(workspace.Guid, fd.FileName);
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

        internal void ShowPackageManager(object parameters)
        {
            if (parameters == null)
            {
                OnRequestPackageManagerDialog(this, EventArgs.Empty);
            }
            else
            {
                //When we pass the PackageManagerSizeEventArgs means that we want to start the PackageManagerView with a specific Width and Height
                if (parameters is PackageManagerSizeEventArgs)
                {
                    OnRequestPackageManagerDialog(this, parameters as PackageManagerSizeEventArgs);
                }
                else
                {
                    var param = (string)parameters;
                    OnRequestPackageManagerDialog(this, new OpenPackageManagerEventArgs(param));
                }
            }
        }

        internal bool CanShowPackageManagerSearch(object parameters)
        {
            return !model.IsServiceMode && !model.NoNetworkMode;
        }

        internal bool CanShowPackageManager(object parameters)
        {
            return !model.IsServiceMode && !model.NoNetworkMode;
        }

        /// <summary>
        ///     Change the currently visible workspace to a custom node's workspace, unless the silent flag is set to true.
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        /// <param name="silent">When true, the focus will not switch to the workspace, but it will be opened silently.</param>
        internal void FocusCustomNodeWorkspace(Guid symbol, bool silent = false)
        {
            if (symbol == null)
            {
                throw new Exception(Resources.MessageNodeWithNullFunction);
            }
            var res = silent ? model.OpenCustomNodeWorkspaceSilent(symbol) : model.OpenCustomNodeWorkspace(symbol);
            if (res)
            {
                //set the zoom and offsets events
                CurrentSpace.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(CurrentSpace.X, CurrentSpace.Y)));
                CurrentSpaceViewModel.OnZoomChanged(this, new ZoomEventArgs(CurrentSpaceViewModel.Zoom));
            }
        }

        /// <summary>
        /// Attempts to find a NodeModel and focuses the view around it
        /// Default boolean introduced to allow for this method to be used in Automatic mode
        /// </summary>
        /// <param name="e"></param>
        /// <param name="forceShowElement"></param>
        internal void ShowElement(ModelBase e, bool forceShowElement = true)
        {
            if (HomeSpace.RunSettings.RunType == RunType.Automatic && forceShowElement)
                return;

            // Handle NodeModel
            if (e is NodeModel node)
            {
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
            }
            // Center the view on the model
            this.CurrentSpaceViewModel.OnRequestCenterViewOnElement(this, new ModelEventArgs(e));
            FitView(false);
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

                SetDefaultScaleFactor(Workspaces.FirstOrDefault(viewModels => viewModels.Model is CustomNodeWorkspaceModel));
                OnEnableShortcutBarItems(true);
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
        [Obsolete("The preset functionality is deprecated, DO NOT USE, will be removed in future version.")]
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
                {
                    ShowSaveDialogAndSaveResult(parameter);
                }
            }
            else
            {
                if (CanSave(parameter))
                {
                    Save(parameter);
                }
            }
        }

        internal bool CanShowSaveDialogIfNeededAndSaveResultCommand(object parameter)
        {
            return true;
        }

        public void ShowSaveDialogAndSaveResult(object parameter)
        {
            if (!ShowWarningDialogOnSaveWithUnresolvedIssues())
            {
                return;
            }

            var vm = this;

            try
            {
                FileDialog _fileDialog = vm.GetSaveDialog(vm.Model.CurrentWorkspace);

                // If the current workspace is a template use the last saved location.
                if (vm.Model.CurrentWorkspace.IsTemplate)
                {
                    var loc = string.IsNullOrEmpty(LastSavedLocation) ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) :
                        LastSavedLocation;
                    var fi = new DirectoryInfo(loc);
                    _fileDialog.InitialDirectory = fi.FullName;
                }
                // If the filePath is not empty and is not a template path, set the default directory
                else if (!vm.Model.CurrentWorkspace.IsTemplate && !string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FileName))
                {
                    var fi = new FileInfo(vm.Model.CurrentWorkspace.FileName);
                    _fileDialog.InitialDirectory = fi.DirectoryName;
                    _fileDialog.FileName = fi.Name;
                }

                if (vm.Model.CurrentWorkspace is CustomNodeWorkspaceModel)
                {
                    var pathManager = vm.model.PathManager;
                    _fileDialog.InitialDirectory = pathManager.DefaultUserDefinitions;
                }

                if (_fileDialog.ShowDialog() == DialogResult.OK)
                {
                    SaveAs(_fileDialog.FileName);
                    if(FileTrustViewModel != null)
                        FileTrustViewModel.DynFileDirectoryName = _fileDialog.FileName;
                    LastSavedLocation = Path.GetDirectoryName(_fileDialog.FileName);
                    //set the IsTemplate to false, after saving it as a file
                    vm.Model.CurrentWorkspace.IsTemplate = false;
                }
            }
            catch (PathTooLongException)
            {
                string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
                var args = new TaskDialogEventArgs(
                    new Uri(imageUri, UriKind.Relative),
                    WpfResources.GraphIssuesOnSavePath_Title,
                    WpfResources.GraphIssuesOnSavePath_Summary,
                    WpfResources.GraphIssuesOnSavePath_Description);

                args.AddRightAlignedButton((int)DynamoModel.ButtonId.Ok, WpfResources.OKButton);

                var dialog = new GenericTaskDialog(args);
                dialog.ShowDialog();
            }
        }

        private bool ShowWarningDialogOnSaveWithUnresolvedIssues()
        {
            if (Model.LinterManager is null ||
                Model.LinterManager.RuleEvaluationResults.Count <= 0)
            {
                return true;
            }

            string imageUri = "/DynamoCoreWpf;component/UI/Images/task_dialog_future_file.png";
            var args = new TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                WpfResources.GraphIssuesOnSave_Title,
                WpfResources.GraphIssuesOnSave_Summary,
                WpfResources.GraphIssuesOnSave_Description);

            args.AddRightAlignedButton((int)DynamoModel.ButtonId.Proceed, WpfResources.GraphIssuesOnSave_ProceedBtn);
            args.AddRightAlignedButton((int)DynamoModel.ButtonId.Cancel, WpfResources.GraphIssuesOnSave_CancelBtn);

            var dialog = new GenericTaskDialog(args);

            if (DynamoModel.IsTestMode)
            {
                dialog.Show();
                if (dialog.IsActive)
                {
                    var saveWarningArgs = new SaveWarningOnUnresolvedIssuesArgs(dialog);
                    OnSaveWarningOnUnresolvedIssuesShows(saveWarningArgs);
                }
                return false;
            }

            dialog.ShowDialog();

            // If cancel ('x' in top right corner) is pressed the ClickedButtonId on the
            // GenericTaskDialog is 0
            if (args.ClickedButtonId == (int)DynamoModel.ButtonId.Cancel ||
                args.ClickedButtonId == 0)
            {
                //Open Graph Status view extension
                OnViewExtensionOpenRequest("3467481b-d20d-4918-a454-bf19fc5c25d7");
                return false;
            }

            return true;
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
            if (BackgroundPreviewViewModel != null)
            {
                // This will change both the live object property and the PreferenceSettings for persistence
                BackgroundPreviewViewModel.IsGridVisible = !BackgroundPreviewViewModel.IsGridVisible;
            }
            else
            {
                PreferenceSettings.IsBackgroundGridVisible = !PreferenceSettings.IsBackgroundGridVisible;
            }
        }

        /// <summary>
        /// Updates grapic helpers (grid) inside the background preview VM
        /// </summary>
        /// <param name="parameter"></param>
        public void UpdateGraphicHelpersScale(object parameter)
        {
            if (BackgroundPreviewViewModel == null) return;

            BackgroundPreviewViewModel.GridScale = PreferenceSettings.GridScaleFactor;
            BackgroundPreviewViewModel.UpdateHelpers();
        }

        internal bool CanToggleBackgroundGridVisibility(object parameter)
        {
            return true;
        }

        internal bool CanUpdateGraphicHelpersScale(object parameter)
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
            if (CurrentSpaceViewModel.GraphAutoLayoutCommand.CanExecute(parameter))
            {
                CurrentSpaceViewModel.GraphAutoLayoutCommand.Execute(parameter);
            }
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

        public void UnpinAllPreviewBubbles(object parameter)
        {
            this.CurrentSpaceViewModel.UnpinAllPreviewBubbles(null);
        }

        internal bool CanUnpinAllPreviewBubbles(object parameter)
        {
            return this.CurrentSpaceViewModel.CanUnpinAllPreviewBubbles(null);
        }

        public void MakeNewHomeWorkspace(object parameter)
        {
            Analytics.TrackTaskFileOperationEvent(
                                  HomeSpace.Name,
                                  Actions.New,
                                  Convert.ToInt32(null));

            if (ClearHomeWorkspaceInternal())
            {
                var t = new DelegateBasedAsyncTask(model.Scheduler, () => model.ResetEngine());
                model.Scheduler.ScheduleForExecution(t);

                ShowStartPage = false; // Hide start page if there's one.

                SetDefaultScaleFactor(Workspaces.FirstOrDefault());
                OnEnableShortcutBarItems(true);
            }
        }

        internal bool CanMakeNewHomeWorkspace(object parameter) => CanRunGraph;

        private void CloseHomeWorkspace(object parameter)
        {
            // Tracking analytics for workspace file close operation.
            Analytics.TrackTaskFileOperationEvent(
                                     Path.GetFileName(model.CurrentWorkspace.FileName),
                                     Logging.Actions.Close,
                                     model.CurrentWorkspace.Nodes.Count());

            // Upon closing a workspace, validate if the workspace is valid to be sent to the ML datapipeline and then send it.
            if (!DynamoModel.IsTestMode && !HomeSpace.HasUnsavedChanges && (currentWorkspaceViewModel?.IsHomeSpace ?? true) && HomeSpace.HasRunWithoutCrash)
            {
                if (!IsDNADataIngestionPipelineinBeta && Model.CurrentWorkspace.IsValidForFDX && currentWorkspaceViewModel.Checksum != string.Empty)
                {
                    if (HasDifferentialCheckSum())
                    {
                        Model.Logger.Log("This Workspace is being shared to train the Dynamo Machine Learning model.", LogLevel.File);
                        var workspacePath = model.CurrentWorkspace.FileName;

                        Task.Run(() =>
                        {
                            try
                            {
                                MLDataPipelineExtension.DynamoMLDataPipeline.SendWorkspaceLog(workspacePath);
                            }
                            catch (Exception ex)
                            {
                                Model.Logger.Log("Failed to share this workspace with ML pipeline.", LogLevel.File);
                                Model.Logger.Log(ex.StackTrace, LogLevel.File);
                            }
                        });
                    }
                }
            }

            if (ClearHomeWorkspaceInternal())
            {
                // If after closing the HOME workspace, and there are no other custom
                // workspaces opened at the time, then we should show the start page.
                this.ShowStartPage = (Model.Workspaces.Count() <= 1);
                if (this.ShowStartPage)
                {
                    OnEnableShortcutBarItems(false);
                }
                RunSettings.ForceBlockRun = false;
                OnRequestCloseHomeWorkSpace();
            }
        }

        private bool CanCloseHomeWorkspace(object parameter)
        {
            return CanRunGraph || RunSettings.ForceBlockRun;
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
        internal bool ClearHomeWorkspaceInternal()
        {
            // if the workspace is unsaved, prompt to save
            // otherwise overwrite the home workspace with new workspace
            if (!HomeSpace.HasUnsavedChanges || AskUserToSaveWorkspaceOrCancel(HomeSpace))
            {
                Model.CurrentWorkspace = HomeSpace;

                model.ClearCurrentWorkspace();

                var defaultWorkspace = Workspaces.FirstOrDefault();
                //Every time that a new workspace is created we have to assign the Default Geometry Scaling value defined in Preferences
                if (defaultWorkspace !=null && defaultWorkspace.GeoScalingViewModel != null && preferencesViewModel != null)
                    defaultWorkspace.GeoScalingViewModel.ScaleSize = preferencesViewModel.DefaultGeometryScaling;

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
        /// <param name="allowCancel"></param>
        /// <returns>False if the user cancels, otherwise true</returns>
        public bool AskUserToSaveWorkspaceOrCancel(WorkspaceModel workspace, bool allowCancel = true)
        {
            var args = new WorkspaceSaveEventArgs(workspace, allowCancel);
            OnRequestUserSaveWorkflow(this, args);

            workspace.HasUnsavedChanges = !args.Success;

            return args.Success;
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

            string message = String.Concat(WpfResources.ExportWorkspaceAsImage, parameters.ToString());

            MainGuideManager?.CreateRealTimeInfoWindow(message, true);

            Dynamo.Logging.Analytics.TrackTaskCommandEvent("ImageCapture",
                "NodeCount", CurrentSpace.Nodes.Count());
        }

        private void Save3DImage(object parameters)
        {
            // Save the parameters
            OnRequestSave3DImage(this, new ImageSaveEventArgs(parameters.ToString()));

            string message = String.Concat(WpfResources.ExportWorkspaceAs3DImage, parameters.ToString());

            MainGuideManager?.CreateRealTimeInfoWindow(message, true);
        }

        internal bool CanSaveImage(object parameters)
        {
            return true;
        }

        public void ShowSaveImageDialogAndSave(object parameter)
        {
            FileDialog _fileDialog = null;

            if (_fileDialog == null)
            {
                _fileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = ".png",
                    Filter = string.Format(Resources.FileDialogPNGFiles, "*.png"),
                    Title = Resources.SaveWorkbenToImageDialogTitle
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(model.CurrentWorkspace.FileName);
                var snapshotName = PathHelper.GetScreenCaptureNameFromPath(fi.FullName, preferencesViewModel.IsTimeStampIncludedInExportFilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
                _fileDialog.FileName = snapshotName;
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

        public void ValidateWorkSpaceBeforeToExportAsImage(object parameter)
        {
            OnRequestExportWorkSpaceAsImage(parameter);
        }

        internal bool CanValidateWorkSpaceBeforeToExportAsImage(object parameter)
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

        public void SetConnectorType(object parameters)
        {
            ConnectorType = ConnectorType.BEZIER;
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
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoWikiLink) { UseShellExecute = true });
        }

        internal bool CanGoToWiki(object parameter)
        {
            return true;
        }
        public void GoToWebsite(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoSiteLink) { UseShellExecute = true });
        }

        internal bool CanGoToWebsite(object parameter)
        {
            return true;
        }

        public void GoToSourceCode(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.GitHubDynamoLink) { UseShellExecute = true });
        }

        internal bool CanGoToSourceCode(object parameter)
        {
            return true;
        }

        public void GoToDictionary(object parameter)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoDictionary) { UseShellExecute = true });
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

        private bool CanDisplayInteractiveGuide(object parameter)
        {
            return !this.ShowStartPage;
        }

        private void StartGettingStartedGuide(object parameter)
        {
            try
            {
                if (ClearHomeWorkspaceInternal())
                {
                    OpenOnboardingGuideFile();
                    MainGuideManager.LaunchTour(GuidesManager.OnboardingGuideName);
                }
            }
            catch (Exception ex)
            {
                Model.Logger.Log(ex.Message);
                Model.Logger.Log(ex.StackTrace);
            }
        }

        private bool CanStartGettingStartedGuide(object parameter)
        {
            // Disable Getting Started guided tour when ASM is not loaded
            return Model.IsASMLoaded;
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

        internal void OpenDocumentationLink(object parameter)
        {
            OnRequestOpenDocumentationLink((OpenDocumentationLinkEventArgs)parameter);
        }

        internal void ShowNodeDocumentation(object parameter)
        {
            var node = DynamoSelection.Instance.Selection.OfType<NodeModel>().LastOrDefault();

            var nodeAnnotationEventArgs = new OpenNodeAnnotationEventArgs(node, this);
            OpenDocumentationLink(nodeAnnotationEventArgs);
        }
        internal bool CanShowNodeDocumentation(object parameter)
        {
            var node = DynamoSelection.Instance.Selection.OfType<NodeModel>().LastOrDefault();

            if (node == null || !(node is NodeModel))
            {
                return false;
            }
            return true;
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

            if (parameter is bool)
            {
                CurrentSpaceViewModel.FitViewInternal((bool)parameter);
                return;
            }
            CurrentSpaceViewModel.FitViewInternal();
        }

        private bool CanFitView(object parameter)
        {
            return true;
        }

        private static void LoadLibraryEvents_LoadLibraryFailure(string failureMessage, string messageBoxTitle)
        {
            Wpf.Utilities.MessageBoxService.Show(failureMessage, messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// Returns a Minimum Qualified Name for a node, given it's NodeModel object.
        /// The MQN consist of the node namespace, node category, concatenated by the names of it's input and output ports, in case of overloaded nodes.
        /// </summary>
        /// <param name="nodeModel"></param>
        /// <returns></returns>
        internal string GetMinimumQualifiedName(NodeModel nodeModel)
        {
            switch (nodeModel)
            {
                case Graph.Nodes.CustomNodes.Function function:
                    var category = function.Category;
                    var name = function.Name;
                    if (CustomNodeHasCollisons(name, GetMainCategory(nodeModel)))
                    {
                        var inputString = GetInputNames(function);
                        return $"{category}.{name}({inputString})";
                    }
                    return $"{category}.{name}";

                case Graph.Nodes.ZeroTouch.DSFunctionBase dSFunction:
                    var descriptor = dSFunction.Controller.Definition;
                    if (descriptor.IsOverloaded)
                    {
                        var inputString = GetInputNames(nodeModel);
                        return $"{descriptor.QualifiedName}({inputString})";
                    }

                    return descriptor.QualifiedName;

                case NodeModel node:
                    var type = node.GetType();
                    if (NodeModelHasCollisions(type.FullName))
                    {
                        return $"{type.FullName}({GetInputNames(nodeModel)})";
                    }

                    return type.FullName;

                default:
                    return string.Empty;
            }
        }

        internal bool CustomNodeHasCollisons(string nodeName, string packageName)
        {
            var pmExtension = Model.GetPackageManagerExtension();
            if (pmExtension is null)
                return false;

            var package = pmExtension.PackageLoader.LocalPackages
                .Where(x => x.Name == packageName)
                .FirstOrDefault();

            if (package is null)
                return false;

            var loadedNodesWithSameName = package.LoadedCustomNodes
                .Where(x => x.Name == nodeName)
                .ToList();

            if (loadedNodesWithSameName.Count == 1)
                return false;
            return true;
        }

        internal bool NodeModelHasCollisions(string typeName)
        {
            var entries = Model.SearchModel.Entries
                .Where(x => x.CreationName == typeName)
                .Select(x => x).ToList();

            if (entries.Count() > 1)
                return true;

            return false;
        }

        internal string GetMainCategory(NodeModel node)
        {
            return node.Category.Split(new char[] { '.' }).FirstOrDefault();
        }

        internal string GetInputNames(NodeModel node)
        {
            var inputNames = node.InPorts.Select(x => x.Name).ToArray();
            // Match https://github.com/DynamoDS/Dynamo/blame/master/src/DynamoCore/Search/SearchElements/ZeroTouchSearchElement.cs#L51
            return string.Join(", ", inputNames);
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
                    CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults();
                }
                catch(LibraryLoadFailedException ex)
                {
                    Wpf.Utilities.MessageBoxService.Show(
                        Owner,
                        $"{ex.Message}-{WpfResources.LibraryLoadFailureMessageSuffix}"  ,
                        Properties.Resources.LibraryLoadFailureMessageBoxTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
                catch(DynamoServices.AssemblyBlockedException ex)
                {
                    var failureMessage = string.Format(Properties.Resources.LibraryLoadFailureForBlockedAssembly, ex.Message);
                    Wpf.Utilities.MessageBoxService.Show(
                        Owner,
                        failureMessage,
                        Properties.Resources.LibraryLoadFailureMessageBoxTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
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
            BackgroundPreviewViewModel.RefreshState();
        }

        /// <summary>
        /// Checking if a custom Group style has been updated, if so it will update the styling of the existing groups
        /// </summary>
        /// <param name="originalCustomGroupStyles"></param>
        public void CheckCustomGroupStylesChanges(List<GroupStyleItem> originalCustomGroupStyles)
        {
            foreach (var originalCustomGroupStyle in originalCustomGroupStyles)
            {
                var currentCustomGroupStyle = PreferenceSettings.GroupStyleItemsList.Where(
                    groupStyle => !groupStyle.GroupStyleId.Equals(Guid.Empty) && groupStyle.GroupStyleId.Equals(originalCustomGroupStyle.GroupStyleId)).FirstOrDefault();

                if (currentCustomGroupStyle != null)
                {
                    if (!originalCustomGroupStyle.HexColorString.Equals(currentCustomGroupStyle.HexColorString)
                        || !originalCustomGroupStyle.FontSize.Equals(currentCustomGroupStyle.FontSize))
                    {
                        foreach (var annotation in CurrentSpaceViewModel.Annotations)
                        {
                            if (annotation.GroupStyleId.Equals(currentCustomGroupStyle.GroupStyleId))
                            {
                                annotation.FontSize = currentCustomGroupStyle.FontSize;
                                annotation.Background = (Color)ColorConverter.ConvertFromString("#" + currentCustomGroupStyle.HexColorString);
                            }
                        }
                    }
                }
            }
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

                Dynamo.Logging.Analytics.TrackTaskCommandEvent("ExportToSTL");
            }
        }

        /// <summary>
        /// Hide or show file trust warning popup for the current workspace when preference settings are updated.
        /// </summary>
        internal void ShowHideFileTrustWarningIfCurrentWorkspaceTrusted()
        {
            if (FileTrustViewModel == null || DynamoModel.IsTestMode) return;
            if ((FileTrustViewModel.ShowWarningPopup
                && model.PreferenceSettings.IsTrustedLocation(FileTrustViewModel.DynFileDirectoryName))
                || model.PreferenceSettings.DisableTrustWarnings)
            {
                FileTrustViewModel.ShowWarningPopup = false;
                RunSettings.ForceBlockRun = false;
                Model.CurrentWorkspace.RequestRun();
                return;
            }

            var targetPath = string.Empty;
            if (String.IsNullOrEmpty(FileTrustViewModel.DynFileDirectoryName))
            {
                if(!String.IsNullOrEmpty(currentWorkspaceViewModel.FileName))
                    targetPath = Path.GetDirectoryName(currentWorkspaceViewModel.FileName);
            }
            else
            {
                targetPath = FileTrustViewModel.DynFileDirectoryName;
            }
            if (!FileTrustViewModel.ShowWarningPopup
                && !model.PreferenceSettings.IsTrustedLocation(targetPath)
                && (currentWorkspaceViewModel?.IsHomeSpace ?? false) && !ShowStartPage
                && !FileTrustViewModel.AllowOneTimeTrust
                && !model.PreferenceSettings.DisableTrustWarnings
                && !string.IsNullOrWhiteSpace(currentWorkspaceViewModel.FileName))
            {
                FileTrustViewModel.ShowWarningPopup = true;
                RunSettings.ForceBlockRun = true;
                (HomeSpaceViewModel as HomeWorkspaceViewModel).UpdateRunStatusMsgBasedOnStates();
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

        private void SetDefaultScaleFactor(WorkspaceViewModel defaultWorkspace)
        {
            if (defaultWorkspace != null)
            {
                defaultWorkspace.GeoScalingViewModel.ScaleValue = PreferenceSettings.DefaultScaleFactor;
                defaultWorkspace.GeoScalingViewModel.UpdateGeometryScale(PreferenceSettings.DefaultScaleFactor);
            }
        }

        private DirectoryInfo GetFallBackDocPath()
        {
            DirectoryInfo fallbackDocPath = null;
            const string FALLBACK_DOC_DIRECTORY_NAME = "fallback_docs";
            var pathManager = Model.PathManager;

            if (!string.IsNullOrEmpty(pathManager.DynamoCoreDirectory))
            {
                var docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, System.Threading.Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            if (!string.IsNullOrEmpty(pathManager.HostApplicationDirectory))
            {
                //when running over any host app like Revit, FormIt, Civil3D... the path to the fallback_docs can change.
                //e.g. for Revit the variable HostApplicationDirectory = C:\Program Files\Autodesk\Revit 2023\AddIns\DynamoForRevit\Revit
                //Then we need to remove the last folder from the path so we can find the fallback_docs directory.
                var hostAppDirectory = Directory.GetParent(pathManager.HostApplicationDirectory).FullName;
                var docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, System.Threading.Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }
            return fallbackDocPath;
        }
        private DirectoryInfo GetNodeHelpDocPath()
        {
            var nodeHelpDocPath = new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
                    Configurations.DynamoNodeHelpDocs));

            return nodeHelpDocPath;
        }

        /// <summary>
        /// Debug method to export missing node help data for core nodes
        /// </summary>
        /// <param name="parameter"></param>
        internal void DumpNodeHelpData(object parameter)
        {
            const string CSV_HEADER = "Node Name,Full Name,Hash,MD File,DYN File,IMG File, MD File State";
            const string IMG_NOT_FOUND = "Image not found in MD file";
            const string IN_DEPTH_NOT_FOUND = "In Depth section not found in MD file";
            const string MISSING_MD_FILE = "Missing MD file";
            const string MISSING_DYN_FILE = "Missing DYN file";
            const string MISSING_IMG_FILE = "Missing IMG file";
            string fileName = String.Format("NodeHelpData_{0}.csv", DateTime.Now.ToString("yyyyMMddHmmss"));
            string fullFileName = Path.Combine(Model.PathManager.LogDirectory, fileName);
            DirectoryInfo fallbackDocPath = GetFallBackDocPath();
            DirectoryInfo nodeHelpDocPath = GetNodeHelpDocPath();
            var csv = new System.Text.StringBuilder();
            //creating a copy to avoid collection changed exceptions
            var entriesCopy = Model.SearchModel.Entries.ToList();
            List<string> mdSuffix = new List<string>() { ".md" };
            List<string> dynSuffix = new List<string>() { ".dyn" };
            List<string> imgSuffix = new List<string>() { "_img.jpg", "_img.png", "_img.gif" };
            Dictionary<string, List<string>> stats = new Dictionary<string, List<string>>
            {
                { "MD", new List<string>() },
                { "IMG", new List<string>() },
                { "DYN", new List<string>() },
                { "IMG_A", new List<string>() },
                { "MD_D", new List<string>() }
            };

            csv.AppendLine(CSV_HEADER);
            foreach (var ent in entriesCopy)
            {
                var node = ent.CreateNode();
                var mqn = GetMinimumQualifiedName(node);
                var hash = Hash.GetHashFilenameFromString(mqn);

                string helpDocPath = string.Empty;
                string helpImgPath = string.Empty;
                string helpDynPath = string.Empty;
                string helpImageAttached = IMG_NOT_FOUND;
                string helpInDepth = IN_DEPTH_NOT_FOUND;
                if (fallbackDocPath != null)
                {
                    var matchingDoc = GetMatchingDocFromDirectory(mqn, hash, mdSuffix, fallbackDocPath);
                    if (matchingDoc is null)
                    {
                        stats["MD"].Add(mqn);
                        helpDocPath = MISSING_MD_FILE;
                        helpImageAttached = string.Empty;
                        helpInDepth = string.Empty;
                    }
                    else
                    {
                        helpDocPath = matchingDoc.FullName;
                        var lines = File.ReadAllLines(matchingDoc.FullName);
                        bool hasImageFile = false;
                        bool hasImageHeader = false;
                        bool hasInDepth = false;
                        for (var i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            if (line.Trim().ToLower().Contains("## in depth"))
                            {
                                hasInDepth = true;
                                helpInDepth = string.Empty;
                                continue;
                            }
                            if (line.Trim().ToLower().Contains("## example file"))
                            {
                                hasImageHeader = true;
                                continue;
                            }
                            if (imgSuffix.Any(s => line.Trim().ToLower().Contains(s)))
                            {
                                hasImageFile = true;
                            }
                            if (hasImageFile && hasImageHeader)
                            {
                                helpImageAttached = string.Empty;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(helpImageAttached))
                        {
                            stats["IMG_A"].Add(mqn);
                        }
                        if (!hasInDepth)
                        {
                            stats["MD_D"].Add(mqn);
                        }
                    }

                    var matchingDyn = GetMatchingDocFromDirectory(mqn, hash, dynSuffix, nodeHelpDocPath);
                    if (matchingDyn is null)
                    {
                        stats["DYN"].Add(mqn);
                        helpDynPath = MISSING_DYN_FILE;
                    }
                    else { helpDynPath = matchingDyn.FullName; }

                    var matchingSpecialImg = GetMatchingDocFromDirectory(mqn, hash, imgSuffix, nodeHelpDocPath);
                    if (matchingSpecialImg is null)
                    {
                        stats["IMG"].Add(mqn);
                        helpImgPath = MISSING_IMG_FILE;
                    }
                    else { helpImgPath = matchingSpecialImg.FullName; }
                }

                //replaced ',' with '-' for nodes with overload to be displayed in a single cell
                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", node.Name, mqn.Replace(",", "-"), hash, helpDocPath.Replace(",", "-"), helpDynPath.Replace(",", "-"), helpImgPath.Replace(",", "-"), helpImageAttached, helpInDepth);
                csv.AppendLine(newLine);
            }
            var stat = new System.Text.StringBuilder();
            stat.AppendLine("Total," + entriesCopy.Count);
            stat.AppendLine("Missing MD Files (MD)," + stats["MD"].Count);
            stat.AppendLine("Missing DYN Files (DYN)," + stats["DYN"].Count);
            stat.AppendLine("Missing IMG Files (IMG)," + stats["IMG"].Count);
            stat.AppendLine("Missing IMG attached in MD File (IMG_A)," + stats["IMG_A"].Count);
            stat.AppendLine("Missing In Depth section in MD File (MD_D)," + stats["MD_D"].Count);
            Model.Logger.Log(string.Format(Resources.NodeHelpIsDumped, fullFileName + Environment.NewLine + stat.ToString()));
            stat.AppendLine(Environment.NewLine);

            foreach (var k in stats.Keys)
            {
                stat.AppendLine(k + Environment.NewLine);
                foreach (var node in stats[k])
                {
                    stat.AppendLine(node.Replace(",", "-"));
                }
                stat.AppendLine(Environment.NewLine);
            }

            stat.AppendLine(csv.ToString());

            File.WriteAllText(fullFileName, stat.ToString());
        }

        internal void DumpNodeIconData(object parameter)
        {
            //set to manual run mode to prevent execution of the nodes as wel place them
            this.HomeSpace.RunSettings.RunType = RunType.Manual;

            string nodesWithoutIconsFileName = String.Format("NodesWithoutIcons_{0}.csv", DateTime.Now.ToString("yyyyMMddHmmss"));
            string nodesWithoutIconsFullFileName = Path.Combine(Model.PathManager.LogDirectory, nodesWithoutIconsFileName);

            //creating a copy to avoid collection changed exceptions
            var entriesCopy = Model.SearchModel.Entries.Where(n => n.IsVisibleInSearch).ToList();

            StreamWriter sw = File.CreateText(nodesWithoutIconsFullFileName);

            sw.WriteLine("NODE ASSEMBLY,NODE NAME");

            foreach (var nse in entriesCopy)
            {
                var newNode = nse.CreateNode();
                this.CurrentSpace.AddAndRegisterNode(newNode);
                var placedNode = this.CurrentSpaceViewModel.Nodes.Last();
                var imageSource = placedNode.ImageSource;

                //if image source is null, then no icon is found
                if (imageSource is null)
                {
                    sw.WriteLine($"{nse.Assembly},{nse.Name}");
                }
                else
                {
                    this.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(placedNode.Id));
                }
            }

            sw.Close();

            //alert user to new file location
            MainGuideManager.CreateRealTimeInfoWindow(string.Format(Resources.NodeIconDataIsDumped, nodesWithoutIconsFullFileName), true);
        }

        private FileInfo GetMatchingDocFromDirectory(string nodeName, string hash, List<string> suffix, DirectoryInfo dir)
        {
            FileInfo matchingFile = null;
            foreach (var sfx in suffix)
            {
                matchingFile = dir.GetFiles($"{hash}{sfx}").FirstOrDefault();
                if (matchingFile is null && !string.IsNullOrWhiteSpace(nodeName) && !nodeName.Contains("/") && !nodeName.Contains("\\"))
                {
                    matchingFile = dir.GetFiles($"{nodeName}{sfx}").FirstOrDefault();
                }
                if (matchingFile != null)
                {
                    break;
                }
            }

            return matchingFile;
        }

        internal bool CanDumpNodeHelpData(object obj)
        {
            return true;
        }
        internal bool CanDumpNodeIconData(object obj)
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

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException -= CurrentDispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            this.Model.RequestsCrashPrompt -= CrashReportTool.ShowCrashWindow;

            // Request the View layer to close its window (see
            // ShutdownParams.CloseDynamoView member for details).
            if (shutdownParams.CloseDynamoView)
            {
                OnRequestClose(this, EventArgs.Empty);
            }

            BackgroundPreviewViewModel.Dispose();
            foreach (var wsvm in workspaces)
            {
                wsvm.Dispose();
            }
            MainGuideManager?.CloseRealTimeInfoWindow();

            model.ShutDown(shutdownParams.ShutdownHost);
            UsageReportingManager.DestroyInstance();

            this.model.CommandStarting -= OnModelCommandStarting;
            this.model.CommandCompleted -= OnModelCommandCompleted;
            BackgroundPreviewViewModel.PropertyChanged -= Watch3DViewModelPropertyChanged;
            WatchHandler.RequestSelectGeometry -= BackgroundPreviewViewModel.AddLabelForPath;
            model.ComputeModelDeserialized -= model_ComputeModelDeserialized;
            model.RequestNotification -= model_RequestNotification;

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

        /// <summary>
        /// Check if the current file is located in a Trusted Location in order to display to the User the proper message
        /// </summary>
        public void CheckCurrentFileInTrustedLocation()
        {
            PreferenceSettings.AskForTrustedLocationResult askToTheUser =
                PreferenceSettings.AskForTrustedLocation(CurrentSpaceViewModel.FileName.Length > 0,
                CurrentSpaceViewModel.FileName.Length > 0 ? PreferenceSettings.IsTrustedLocation(Path.GetDirectoryName(CurrentSpaceViewModel.FileName)) : false,
                (currentWorkspaceViewModel?.IsHomeSpace ?? false),
                ShowStartPage,
                model.PreferenceSettings.DisableTrustWarnings);

            if (askToTheUser == PreferenceSettings.AskForTrustedLocationResult.Ask) {

                FileTrustViewModel.AllowOneTimeTrust = false;
            }
        }

        #endregion
    }
}

using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Prompts;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf;
using Dynamo.Wpf.Authentication;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.Views.Gallery;
using Dynamo.Wpf.Views.PackageManager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HelixToolkit.Wpf.SharpDX;
using ResourceNames = Dynamo.Wpf.Interfaces.ResourceNames;
using String = System.String;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class DynamoView : Window, IDisposable
    {
        public const string BackgroundPreviewName = "BackgroundPreview";
        private const int navigationInterval = 100;
        // This is used to determine whether ESC key is being held down
        private bool IsEscKeyPressed = false;

        private readonly NodeViewCustomizationLibrary nodeViewCustomizationLibrary;
        private DynamoViewModel dynamoViewModel;
        private readonly Stopwatch _timer;
        private StartPageViewModel startPage;
        private int tabSlidingWindowStart, tabSlidingWindowEnd;
        private GalleryView galleryView;
        private readonly LoginService loginService;
        internal ViewExtensionManager viewExtensionManager = new ViewExtensionManager();
        private ShortcutToolbar shortcutBar;

        // This is to identify whether the PerformShutdownSequenceOnViewModel() method has been
        // called on the view model and the process is not cancelled
        private bool isPSSCalledOnViewModelNoCancel = false;

        private readonly DispatcherTimer _workspaceResizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500), IsEnabled = false };

        internal Watch3DView BackgroundPreview { get; private set; }

        public DynamoView(DynamoViewModel dynamoViewModel)
        {
            // The user's choice to enable hardware acceleration is now saved in
            // the Dynamo preferences. It is set to true by default. 
            // When the view is constructed, we enable or disable hardware acceleration based on that preference. 
            //This preference is not exposed in the UI and can be used to debug hardware issues only
            // by modifying the preferences xml.
            RenderOptions.ProcessRenderMode = dynamoViewModel.Model.PreferenceSettings.UseHardwareAcceleration ? 
                RenderMode.Default : RenderMode.SoftwareOnly;
            
            this.dynamoViewModel = dynamoViewModel;
            this.dynamoViewModel.UIDispatcher = Dispatcher;            
            nodeViewCustomizationLibrary = new NodeViewCustomizationLibrary(this.dynamoViewModel.Model.Logger);

            DataContext = dynamoViewModel;
            Title = dynamoViewModel.BrandingResourceProvider.GetString(ResourceNames.MainWindow.Title);

            tabSlidingWindowStart = tabSlidingWindowEnd = 0;

            _timer = new Stopwatch();
            _timer.Start();

            InitializeComponent();

            ToggleIsUsageReportingApprovedCommand.ToolTip = string.Format(
                Wpf.Properties.Resources.DynamoViewSettingMenuEnableDataReportingTooltip,
                dynamoViewModel.BrandingResourceProvider.ProductName);

            Loaded += DynamoView_Loaded;
            Unloaded += DynamoView_Unloaded;

            SizeChanged += DynamoView_SizeChanged;
            LocationChanged += DynamoView_LocationChanged;

            // Check that preference bounds are actually within one
            // of the available monitors.
            if (CheckVirtualScreenSize())
            {
                Left = dynamoViewModel.Model.PreferenceSettings.WindowX;
                Top = dynamoViewModel.Model.PreferenceSettings.WindowY;
                Width = dynamoViewModel.Model.PreferenceSettings.WindowW;
                Height = dynamoViewModel.Model.PreferenceSettings.WindowH;
            }
            else
            {
                Left = 0;
                Top = 0;
                Width = 1024;
                Height = 768;
            }

            _workspaceResizeTimer.Tick += _resizeTimer_Tick;

            if (dynamoViewModel.Model.AuthenticationManager.HasAuthProvider)
            {
                loginService = new LoginService(this, new System.Windows.Forms.WindowsFormsSynchronizationContext());
                dynamoViewModel.Model.AuthenticationManager.AuthProvider.RequestLogin += loginService.ShowLogin;
            }

            var viewExtensions = new List<IViewExtension>();
            foreach (var dir in dynamoViewModel.Model.PathManager.ViewExtensionsDirectories)
            {
                viewExtensions.AddRange(viewExtensionManager.ExtensionLoader.LoadDirectory(dir));
            }

            viewExtensionManager.MessageLogged += Log;

            var startupParams = new ViewStartupParams(dynamoViewModel);

            foreach (var ext in viewExtensions)
            {
                try
                {
                    var logSource = ext as ILogSource;
                    if (logSource != null)
                        logSource.MessageLogged += Log;

                    ext.Startup(startupParams);
                    viewExtensionManager.Add(ext);
                }
                catch (Exception exc)
                {
                    Log(ext.Name + ": " + exc.Message);
                }
            }

            this.dynamoViewModel.RequestPaste += OnRequestPaste;
            this.dynamoViewModel.RequestReturnFocusToView += OnRequestReturnFocusToView;
            FocusableGrid.InputBindings.Clear();
        }

        private void OnRequestReturnFocusToView()
        {
            // focusing grid allows to remove focus from current textbox
            FocusableGrid.Focus();
            // keep handling input bindings of DynamoView
            Keyboard.Focus(this);
        }

        private void OnRequestPaste()
        {
            var clipBoard = dynamoViewModel.Model.ClipBoard;
            var locatableModels = clipBoard.Where(item => item is NoteModel || item is NodeModel);

            var modelBounds = locatableModels.Select(lm =>
                new Rect {X = lm.X, Y = lm.Y, Height = lm.Height, Width = lm.Width});

            // Find workspace view.
            var workspace = this.ChildOfType<WorkspaceView>();
            var workspaceBounds = workspace.GetVisibleBounds();

            // is at least one note/node located out of visible workspace part
            var outOfView = modelBounds.Any(m => !workspaceBounds.Contains(m));

            // If copied nodes are out of view, we paste their copies under mouse cursor or at the center of workspace.
            if (outOfView)
            {
                // If mouse is over workspace, paste copies under mouse cursor.
                if (workspace.IsMouseOver)
                {
                    dynamoViewModel.Model.Paste(Mouse.GetPosition(workspace.WorkspaceElements).AsDynamoType(), false);
                }
                else // If mouse is out of workspace view, then paste copies at the center.
                {
                    PasteNodeAtTheCenter(workspace);
                }

                return;
            }

            // All nodes are inside of workspace and visible for user.
            // Order them by CenterX and CenterY.
            var orderedItems = locatableModels.OrderBy(item => item.CenterX + item.CenterY);

            // Search for the rightmost item. It's item with the biggest X, Y coordinates of center.
            var rightMostItem = orderedItems.Last();
            // Search for the leftmost item. It's item with the smallest X, Y coordinates of center.
            var leftMostItem = orderedItems.First();

            // Compute shift so that left most item will appear at right most item place with offset.
            var shiftX = rightMostItem.X + rightMostItem.Width - leftMostItem.X;
            var shiftY = rightMostItem.Y - leftMostItem.Y;

            // Find new node bounds.
            var newNodeBounds = modelBounds
                .Select(node => new Rect(node.X + shiftX + workspace.ViewModel.Model.CurrentPasteOffset,
                                         node.Y + shiftY + workspace.ViewModel.Model.CurrentPasteOffset,
                                         node.Width, node.Height));

            outOfView = newNodeBounds.Any(node => !workspaceBounds.Contains(node));

            // If new node bounds appeare out of workspace view, we should paste them at the center.
            if (outOfView)
            {
                PasteNodeAtTheCenter(workspace);
                return;
            }

            var x = shiftX + locatableModels.Min(m => m.X);
            var y = shiftY + locatableModels.Min(m => m.Y);

            // All copied nodes are inside of workspace.
            // Paste them with little offset.           
            dynamoViewModel.Model.Paste(new Point2D(x, y));
        }

        /// <summary>
        /// Paste nodes at the center of workspace view.
        /// </summary>
        /// <param name="workspace">workspace view</param>
        private void PasteNodeAtTheCenter(WorkspaceView workspace)
        {
            var centerPoint = workspace.GetCenterPoint().AsDynamoType();
            dynamoViewModel.Model.Paste(centerPoint);
        }

        #region NodeViewCustomization

        private void LoadNodeViewCustomizations()
        {
            nodeViewCustomizationLibrary.Add(new CoreNodeViewCustomizations());
            foreach (var assem in dynamoViewModel.Model.Loader.LoadedAssemblies)
                nodeViewCustomizationLibrary.Add(new AssemblyNodeViewCustomizations(assem));
        }

        private void SubscribeNodeViewCustomizationEvents()
        {
            dynamoViewModel.Model.Loader.AssemblyLoaded += LoaderOnAssemblyLoaded;
            dynamoViewModel.NodeViewReady += ApplyNodeViewCustomization;
        }

        private void UnsubscribeNodeViewCustomizationEvents()
        {
            if (dynamoViewModel == null) return;

            dynamoViewModel.Model.Loader.AssemblyLoaded -= LoaderOnAssemblyLoaded;
            dynamoViewModel.NodeViewReady -= ApplyNodeViewCustomization;
        }

        private void ApplyNodeViewCustomization(object nodeView, EventArgs args)
        {
            var nodeViewImp = nodeView as NodeView;
            if (nodeViewImp != null)
            {
                nodeViewCustomizationLibrary.Apply(nodeViewImp);
            }
        }

        private void LoaderOnAssemblyLoaded(NodeModelAssemblyLoader.AssemblyLoadedEventArgs args)
        {
            nodeViewCustomizationLibrary.Add(new AssemblyNodeViewCustomizations(args.Assembly));
        }

        #endregion

        private bool CheckVirtualScreenSize()
        {
            var w = SystemParameters.VirtualScreenWidth;
            var h = SystemParameters.VirtualScreenHeight;
            var ox = SystemParameters.VirtualScreenLeft;
            var oy = SystemParameters.VirtualScreenTop;

            // TODO: Remove 10 pixel check if others can't reproduce
            // On Ian's Windows 8 setup, when Dynamo is maximized, the origin
            // saves at -8,-8. There doesn't seem to be any documentation on this
            // so we'll put in a 10 pixel check to still allow the window to maximize.
            if (dynamoViewModel.Model.PreferenceSettings.WindowX < ox - 10 ||
                dynamoViewModel.Model.PreferenceSettings.WindowY < oy - 10)
            {
                return false;
            }

            // Check that the window is smaller than the available area.
            if (dynamoViewModel.Model.PreferenceSettings.WindowW > w ||
                dynamoViewModel.Model.PreferenceSettings.WindowH > h)
            {
                return false;
            }

            return true;
        }

        private void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            dynamoViewModel.Model.PreferenceSettings.WindowX = Left;
            dynamoViewModel.Model.PreferenceSettings.WindowY = Top;
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dynamoViewModel.Model.PreferenceSettings.WindowW = e.NewSize.Width;
            dynamoViewModel.Model.PreferenceSettings.WindowH = e.NewSize.Height;
        }

        private void InitializeLogin()
        {
            if ( dynamoViewModel.ShowLogin && dynamoViewModel.Model.AuthenticationManager.HasAuthProvider)
            {
                var login = new Login(dynamoViewModel.PackageManagerClientViewModel);
                loginGrid.Children.Add(login);
            }
        }

        private void InitializeShortcutBar()
        {
            shortcutBar = new ShortcutToolbar(this.dynamoViewModel.Model.UpdateManager) {Name = "ShortcutToolbar"};

            var newScriptButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarNewButtonTooltip,
                ShortcutCommand = dynamoViewModel.NewHomeWorkspaceCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/new_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/new_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/new_hover.png"
            };

            var openScriptButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarOpenButtonTooltip,
                ShortcutCommand = dynamoViewModel.ShowOpenDialogAndOpenResultCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/open_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/open_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/open_hover.png"
            };

            var saveButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarSaveButtonTooltip,
                ShortcutCommand = dynamoViewModel.ShowSaveDialogIfNeededAndSaveResultCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/save_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/save_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/save_hover.png"
            };

            var screenShotButton = new ImageExportShortcutBarItem(dynamoViewModel)
            {
                ShortcutCommand = dynamoViewModel.ShowSaveImageDialogAndSaveResultCommand,
                ShortcutCommandParameter = Wpf.Properties.Resources.ScreenShotFrom3DShortcutParameter,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/screenshot_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/screenshot_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/screenshot_hover.png"
            };

            var undoButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarUndoButtonTooltip,
                ShortcutCommand = dynamoViewModel.UndoCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/undo_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/undo_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/undo_hover.png"
            };

            var redoButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarRedoButtonTooltip,
                ShortcutCommand = dynamoViewModel.RedoCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/redo_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/redo_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/redo_hover.png"
            };

            shortcutBar.ShortcutBarItems.Add(newScriptButton);
            shortcutBar.ShortcutBarItems.Add(openScriptButton);
            shortcutBar.ShortcutBarItems.Add(saveButton);
            shortcutBar.ShortcutBarItems.Add(undoButton);
            shortcutBar.ShortcutBarItems.Add(redoButton);

            shortcutBar.ShortcutBarRightSideItems.Add(screenShotButton);

            shortcutBarGrid.Children.Add(shortcutBar);
        }

        /// <summary>
        /// This method inserts an instance of "StartPageViewModel" into the 
        /// "startPageItemsControl", results of which displays the Start Page on 
        /// "DynamoView" through the list item's data template. This method also
        /// ensures that there is at most one item in the "startPageItemsControl".
        /// Only when this method is invoked the cost of initializing the start 
        /// page is incurred, when user opts to not display start page at start 
        /// up, then this method will not be called (therefore incurring no cost).
        /// </summary>
        /// <param name="isFirstRun">
        /// Indicates if it is the first time new Dynamo version runs.
        /// It is used to decide whether the Gallery need to be shown on the StartPage.
        /// </param>
        private void InitializeStartPage(bool isFirstRun)
        {
            if (DynamoModel.IsTestMode) // No start screen in unit testing.
                return;

            if (startPage == null)
            {
                if (startPageItemsControl.Items.Count > 0)
                {
                    var message = "'startPageItemsControl' must be empty";
                    throw new InvalidOperationException(message);
                }

                startPage = new StartPageViewModel(dynamoViewModel, isFirstRun);
                startPageItemsControl.Items.Add(startPage);
            }
        }

        private void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(UpdateLayout), DispatcherPriority.Render, null);
        }

        private void DynamoViewModelRequestViewOperation(ViewOperationEventArgs e)
        {
            if (dynamoViewModel.BackgroundPreviewViewModel.CanNavigateBackground == false)
                return;

            switch (e.ViewOperation)
            {
                case ViewOperationEventArgs.Operation.FitView:
                    if (dynamoViewModel.BackgroundPreviewViewModel != null)
                    {
                        dynamoViewModel.BackgroundPreviewViewModel.ZoomToFitCommand.Execute(null);
                        return;
                    }
                    BackgroundPreview.View.ZoomExtents();
                    break;

                case ViewOperationEventArgs.Operation.ZoomIn:
                    var camera1 = BackgroundPreview.View.CameraController;
                    camera1.Zoom(-0.5 * BackgroundPreview.View.ZoomSensitivity);
                    break;

                case ViewOperationEventArgs.Operation.ZoomOut:
                    var camera2 = BackgroundPreview.View.CameraController;
                    camera2.Zoom(0.5 * BackgroundPreview.View.ZoomSensitivity);
                    break;
            }
        }

        private void DynamoView_Loaded(object sender, EventArgs e)
        {
            // Do an initial load of the cursor collection
            CursorLibrary.GetCursor(CursorSet.ArcSelect);

            //Backing up IsFirstRun to determine whether to show Gallery
            var isFirstRun = dynamoViewModel.Model.PreferenceSettings.IsFirstRun;
            // If first run, Collect Info Prompt will appear
            UsageReportingManager.Instance.CheckIsFirstRun(this, dynamoViewModel.BrandingResourceProvider);

            WorkspaceTabs.SelectedIndex = 0;
            dynamoViewModel = (DataContext as DynamoViewModel);
            dynamoViewModel.Model.RequestLayoutUpdate += vm_RequestLayoutUpdate;
            dynamoViewModel.RequestViewOperation += DynamoViewModelRequestViewOperation;
            dynamoViewModel.PostUiActivationCommand.Execute(null);

            _timer.Stop();
            dynamoViewModel.Model.Logger.Log(String.Format(Wpf.Properties.Resources.MessageLoadingTime,
                                                                     _timer.Elapsed, dynamoViewModel.BrandingResourceProvider.ProductName));
            InitializeLogin();
            InitializeShortcutBar();
            InitializeStartPage(isFirstRun);

#if !__NO_SAMPLES_MENU
            LoadSamplesMenu();
#endif
            #region Search initialization

            var search = new SearchView(
                dynamoViewModel.SearchViewModel,
                dynamoViewModel);
            sidebarGrid.Children.Add(search);
            dynamoViewModel.SearchViewModel.Visible = true;

            #endregion

            #region Package manager

            dynamoViewModel.RequestPackagePublishDialog += DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestManagePackagesDialog += DynamoViewModelRequestShowInstalledPackages;
            dynamoViewModel.RequestPackageManagerSearchDialog += DynamoViewModelRequestShowPackageManagerSearch;
            dynamoViewModel.RequestPackagePathsDialog += DynamoViewModelRequestPackagePaths;
            dynamoViewModel.RequestScaleFactorDialog += DynamoViewModelChangeScaleFactor;

            #endregion

            #region Node view injection

            // scan for node view overrides



            #endregion

            //FUNCTION NAME PROMPT
            dynamoViewModel.Model.RequestsFunctionNamePrompt += DynamoViewModelRequestsFunctionNamePrompt;

            //Preset Name Prompt
            dynamoViewModel.Model.RequestPresetsNamePrompt += DynamoViewModelRequestPresetNamePrompt;
            dynamoViewModel.RequestPresetsWarningPrompt += DynamoViewModelRequestPresetWarningPrompt;

            dynamoViewModel.RequestClose += DynamoViewModelRequestClose;
            dynamoViewModel.RequestSaveImage += DynamoViewModelRequestSaveImage;
            dynamoViewModel.RequestSave3DImage += DynamoViewModelRequestSave3DImage;
            dynamoViewModel.SidebarClosed += DynamoViewModelSidebarClosed;

            dynamoViewModel.Model.RequestsCrashPrompt += Controller_RequestsCrashPrompt;
            dynamoViewModel.Model.RequestTaskDialog += Controller_RequestTaskDialog;

            DynamoSelection.Instance.Selection.CollectionChanged += Selection_CollectionChanged;

            dynamoViewModel.RequestUserSaveWorkflow += DynamoViewModelRequestUserSaveWorkflow;

            dynamoViewModel.Model.ClipBoard.CollectionChanged += ClipBoard_CollectionChanged;

            //ABOUT WINDOW
            dynamoViewModel.RequestAboutWindow += DynamoViewModelRequestAboutWindow;

            //SHOW or HIDE GALLERY
            dynamoViewModel.RequestShowHideGallery += DynamoViewModelRequestShowHideGallery;

            LoadNodeViewCustomizations();
            SubscribeNodeViewCustomizationEvents();

            // Kick start the automation run, if possible.
            dynamoViewModel.BeginCommandPlayback(this);

            var loadedParams = new ViewLoadedParams(this, dynamoViewModel);

            foreach (var ext in viewExtensionManager.ViewExtensions)
            {
                try
                {
                    ext.Loaded(loadedParams);
                }
                catch (Exception exc)
                {
                    Log(ext.Name + ": " + exc.Message);
                }
            }

            BackgroundPreview = new Watch3DView {Name = BackgroundPreviewName};
            background_grid.Children.Add(BackgroundPreview);
            BackgroundPreview.DataContext = dynamoViewModel.BackgroundPreviewViewModel;
            BackgroundPreview.Margin = new System.Windows.Thickness(0,20,0,0);
            var vizBinding = new Binding
            {
                Source = dynamoViewModel.BackgroundPreviewViewModel,
                Path = new PropertyPath("Active"),
                Mode = BindingMode.TwoWay,
                Converter = new BooleanToVisibilityConverter()
            };
            BackgroundPreview.SetBinding(VisibilityProperty, vizBinding);
            TrackStartupAnalytics();

            // In native host scenario (e.g. Revit), the "Application.Current" will be "null". Therefore, the InCanvasSearchControl.OnRequestShowInCanvasSearch
            // will not work. Instead, we have to check if the Owner Window (DynamoView) is deactivated or not.  
            if (Application.Current == null)
            {
                this.Deactivated += (s, args) => { HidePopupWhenWindowDeactivated(); };
            }
        }

        /// <summary>
        /// Close Popup when the Dynamo window is not in the foreground.
        /// </summary>

        private void HidePopupWhenWindowDeactivated()
        {
            var workspace = this.ChildOfType<WorkspaceView>();
            if(workspace != null)
                workspace.HidePopUp();
         }

        private void TrackStartupAnalytics()
        {
            if (!Analytics.ReportingAnalytics) return;

            string packages = string.Empty;
            var pkgExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            if(pkgExtension != null)
            {
                packages = pkgExtension.PackageLoader.LocalPackages
                    .Select(p => string.Format("{0} {1}", p.Name, p.VersionName))
                    .Aggregate(String.Empty, (x, y) => string.Format("{0}, {1}", x, y));
            }
            Analytics.TrackTimedEvent(Categories.Performance, "ViewStartup", dynamoViewModel.Model.stopwatch.Elapsed, packages);
        }

        /// <summary>
        /// Call this method to optionally bring up terms of use dialog. User 
        /// needs to accept terms of use before any packages can be downloaded 
        /// from package manager.
        /// </summary>
        /// <returns>Returns true if the terms of use for downloading a package 
        /// is accepted by the user, or false otherwise. If this method returns 
        /// false, then download of package should be terminated.</returns>
        /// 
        private bool DisplayTermsOfUseForAcceptance()
        {
            var prefSettings = dynamoViewModel.Model.PreferenceSettings;
            if (prefSettings.PackageDownloadTouAccepted)
                return true; // User accepts the terms of use.

            var acceptedTermsOfUse = TermsOfUseHelper.ShowTermsOfUseDialog(false, null);
            prefSettings.PackageDownloadTouAccepted = acceptedTermsOfUse;

            // User may or may not accept the terms.
            return prefSettings.PackageDownloadTouAccepted;
        }

        private void DynamoView_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeNodeViewCustomizationEvents();
        }

        private void DynamoViewModelRequestAboutWindow(DynamoViewModel model)
        {
            var aboutWindow = model.BrandingResourceProvider.CreateAboutBox(model);
            aboutWindow.Owner = this;
            aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutWindow.ShowDialog();
        }

        private void OnGalleryBackgroundMouseClick(object sender, MouseButtonEventArgs e)
        {
            dynamoViewModel.CloseGalleryCommand.Execute(null);
            e.Handled = true;
        }

        private void DynamoViewModelRequestShowHideGallery(bool showGallery)
        {
            if (showGallery)
            {
                if (galleryView == null) //On-demand instantiation
                {
                    galleryView = new GalleryView(new GalleryViewModel(dynamoViewModel));
                    Grid.SetColumnSpan(galleryBackground, mainGrid.ColumnDefinitions.Count);
                    Grid.SetRowSpan(galleryBackground, mainGrid.RowDefinitions.Count);
                }

                if (galleryView.ViewModel.HasContents)
                {
                    galleryBackground.Children.Add(galleryView);
                    galleryBackground.Visibility = Visibility.Visible;
                    galleryView.Focus(); //get keyboard focus
                }
            }
            //hide gallery
            else
            {
                if (galleryBackground != null)
                {
                    if (galleryView != null && galleryBackground.Children.Contains(galleryView))
                        galleryBackground.Children.Remove(galleryView);

                    galleryBackground.Visibility = Visibility.Hidden;
                }
            }
        }

        private PublishPackageView _pubPkgView;

        private void DynamoViewModelRequestRequestPackageManagerPublish(PublishPackageViewModel model)
        {
            var cmd = Analytics.TrackCommandEvent("PublishPackage");
            if (_pubPkgView == null)
            {
                _pubPkgView = new PublishPackageView(model)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _pubPkgView.Closed += (sender, args) => { _pubPkgView = null; cmd.Dispose(); };
                _pubPkgView.Show();

                if (_pubPkgView.IsLoaded && IsLoaded) _pubPkgView.Owner = this;
            }

            _pubPkgView.Focus();
        }

        private PackageManagerSearchView _searchPkgsView;
        private PackageManagerSearchViewModel _pkgSearchVM;

        private void DynamoViewModelRequestShowPackageManagerSearch(object s, EventArgs e)
        {
            if (!DisplayTermsOfUseForAcceptance())
                return; // Terms of use not accepted.

            var cmd = Analytics.TrackCommandEvent("SearchPackage");
            if (_pkgSearchVM == null)
            {
                _pkgSearchVM = new PackageManagerSearchViewModel(dynamoViewModel.PackageManagerClientViewModel);
            }

            if (_searchPkgsView == null)
            {
                _searchPkgsView = new PackageManagerSearchView(_pkgSearchVM)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                _searchPkgsView.Closed += (sender, args) => { _searchPkgsView = null; cmd.Dispose(); };
                _searchPkgsView.Show();

                if (_searchPkgsView.IsLoaded && IsLoaded) _searchPkgsView.Owner = this;
            }

            _searchPkgsView.Focus();
            _pkgSearchVM.RefreshAndSearchAsync();
        }

        private void DynamoViewModelRequestPackagePaths(object sender, EventArgs e)
        {
            var loadPackagesParams = new LoadPackageParams {
                Preferences = dynamoViewModel.PreferenceSettings,
                PathManager = dynamoViewModel.Model.PathManager,
            };
            var customNodeManager = dynamoViewModel.Model.CustomNodeManager;
            var packageLoader = dynamoViewModel.Model.GetPackageManagerExtension().PackageLoader;
            var viewModel = new PackagePathViewModel(packageLoader,loadPackagesParams,customNodeManager);
            var view = new PackagePathView(viewModel) { Owner = this };
            view.ShowDialog();
        }

        private void DynamoViewModelChangeScaleFactor(object sender, EventArgs e)
        {
            var view = new Prompts.ChangeScaleFactorPrompt(dynamoViewModel.ScaleFactorLog) { Owner = this };
            if (view.ShowDialog() == true)
            {
                if (dynamoViewModel.ScaleFactorLog != view.SliderValue)
                {
                    dynamoViewModel.ScaleFactorLog = view.SliderValue;
                    dynamoViewModel.CurrentSpace.HasUnsavedChanges = true;

                    dynamoViewModel.ExecuteCommand(new DynamoModel.ForceRunCancelCommand(false, false));
                }
            }
        }

        private InstalledPackagesView _installedPkgsView;

        private void DynamoViewModelRequestShowInstalledPackages(object s, EventArgs e)
        {
            var cmd = Analytics.TrackCommandEvent("ManagePackage");
            if (_installedPkgsView == null)
            {
                var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
                _installedPkgsView = new InstalledPackagesView(new InstalledPackagesViewModel(dynamoViewModel,
                    pmExtension.PackageLoader))
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                _installedPkgsView.Closed += (sender, args) => { _installedPkgsView = null; cmd.Dispose(); };
                _installedPkgsView.Show();

                if (_installedPkgsView.IsLoaded && IsLoaded) _installedPkgsView.Owner = this;
            }
            _installedPkgsView.Focus();
        }

        private void ClipBoard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dynamoViewModel.CopyCommand.RaiseCanExecuteChanged();
            dynamoViewModel.PasteCommand.RaiseCanExecuteChanged();
        }

        private void DynamoViewModelRequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            var dialogText = "";
            // If the file is read only, display a different message.
            if (e.Workspace.IsReadOnly)
            {
                dialogText = String.Format(Dynamo.Wpf.Properties.Resources.MessageConfirmToSaveReadOnlyCustomNode, e.Workspace.FileName);
            }
            else
            {
                if (e.Workspace is CustomNodeWorkspaceModel)
                {
                    dialogText = String.Format(Dynamo.Wpf.Properties.Resources.MessageConfirmToSaveCustomNode, e.Workspace.Name);
                }
                else // home workspace
                {
                    if (string.IsNullOrEmpty(e.Workspace.FileName))
                    {
                        dialogText = Dynamo.Wpf.Properties.Resources.MessageConfirmToSaveHomeWorkSpace;
                    }
                    else
                    {
                        dialogText = String.Format(Dynamo.Wpf.Properties.Resources.MessageConfirmToSaveNamedHomeWorkSpace, Path.GetFileName(e.Workspace.FileName));
                    }
                }
            }

            var buttons = e.AllowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show(this  ,dialogText,
                Dynamo.Wpf.Properties.Resources.SaveConfirmationMessageBoxTitle,
                buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // If the file is read-only, redirect yes to save-as.
                if (e.Workspace.IsReadOnly)
                    dynamoViewModel.ShowSaveDialogAndSaveResult(e.Workspace);
                else
                    e.Success = dynamoViewModel.ShowSaveDialogIfNeededAndSave(e.Workspace);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                //return false;
                e.Success = false;
            }
            else
            {
                e.Success = true;
            }
        }

        private void Selection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            dynamoViewModel.CopyCommand.RaiseCanExecuteChanged();
            dynamoViewModel.PasteCommand.RaiseCanExecuteChanged();
            dynamoViewModel.NodeFromSelectionCommand.RaiseCanExecuteChanged();           
        }

        private void Controller_RequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            var prompt = new CrashPrompt(args, dynamoViewModel);
            prompt.ShowDialog();
        }

        private void Controller_RequestTaskDialog(object sender, TaskDialogEventArgs e)
        {
            var taskDialog = new UI.Prompts.GenericTaskDialog(e);
            taskDialog.ShowDialog();
        }

        private void DynamoViewModelRequestSaveImage(object sender, ImageSaveEventArgs e)
        {
            var workspace = this.ChildOfType<WorkspaceView>();
            workspace.SaveWorkspaceAsImage(e.Path);
        }

        private void DynamoViewModelRequestSave3DImage(object sender, ImageSaveEventArgs e)
        {
            var canvas = (DPFCanvas)BackgroundPreview.View.RenderHost;

            var encoder = new PngBitmapEncoder();
            var rtBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96,
                PixelFormats.Pbgra32);
            rtBitmap.Render(canvas);

            encoder.Frames.Add(BitmapFrame.Create(rtBitmap));

            if (File.Exists(e.Path))
            {
                File.Delete(e.Path);
            }

            using (var stream = File.Create(e.Path))
            {
                encoder.Save(stream);
            }
        }

        private void DynamoViewModelRequestClose(object sender, EventArgs e)
        {
            Close();
        }

        private void DynamoViewModelSidebarClosed(object sender, EventArgs e)
        {
            LibraryClicked(sender, e);
        }

        /// <summary>
        /// Handles the request for the presentation of the function name prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamoViewModelRequestsFunctionNamePrompt(object sender, FunctionNamePromptEventArgs e)
        {
            ShowNewFunctionDialog(e);
        }

        /// <summary>
        /// Presents the function name dialogue. Returns true if the user enters
        /// a function name and category.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        internal void ShowNewFunctionDialog(FunctionNamePromptEventArgs e)
        {
            var categorized =
                SearchCategoryUtil.CategorizeSearchEntries(
                    dynamoViewModel.Model.SearchModel.SearchEntries,
                    entry => entry.Categories);

            var allCategories =
                categorized.SubCategories.SelectMany(sub => sub.GetAllCategoryNames());

            var dialog = new FunctionNamePrompt(allCategories)
            {
                categoryBox = { Text = e.Category },
                DescriptionInput = { Text = e.Description },
                nameView = { Text = e.Name },
                nameBox = { Text = e.Name },
                // center the prompt
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (e.CanEditName)
            {
                dialog.nameBox.Visibility = Visibility.Visible;
                dialog.nameView.Visibility = Visibility.Collapsed;
            }
            else
            {
                dialog.nameView.Visibility = Visibility.Visible;
                dialog.nameBox.Visibility = Visibility.Collapsed;
            }

            if (dialog.ShowDialog() != true)
            {
                e.Success = false;
                return;
            }

            e.Name = dialog.Text;
            e.Category = dialog.Category;
            e.Description = dialog.Description;

            e.Success = true;
        }

        /// <summary>
        /// Handles the request for the presentation of the preset name prompt
        /// </summary>
        /// <param name="e">a parameter object contains default Name and Description,
        /// and Success bool returned from the dialog</param>
        private void DynamoViewModelRequestPresetNamePrompt (PresetsNamePromptEventArgs e)
        {
            ShowNewPresetDialog(e);
        }

        private void DynamoViewModelRequestPresetWarningPrompt()
        {
            ShowPresetWarning();
        }

        /// <summary>
        /// Presents the preset name dialogue. sets eventargs.Success to true if the user enters
        /// a preset name/timestamp and description.
        /// </summary>
        internal void ShowNewPresetDialog(PresetsNamePromptEventArgs e)
        {
            string error = "";

            do
            {
                var dialog = new PresetPrompt()
                {
                    DescriptionInput = { Text = e.Description },
                    nameView = { Text = "" },
                    nameBox = { Text = e.Name },
                    // center the prompt
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

               
                if (dialog.ShowDialog() != true)
                {
                    e.Success = false;
                    return;
                }

                if (String.IsNullOrEmpty(dialog.Text))
                {
                   //if the name is empty, then default to the current time
                    e.Name = System.DateTime.Now.ToString();
                    break;
                }
                else
                {
                    error = "";
                }

                e.Name = dialog.Text;
                e.Description = dialog.Description;

            } while (!error.Equals(""));

            e.Success = true;
        }

        private void ShowPresetWarning()
        {
            var newDialog = new  PresetOverwritePrompt()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Text =  Wpf.Properties.Resources.PresetWarningMessage,
                IsCancelButtonVisible = Visibility.Collapsed
            };

            newDialog.ShowDialog();
        }
        
        private bool PerformShutdownSequenceOnViewModel()
        {
            // Test cases that make use of views (e.g. recorded tests) have 
            // their own tear down logic as part of the test set-up (mainly 
            // because DynamoModel should stay long enough for verification
            // code to verify data much later than the window closing).
            // 
            if (DynamoModel.IsTestMode)
                return false;

            var sp = new DynamoViewModel.ShutdownParams(
                shutdownHost: false,
                allowCancellation: true,
                closeDynamoView: false);

            if (dynamoViewModel.PerformShutdownSequence(sp))
            {
                //Shutdown wasn't cancelled
                SizeChanged -= DynamoView_SizeChanged;
                LocationChanged -= DynamoView_LocationChanged;
                return true;
            }
            else
            {
                //Shutdown was cancelled
                return false;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (!PerformShutdownSequenceOnViewModel() && !DynamoModel.IsTestMode)
            {
                e.Cancel = true;
            }
            else
            {
                isPSSCalledOnViewModelNoCancel = true;
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            //There will be chances that WindowsClosed is called but WindowClosing is not called.
            //This is to ensure PerformShutdownSequence is always called on the view model.
            if (!isPSSCalledOnViewModelNoCancel)
            {
                PerformShutdownSequenceOnViewModel();
            }

            dynamoViewModel.Model.RequestLayoutUpdate -= vm_RequestLayoutUpdate;
            dynamoViewModel.RequestViewOperation -= DynamoViewModelRequestViewOperation;

            //PACKAGE MANAGER
            dynamoViewModel.RequestPackagePublishDialog -= DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestManagePackagesDialog -= DynamoViewModelRequestShowInstalledPackages;
            dynamoViewModel.RequestPackageManagerSearchDialog -= DynamoViewModelRequestShowPackageManagerSearch;
            dynamoViewModel.RequestPackagePathsDialog -= DynamoViewModelRequestPackagePaths;

            //FUNCTION NAME PROMPT
            dynamoViewModel.Model.RequestsFunctionNamePrompt -= DynamoViewModelRequestsFunctionNamePrompt;

            //Preset Name Prompt
            dynamoViewModel.Model.RequestPresetsNamePrompt -= DynamoViewModelRequestPresetNamePrompt;
            dynamoViewModel.RequestPresetsWarningPrompt -= DynamoViewModelRequestPresetWarningPrompt;

            dynamoViewModel.RequestClose -= DynamoViewModelRequestClose;
            dynamoViewModel.RequestSaveImage -= DynamoViewModelRequestSaveImage;
            dynamoViewModel.SidebarClosed -= DynamoViewModelSidebarClosed;

            DynamoSelection.Instance.Selection.CollectionChanged -= Selection_CollectionChanged;

            dynamoViewModel.RequestUserSaveWorkflow -= DynamoViewModelRequestUserSaveWorkflow;

            if (dynamoViewModel.Model != null)
            {
                dynamoViewModel.Model.RequestsCrashPrompt -= Controller_RequestsCrashPrompt;
                dynamoViewModel.Model.RequestTaskDialog -= Controller_RequestTaskDialog;
                dynamoViewModel.Model.ClipBoard.CollectionChanged -= ClipBoard_CollectionChanged;
            }

            //ABOUT WINDOW
            dynamoViewModel.RequestAboutWindow -= DynamoViewModelRequestAboutWindow;

            //SHOW or HIDE GALLERY
            dynamoViewModel.RequestShowHideGallery -= DynamoViewModelRequestShowHideGallery;

            foreach (var ext in viewExtensionManager.ViewExtensions)
            {
                try
                {
                    ext.Shutdown();
                }
                catch (Exception exc)
                {
                    Log(ext.Name + ": " + exc.Message);
                }
            }

            viewExtensionManager.MessageLogged -= Log;
        }

        // the key press event is being intercepted before it can get to
        // the active workspace. This code simply grabs the key presses and
        // passes it to thecurrent workspace
        private void DynamoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || !IsMouseOver) return;

            var vm = dynamoViewModel.BackgroundPreviewViewModel;


            // ESC key to navigate has long lag on some machines.
            // This issue was caused by using KeyEventArgs.IsRepeated API
            // In order to fix this we need to use our own extension method DelayInvoke to determine
            // whether ESC key is being held down or not
            if (!IsEscKeyPressed && !vm.NavigationKeyIsDown)
            {
                IsEscKeyPressed = true;
                dynamoViewModel.UIDispatcher.DelayInvoke(navigationInterval, () =>
                {
                    if (IsEscKeyPressed)
                    {
                        vm.NavigationKeyIsDown = true;
                    }
                });
            }           

            else
            {
                vm.CancelNavigationState();
            }

            e.Handled = true;
        }

        private void DynamoView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            IsEscKeyPressed = false;
            if (dynamoViewModel.BackgroundPreviewViewModel.CanNavigateBackground)
            {
                dynamoViewModel.BackgroundPreviewViewModel.NavigationKeyIsDown = false;
                dynamoViewModel.EscapeCommand.Execute(null);
                e.Handled = true;
            }            
        }

        private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dynamoViewModel != null)
            {
                int workspace_index = dynamoViewModel.CurrentWorkspaceIndex;

                //this condition is added for shutdown when we are clearing
                //the workspace collection
                if (workspace_index == -1) return;

                var workspace_vm = dynamoViewModel.Workspaces[workspace_index];
                workspace_vm.Model.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(workspace_vm.Model.X, workspace_vm.Model.Y)));
                workspace_vm.Model.OnZoomChanged(this, new ZoomEventArgs(workspace_vm.Zoom));

                ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogScroller.ScrollToBottom();
        }

        private void LoadPresetsMenus(object sender, RoutedEventArgs e)
        {
            //grab serialized presets from current workspace
            var PresetSet = dynamoViewModel.Model.CurrentWorkspace.Presets;
            // now grab all the states off the set and create a menu item for each one

            var statesMenu = (sender as MenuItem);
            var senderItems =  statesMenu.Items.OfType<MenuItem>().Select(x => x.Tag).ToList();
            //only update the states menus if the states have been updated or the user
            // has switched workspace contexts, can check if stateItems List is different
            //from the presets on the current workspace
            if (!PresetSet.SequenceEqual(senderItems))
            {
                //dispose all state items in the menu
                statesMenu.Items.Clear();

                foreach (var state in PresetSet)
              {
                //create a new menu item for each state in the options set
                //when any of this buttons are clicked we'll call the SetWorkSpaceToStateCommand(state)
                 var stateItem = new MenuItem
                        {
                            Header = state.Name,
                            Tag = state
                        };
                  //if the sender was the restoremenu then add restore delegates
                 if (sender == RestorePresetMenu)
                 {
                     stateItem.Click += RestoreState_Click;
                 }
                 else
                 {
                     //else the sender was the delete menu
                     stateItem.Click += DeleteState_Click;
                 }
                 stateItem.ToolTip = state.Description;
                 ((MenuItem)sender).Items.Add(stateItem);
                }
            }
        }


        private void RestoreState_Click(object sender, RoutedEventArgs e)
        {
            PresetModel state = (sender as MenuItem).Tag as PresetModel;
            var workspace = dynamoViewModel.CurrentSpace;
            dynamoViewModel.ExecuteCommand(new DynamoModel.ApplyPresetCommand(workspace.Guid, state.GUID));
        }

        private void DeleteState_Click(object sender, RoutedEventArgs e)
        {
            PresetModel state = (sender as MenuItem).Tag as PresetModel;
            var workspace = dynamoViewModel.CurrentSpace;
            workspace.HasUnsavedChanges = true;
            dynamoViewModel.Model.ExecuteCommand(new DynamoModel.DeleteModelCommand(state.GUID));
            //This is to remove the PATH (>) indicator from the preset submenu header
            //if there are no presets.
            dynamoViewModel.ShowNewPresetsDialogCommand.RaiseCanExecuteChanged();                       
        }
        
#if !__NO_SAMPLES_MENU
        /// <summary>
        ///     Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        private void LoadSamplesMenu()
        {
            var samplesDirectory = dynamoViewModel.Model.PathManager.SamplesDirectory;
            if (Directory.Exists(samplesDirectory))
            {
                var sampleFiles = new System.Collections.Generic.List<string>();
                string[] dirPaths = Directory.GetDirectories(samplesDirectory);
                string[] filePaths = Directory.GetFiles(samplesDirectory, "*.dyn");

                // handle top-level files
                if (filePaths.Any())
                {
                    foreach (string path in filePaths)
                    {
                        var item = new MenuItem
                        {
                            Header = Path.GetFileNameWithoutExtension(path),
                            Tag = path
                        };
                        item.Click += OpenSample_Click;
                        SamplesMenu.Items.Add(item);
                        sampleFiles.Add(path);
                    }
                }

                // handle top-level dirs, TODO - factor out to a seperate function, make recusive
                if (dirPaths.Any())
                {
                    foreach (string dirPath in dirPaths)
                    {
                        var dirItem = new MenuItem
                        {
                            Header = Path.GetFileName(dirPath),
                            Tag = Path.GetFileName(dirPath)
                        };

                        filePaths = Directory.GetFiles(dirPath, "*.dyn");
                        if (filePaths.Any())
                        {
                            foreach (string path in filePaths)
                            {
                                var item = new MenuItem
                                {
                                    Header = Path.GetFileNameWithoutExtension(path),
                                    Tag = path
                                };
                                item.Click += OpenSample_Click;
                                dirItem.Items.Add(item);
                                sampleFiles.Add(path);
                            }
                        }
                        SamplesMenu.Items.Add(dirItem);
                    }
                }

                if (dirPaths.Any())
                {
                    var showInFolder = new MenuItem
                    {
                        Header = Wpf.Properties.Resources.DynamoViewHelpMenuShowInFolder,
                        Tag = dirPaths[0]
                    };
                    showInFolder.Click += OnShowInFolder;
                    SamplesMenu.Items.Add(new Separator());
                    SamplesMenu.Items.Add(showInFolder);
                }

                if (sampleFiles.Any() && startPage != null)
                {
                    var firstFilePath = Path.GetDirectoryName(sampleFiles.ToArray()[0]);
                    var rootPath = Path.GetDirectoryName(firstFilePath);
                    var root = new DirectoryInfo(rootPath);
                    var rootProperty = new SampleFileEntry("Samples", "Path");
                    startPage.WalkDirectoryTree(root, rootProperty);
                    startPage.SampleFiles.Add(rootProperty);
                }
            }
        }

        private static void OnShowInFolder(object sender, RoutedEventArgs e)
        {
            var folderPath = (string)((MenuItem)sender).Tag;
            Process.Start("explorer.exe", "/select," + folderPath);
        }
#endif

        /// <summary>
        /// Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        private void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((MenuItem)sender).Tag;

            var workspace = dynamoViewModel.HomeSpace;
            if (workspace.HasUnsavedChanges)
            {
                if (!dynamoViewModel.AskUserToSaveWorkspaceOrCancel(workspace))
                    return; // User has not saved his/her work.
            }

            dynamoViewModel.Model.CurrentWorkspace = dynamoViewModel.HomeSpace;
            dynamoViewModel.OpenCommand.Execute(path);
        }

        private void TabControlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be = ((MenuItem)sender).GetBindingExpression(HeaderedItemsControl.HeaderProperty);
            WorkspaceViewModel wsvm = (WorkspaceViewModel)be.DataItem;
            WorkspaceTabs.SelectedIndex = dynamoViewModel.Workspaces.IndexOf(wsvm);
            ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
        }

        public CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            Point popupLocation = new Point(targetSize.Width - popupSize.Width, targetSize.Height);

            CustomPopupPlacement placement1 =
                new CustomPopupPlacement(popupLocation, PopupPrimaryAxis.Vertical);

            CustomPopupPlacement placement2 =
                new CustomPopupPlacement(popupLocation, PopupPrimaryAxis.Horizontal);

            CustomPopupPlacement[] ttplaces =
                new CustomPopupPlacement[] { placement1, placement2 };
            return ttplaces;
        }

        private void ToggleWorkspaceTabVisibility(int tabSelected)
        {
            SlideWindowToIncludeTab(tabSelected);

            for (int tabIndex = 1; tabIndex < WorkspaceTabs.Items.Count; tabIndex++)
            {
                TabItem tabItem = (TabItem)WorkspaceTabs.ItemContainerGenerator.ContainerFromIndex(tabIndex);
                if (tabIndex < tabSlidingWindowStart || tabIndex > tabSlidingWindowEnd)
                    tabItem.Visibility = Visibility.Collapsed;
                else
                    tabItem.Visibility = Visibility.Visible;
            }
        }

        private int GetSlidingWindowSize()
        {
            int tabCount = WorkspaceTabs.Items.Count;

            // Note: returning -1 for Home tab being always visible.
            // Home tab is not taken account into sliding window
            if (tabCount > Configurations.MinTabsBeforeClipping)
            {
                // Usable tab control width need to exclude tabcontrol menu
                int usableTabControlWidth = (int)WorkspaceTabs.ActualWidth - Configurations.TabControlMenuWidth;

                int fullWidthTabsVisible = usableTabControlWidth / Configurations.TabDefaultWidth;

                if (fullWidthTabsVisible < Configurations.MinTabsBeforeClipping)
                    return Configurations.MinTabsBeforeClipping - 1;
                else
                {
                    if (tabCount < fullWidthTabsVisible)
                        return tabCount - 1;

                    return fullWidthTabsVisible - 1;
                }
            }
            else
                return tabCount - 1;
        }

        private void SlideWindowToIncludeTab(int tabSelected)
        {
            int newSlidingWindowSize = GetSlidingWindowSize();

            if (newSlidingWindowSize == 0)
            {
                tabSlidingWindowStart = tabSlidingWindowEnd = 0;
                return;
            }

            if (tabSelected != 0)
            {
                // Selection is not home tab
                if (tabSelected < tabSlidingWindowStart)
                {
                    // Slide window towards the front
                    tabSlidingWindowStart = tabSelected;
                    tabSlidingWindowEnd = tabSlidingWindowStart + (newSlidingWindowSize - 1);
                }
                else if (tabSelected > tabSlidingWindowEnd)
                {
                    // Slide window towards the end
                    tabSlidingWindowEnd = tabSelected;
                    tabSlidingWindowStart = tabSlidingWindowEnd - (newSlidingWindowSize - 1);
                }
                else
                {
                    int currentSlidingWindowSize = tabSlidingWindowEnd - tabSlidingWindowStart + 1;
                    int windowDiff = Math.Abs(currentSlidingWindowSize - newSlidingWindowSize);

                    // Handles sliding window size change caused by window resizing
                    if (currentSlidingWindowSize > newSlidingWindowSize)
                    {
                        // Trim window
                        while (windowDiff > 0)
                        {
                            if (tabSelected == tabSlidingWindowEnd)
                                tabSlidingWindowStart++; // Trim from front
                            else
                                tabSlidingWindowEnd--; // Trim from end

                            windowDiff--;
                        }
                    }
                    else if (currentSlidingWindowSize < newSlidingWindowSize)
                    {
                        // Expand window
                        int lastTab = WorkspaceTabs.Items.Count - 1;

                        while (windowDiff > 0)
                        {
                            if (tabSlidingWindowEnd == lastTab)
                                tabSlidingWindowStart--;
                            else
                                tabSlidingWindowEnd++;

                            windowDiff--;
                        }
                    }
                    else
                    {
                        // Handle tab closing

                    }
                }
            }
            else
            {
                // Selection is home tab
                int currentSlidingWindowSize = tabSlidingWindowEnd - tabSlidingWindowStart + 1;
                int windowDiff = Math.Abs(currentSlidingWindowSize - newSlidingWindowSize);

                int lastTab = WorkspaceTabs.Items.Count - 1;

                // Handles sliding window size change caused by window resizing and tab close
                if (currentSlidingWindowSize > newSlidingWindowSize)
                {
                    // Trim window
                    while (windowDiff > 0)
                    {
                        tabSlidingWindowEnd--; // Trim from end

                        windowDiff--;
                    }
                }
                else if (currentSlidingWindowSize < newSlidingWindowSize)
                {
                    // Expand window due to window resize
                    while (windowDiff > 0)
                    {
                        if (tabSlidingWindowEnd == lastTab)
                            tabSlidingWindowStart--;
                        else
                            tabSlidingWindowEnd++;

                        windowDiff--;
                    }
                }
                else
                {
                    // Handle tab closing with no change in window size
                    // Shift window
                    if (tabSlidingWindowEnd > lastTab)
                    {
                        tabSlidingWindowStart--;
                        tabSlidingWindowEnd--;
                    }
                    // Handle case that selected tab is still 0 and 
                    // a new tab is created. 
                    else if (tabSlidingWindowEnd < lastTab)
                    {
                        tabSlidingWindowEnd++;
                    }
                }
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            TextBlock tb = (TextBlock)(g.Children[1]);
            var bc = new BrushConverter();
            tb.Foreground = (Brush)bc.ConvertFrom("#cccccc");
            Image collapseIcon = (Image)g.Children[0];
            var imageUri = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_hover.png");

            BitmapImage hover = new BitmapImage(imageUri);
            // hover.Rotation = Rotation.Rotate180;

            collapseIcon.Source = hover;
        }

        private void OnCollapsedSidebarClick(object sender, EventArgs e)
        {
            LibraryViewColumn.MinWidth = Configurations.MinWidthLibraryView;

            UserControl view = (UserControl)sidebarGrid.Children[0];
            if (view.Visibility == Visibility.Collapsed)
            {
                view.Width = double.NaN;
                view.HorizontalAlignment = HorizontalAlignment.Stretch;
                view.Height = double.NaN;
                view.VerticalAlignment = VerticalAlignment.Stretch;

                mainGrid.ColumnDefinitions[0].Width = new GridLength(restoreWidth);
                verticalSplitter.Visibility = Visibility.Visible;
                view.Visibility = Visibility.Visible;
                sidebarGrid.Visibility = Visibility.Visible;
                collapsedSidebar.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            TextBlock tb = (TextBlock)(g.Children[1]);
            var bc = new BrushConverter();
            tb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            Image collapseIcon = (Image)g.Children[0];

            // Change the collapse icon and rotate
            var imageUri = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_normal.png");
            BitmapImage hover = new BitmapImage(imageUri);

            collapseIcon.Source = hover;
        }

        private double restoreWidth = 0;

        private void LibraryClicked(object sender, EventArgs e)
        {
            restoreWidth = sidebarGrid.ActualWidth;
            LibraryViewColumn.MinWidth = 0;

            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
            verticalSplitter.Visibility = Visibility.Collapsed;
            sidebarGrid.Visibility = Visibility.Collapsed;

            horizontalSplitter.Width = double.NaN;
            UserControl view = (UserControl)sidebarGrid.Children[0];
            view.Visibility = Visibility.Collapsed;

            sidebarGrid.Visibility = Visibility.Collapsed;
            collapsedSidebar.Visibility = Visibility.Visible;
        }

        private void Workspace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //http://stackoverflow.com/questions/4474670/how-to-catch-the-ending-resize-window

            // Children of the workspace, including the zoom border and the endless grid
            // are expensive to resize. We use a timer here to defer resizing until 
            // after workspace resizing is complete. This improves the responziveness of
            // the UI during resize.

            _workspaceResizeTimer.IsEnabled = true;
            _workspaceResizeTimer.Stop();
            _workspaceResizeTimer.Start();
        }

        private void _resizeTimer_Tick(object sender, EventArgs e)
        {
            _workspaceResizeTimer.IsEnabled = false;

            // end of timer processing
            if (dynamoViewModel == null)
                return;
            dynamoViewModel.WorkspaceActualSize(border.ActualWidth, border.ActualHeight);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dynamoViewModel.IsMouseDown = true;
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            dynamoViewModel.IsMouseDown = false;
        }

        private void WorkspaceTabs_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (WorkspaceTabs.SelectedIndex >= 0)
                ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
        }

        private void WorkspaceTabs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
        }

        private void DynamoView_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Activate();
                // Note that you can have more than one file.
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (dynamoViewModel.HomeSpace.HasUnsavedChanges && !dynamoViewModel.AskUserToSaveWorkspaceOrCancel(dynamoViewModel.HomeSpace))
                {
                    return;
                }

                if (dynamoViewModel.OpenCommand.CanExecute(files[0]))
                {
                    dynamoViewModel.OpenCommand.Execute(files[0]);
                }

            }

            e.Handled = true;
        }


        private void Log(ILogMessage obj)
        {
            dynamoViewModel.Model.Logger.Log(obj);
        }

        private void Log(string message)
        {
            Log(LogMessage.Info(message));
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HidePopupWhenWindowDeactivated();
        }

        public void Dispose()
        {
            foreach (var ext in viewExtensionManager.ViewExtensions)
            {
                try
                {
                    ext.Dispose();
                }
                catch (Exception exc)
                {
                    Log(ext.Name + ": " + exc.Message);
                }
            }
            if (dynamoViewModel.Model.AuthenticationManager.HasAuthProvider && loginService != null)
            {
                dynamoViewModel.Model.AuthenticationManager.AuthProvider.RequestLogin -= loginService.ShowLogin;
            }
        }
    }
}

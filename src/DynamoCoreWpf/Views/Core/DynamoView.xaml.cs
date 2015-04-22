using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Prompts;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.Controls;
using DynamoUtilities;

using String = System.String;
using System.Windows.Data;
using Dynamo.UI.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Services;
using ResourceNames = Dynamo.Wpf.Interfaces.ResourceNames;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class DynamoView : Window
    {
        private readonly NodeViewCustomizationLibrary nodeViewCustomizationLibrary;
        private DynamoViewModel dynamoViewModel;
        private Stopwatch _timer;
        private StartPageViewModel startPage;
        private int tabSlidingWindowStart, tabSlidingWindowEnd;

        DispatcherTimer _workspaceResizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500), IsEnabled = false };

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

        bool CheckVirtualScreenSize()
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

        void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            dynamoViewModel.Model.PreferenceSettings.WindowX = Left;
            dynamoViewModel.Model.PreferenceSettings.WindowY = Top;
        }

        void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dynamoViewModel.Model.PreferenceSettings.WindowW = e.NewSize.Width;
            dynamoViewModel.Model.PreferenceSettings.WindowH = e.NewSize.Height;
        }

        void InitializeLogin()
        {
            if ( dynamoViewModel.ShowLogin && dynamoViewModel.PackageManagerClientViewModel.HasAuthProvider)
            {
                var login = new Login(dynamoViewModel.PackageManagerClientViewModel);
                loginGrid.Children.Add(login);
            }
        }

        void InitializeShortcutBar()
        {
            ShortcutToolbar shortcutBar = new ShortcutToolbar(this.dynamoViewModel.Model.UpdateManager);
            shortcutBar.Name = "ShortcutToolbar";

            ShortcutBarItem newScriptButton = new ShortcutBarItem();
            newScriptButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarNewButtonTooltip;
            newScriptButton.ShortcutCommand = dynamoViewModel.NewHomeWorkspaceCommand;
            newScriptButton.ShortcutCommandParameter = null;
            newScriptButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/new_normal.png";
            newScriptButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/new_disabled.png";
            newScriptButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/new_hover.png";

            ShortcutBarItem openScriptButton = new ShortcutBarItem();
            openScriptButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarOpenButtonTooltip;
            openScriptButton.ShortcutCommand = dynamoViewModel.ShowOpenDialogAndOpenResultCommand;
            openScriptButton.ShortcutCommandParameter = null;
            openScriptButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/open_normal.png";
            openScriptButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/open_disabled.png";
            openScriptButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/open_hover.png";

            ShortcutBarItem saveButton = new ShortcutBarItem();
            saveButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarSaveButtonTooltip;
            saveButton.ShortcutCommand = dynamoViewModel.ShowSaveDialogIfNeededAndSaveResultCommand;
            saveButton.ShortcutCommandParameter = null;
            saveButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/save_normal.png";
            saveButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/save_disabled.png";
            saveButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/save_hover.png";

            ShortcutBarItem screenShotButton = new ShortcutBarItem();
            screenShotButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarExportButtonTooltip;
            screenShotButton.ShortcutCommand = dynamoViewModel.ShowSaveImageDialogAndSaveResultCommand;
            screenShotButton.ShortcutCommandParameter = null;
            screenShotButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/screenshot_normal.png";
            screenShotButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/screenshot_disabled.png";
            screenShotButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/screenshot_hover.png";

            ShortcutBarItem undoButton = new ShortcutBarItem();
            undoButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarUndoButtonTooltip;
            undoButton.ShortcutCommand = dynamoViewModel.UndoCommand;
            undoButton.ShortcutCommandParameter = null;
            undoButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/undo_normal.png";
            undoButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/undo_disabled.png";
            undoButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/undo_hover.png";

            ShortcutBarItem redoButton = new ShortcutBarItem();
            redoButton.ShortcutToolTip = Dynamo.Wpf.Properties.Resources.DynamoViewToolbarRedoButtonTooltip;
            redoButton.ShortcutCommand = dynamoViewModel.RedoCommand;
            redoButton.ShortcutCommandParameter = null;
            redoButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/redo_normal.png";
            redoButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/redo_disabled.png";
            redoButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/redo_hover.png";

            // PLACEHOLDER FOR FUTURE SHORTCUTS
            //ShortcutBarItem runButton = new ShortcutBarItem();
            //runButton.ShortcutToolTip = "Run [Ctrl + R]";
            ////runButton.ShortcutCommand = viewModel.RunExpressionCommand; // Function implementation in progress
            //runButton.ShortcutCommandParameter = null;
            //runButton.ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/run_normal.png";
            //runButton.ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/run_disabled.png";
            //runButton.ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/run_hover.png";

            shortcutBar.ShortcutBarItems.Add(newScriptButton);
            shortcutBar.ShortcutBarItems.Add(openScriptButton);
            shortcutBar.ShortcutBarItems.Add(saveButton);
            shortcutBar.ShortcutBarItems.Add(undoButton);
            shortcutBar.ShortcutBarItems.Add(redoButton);
            //shortcutBar.ShortcutBarItems.Add(runButton);            

            //shortcutBar.ShortcutBarRightSideItems.Add(updateButton);
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
        /// 
        private void InitializeStartPage()
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

                startPage = new StartPageViewModel(dynamoViewModel);
                startPageItemsControl.Items.Add(startPage);
            }
        }

        void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(UpdateLayout), DispatcherPriority.Render, null);
        }

        void DynamoViewModelRequestViewOperation(ViewOperationEventArgs e)
        {
            if (dynamoViewModel.CanNavigateBackground == false)
                return;

            switch (e.ViewOperation)
            {
                case ViewOperationEventArgs.Operation.FitView:
                    background_preview.View.ZoomExtents();
                    break;

                case ViewOperationEventArgs.Operation.ZoomIn:
                    var camera1 = background_preview.View.CameraController;
                    camera1.Zoom(-0.5 * background_preview.View.ZoomSensitivity);
                    break;

                case ViewOperationEventArgs.Operation.ZoomOut:
                    var camera2 = background_preview.View.CameraController;
                    camera2.Zoom(0.5 * background_preview.View.ZoomSensitivity);
                    break;
            }
        }

        private void DynamoView_Loaded(object sender, EventArgs e)
        {

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
            InitializeStartPage();

#if !__NO_SAMPLES_MENU
            LoadSamplesMenu();
#endif
            #region Search and incanvas search initialization

            var search = new SearchView(
                dynamoViewModel.SearchViewModel,
                dynamoViewModel);
            sidebarGrid.Children.Add(search);
            dynamoViewModel.SearchViewModel.Visible = true;

            var incanvasSeachViewModel = new SearchViewModel(dynamoViewModel, dynamoViewModel.Model.SearchModel);
            incanvasSeachViewModel.Visible = true;
            IncanvasSearch.DataContext = incanvasSeachViewModel;
            #endregion

            #region Package manager

            dynamoViewModel.RequestPackagePublishDialog += DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestManagePackagesDialog += DynamoViewModelRequestShowInstalledPackages;
            dynamoViewModel.RequestPackageManagerSearchDialog += DynamoViewModelRequestShowPackageManagerSearch;

            #endregion

            #region Node view injection

            // scan for node view overrides



            #endregion

            //FUNCTION NAME PROMPT
            dynamoViewModel.Model.RequestsFunctionNamePrompt += DynamoViewModelRequestsFunctionNamePrompt;

            dynamoViewModel.RequestClose += DynamoViewModelRequestClose;
            dynamoViewModel.RequestSaveImage += DynamoViewModelRequestSaveImage;
            dynamoViewModel.SidebarClosed += DynamoViewModelSidebarClosed;

            dynamoViewModel.Model.RequestsCrashPrompt += Controller_RequestsCrashPrompt;
            dynamoViewModel.Model.RequestTaskDialog += Controller_RequestTaskDialog;

            DynamoSelection.Instance.Selection.CollectionChanged += Selection_CollectionChanged;

            dynamoViewModel.RequestUserSaveWorkflow += DynamoViewModelRequestUserSaveWorkflow;

            dynamoViewModel.Model.ClipBoard.CollectionChanged += ClipBoard_CollectionChanged;

            //ABOUT WINDOW
            dynamoViewModel.RequestAboutWindow += DynamoViewModelRequestAboutWindow;

            LoadNodeViewCustomizations();
            SubscribeNodeViewCustomizationEvents();

            // Kick start the automation run, if possible.
            dynamoViewModel.BeginCommandPlayback(this);

            watchSettingsControl.DataContext = background_preview;
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
        bool DisplayTermsOfUseForAcceptance()
        {
            var prefSettings = dynamoViewModel.Model.PreferenceSettings;
            if (prefSettings.PackageDownloadTouAccepted)
                return true; // User accepts the terms of use.

            var acceptedTermsOfUse = TermsOfUseHelper.ShowTermsOfUseDialog();
            prefSettings.PackageDownloadTouAccepted = acceptedTermsOfUse;

            // User may or may not accept the terms.
            return prefSettings.PackageDownloadTouAccepted;
        }

        void DynamoView_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeNodeViewCustomizationEvents();
        }

        void DynamoViewModelRequestAboutWindow(DynamoViewModel model)
        {
            var aboutWindow = model.BrandingResourceProvider.CreateAboutBox(model);
            aboutWindow.Owner = this;
            aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutWindow.ShowDialog();
        }

        private PublishPackageView _pubPkgView;
        void DynamoViewModelRequestRequestPackageManagerPublish(PublishPackageViewModel model)
        {
            if (_pubPkgView == null)
            {
                _pubPkgView = new PublishPackageView(model)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _pubPkgView.Closed += (sender, args) => _pubPkgView = null;
                _pubPkgView.Show();

                if (_pubPkgView.IsLoaded && IsLoaded) _pubPkgView.Owner = this;
            }

            _pubPkgView.Focus();
        }

        private PackageManagerSearchView _searchPkgsView;
        private PackageManagerSearchViewModel _pkgSearchVM;
        void DynamoViewModelRequestShowPackageManagerSearch(object s, EventArgs e)
        {
            if (!DisplayTermsOfUseForAcceptance())
                return; // Terms of use not accepted.

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

                _searchPkgsView.Closed += (sender, args) => _searchPkgsView = null;
                _searchPkgsView.Show();

                if (_searchPkgsView.IsLoaded && IsLoaded) _searchPkgsView.Owner = this;
            }

            _searchPkgsView.Focus();
            _pkgSearchVM.RefreshAndSearchAsync();
        }

        private InstalledPackagesView _installedPkgsView;
        void DynamoViewModelRequestShowInstalledPackages(object s, EventArgs e)
        {
            if (_installedPkgsView == null)
            {
                _installedPkgsView = new InstalledPackagesView(new InstalledPackagesViewModel(dynamoViewModel,
                    dynamoViewModel.Model.PackageLoader))
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                _installedPkgsView.Closed += (sender, args) => _installedPkgsView = null;
                _installedPkgsView.Show();

                if (_installedPkgsView.IsLoaded && IsLoaded) _installedPkgsView.Owner = this;
            }
            _installedPkgsView.Focus();
        }

        void ClipBoard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dynamoViewModel.CopyCommand.RaiseCanExecuteChanged();
            dynamoViewModel.PasteCommand.RaiseCanExecuteChanged();
        }

        void DynamoViewModelRequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            var dialogText = "";
            if (e.Workspace is CustomNodeWorkspaceModel)
            {
                dialogText = String.Format(Dynamo.Wpf.Properties.Resources.MessageConfirmToSaveCustomNode, e.Workspace.Name);
            }
            else // homeworkspace
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

            var buttons = e.AllowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show(dialogText,
                Dynamo.Wpf.Properties.Resources.SaveConfirmationMessageBoxTitle,
                buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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

        void Selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dynamoViewModel.CopyCommand.RaiseCanExecuteChanged();
            dynamoViewModel.PasteCommand.RaiseCanExecuteChanged();
        }

        void Controller_RequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            var prompt = new CrashPrompt(args,dynamoViewModel);
            prompt.ShowDialog();
        }

        void Controller_RequestTaskDialog(object sender, TaskDialogEventArgs e)
        {
            var taskDialog = new UI.Prompts.GenericTaskDialog(e);
            taskDialog.ShowDialog();
        }

        void DynamoViewModelRequestSaveImage(object sender, ImageSaveEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Path))
            {
                var control = WpfUtilities.ChildOfType<ZoomBorder>(this, "zoomBorder");

                double width = 1;
                double height = 1;

                // connectors are most often within the bounding box of the nodes and notes

                foreach (NodeModel n in dynamoViewModel.Model.CurrentWorkspace.Nodes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                foreach (NoteModel n in dynamoViewModel.Model.CurrentWorkspace.Notes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                var rtb = new RenderTargetBitmap(Math.Max((int)control.ActualWidth, (int)width),
                                                  Math.Max((int)control.ActualHeight, (int)height),
                                                  96,
                                                  96,
                                                  PixelFormats.Default);

                rtb.Render(control);

                //endcode as PNG
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                try
                {
                    using (var stm = File.Create(e.Path))
                    {
                        pngEncoder.Save(stm);
                    }
                }
                catch
                {
                    dynamoViewModel.Model.Logger.Log(Wpf.Properties.Resources.MessageFailedToSaveAsImage);
                }
            }
        }

        void DynamoViewModelRequestClose(object sender, EventArgs e)
        {
            Close();
        }

        void DynamoViewModelSidebarClosed(object sender, EventArgs e)
        {
            LibraryClicked(sender, e);
        }

        /// <summary>
        /// Handles the request for the presentation of the function name prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DynamoViewModelRequestsFunctionNamePrompt(object sender, FunctionNamePromptEventArgs e)
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
        public void ShowNewFunctionDialog(FunctionNamePromptEventArgs e)
        {
            string error = "";

            do
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

                if (String.IsNullOrEmpty(dialog.Text))
                {
                    MessageBox.Show(Dynamo.Wpf.Properties.Resources.MessageCustomNodeNoName,
                        Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //else if (e.Name != dialog.Text && dynamoViewModel.Model.BuiltInTypesByNickname.ContainsKey(dialog.Text))
                //{
                //    error = "A built-in node with the given name already exists.";
                //    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                //                                   MessageBoxImage.Error);
                //}

                else if (dialog.Category.Equals(""))
                {
                    MessageBox.Show(Dynamo.Wpf.Properties.Resources.MessageCustomNodeNeedNewCategory,
                        Dynamo.Wpf.Properties.Resources.CustomNodePropertyErrorMessageBoxTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    error = "";
                }

                e.Name = dialog.Text;
                e.Category = dialog.Category;
                e.Description = dialog.Description;

            } while (!error.Equals(""));

            e.Success = true;
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            // Test cases that make use of views (e.g. recorded tests) have 
            // their own tear down logic as part of the test set-up (mainly 
            // because DynamoModel should stay long enough for verification
            // code to verify data much later than the window closing).
            // 
            if (DynamoModel.IsTestMode)
                return;

            var sp = new DynamoViewModel.ShutdownParams(
                shutdownHost: false,
                allowCancellation: true,
                closeDynamoView: false);

            if (dynamoViewModel.PerformShutdownSequence(sp))
            {
                //Shutdown wasn't cancelled
                SizeChanged -= DynamoView_SizeChanged;
                LocationChanged -= DynamoView_LocationChanged;
            }
            else
            {
                //Shutdown was cancelled
                e.Cancel = true;
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            dynamoViewModel.Model.RequestLayoutUpdate -= vm_RequestLayoutUpdate;
            dynamoViewModel.RequestViewOperation -= DynamoViewModelRequestViewOperation;

            //PACKAGE MANAGER
            dynamoViewModel.RequestPackagePublishDialog -= DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestManagePackagesDialog -= DynamoViewModelRequestShowInstalledPackages;
            dynamoViewModel.RequestPackageManagerSearchDialog -= DynamoViewModelRequestShowPackageManagerSearch;

            //FUNCTION NAME PROMPT
            dynamoViewModel.Model.RequestsFunctionNamePrompt -= DynamoViewModelRequestsFunctionNamePrompt;

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
        }

        // the key press event is being intercepted before it can get to
        // the active workspace. This code simply grabs the key presses and
        // passes it to thecurrent workspace
        void DynamoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                dynamoViewModel.WatchEscapeIsDown = true;
        }

        void DynamoView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                dynamoViewModel.WatchEscapeIsDown = false;
                dynamoViewModel.EscapeCommand.Execute(null);
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

        void _resizeTimer_Tick(object sender, EventArgs e)
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
    }
}

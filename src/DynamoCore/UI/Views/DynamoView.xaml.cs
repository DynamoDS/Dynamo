using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Prompts;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Views;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using String = System.String;
using System.Collections.ObjectModel;
using Dynamo.UI.Commands;
using System.Windows.Data;
using Dynamo.UI.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Core;
using Dynamo.Services;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoForm.xaml
    /// </summary>
    public partial class DynamoView : Window
    {
        public const int CANVAS_OFFSET_Y = 0;
        public const int CANVAS_OFFSET_X = 0;

        private Point dragOffset;
#pragma warning disable 649
        private dynNodeView draggedNode;
#pragma warning restore 649
        private DynamoViewModel _vm;
        private Stopwatch _timer;

        private int tabSlidingWindowStart, tabSlidingWindowEnd;

        public bool ConsoleShowing
        {
            get { return LogScroller.Height > 0; }
        }

        public static Application MakeSandboxAndRun(string commandFilePath)
        {
            var controller = DynamoController.MakeSandbox(commandFilePath);
            var app = new Application();

            //create the view
            var ui = new DynamoView();
            ui.DataContext = controller.DynamoViewModel;
            controller.UIDispatcher = ui.Dispatcher;

            app.Run(ui);

            return app;
        }

        public DynamoView()
        {
            tabSlidingWindowStart = tabSlidingWindowEnd = 0;            

            _timer = new Stopwatch();
            _timer.Start();

            InitializeComponent();

            //LibraryManagerMenu.Visibility = System.Windows.Visibility.Collapsed;

            this.Loaded += dynBench_Activated;

            //setup InfoBubble for library items tooltip
            InfoBubbleView InfoBubble = new InfoBubbleView { DataContext = dynSettings.Controller.InfoBubbleViewModel };
            InfoBubbleGrid.Children.Add(InfoBubble);
        }

        void InitializeShortcutBar()
        {
            ShortcutToolbar shortcutBar = new ShortcutToolbar();
            shortcutBar.Name = "ShortcutToolbar";

            ShortcutBarItem newScriptButton = new ShortcutBarItem();
            newScriptButton.ShortcutToolTip = "New [Ctrl + N]";
            newScriptButton.ShortcutCommand = _vm.NewHomeWorkspaceCommand;
            newScriptButton.ShortcutCommandParameter = null;
            newScriptButton.ImgNormalSource = "/DynamoCore;component/UI/Images/new_normal.png";
            newScriptButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/new_disabled.png";
            newScriptButton.ImgHoverSource = "/DynamoCore;component/UI/Images/new_hover.png";

            ShortcutBarItem openScriptButton = new ShortcutBarItem();
            openScriptButton.ShortcutToolTip = "Open [Ctrl + O]";
            openScriptButton.ShortcutCommand = _vm.ShowOpenDialogAndOpenResultCommand;
            openScriptButton.ShortcutCommandParameter = null;
            openScriptButton.ImgNormalSource = "/DynamoCore;component/UI/Images/open_normal.png";
            openScriptButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/open_disabled.png";
            openScriptButton.ImgHoverSource = "/DynamoCore;component/UI/Images/open_hover.png";

            ShortcutBarItem saveButton = new ShortcutBarItem();
            saveButton.ShortcutToolTip = "Save [Ctrl + S]";
            saveButton.ShortcutCommand = _vm.ShowSaveDialogIfNeededAndSaveResultCommand;
            saveButton.ShortcutCommandParameter = null;
            saveButton.ImgNormalSource = "/DynamoCore;component/UI/Images/save_normal.png";
            saveButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/save_disabled.png";
            saveButton.ImgHoverSource = "/DynamoCore;component/UI/Images/save_hover.png";

            ShortcutBarItem screenShotButton = new ShortcutBarItem();
            screenShotButton.ShortcutToolTip = "Export Workspace As Image";
            screenShotButton.ShortcutCommand = _vm.ShowSaveImageDialogAndSaveResultCommand;
            screenShotButton.ShortcutCommandParameter = null;
            screenShotButton.ImgNormalSource = "/DynamoCore;component/UI/Images/screenshot_normal.png";
            screenShotButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/screenshot_disabled.png";
            screenShotButton.ImgHoverSource = "/DynamoCore;component/UI/Images/screenshot_hover.png";

            ShortcutBarItem undoButton = new ShortcutBarItem();
            undoButton.ShortcutToolTip = "Undo [Ctrl + Z]";
            undoButton.ShortcutCommand = _vm.UndoCommand;
            undoButton.ShortcutCommandParameter = null;
            undoButton.ImgNormalSource = "/DynamoCore;component/UI/Images/undo_normal.png";
            undoButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/undo_disabled.png";
            undoButton.ImgHoverSource = "/DynamoCore;component/UI/Images/undo_hover.png";

            ShortcutBarItem redoButton = new ShortcutBarItem();
            redoButton.ShortcutToolTip = "Redo [Ctrl + Y]";
            redoButton.ShortcutCommand = _vm.RedoCommand;
            redoButton.ShortcutCommandParameter = null;
            redoButton.ImgNormalSource = "/DynamoCore;component/UI/Images/redo_normal.png";
            redoButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/redo_disabled.png";
            redoButton.ImgHoverSource = "/DynamoCore;component/UI/Images/redo_hover.png";

            /*
            ShortcutBarItem updateButton = new ShortcutBarItem();
            //redoButton.ShortcutToolTip = "Update [Ctrl + ]";
            updateButton.ShortcutCommand = _vm.CheckForUpdateCommand;
            updateButton.ShortcutCommandParameter = null;
            updateButton.ImgNormalSource = "/DynamoCore;component/UI/Images/Update/update_static.png";
            updateButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/Update/update_static.png";
            updateButton.ImgHoverSource = "/DynamoCore;component/UI/Images/Update/update_static.png";
            */

            // PLACEHOLDER FOR FUTURE SHORTCUTS
            //ShortcutBarItem runButton = new ShortcutBarItem();
            //runButton.ShortcutToolTip = "Run [Ctrl + R]";
            ////runButton.ShortcutCommand = viewModel.RunExpressionCommand; // Function implementation in progress
            //runButton.ShortcutCommandParameter = null;
            //runButton.ImgNormalSource = "/DynamoCore;component/UI/Images/run_normal.png";
            //runButton.ImgDisabledSource = "/DynamoCore;component/UI/Images/run_disabled.png";
            //runButton.ImgHoverSource = "/DynamoCore;component/UI/Images/run_hover.png";

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

        void vm_RequestLayoutUpdate(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(UpdateLayout), DispatcherPriority.Render, null);
        }

        private void dynBench_Activated(object sender, EventArgs e)
        {
            // If first run, Collect Info Prompt will appear
            UsageReportingManager.Instance.CheckIsFirstRun();

            this.WorkspaceTabs.SelectedIndex = 0;
            _vm = (DataContext as DynamoViewModel);
            _vm.Model.RequestLayoutUpdate += vm_RequestLayoutUpdate;
            _vm.PostUiActivationCommand.Execute(null);

            _timer.Stop();
            DynamoLogger.Instance.Log(String.Format("{0} elapsed for loading Dynamo main window.",
                                                                     _timer.Elapsed));
            InitializeShortcutBar();
            LoadSamplesMenu();

            #region Search initialization

            var search = new SearchView { DataContext = dynSettings.Controller.SearchViewModel };
            sidebarGrid.Children.Add(search);
            dynSettings.Controller.SearchViewModel.Visible = true;

            #endregion

            //PACKAGE MANAGER
            _vm.RequestPackagePublishDialog += _vm_RequestRequestPackageManagerPublish;
            _vm.RequestManagePackagesDialog += _vm_RequestShowInstalledPackages;
            _vm.RequestPackageManagerSearchDialog += _vm_RequestShowPackageManagerSearch;

            //FUNCTION NAME PROMPT
            _vm.Model.RequestsFunctionNamePrompt += _vm_RequestsFunctionNamePrompt;

            _vm.RequestClose += _vm_RequestClose;
            _vm.RequestSaveImage += _vm_RequestSaveImage;
            _vm.SidebarClosed += _vm_SidebarClosed;

            dynSettings.Controller.RequestsCrashPrompt += Controller_RequestsCrashPrompt;

            DynamoSelection.Instance.Selection.CollectionChanged += Selection_CollectionChanged;

            _vm.RequestUserSaveWorkflow += _vm_RequestUserSaveWorkflow;

            dynSettings.Controller.ClipBoard.CollectionChanged += ClipBoard_CollectionChanged;

            //ABOUT WINDOW
            _vm.RequestAboutWindow += _vm_RequestAboutWindow;

            //ABOUT WINDOW
            _vm.RequestAboutWindow += _vm_RequestAboutWindow;

            // Kick start the automation run, if possible.
            _vm.BeginCommandPlayback(this);
        }

        private UI.Views.AboutWindow _aboutWindow;
        void _vm_RequestAboutWindow(DynamoViewModel model)
        {
            if (_aboutWindow == null)
            {
                _aboutWindow = new AboutWindow(DynamoLogger.Instance, model);
                _aboutWindow.Closed += (sender, args) => _aboutWindow = null;
                _aboutWindow.Show();

                if (_aboutWindow.IsLoaded && this.IsLoaded) _aboutWindow.Owner = this;
            }

            _aboutWindow.Focus();
        }

        private PackageManagerPublishView _pubPkgView;
        void _vm_RequestRequestPackageManagerPublish(PublishPackageViewModel model)
        {
            if (_pubPkgView == null)
            {
                _pubPkgView = new PackageManagerPublishView(model);
                _pubPkgView.Closed += (sender, args) => _pubPkgView = null;
                _pubPkgView.Show();

                if (_pubPkgView.IsLoaded && this.IsLoaded) _pubPkgView.Owner = this;
            }

            _pubPkgView.Focus();
        }

        private PackageManagerSearchView _searchPkgsView;
        private PackageManagerSearchViewModel _pkgSearchVM;
        void _vm_RequestShowPackageManagerSearch(object s, EventArgs e)
        {
            if (_pkgSearchVM == null)
            {
                _pkgSearchVM = new PackageManagerSearchViewModel(dynSettings.PackageManagerClient);
            }

            if (_searchPkgsView == null)
            {
                _searchPkgsView = new PackageManagerSearchView(_pkgSearchVM);
                _searchPkgsView.Closed += (sender, args) => _searchPkgsView = null;
                _searchPkgsView.Show();

                if (_searchPkgsView.IsLoaded && this.IsLoaded) _searchPkgsView.Owner = this;
            }
            
            _searchPkgsView.Focus();
            _pkgSearchVM.RefreshAndSearchAsync();
        }

        private InstalledPackagesView _installedPkgsView;
        void _vm_RequestShowInstalledPackages(object s, EventArgs e)
        {
            if (_installedPkgsView == null)
            {
                _installedPkgsView = new InstalledPackagesView();
                _installedPkgsView.Closed += (sender, args) => _installedPkgsView = null;
                _installedPkgsView.Show();

                if (_installedPkgsView.IsLoaded && this.IsLoaded) _installedPkgsView.Owner = this;
            }
            _installedPkgsView.Focus();
        }

        void ClipBoard_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _vm.CopyCommand.RaiseCanExecuteChanged();
            _vm.PasteCommand.RaiseCanExecuteChanged();
        }

        void _vm_RequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            var dialogText = "";
            if (e.Workspace is CustomNodeWorkspaceModel)
            {
                dialogText = "You have unsaved changes to custom node workspace: \"" + e.Workspace.Name +
                             "\"\n\n Would you like to save your changes?";
            }
            else // homeworkspace
            {
                if (string.IsNullOrEmpty(e.Workspace.FileName))
                {
                    dialogText = "You have unsaved changes to the Home workspace." +
                                 "\n\n Would you like to save your changes?";
                }
                else
                {
                    dialogText = "You have unsaved changes to " + Path.GetFileName(e.Workspace.FileName) +
                    "\n\n Would you like to save your changes?";
                }
            }

            var buttons = e.AllowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show(dialogText, "Confirmation", buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _vm.ShowSaveDialogIfNeededAndSave(e.Workspace);
                e.Success = true;
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
            _vm.CopyCommand.RaiseCanExecuteChanged();
            _vm.PasteCommand.RaiseCanExecuteChanged();
        }
        
        void Controller_RequestsCrashPrompt(object sender, CrashPromptArgs args)
        {
            var prompt = new CrashPrompt(args);
            prompt.ShowDialog();
        }

        //void PackageManagerClient_RequestSetLoginState(object sender, LoginStateEventArgs e)
        //{
        //    PackageManagerLoginState.Text = e.Text;
        //    PackageManagerLoginButton.IsEnabled = e.Enabled;
        //}

        void _vm_RequestSaveImage(object sender, ImageSaveEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Path))
            {
                //var bench = dynSettings.Bench;

                //if (bench == null)
                //{
                //    DynamoLogger.Instance.Log("Cannot export bench as image without UI.  No image wil be exported.");
                //    return;
                //}

                var control = WPF.FindChild<DragCanvas>(this, null);

                double width = 1;
                double height = 1;

                // connectors are most often within the bounding box of the nodes and notes

                foreach (NodeModel n in dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                foreach (NoteModel n in dynSettings.Controller.DynamoModel.CurrentWorkspace.Notes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                var rtb = new RenderTargetBitmap(Math.Max(1, (int)width),
                                                  Math.Max(1, (int)height),
                                                  96,
                                                  96,
                                                  System.Windows.Media.PixelFormats.Default);

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
                    DynamoLogger.Instance.Log("Failed to save the Workspace an image.");
                }
            }
        }

        void _vm_RequestClose(object sender, EventArgs e)
        {
            Close();
        }

        void _vm_SidebarClosed(object sender, EventArgs e)
        {
            LibraryClicked(sender, e);
        }

        /// <summary>
        /// Handles the request for the presentation of the function name prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _vm_RequestsFunctionNamePrompt(object sender, FunctionNamePromptEventArgs e)
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
                var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories)
                {
                    categoryBox = { Text = e.Category },
                    DescriptionInput = { Text = e.Description },
                    nameView = { Text = e.Name },
                    nameBox = { Text = e.Name }
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
                    error = "You must supply a name.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (e.Name != dialog.Text && dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(dialog.Text))
                {
                    error = "A built-in node with the given name already exists.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (dialog.Category.Equals(""))
                {
                    error = "You must enter a new category or choose one from the existing categories.";
                    MessageBox.Show(error, "Custom Node Property Error", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
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
            if (_vm.exitInvoked)
                return;

            var res = _vm.AskUserToSaveWorkspacesOrCancel();
            if (!res)
            {
                e.Cancel = true;
                return;
            }

            if (!dynSettings.Controller.Testing)
            {
                dynSettings.Controller.ShutDown(false);
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {

        }

        private void OverlayCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_vm.IsUILocked)
                return;

            dynNodeView el = draggedNode;

            Point pos = e.GetPosition(overlayCanvas);

            Canvas.SetLeft(el, pos.X - dragOffset.X);
            Canvas.SetTop(el, pos.Y - dragOffset.Y);
        }

        // the key press event is being intercepted before it can get to
        // the active workspace. This code simply grabs the key presses and
        // passes it to thecurrent workspace
        void DynamoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            WorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            _vm.WatchEscapeIsDown = true;
        }

        void DynamoView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            int workspace_index = _vm.CurrentWorkspaceIndex;

            WorkspaceViewModel view_model = _vm.Workspaces[workspace_index];

            _vm.WatchEscapeIsDown = false;
            _vm.EscapeCommand.Execute(null);
        }

        private void WorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm != null)
            {
                int workspace_index = _vm.CurrentWorkspaceIndex;
                var workspace_vm = _vm.Workspaces[workspace_index];
                workspace_vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(workspace_vm.Model.X, workspace_vm.Model.Y)));
                workspace_vm.OnZoomChanged(this, new ZoomEventArgs(workspace_vm.Zoom));

                ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogScroller.ScrollToBottom();
        }

        /// <summary>
        ///     Setup the "Samples" sub-menu with contents of samples directory.
        /// </summary>
        /// <param name="bench">The bench where the UI will be loaded</param>
        private void LoadSamplesMenu()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string samplesPath = Path.Combine(directory, "samples");

            if (Directory.Exists(samplesPath))
            {
                string[] dirPaths = Directory.GetDirectories(samplesPath);
                string[] filePaths = Directory.GetFiles(samplesPath, "*.dyn");

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
                            }
                        }
                        SamplesMenu.Items.Add(dirItem);
                    }
                    return;
                }
            }
            //this.fileMenu.Items.Remove(this.samplesMenu);
        }

        /// <summary>
        ///     Callback for opening a sample.
        /// </summary>
        private void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            var path = (string)((MenuItem)sender).Tag;

            if (_vm.IsUILocked)
                _vm.QueueLoad(path);
            else
            {
                if (dynSettings.Controller.DynamoModel.CanGoHome(null))
                    dynSettings.Controller.DynamoModel.Home(null);

                _vm.OpenCommand.Execute(path);
            }
        }

        private void TabControlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be = ((MenuItem)sender).GetBindingExpression(MenuItem.HeaderProperty);
            WorkspaceViewModel wsvm = (WorkspaceViewModel)be.DataItem;
            WorkspaceTabs.SelectedIndex = _vm.Workspaces.IndexOf(wsvm);
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
            var imageUri = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/expand_hover.png");

            BitmapImage hover = new BitmapImage(imageUri);
            // hover.Rotation = Rotation.Rotate180;

            collapseIcon.Source = hover;
        }

		private void Button_Click(object sender, EventArgs e)
        {
            SearchView sv = (SearchView)this.sidebarGrid.Children[0];
            if (sv.Visibility == Visibility.Collapsed)
            {
                //this.sidebarGrid.Width = restoreWidth;
                sv.Width = double.NaN;
                sv.HorizontalAlignment = HorizontalAlignment.Stretch;
                sv.Height = double.NaN;
                sv.VerticalAlignment = VerticalAlignment.Stretch;

                this.mainGrid.ColumnDefinitions[0].Width = new System.Windows.GridLength(restoreWidth);
                this.verticalSplitter.Visibility = Visibility.Visible;
                sv.Visibility = Visibility.Visible;
                this.sidebarGrid.Visibility = Visibility.Visible;
                this.collapsedSidebar.Visibility = Visibility.Collapsed;
            }
            //SearchView sv = (SearchView)this.sidebarGrid.Children[0];
            //sv.Width = double.NaN;
            //this.sidebarGrid.Width = 250;
            //this.collapsedSidebar.Visibility = Visibility.Collapsed;
        }

		private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            TextBlock tb = (TextBlock)(g.Children[1]);
            var bc = new BrushConverter();
            tb.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");
            Image collapseIcon = (Image)g.Children[0];

            // Change the collapse icon and rotate
            var imageUri = new Uri(@"pack://application:,,,/DynamoCore;component/UI/Images/expand_normal.png");
            BitmapImage hover = new BitmapImage(imageUri);

            collapseIcon.Source = hover;
        }

        private double restoreWidth = 0;

        private void LibraryClicked(object sender, EventArgs e)
        {
            // this.sidebarGrid.Visibility = Visibility.Collapsed;
            restoreWidth = this.sidebarGrid.ActualWidth;

            // this.sidebarGrid.Width = 0;
            this.mainGrid.ColumnDefinitions[0].Width = new System.Windows.GridLength(0.0);
            this.verticalSplitter.Visibility = System.Windows.Visibility.Collapsed;
            this.sidebarGrid.Visibility = System.Windows.Visibility.Collapsed;
            
            this.horizontalSplitter.Width = double.NaN;
            SearchView sv = (SearchView)this.sidebarGrid.Children[0];
            sv.Visibility = Visibility.Collapsed;

            this.sidebarGrid.Visibility = Visibility.Collapsed;
            this.collapsedSidebar.Visibility = Visibility.Visible;
        }

        private void Workspace_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this._vm == null)
                return;
            this._vm.WorkspaceActualSize(border.ActualWidth, border.ActualHeight);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _vm.IsMouseDown = true;
		}

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _vm.IsMouseDown = false;
		}

        private void WorkspaceTabs_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
        }

        private void WorkspaceTabs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);
        }
       
        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            dynSettings.ReturnFocusToSearch();
        }

        private void DynamoView_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (_vm.Model.HomeSpace.HasUnsavedChanges && !_vm.AskUserToSaveWorkspaceOrCancel(_vm.Model.HomeSpace))
                {
                    return;
                }

                if (_vm.OpenCommand.CanExecute(files[0]))
                {
                    _vm.OpenCommand.Execute(files[0]);
                }
                
            }

            e.Handled = true;
        }
    }
}

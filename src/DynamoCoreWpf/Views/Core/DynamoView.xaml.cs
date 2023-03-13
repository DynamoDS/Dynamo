using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph;
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
using Dynamo.Search.SearchElements;
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
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.Views;
using Dynamo.Wpf.Views.Debug;
using Dynamo.Wpf.Views.FileTrust;
using Dynamo.Wpf.Windows;
using HelixToolkit.Wpf.SharpDX;
using Brush = System.Windows.Media.Brush;
using Exception = System.Exception;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Res = Dynamo.Wpf.Properties.Resources;
using ResourceNames = Dynamo.Wpf.Interfaces.ResourceNames;
using Size = System.Windows.Size;
using String = System.String;

namespace Dynamo.Controls
{
    /// <summary>
    ///     Interaction logic for DynamoView.xaml
    /// </summary>
    public partial class DynamoView : Window, IDisposable
    {
        public const string BackgroundPreviewName = "BackgroundPreview";
        private const int navigationInterval = 100;
        // This is used to determine whether ESC key is being held down
        private bool IsEscKeyPressed = false;
        // internal for testing.
        internal readonly NodeViewCustomizationLibrary nodeViewCustomizationLibrary;
        private double restoreWidth = 0;
        private DynamoViewModel dynamoViewModel;
        private readonly Stopwatch _timer;
        private StartPageViewModel startPage;
        private int tabSlidingWindowStart, tabSlidingWindowEnd;
        private readonly LoginService loginService;
        private ShortcutToolbar shortcutBar;
        private PreferencesView preferencesWindow;
        private bool loaded = false;
        // This is to identify whether the PerformShutdownSequenceOnViewModel() method has been
        // called on the view model and the process is not cancelled
        private bool isPSSCalledOnViewModelNoCancel = false;
        private readonly DispatcherTimer _workspaceResizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500), IsEnabled = false };
        private ViewLoadedParams sharedViewExtensionLoadedParams;
        /// <summary>
        /// This event is raised on the dynamo view when an extension tab is closed.
        /// </summary>
        internal static event Action<String> CloseExtension;
        internal ObservableCollection<TabItem> ExtensionTabItems { set; get; } = new ObservableCollection<TabItem>();
        /// <summary>
        /// Extensions currently displayed as windows.
        /// Made internal for testing purposes only.
        /// </summary>
        internal Dictionary<string, ExtensionWindow> ExtensionWindows { get; set; } = new Dictionary<string, ExtensionWindow>();
        internal ViewExtensionManager viewExtensionManager;
        internal Watch3DView BackgroundPreview { get; private set; }

        private FileTrustWarning fileTrustWarningPopup = null;

        internal ShortcutToolbar ShortcutBar { get { return shortcutBar; } }

        internal PreferencesView PreferencesWindow {
            get { return preferencesWindow; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel">Dynamo view model</param>
        public DynamoView(DynamoViewModel dynamoViewModel)
        {
            // The user's choice to enable hardware acceleration is now saved in
            // the Dynamo preferences. It is set to true by default. 
            // When the view is constructed, we enable or disable hardware acceleration based on that preference. 
            // This preference is not exposed in the UI and can be used to debug hardware issues only
            // by modifying the preferences xml.
            RenderOptions.ProcessRenderMode = dynamoViewModel.Model.PreferenceSettings.UseHardwareAcceleration ?
                RenderMode.Default : RenderMode.SoftwareOnly;

            this.dynamoViewModel = dynamoViewModel;
            this.dynamoViewModel.UIDispatcher = Dispatcher;
            nodeViewCustomizationLibrary = new NodeViewCustomizationLibrary(this.dynamoViewModel.Model.Logger);

            DataContext = dynamoViewModel;
            Title = dynamoViewModel.BrandingResourceProvider.GetString(ResourceNames.MainWindow.Title);

            tabSlidingWindowStart = tabSlidingWindowEnd = 0;

            //Initialize the ViewExtensionManager with the CommonDataDirectory so that view extensions found here are checked first for dll's with signed certificates
            viewExtensionManager = new ViewExtensionManager(dynamoViewModel.Model.ExtensionManager, new[] { dynamoViewModel.Model.PathManager.CommonDataDirectory });

            _timer = new Stopwatch();
            _timer.Start();

            InitializeComponent();

            Loaded += DynamoView_Loaded;
            Unloaded += DynamoView_Unloaded;

            SizeChanged += DynamoView_SizeChanged;
            LocationChanged += DynamoView_LocationChanged;

            // Apply appropriate expand/collapse library button state depending on initial width
            UpdateLibraryCollapseIcon();

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

            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Res.SplashScreenViewExtensions, 100));
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
                    if (ext is ILogSource logSource)
                    {
                        logSource.MessageLogged += Log;
                    }

                    if (ext is INotificationSource notificationSource)
                    {
                        notificationSource.NotificationLogged += LogNotification;
                    }

                    ext.Startup(startupParams);
                    // if we are starting ViewExtension (A) which is a source of other extensions (like packageManager)
                    // then we can start the ViewExtension(s) (B) that it requested be loaded.
                    if (ext is IViewExtensionSource)
                    {
                        foreach (var loadedExtension in ((ext as IViewExtensionSource).RequestedExtensions))
                        {
                            (loadedExtension).Startup(startupParams);
                        }
                    }
                    viewExtensionManager.Add(ext);

                }
                catch (Exception exc)
                {
                    Log(ext.Name + ": " + exc.Message);
                }
            }

            // when an extension is added if dynamoView is loaded, call loaded on
            // that extension (this alerts late loaded extensions).
            this.viewExtensionManager.ExtensionAdded += (extension) =>
             {
                 if (this.loaded)
                 {
                     DynamoLoadedViewExtensionHandler(new ViewLoadedParams(this, this.dynamoViewModel),
                         new List<IViewExtension>() { extension });
                 }
             };

            // Add an event handler to check if the collection is modified.   
            ExtensionTabItems.CollectionChanged += this.OnCollectionChanged;

            this.HideOrShowRightSideBar();

            this.dynamoViewModel.RequestPaste += OnRequestPaste;
            this.dynamoViewModel.RequestReturnFocusToView += OnRequestReturnFocusToView;
            this.dynamoViewModel.Model.WorkspaceSaving += OnWorkspaceSaving;
            this.dynamoViewModel.Model.WorkspaceOpened += OnWorkspaceOpened;
            this.dynamoViewModel.RequestEnableShortcutBarItems += DynamoViewModel_RequestEnableShortcutBarItems;
            FocusableGrid.InputBindings.Clear();

            if (fileTrustWarningPopup == null)
            {
                fileTrustWarningPopup = new FileTrustWarning(this);
            }
            if (!DynamoModel.IsTestMode && Application.Current != null)
            {
                Application.Current.MainWindow = this;
            }
        }

        private void DynamoViewModel_RequestEnableShortcutBarItems(bool enable)
        {
            saveThisButton.IsEnabled = enable;
            saveButton.IsEnabled = enable;
            exportMenu.IsEnabled = enable;
            
            shortcutBar.IsNewButtonEnabled = enable;
            shortcutBar.IsOpenButtonEnabled = enable;
            shortcutBar.IsSaveButtonEnabled = enable;
            shortcutBar.IsLoginMenuEnabled = enable;
            shortcutBar.IsExportMenuEnabled  = enable;
            shortcutBar.IsNotificationCenterEnabled = enable;

            if(dynamoViewModel.ShowStartPage)
            {
                shortcutBar.IsNewButtonEnabled = true;
                shortcutBar.IsOpenButtonEnabled = true;
                shortcutBar.IsLoginMenuEnabled = true;
                shortcutBar.IsNotificationCenterEnabled = true;
            }
        }

        private void OnWorkspaceOpened(WorkspaceModel workspace)
        {
            saveThisButton.IsEnabled = true;
            saveButton.IsEnabled = true;
            exportMenu.IsEnabled = true;

            ShortcutBar.IsSaveButtonEnabled = true;
            shortcutBar.IsExportMenuEnabled = true;

            if (!(workspace is HomeWorkspaceModel hws))
            return;
            
            foreach (var extension in viewExtensionManager.StorageAccessViewExtensions)
            {
                DynamoModel.RaiseIExtensionStorageAccessWorkspaceOpened(hws, extension, dynamoViewModel.Model.Logger);
            }            
        }

        private void OnWorkspaceSaving(WorkspaceModel workspace, Graph.SaveContext saveContext)
        {
            if (!(workspace is HomeWorkspaceModel hws))
                return;

            foreach (var extension in viewExtensionManager.StorageAccessViewExtensions)
            {
                DynamoModel.RaiseIExtensionStorageAccessWorkspaceSaving(hws, extension, saveContext, dynamoViewModel.Model.Logger);
            }
        }

        /// <summary>
        /// Adds an extension control or if it already exists it makes sure it is focused.
        /// The control may be added as a window or a tab in the extension bar depending on settings.
        /// </summary>
        /// <param name="viewExtension">View extension adding the content</param>
        /// <param name="content">Control being added</param>
        /// <returns>True if the control was added, false if it already existed</returns>
        internal bool AddOrFocusExtensionControl(IViewExtension viewExtension, UIElement content)
        {
            var window = ExtensionWindows.ContainsKey(viewExtension.Name) ? ExtensionWindows[viewExtension.Name] : null;
            var tab = FindExtensionTab(viewExtension);
            var addExtensionControl = window == null && tab == null;

            if (addExtensionControl)
            {
                var settings = this.dynamoViewModel.PreferenceSettings.ViewExtensionSettings.Find(s => s.UniqueId == viewExtension.UniqueId);
                // Create default settings if they do not currently exist
                if (settings == null)
                {
                    settings = new ViewExtensionSettings()
                    {
                        Name = viewExtension.Name,
                        UniqueId = viewExtension.UniqueId,
                        DisplayMode = ViewExtensionDisplayMode.DockRight
                    };
                    this.dynamoViewModel.PreferenceSettings.ViewExtensionSettings.Add(settings);
                }

                if (settings.DisplayMode == ViewExtensionDisplayMode.FloatingWindow)
                {
                    window = AddExtensionWindow(viewExtension, content, settings.WindowSettings);
                }
                else
                {
                    tab = AddExtensionTab(viewExtension, content);
                }
            }
            else
            {
                // Set focus on the existing control
                if (window != null)
                {
                    window.Focus();
                }
                else if (tab != null)
                {
                    // Make sure the extension bar is visible
                    if (ExtensionsCollapsed)
                    {
                        ToggleExtensionBarCollapseStatus();
                    }

                    tabDynamic.SelectedItem = tab;
                }
            }

            return addExtensionControl;
        }

        private ExtensionWindow AddExtensionWindow(IViewExtension viewExtension, UIElement content, WindowSettings windowSettings)
        {
            ExtensionWindow window;
            if (windowSettings == null)
            {
                window = new ExtensionWindow();
                window.Owner = this;
            }
            else
            {
                var windowRect = new ModelessChildWindow.WindowRect()
                {
                    Left = windowSettings.Left,
                    Top = windowSettings.Top,
                    Width = windowSettings.Width,
                    Height = windowSettings.Height
                };
                window = new ExtensionWindow(this, ref windowRect);
                if (windowSettings.Status == WindowStatus.Maximized)
                {
                    // Rather than setting the WindowState here, this is delayed to the Loaded event.
                    // This helps overcome a bug which makes the window appear always on the primary screen.
                    window.ShouldMaximize = true;
                }
            }
            
            // Setting the content of the undocked window
            // Icon is passed from DynamoView (respecting Host integrator icon)
            SetApplicationIcon();
            window.Icon = this.Icon;
            if (content is Window container)
            {
                content = container.Content as UIElement;
                container.Owner = this;
            }
            window.ExtensionContent.Content = content;
            window.Title = viewExtension.Name;
            window.Tag = viewExtension;
            window.Uid = viewExtension.UniqueId;
            window.Closing += ExtensionWindow_Closing;
            window.Closed += ExtensionWindow_Closed;

            window.Show();

            ExtensionWindows.Add(viewExtension.Name, window);

            return window;
        }

        private void ExtensionWindow_Closing(object sender, CancelEventArgs e)
        {
            var window = sender as ExtensionWindow;
            SaveExtensionWindowSettings(window);
        }

        private void SaveExtensionWindowSettings(ExtensionWindow window)
        {
            var extension = window.Tag as IViewExtension;
            var settings = this.dynamoViewModel.Model.PreferenceSettings.ViewExtensionSettings.Find(ext => ext.UniqueId == extension.UniqueId);
            if (settings != null)
            {
                if (settings.WindowSettings == null)
                {
                    settings.WindowSettings = new WindowSettings();
                }
                settings.WindowSettings.Status = window.WindowState == WindowState.Maximized ? WindowStatus.Maximized : WindowStatus.Normal;
                settings.WindowSettings.Left = (int)window.SavedWindowRect.Left;
                settings.WindowSettings.Top = (int)window.SavedWindowRect.Top;
                settings.WindowSettings.Width = (int)window.SavedWindowRect.Width;
                settings.WindowSettings.Height = (int)window.SavedWindowRect.Height;
            }
        }

        private TabItem AddExtensionTab(IViewExtension viewExtension, UIElement content)
        {
            tabDynamic.DataContext = null;

            // creates a new tab item
            var tab = new TabItem();
            tab.Header = viewExtension.Name;
            tab.Tag = viewExtension;
            tab.Uid = viewExtension.UniqueId;
            tab.HeaderTemplate = tabDynamic.FindResource("TabHeader") as DataTemplate;

            // setting the extension UI to the current tab content 
            // based on whether it is a UserControl element or window element. 
            if (content is Window container)
            {
                content = container.Content as UIElement;
                // Make sure the extension window closes with Dynamo
                container.Owner = this;
            }
            tab.Content = content;

            //Insert the tab at the end
            ExtensionTabItems.Insert(ExtensionTabItems.Count, tab);

            tabDynamic.DataContext = ExtensionTabItems;
            tabDynamic.SelectedItem = tab;

            return tab;
        }

        /// <summary>
        /// This method will close an extension control, whether it's on the side bar or undocked as a window.
        /// </summary>
        /// <param name="viewExtension">Extension to be closed</param>
        /// <returns></returns>
        internal void CloseExtensionControl(IViewExtension viewExtension)
        {
            string tabName = viewExtension.Name;
            TabItem tabitem = ExtensionTabItems.OfType<TabItem>().SingleOrDefault(n => n.Header.ToString() == tabName);

            if (viewExtension is ViewExtensionBase viewExtensionBase)
            {
                viewExtensionBase.Closed();
            }

            CloseExtensionTab(tabitem);
            CloseExtensionWindow(tabName);
        }
 
        /// <summary>
        /// Event handler for the CloseButton.
        /// This method triggers the close operation on the selected tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CloseExtensionTab(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).DataContext.ToString();
            TabItem tabitem = ExtensionTabItems.OfType<TabItem>().SingleOrDefault(n => n.Header.ToString() == tabName);

            if (tabitem.Tag is ViewExtensionBase viewExtensionBase)
            {
                viewExtensionBase.Closed();
            }

            CloseExtensionTab(tabitem);
        }

        /// <summary>
        /// Close extension tab by extension tab item
        /// </summary>
        /// <param name="tabitem">target tab item</param>
        private void CloseExtensionTab(TabItem tabitem)
        {
            TabItem tabToBeRemoved = tabitem;

            // get the selected tab
            TabItem selectedTab = tabDynamic.SelectedItem as TabItem;

            if (tabToBeRemoved != null && ExtensionTabItems.Count > 0)
            {
                // clear tab control binding and bind to the new tab-list. 
                tabDynamic.DataContext = null;
                ExtensionTabItems.Remove(tabToBeRemoved);
                // Disconnect content from tab to allow it to be moved.
                tabToBeRemoved.Content = null;
                tabDynamic.DataContext = ExtensionTabItems;

                // Highlight previously selected tab. if that is removed then Highlight the first tab
                if (selectedTab.Equals(tabToBeRemoved))
                {
                    if (ExtensionTabItems.Count > 0)
                    {
                        selectedTab = ExtensionTabItems[0];
                    }
                }
                tabDynamic.SelectedItem = selectedTab;
            }
        }

        private void CloseExtensionWindow(string name)
        {
            if (ExtensionWindows.ContainsKey(name))
            {
                var extension = ExtensionWindows[name];
                extension.Close();
                // Disconnect content to allow it to be moved.
                extension.ExtensionContent.Content = null;
                ExtensionWindows.Remove(name);
            }
        }

        internal void UndockExtensionTab(object sender, RoutedEventArgs e)
        {
            var tabName = (sender as Button).DataContext.ToString();
            UndockExtension(tabName);
            Logging.Analytics.TrackEvent(
               Actions.Undock,
               Categories.ViewExtensionOperations, tabName);
        }

        /// <summary>
        /// Undocks the extension with the given name.
        /// Made internal for testing purposes only.
        /// </summary>
        /// <param name="name">Name of the extension</param>
        internal void UndockExtension(string name)
        {
            var tabItem = ExtensionTabItems.OfType<TabItem>().SingleOrDefault(tab => tab.Header.ToString() == name);
            var content = tabItem.Content as UIElement;
            CloseExtensionTab(tabItem);
            var extension = tabItem.Tag as IViewExtension;
            var settings = this.dynamoViewModel.PreferenceSettings.ViewExtensionSettings.Find(s => s.UniqueId == extension.UniqueId);
            AddExtensionWindow(extension, content, settings?.WindowSettings);
            if (settings != null)
            {
                settings.DisplayMode = ViewExtensionDisplayMode.FloatingWindow;
            }
        }

        /// <summary>
        /// Sets DynamoView icon to that of the currently running application. This is set for reuse
        /// in custom child windows rather than for the main window itself, which is not customized.
        /// </summary>
        private void SetApplicationIcon()
        {
            if (this.Icon == null && !DynamoModel.IsTestMode)
            {
                var applicationPath = Process.GetCurrentProcess().MainModule.FileName;
                try
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(applicationPath);
                    var bmp = icon.ToBitmap();
                    MemoryStream stream = new MemoryStream();
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    PngBitmapDecoder pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    this.Icon = pngDecoder.Frames[0];
                }
                catch (Exception ex)
                {
                    Log(string.Format(Dynamo.Wpf.Properties.Resources.ErrorLoadingIcon, ex.Message));
                }
            }
        }

        private void ExtensionWindow_Closed(object sender, EventArgs e)
        {
            var window = sender as ExtensionWindow;
            var extName = window.Title;
            var content = window.ExtensionContent.Content as UIElement;
            // Release content from window
            window.ExtensionContent.Content = null;
            ExtensionWindows.Remove(extName);
            var extension = window.Tag as IViewExtension;
            if (window.DockRequested)
            {
                AddExtensionTab(extension, content);

                var settings = this.dynamoViewModel.PreferenceSettings.ViewExtensionSettings.Find(s => s.UniqueId == extension.UniqueId);
                if (settings != null)
                {
                    settings.DisplayMode = ViewExtensionDisplayMode.DockRight;
                }
                Analytics.TrackEvent(Actions.Dock, Categories.ViewExtensionOperations, extName);
            }
            else
            {
                if (extension is ViewExtensionBase viewExtensionBase)
                {
                    viewExtensionBase.Closed();
                }
            }
        }

        // This event is triggered when the tabitems list is changed and will show/hide the right side bar accordingly.
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HideOrShowRightSideBar();
        }

        private TabItem FindExtensionTab(IViewExtension viewExtension)
        {
            return ExtensionTabItems.FirstOrDefault(tab => ((IViewExtension)tab.Tag).GetType().Equals(viewExtension.GetType()));
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
            var modelsExcludingConnectorPins = locatableModels.Where(model => !(model is ConnectorPinModel));
            if(modelsExcludingConnectorPins is null || modelsExcludingConnectorPins.Count()<1) { return; }

            var modelBounds = modelsExcludingConnectorPins.Select(lm =>
                new Rect { X = lm.X, Y = lm.Y, Height = lm.Height, Width = lm.Width });

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
            var orderedItems = modelsExcludingConnectorPins.OrderBy(item => item.CenterX + item.CenterY);

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

            var x = shiftX + modelsExcludingConnectorPins.Min(m => m.X);
            var y = shiftY + modelsExcludingConnectorPins.Min(m => m.Y);

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

            //When the Dynamo window is moved to another place we need to update the Steps location
            if(dynamoViewModel.MainGuideManager != null)
                dynamoViewModel.MainGuideManager.UpdateGuideStepsLocation();

            if (fileTrustWarningPopup != null && fileTrustWarningPopup.IsOpen)
            {
                fileTrustWarningPopup.UpdatePopupLocation();
            }

            UpdateGeometryScalingPopupLocation();
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dynamoViewModel.Model.PreferenceSettings.WindowW = e.NewSize.Width;
            dynamoViewModel.Model.PreferenceSettings.WindowH = e.NewSize.Height;

            //When the Dynamo window size is changed then we need to update the Steps location
            if (dynamoViewModel.MainGuideManager != null)
            {
                dynamoViewModel.MainGuideManager.UpdateGuideStepsLocation();
            }

            if (fileTrustWarningPopup != null && fileTrustWarningPopup.IsOpen)
            {
                fileTrustWarningPopup.UpdatePopupLocation();
            }

            UpdateGeometryScalingPopupLocation();
        }

        private void UpdateGeometryScalingPopupLocation()
        {
            var workspaceView = this.ChildOfType<WorkspaceView>();
            if (workspaceView != null && workspaceView.GeoScalingPopup != null)
            {
                workspaceView.GeoScalingPopup.UpdatePopupLocation();
            }               
        }

        private void InitializeShortcutBar()
        {
            shortcutBar = new ShortcutToolbar(this.dynamoViewModel) { Name = "ShortcutToolbar" };

            var newScriptButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarNewButtonTooltip,
                ShortcutCommand = dynamoViewModel.NewHomeWorkspaceCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/new_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/new_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/new_normal.png",
                Name = "New"
            };

            var openScriptButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarOpenButtonTooltip,
                ShortcutCommand = dynamoViewModel.ShowOpenDialogAndOpenResultCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/open_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/open_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/open_normal.png",
                Name = "Open"
            };

            var saveButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarSaveButtonTooltip,
                ShortcutCommand = dynamoViewModel.ShowSaveDialogIfNeededAndSaveResultCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/save_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/save_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/save_normal.png",
                Name = "Save"
            };

            var undoButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarUndoButtonTooltip,
                ShortcutCommand = dynamoViewModel.UndoCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/undo_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/undo_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/undo_normal.png",
                Name = "Undo"
            };

            var redoButton = new ShortcutBarItem
            {
                ShortcutToolTip = Wpf.Properties.Resources.DynamoViewToolbarRedoButtonTooltip,
                ShortcutCommand = dynamoViewModel.RedoCommand,
                ShortcutCommandParameter = null,
                ImgNormalSource = "/DynamoCoreWpf;component/UI/Images/redo_normal.png",
                ImgDisabledSource = "/DynamoCoreWpf;component/UI/Images/redo_disabled.png",
                ImgHoverSource = "/DynamoCoreWpf;component/UI/Images/redo_normal.png",
                Name = "Redo"
            };

            shortcutBar.ShortcutBarItems.Add(newScriptButton);
            shortcutBar.ShortcutBarItems.Add(openScriptButton);
            shortcutBar.ShortcutBarItems.Add(saveButton);
            shortcutBar.ShortcutBarItems.Add(undoButton);
            shortcutBar.ShortcutBarItems.Add(redoButton);
            
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
                    BackgroundPreview.View.AddZoomForce(-0.5);
                    break;

                case ViewOperationEventArgs.Operation.ZoomOut:
                    BackgroundPreview.View.AddZoomForce(0.5);
                    break;
            }
        }

        private void DynamoLoadedViewExtensionHandler(ViewLoadedParams loadedParams, IEnumerable<IViewExtension> extensions)
        {
            foreach (var ext in extensions)
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
        }

        private void DynamoView_Loaded(object sender, EventArgs e)
        {
            // Do an initial load of the cursor collection
            CursorLibrary.GetCursor(CursorSet.ArcSelect);

            //Backing up IsFirstRun to determine whether to do certain action
            var isFirstRun = dynamoViewModel.Model.PreferenceSettings.IsFirstRun;

            // If first run, Collect Info Prompt will appear
            UsageReportingManager.Instance.CheckIsFirstRun(this, dynamoViewModel.BrandingResourceProvider);
            

            WorkspaceTabs.SelectedIndex = 0;
            dynamoViewModel = (DataContext as DynamoViewModel);
            dynamoViewModel.Model.RequestLayoutUpdate += vm_RequestLayoutUpdate;
            dynamoViewModel.RequestViewOperation += DynamoViewModelRequestViewOperation;
            dynamoViewModel.PostUiActivationCommand.Execute(null);

            // Initialize Guide Manager as a member on Dynamo ViewModel so other than guided tour,
            // other part of application can also leverage it.
            dynamoViewModel.MainGuideManager = new GuidesManager(_this, dynamoViewModel);
            GuideFlowEvents.GuidedTourStart += GuideFlowEvents_GuidedTourStart;
            _timer.Stop();
            dynamoViewModel.Model.Logger.Log(String.Format(Wpf.Properties.Resources.MessageLoadingTime,
                                                                     _timer.Elapsed, dynamoViewModel.BrandingResourceProvider.ProductName));
            InitializeShortcutBar();
            InitializeStartPage(isFirstRun);

#if !__NO_SAMPLES_MENU
            LoadSamplesMenu();
#endif

            #region Package manager

            dynamoViewModel.RequestPackagePublishDialog += DynamoViewModelRequestRequestPackageManagerPublish;
            dynamoViewModel.RequestPackageManagerSearchDialog += DynamoViewModelRequestShowPackageManagerSearch;

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

            LoadNodeViewCustomizations();
            SubscribeNodeViewCustomizationEvents();

            // Kick start the automation run, if possible.
            dynamoViewModel.BeginCommandPlayback(this);

            sharedViewExtensionLoadedParams = new ViewLoadedParams(this, dynamoViewModel);
            this.DynamoLoadedViewExtensionHandler(sharedViewExtensionLoadedParams, viewExtensionManager.ViewExtensions);

            BackgroundPreview = new Watch3DView { Name = BackgroundPreviewName };
            background_grid.Children.Add(BackgroundPreview);
            BackgroundPreview.DataContext = dynamoViewModel.BackgroundPreviewViewModel;
            BackgroundPreview.Margin = new System.Windows.Thickness(0, 20, 0, 0);
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
                this.Deactivated += (s, args) => { HidePopupWhenWindowDeactivated(null); };
            }
            loaded = true;


            //The following code illustrates use of FeatureFlagsManager.
            //safe to remove.
            if (DynamoModel.FeatureFlags != null)
            {
                CheckTestFlags();
            }
            //if feature flags is not yet initialized, subscribe to the event and wait.
            else
            {
                DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved += CheckTestFlags;
            }            
        }

        private void GuideFlowEvents_GuidedTourStart(GuidedTourStateEventArgs args)
        {
            if(sidebarGrid.Visibility != Visibility.Visible || sidebarGrid.ActualWidth < 2)
            {
                OnCollapsedLeftSidebarClick(null, null);
            }
        }

        /// <summary>
        /// Close Popup when the Dynamo window is not in the foreground.
        /// </summary>

        private void HidePopupWhenWindowDeactivated(object obj)
        {
            var workspace = this.ChildOfType<WorkspaceView>();
            if (workspace != null)
                workspace.HideAllPopUp(obj);
        }
        /// <summary>
        /// check some test flags from launch darkly.
        /// code is safe to remove at any time.
        /// </summary>
        private void CheckTestFlags()
        {
            if (!DynamoModel.IsTestMode)
            {
                //feature flag test.
                if (DynamoModel.FeatureFlags?.CheckFeatureFlag<bool>("EasterEggIcon1", false) == true)
                {
                    dynamoViewModel.Model.Logger.Log("EasterEggIcon1 is true from view");
                }
                else
                {
                    dynamoViewModel.Model.Logger.Log("EasterEggIcon1 is false from view");
                }

                if (DynamoModel.FeatureFlags?.CheckFeatureFlag<string>("EasterEggMessage1", "NA") is string ffs && ffs != "NA")
                {
                    dynamoViewModel.Model.Logger.Log("EasterEggMessage1 is enabled from view");
                    MessageBoxService.Show(this, ffs, "EasterEggMessage1", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
                else
                {
                    dynamoViewModel.Model.Logger.Log("EasterEggMessage1 is disabled from view");
                }
            }
        }

        private void TrackStartupAnalytics()
        {
            if (!Analytics.ReportingAnalytics) return;

            string packages = string.Empty;
            var pkgExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            if (pkgExtension != null)
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

            Window packageManParent = null;
            //If any Guide is being executed then the ShowTermsOfUse Window WON'T be modal otherwise will be modal (as in the normal behavior)
            if (dynamoViewModel.MainGuideManager != null && GuideFlowEvents.IsAnyGuideActive)
                packageManParent = _this;
            var acceptedTermsOfUse = TermsOfUseHelper.ShowTermsOfUseDialog(false, null, packageManParent);
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
            var result = MessageBoxService.Show(this, dialogText,
                Dynamo.Wpf.Properties.Resources.UnsavedChangesMessageBoxTitle,
                buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // If the file is read-only, redirect yes to save-as.
                if (e.Workspace.IsReadOnly)
                    dynamoViewModel.ShowSaveDialogAndSaveResult(e.Workspace);
                else
                    e.Success = dynamoViewModel.ShowSaveDialogIfNeededAndSave(e.Workspace);
            }
            else if (result == MessageBoxResult.No)
            {
                //return true;
                e.Success = true;
            }
            else
            {
                e.Success = false;
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
            if (CrashReportTool.ShowCrashErrorReportWindow(dynamoViewModel,
                (args is CrashErrorReportArgs cerArgs) ? cerArgs : 
                new CrashErrorReportArgs(args.Details)))
            {
                return;
            }
            // Backup crash reporting dialog (in case ADSK CER is not found)
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
            var dpiX = 0.0;
            var dpiY = 0.0;

            // dpi aware, otherwise incorrect images are created
            try
            {
                var scale = VisualTreeHelper.GetDpi(this);
                dpiX = scale.PixelsPerInchX;
                dpiY = scale.PixelsPerInchY;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());

                dpiX = 96;
                dpiY = 96;
            }
            
            var bitmapSource = BackgroundPreview.View.RenderBitmap();
            // this image only really needs 24bits per pixel but to match previous implementation we'll use 32bit images.
            var rtBitmap = new RenderTargetBitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            rtBitmap.Render(BackgroundPreview.View);
            var encoder = new PngBitmapEncoder();
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
            var elements = dynamoViewModel.Model.SearchModel.SearchEntries;

            // Unique package and custom node categories
            var allCategories = getUniqueAddOnCategories(elements);

            var dialog = new FunctionNamePrompt(allCategories)
            {
                categoryBox = { Text = e.Category },
                DescriptionInput = { Text = e.Description },
                nameBox = { Text = e.Name },
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (e.CanEditName)
            {
                dialog.nameBox.Visibility = Visibility.Visible;
            }
            else
            {
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
        /// Helper function returns enum containing all unique package and custom node categories including all nested levels
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static IEnumerable<string> getUniqueAddOnCategories(IEnumerable<NodeSearchElement> elements)
        {
            List<string> addOns = new List<string>();
            foreach (var element in elements)
            {
                // Only include packages and custom nodes
                if (element.ElementType.HasFlag(ElementTypes.Packaged) || element.ElementType.HasFlag(ElementTypes.CustomNode))
                {
                    // Ordered list of all categories for the search element including all nested categories
                    var allAddOns = element.Categories.ToList();

                    string category = "";

                    // Construct all categories levels for the element starting at the top level
                    for (int i = 0; i < allAddOns.Count; i++)
                    {
                        // Append nested categories for the given element
                        if (i != 0)
                        {
                            category += "." + allAddOns[i];
                            addOns.Add(category);
                        }
                        // The first list item is the package/custom nodes top level category
                        else
                        {
                            category += allAddOns[i];
                            addOns.Add(allAddOns[i]);
                        }
                    }
                }
            }

            return addOns.Distinct();
        }

        /// <summary>
        /// Handles the request for the presentation of the preset name prompt
        /// </summary>
        /// <param name="e">a parameter object contains default Name and Description,
        /// and Success bool returned from the dialog</param>
        private void DynamoViewModelRequestPresetNamePrompt(PresetsNamePromptEventArgs e)
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
            var newDialog = new PresetOverwritePrompt()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Text = Wpf.Properties.Resources.PresetWarningMessage,
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
                //Shutdown was canceled
                return false;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            SaveExtensionWindowsState();

            if (!PerformShutdownSequenceOnViewModel() && !DynamoModel.IsTestMode)
            {
                e.Cancel = true;
            }
            else
            {
                isPSSCalledOnViewModelNoCancel = true;
            }
        }

        /// <summary>
        /// Saves the state of currently displayed extension windows. This is needed because the closing event is
        /// not called on child windows: https://docs.microsoft.com/en-us/dotnet/api/system.windows.window.closing
        /// </summary>
        private void SaveExtensionWindowsState()
        {
            foreach (var window in ExtensionWindows.Values)
            {
                SaveExtensionWindowSettings(window);
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
            dynamoViewModel.RequestPackageManagerSearchDialog -= DynamoViewModelRequestShowPackageManagerSearch;

            //FUNCTION NAME PROMPT
            dynamoViewModel.Model.RequestsFunctionNamePrompt -= DynamoViewModelRequestsFunctionNamePrompt;

            //Preset Name Prompt
            dynamoViewModel.Model.RequestPresetsNamePrompt -= DynamoViewModelRequestPresetNamePrompt;
            dynamoViewModel.RequestPresetsWarningPrompt -= DynamoViewModelRequestPresetWarningPrompt;

            dynamoViewModel.RequestClose -= DynamoViewModelRequestClose;
            dynamoViewModel.RequestSaveImage -= DynamoViewModelRequestSaveImage;
            dynamoViewModel.RequestSave3DImage -= DynamoViewModelRequestSave3DImage;

            dynamoViewModel.SidebarClosed -= DynamoViewModelSidebarClosed;

            DynamoSelection.Instance.Selection.CollectionChanged -= Selection_CollectionChanged;

            dynamoViewModel.RequestUserSaveWorkflow -= DynamoViewModelRequestUserSaveWorkflow;
            GuideFlowEvents.GuidedTourStart -= GuideFlowEvents_GuidedTourStart;

            if (dynamoViewModel.Model != null)
            {
                dynamoViewModel.Model.RequestsCrashPrompt -= Controller_RequestsCrashPrompt;
                dynamoViewModel.Model.RequestTaskDialog -= Controller_RequestTaskDialog;
                dynamoViewModel.Model.ClipBoard.CollectionChanged -= ClipBoard_CollectionChanged;
            }

            //ABOUT WINDOW
            dynamoViewModel.RequestAboutWindow -= DynamoViewModelRequestAboutWindow;

            //first all view extensions have their shutdown methods called
            //when this view is finally disposed, dispose will be called on them.
            foreach (var ext in viewExtensionManager.ViewExtensions)
            {
                if (ext is ILogSource logSource)
                {
                    logSource.MessageLogged -= Log;
                }

                if (ext is INotificationSource notificationSource)
                {
                    notificationSource.NotificationLogged -= LogNotification;
                }

                try
                {
                    ext.Shutdown();
                }
                catch (Exception exc)
                {
                    Log($"{ext.Name} :  {exc.Message} during shutdown" );
                }
            }
          

            viewExtensionManager.MessageLogged -= Log;
            BackgroundPreview = null;
            background_grid.Children.Clear();

            //COMMANDS
            this.dynamoViewModel.RequestPaste -= OnRequestPaste;
            this.dynamoViewModel.RequestReturnFocusToView -= OnRequestReturnFocusToView;
            this.dynamoViewModel.Model.WorkspaceSaving -= OnWorkspaceSaving;
            this.dynamoViewModel.Model.WorkspaceOpened -= OnWorkspaceOpened;
            DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved -= CheckTestFlags;

            this.dynamoViewModel.RequestEnableShortcutBarItems -= DynamoViewModel_RequestEnableShortcutBarItems;

            this.Dispose();
            sharedViewExtensionLoadedParams?.Dispose();
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
                workspace_vm.Model.OnCurrentOffsetChanged(this, new PointEventArgs(new Point2D(workspace_vm.X, workspace_vm.Y)));
                workspace_vm.OnZoomChanged(this, new ZoomEventArgs(workspace_vm.Zoom));

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
            var senderItems = statesMenu.Items.OfType<MenuItem>().Select(x => x.Tag).ToList();
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
                    //the sender was the delete menu
                    stateItem.Click += DeleteState_Click;
                    stateItem.ToolTip = state.Description;
                    ((MenuItem)sender).Items.Add(stateItem);
                }
            }
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

        private void OnDebugModesClick(object sender, RoutedEventArgs e)
        {
            var debugModesWindow = new DebugModesWindow();
            debugModesWindow.Owner = this;
            debugModesWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            debugModesWindow.ShowDialog();
        }

        private void OnPreferencesWindowClick(object sender, RoutedEventArgs e)
        {
            preferencesWindow = new PreferencesView(this);
            dynamoViewModel.OnPreferencesWindowChanged(preferencesWindow);
            preferencesWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            preferencesWindow.ShowDialog();
        }

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

        private void LibraryHandle_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            StackPanel sp = (StackPanel)(g.Children[0]);
            TextBlock tb = (TextBlock)(sp.Children[0]);
            Image collapseIcon = (Image)sp.Children[1];

            UpdateHandleHoveredStyle(tb, collapseIcon);
        }

        private void UpdateHandleHoveredStyle(TextBlock text, Image icon)
        {
            var bc = new BrushConverter();
            text.Foreground = (Brush)bc.ConvertFrom("#cccccc");

            if (LibraryCollapsed || ExtensionsCollapsed)
            {
                Uri imageUri;
                imageUri = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_hover.png");
                BitmapImage hover = new BitmapImage(imageUri);
                icon.Source = hover;
            }
        }

        private void ExtensionHandle_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            StackPanel sp = (StackPanel)(g.Children[0]);
            TextBlock tb = (TextBlock)(sp.Children[1]);
            Image collapseIcon = (Image)sp.Children[0];

            UpdateHandleHoveredStyle(tb, collapseIcon);
        }

        private bool libraryCollapsed;
        private bool extensionsCollapsed;
        private GridLength? extensionsColumnWidth;

        // Default side bar width
        private const int defaultSideBarWidth = 200;
        // By default the extension bar over canvas size ratio is 2/5
        private const int DefaultExtensionBarWidthMultiplier = 2;

        /// <summary>
        /// Check if library is collapsed or expanded
        /// </summary>
        public bool LibraryCollapsed
        {
            get
            {
                // Threshold that determines if button should be displayed
                if (LeftExtensionsViewColumn.Width.Value < 20)
                { libraryCollapsed = true; }

                else
                { libraryCollapsed = false; }

                return libraryCollapsed;
            }
        }

        /// <summary>
        /// Check if extensions right side bar is collapsed or expanded
        /// </summary>
        public bool ExtensionsCollapsed
        {
            get
            {
                // Special case: when the extension bar was never resized its size will be 2.
                // While 2 is a valid size for the extension bar, 5 is not one for the canvas,
                // so that's a safer check to be made.
                if (CanvasColumn.Width.Value == 5)
                {
                    extensionsCollapsed = RightExtensionsViewColumn.Width.Value == 0;
                }
                else
                {
                    extensionsCollapsed = RightExtensionsViewColumn.Width.Value < 20;
                }

                return extensionsCollapsed;
            }
        }

        // Check if library is collapsed or expanded and apply appropriate button state
        private void UpdateLibraryCollapseIcon()
        {
            if (LibraryCollapsed)
            {
                collapsedLibrarySidebar.Visibility = Visibility.Visible;
            }
            else
            {
                collapsedLibrarySidebar.Visibility = Visibility.Collapsed;
            }

        }

        // Show the extensions right side bar when there is atleast one extension
        private void HideOrShowRightSideBar()
        {
            if (ExtensionTabItems.Count == 0)
            {
                if (RightExtensionsViewColumn.Width.Value != 0)
                {
                    extensionsColumnWidth = RightExtensionsViewColumn.Width;
                }
                RightExtensionsViewColumn.Width = new GridLength(0, GridUnitType.Star);
                collapsedExtensionSidebar.Visibility = Visibility.Collapsed;
            }
            else
            {
                // The introduction of extensionsColumnWidth is two-fold:
                // 1. It allows the resized width to be remembered which is nice to have.
                // 2. It allows to avoid a slider glitch which sets the panels size in pixel amount but using star,
                // changing the proportions so that the initial value is counted as pixels after the first resize.
                if (extensionsColumnWidth == null)
                {
                    RightExtensionsViewColumn.Width = new GridLength(DefaultExtensionBarWidthMultiplier, GridUnitType.Star);
                }
                else
                {
                    RightExtensionsViewColumn.Width = extensionsColumnWidth.Value;
                }
                collapsedExtensionSidebar.Visibility = Visibility.Visible;
            }
        }

        private void OnCollapsedLeftSidebarClick(object sender, EventArgs e)
        {
            if (LibraryCollapsed)
            {
                // Restore extension view to default width (200)
                LeftExtensionsViewColumn.Width = new GridLength(defaultSideBarWidth, GridUnitType.Star);
            }
            else
            {
                LeftExtensionsViewColumn.Width = new GridLength(0, GridUnitType.Star);
            }

            UpdateLibraryCollapseIcon();
        }

        private void OnCollapsedRightSidebarClick(object sender, EventArgs e)
        {
            ToggleExtensionBarCollapseStatus();
        }

        /// <summary>
        /// Made internal for testing purposes only.
        /// </summary>
        internal void ToggleExtensionBarCollapseStatus()
        {
            if (ExtensionsCollapsed)
            {
                if (extensionsColumnWidth == null)
                {
                    RightExtensionsViewColumn.Width = new GridLength(DefaultExtensionBarWidthMultiplier, GridUnitType.Star);
                }
                else
                {
                    RightExtensionsViewColumn.Width = extensionsColumnWidth.Value;
                }
            }
            else
            {
                if (RightExtensionsViewColumn.Width.Value != 0)
                {
                    extensionsColumnWidth = RightExtensionsViewColumn.Width;
                }
                RightExtensionsViewColumn.Width = new GridLength(0, GridUnitType.Star);
            }

            // TODO: Maynot need this depending on tab design
            UpdateLibraryCollapseIcon();
        }

        private void LibraryHandle_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            StackPanel sp = (StackPanel)(g.Children[0]);
            TextBlock tb = (TextBlock)(sp.Children[0]);
            Image collapseIcon = (Image)sp.Children[1];

            UpdateHandleUnhoveredStyle(tb, collapseIcon);
            UpdateLibraryCollapseIcon();
        }

        private void UpdateHandleUnhoveredStyle(TextBlock text, Image icon)
        {
            var bc = new BrushConverter();
            text.Foreground = (Brush)bc.ConvertFromString("#aaaaaa");

            Uri imageUri;
            imageUri = new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/expand_normal.png");
            BitmapImage hover = new BitmapImage(imageUri);
            icon.Source = hover;
        }

        private void ExtensionHandle_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            StackPanel sp = (StackPanel)(g.Children[0]);
            TextBlock tb = (TextBlock)(sp.Children[1]);
            Image collapseIcon = (Image)sp.Children[0];

            UpdateHandleUnhoveredStyle(tb, collapseIcon);
        }

        private void LibraryClicked(object sender, EventArgs e)
        {
            restoreWidth = sidebarGrid.ActualWidth;
            LeftExtensionsViewColumn.MinWidth = 0;

            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
            verticalSplitter.Visibility = Visibility.Collapsed;
            sidebarGrid.Visibility = Visibility.Collapsed;

            horizontalSplitter.Width = double.NaN;
            UserControl view = (UserControl)sidebarGrid.Children[0];
            view.Visibility = Visibility.Collapsed;

            sidebarGrid.Visibility = Visibility.Collapsed;
            collapsedLibrarySidebar.Visibility = Visibility.Visible;
        }

        private void ExtensionsButtonClicked(object sender, EventArgs e)
        {
            restoreWidth = sidebarExtensionsGrid.ActualWidth;
            RightExtensionsViewColumn.MinWidth = 0;

            mainGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
            extensionSplitter.Visibility = Visibility.Collapsed;
            sidebarExtensionsGrid.Visibility = Visibility.Collapsed;

            horizontalSplitter.Width = double.NaN;
            UserControl view = (UserControl)sidebarExtensionsGrid.Children[0];
            view.Visibility = Visibility.Collapsed;

            sidebarExtensionsGrid.Visibility = Visibility.Collapsed;
            collapsedExtensionSidebar.Visibility = Visibility.Visible;
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
            UpdateWorkspaceTabSizes();
        }

        private void WorkspaceTabs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ToggleWorkspaceTabVisibility(WorkspaceTabs.SelectedIndex);

            UpdateWorkspaceTabSizes();

            // When workspace is resized apply appropriate library expand/collapse icon
            UpdateLibraryCollapseIcon();
        }

        /// <summary>
        /// Updates the workspace TabItems to have the correct margins in response to events
        /// such as the library being stretched or a Custom Node workspace being created.
        /// </summary>
        private void UpdateWorkspaceTabSizes()
        {
            // The Workspace TabItems must appear to the right of icon buttons (New File, Open, Save, Undo, Redo)
            // but never overlap them. 230 is the minimum offset required to achieve this. 
            // If the library panel is stretched greater than 230, they must align with its width instead.
            const int FirstTabItemMinimumLeftMarginOffset = 230;
            const int LibraryScrollBarWidth = 15;
            
            // We measure the full library width at runtime.
            int fullLibraryWidth = dynamoViewModel.LibraryWidth + LibraryScrollBarWidth;
            
            // Difference between the full library width (at runtime) and the minimum offset required
            // by the TabItems to not overlap the 5 icon buttons.
            int difference = fullLibraryWidth - FirstTabItemMinimumLeftMarginOffset;

            // If the library is narrower than the minimum width, we set the TabItems' left margin
            // to be the minimum offset required to not overlap the 5 icon buttons. 
            // If it's equal to or greater, we set the TabItems' left margin to be the difference
            // i.e. to align with the library panel.
            int leftMargin = fullLibraryWidth < FirstTabItemMinimumLeftMarginOffset ? difference : 0;

            List<TabItem> tabItems = WpfUtilities.ChildrenOfType<TabItem>(WorkspaceTabs).ToList();
            if (tabItems.Count < 1) return;

            // We iterate through each TabItem in the WorkspaceTabs TabControl and set its left and 
            // right margins, thereby offsetting the TabItem horizontally from the left edge
            // of the TabControl (AKA the left edge of the workspace).
            foreach (TabItem tabItem in tabItems)
            {
                tabItem.Margin = new System.Windows.Thickness(-leftMargin, 0, leftMargin, 0);
            }
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
        private void LogNotification(NotificationMessage notification)
        {
            dynamoViewModel.Model.Logger.LogNotification(notification.Sender, notification.Title,notification.ShortMessage, notification.DetailedMessage);
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if original sender was scroll bar(i.e Thumb) don't close the popup.
            if(!(e.OriginalSource is Thumb) && !(e.OriginalSource is TextBox))
            {
                HidePopupWhenWindowDeactivated(sender);
            }
        }

        private void GetStartedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowGetStartedGuidedTour();
        }

        /// <summary>
        /// This method probably will be modified or deleted in the future when the GuideManager and Guide class are created
        /// For now will be used just for testing/demo purposes since the popups will be created probably in the Guide class.
        /// </summary>
        private void ShowGetStartedGuidedTour()
        {
            //We pass the root UIElement to the GuidesManager so we can found other child UIElements
            try
            {                
                dynamoViewModel.MainGuideManager.LaunchTour(GuidesManager.GetStartedGuideName);
            }
            catch (Exception)
            {
                sidebarGrid.Visibility = Visibility.Visible;
            }
        }

        private void RightExtensionSidebar_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //Setting the width of right extension after resize to
            extensionsColumnWidth = RightExtensionsViewColumn.Width;
        }

        private void PackagesMenuGuide_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamoViewModel.MainGuideManager.LaunchTour(GuidesManager.PackagesGuideName);                
            }
            catch (Exception)
            {
                sidebarGrid.Visibility = Visibility.Visible;
            }
        }

        private void FileTrustWarning_Click(object sender, RoutedEventArgs e)
        {
            var dynViewModel = DataContext as DynamoViewModel;
            if (dynViewModel.FileTrustViewModel == null) return;
            dynViewModel.FileTrustViewModel.ShowWarningPopup = true;
        }

        private void DynamoView_Activated(object sender, EventArgs e)
        {            
            if (fileTrustWarningPopup != null && dynamoViewModel.ViewingHomespace)
            {
                fileTrustWarningPopup.ManagePopupActivation(true);
            }
        }

        private void DynamoView_Deactivated(object sender, EventArgs e)
        {
            if(fileTrustWarningPopup != null)
                fileTrustWarningPopup.ManagePopupActivation(false);
        }

        public void Dispose()
        {
            viewExtensionManager.Dispose();
            if (dynamoViewModel.Model.AuthenticationManager.HasAuthProvider && loginService != null)
            {
                dynamoViewModel.Model.AuthenticationManager.AuthProvider.RequestLogin -= loginService.ShowLogin;
            }

            // Removing the tab items list handler
            ExtensionTabItems.CollectionChanged -= this.OnCollectionChanged;

            if (fileTrustWarningPopup != null)
                fileTrustWarningPopup.CleanPopup();
        }
    }
}

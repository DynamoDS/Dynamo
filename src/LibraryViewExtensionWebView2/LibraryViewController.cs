using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CoreNodeModels.Properties;
using Dynamo.Extensions;
using Dynamo.LibraryViewExtensionWebView2.Handlers;
using Dynamo.LibraryViewExtensionWebView2.ViewModels;
using Dynamo.LibraryViewExtensionWebView2.Views;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;

namespace Dynamo.LibraryViewExtensionWebView2
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisibleAttribute(true)]

    /// <summary>
    /// This is for the object we're gonna host exposing the functions
    /// for clipboard management to React component
    /// </summary>
    public class ScriptObject
    {
        Action<string> onCopyToClipboard;
        Func<string> onPasteFromClipboard;

        internal ScriptObject(Action<string> onCopyToClipboard, Func<string> onPasteFromClipboard)
        {
            this.onCopyToClipboard = onCopyToClipboard;
            this.onPasteFromClipboard = onPasteFromClipboard;
        }

        /// <summary>
        /// This is the function we expose for adding a string to the clipboard
        /// In React component will be accesible from chrome.webview.hostObjects.scriptObject.CopyToClipboard(text)
        /// </summary>
        /// <param name="text">text to be added to the clipboard</param>
        public void CopyToClipboard(string text)
        {
            onCopyToClipboard(text);
        }

        /// <summary>
        /// This is the function we expose for paste a string from the clipboard
        /// In React component will be accesible from chrome.webview.hostObjects.scriptObject.PasteFromClipboard();
        /// </summary>
        public string PasteFromClipboard()
        {
            return onPasteFromClipboard();
        }

    }

    public class LibraryViewController : IDisposable
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DynamoViewModel dynamoViewModel;
        private FloatingLibraryTooltipPopup libraryViewTooltip;
        // private ResourceHandlerFactory resourceFactory;
        private IDisposable observer;
        internal DynamoWebView2 browser;
        ScriptingObject twoWayScriptingObject;
        private const string CreateNodeInstrumentationString = "Search-NodeAdded";
        // TODO remove this when we can control the library state from Dynamo more precisely.
        private bool disableObserver = false;

        private LayoutSpecProvider layoutProvider;
        private NodeItemDataProvider nodeProvider;
        internal SearchResultDataProvider searchResultDataProvider;
        internal IconResourceProvider iconProvider;
        private LibraryViewCustomization customization;
        internal string WebBrowserUserDataFolder { get; set; }

        //Assuming that the fon size is 14px and the screen height is 1080 initially
        private const int standardFontSize = 14;
        private const int standardScreenHeight = 1080;
        private double libraryFontSize;
        private const double minimumZoomScale = 0.25d;

        /// <summary>
        /// Creates a LibraryViewController.
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        internal LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive, LibraryViewCustomization customization)
        {
            this.dynamoWindow = dynamoView;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            this.customization = customization;
            libraryViewTooltip = CreateTooltipControl();

            this.commandExecutive = commandExecutive;
            InitializeResourceProviders(dynamoViewModel.Model, customization);
            dynamoWindow.StateChanged += DynamoWindowStateChanged;
            dynamoWindow.SizeChanged += DynamoWindow_SizeChanged;

            dynamoViewModel.PreferencesWindowChanged += DynamoViewModel_PreferencesWindowChanged;

            DirectoryInfo webBrowserUserDataFolder;
            var userDataDir = new DirectoryInfo(dynamoViewModel.Model.PathManager.UserDataDirectory);
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;
            if (webBrowserUserDataFolder != null)
            {
                WebBrowserUserDataFolder = webBrowserUserDataFolder.FullName;
            }

            /// Create and add the library view to the WPF visual tree.
            /// Also load the library.html and js files.
            LibraryViewModel model = new LibraryViewModel();
            LibraryView view = new LibraryView(model);
           
            //Adding the LibraryView to the sidebar ensures that the webview2 component is visible.
            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            sidebarGrid.Children.Add(view);

            browser = view.mainGrid.Children.OfType<DynamoWebView2>().FirstOrDefault();

            browser.Loaded += Browser_Loaded;
            browser.SizeChanged += Browser_SizeChanged;

            LibraryViewController.SetupSearchModelEventsObserver(browser, dynamoViewModel.Model.SearchModel,
                    this, this.customization);
        }

        private void DynamoViewModel_PreferencesWindowChanged(object sender, EventArgs e)
        {
            var preferencesView = (Wpf.Views.PreferencesView)sender;
            preferencesView.LibraryZoomScalingSlider.ValueChanged += DynamoSliderValueChanged;
        }

        private void Browser_ZoomFactorChanged(object sender, EventArgs e)
        {
            //Multiplies by 100 so the value can be saved as a percentage
            dynamoViewModel.Model.PreferenceSettings.LibraryZoomScale = (int)(browser.ZoomFactor * 100);
        }

        //if the window is resized toggle visibility of browser to force redraw
        private void DynamoWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (browser != null)
            {
                browser.InvalidateVisual();
            }
        }

        //if the dynamo window is minimized and then restored, force a layout update.
        private void DynamoWindowStateChanged(object sender, EventArgs e)
        {
            if (browser != null)
            {
                browser.InvalidateVisual();
            }
        }

        #region methods exposed to JS
        /// <summary>
        /// Call this method to create a new node in Dynamo canvas.
        /// </summary>
        /// <param name="nodeName">Node creation name</param>
        public void CreateNode(string nodeName)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if the node we're trying to create is a customNode, lets disable the eventObserver.
                // this will stop the libraryController from refreshing the libraryView on custom node creation.
                var resultGuid = Guid.Empty;
                if (Guid.TryParse(nodeName, out resultGuid))
                {
                    this.disableObserver = true;
                }
                //Create the node of given item name
                var cmd = new DynamoModel.CreateNodeCommand(Guid.NewGuid().ToString(), nodeName, -1, -1, true, false);
                commandExecutive.ExecuteCommand(cmd, Guid.NewGuid().ToString(), LibraryViewExtensionWebView2.ExtensionName);

                this.disableObserver = false;
            }));
        }

        /// Call this method to import a zero touch library. It will prompt
        /// user to select the zero touch dll.
        /// </summary>
        public void ImportLibrary()
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
                dynamoViewModel.ImportLibraryCommand.Execute(null)
            ));
        }

        /// <summary>
        /// This method will read the layoutSpecs.json and layoutType and then pass the info to javacript so all the resources can be loaded
        /// </summary>
        /// <param name="browser"></param>
        internal void RefreshLibraryView(WebView2 browser)
        {
            string layoutSpecsjson = String.Empty;
            string loadedTypesjson = String.Empty;
            if (!dynamoViewModel.Model.IsServiceMode)
            {
                dynamoWindow.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var ext1 = string.Empty;
                    var ext2 = string.Empty;
                    var reader = new StreamReader(nodeProvider.GetResource(null, out ext1));
                    var loadedTypes = reader.ReadToEnd();

                    var reader2 = new StreamReader(layoutProvider.GetResource(null, out ext2));
                    var layoutSpec = reader2.ReadToEnd();

                    ExecuteScriptFunctionAsync(browser, "refreshLibViewFromData", loadedTypes, layoutSpec);
                }));
            }
        }

        #endregion

        /// <summary>
        /// This function will copy a string to clipboard 
        /// </summary>
        /// <param name="text">text to be added to clipboard</param>
        internal void OnCopyToClipboard(string text)
        {
            dynamoViewModel.Model.ClipBoard.Clear();
            Clipboard.SetText(text);
        }

        /// <summary>
        /// This function will return the clipboard content
        /// </summary>
        internal string OnPasteFromClipboard() {
            return Clipboard.GetText();
        }

        private string ReplaceUrlWithBase64Image(string html, string minifiedURL, bool magicreplace = true)
        {
            var ext = string.Empty;
            var magicstringprod = "__webpack_require__.p+";
            var minifiedURLHtmlReplacement = minifiedURL;
            // this depends on librariejs minification producing the same results - 
            // longterm this is fragile. We should intercept these requests and handle them instead.
            if (magicreplace)
            {
                minifiedURL = $"{magicstringprod}\"{minifiedURL}\"";
            }
            var searchString = minifiedURL.Replace(magicstringprod, @"./dist/").Replace("\"", "");
            var base64 = iconProvider.GetResourceAsString(searchString, out ext);
            if (string.IsNullOrEmpty(base64))
            {
                throw new Exception($"could not find resource {searchString}");
            }
            //replace some urls to svg icons with base64 data.
            if (ext == "svg")
            {
                ext = "svg+xml";
                base64 = $"data:image/{ext};base64, {base64}";
            }


            if (ext == "ttf" || ext == "woff" || ext == "woff2" || ext == "eot")
            {

                base64 = $"data:application/x-font-{ext};charset=utf-8;base64,{base64}";
            }

            //In Libraryjs project, Webpack5 is removing the initial slash "/" when using resource files so for example in the html we have the string like "/resources/image.svg" then we need to remove the first slash (the first char) otherwise it won't be found and replaced by the base64 content.
            html = html.Replace(minifiedURL.Replace(minifiedURLHtmlReplacement, minifiedURLHtmlReplacement.TrimStart('/')), '"' + base64 + '"');
            return html;
        }

        //list of resources which have paths embedded directly into the source.
        private readonly Tuple<string, bool>[] dynamicResourcePaths = new Tuple<string, bool>[]
        {
           Tuple.Create("resources/ArtifaktElement-Bold.woff",true),
           Tuple.Create("resources/ArtifaktElement-Regular.woff",true),
           Tuple.Create("resources/bin.svg",true),
           Tuple.Create("resources/default-icon.svg",true),
           Tuple.Create("resources/library-action.svg",true),
           Tuple.Create("resources/library-create.svg",true),
           Tuple.Create("resources/library-query.svg",true),
           Tuple.Create("resources/indent-arrow-category-down.svg",true),
           Tuple.Create("resources/indent-arrow-category-right.svg",true),
           Tuple.Create("resources/indent-arrow-down.svg",true),
           Tuple.Create("resources/indent-arrow-right.svg",true),
           Tuple.Create("resources/plus-symbol.svg",true),
           Tuple.Create("resources/search-detailed.svg",true),
           Tuple.Create("resources/search-filter.svg",true),
           Tuple.Create("resources/search-filter-selected.svg",true),
           Tuple.Create("resources/search-icon.svg",true),
           Tuple.Create("resources/search-icon-clear.svg",true)
        };

        async void InitializeAsync()
        {
            try
            {
                var absolutePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"runtimes\win-x64\native");
                CoreWebView2Environment.SetLoaderDllFolderPath(absolutePath);
            }
            catch (InvalidOperationException e)
            {
                LogToDynamoConsole("WebView2Loader.dll is already loaded successfully.");
            }
            
            browser.CoreWebView2InitializationCompleted += Browser_CoreWebView2InitializationCompleted;

            if (!string.IsNullOrEmpty(WebBrowserUserDataFolder))
            {
                //This indicates in which location will be created the WebView2 cache folder
                this.browser.CreationProperties = new CoreWebView2CreationProperties()
                {
                    UserDataFolder = WebBrowserUserDataFolder
                };
            }

            try
            {
                await browser.Initialize(LogToDynamoConsole);

                this.browser.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                twoWayScriptingObject = new ScriptingObject(this);
                //register the interop object into the browser.
                this.browser.CoreWebView2.AddHostObjectToScript("bridgeTwoWay", twoWayScriptingObject);
                browser.CoreWebView2.Settings.IsZoomControlEnabled = true;
            }
            catch (ObjectDisposedException ex)
            {
                LogToDynamoConsole(ex.Message);
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            String strMessage = args.TryGetWebMessageAsString();
            twoWayScriptingObject.Notify(strMessage);
        }

        private void Browser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                if (e.InitializationException != null)
                {
                    LogToDynamoConsole(e.InitializationException.Message);
                }
                LogToDynamoConsole("LibraryViewExtension CoreWebView2 initialization failed.");
                return;
            }

            LibraryViewModel model = new LibraryViewModel();
            LibraryView view = new LibraryView(model);

            var lib_min_template = "LIBPLACEHOLDER";
            var libHTMLURI = "Dynamo.LibraryViewExtensionWebView2.Packages.LibrarieJS.library.html";
            var stream = LoadResource(libHTMLURI);

            var libMinURI = "Dynamo.LibraryViewExtensionWebView2.Packages.LibrarieJS.build.librarie.min.js";
            var libminstream = LoadResource(libMinURI);
            var libminstring = "LIBJS";
            var libraryHTMLPage = "LIBRARY HTML WAS NOT FOUND";

            using (var reader = new StreamReader(libminstream))
            {
                libminstring = reader.ReadToEnd();
                // replace some resources and their paths so that no requests are generated
                // instead the base64 data is already embedded. This list includes common icons and fonts.
                dynamicResourcePaths.ToList().ForEach(path =>
                {
                    libminstring = ReplaceUrlWithBase64Image(libminstring, path.Item1, path.Item2);
                });
            }

            using (var reader = new StreamReader(stream))
            {
                libraryHTMLPage = reader.ReadToEnd().Replace(lib_min_template, libminstring);
            }

            try
            {
                this.browser.NavigateToString(libraryHTMLPage);
                SetLibraryFontSize();
                SetTooltipText();
                browser.ZoomFactor = (double)dynamoViewModel.Model.PreferenceSettings.LibraryZoomScale / 100d;
                browser.ZoomFactorChanged += Browser_ZoomFactorChanged;
                browser.KeyDown += Browser_KeyDown;

                // Hosts an object that will expose the properties and methods to be called from the javascript side
                browser.CoreWebView2.AddHostObjectToScript("scriptObject",
                    new ScriptObject(OnCopyToClipboard, OnPasteFromClipboard));
            }
            catch (Exception ex)
            {
                LogToDynamoConsole("LibraryViewExtension CoreWebView2 initialization failed: " + ex.Message);
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            string msg = "Browser Loaded";
            LogToDynamoConsole(msg);

            InitializeAsync();
        }

        // This enum is for matching the modifier keys between C# and javaScript
        enum ModifiersJS
        {
            none = 0,
            altKey = 1,
            ctrlKey = 2,
            shiftKey = 4
        }

        // This enum is for define the events to be tracked
        enum EventsTracked
        {
            Delete,
            A,
            C,
            V
        }

        /// <summary>
        /// Collect the main and modifier key from KeyEventArgs in order to pass
        /// that data to eventDispatcher (located in library.html) which is responsible
        /// for binding KeyDown events between dynamo and webview instances
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///

        private void Browser_KeyDown(object sender, KeyEventArgs e)

        {
            if (!Enum.IsDefined(typeof(EventsTracked), e.Key.ToString())) return;

            var synteticEventData = new Dictionary<string, string>
            {
                [Enum.GetName(typeof(ModifiersJS), e.KeyboardDevice.Modifiers)] = "true",
                ["key"] = e.Key.ToString()
            };

            _ = ExecuteScriptFunctionAsync(browser, "eventDispatcher", synteticEventData);
        }


        //if the browser window itself is resized, toggle visibility to force redraw.
        private void Browser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (browser != null)
            {
                browser.InvalidateVisual();
                UpdatePopupLocation();
                SetLibraryFontSize();
            }
        }

        //Changes the size of the font's library depending of the screen height
        private async void SetLibraryFontSize()
        {
            //Gets the height of the primary monitor
            var height = SystemParameters.PrimaryScreenHeight;

            //Calculates the proportion of the font size depending on the screen height
            //Changing the scale also changes the screen height (F.E: height of 1080px with 150% will be actually 720px)
            var fontSize = (standardFontSize * height) / standardScreenHeight;

            if(fontSize != libraryFontSize)
            {
                var result = await ExecuteScriptFunctionAsync(browser, "setLibraryFontSize", fontSize);
                if(result != null)
                    libraryFontSize = fontSize;
            }
        }

        private async void SetTooltipText()
        {
            var jsonTooltipText = new { create = Resources.TooltipTextCreate, action = Resources.TooltipTextAction, query = Resources.TooltipTextQuery };
            var jsonString = JsonConvert.SerializeObject(jsonTooltipText);
            var result = await ExecuteScriptFunctionAsync(browser, "setTooltipText", jsonString);
        }

        #region Tooltip

        /// <summary>
        /// Call this method to create a new node in Dynamo canvas.
        /// </summary>
        /// <param name="nodeName">Node creation name</param>
        /// <param name="y">The y position</param>
        public void ShowNodeTooltip(string nodeName, double y)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowTooltip(nodeName, y);
            }));
        }

        /// <summary>
        /// Call this method to create a new node in Dynamo canvas.
        /// </summary>
        public void CloseNodeTooltip(bool closeImmediately)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                CloseTooltip(closeImmediately);
                //The packages installed are shown at this moment then we need to update the Popup location and the interactions with the Library
                if (GuideFlowEvents.IsAnyGuideActive)
                {
                    GuideFlowEvents.OnUpdatePopupLocation();
                    GuideFlowEvents.OnUpdateLibraryInteractions();
                }
            }));
        }

        private FloatingLibraryTooltipPopup CreateTooltipControl()
        {
            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;

            var tooltipPopup = new FloatingLibraryTooltipPopup(200)
            {
                Name = "libraryToolTipPopup",
                StaysOpen = true,
                AllowsTransparency = true,
                PlacementTarget = sidebarGrid
            };
            sidebarGrid.Children.Add(tooltipPopup);

            return tooltipPopup;
        }

        private NodeSearchElementViewModel FindTooltipContext(String nodeName)
        {
            return dynamoViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FindViewModelForNode(nodeName);
        }

        private void ShowTooltip(String nodeName, double y)
        {
            var nseViewModel = FindTooltipContext(nodeName);
            if (nseViewModel == null)
            {
                return;
            }

            libraryViewTooltip.UpdateYPosition(y);
            libraryViewTooltip.SetDataContext(nseViewModel);
        }

        private void CloseTooltip(bool closeImmediately)
        {
            libraryViewTooltip.SetDataContext(null, closeImmediately);
        }

        #endregion

        private Stream LoadResource(string url)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var textStream = assembly.GetManifestResourceStream(url);
            return textStream;
        }

        internal static IDisposable SetupSearchModelEventsObserver(WebView2 webview, NodeSearchModel model, LibraryViewController controller, ILibraryViewCustomization customization, int throttleTime = 200)
        {
            customization.SpecificationUpdated += (o, e) =>
            {
                controller.RefreshLibraryView(webview);
                controller.CloseNodeTooltip(true);
            };

            var observer = new EventObserver<NodeSearchElement, IEnumerable<NodeSearchElement>>(
                                elements => NotifySearchModelUpdate(customization, elements), CollectToList
                            ).Throttle(TimeSpan.FromMilliseconds(throttleTime));

            Action<NodeSearchElement> handler = (searchElement) =>
            {
                var libraryViewController = (controller as LibraryViewController);
                if ((libraryViewController != null) && (libraryViewController.disableObserver))
                {
                    return;
                }

                observer.OnEvent(searchElement);
            };
            Action<NodeSearchElement> onRemove = e => handler(null);

            if (model != null)
            {
                //Set up the event callback
                model.EntryAdded += handler;
                model.EntryRemoved += onRemove;
                model.EntryUpdated += handler;

                //Set up the dispose event handler
                observer.Disposed += () =>
                {
                    model.EntryAdded -= handler;
                    model.EntryRemoved -= onRemove;
                    model.EntryUpdated -= handler;
                };
            }

            return observer;
        }

        /// <summary>
        /// Returns a new list by adding the given element to the given list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">This old list of elements</param>
        /// <param name="element">The element to add to the list</param>
        /// <returns>The new list containing all items from input list and the given element</returns>
        private static IEnumerable<T> CollectToList<T>(IEnumerable<T> list, T element)
        {
            //Accumulate all non-null element into a list.
            if (list == null) list = Enumerable.Empty<T>();
            return element != null ? list.Concat(Enumerable.Repeat(element, 1)) : list;
        }

        /// <summary>
        /// Notifies the SearchModel update event with list of updated elements
        /// </summary>
        /// <param name="customization">ILibraryViewCustomization to update the 
        /// specification and raise specification changed event.</param>
        /// <param name="elements">List of updated elements</param>
        private static void NotifySearchModelUpdate(ILibraryViewCustomization customization, IEnumerable<NodeSearchElement> elements)
        {
            //elements might be null if we have removed an element.
            if (elements != null)
            {
                var includes = elements
               .Select(NodeItemDataProvider.GetFullyQualifiedName)
               .Select(name => name.Split('.').First())
               .Distinct()
               .SkipWhile(s => s.Contains("://"))
               .Select(p => new LayoutIncludeInfo() { path = p });

                customization.AddIncludeInfo(includes, "Add-ons");
            }
        }

        /// <summary>
        /// creates the resource providers that are used throughout the extensions lifetime
        /// to retrieve images and other resource files from disk.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="customization"></param>
        private void InitializeResourceProviders(DynamoModel model, LibraryViewCustomization customization)
        {
            var dllProvider = new DllResourceProvider("http://localhost/dist", "Dynamo.LibraryViewExtensionWebView2.Packages.LibrarieJS");
            iconProvider = new IconResourceProvider(model.PathManager, dllProvider, customization);
            nodeProvider = new NodeItemDataProvider(model.SearchModel, iconProvider);
            searchResultDataProvider = new SearchResultDataProvider(model.SearchModel, iconProvider);
            layoutProvider = new LayoutSpecProvider(customization, iconProvider, "Dynamo.LibraryViewExtensionWebView2.Packages.LibrarieJS.layoutSpecs.json");
        }

        private void DynamoSliderValueChanged(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;
            //The default value of the zoom factor is 1.0. The value that comes from the slider is in percentage, so we divide by 100 to be equivalent
            double zoomFactor = slider.Value / 100d;

            //To avoid an invalid value for the zoom factor
            if (zoomFactor < minimumZoomScale)
                zoomFactor = minimumZoomScale;

            browser.ZoomFactor = zoomFactor;
            dynamoViewModel.Model.PreferenceSettings.LibraryZoomScale = ((int)slider.Value);
        }

        /// <summary>
        /// This method will execute the action of moving the Guide to the next Step (it is triggered when a specific html div that contains the package is clicked).
        /// </summary>
        internal void MoveToNextStep()
        {
            GuideFlowEvents.OnGuidedTourNext();
        }

        //This method will be called when the Library was resized and the current Popup location needs to be updated
        internal void UpdatePopupLocation()
        {
            GuideFlowEvents.OnUpdatePopupLocation();
        }

        /// <summary>
        /// Convenience method for logging to Dynamo Console.
        /// </summary>
        /// <param name="meessage"></param>
        internal void LogToDynamoConsole(string message)
        {
            if (DynamoModel.IsTestMode)
            {
                System.Console.WriteLine(message);
            }
            else
            {
                this.dynamoViewModel?.Model?.Logger?.Log(message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (observer != null) observer.Dispose();
            observer = null;
            if (this.dynamoWindow != null)
            {
                dynamoWindow.StateChanged -= DynamoWindowStateChanged;
                dynamoWindow.SizeChanged -= DynamoWindow_SizeChanged;
                browser.ZoomFactorChanged -= Browser_ZoomFactorChanged;
                dynamoViewModel.PreferencesWindowChanged-= DynamoViewModel_PreferencesWindowChanged;
                dynamoWindow = null;
                browser.KeyDown -= Browser_KeyDown;
            }
            if (this.browser != null)
            {
                browser.CoreWebView2.RemoveHostObjectFromScript("bridgeTwoWay");
                browser.SizeChanged -= Browser_SizeChanged;
                browser.Loaded -= Browser_Loaded;
                browser.Dispose();
                browser = null;
            }
            twoWayScriptingObject.Dispose();
            dynamoViewModel = null;
            commandExecutive = null;
        }

        public static async Task<string> ExecuteScriptFunctionAsync(WebView2 webView2, string functionName, params object[] parameters)
        {
            if (webView2.CoreWebView2 == null)
                return null;

            string script = functionName + "(";
            for (int i = 0; i < parameters.Length; i++)
            {
                script += JsonConvert.SerializeObject(parameters[i]);
                if (i < parameters.Length - 1)
                {
                    script += ", ";
                }
            }
            script += ");";
            return await webView2.ExecuteScriptAsync(script);
        }
        /// <summary>
        /// Gives the library UI information on Dynamo's current context.
        /// </summary>
        /// <param name="type"></param>
        internal void UpdateContext(string type)
        {
            ExecuteScriptFunctionAsync(browser,"libController.setHostContext", type);
            ExecuteScriptFunctionAsync(browser, "replaceImages");
        }
    }
}

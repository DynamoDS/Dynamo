using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.LibraryViewExtensionMSWebBrowser.Handlers;
using Dynamo.LibraryViewExtensionMSWebBrowser.ViewModels;
using Dynamo.LibraryViewExtensionMSWebBrowser.Views;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dynamo.LibraryViewExtensionMSWebBrowser
{
    /// <summary>
    /// This object is exposed to the browser to pass data back and forth between contexts.
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisibleAttribute(true)]
    public class scriptingObject
    {
        private LibraryViewController controller;

        public scriptingObject(LibraryViewController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Main callback which libraryJS uses to notify the extension to perform some action.
        /// </summary>
        /// <param name="data"></param>
        public void notify(string data)
        {
            scriptNotifyHandler(data);
        }

        /// <summary>
        /// Used to get access to the iconResourceProvider and return a base64encoded string version of an icon.
        /// </summary>
        /// <param name="iconurl"></param>
        public void GetBase64StringFromPath(string iconurl)
        {

            string ext;
            var iconAsBase64 = controller.iconProvider.GetResourceAsString(iconurl, out ext);
            if (string.IsNullOrEmpty(iconAsBase64))
            {
                controller.browser.InvokeScript("completeReplaceImages", "");
            }
            if (ext.Contains("svg"))
            {
                ext = "svg+xml";
            }
            //send back result.
            controller.browser.InvokeScript("completeReplaceImages", $"data:image/{ext};base64, {iconAsBase64}");
        }

        private void scriptNotifyHandler(string dataFromjs)
        {

            if (string.IsNullOrEmpty(dataFromjs))
            {
                return;
            }
         
            try
            {
                //a simple refresh of the libary is requested from js context.
                if (dataFromjs == "requestUpdateLibrary")
                {
                    controller.RefreshLibraryView(controller.browser);
                    return;
                }
                //a more complex action needs to be taken on the c# side.
                /*dataFromjs will be an object like:

                {func:funcName,
                data:"string" | data:object[] | bool}
                 */

                var simpleRPCPayload = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataFromjs);
                var funcName = simpleRPCPayload["func"] as string;
                if (funcName == "createNode")
                {
                    var data = simpleRPCPayload["data"] as string;
                    controller.CreateNode(data);
                }
                else if (funcName == "showNodeTooltip")
                {
                    var data = (simpleRPCPayload["data"] as JArray).Children();
                    controller.ShowNodeTooltip(data.ElementAt(0).Value<string>(), data.ElementAt(1).Value<double>());
                }
                else if (funcName == "closeNodeTooltip")
                {
                    var data = (bool)simpleRPCPayload["data"];
                    controller.CloseNodeTooltip(data);
                }
                else if (funcName == "importLibrary")
                {
                    controller.ImportLibrary();
                }
                else if (funcName == "logEventsToInstrumentation")
                {
                    var data = (simpleRPCPayload["data"] as JArray).Children();
                    controller.LogEventsToInstrumentation(data.ElementAt(0).Value<string>(), data.ElementAt(1).Value<string>());
                }
                else if (funcName == "performSearch")
                {
                    var data = simpleRPCPayload["data"] as string;
                    var extension = string.Empty;
                    var searchStream = controller.searchResultDataProvider.GetResource(data, out extension);
                    var searchReader = new StreamReader(searchStream);
                    var results = searchReader.ReadToEnd();
                    //send back results to libjs
                    controller.browser.
                     InvokeScript("completeSearch", results);
                    searchReader.Dispose();
                }
            }
            catch (Exception e)
            {
                this.controller.LogToDynamoConsole($"Error while parsing command data from javascript{Environment.NewLine}{e.Message}");
            }
        }
    }

    /// <summary>
    /// This class acts as the entry point to create the browser view as well 
    /// as implementing the methods which are called from the javascript context.
    /// </summary>
    public class LibraryViewController : IDisposable
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DynamoViewModel dynamoViewModel;
        private FloatingLibraryTooltipPopup libraryViewTooltip;
        // private ResourceHandlerFactory resourceFactory;
        private IDisposable observer;
        internal WebBrowser browser;
        private const string CreateNodeInstrumentationString = "Search-NodeAdded";
        // TODO remove this when we can control the library state from Dynamo more precisely.
        private bool disableObserver = false;

        private LayoutSpecProvider layoutProvider;
        private NodeItemDataProvider nodeProvider;
        internal SearchResultDataProvider searchResultDataProvider;
        internal IconResourceProvider iconProvider;
        private LibraryViewCustomization customization;
        private scriptingObject interop;

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
            interop = new scriptingObject(this);
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
                commandExecutive.ExecuteCommand(cmd, Guid.NewGuid().ToString(), LibraryViewExtensionMSWebBrowser.ExtensionName);
                LogEventsToInstrumentation(CreateNodeInstrumentationString, nodeName);

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
        /// This function logs events to instrumentation if it matches a set of known events
        /// </summary>
        /// <param name="eventName">Event Name that gets logged to instrumentation</param>
        /// <param name="data"> Data that gets logged to instrumentation </param>
        public void LogEventsToInstrumentation(string eventName, string data)
        {
            if (eventName == "Search" || eventName == "Filter-Categories" || eventName == "Search-NodeAdded")
            {
                Analytics.LogPiiInfo(eventName, data);
            }
        }
        internal void RefreshLibraryView(WebBrowser browser)
        {
            dynamoWindow.Dispatcher.BeginInvoke(
            new Action(() =>
            {

                var ext1 = string.Empty;
                var ext2 = string.Empty;
            //read the full loadedTypes and LayoutSpec json strings.
            var reader = new StreamReader(nodeProvider.GetResource(null, out ext1));
                var loadedTypes = reader.ReadToEnd();

                var reader2 = new StreamReader(layoutProvider.GetResource(null, out ext2));
                var layoutSpec = reader2.ReadToEnd();

                try
                {
                    browser.InvokeScript
                         ("refreshLibViewFromData", loadedTypes, layoutSpec);
                }
                catch (Exception e)
                {
                    this.dynamoViewModel.Model.Logger.Log(e);
                }
                reader.Dispose();
                reader2.Dispose();

            }));

        }

        #endregion

        private string ReplaceUrlWithBase64Image(string html, string minifiedURL, bool magicreplace = true)
        {
            var ext = string.Empty;
            // this depends on librariejs minification producing the same results - 
            // longterm this is fragile. We should intercept these requests and handle them instead.
            if (magicreplace)
            {
                minifiedURL = $"n.p+\"{minifiedURL}\"";
            }
            var searchString = minifiedURL.Replace("n.p+", @"./dist").Replace("\"", "");
            var base64 = iconProvider.GetResourceAsString(searchString, out ext);
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

            html = html.Replace(minifiedURL, '"' + base64 + '"');
            return html;
        }

        //list of resources which have paths embedded directly into the source.
        private readonly Tuple<string, bool>[] dynamicResourcePaths = new Tuple<string, bool>[15]
        {
           Tuple.Create("/resources/library-create.svg",true),
           Tuple.Create("/resources/default-icon.svg",true),
           Tuple.Create("/resources/fontawesome-webfont.eot",true),
           Tuple.Create("/resources/fontawesome-webfont.ttf",true),
           Tuple.Create("/resources/fontawesome-webfont.woff2",true),
           Tuple.Create("/resources/fontawesome-webfont.woff",true),
           Tuple.Create("/resources/library-action.svg",true),
           Tuple.Create("/resources/library-query.svg",true),
           Tuple.Create("/resources/indent-arrow-down-wo-lines.svg",true),
           Tuple.Create("/resources/indent-arrow-down.svg",true),
           Tuple.Create("/resources/indent-arrow-right-last.svg",true),
           Tuple.Create("/resources/indent-arrow-right-wo-lines.svg",true),
           Tuple.Create("/resources/indent-arrow-right.svg",true),
           Tuple.Create("/resources/ArtifaktElement-Bold.woff",true),
           Tuple.Create("/resources/ArtifaktElement-Regular.woff",true)
        };

        /// <summary>
        /// Creates and adds the library view to the WPF visual tree.
        /// Also loads the library.html and js files.
        /// </summary>
        /// <returns>LibraryView control</returns>
        internal void AddLibraryView()
        {
            LibraryViewModel model = new LibraryViewModel();
            LibraryView view = new LibraryView(model);

            var lib_min_template = "LIBPLACEHOLDER";
            var libHTMLURI = "Dynamo.LibraryViewExtensionMSWebBrowser.web.library.library.html";
            var stream = LoadResource(libHTMLURI);

            var libMinURI = "Dynamo.LibraryViewExtensionMSWebBrowser.web.library.librarie.min.js";
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

            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            sidebarGrid.Children.Add(view);
            //register the interop object into the browser.
            view.Browser.ObjectForScripting = interop;
            //open the library ui page.
            view.Browser.NavigateToString(libraryHTMLPage);

            browser = view.Browser;
            browser.Loaded += Browser_Loaded;
            browser.SizeChanged += Browser_SizeChanged;
            LibraryViewController.SetupSearchModelEventsObserver(browser, dynamoViewModel.Model.SearchModel, this, this.customization);
        }



        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            string msg = "Browser Loaded";
            LogToDynamoConsole(msg);
        }

        //if the browser window itself is resized, toggle visibility to force redraw.
        private void Browser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (browser != null)
            {
                browser.InvalidateVisual();
            }
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

        internal static IDisposable SetupSearchModelEventsObserver(WebBrowser webview, NodeSearchModel model, LibraryViewController controller, ILibraryViewCustomization customization, int throttleTime = 200)
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
            var dllProvider = new DllResourceProvider("http://localhost/dist", "Dynamo.LibraryViewExtensionMSWebBrowser.web.library");
            iconProvider = new IconResourceProvider(model.PathManager, dllProvider, customization);
            nodeProvider = new NodeItemDataProvider(model.SearchModel, iconProvider);
            searchResultDataProvider = new SearchResultDataProvider(model.SearchModel, iconProvider);
            layoutProvider = new LayoutSpecProvider(customization, iconProvider, "Dynamo.LibraryViewExtensionMSWebBrowser.web.library.layoutSpecs.json");
        }

        /// <summary>
        /// Convenience method for logging to Dynamo Console.
        /// </summary>
        /// <param name="meessage"></param>
        internal void LogToDynamoConsole(string message)
        {
            this.dynamoViewModel.Model.Logger.Log(message);
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
                dynamoWindow = null;
            }
            if (this.browser != null)
            {
                browser.SizeChanged -= Browser_SizeChanged;
                browser.Loaded -= Browser_Loaded;
                browser.Dispose();
                browser = null;
            }
        }
    }
}

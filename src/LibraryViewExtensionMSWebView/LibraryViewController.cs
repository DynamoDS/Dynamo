using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Dynamo.Extensions;
using Dynamo.LibraryViewExtensionMSWebView.Handlers;
using Dynamo.LibraryViewExtensionMSWebView.ViewModels;
using Dynamo.LibraryViewExtensionMSWebView.Views;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dynamo.LibraryViewExtensionMSWebView
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisibleAttribute(true)]
    public class scriptingObject
    {
        private LibraryViewController controller;

        public scriptingObject(LibraryViewController controller)
        {
            this.controller = controller;
        }

        public void notify(string data)
        {
            scriptNotifyHandler(data);
        }

        public void getBase64StringFromPath (string iconurl)
        {
            System.Diagnostics.Debug.WriteLine(iconurl);
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
            controller.browser.InvokeScript("completeReplaceImages", $"data:image/{ext};base64, {iconAsBase64}");
        }

        private void scriptNotifyHandler(string dataFromjs)
        {

            if (dataFromjs == "requestUpdateLibrary")
            {
                //TODO why do we need to pass this ref.
                controller.refreshLibraryView(controller.browser);
                
            }
            //a more complex action needs to be taken on the c# side.
            else if (!string.IsNullOrEmpty(dataFromjs))
            {
                /*will be an object like:
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


        }
    }

    //TODO remove.
    internal class DebounceDispatcher
    {
        private DispatcherTimer timer;
        public void Debounce(int interval, Action<object> action,
            object param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher disp = null)
        {
            // kill pending timer and pending ticks
            timer?.Stop();
            timer = null;

            if (disp == null)
                disp = Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (timer == null)
                    return;

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            }, disp);

            timer.Start();
        }
    }

    public interface IEventController
    {
        void On(string eventName, object callback);
        void RaiseEvent(string eventName, params object[] parameters);
    }


    /// <summary>
    /// This class holds methods and data to be called from javascript
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
        private DebounceDispatcher uiDebouncer = new DebounceDispatcher();
        public static Stopwatch stopwatch = new Stopwatch();

        public static  DynamoLogger logger;

        /// <summary>
        /// Creates LibraryViewController
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
            LibraryViewController.logger = (dynamoWindow.DataContext as DynamoViewModel).Model.Logger;
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
                commandExecutive.ExecuteCommand(cmd, Guid.NewGuid().ToString(), LibraryViewExtensionMSWebView.ExtensionName);
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
        private string replaceUrlWithBase64Image(string html, string minifiedURL, bool magicreplace = true)
        {
            //TODO use string builder for better perf and memory.
            var ext = string.Empty;
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

        private readonly (string, bool)[] dynamicIconPaths = new (string, bool)[15]
        {
           ("/resources/library-create.svg",true),
           ("/resources/default-icon.svg",true),
           ("/resources/fontawesome-webfont.eot",true),
           //"/resources/fontawesome-webfont.svg",
           ("/resources/fontawesome-webfont.ttf",true),
           ("/resources/fontawesome-webfont.woff2",true),
           ("/resources/fontawesome-webfont.woff",true),
           ("/resources/library-action.svg",true),
           ("/resources/library-query.svg",true),
           ("/resources/indent-arrow-down-wo-lines.svg",true),
           ("/resources/indent-arrow-down.svg",true),
           ("/resources/indent-arrow-right-last.svg",true),
           ("/resources/indent-arrow-right-wo-lines.svg",true),
           ("/resources/indent-arrow-right.svg",true),
           ("/resources/ArtifaktElement-Bold.woff",true),
           ("/resources/ArtifaktElement-Regular.woff",true)
        };

        /// <summary>
        /// Creates and add the library view to the WPF visual tree
        /// </summary>
        /// <returns>LibraryView control</returns>
        internal void AddLibraryView()
        {
            //TODO this address does nothing now.
            LibraryViewModel model = new LibraryViewModel("http://localhost/library.html");
            LibraryView view = new LibraryView(model);

           
            stopwatch.Reset();
            stopwatch.Start();

            var lib_min_template = "LIBPLACEHOLDER";
            var libHTMLURI = "Dynamo.LibraryViewExtensionMSWebView.web.library.library.html";
            var stream = LoadResource(libHTMLURI);

            var libMinURI = "Dynamo.LibraryViewExtensionMSWebView.web.library.librarie.min.js";
            var libminstream = LoadResource(libMinURI);
            var libminstring = "LIBJS";
            var libraryHTMLPage = "HELLO WORLD";

 
            using (var reader = new StreamReader(libminstream))
            {
                libminstring = reader.ReadToEnd();
                dynamicIconPaths.ToList().ForEach(path =>
                {
                    libminstring = replaceUrlWithBase64Image(libminstring, path.Item1, path.Item2);
                });

            }

            using (var reader = new StreamReader(stream))
            {
                libraryHTMLPage = reader.ReadToEnd().Replace(lib_min_template, libminstring);
            }

            stopwatch.Stop();
            logger.LogError($"{stopwatch.ElapsedMilliseconds} to load html and js initially.");

            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            sidebarGrid.Children.Add(view);
            view.Browser.ObjectForScripting = interop;
            view.Browser.NavigateToString(libraryHTMLPage);

            browser = view.Browser;
            browser.Loaded += Browser_Loaded;
            browser.SizeChanged += Browser_SizeChanged;
            LibraryViewController.SetupSearchModelEventsObserver(browser, dynamoViewModel.Model.SearchModel, this, this.customization);
        }

        internal void refreshLibraryView(WebBrowser browser)
        {
            dynamoWindow.Dispatcher.BeginInvoke(
            new Action(() =>
           {
           
               var ext1 = string.Empty;
               var ext2 = string.Empty;

               var reader = new StreamReader(nodeProvider.GetResource(null, out ext1));
               //this is a json string now.
               var loadedTypes = reader.ReadToEnd();
               stopwatch.Restart();

               var reader2 = new StreamReader(layoutProvider.GetResource(null, out ext2));
               var layoutSpec = reader2.ReadToEnd();
               logger.LogError($"{stopwatch.ElapsedMilliseconds} to refresh library layoutSpec.");

               stopwatch.Reset();

               try
               {
                   browser.InvokeScript
                        ("refreshLibViewFromData", loadedTypes, layoutSpec);
               }
               catch (Exception e)
               {
                   MessageBox.Show(e.Message);
               }
               reader.Dispose();
               reader2.Dispose();

              

           }));

        }


      

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            // Attempt to load resources
            try
            {
                //   RegisterResources(this.browser);
                string msg = "Preparing to load the library resources.";
                this.dynamoViewModel.Model.Logger.Log(msg);
            }
            catch (Exception ex)
            {
                string error = "Failed to load the library resources." +
                    Environment.NewLine +
                    "Exception: " + ex.Message;
                this.dynamoViewModel.Model.Logger.LogError(error);
            }
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
                controller.refreshLibraryView(webview);
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

        private void InitializeResourceProviders(DynamoModel model, LibraryViewCustomization customization)
        {
         
            var dllProvider = new DllResourceProvider("http://localhost/dist", "Dynamo.LibraryViewExtensionMSWebView.web.library");
            iconProvider = new IconResourceProvider(model.PathManager, dllProvider, customization);
            nodeProvider = new NodeItemDataProvider(model.SearchModel, iconProvider);
            searchResultDataProvider = new SearchResultDataProvider(model.SearchModel, iconProvider);
            layoutProvider = new LayoutSpecProvider(customization, iconProvider, "Dynamo.LibraryViewExtensionMSWebView.web.library.layoutSpecs.json");
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
                //browser.ScriptNotify -= scriptNotifyHandler;
                browser.Dispose();
                browser = null;
            }          
        }
    }
}

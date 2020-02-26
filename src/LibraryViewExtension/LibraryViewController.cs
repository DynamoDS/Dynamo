using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.Extensions;
using Dynamo.LibraryUI.Handlers;
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.LibraryUI
{
    public interface IEventController
    {
        void On(string eventName, object callback);
        void RaiseEvent(string eventName, params object[] parameters);
    }

    public class EventController : IEventController
    {
        private object contextData = null;
        private Dictionary<string, List<IJavascriptCallback>> callbacks = new Dictionary<string, List<IJavascriptCallback>>();

        public void On(string eventName, object callback)
        {
            List<IJavascriptCallback> cblist;
            if (!callbacks.TryGetValue(eventName, out cblist))
            {
                cblist = new List<IJavascriptCallback>();
            }
            cblist.Add(callback as IJavascriptCallback);
            callbacks[eventName] = cblist;
        }

        [JavascriptIgnore]
        public void RaiseEvent(string eventName, params object[] parameters)
        {
            List<IJavascriptCallback> cblist;
            if (callbacks.TryGetValue(eventName, out cblist))
            {
                foreach (var cbfunc in cblist)
                {
                    if (cbfunc.CanExecute)
                    {
                        cbfunc.ExecuteAsync(parameters);
                    }
                }
            }
        }

        /// <summary>
        /// Gets details view context data, e.g. packageId if it shows details of a package
        /// </summary>
        public object DetailsViewContextData
        {
            get { return contextData; }
            set
            {
                contextData = value;
                this.RaiseEvent("detailsViewContextDataChanged", contextData);
            }
        }
    }

    /// <summary>
    /// This class holds methods and data to be called from javascript
    /// </summary>
    public class LibraryViewController : EventController, IDisposable
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DynamoViewModel dynamoViewModel;
        private FloatingLibraryTooltipPopup libraryViewTooltip;
        private ResourceHandlerFactory resourceFactory;
        private IDisposable observer;
        private ChromiumWebBrowser browser;
        private const string CreateNodeInstrumentationString = "Search-NodeAdded";
        // TODO remove this when we can control the library state from Dynamo more precisely.
        private bool disableObserver = false;

        /// <summary>
        /// Creates LibraryViewController
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        internal LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive, LibraryViewCustomization customization)
        {
            this.dynamoWindow = dynamoView;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            libraryViewTooltip = CreateTooltipControl();

            this.commandExecutive = commandExecutive;
            InitializeResourceStreams(dynamoViewModel.Model, customization);
            dynamoWindow.StateChanged += DynamoWindowStateChanged;
            dynamoWindow.SizeChanged += DynamoWindow_SizeChanged;
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
                commandExecutive.ExecuteCommand(cmd, Guid.NewGuid().ToString(), ViewExtension.ExtensionName);
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

        /// <summary>
        /// Creates and add the library view to the WPF visual tree
        /// </summary>
        /// <returns>LibraryView control</returns>
        internal void AddLibraryView()
        {
            LibraryViewModel model = new LibraryViewModel("http://localhost/library.html");
            LibraryView view = new LibraryView(model);
            view.Loaded += OnLibraryViewLoaded;

            // TODO: This needs to be more generic, e.g. Window.leftExtensionGrid.Add()
            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            sidebarGrid.Children.Add(view);

            browser = view.Browser;
            browser.RegisterAsyncJsObject("controller", this);

            browser.Loaded += Browser_Loaded;
            browser.SizeChanged += Browser_SizeChanged;
            browser.LoadError += Browser_LoadError;
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            // Attempt to load resources
            try
            {
                RegisterResources(this.browser);
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

        // Browser LoadError events occur when the resource load for a navigation fails or is canceled
        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("*****Chromium Browser Messages******");
            System.Diagnostics.Trace.Write(e.ErrorText);

#if DEBUG
            // TODO - The browser should not be loaded before the loadedTypesJson or layoutSpecsJson are fully loaded.
            // Since these assets get loaded via a Javascript function in the html there is no way to guarantee this without moving the logic.
            // A better strategy is required for preloading these assests before the browser attempts to initialize in order to prevent a reload.
            // Having long running javascript in the Library.html file is problematic as it doesn't complete before the C# layer continues to execute.

            // This error is expected to occur if the loadedTypesJson or layoutSpecsJson was not fully loaded
            // on the first loading attempt.  When the resources are ready the browser is refreshed/reloaded.
            // See this thread for more details: https://magpcss.org/ceforum/viewtopic.php?f=10&t=11507 
            if (e.ErrorText == "ERR_ABORTED")
            {
                this.dynamoViewModel.Model.Logger.LogError("The library browser has been reloaded.");
            }
            else
            {
                this.dynamoViewModel.Model.Logger.LogError(e.ErrorText);
            }
#endif
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

        private void OnLibraryViewLoaded(object sender, RoutedEventArgs e)
        {
            var libraryView = sender as LibraryView;
#if DEBUG
            var browser = libraryView.Browser;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
#endif
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("*****Chromium Browser Messages******");
            System.Diagnostics.Trace.Write(e.Message);
        }

        internal static IDisposable SetupSearchModelEventsObserver(NodeSearchModel model, IEventController controller, ILibraryViewCustomization customization, int throttleTime = 200)
        {
            customization.SpecificationUpdated += (o, e) => controller.RaiseEvent("libraryDataUpdated");

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

        private void InitializeResourceStreams(DynamoModel model, LibraryViewCustomization customization)
        {
            //TODO: Remove the parameter after testing.
            //For testing purpose.
            resourceFactory = new ResourceHandlerFactory(model.Logger);

            //Register the resource stream registered through the LibraryViewCustomization
            foreach (var item in customization.Resources)
            {
                OnResourceStreamRegistered(item.Key, item.Value);
            }

            //Setup the event handler for resource registration
            customization.ResourceStreamRegistered += OnResourceStreamRegistered;

            resourceFactory.RegisterProvider("/dist",
                new DllResourceProvider("http://localhost/dist",
                    "Dynamo.LibraryUI.web.library"));

            resourceFactory.RegisterProvider(IconUrl.ServiceEndpoint, new IconResourceProvider(model.PathManager));

            {
                var url = "http://localhost/library.html";
                var resource = "Dynamo.LibraryUI.web.library.library.html";
                var stream = LoadResource(resource);
                resourceFactory.RegisterHandler(url, ResourceHandler.FromStream(stream));
            }

            //Register provider for node data
            resourceFactory.RegisterProvider("/loadedTypes", new NodeItemDataProvider(model.SearchModel));

            //Register provider for layout spec
            resourceFactory.RegisterProvider("/layoutSpecs", new LayoutSpecProvider(customization, "Dynamo.LibraryUI.web.library.layoutSpecs.json"));

            //Setup the event observer for NodeSearchModel to update customization/spec provider.
            observer = SetupSearchModelEventsObserver(model.SearchModel, this, customization);

            //Register provider for searching node data
            resourceFactory.RegisterProvider(SearchResultDataProvider.serviceIdentifier, new SearchResultDataProvider(model.SearchModel));
        }

        private void OnResourceStreamRegistered(string key, Stream value)
        {
            Uri url = new Uri(key, UriKind.RelativeOrAbsolute);
            if (!url.IsAbsoluteUri)
                url = new Uri(new Uri("http://localhost"), url);

            var extension = Path.GetExtension(key);
            var handler = ResourceHandler.FromStream(value, ResourceHandler.GetMimeType(extension));
            resourceFactory.RegisterHandler(url.AbsoluteUri, handler);
        }

        private void RegisterResources(ChromiumWebBrowser browser)
        {
            browser.ResourceHandlerFactory = resourceFactory;
        }

        private void OnDescriptionViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var view = sender as DetailsView;
            var browser = view.Browser;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
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
                browser.LoadError -= Browser_LoadError;
                browser.Dispose();
                browser = null;
            }
        }
    }
}

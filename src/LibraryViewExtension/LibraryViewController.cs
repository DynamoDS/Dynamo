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
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels;
using Newtonsoft.Json;

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
    public class LibraryViewController : EventController
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DynamoViewModel dynamoViewModel;
        private FloatingLibraryTooltipPopup libraryViewTooltip;
        private ResourceHandlerFactory resourceFactory;
        private IDisposable observer;

        /// <summary>
        /// Creates LibraryViewController
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        public LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive, ILibraryViewCustomization customization)
        {
            this.dynamoWindow = dynamoView;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            libraryViewTooltip = CreateTooltipControl();

            this.commandExecutive = commandExecutive;
            InitializeResourceStreams(dynamoViewModel.Model, customization);
        }

        /// <summary>
        /// Call this method to create a new node in Dynamo canvas.
        /// </summary>
        /// <param name="nodeName">Node creation name</param>
        public void CreateNode(string nodeName)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                //Create the node of given item name
                var cmd = new DynamoModel.CreateNodeCommand(Guid.NewGuid().ToString(), nodeName, -1, -1, true, false);
                commandExecutive.ExecuteCommand(cmd, Guid.NewGuid().ToString(), ViewExtension.ExtensionName);
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
        /// Get data for showing item summary within library
        /// </summary>
        /// <param name="nodeName">Node creation name</param>
        public string GetItemSummaryData(string nodeName)
        {
            var node = dynamoViewModel.SearchViewModel.FindViewModelForNode(nodeName);
            var nodeData = new Dictionary<string, object>();
            var inputParams = new List<Dictionary<string, string>>();

            foreach(Tuple<string, string> inputParameter in node.InputParameters)
            {
                var param = new Dictionary<string, string>();
                param.Add("name", inputParameter.Item1);
                param.Add("type", inputParameter.Item2);
                inputParams.Add(param);
            }

            nodeData.Add("inputParameters", inputParams);
            nodeData.Add("outputParameters", node.OutputParameters);
            nodeData.Add("description", node.Description);

            return JsonConvert.SerializeObject(nodeData);
        }

        /// <summary>
        /// Creates and add the library view to the WPF visual tree
        /// </summary>
        /// <returns>LibraryView control</returns>
        internal LibraryView AddLibraryView()
        {
            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            var model = new LibraryViewModel("http://localhost/library.html");
            var view = new LibraryView(model);

            var browser = view.Browser;
            sidebarGrid.Children.Add(view);
            browser.RegisterJsObject("controller", this);
            RegisterResources(browser);

            view.Loaded += OnLibraryViewLoaded;
            return view;
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

            var tooltipPopup = new FloatingLibraryTooltipPopup(200){ Name = "libraryToolTipPopup",
                                    StaysOpen = true, AllowsTransparency = true, PlacementTarget = sidebarGrid };
            sidebarGrid.Children.Add(tooltipPopup);

            return tooltipPopup;
        }

        private NodeSearchElementViewModel FindTooltipContext(String nodeName)
        {
            return dynamoViewModel.SearchViewModel.FindViewModelForNode(nodeName);
        }

        private void ShowTooltip(String nodeName, double y)
        {
            var nseViewModel = FindTooltipContext(nodeName);
            if(nseViewModel == null)
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
            customization.SpecificationUpdated += (o,e) => controller.RaiseEvent("libraryDataUpdated");

            var observer = new EventObserver<NodeSearchElement, IEnumerable<NodeSearchElement>>(
                    elements => NotifySearchModelUpdate(customization, elements), CollectToList
                ).Throttle(TimeSpan.FromMilliseconds(throttleTime));

            Action<NodeSearchElement> onRemove = e => observer.OnEvent(null);

            //Set up the event callback
            model.EntryAdded += observer.OnEvent;
            model.EntryRemoved += onRemove;
            model.EntryUpdated += observer.OnEvent;

            //Set up the dispose event handler
            observer.Disposed += () =>
            {
                model.EntryAdded -= observer.OnEvent;
                model.EntryRemoved -= onRemove;
                model.EntryUpdated -= observer.OnEvent;
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
            var includes = elements
                .Select(NodeItemDataProvider.GetFullyQualifiedName)
                .Select(name => name.Split('.').First())
                .Distinct()
                .SkipWhile(s => s.Contains("://"))
                .Select(p => new LayoutIncludeInfo() { path = p });

            customization.AddIncludeInfo(includes, "Add-ons");
        }

        private void InitializeResourceStreams(DynamoModel model, ILibraryViewCustomization customization)
        {
            resourceFactory = new ResourceHandlerFactory();
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.Extensions;
using Dynamo.LibraryUI.Handlers;
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using System.Windows.Input;
using Dynamo.Wpf.ViewModels;
using Dynamo.Controls;
using Dynamo.Search;
using Dynamo.Search.SearchElements;

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
        private DetailsView detailsView;
        private DetailsViewModel detailsViewModel;
        private DynamoPackagesHelper packageHelper;
        private FloatingLibraryTooltipPopup libraryViewTooltip;
        private ResourceHandlerFactory resourceFactory;
        private IDisposable observer;

        /// <summary>
        /// Creates LibraryViewController
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        public LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive)
        {
            this.dynamoWindow = dynamoView;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            libraryViewTooltip = CreateTooltipControl();

            this.commandExecutive = commandExecutive;
            InitializeResourceStreams(dynamoViewModel.Model);
            this.observer = SetupSearchModelEventsObserver(dynamoViewModel.Model.SearchModel, this);

            packageHelper = new DynamoPackagesHelper(this, dynamoViewModel.Model);
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

        /// <summary>
        /// Displays the details view over Dynamo canvas.
        /// </summary>
        /// <param name="item">item data for which details need to be shown</param>
        public void ShowDetailsView(object data)
        {
            DetailsViewContextData = data;
            if (detailsView == null)
            {
                dynamoWindow.Dispatcher.BeginInvoke(new Action(() => AddDetailsView()));
            }
            else
            {
                dynamoWindow.Dispatcher.BeginInvoke(new Action(() => detailsView.Visibility = Visibility.Visible));
            }
        }

        /// <summary>
        /// Closes the details view
        /// </summary>
        public void CloseDetailsView()
        {
            if (detailsView != null)
            {
                dynamoWindow.Dispatcher.BeginInvoke(new Action(() => detailsView.Visibility = Visibility.Collapsed));
            }
        }

        /// <summary>
        /// Returns a JSON string of all the packages installed on the system.
        /// </summary>
        /// <returns>string representing JSON object.</returns>
        public string GetInstalledPackagesJSON()
        {
            return packageHelper.GetInstalledPackagesJSON();
        }

        /// <summary>
        /// Gets the version name for the given installed package.
        /// </summary>
        /// <param name="packageName">Name of the package</param>
        /// <returns>Returns version name of a given package if it is installed, else empty string</returns>
        public string GetInstalledPackageVersion(string packageName)
        {
            return packageHelper.GetInstalledPackageVersion(packageName);
        }

        /// <summary>
        /// Installs a dynamo package of given package id.
        /// </summary>
        /// <param name="name">name of the package to install</param>
        /// <param name="version">version of package to install</param>
        /// <param name="pkgId">package id</param>
        /// <param name="installPath">path to install</param>
        public void InstallPackage(string name, string version, string pkgId, string installPath)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() => packageHelper.DownlodAndInstall(pkgId, name, version, installPath)));
        }

        /// <summary>
        /// Uninstalls the given package
        /// </summary>
        /// <param name="packageName">Package name to uninstall</param>
        public void UninstallPackage(string packageName)
        {
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() => packageHelper.UninstallPackage(packageName)));
        }

        /// <summary>
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
        /// Creates and add the library view to the WPF visual tree
        /// </summary>
        /// <returns>LibraryView control</returns>
        internal LibraryView AddLibraryView()
        {
            var sidebarGrid = dynamoWindow.FindName("sidebarGrid") as Grid;
            var model = new LibraryViewModel("http://localhost/index.html");
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

        private DetailsView AddDetailsView()
        {
            detailsViewModel = new DetailsViewModel("http://localhost/details.html");

            var tabcontrol = dynamoWindow.FindName("WorkspaceTabs") as TabControl;
            var grid = tabcontrol.Parent as Grid;

            detailsView = new DetailsView(detailsViewModel, grid);
            grid.Children.Add(detailsView);

            var browser = detailsView.Browser;
            browser.RegisterJsObject("controller", this);
            RegisterResources(browser);
            detailsView.Loaded += OnDescriptionViewLoaded;

            return detailsView;
        }

        internal static IDisposable SetupSearchModelEventsObserver(NodeSearchModel model, IEventController controller, int throttleTime = 200)
        {
            var observer = new EventObserver<NodeSearchElement, string>(
                    nodes => controller.RaiseEvent("libraryDataUpdated", nodes),
                    (s, e) => {
                            var name = NodeItemDataProvider.GetFullyQualifiedName(e);
                            return string.IsNullOrEmpty(s) ? name : string.Format("{0}, {1}", s, name);
                        }
                ).Throttle(TimeSpan.FromMilliseconds(throttleTime));

            //Set up the event callback
            model.EntryAdded += observer.OnEvent;
            model.EntryRemoved += observer.OnEvent;
            model.EntryUpdated += observer.OnEvent;

            //Set up the dispose event handler
            observer.Disposed += () =>
            {
                model.EntryAdded -= observer.OnEvent;
                model.EntryRemoved -= observer.OnEvent;
                model.EntryUpdated -= observer.OnEvent;
            };

            return observer;
        }

        private void InitializeResourceStreams(DynamoModel model)
        {
            resourceFactory = new ResourceHandlerFactory();
            resourceFactory.RegisterProvider("/dist/v0.0.1", 
                new DllResourceProvider("http://localhost/dist/v0.0.1",
                    "Dynamo.LibraryUI.web.library"));

            resourceFactory.RegisterProvider(IconUrl.ServiceEndpoint, new IconResourceProvider(model.PathManager));

            //Register provider for node data
            resourceFactory.RegisterProvider("/loadedTypes", new NodeItemDataProvider(model.SearchModel));

            resourceFactory.RegisterProvider("/dist/resources",
                new DllResourceProvider("http://localhost/dist/resources",
                    "Dynamo.LibraryUI.web.packageManager.resources"));

            resourceFactory.RegisterProvider("/dist/resources/fonts",
                new DllResourceProvider("http://localhost/dist/resources/fonts",
                    "Dynamo.LibraryUI.web.fonts"));

            resourceFactory.RegisterProvider("/dist/resources/fonts/font-awesome-4.7.0",
                new DllResourceProvider("http://localhost/dist/resources/fonts/font-awesome-4.7.0",
                    "Dynamo.LibraryUI.web.fonts.font_awesome_4._7._0"));

            var packageProvider = new DynamoPackagesProvider(this, model);
            resourceFactory.RegisterProvider("/packages", packageProvider);
            resourceFactory.RegisterProvider("/package", packageProvider);

            //Register static resources
            RegisterDllResourceHandler("/index.html", "index.html");
            RegisterDllResourceHandler("/layoutSpecs", "library.layoutSpecs.json");
            RegisterDllResourceHandler("/details.html", "details.html");
            RegisterDllResourceHandler("/dist/bundle.js", "packageManager.bundle.js");
        }

        private void RegisterDllResourceHandler(string url, string resource)
        {
            const string localhost = "http://localhost";
            const string web = "Dynamo.LibraryUI.web.";
            var fullurl = localhost + url;
            var resourceName = web + resource;
            var stream = LoadResource(resourceName);
            resourceFactory.RegisterHandler(fullurl, ResourceHandler.FromStream(stream));
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

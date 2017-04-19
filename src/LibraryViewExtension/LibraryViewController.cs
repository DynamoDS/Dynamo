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
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;

namespace Dynamo.LibraryUI
{
    public interface IEventController
    {
        void On(string eventName, object callback);
        void RaiseEvent(string eventName, params object[] parameters);
    }

    /// <summary>
    /// This class holds methods and data to be called from javascript
    /// </summary>
    public class LibraryViewController : IEventController
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DetailsView detailsView;
        private DetailsViewModel detailsViewModel;
        private DynamoPackagesHelper packageHelper;

        private Dictionary<string, Stream> resourceStreams = new Dictionary<string, Stream>();
        private Dictionary<string, object> callbacks = new Dictionary<string, object>();
        
        /// <summary>
        /// Creates LibraryViewController
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        public LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive)
        {
            this.dynamoWindow = dynamoView;
            var dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            var dynamoModel = dynamoViewModel.Model;
            packageHelper = new DynamoPackagesHelper(this, dynamoModel);

            this.commandExecutive = commandExecutive;
            InitializeResourceStreams();
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
        public void ShowDetailsView(string item)
        {
            DetailsViewContextData = item;
            RaiseEvent("detailsViewContextDataChanged", item);
            if(detailsView == null)
            {
                dynamoWindow.Dispatcher.BeginInvoke(new Action(() => AddDetailsView(item)));
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
            dynamoWindow.Dispatcher.BeginInvoke(new Action(() => detailsView.Visibility = Visibility.Collapsed));
        }

        public void On(string eventName, object callback)
        {
            callbacks.Add(eventName, callback);
        }

        [JavascriptIgnore]
        public void RaiseEvent(string eventName, params object[] parameters)
        {
            object callback = null;
            if(callbacks.TryGetValue(eventName, out callback))
            {
                var cbfunc = callback as IJavascriptCallback;
                if(cbfunc.CanExecute)
                {
                    cbfunc.ExecuteAsync(parameters);
                }
            }
        }

        /// <summary>
        /// Gets details view context data, e.g. packageId if it shows details of a package
        /// </summary>
        public string DetailsViewContextData { get; set; }

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
        
        private Stream LoadResource(string url)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var textStream = assembly.GetManifestResourceStream(url);
            return textStream;
        }

        private void OnLibraryViewLoaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var view = sender as LibraryView;
            var browser = view.Browser;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
#endif
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("*****Chromium Browser Messages******");
            System.Diagnostics.Trace.Write(e.Message);
        }

        private DetailsView AddDetailsView(string item)
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

        private void InitializeResourceStreams()
        {
            var rootnamespace = "Dynamo.LibraryUI.web.";
            var resourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames();
            foreach (var resource in resourceNames)
            {
                if (!resource.StartsWith(rootnamespace))
                    continue;

                var url = resource.Replace(rootnamespace, "");
                if (url.StartsWith("dist."))
                {
                    url = url.Replace("dist.", "dist/");
                    url = url.Replace("/v0._0._1.", "/v0.0.1/");
                    url = url.Replace("/resources.", "/resources/");
                    url = url.Replace("/icons.", "/icons/");
                    url = url.Replace(".font_awesome_4._7._0.", "/font-awesome-4.7.0/");
                    url = url.Replace("/fonts.", "/fonts/");
                    url = url.Replace("less.", "less/");
                    url = url.Replace("css.", "css/");
                }
                else
                    url = url.Replace("package.", "package/");

                if (url.EndsWith(".json"))
                {
                    url = url.Replace(".json", "");
                }
                var r = LoadResource(resource);
                resourceStreams.Add("http://localhost/" + url, r);
            }
        }

        private void RegisterResources(ChromiumWebBrowser browser)
        {
            var factory = (DefaultResourceHandlerFactory)(browser.ResourceHandlerFactory);
            if (factory == null) return;

            foreach (var pair in resourceStreams)
            {
                var url = pair.Key;
                var idx = url.LastIndexOf('.');
                var mime = "text/html";
                if(idx > 0)
                {
                    mime = ResourceHandler.GetMimeType(url.Substring(idx));
                }
                
                factory.RegisterHandler(url, ResourceHandler.FromStream(pair.Value, mime));
            }
        }

        private void OnDescriptionViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var view = sender as DetailsView;
            var browser = view.Browser;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
        }
    }
}

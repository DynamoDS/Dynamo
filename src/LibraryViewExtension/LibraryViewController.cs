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

namespace Dynamo.LibraryUI
{
    /// <summary>
    /// This class holds methods and data to be called from javascript
    /// </summary>
    public class LibraryViewController
    {
        private Window dynamoWindow;
        private ICommandExecutive commandExecutive;
        private DetailsView detailsView;
        private DetailsViewModel detailsViewModel;
        
        /// <summary>
        /// Creates LibraryViewController
        /// </summary>
        /// <param name="dynamoView">DynamoView hosting library component</param>
        /// <param name="commandExecutive">Command executive to run dynamo commands</param>
        public LibraryViewController(Window dynamoView, ICommandExecutive commandExecutive)
        {
            this.dynamoWindow = dynamoView;
            this.commandExecutive = commandExecutive;
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

        /// <summary>
        /// Gets details view context data, e.g. packageId if it shows details of a package
        /// </summary>
        public string DetailsViewContextData { get; set; }
        
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

        /// <summary>
        /// Returns a mime type based on the path extension 
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <returns>mime type string</returns>
        private string MimeType(string path)
        {
            var idx = path.LastIndexOf('.');
            var ext = path.Substring(idx);
            switch (ext)
            {
                case "svg":
                    return "text/svg";
                case "png":
                    return "image/png";
                case "js":
                    return "text/javascript";
                default:
                    return "text/html";
            }
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

        private void RegisterResources(ChromiumWebBrowser browser)
        {
            var rootnamespace = "Dynamo.LibraryUI.web.";

            var factory = (DefaultResourceHandlerFactory)(browser.ResourceHandlerFactory);
            if (factory == null) return;

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
                    url = url.Replace("/fonts.", "/fonts/");
                }
                var mime = MimeType(url);

                if (url.EndsWith(".json"))
                {
                    var idx = url.LastIndexOf('.');
                    url = url.Substring(0, idx);
                }
                var r = LoadResource(resource);
                factory.RegisterHandler("http://localhost/" + url,
                     ResourceHandler.FromStream(r, mime));
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

using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.LibraryUI.ViewModels;
using Dynamo.LibraryUI.Views;
using Dynamo.Wpf.Extensions;

namespace Dynamo.LibraryUI
{
    public class ViewExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private ViewStartupParams viewStartupParams;

        private LibraryViewModel model;

        public string UniqueId
        {
            get { return "85941358-5525-4FF4-8D61-6CA831F122AB"; }
        }

        public static readonly string ExtensionName = "LibraryUI";

        public string Name
        {
            get { return ExtensionName; }
        }

        public void Startup(ViewStartupParams p)
        {
            viewStartupParams = p;
        }
        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            AddLibraryView(p);
        }

        private void AddLibraryView(ViewLoadedParams p)
        {
            var sidebarGrid = p.DynamoWindow.FindName("sidebarGrid") as Grid;
            model = new LibraryViewModel("http://localhost/library.html");
            var view = new LibraryView(model);
            var browser = view.Browser;
            var rootnamespace = "Dynamo.LibraryUI.web.";

            sidebarGrid.Children.Add(view);
            view.Browser.RegisterJsObject("controller", new LibraryViewController(browser, p.CommandExecutive));
            var factory = (DefaultResourceHandlerFactory)(browser.ResourceHandlerFactory);
            if (factory == null) return;

            var resourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames();
            foreach (var resource in resourceNames)
            {
                if (!resource.StartsWith(rootnamespace))
                    continue;

                var url = resource.Replace(rootnamespace, "");
                if(url.StartsWith("dist."))
                {
                    url = url.Replace("dist.v0._0._1.", "dist/v0.0.1/");
                    url = url.Replace("v0.0.1/resources.", "v0.0.1/resources/");
                }

                var r = LoadResource(resource);                
                factory.RegisterHandler("http://localhost/" + url,
                     ResourceHandler.FromStream(r, MimeType(url)));
            }

            view.Loaded += OnLibraryViewLoaded;
        }

        private string MimeType(string url)
        {
            var idx = url.LastIndexOf('.');
            var ext = url.Substring(idx);
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

        private void OnLibraryViewLoaded(object sender, System.Windows.RoutedEventArgs e)
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

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {

        }

    }
}
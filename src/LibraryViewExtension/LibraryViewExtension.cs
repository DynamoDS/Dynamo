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

        public string Name
        {
            get { return "LibraryUI"; }
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
            model = new LibraryViewModel("http://54.169.171.233:3456/");
            sidebarGrid.Children.Add(new LibraryView(model));
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
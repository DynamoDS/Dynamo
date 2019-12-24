using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryViewExtensionMSWebView
{
    /// <summary>
    /// This extension duplicates many of the types in the CEF based LibraryViewExtension
    /// but is based
    /// </summary>
    public class LibraryViewExtensionMSWebView : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private ViewStartupParams viewStartupParams;
        private LibraryViewCustomization customization = new LibraryViewCustomization();
        private LibraryViewController controller;

        public string UniqueId
        {
            get { return "63cd0755-4a36-4670-ae89-b68e772633c4"; }
        }

        public static readonly string ExtensionName = "LibraryUI2";

        public string Name
        {
            get { return ExtensionName; }
        }

        public void Startup(ViewStartupParams p)
        {
            viewStartupParams = p;
            p.ExtensionManager.RegisterService<ILibraryViewCustomization>(customization);
        }

        public void Loaded(ViewLoadedParams p)
        {
            if (!DynamoModel.IsTestMode)
            {
                viewLoadedParams = p;
                controller = new LibraryViewController(p.DynamoWindow, p.CommandExecutive, customization);
                controller.AddLibraryView();
            }
            (p.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += handleDynamoViewPropertyChanges;
        }

        private void handleDynamoViewPropertyChanges(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           if(e.PropertyName == "ShowStartPage")
            {
                var sp = (sender as DynamoViewModel).ShowStartPage;
                var vis = sp == true ? Visibility.Hidden : Visibility.Visible;
                if(controller.browser != null)
                {
                    controller.browser.Visibility = vis;
                }
            }
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (controller != null) controller.Dispose();
            if (customization != null) customization.Dispose();

            customization = null;
            controller = null;
        }

    }
}

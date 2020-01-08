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

namespace Dynamo.LibraryViewExtensionMSWebBrowser
{
    /// <summary>
    /// This extension duplicates many of the types in the CEF based LibraryViewExtension
    /// but is based on MSWebBrowser control from system.windows to avoid conflicts with CEF and CEFSharp.
    /// </summary>
    public class LibraryViewExtensionMSWebBrowser : IViewExtension
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
                (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += handleDynamoViewPropertyChanges;
            }
            
        }

        //hide browser directly when startpage is shown to deal with air space problem.
        //https://github.com/dotnet/wpf/issues/152
        private void handleDynamoViewPropertyChanges(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           DynamoViewModel senderDVM = sender as DynamoViewModel;
           
           if (senderDVM!= null && e.PropertyName == nameof(senderDVM.ShowStartPage))
            {
                var sp = senderDVM.ShowStartPage;
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
            if(viewLoadedParams != null && viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel != null)
            {
                (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged -= handleDynamoViewPropertyChanges;
            }
          

            customization = null;
            controller = null;
        }

    }
}

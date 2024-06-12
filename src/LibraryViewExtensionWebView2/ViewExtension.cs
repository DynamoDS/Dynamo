using System.Windows;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryViewExtensionWebView2
{
    public class LibraryViewExtensionWebView2 : IViewExtension
    {
        /// <summary>
        /// Possible hosting contexts for the library. These represent
        /// Dynamo home and custom workspaces.
        /// </summary>
        private enum HostingContextType { home,custom, none};

        private ViewLoadedParams viewParams;
        private LibraryViewCustomization customization = new LibraryViewCustomization();
        private LibraryViewController controller;

        public string UniqueId
        {
            get { return "8b093acd-5656-4914-b6b0-c54b26ca6d0e"; }
        }

        public static readonly string ExtensionName = "LibraryUI - WebView2";

        public string Name
        {
            get { return ExtensionName; }
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            viewStartupParams.ExtensionManager.RegisterService<ILibraryViewCustomization>(customization);
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (!DynamoModel.IsTestMode)
            {
                viewParams = viewLoadedParams;
                controller = new LibraryViewController(viewLoadedParams.DynamoWindow, viewLoadedParams.CommandExecutive, customization);
                (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += handleDynamoViewPropertyChanges;
            }
            viewLoadedParams.CurrentWorkspaceChanged += ViewParams_CurrentWorkspaceChanged;
        }

        private void ViewParams_CurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            var type = workspace switch
            {
                HomeWorkspaceModel _ => HostingContextType.home.ToString(),
                CustomNodeWorkspaceModel _ => HostingContextType.custom.ToString()
            };
            controller.UpdateContext(type);
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
            // Do nothing for now
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
            if(viewParams != null && viewParams.DynamoWindow.DataContext as DynamoViewModel != null)
            {
                (viewParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged -= handleDynamoViewPropertyChanges;
            }
            if (viewParams != null)
            {
                viewParams.CurrentWorkspaceChanged -= ViewParams_CurrentWorkspaceChanged;
            }

            customization = null;
            controller = null;
        }

    }
}

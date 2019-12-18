using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;

namespace LibraryViewExtensionMSWebView
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

        public static readonly string ExtensionName = "LibraryUI";

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
            //TODO this is unfortunate but we need to delay this a bit
            //or sharpdx seems to run into problems. We really want to wait
            //until DynamoView is completely loaded.
            Task.Delay(5000).ContinueWith((t) =>
            {
                if (!DynamoModel.IsTestMode)
                {
                    viewLoadedParams = p;
                    controller = new LibraryViewController(p.DynamoWindow, p.CommandExecutive, customization);
                    controller.AddLibraryView();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

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

    public static class DynamoModelExtensions
    {
        public static PackageManagerExtension GetPackageManagerExtension(this DynamoModel model)
        {
            return PackageManager.DynamoModelExtensions.GetPackageManagerExtension(model);
        }
    }
}

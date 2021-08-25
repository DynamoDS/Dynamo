using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryUI
{
    public class ViewExtension : IViewExtension
    {
        private LibraryViewCustomization customization = new LibraryViewCustomization();
        private LibraryViewController controller;

        public string UniqueId
        {
            get { return "85941358-5525-4FF4-8D61-6CA831F122AB"; }
        }

        public static readonly string ExtensionName = "LibraryUI";

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
                controller = new LibraryViewController(viewLoadedParams.DynamoWindow, viewLoadedParams.CommandExecutive, customization);
                controller.AddLibraryView();
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
using Dynamo.PackageManager.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewExtension : ViewExtensionBase
    {
        public override void Dispose()
        {
            
        }

        private const string EXTENSION_NAME = "Package Details";
        private const string EXTENSION_GUID = "C71CA1B9-BF9F-425A-A12C-53DF56770406";

        private ViewLoadedParams viewLoadedParamsReference;
        public override string UniqueId => EXTENSION_GUID;
        public override string Name => EXTENSION_NAME;

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            viewLoadedParams.ViewExtensionOpenRequestWithParameter += OnViewExtensionOpenWithParameterRequest;
        }

        private void OnViewExtensionOpenWithParameterRequest(string extensionName, object obj)
        {
            if (extensionName != Name ||
                !(obj is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            // We need to clean up the event handlers when switching the PackageDetailsViewModel.
            // Private VM property, every time this is called we check if hte propety is null, if not do 
            // all fo the required cleanup and then creat itagain.

            PackageDetailsViewModel packageDetailsViewModel = new PackageDetailsViewModel(packageManagerSearchElementViewModel);
            PackageDetailsView packageDetailsView = new PackageDetailsView(packageDetailsViewModel);
            
            viewLoadedParamsReference?.AddToExtensionsSideBar(this, packageDetailsView);
        }


        public override void Shutdown()
        {
            viewLoadedParamsReference.ViewExtensionOpenRequestWithParameter -= OnViewExtensionOpenWithParameterRequest;
        }
    }
}

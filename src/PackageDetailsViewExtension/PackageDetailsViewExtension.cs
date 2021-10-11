using System.Collections.Generic;
using System.Linq;
using Dynamo.PackageManager;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Package Details";
        private const string EXTENSION_GUID = "C71CA1B9-BF9F-425A-A12C-53DF56770406";

        public PackageManagerExtension PackageManagerExtension { get; set; }
        public PackageDetailsView PackageDetailsView { get; set; }
        public PackageDetailsViewModel PackageDetailsViewModel { get; set; }
        private PackageManagerClientViewModel packageManagerClientViewModel;

        public DelegateCommand OpenPackageDetailsCommand { get; set; }

        private ViewLoadedParams viewLoadedParamsReference;
        public string UniqueId => EXTENSION_GUID;
        public string Name => EXTENSION_NAME;

        public void Startup(ViewStartupParams viewStartupParams)
        {
            var packageManager = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            this.PackageManagerExtension = packageManager;
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            viewLoadedParamsReference = viewLoadedParams;
            viewLoadedParams.ViewExtensionOpenRequestWithParameter += OnViewExtensionOpenWithParameterRequest;
            OpenPackageDetailsCommand = new DelegateCommand(OpenPackageDetails);

            DynamoViewModel dynamoViewModel = viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            this.packageManagerClientViewModel = dynamoViewModel.PackageManagerClientViewModel;
        }

        private void OnViewExtensionOpenWithParameterRequest(string extensionName, object obj)
        {
            if (extensionName != Name ||
                !(obj is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            // We need to clean up the event handlers when switching the PackageDetailsViewModel.
            // Private VM property, every time this is called we check if hte propety is null, if not do 
            // all fo the required cleanup and then creat itagain.

            OpenPackageDetailsCommand.Execute(packageManagerSearchElementViewModel);
        }
        
        public void Shutdown()
        {
            viewLoadedParamsReference.ViewExtensionOpenRequestWithParameter -= OnViewExtensionOpenWithParameterRequest;
        }

        public void Dispose()
        {

        }

        private void OpenPackageDetails
        (
            object obj
        )
        {
            if (!(obj is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            this.PackageDetailsViewModel = new PackageDetailsViewModel
            (
                    this.PackageManagerExtension.PackageLoader,
                    this.packageManagerClientViewModel,
                    packageManagerSearchElementViewModel
            );
            this.PackageDetailsView = new PackageDetailsView(this.PackageDetailsViewModel);
            
            viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.PackageDetailsView);
        }
    }
}

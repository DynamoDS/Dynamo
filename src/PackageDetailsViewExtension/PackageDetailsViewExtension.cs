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
        public PackageManagerClientViewModel packageManagerClientViewModel;

        public DelegateCommand OpenPackageDetailsCommand { get; set; }

        private ViewLoadedParams viewLoadedParamsReference;
        public string UniqueId => EXTENSION_GUID;
        public string Name => EXTENSION_NAME;

        public void Startup(ViewStartupParams viewStartupParams)
        {
            var packageManager = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            PackageManagerExtension = packageManager;
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            viewLoadedParamsReference = viewLoadedParams;
            viewLoadedParams.ViewExtensionOpenRequestWithParameter += OnViewExtensionOpenWithParameterRequest;
            OpenPackageDetailsCommand = new DelegateCommand(OpenPackageDetails);

            DynamoViewModel dynamoViewModel = viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            packageManagerClientViewModel = dynamoViewModel.PackageManagerClientViewModel;
        }

        private void OnViewExtensionOpenWithParameterRequest(string extensionName, object obj)
        {
            if (extensionName != Name ||
                !(obj is PackageManagerSearchElement packageManagerSearchElement)) return;

            OpenPackageDetailsCommand.Execute(packageManagerSearchElement);
        }
        
        public void Shutdown()
        {
            viewLoadedParamsReference.ViewExtensionOpenRequestWithParameter -= OnViewExtensionOpenWithParameterRequest;
        }

        public void Dispose()
        {

        }

        private void OpenPackageDetails(object obj)
        {
            if (!(obj is PackageManagerSearchElement packageManagerSearchElement)) return;

            PackageDetailsViewModel = new PackageDetailsViewModel(this, packageManagerSearchElement);
            
            if (PackageDetailsView == null) PackageDetailsView = new PackageDetailsView(PackageDetailsViewModel);
            PackageDetailsView.DataContext = PackageDetailsViewModel;

            viewLoadedParamsReference?.AddToExtensionsSideBar(this, PackageDetailsView);
        }
    }
}

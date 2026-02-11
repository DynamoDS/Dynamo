using Dynamo.PackageManager;
using Dynamo.Tests;
using Dynamo.ViewModels;
using Greg;
using Moq;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests.PackageManager
{
    internal class PackageManagerViewModelTests : SystemTestBase
    {
        /// <summary>
        /// Assures all separate view models and fields are loaded when initializing the PackageManagerViewModel
        /// </summary>
        [Test]
        public void PackageManagerLoadAllViewModelsTests()
        {
            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<Dynamo.PackageManager.PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty, false);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true }; ;

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            var packageManagerViewModel = new PackageManagerViewModel(ViewModel, packageManagerSearchViewModel);

            var filters = packageManagerViewModel.Filters;
            var localPackages = packageManagerViewModel.LocalPackages;
            var preferenceViewModel = packageManagerViewModel.PreferencesViewModel;
            var publishPackageViewModel = packageManagerViewModel.PublishPackageViewModel;

            Assert.IsNotNull(filters);
            Assert.IsNotNull(localPackages);
            Assert.IsNotNull(preferenceViewModel);
            Assert.IsNotNull(publishPackageViewModel);
        }
    }
}

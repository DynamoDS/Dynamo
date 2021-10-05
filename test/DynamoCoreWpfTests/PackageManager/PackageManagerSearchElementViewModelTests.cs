using SystemTestServices;
using Dynamo.PackageManager.ViewModels;
using Moq;
using NUnit.Framework;
using Greg.Responses;
using Greg;
using Dynamo.Tests;
using Dynamo.ViewModels;

namespace Dynamo.PackageManager.Wpf.Tests
{
    class PackageManagerSearchElementViewModelTests : SystemTestBase
    {
        /// <summary>
        /// A test to ensure the CanInstall property of a package updates correctly.
        /// </summary>
        [Test]
        public void TestPackageManagerSearchElementCanInstall()
        {
            var name1 = "non-duplicate";
            var name2 = "duplicate";
            var version = "1.0.0";

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object);

            var ext = Model.GetPackageManagerExtension();
            var loader = ext.PackageLoader;
            loader.Add(new Package("", name2, version, ""));

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            var newSE1 = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
            {
                name = name1
            }), false);

            var newSE2 = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
            {
                name = name2
            }), false);

            packageManagerSearchViewModel.AddToSearchResults(newSE1);
            packageManagerSearchViewModel.AddToSearchResults(newSE2);

            // Default CanInstall should be true
            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(true, newSE2.CanInstall);

            var dHandle1 = new PackageDownloadHandle()
            {
                Id = name1,
                VersionName = version,
                Name = name1
            };

            var dHandle2 = new PackageDownloadHandle()
            {
                Id = name2,
                VersionName = version,
                Name = name2
            };

            pmCVM.Object.Downloads.Add(dHandle1);
            pmCVM.Object.Downloads.Add(dHandle2);

            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(true, newSE2.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Downloading;
            Assert.AreEqual(false, newSE1.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Downloaded;
            Assert.AreEqual(false, newSE1.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Error;
            dHandle2.DownloadState = PackageDownloadHandle.State.Downloading;

            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Installing;
            dHandle2.DownloadState = PackageDownloadHandle.State.Downloaded;
            Assert.AreEqual(false, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            // Simulate that the package corresponding to name1 was added successfully
            var package1 = new Package("", name1, version, "") { };
            package1.SetAsLoaded();
            loader.Add(package1);

            dHandle1.DownloadState = PackageDownloadHandle.State.Installed;
            dHandle2.DownloadState = PackageDownloadHandle.State.Installed;

            Assert.AreEqual(false, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            packageManagerSearchViewModel.ClearSearchResults();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SystemTestServices;
using Dynamo.Controls;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Wpf.Utilities;
using Moq;
using NUnit.Framework;
using Greg.Responses;
using Greg;
using Greg.Requests;
using Dynamo.Tests;

namespace Dynamo.PackageManager.Wpf.Tests
{
    class PackageManagerSearchElementViewModelTests : SystemTestBase
    {
        /// <summary>
        /// A test to ensure the IsInstalled property of a package updates correctly once it is installed.
        /// </summary>
        [Test]
        public void TestPackageManagerSearchElementIsInstalled()
        {
            var id = "test-123";
            var deps = new List<Dependency>() { new Dependency() { _id = id, name = "Foo" } };
            var depVers = new List<string>() { "1.0.0" };
            var pkgHeader = new PackageVersion() {
                version = "1.0.0",
                engine_version = "2.3.0",
                name = "test-123",
                id = "test-123",
                full_dependency_ids = deps,
                full_dependency_versions = depVers
            };

            var mockGreg = new Mock<IGregClient>();
            mockGreg.Setup(m => m.ExecuteAndDeserializeWithContent<PackageVersion>(It.IsAny<Request>()))
            .Returns(new ResponseWithContentBody<PackageVersion>()
            {
                content = pkgHeader,
                success = true
            });

            var client = new PackageManagerClient(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);

            // Arrange
            PackageManagerSearchViewModel packageManagerSearchViewModel = new PackageManagerSearchViewModel(new Dynamo.ViewModels.PackageManagerClientViewModel(ViewModel, client));
            packageManagerSearchViewModel.SearchResults = new System.Collections.ObjectModel.ObservableCollection<PackageManagerSearchElementViewModel>() { };

            var newSE = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
            {
                name = "test-123",
                versions = new List<PackageVersion>()
                {
                    pkgHeader
                }
            }), false, true);
            packageManagerSearchViewModel.SearchResults.Add(newSE);
            packageManagerSearchViewModel.SearchState = PackageManagerSearchViewModel.PackageSearchState.Results;


            newSE.RequestDownload += packageManagerSearchViewModel.PackageOnExecuted;

            // Necessary to dismiss any MessageBox which appears during the installation..
            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            // Act
            Assert.AreEqual(true, newSE.CanInstall);
            newSE.DownloadLatestCommand.Execute(null);
            
            // Assert
            Assert.AreEqual(false, newSE.CanInstall);
        }
    }
}

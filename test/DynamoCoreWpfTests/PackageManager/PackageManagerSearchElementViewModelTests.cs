using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SystemTestServices;
using Dynamo.Controls;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Wpf.Utilities;
using Moq;
using NUnit.Framework;

namespace Dynamo.PackageManager.Wpf.Tests
{
    class PackageManagerSearchElementViewModelTests : SystemTestBase
    {
        /// <summary>
        /// A test to ensure the IsInstalled property of a package updates correctly once it is installed.
        /// </summary>
        [Test, Category("Failure")]
        public void TestPackageManagerSearchElementIsInstalled()
        {
            // Arrange
            PackageManagerSearchViewModel packageManagerSearchViewModel = new PackageManagerSearchViewModel(ViewModel.PackageManagerClientViewModel);
            packageManagerSearchViewModel.RefreshAndSearchAsync();

            List<PackageManagerSearchElement> packageManagerSearchElements = ViewModel.PackageManagerClientViewModel.ListAll();

            PackageManagerSearchElementViewModel packageManagerSearchElementViewModel = new PackageManagerSearchElementViewModel
            (
                packageManagerSearchElements.First(),
                false,
                packageManagerSearchViewModel.UserAlreadyHasPackageInstalled
            );

            packageManagerSearchElementViewModel.RequestDownload += packageManagerSearchViewModel.PackageOnExecuted;

            // Necessary to dismiss any MessageBox which appears during the installation..
            var dlgMock = new Mock<MessageBoxService.IMessageBox>();
            dlgMock.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), It.Is<MessageBoxButton>(x => x == MessageBoxButton.OKCancel || x == MessageBoxButton.OK), It.IsAny<MessageBoxImage>()))
                .Returns(MessageBoxResult.OK);
            MessageBoxService.OverrideMessageBoxDuringTests(dlgMock.Object);

            // Act
            Assert.AreEqual(false, packageManagerSearchElementViewModel.IsInstalled);
            packageManagerSearchElementViewModel.DownloadLatestCommand.Execute(null);
            
            // Assert
            Assert.AreEqual(true, packageManagerSearchElementViewModel.IsInstalled);
        }
    }
}

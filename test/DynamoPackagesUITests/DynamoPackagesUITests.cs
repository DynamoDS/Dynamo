using Dynamo.DynamoPackagesUI;
using Dynamo.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemTestServices;
using System.Windows;
using DynamoPackagesUI.Views;
using Dynamo.DynamoPackagesUI.Utilities;
using Dynamo.PackageManager;
using System.Reflection;
using Moq;
using Dynamo.DynamoPackagesUI.ViewModels;
using Newtonsoft.Json;
using Dynamo.Wpf.Extensions;

namespace DynamoPackagesUITests
{
    [TestFixture]
    public class DynamoPackagesUITests : SystemTestBase
    {
        string extensionsPath;

        private void AssertWindowOwnedByDynamoView<T>()
        {
            var windows = GetWindowEnumerable(View.OwnedWindows);
            Assert.AreEqual(1, windows.Count(x => x is T));

            var window = windows.FirstOrDefault(x => x is T);
            Assert.IsNotNull(window);

            Assert.IsTrue(window.Owner == (Window)View);
        }

        private IEnumerable<Window> GetWindowEnumerable(WindowCollection windows)
        {
            var enumerator = windows.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return (Window)enumerator.Current;
            }
        }

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "viewExtensions");
        }

        [Test]
        public void UIExtensionIsExtracted()
        {
            var extensionManager = new ExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);
            Assert.Greater(extensions.Count(), 0);

            Assert.AreEqual(extensions.OfType<Dynamo.DynamoPackagesUI.PackageManagerExtension>().Count(), 1);
        }

        [Test]
        public void UIExtensionsRendered()
        {
            var extensionManager = new ViewExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);
            var uiExtension = extensions.OfType<Dynamo.DynamoPackagesUI.PackageManagerExtension>().FirstOrDefault();

            //Initialize UI Extension
            var loadedParams = new ViewLoadedParams(View, ViewModel);
            var startupParams = new ViewStartupParams(ViewModel);
            uiExtension.Loaded(loadedParams);
            uiExtension.Startup(startupParams);

            //uiExtension.OnPackageManagerClick();
            //AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void InstallDynamoPackage()
        {
            var packageManagerCommands = new Mock<IPackageManagerCommands>();
            packageManagerCommands.Setup(t => t.Model).Returns(this.Model);
            packageManagerCommands.Setup(t => t.Loader).Returns(this.Model.GetPackageManagerExtension().PackageLoader);
            packageManagerCommands.Setup(t => t.LoadPackage(It.IsAny<Package>())).Callback<Package>((t) => {
                Assert.AreEqual(t.ID, "12343");
                Directory.Delete(t.RootDirectory, true);
            });

            string testDirectoryPath = Path.Combine(new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.FullName, "test");
            IPackageManagerMessageBox messageBox = new PackageManagerMessageBox();
            PackageManagerViewModel viewModel = new PackageManagerViewModel(packageManagerCommands.Object, messageBox, "assets");
            viewModel.DownloadRequest = JsonConvert.DeserializeObject(JsonConvert.SerializeObject((dynamic)new { asset_name = "test", asset_id = "12343" }));
            viewModel.InstallPackage(Path.Combine(testDirectoryPath, "pkgs", "TestPackage.zip"));

            packageManagerCommands.Verify(t => t.LoadPackage(It.IsAny<Package>()), Times.Once);
        }

        [Test]
        public void InstallPackageClick()
        {
            var packageManagerCommands = new Mock<IPackageManagerCommands>();
            packageManagerCommands.Setup(t => t.Model).Returns(this.Model);
            packageManagerCommands.Setup(t => t.Loader).Returns(this.Model.GetPackageManagerExtension().PackageLoader);

            var package = new { asset_id = "12343", asset_name = "test" };
            var version = new
            {
                contents = string.Empty,
                contains_binaries = false,
                node_libraries = new List<dynamic>(),
                dependencies = string.Empty,
                engine_version = "0.6.2.19362",
                url = @"https://s3.amazonaws.com/greg-pkgs-dev/a5710c62-80bf-4d8a-92b5-28e242df9f4fgregPkg261.zip",
                version = "2013.11.10",
                file_id = "12133"
            };
            var dictPackage = package.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(package, null));
            var dictVersion = version.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(version, null));
            
            
            //1. Confirm Install Package Message Box
            var packageManagerMessageBox = new Mock<IPackageManagerMessageBox>();
            packageManagerMessageBox.Setup(t => t.ShowConfirmToInstallPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.Cancel);

            PackageManagerViewModel viewModel = new PackageManagerViewModel(packageManagerCommands.Object, packageManagerMessageBox.Object, "assets");
            string returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "cancel");

            //2. Python Script Chaeck
            packageManagerMessageBox.Setup(t => t.ShowConfirmToInstallPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.OK);
            packageManagerMessageBox.Setup(t => t.ShowPackageContainPythonScript(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.Cancel);
            dictVersion["contains_binaries"] = true;
            returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "cancel");

            //3. Check Dynamo Version
            packageManagerMessageBox.Setup(t => t.ShowConfirmToInstallPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.OK);
            packageManagerMessageBox.Setup(t => t.ShowPackageContainPythonScript(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.OK);
            dictVersion["contains_binaries"] = true;
            returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "12343,12133,test");
        }

        [Test]
        public void UnInstallPackage()
        {
            var packageManagerCommands = new Mock<IPackageManagerCommands>();
            packageManagerCommands.Setup(t => t.Model).Returns(this.Model);
            packageManagerCommands.Setup(t => t.Loader).Returns(this.Model.GetPackageManagerExtension().PackageLoader);
            packageManagerCommands.Setup(t => t.LocalPackages).Returns(new List<Package> { { new Package("test", "test", "1.0.0", "MIT") } });

            var packageManagerMessageBox = new Mock<IPackageManagerMessageBox>();
            packageManagerMessageBox.Setup(t => t.ShowConfirmToUninstallPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.No);

            //Confirm UnInstall
            PackageManagerViewModel viewModel = new PackageManagerViewModel(packageManagerCommands.Object, packageManagerMessageBox.Object, "assets");
            viewModel.PkgRequest = JsonConvert.DeserializeObject(JsonConvert.SerializeObject((dynamic)new { asset_name = "test" }));
            bool returnValue = viewModel.Uninstall();
            Assert.AreEqual(false, returnValue, string.Empty);

            //UnInstall
            packageManagerMessageBox.Setup(t => t.ShowConfirmToUninstallPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).Returns(MessageBoxResult.Yes);
            returnValue = viewModel.Uninstall();
            packageManagerCommands.Verify(t => t.UnloadPackage(It.IsAny<Package>()), Times.Once);
        }

    }
}

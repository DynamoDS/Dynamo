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
        private string extensionsPath;
        private Mock<IPackageManagerCommands> packageManagerCommands;
        private PackageManagerViewModel viewModel;
        private Dictionary<string, object> dictPackage;
        private Dictionary<string, object> dictVersion;
        private Dictionary<MessageTypes, MessageBoxResult> PkgManagerMessages;
        private MessageTypes msgID;

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
            InitializePackageManagerMessageReturnValues();
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "viewExtensions");

            packageManagerCommands = new Mock<IPackageManagerCommands>();
            packageManagerCommands.Setup(t => t.Model).Returns(this.Model);
            packageManagerCommands.Setup(t => t.Loader).Returns(this.Model.GetPackageManagerExtension().PackageLoader);
            packageManagerCommands.Setup(t => t.ShowMessageBox(It.IsAny<MessageTypes>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                .Callback<MessageTypes, string, string, MessageBoxButton, MessageBoxImage>((messageID, msg, caption, options, boxImage) => msgID = messageID)
                .Returns(() => PkgManagerMessages[msgID]);

            var package = new { asset_id = "12343", asset_name = "test" };
            var version = new
            {
                contents = string.Empty,
                contains_binaries = false,
                node_libraries = new List<dynamic>(),
                dependencies = string.Empty,
                engine_version = "1.0.0.0",
                url = @"https://package.com/package.zip",
                version = "2013.11.10",
                file_id = "12133"
            };
            dictPackage = package.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(package, null));
            dictVersion = version.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(version, null));

            
            viewModel = new PackageManagerViewModel(packageManagerCommands.Object, "packages");
        }

        private void InitializePackageManagerMessageReturnValues()
        {
            PkgManagerMessages = new Dictionary<MessageTypes, MessageBoxResult>();
            PkgManagerMessages.Add(MessageTypes.ConfirmToInstallPackage, MessageBoxResult.Cancel);
            PkgManagerMessages.Add(MessageTypes.PackageContainPythinScript, MessageBoxResult.Cancel);
            PkgManagerMessages.Add(MessageTypes.ConfirmToUninstall, MessageBoxResult.No);
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

            //TODO: Add the assert
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
            PackageManagerViewModel viewModel = new PackageManagerViewModel(packageManagerCommands.Object, "packages");
            viewModel.DownloadRequest = JsonConvert.DeserializeObject(JsonConvert.SerializeObject((dynamic)new { asset_name = "test", asset_id = "12343" }));
            viewModel.InstallPackage(Path.Combine(testDirectoryPath, "pkgs", "TestPackage.zip"));

            packageManagerCommands.Verify(t => t.LoadPackage(It.IsAny<Package>()), Times.Once);
        }

        [Test]
        public void InstallPackageCancel()
        {
            //1. Confirm Install Package Message Box
            string returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "cancel");
        }

        [Test]
        public void InstallPythonScriptCheckCancel()
        {
            //2. Python Script Chaeck
            PkgManagerMessages[MessageTypes.ConfirmToInstallPackage] = MessageBoxResult.OK;
            dictVersion["contains_binaries"] = true;
            string returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "cancel");
        }

        [Test]
        public void InstallCheckDynamoVersion()
        {
            //3. Check Dynamo Version
            PkgManagerMessages[MessageTypes.ConfirmToInstallPackage] = MessageBoxResult.OK;
            PkgManagerMessages[MessageTypes.PackageContainPythinScript] = MessageBoxResult.OK;
            dictVersion["contains_binaries"] = true;
            //TODO: Return some concrete type.
            string returnValue = viewModel.PackageOnExecuted(dictPackage, dictVersion);
            StringAssert.AreEqualIgnoringCase(returnValue, "12343,12133,test");
        }

        [Test]
        public void UnInstallPackage()
        {
            packageManagerCommands.Setup(t => t.LocalPackages).Returns(new List<Package> { { new Package("test", "test", "1.0.0", "MIT") } });

            //Confirm UnInstall
            viewModel.PkgRequest = JsonConvert.DeserializeObject(JsonConvert.SerializeObject((dynamic)new { asset_name = "test" }));
            bool returnValue = viewModel.Uninstall();
            Assert.AreEqual(false, returnValue, string.Empty);

            //UnInstall
            PkgManagerMessages[MessageTypes.ConfirmToUninstall] = MessageBoxResult.OK;
            returnValue = viewModel.Uninstall();
            packageManagerCommands.Verify(t => t.UnloadPackage(It.IsAny<Package>()), Times.Once);
        }

    }
}

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
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
        }

        [Test]
        public void UIExtensionsAreExtracted()
        {
            var extensionManager = new ExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);
            Assert.Greater(extensions.Count(), 0);

            Assert.AreEqual(extensions.OfType<Dynamo.DynamoPackagesUI.PackageManagerExtension>().Count(), 1);
        }

        [Test]
        public void UIExtensionsRendered()
        {
            var extensionManager = new ExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);

            var uiExtension = extensions.OfType<Dynamo.DynamoPackagesUI.PackageManagerExtension>().FirstOrDefault();
            uiExtension.OnPackageManagerClick();

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void InstallDynamoPackage()
        {
            var client = new Mock<IPackageManagerCommands>();
            client.Verify(t => t.InstallPackage("test"));
            //string testDirectoryPath =  Path.Combine(new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.FullName, "test");
            //PackageManagerCommands packageManagerCommand = new PackageManagerCommands(Model.GetPackageManagerExtension().PackageLoader, Model);
            //packageManagerCommand.DownloadRequest = " {"asset_name":"test", "asset_id" = "1234" }";
            //packageManagerCommand.InstallPackage(Path.Combine(testDirectoryPath, "pkgs", "TestPackage.zip"));
            //Assert.Pass();
        }

    }
}

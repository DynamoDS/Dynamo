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

            Assert.AreEqual(extensions.OfType<PackageManagerExtension>().Count(), 1);
        }

        [Test]
        public void UIExtensionsRendered()
        {
            var extensionManager = new ExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);

            var uiExtension = extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            uiExtension.OnPackageManagerClick();

            AssertWindowOwnedByDynamoView<PackageManagerView>();
        }

        [Test]
        public void InstallDynamoPackage()
        {
            
        }

    }
}

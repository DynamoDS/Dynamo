using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo.Extensions;
using System.IO;
using Dynamo.PackageManager;
using System.Reflection;
using Dynamo.Models;
using Moq;
using Dynamo.Interfaces;

namespace PackageManagerTests
{
    class ExtensionTests
    {
        string extensionsPath;
        Mock<IExtension> extMock;
        DynamoModel model;

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
            extMock = new Mock<IExtension>();

            model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    Extensions = new List<IExtension> { extMock.Object }
                });
        }

        [Test]
        public void DirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(extensionsPath));
        }

        [Test]
        public void ExtensionsAreExtracted()
        {
            var extensionManager = new ExtensionManager();
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);
            Assert.Greater(extensions.Count(), 0);

            Assert.AreEqual(extensions.OfType<PackageManagerExtension>().Count(), 1);
        }

        [Test]
        public void ExtensionIsStarted()
        {
            extMock.Verify(ext => ext.Startup(It.IsAny<StartupParams>()));
        }

        [Test]
        public void ExtensionIsLoaded()
        {
            extMock.Verify(ext => ext.Load(It.IsAny<IPreferences>(), It.IsAny<IPathManager>()));
        }

        [Test]
        public void ExtensionIsAdded()
        {
            Assert.IsTrue(model.ExtensionManager.Extensions.Contains(extMock.Object));
        }

        [Test]
        public void ExtensionIsReady()
        {
            extMock.Verify(ext => ext.Ready(It.IsAny<ReadyParams>()));
        }

        [Test]
        public void ExtensionIsDisposed()
        {
            model.Dispose();
            extMock.Verify(ext => ext.Dispose());
        }
    }
}

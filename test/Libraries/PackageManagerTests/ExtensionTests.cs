using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo.Extensions;
using System.IO;
using Dynamo.PackageManager;

namespace PackageManagerTests
{
    class ExtensionTests
    {
        string extensionsPath;

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
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
    }
}

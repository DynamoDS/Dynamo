using System.IO;
using System.Linq;
using Dynamo.Extensions;
using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageManagerExtensionTests
    {
        string extensionsPath;

        [SetUp]
        public void Init()
        {
            extensionsPath = Path.Combine(Directory.GetCurrentDirectory(), "extensions");
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

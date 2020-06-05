using System.IO;
using System.Linq;
using Dynamo.Extensions;
using NUnit.Framework;

namespace Dynamo.Tests.Extensions
{
    [TestFixture]
    class ExtensionLoaderTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the private IExtension Load(ExtensionDefinition extension) method from the ExtensionLoader class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ExtensionsLoadExtensionInvalidCertificate()
        {
            //Arrange
            string extensionsPath = Path.Combine(TestDirectory, "pkgs\\sampleExtension\\extra");         
            var extensionManager = new ExtensionManager(new[] { extensionsPath });

            //Act
            //The extension definition is located in a folder without certificate, then when calling the CheckAssemblyForValidCertificate() method it will raise an exception
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);

            //Assert
            //Just checks that the extension was not loaded due to a invalid certificate (when trying to check the certificate, it will raise an exception handled internally)
            Assert.AreEqual(extensions.Count(), 0);
        }

        /// <summary>
        /// This test method will execute the  public IExtension Load(string extensionPath) method from the ExtensionLoader class, specifically the Log section
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ExtensionsLoadExtensionWrongDefinition()
        {
            //Arrange
            //The xml inside the wrong_extra is malformed.
            string extensionsPath = Path.Combine(TestDirectory, "pkgs\\sampleExtension\\wrong_extra");
            var extensionManager = new ExtensionManager();

            //Act
            //This will execute the Load(string extensionPath) method and due that the xml is malformed will execute the Log section
            var extensions = extensionManager.ExtensionLoader.LoadDirectory(extensionsPath);

            //Assert
            //Just checks that NO extension was loaded in the local ExtensionManager
            Assert.AreEqual(extensions.Count(), 0);
        }
    }
}

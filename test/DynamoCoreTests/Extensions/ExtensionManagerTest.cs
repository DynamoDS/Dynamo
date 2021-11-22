using System;
using System.Linq;
using Dynamo.Extensions;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests.Extensions
{
    [TestFixture]
    class ExtensionManagerTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the public void Remove(IExtension extension) from the ExtensionManager class 
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ExtensionsManagerRemoveExtension()
        {       
            //Arrange
            //A empty Mock Extension will be used
            var mockExtension = new Mock<IExtension>();

            //Act
            //The Base class only loads the PackageManager extension and IronPython extension
            //then we will try to remove a not existing extension and  will execute the Log section in the Remove method
            CurrentDynamoModel.ExtensionManager.Remove(mockExtension.Object);

            //Assert
            //Checking that only the PackageManagerextension remains in the extensions list (no more IronPython).
            Assert.AreEqual(CurrentDynamoModel.ExtensionManager.Extensions.Count(), 1);
        }

        /// <summary>
        /// This test method will execute the public void Remove(IExtension extension) from the ExtensionManager class and execute the exception section
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ExtensionsManagerRemoveExtensionException()
        {
            //Arrange
            //A empty Mock Extension will be used and when calling the Dispose method will raise an Exception
            var mockExtension = new Mock<IExtension>();
            mockExtension.Setup( ex => ex.Dispose()).Throws<Exception>();

            //We add the empty Mocked Extension
            CurrentDynamoModel.ExtensionManager.Add(mockExtension.Object);

            //Assert
            //Checking that now we have 2 extensions
            //PackageManager (added by the base class) and the Mock extension
            Assert.AreEqual(CurrentDynamoModel.ExtensionManager.Extensions.Count(), 2);

            //Act
            //This will execute the exception section from the Remove() method (but the extension is removed) since in this way was setup in the Mocked Extension
            CurrentDynamoModel.ExtensionManager.Remove(mockExtension.Object);

            //Assert
            //Checking that the extension was removed from the list even when an exception was raised
            Assert.AreEqual(CurrentDynamoModel.ExtensionManager.Extensions.Count(), 1);
        }
    }
}

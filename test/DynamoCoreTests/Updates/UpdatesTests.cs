using System;
using System.IO;
using Dynamo.Updates;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Test class to test the UpdateManager class and its related classes
    /// </summary>
    [TestFixture]
    public class UpdatesTests
    {
        private UpdateManager updateManager;

        [SetUp]
        public void Init()
        {
            var config = UpdateManagerConfiguration.GetSettings(null);
            updateManager = new UpdateManager(config);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateDownloadedEventArgsConstructorTest()
        {
            Exception e = new Exception("Error");
            string fileLocation = "FileLocation";

            var eventArgs = new UpdateDownloadedEventArgs(e, fileLocation);

            Assert.AreEqual(e, eventArgs.Error);
            Assert.AreEqual(fileLocation, eventArgs.UpdateFileLocation);
            Assert.IsTrue(eventArgs.UpdateAvailable);

            eventArgs = new UpdateDownloadedEventArgs(e, null);

            Assert.IsFalse(eventArgs.UpdateAvailable);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfiguration_LoadTest()
        {
            Assert.IsNull(UpdateManagerConfiguration.Load(null, updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("filePath", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load(Path.GetTempFileName(), updateManager));
        }
    }
}
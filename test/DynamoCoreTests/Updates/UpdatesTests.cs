using System;
using System.Collections.Generic;
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

        #region UpdateDownloadedEventArgs
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
        #endregion

        #region UpdateManagerConfiguration
        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfiguration_LoadTest()
        {
            Assert.IsNull(UpdateManagerConfiguration.Load(null, updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("filePath", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load(Path.GetTempFileName(), updateManager));
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfiguration_SaveTest()
        {
            var config = new UpdateManagerConfiguration();
            Assert.DoesNotThrow(() => config.Save(Path.GetTempFileName(), updateManager));
        }

        [Test]
        [Category("UnitTests")]
        public void CheckNewerDailyBuildTest()
        {
            var config = new UpdateManagerConfiguration();
            var oldValue = config.CheckNewerDailyBuild;

            config.CheckNewerDailyBuild = !config.CheckNewerDailyBuild;
            Assert.AreEqual(!oldValue, config.CheckNewerDailyBuild);
        }

        [Test]
        [Category("UnitTests")]
        public void ForceUpdateTest()
        {
            var config = new UpdateManagerConfiguration();
            var oldValue = config.ForceUpdate;

            config.ForceUpdate = !config.ForceUpdate;
            Assert.AreEqual(!oldValue, config.ForceUpdate);
        }
        #endregion

        #region UpdateManager
        [Test]
        [Category("UnitTests")]
        public void QuitAndInstallUpdateTest()
        {
            Assert.DoesNotThrow(() => updateManager.QuitAndInstallUpdate());
        }

        [Test]
        [Category("UnitTests")]
        public void HostApplicationBeginQuitTest()
        {
            Assert.DoesNotThrow(() => updateManager.HostApplicationBeginQuit());
        }
        #endregion

        #region DynamoLookup
        private class DynamoLookupChildTest : DynamoLookUp
        {
            public override IEnumerable<string> GetDynamoInstallLocations()
            {
                return new List<string>();
            }
        }

        [Test]
        [Category("UnitTests")]
        public void DynamoLookupTests()
        {
            var lookup = new DynamoLookupChildTest();

            Assert.IsNull(lookup.LatestProduct);
            Assert.IsNull(lookup.GetDynamoVersion(Path.GetTempPath()));
            Assert.IsNotNull(lookup.GetDynamoUserDataLocations());
        }
        #endregion

        #region UpdateRequest
        [Test]
        [Category("UnitTests")]
        public void UpdateRequest_ConstructorTest()
        {
            var path = new Uri(Path.GetTempPath());
            var updateRequest = new UpdateRequest(path, updateManager);

            Assert.IsNotNull(updateRequest.OnRequestCompleted);
            Assert.IsNotNull(updateRequest.Data);
            Assert.IsNotNull(updateRequest.Error);
            Assert.IsNotNull(updateRequest.Path);
        }
        #endregion
    }
}
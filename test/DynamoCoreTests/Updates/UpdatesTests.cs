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
            //UpdateAvailable depends on FileLocation. If it's null, it will be false.
            Assert.IsFalse(eventArgs.UpdateAvailable);
        }
        #endregion

        #region UpdateManagerConfiguration
        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfigurationLoadTest()
        {
            Assert.IsNull(UpdateManagerConfiguration.Load(null, updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load("filePath", updateManager));
            Assert.IsNull(UpdateManagerConfiguration.Load(Path.GetTempFileName(), updateManager));
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfigurationSaveTest()
        {
            var config = new UpdateManagerConfiguration();
            Assert.DoesNotThrow(() => config.Save(Path.GetTempFileName(), updateManager));
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfigurationPropertiesTest()
        {
            var config = new UpdateManagerConfiguration();
            var checkNewerDailyBuildOldValue = config.CheckNewerDailyBuild;

            config.CheckNewerDailyBuild = !config.CheckNewerDailyBuild;
            Assert.AreEqual(!checkNewerDailyBuildOldValue, config.CheckNewerDailyBuild);

            var forceUpdateOldValue = config.ForceUpdate;

            config.ForceUpdate = !config.ForceUpdate;
            Assert.AreEqual(!forceUpdateOldValue, config.ForceUpdate);
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

            //LatestProduct will be null in this case because in DynamoLookupChildTest, the implementation of GetDynamoInstallLocations returns an empty list.
            Assert.IsNull(lookup.LatestProduct);
            Assert.IsNull(lookup.GetDynamoVersion(Path.GetTempPath()));
            Assert.IsNotNull(lookup.GetDynamoUserDataLocations());
        }
        #endregion

        #region UpdateRequest
        [Test]
        [Category("UnitTests")]
        public void UpdateRequestConstructorTest()
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
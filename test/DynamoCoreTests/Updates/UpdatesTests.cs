using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Logging;
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
            //Create an instance of the UpdateManager class to be used in the tests.
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
            
            //UpdateAvailable depends on FileLocation. If it's null, it will be false, else it is true.
            Assert.IsTrue(eventArgs.UpdateAvailable);
            
            eventArgs = new UpdateDownloadedEventArgs(e, null);
            Assert.IsFalse(eventArgs.UpdateAvailable);
        }
        #endregion

        #region UpdateManagerConfiguration
        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfigurationLoadTest()
        {
            //Testing all the calls for Load that return null
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
            var goodPath = Path.GetTempFileName();
            var badPath = "W:\\PathThatDoesntExist";

            //Happy path
            Assert.DoesNotThrow(() => config.Save(goodPath, updateManager));

            //if statement in the catch block
            Assert.DoesNotThrow(() => config.Save(badPath, updateManager));
            
            //else statement in the catch block
            Assert.Throws<DirectoryNotFoundException>(() => config.Save(badPath, null));
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerConfigurationPropertiesTest()
        {
            var config = (UpdateManagerConfiguration)updateManager.Configuration;

            //CheckNewerDailyBuild
            //Set to false and then true to trigger if statement.
            config.CheckNewerDailyBuild = false;
            config.CheckNewerDailyBuild = true;

            Assert.AreEqual(true, config.CheckNewerDailyBuild);

            //ForceUpdate
            //Set to false and then true to trigger if statement.
            config.ForceUpdate = false;
            config.ForceUpdate = true;

            Assert.AreEqual(true, config.ForceUpdate);
        }
        #endregion

        #region UpdateManager
        [Test]
        [Category("UnitTests")]
        public void QuitAndInstallUpdateTest()
        {
            //This handler is used to cover the if statement
            updateManager.ShutdownRequested += TestShutdownRequestedHandler;
            Assert.DoesNotThrow(() => updateManager.QuitAndInstallUpdate());
            updateManager.ShutdownRequested -= TestShutdownRequestedHandler;
        }

        private void TestShutdownRequestedHandler(IUpdateManager updateManager) { }

        [Test]
        [Category("UnitTests")]
        public void BaseVersionTest()
        {
            updateManager.DownloadedUpdateInfo = new AppVersionInfo() { Version = BinaryVersion.FromString("5.5.5.5") };
            updateManager.UpdateInfo = new AppVersionInfo() { Version = BinaryVersion.FromString("5.5.5.5") };

            //These two cases are to cover both routes in the BaseVersion if statement.
            //ProductVersion < HostVersion
            updateManager.HostVersion = new Version(9, 9, 9, 9);
            Assert.IsTrue(updateManager.IsUpdateAvailable);

            //ProductVersion > HostVersion
            updateManager.HostVersion = new Version(1, 1, 1, 1);
            Assert.IsTrue(updateManager.IsUpdateAvailable);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateManagerPropertiesTest()
        {
            //CheckNewerDailyBuild
            //Set to false and then true to trigger if statement.
            updateManager.CheckNewerDailyBuilds = false;
            updateManager.CheckNewerDailyBuilds = true;

            Assert.AreEqual(true, updateManager.CheckNewerDailyBuilds);

            //ForceUpdate
            //Set to false and then true to trigger if statement.
            updateManager.ForceUpdate = false;
            updateManager.ForceUpdate = true;

            Assert.AreEqual(true, updateManager.ForceUpdate);

            // HostName
            updateManager.HostName = "Host Name";
            Assert.AreEqual("Host Name", updateManager.HostName);

            //UpdateFileLocation
            _ = new UpdateManager(new UpdateManagerConfiguration());
            //  For a new UpdateManager, the UpdateFileLocation property is not initialized.
            Assert.That(updateManager.UpdateFileLocation, Is.Null.Or.Empty);
        }

        [Test]
        [Category("UnitTests")]
        public void IsStableBuildTest()
        {
            //If fileName does not contain installNameBase, returns false.
            Assert.IsFalse(UpdateManager.IsStableBuild("InstallNameBase", "FileName"));
        }

        [Test]
        [Category("UnitTests")]
        public void IsDailyBuildTest()
        {
            //If fileName does not contain installNameBase, returns false.
            Assert.IsFalse(UpdateManager.IsDailyBuild("InstallNameBase", "FileName"));
        }

        [Test]
        [Category("UnitTests")]
        public void RegisterExternalApplicationProcessIdTest()
        {
            //This is a void method that sets a private attribute. There is no way to validate whether the
            //value was changed or not, but this covers the code and makes sure no exceptions are thrown.
            Assert.DoesNotThrow(() => updateManager.RegisterExternalApplicationProcessId(1));
        }

        [Test]
        [Category("UnitTests")]
        public void OnLogTest()
        {
            //Create and validate the LogEventArgs
            var logEventArgs = new LogEventArgs("Test", LogLevel.Console);

            Assert.AreEqual(LogLevel.Console, logEventArgs.Level);
            Assert.AreEqual("Test", logEventArgs.Message);

            //Bind the handler and call OnLog
            updateManager.Log += TestLogHandler;
            updateManager.OnLog(logEventArgs);
            updateManager.Log -= TestLogHandler;
        }
        
        private void TestLogHandler(LogEventArgs args) { }
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

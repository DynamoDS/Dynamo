using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Dynamo.UpdateManager;

using ProtoCore.Lang;

using DynUpdateManager = Dynamo.UpdateManager.UpdateManager;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Test cases to mock return values.
    /// </summary>
    public class UpdateManagerTestNotUpToDate
    {
        /// <summary>
        /// Utility class to inject the UpdateManagerConfiguration to
        /// UpdateManager instance and then resets it to previous value
        /// when its disposed.
        /// </summary>
        class ConfigInjection : IDisposable
        {
            private readonly IUpdateManager updateManager; //updatemanager instance.
            private readonly object configuration; //old configuration value.
            private readonly FieldInfo fieldInfo; //internal configuration field.

            /// <summary>
            /// Creates ConfigInjection instance.
            /// </summary>
            /// <param name="updateManager">UpdateManager instance to which configuration is to be injected.</param>
            /// <param name="configuration">The configuration for injection.</param>
            public ConfigInjection(IUpdateManager updateManager, UpdateManagerConfiguration configuration)
            {
                this.updateManager = updateManager;
                fieldInfo = updateManager.GetType()
                    .GetField("configuration", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(fieldInfo);

                this.configuration = fieldInfo.GetValue(updateManager);
                fieldInfo.SetValue(updateManager, configuration);
            }

            public void Dispose()
            {
                //Restore the old configuration
                fieldInfo.SetValue(updateManager, configuration);
            }
        }

        private const string DOWNLOAD_SOURCE_PATH_S = "http://downloadsourcepath/";
        private const string SIGNATURE_SOURCE_PATH_S = "http://SignatureSourcePath/";

        static UpdateManagerConfiguration NewConfiguration(bool checkNewerDailyBuild =false, bool forceUpdate =false)
        {
            return new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S,
                CheckNewerDailyBuild = checkNewerDailyBuild,
                ForceUpdate = forceUpdate,
            };
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsInfoWhenNewerVersionAvaialable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);

            var updateManager = UpdateManager.UpdateManager.Instance;
            using (new ConfigInjection(updateManager, NewConfiguration()))
            {
                updateManager.UpdateDataAvailable(updateRequest.Object);
                Assert.NotNull(updateManager.UpdateInfo);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsInfoWhenNewerDailyBuildAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.dailyBuildAvailableData);

            var updateManager = UpdateManager.UpdateManager.Instance;
            using (new ConfigInjection(updateManager, NewConfiguration(checkNewerDailyBuild:true)))
            {
                updateManager.UpdateDataAvailable(updateRequest.Object);
                Assert.NotNull(updateManager.UpdateInfo);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsCorrectVersionWhenAvailable()
        {
            var um = UpdateManager.UpdateManager.Instance as DynUpdateManager;
            Assert.IsNotNull(um);

            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);
            um.UpdateDataAvailable(updateRequest.Object);

            // Spoof a download completion by setting the downloaded update info to the update info
            um.DownloadedUpdateInfo = um.UpdateInfo;
            Assert.NotNull(um.UpdateInfo);
            Assert.AreEqual(um.AvailableVersion.ToString(), "9.9.9.0");
        }

        [Test, Category("UnitTests")]
        public void ConfigurationSerialization()
        {
            var config = new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S
            };
            
            //save to a temp file.
            var tempFile = Path.GetTempFileName();
            Assert.DoesNotThrow(() => config.Save(tempFile, null));
            
            //read from a temp file.
            UpdateManagerConfiguration savedConfig = null;
            Assert.DoesNotThrow(() => savedConfig = UpdateManagerConfiguration.Load(tempFile, null));
            
            //Compare parameters.
            Assert.IsNotNull(savedConfig);
            Assert.AreEqual(config.CheckNewerDailyBuild, savedConfig.CheckNewerDailyBuild);
            Assert.AreEqual(config.SignatureSourcePath, savedConfig.SignatureSourcePath);
            Assert.AreEqual(config.DownloadSourcePath, savedConfig.DownloadSourcePath);
            Assert.AreEqual(config.ForceUpdate, savedConfig.ForceUpdate);
        }

        [Test]
        [Category("UnitTests")]
        public void ConfigurationRedirection()
        {
            var um = DynUpdateManager.Instance as DynUpdateManager;
            Assert.IsNotNull(um);

            //Inject test config to UpdateManager instance, using reflection.
            var config = new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S
            };

            using (new ConfigInjection(um, config))
            {
                var updateRequest = new Mock<IAsynchronousRequest>();
                updateRequest.Setup(ur => ur.Data)
                    .Returns(UpdateManagerTestHelpers.updateAvailableData);
                um.UpdateDataAvailable(updateRequest.Object);

                // Spoof a download completion by setting the downloaded update info to the update info
                um.DownloadedUpdateInfo = um.UpdateInfo;
                Assert.NotNull(um.UpdateInfo);
                Assert.AreEqual("9.9.9.0", um.AvailableVersion.ToString());
                Assert.AreEqual(DOWNLOAD_SOURCE_PATH_S, um.UpdateInfo.VersionInfoURL);
                Assert.AreEqual(
                    SIGNATURE_SOURCE_PATH_S + @"DynamoInstall9.9.9.sig",
                    um.UpdateInfo.SignatureURL);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNoNewerVersionAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);
            UpdateManager.UpdateManager.Instance.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(UpdateManager.UpdateManager.Instance.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNoVersionsAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noData);
            UpdateManager.UpdateManager.Instance.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(UpdateManager.UpdateManager.Instance.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNotConnected()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(string.Empty);

            UpdateManager.UpdateManager.Instance.CheckForProductUpdate(updateRequest.Object);

            Assert.Null(UpdateManager.UpdateManager.Instance.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void ShouldRecognizeStableInstallerWithProperName()
        {
            var test =
                UpdateManager.UpdateManager.IsStableBuild("DynamoInstall", "DynamoInstall0.7.1.exe");
            Assert.True(test);
        }

        [Test]
        [Category("UnitTests")]
        public void ShouldRecognizeOldDailyBuilds()
        {
            var test =
                UpdateManager.UpdateManager.IsDailyBuild("DynamoDailyInstall", "DynamoDailyInstall20140625T0009.exe");
            Assert.True(test);
        }

        [Test]
        [Category("UnitTests")]
        public void ShouldRecognizeDailyInstallerWithProperName()
        {
            var test =
                UpdateManager.UpdateManager.IsStableBuild("DynamoInstall", "DynamoInstall0.7.1.20140625T0009.exe");
            Assert.False(test);

            test = UpdateManager.UpdateManager.IsDailyBuild("DynamoInstall", "DynamoInstall0.7.1.20140625T0009.exe");
            Assert.True(test);
        }

        [Test]
        [Category("UnitTests")]
        public void ShouldGetBinaryVersionFromInstaller()
        {
            var version = UpdateManager.UpdateManager.GetBinaryVersionFromFilePath("DynamoInstall", "DynamoInstall0.7.1.exe");
            Assert.AreEqual("0.7.1.0", version.ToString());

            version = UpdateManager.UpdateManager.GetBinaryVersionFromFilePath("DynamoInstall", "DynamoInstall0.7.1.20140625T0009.exe");
            Assert.AreEqual("0.7.1.0", version.ToString());

            version = UpdateManager.UpdateManager.GetBinaryVersionFromFilePath("DynamoInstall", "Blah.exe");
            Assert.IsNull(version);
        }

        [Test]
        [Category("UnitTests")]
        public void ShouldGetDateTimeFromDailyInstaller()
        {
            var dateTime = UpdateManager.UpdateManager.GetBuildTimeFromFilePath("DynamoInstall", "DynamoInstall0.7.1.20140625T0009.exe");
            Assert.AreEqual(dateTime.Year, 2014);
            Assert.AreEqual(dateTime.Month, 6);
            Assert.AreEqual(dateTime.Day, 25);
            Assert.AreEqual(dateTime.Minute, 9);
            Assert.AreEqual(dateTime.Hour, 0);

            dateTime = UpdateManager.UpdateManager.GetBuildTimeFromFilePath("DynamoInstall", "DynamoInstall0.7.1.exe");
            Assert.AreEqual(dateTime, DateTime.MinValue);
        }

        [Test, Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenGivenBadData()
        {
            // Here we simulate some xml that is returned that is well-formed
            // but not what we are looking for. This is the case often when you
            // are logging into a network that requires an additional login, like
            // hotel wi-fi. In this case, the ListBucketResult element will not 
            // be found in the xml and we'll get null UpdateInfo.

            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.badData);

            UpdateManager.UpdateManager.Instance.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(UpdateManager.UpdateManager.Instance.UpdateInfo);
        }
    }

    internal static class UpdateManagerTestHelpers
    {
        public const string updateAvailableData =
            "<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">" +
                "<Name>dyn-builds-data</Name>" +
                "<Prefix/>" +
                "<Marker/>" +
                "<MaxKeys>1000</MaxKeys>" +
                "<IsTruncated>true</IsTruncated>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.0.exe</Key>" +
                "<LastModified>2013-11-01T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall1.0.0.exe</Key>" +
                "<LastModified>2013-11-02T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall9.9.9.exe</Key>" +
                "<LastModified>2013-11-03T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "</ListBucketResult>";

        // Daily build data for the year 2099 :)
        public const string dailyBuildAvailableData =
            "<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">" +
                "<Name>dyn-builds-data</Name>" +
                "<Prefix/>" +
                "<Marker/>" +
                "<MaxKeys>1000</MaxKeys>" +
                "<IsTruncated>true</IsTruncated>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.0.exe</Key>" +
                "<LastModified>2013-11-01T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.0.20990101T0001.exe</Key>" +
                "<LastModified>2099-11-02T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.0.20990101T0002.exe</Key>" +
                "<LastModified>2099-11-03T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "</ListBucketResult>";

        public const string noUpdateAvailableData =
            "<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">" +
                "<Name>dyn-builds-data</Name>" +
                "<Prefix/>" +
                "<Marker/>" +
                "<MaxKeys>1000</MaxKeys>" +
                "<IsTruncated>true</IsTruncated>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.0.exe</Key>" +
                "<LastModified>2013-11-01T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.1.exe</Key>" +
                "<LastModified>2013-11-02T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "<Contents>" +
                "<Key>DynamoInstall0.1.2.exe</Key>" +
                "<LastModified>2013-11-03T17:02:59.000Z</LastModified>" +
                "</Contents>" +
                "</ListBucketResult>";

        public const string noData =
            "<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">" +
                "<Name>dyn-builds-data</Name>" +
                "<Prefix/>" +
                "<Marker/>" +
                "<MaxKeys>1000</MaxKeys>" +
                "<IsTruncated>true</IsTruncated>" +
                "</ListBucketResult>";

        public const string badData =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<note>" +
                "<to> Foo</to>" +
                "<from>Bar</from>" +
                "<heading>Reminder</heading>" +
                "<body>This is some bad xml!</body>" +
                "</note>";

        public static void DoNothing()
        {
            Debug.WriteLine("Doing nothing.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Dynamo.UpdateManager;
using Moq;
using NUnit.Framework;
using DynUpdateManager = Dynamo.UpdateManager.UpdateManager;


namespace Dynamo.Tests
{
    /// <summary>
    /// Test cases to mock return values.
    /// </summary>
    public class UpdateManagerTestNotUpToDate
    {
        private const string DOWNLOAD_SOURCE_PATH_S = "http://downloadsourcepath/";
        private const string SIGNATURE_SOURCE_PATH_S = "http://SignatureSourcePath/";

        static UpdateManagerConfiguration NewConfiguration(bool checkNewerDailyBuild =false, bool forceUpdate =false, IDynamoLookUp lookup = null)
        {
            return new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S,
                CheckNewerDailyBuild = checkNewerDailyBuild,
                ForceUpdate = forceUpdate,
                DynamoLookUp = lookup,
            };
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsInfoWhenNewerVersionAvaialable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);

            var updateManager = new DynUpdateManager(NewConfiguration());
            updateManager.UpdateDataAvailable(updateRequest.Object);
            Assert.NotNull(updateManager.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsInfoWhenNewerDailyBuildAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.dailyBuildAvailableData);

            var updateManager = new DynUpdateManager(NewConfiguration(checkNewerDailyBuild: true));
            updateManager.UpdateDataAvailable(updateRequest.Object);
            Assert.NotNull(updateManager.UpdateInfo);
        }

        [Test, Category("UnitTests")]
        public void IsUpdateAvailableWhenNewerVersionAvaialable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);

            var updateManager = new DynUpdateManager(NewConfiguration());
            updateManager.UpdateDataAvailable(updateRequest.Object);
            updateManager.DownloadedUpdateInfo = updateManager.UpdateInfo;

            Assert.IsTrue(updateManager.IsUpdateAvailable);
        }

        [Test, Category("UnitTests")]
        public void IsUpdateAvailableWhenForceUpdateIsTrue()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);

            var updateManager = new DynUpdateManager(NewConfiguration(forceUpdate:true));
            updateManager.UpdateDataAvailable(updateRequest.Object);
            updateManager.DownloadedUpdateInfo = updateManager.UpdateInfo;

            Assert.IsTrue(updateManager.IsUpdateAvailable);
        }

        [Test, Category("UnitTests")]
        public void NoUpdateAvailableWhenUpToDate()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);

            var updateManager = new DynUpdateManager(NewConfiguration());
            updateManager.UpdateDataAvailable(updateRequest.Object);
            updateManager.DownloadedUpdateInfo = updateManager.UpdateInfo;

            Assert.IsFalse(updateManager.IsUpdateAvailable);
        }

        [Test, Category("UnitTests")]
        public void NoUpdateIsAvailableWhenHigherVersionDynamoIsInstalled()
        {
            var lookup = new Mock<DynamoLookUp>();
            lookup.Setup(l => l.GetDynamoInstallLocations()).Returns(new[] { "A" });
            lookup.Setup(l => l.GetDynamoVersion(It.IsAny<string>()))
                .Returns<string>(s => Version.Parse("9.9.9.0"));

            var um = new DynUpdateManager(NewConfiguration(false, false, lookup.Object));
            Assert.IsNotNull(um);

            DynUpdateManager.CheckForProductUpdate(um);
            um.DownloadedUpdateInfo = um.UpdateInfo;

            Assert.IsNull(um.UpdateInfo);
            Assert.IsFalse(um.IsUpdateAvailable);
        }

        [Test, Category("UnitTests")]
        public void NoUpdateAvailableWhenUpdateInfoIsNotYetDownloaded()
        {
            var updateManager = new DynUpdateManager(NewConfiguration());
            
            Assert.IsFalse(updateManager.IsUpdateAvailable);
        }

        [Test, Category("UnitTests")]
        public void UpdateCheckReturnsCorrectVersionWhenAvailable()
        {
            var um = new DynUpdateManager(NewConfiguration());
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
            //Inject test config to UpdateManager instance, using reflection.
            var config = new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S
            };

            var um = new DynUpdateManager(config);
            Assert.IsNotNull(um);
        
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

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNoNewerVersionAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);
            var um = new DynUpdateManager(NewConfiguration());
            um.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(um.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNoVersionsAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noData);
            var um = new DynUpdateManager(NewConfiguration());
            um.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(um.UpdateInfo);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateCheckReturnsNothingWhenNotConnected()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(string.Empty);

            var um = new DynUpdateManager(NewConfiguration());
            um.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(um.UpdateInfo);
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

            var um = new DynUpdateManager(NewConfiguration());
            um.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(um.UpdateInfo);
        }

        [Test, Category("UnitTests")]
        public void GetLatestProductFromGoodData()
        {
            var products = new Dictionary<string, Version>
            {
                { "A", new Version(0, 1, 2, 3) },
                { "B", new Version(0, 1, 2, 3) },
                { "C", new Version(1, 2, 0, 0) },
                { "D", new Version(0, 1, 3, 3) },
            };

            var lookup = new Mock<DynamoLookUp>();
            lookup.Setup(l => l.GetDynamoInstallLocations()).Returns(products.Keys);
            lookup.Setup(l => l.GetDynamoVersion(It.IsAny<string>()))
                .Returns<string>(s => products[s]);

            var latest = lookup.Object.LatestProduct;
            Assert.AreEqual("1.2.0.0", latest.ToString());
        }

        [Test, Category("UnitTests")]
        public void NoLatestProductVersion()
        {
            var lookup = new Mock<DynamoLookUp>();
            lookup.Setup(l => l.GetDynamoInstallLocations()).Returns(new[] { "A" });
            lookup.Setup(l => l.GetDynamoVersion(It.IsAny<string>()))
                .Returns<string>(null);

            var latest = lookup.Object.LatestProduct;
            Assert.AreEqual(null, latest);
        }

        [Test, Category("UnitTests")]
        public void GetLatestProductVersion()
        {
            var products = new Dictionary<string, Version>
            {
                { "A", new Version() },
                { "B", new Version(0, 1, 2, 3) },
                { "C", new Version(1, 2, 0, 0) },
                { "D", null },
            };

            var lookup = new Mock<DynamoLookUp>();
            lookup.Setup(l => l.GetDynamoInstallLocations()).Returns(products.Keys);
            lookup.Setup(l => l.GetDynamoVersion(It.IsAny<string>()))
                .Returns<string>(s => products[s]);
            var latest = lookup.Object.LatestProduct;
            Assert.AreEqual("1.2.0.0", latest.ToString());
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

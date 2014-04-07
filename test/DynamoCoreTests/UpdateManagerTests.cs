using System;
using System.Diagnostics;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Test cases to mock return values.
    /// </summary>
    public class UpdateManagerTestNotUpToDate : DynamoUnitTest
    {
        [Test]
        public void UpdateCheckReturnsInfoWhenNewerVersionAvaialable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);
            dynSettings.Controller.UpdateManager.UpdateDataAvailable(updateRequest.Object);

            Assert.NotNull(Controller.UpdateManager.UpdateInfo);
        }

        [Test]
        public void UpdateCheckReturnsCorrectVersionWhenAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.updateAvailableData);
            dynSettings.Controller.UpdateManager.UpdateDataAvailable(updateRequest.Object);

            Assert.NotNull(Controller.UpdateManager.UpdateInfo);
            Assert.AreEqual(Controller.UpdateManager.AvailableVersion.ToString(), "9.9.9.0");
        }

        [Test]
        public void UpdateCheckReturnsNothingWhenNoNewerVersionAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);
            dynSettings.Controller.UpdateManager.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(Controller.UpdateManager.UpdateInfo);
        }

        [Test]
        public void UpdateCheckReturnsNothingWhenNoVersionsAvailable()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(UpdateManagerTestHelpers.noData);
            dynSettings.Controller.UpdateManager.UpdateDataAvailable(updateRequest.Object);

            Assert.Null(Controller.UpdateManager.UpdateInfo);
        }

        [Test]
        public void UpdateCheckReturnsNothingWhenNotConnected()
        {
            var updateRequest = new Mock<IAsynchronousRequest>();
            updateRequest.Setup(ur => ur.Data).Returns(string.Empty);

            Controller.UpdateManager.CheckForProductUpdate(updateRequest.Object);

            Assert.Null(Controller.UpdateManager.UpdateInfo);
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

        public static void DoNothing()
        {
            Debug.WriteLine("Doing nothing.");
        }
    }
}

using System.Diagnostics;
using Dynamo.Utilities;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class UpdateManagerTestNotUpToDate : DynamoUnitTest
    {
        /*[Test]
        public void IsUpdateAvailableReturnsTrueWhenNewerVersionAvaialable()
        {
            var updateRequest = new Mock<IUpdateRequest>();
            updateRequest.Setup(ur => ur.UpdateRequestData).Returns(UpdateManagerTestHelpers.updateAvailableData);

            var update = dynSettings.Controller.UpdateManager.IsUpdateAvailable(updateRequest.Object);
            Assert.True(update);
        }

        [Test]
        public void IsUpdateAvailableReturnsCorrectVersionWhenAvailable()
        {
            var updateRequest = new Mock<IUpdateRequest>();
            updateRequest.Setup(ur => ur.UpdateRequestData).Returns(UpdateManagerTestHelpers.updateAvailableData);

            var update = dynSettings.Controller.UpdateManager.IsUpdateAvailable(updateRequest.Object);
            Assert.True(update);

            Assert.AreEqual(dynSettings.Controller.UpdateManager.AvailableVersion.ToString(), "9.9.9.0");
        }

        [Test]
        public void IsUpdateAvailableReturnsFalseWhenNoNewerVersionAvailable()
        {
            var updateRequest = new Mock<IUpdateRequest>();
            updateRequest.Setup(ur => ur.UpdateRequestData).Returns(UpdateManagerTestHelpers.noUpdateAvailableData);

            var update = dynSettings.Controller.UpdateManager.IsUpdateAvailable(updateRequest.Object);
            Assert.False(update);
        }

        [Test]
        public void IsUpdateAvailableReturnsFalseWhenNoVersionsAvailable()
        {
            var updateRequest = new Mock<IUpdateRequest>();
            updateRequest.Setup(ur => ur.UpdateRequestData).Returns(UpdateManagerTestHelpers.noData);

            var update = dynSettings.Controller.UpdateManager.IsUpdateAvailable(updateRequest.Object);
            Assert.False(update);
        }

        [Test]
        public void IsUpdateAvaialableReturnsFalseWhenNoConnected()
        {
            var updateRequest = new Mock<IUpdateRequest>();
            updateRequest.Setup(ur => ur.UpdateRequestData).Returns(string.Empty);

            var update = dynSettings.Controller.UpdateManager.IsUpdateAvailable(updateRequest.Object);
            Assert.False(update);
        }*/
    }

    public static class UpdateManagerTestHelpers
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

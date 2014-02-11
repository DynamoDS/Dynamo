using Dynamo.Core;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests
{

    internal class UpdateManagerTests : DynamoUnitTest
    {
        [Test]
        public void UpToDateReturnsCorrectlyIfNewerAvaialable()
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("0.6.6.6"));
            DynamoSettings.Controller.UpdateManager = um_mock.Object;
            Assert.False(DynamoSettings.Controller.UpdateManager.IsUpToDate);
        }

        public void UpToDateReturnsCorrectlyIfNewerNotAvaialable()
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("0.6.6.6"));
            DynamoSettings.Controller.UpdateManager = um_mock.Object;
            Assert.True(DynamoSettings.Controller.UpdateManager.IsUpToDate);
        }
    }
}

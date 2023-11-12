using System.Windows;
using Dynamo.UI.Controls;
using Dynamo.Updates;
using Moq;
using NUnit.Framework;
using SystemTestServices;
using DynamoCoreWpfTests.Utility;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class UpdateManagerUITests : SystemTestBase
    {
        private const string DOWNLOAD_SOURCE_PATH_S = "http://downloadsourcepath/";
        private const string SIGNATURE_SOURCE_PATH_S = "http://SignatureSourcePath/";

        private void MockUpdateManager(bool isUpToDate)
        {
            var umMock = new Mock<IUpdateManager>();
            umMock.Setup(um => um.IsUpdateAvailable).Returns(!isUpToDate);

            UpdateManager = umMock.Object;
        }

        [SetUp]
        public override void Setup()
        {
            //override this to avoid the typical startup behavior
        }
    }
}

using System.Windows;
using Dynamo.UI.Controls;
using Dynamo.UpdateManager;
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

        [Test]
        [Category("UnitTests")]
        public void UpdateButtonNotCollapsedIfNotUpToDate()
        {
            MockUpdateManager(isUpToDate: false);

            base.Setup();

            DispatcherUtil.DoEvents();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            Assert.IsNotNull(stb);
            var updateControl = stb.UpdateControl;
            Assert.IsNotNull(updateControl);
            Assert.AreEqual(Visibility.Visible, updateControl.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateButtonCollapsedIfUpToDate()
        {
            MockUpdateManager(isUpToDate:true);

            base.Setup();

            DispatcherUtil.DoEvents();

            var stb = View.shortcutBarGrid.Children[0] as ShortcutToolbar;
            Assert.IsNotNull(stb);
            var updateControl = stb.UpdateControl;
            Assert.IsNotNull(updateControl);
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI.Controls;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoCore.UI.Controls;

using NUnit.Framework;
using Moq;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class UpdateManagerUITests : DynamoTestUIBase
    {
        private const string DOWNLOAD_SOURCE_PATH_S = "http://downloadsourcepath/";
        private const string SIGNATURE_SOURCE_PATH_S = "http://SignatureSourcePath/";

        private static Mock<IUpdateManager> MockUpdateManager(string availableVersion, string productVersion)
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString(availableVersion));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString(productVersion));

            var config = new UpdateManagerConfiguration()
            {
                DownloadSourcePath = DOWNLOAD_SOURCE_PATH_S,
                SignatureSourcePath = SIGNATURE_SOURCE_PATH_S
            };
            um_mock.Setup(um => um.Configuration).Returns(config);

            var fieldInfo = typeof(UpdateManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(fieldInfo);
            fieldInfo.SetValue(UpdateManager.Instance, um_mock.Object);
            return um_mock;
        }

        private void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            var corePath =
                    Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            Model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true,
                    DynamoCorePath = corePath
                });

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            //create the view
            View = new DynamoView(ViewModel);
            View.Show();                             

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            CreateTemporaryFolder();
        }

        [SetUp]
        public override void Start()
        {
            //override this to avoid the typical startup behavior
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateButtonNotCollapsedIfNotUpToDate()
        {
            const string availableVersion = "9.9.9.9";
            const string productVersion = "1.1.1.1";
            MockUpdateManager(availableVersion, productVersion);
            
            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Visible, updateControl.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateButtonCollapsedIfUpToDate()
        {
            const string availableVersion = "1.1.1.1";
            const string productVersion = "9.9.9.9";
            MockUpdateManager(availableVersion, productVersion);

            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateButtonCollapsedIfNotConnected()
        {
            const string availableVersion = "";
            const string productVersion = "9.9.9.9";
            MockUpdateManager(availableVersion, productVersion);

            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }
    }
}

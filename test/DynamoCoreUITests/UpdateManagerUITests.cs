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

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder(TempFolder);
            }
        }

        [SetUp]
        public override void Start()
        {
            //override this to avoid the typical startup behavior
        }

        [Test]
        public void UpdateButtonNotCollapsedIfNotUpToDate()
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            var fieldInfo = typeof(UpdateManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(UpdateManager.Instance, um_mock.Object);

            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Visible, updateControl.Visibility);
        }

        [Test]
        public void UpdateButtonCollapsedIfUpToDate()
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            var fieldInfo = typeof(UpdateManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(UpdateManager.Instance, um_mock.Object);

            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }

        [Test]
        public void UpdateButtonCollapsedIfNotConnected()
        {
            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString(""));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            var fieldInfo = typeof(UpdateManager).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(UpdateManager.Instance, um_mock.Object);
            
            Init();

            var stb = (ShortcutToolbar)View.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }
    }
}

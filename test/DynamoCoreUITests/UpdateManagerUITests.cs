using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.UI.Controls;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.UpdateManager;
using DynamoCore.UI.Controls;
using NUnit.Framework;
using Moq;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class UpdateManagerUITests : DynamoTestUI
    {
        private void Init(IUpdateManager updateManager, ILogger logger)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            Controller = new DynamoController("None", updateManager, logger,
                new DefaultWatchHandler(), new PreferenceSettings());
            DynamoController.IsTestMode = true;
            Controller.DynamoViewModel = new DynamoViewModel(Controller, null);
            Controller.VisualizationManager = new VisualizationManager();

            //create the view
            Ui = new DynamoView { DataContext = Controller.DynamoViewModel };
            Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();                             

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                DynamoTestUI.EmptyTempFolder(TempFolder);
            }
        }

        [SetUp]
        public override void Start()
        {
            //override this to avoid the typical startup behavior
        }

        [Test]
        [Category("Failing")]
        public void UpdateButtonNotCollapsedIfNotUpToDate()
        {
            var logger = new DynamoLogger();

            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("1.1.1.1"));

            Init(um_mock.Object, logger);

            var stb = (ShortcutToolbar)Ui.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Visible, updateControl.Visibility);
        }

        [Test]
        [Category("Failing")]
        public void UpdateButtonCollapsedIfUpToDate()
        {
            var logger = new DynamoLogger();

            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("9.9.9.9"));

            Init(um_mock.Object, logger);

            var stb = (ShortcutToolbar)Ui.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }

        [Test]
        [Category("Failing")]
        public void UpdateButtonCollapsedIfNotConnected()
        {
            var logger = new DynamoLogger();

            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString(""));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            
            Init(um_mock.Object, logger);

            var stb = (ShortcutToolbar)Ui.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }
    }
}

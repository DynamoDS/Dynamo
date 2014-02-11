using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Tests.UI;
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
    public class UpdateManagerNotUpToDateTests : DynamoTestUI
    {
        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            um_mock.Setup(um => um.CheckForProductUpdate()).Callback(UpdateManagerTestHelpers.DoNothing);

            var env = new ExecutionEnvironment();
            Controller = new DynamoController(env, typeof(DynamoViewModel), "None", null, um_mock.Object);
            Controller.Testing = true;
            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
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
                EmptyTempFolder();
            }
        }

        [TearDown]
        public void Exit()
        {
            if (Ui.IsLoaded)
                Ui.Close();
        }

        [Test]
        public void UpdateButtonNotCollapsedIfNotUpToDate()
        {
            var stb = (ShortcutToolbar)Ui.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Visible, updateControl.Visibility);
        }
    }

    [TestFixture]
    public class UpdateManagerUpToDateTests : DynamoTestUI
    {
        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            var um_mock = new Mock<IUpdateManager>();
            um_mock.Setup(um => um.AvailableVersion).Returns(BinaryVersion.FromString("1.1.1.1"));
            um_mock.Setup(um => um.ProductVersion).Returns(BinaryVersion.FromString("9.9.9.9"));
            um_mock.Setup(um => um.CheckForProductUpdate()).Callback(UpdateManagerTestHelpers.DoNothing);

            var env = new ExecutionEnvironment();
            Controller = new DynamoController(env, typeof(DynamoViewModel), "None", null, um_mock.Object);

            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();
            Controller.Testing = true;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder();
            }
        }

        [TearDown]
        public void Exit()
        {
            if (Ui.IsLoaded)
                Ui.Close();
        }

        [Test]
        public void UpdateButtonCollapsedIfUpToDate()
        {
            var stb = (ShortcutToolbar)Ui.shortcutBarGrid.Children[0];
            var sbgrid = (Grid)stb.FindName("ShortcutToolbarGrid");
            var updateControl = (GraphUpdateNotificationControl)sbgrid.FindName("UpdateControl");
            Assert.AreEqual(Visibility.Collapsed, updateControl.Visibility);
        }
    }

    internal class UpdateManagerTestHelpers
    {
        public static void DoNothing()
        {
            Debug.WriteLine("Doing nothing.");
        }
    }

}

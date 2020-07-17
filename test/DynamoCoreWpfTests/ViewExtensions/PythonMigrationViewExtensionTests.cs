using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Models;
using Dynamo.PythonMigration;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        private PythonMigrationViewExtension viewExtension = new PythonMigrationViewExtension();


        private List<string> raisedEvents = new List<string>();


        /// <summary>
        /// This test is created to check if the extension displays a dialog to the user
        /// when opening a saved dyn file that contains python nodes with their engine set to `IronPython2`
        /// </summary>
        [Test]
        public void WillDisplayDialogWhenOpeningGraphWithIronPythonNodes()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            DynamoModel.IsTestMode = false;

            // Act
            // open file
            Open(@"core\python\python.dyn");

            var isIronPythonDialogOpen = this.View.OwnedWindows
                .Cast<Window>()
                .Any(x => x.GetType() == typeof(IronPythonInfoDialog));

            // Assert
            Assert.IsTrue(isIronPythonDialogOpen);
            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test is created to check if a notification is logged to the Dynamo Logger 
        /// when adding a using the IronPython engine
        /// </summary>
        [Test]
        public void WillLogNotificationWhenAddingAnIronPythonNode()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            // Arrange
            string pythonNodeName = "Python Script";
            raisedEvents = new List<string>();
            // Act
            // open file
            this.ViewModel.Model.Logger.NotificationLogged += Logger_NotificationLogged;

            var nodesCountBeforeNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            this.ViewModel.ExecuteCommand(new DynamoModel.
                CreateNodeCommand(Guid.NewGuid().ToString(), pythonNodeName, 0, 0, false, false));

            var nodesCountAfterNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            // Assert
            Assert.AreEqual(nodesCountBeforeNodeAdded + 1, nodesCountAfterNodeAdded);
            Assert.AreEqual(raisedEvents.Count, 1);
            Assert.IsTrue(raisedEvents.Any(x => x.Contains(nameof(PythonMigrationViewExtension))));
            raisedEvents.Clear();
            this.ViewModel.Model.Logger.NotificationLogged -= Logger_NotificationLogged;
            DispatcherUtil.DoEvents();
        }

        private void Logger_NotificationLogged(Dynamo.Logging.NotificationMessage obj)
        {
            raisedEvents.Add(obj.Sender);
        }

        /// <summary>
        /// This test verifies an IronPython warning notification is logged to the Dynamo Logger 
        /// only one time per open graph
        /// </summary>
        [Test]
        public void WillOnlyLogNotificationWhenAddingAnIronPythonNodeOnce()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));

            // Arrange
            string pythonNodeName = "Python Script";
            raisedEvents = new List<string>();

            // Act
            // open file
            this.ViewModel.Model.Logger.NotificationLogged += Logger_NotificationLogged;


            var nodesCountBeforeNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            this.ViewModel.ExecuteCommand(new DynamoModel.
                CreateNodeCommand(Guid.NewGuid().ToString(), pythonNodeName, 0, 0, false, false));
            this.ViewModel.ExecuteCommand(new DynamoModel.
                CreateNodeCommand(Guid.NewGuid().ToString(), pythonNodeName, 0, 0, false, false));

            DispatcherUtil.DoEvents();

            var nodesCountAfterNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            // Assert
            Assert.AreEqual(nodesCountBeforeNodeAdded + 2, nodesCountAfterNodeAdded);
            Assert.AreEqual(raisedEvents.Count, 1);
            Assert.IsTrue(raisedEvents.Any(x => x.Contains(nameof(PythonMigrationViewExtension))));
            raisedEvents.Clear();
            this.ViewModel.Model.Logger.NotificationLogged -= Logger_NotificationLogged;
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Adding Python nodes that use IronPython should not generate notifications
        /// when the Python2ObsoleteMode is disabled.
        /// </summary>
        [Test]
        public void WillNotLogNotificationWhenAddingNodeWhenPython2ObsoleteFlagIsOff()
        {
            var debugMode = DebugModes.GetDebugMode("Python2ObsoleteMode");
            bool shouldReenable = false;
            if (debugMode != null && debugMode.IsEnabled)
            {
                debugMode.IsEnabled = false;
                shouldReenable = true;
            }

            // Arrange
            string pythonNodeName = "Python Script";
            raisedEvents = new List<string>();

            // Act
            // open file
            this.ViewModel.Model.Logger.NotificationLogged += Logger_NotificationLogged;


            var nodesCountBeforeNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            this.ViewModel.ExecuteCommand(new DynamoModel.
                CreateNodeCommand(Guid.NewGuid().ToString(), pythonNodeName, 0, 0, false, false));

            DispatcherUtil.DoEvents();

            var nodesCountAfterNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            // Assert
            Assert.AreEqual(nodesCountBeforeNodeAdded + 1, nodesCountAfterNodeAdded);
            Assert.AreEqual(raisedEvents.Count, 0);
            raisedEvents.Clear();
            this.ViewModel.Model.Logger.NotificationLogged -= Logger_NotificationLogged;
            DispatcherUtil.DoEvents();

            if (shouldReenable)
            {
                debugMode.IsEnabled = true;
            }
        }


        /// <summary>
        /// This test verifies that the IronPython dialog wont show the second time a graph is opened
        /// even if it contains IronPython nodes
        /// </summary>
        [Test]
        public void WillNotDisplayDialogWhenOpeningGraphWithIronPythonNodesSecondTimeInSameSession()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            DynamoModel.IsTestMode = false;
            // Arrange
            var examplePathIronPython = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "python.dyn");
            var examplePathEmptyFile = Path.Combine(UnitTestBase.TestDirectory, @"core\Home.dyn");

            // Act
            // open file
            Open(examplePathIronPython);
            var ironPythonWorkspaceId = this.ViewModel.CurrentSpace.Guid;
            DispatcherUtil.DoEvents();

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();
            Assert.IsNotNull(ironPythonDialog);
            Assert.IsTrue(ironPythonDialog.IsLoaded);
            ironPythonDialog.OkBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            DispatcherUtil.DoEvents();
            // Open empty file before open the the IronPython file again
            Open(examplePathEmptyFile);
            Assert.AreNotEqual(ironPythonWorkspaceId, this.ViewModel.CurrentSpace.Guid);
            DispatcherUtil.DoEvents();

            Open(examplePathIronPython);
            Assert.AreEqual(ironPythonWorkspaceId, this.ViewModel.CurrentSpace.Guid);
            var secondGraphIronPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>();

            DispatcherUtil.DoEvents();
            // Assert
            Assert.AreEqual(0, secondGraphIronPythonDialog.Count());
            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test verifies that the IronPython dialog won't show 
        /// when the Do not show again property is enabled.
        /// </summary>
        [Test]
        public void WillNotDisplayIronPythonDialogAgainWhenDoNotShowAgainSettingIsChecked()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            DynamoModel.IsTestMode = false;
            // Arrange
            var examplePathIronPython = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "python.dyn");

            //Disable iron python alerts
            ViewModel.IsIronPythonDialogDisabled = true;

            // Act
            // open file
            Open(examplePathIronPython);
            DispatcherUtil.DoEvents();

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>();
            Assert.IsEmpty(ironPythonDialog);
            Assert.AreEqual(0, ironPythonDialog.Count());

            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This tests checks if the extension can detect IronPython nodes in the graph 
        /// </summary>
        [Test]
        public void CanDetectIronPythonNodesInGraph()
        {
            var extensionManager = View.viewExtensionManager;
            // Act
            // open file
            Open(@"core\python\python.dyn");

            var pythonMigration = extensionManager.ViewExtensions
                .FirstOrDefault(x => x.Name == viewExtension.Name)
                as PythonMigrationViewExtension;

            // Assert
            Assert.IsTrue(pythonMigration.PythonDependencies.ContainsIronPythonDependencyInCurrentWS());
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Checks if pressing the `More Information` button on the IronPythonInfoDialog window
        /// will open the DocumentaionBrowser ViewExtension.
        /// </summary>
        [Test]
        public void CanOpenDocumentationBrowserWhenMoreInformationIsClicked()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            DynamoModel.IsTestMode = false;

            // Act
            // open file
            var examplePath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "python.dyn");
            Open(examplePath);
            DispatcherUtil.DoEvents();

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();
            var viewExtensionTabsBeforeBtnClick = this.View.ExtensionTabItems.Count;
            DispatcherUtil.DoEvents();

            ironPythonDialog.MoreInformationBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var hasDocumentationBrowserTab = this.View.ExtensionTabItems
                .Any(x => x.Header.ToString() == "Documentation Browser");
            DispatcherUtil.DoEvents();

            // Assert
            Assert.AreEqual(viewExtensionTabsBeforeBtnClick + 1, this.View.ExtensionTabItems.Count);
            Assert.IsTrue(hasDocumentationBrowserTab);
            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test checks that the IronPython dialog is shown to the user,
        /// when the workspace has custom nodes that contain a python node in it. 
        /// </summary>
        [Test]
        public void WillDisplayDialogWhenCustomNodeInsideWorkspaceHasIronPythonNode()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python2ObsoleteMode.config"));
            DynamoModel.IsTestMode = false;

            // open file
            var examplePath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "PythonCustomNodeHomeWorkspace.dyn");
            Open(examplePath);
            DispatcherUtil.DoEvents();

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();

            // Assert that the IronPython dialog is shown. 
            Assert.IsNotNull(ironPythonDialog);
            Assert.IsTrue(ironPythonDialog.IsLoaded);

            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void CustomNodeContainsIronPythonDependencyTest()
        {
            // open file
            var examplePath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "PythonCustomNodeHomeWorkspace.dyn");
            Open(examplePath);

            var customNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<Function>();
            var customNodeManager = ViewModel.Model.CustomNodeManager;

            var result = GraphPythonDependencies.CustomNodesContainIronPythonDependency(customNodes, customNodeManager);

            Assert.IsTrue(result);
            DispatcherUtil.DoEvents();
        }
    }
}

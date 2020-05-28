using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PythonMigration;
using Dynamo.Scheduler;
using Dynamo.Utilities;
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
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python3DebugMode.config"));
            DynamoModel.IsTestMode = false;

            // Act
            // open file
            Open(@"core\python\python.dyn");

            var isIronPythonDialogOpen = this.View.OwnedWindows
                .Cast<Window>()
                .Any(x=>x.GetType() == typeof(IronPythonInfoDialog));
        
            // Assert
            Assert.IsTrue(isIronPythonDialogOpen);
            DynamoModel.IsTestMode = true;
        }

        /// <summary>
        /// This test is created to check if a notification is logged to the Dynamo Logger 
        /// when adding a using the IronPython engine
        /// </summary>
        [Test]
        public void WillLogNotificationWhenAddingAnIronPythonNode()
        {
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
            Assert.AreEqual(nodesCountBeforeNodeAdded+1, nodesCountAfterNodeAdded);
            Assert.AreEqual(raisedEvents.Count, 1);
            Assert.IsTrue(raisedEvents.Any(x => x.Contains(nameof(PythonMigrationViewExtension))));
            raisedEvents.Clear();
            this.ViewModel.Model.Logger.NotificationLogged -= Logger_NotificationLogged;
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

            var nodesCountAfterNodeAdded = this.ViewModel.CurrentSpace.Nodes.Count();

            // Assert
            Assert.AreEqual(nodesCountBeforeNodeAdded+2, nodesCountAfterNodeAdded);
            Assert.AreEqual(raisedEvents.Count, 1);
            Assert.IsTrue(raisedEvents.Any(x => x.Contains(nameof(PythonMigrationViewExtension))));
            raisedEvents.Clear();
            this.ViewModel.Model.Logger.NotificationLogged -= Logger_NotificationLogged;

        }


        /// <summary>
        /// This test verifies that the IronPython dialog wont show the second time a graph is opened
        /// even if it contains IronPython nodes
        /// </summary>
        [Test]
        public void WillNotDisplayDialogWhenOpeningGraphWithIronPythonNodesSecondTimeInSameSession()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python3DebugMode.config"));
            DynamoModel.IsTestMode = false;
            // Arrange
            var examplePathIronPython = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "python.dyn");
            var examplePathEmptyFile = Path.Combine(UnitTestBase.TestDirectory, @"core\Home.dyn");

            // Act
            // open file
            Open(examplePathIronPython);
            var ironPythonWorkspaceId = this.ViewModel.CurrentSpace.Guid;
            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();
            Assert.IsNotNull(ironPythonDialog);
            ironPythonDialog.OkBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Open empty file before open the the IronPython file again
            Open(examplePathEmptyFile);
            Assert.AreNotEqual(ironPythonWorkspaceId, this.ViewModel.CurrentSpace.Guid);
            

            Open(examplePathIronPython);
            Assert.AreEqual(ironPythonWorkspaceId, this.ViewModel.CurrentSpace.Guid);
            var secondGraphIronPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>();

            // Assert
            Assert.AreEqual(0, secondGraphIronPythonDialog.Count());
            DynamoModel.IsTestMode = true;
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
                .Where(x => x.Name == viewExtension.Name)
                .Select(x=>x)
                .First() as PythonMigrationViewExtension;

            // Assert
            Assert.IsTrue(pythonMigration.PythonDependencies.ContainsIronPythonDependencies());
        }

        /// <summary>
        /// Checks if pressing the `More Information` button on the IronPythonInfoDialog window
        /// will open the DocumentaionBrowser ViewExtension.
        /// </summary>
        [Test]
        public void CanOpenDocumentationBrowserWhenMoreInformationIsClicked()
        {
            DebugModes.LoadDebugModesStatusFromConfig(Path.Combine(GetTestDirectory(ExecutingDirectory), "DynamoCoreWpfTests", "python3DebugMode.config"));
            DynamoModel.IsTestMode = false;

            // Act
            // open file
            var examplePath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "python.dyn");
            Open(examplePath);

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();
            var viewExtensionTabsBeforeBtnClick = this.View.ExtensionTabItems.Count;

            ironPythonDialog.MoreInformationBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var hasDocumentationBrowserTab = this.View.ExtensionTabItems
                .Any(x => x.Header.ToString() == "Documentation Browser");

            // Assert
            Assert.AreEqual(viewExtensionTabsBeforeBtnClick + 1, this.View.ExtensionTabItems.Count);
            Assert.IsTrue(hasDocumentationBrowserTab);
            DynamoModel.IsTestMode = true;
        }

    }
}

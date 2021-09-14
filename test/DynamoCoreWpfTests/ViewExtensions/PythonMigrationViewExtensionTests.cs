using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PythonMigration;
using Dynamo.PythonMigration.Controls;
using Dynamo.Utilities;
using Dynamo.Views;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using PythonNodeModelsWpf;

namespace DynamoCoreWpfTests
{
    class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        private readonly PythonMigrationViewExtension viewExtension = new PythonMigrationViewExtension();
        private string CoreTestDirectory { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "core"); } }

        private List<string> raisedEvents = new List<string>();

        /// <summary>
        /// This test is created to check if the extension displays a dialog to the user
        /// when opening a saved dyn file that contains python nodes with their engine set to `IronPython2`
        /// </summary>
        [Test]
        public void WillDisplayDialogWhenOpeningGraphWithIronPythonNodes()
        {
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
        /// This test verifies that the IronPython dialog wont show the second time a graph is opened
        /// even if it contains IronPython nodes
        /// </summary>
        [Test]
        public void WillNotDisplayDialogWhenOpeningGraphWithIronPythonNodesSecondTimeInSameSession()
        {
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
            Assert.IsTrue(pythonMigration.PythonDependencies.CurrentWorkspaceHasIronPythonDependency());
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Checks if pressing the `More Information` button on the IronPythonInfoDialog window
        /// will open the DocumentationBrowser ViewExtension.
        /// </summary>
        [Test]
        public void CanOpenDocumentationBrowserWhenMoreInformationIsClicked()
        {
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

            var pythonMigration = View.viewExtensionManager.ViewExtensions
                .Where(x => x.Name == viewExtension.Name)
                .Select(x => x)
                .First() as PythonMigrationViewExtension;

            var result = pythonMigration.PythonDependencies.CurrentWorkspaceHasIronPythonDependency();

            Assert.IsTrue(result);
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test verifies that the IronPython dialog wont show the second time a custom node is opened
        /// even if it contains IronPython nodes
        /// </summary>
        [Test]
        public void WillNotDisplayDialogWhenOpeningCustomNodeWithIronPythonNodesSecondTimeInSameSession()
        {
            DynamoModel.IsTestMode = false;
            // Arrange
            var examplePathIronPython = Path.Combine(UnitTestBase.TestDirectory, @"core\python", "PythonCustomNodeTest.dyf");
            var examplePathEmptyFile = Path.Combine(UnitTestBase.TestDirectory, @"core\Home.dyn");

            // Act
            // open file
            Open(examplePathIronPython);
            var ironPythonCustomNodeId = (this.ViewModel.CurrentSpace as CustomNodeWorkspaceModel).CustomNodeId;
            DispatcherUtil.DoEvents();

            var ironPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>().First();
            Assert.IsNotNull(ironPythonDialog);
            Assert.IsTrue(ironPythonDialog.IsLoaded);
            ironPythonDialog.OkBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            DispatcherUtil.DoEvents();
            // Open empty file before open the the IronPython file again
            Open(examplePathEmptyFile);
            Assert.AreNotEqual(ironPythonCustomNodeId, this.ViewModel.CurrentSpace.Guid);
            DispatcherUtil.DoEvents();

            Open(examplePathIronPython);
            Assert.AreEqual(ironPythonCustomNodeId, (this.ViewModel.CurrentSpace as CustomNodeWorkspaceModel).CustomNodeId);
            var secondIronPythonDialog = this.View.GetChildrenWindowsOfType<IronPythonInfoDialog>();

            DispatcherUtil.DoEvents();
            // Assert
            Assert.AreEqual(0, secondIronPythonDialog.Count());
            DynamoModel.IsTestMode = true;
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void IronPythonPackageLoadedTest()
        {
            RaiseLoadedEvent(View);

            Open(Path.Combine(CoreTestDirectory, @"python\python.dyn"));

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.Loaded(loadedParams);

            var currentWorkspace = ViewModel.Model.CurrentWorkspace;

            var pkgDependencyInfo = currentWorkspace.OnRequestPackageDependencies().FirstOrDefault();

            Assert.IsTrue(pkgDependencyInfo != null);
            Assert.AreEqual(PackageDependencyState.Loaded, pkgDependencyInfo.State);
            Assert.AreEqual(GraphPythonDependencies.PythonPackage, pkgDependencyInfo.Name);
            Assert.AreEqual(GraphPythonDependencies.PythonPackageVersion, pkgDependencyInfo.Version);
        }

        [Test]
        public void MigratingPython2CodeWillSaveBackupFile()
        {
            // Arrange
            var dynFileName = "2to3Test.dyn";
            var dynBackupFileName = "2to3Test.Python2.dyn";
            DynamoModel.IsTestMode = true;

            var python2dynPath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", dynFileName);
            var backupFilePath = Path.Combine(Model.PathManager.BackupDirectory, dynBackupFileName);
            if (File.Exists(backupFilePath))
                File.Delete(backupFilePath);

            Open(python2dynPath);
            DispatcherUtil.DoEvents();

            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");

            var nodeView = currentWs.ChildOfType<NodeView>();

            var editMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Edit...");

            editMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            DispatcherUtil.DoEvents();

            // Act
            var scriptEditor = View.GetChildrenWindowsOfType<ScriptEditorWindow>().FirstOrDefault();
            scriptEditor.MigrationAssistantBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            DispatcherUtil.DoEvents();
            var assistantWindow = scriptEditor.GetChildrenWindowsOfType<BaseDiffViewer>().FirstOrDefault();
            assistantWindow.AcceptButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            // Assert
            Assert.That(File.Exists(backupFilePath));

            // Clean up
            File.Delete(backupFilePath);
        }

        [Test]
        public void MigratingPython2CodeThatResultsInImaginaryCaseDoesNotCrash()
        {
            // Arrange
            var dynFileName = "pymigrateimaginarydiff.dyn";
            var dynBackupFileName = "pymigrateimaginarydiff.Python2.dyn";
            DynamoModel.IsTestMode = true;

            var python2dynPath = Path.Combine(UnitTestBase.TestDirectory, @"core\python", dynFileName);
            var backupFilePath = Path.Combine(Model.PathManager.BackupDirectory, dynBackupFileName);
            if (File.Exists(backupFilePath))
                File.Delete(backupFilePath);

            Open(python2dynPath);
            DispatcherUtil.DoEvents();

            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");

            var nodeView = currentWs.ChildOfType<NodeView>();

            var editMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Edit...");

            editMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            DispatcherUtil.DoEvents();

            // Act
            Assert.DoesNotThrow(() => {
                var scriptEditor = View.GetChildrenWindowsOfType<ScriptEditorWindow>().FirstOrDefault();
                scriptEditor.MigrationAssistantBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                DispatcherUtil.DoEvents();
                var assistantWindow = scriptEditor.GetChildrenWindowsOfType<BaseDiffViewer>().FirstOrDefault();
                assistantWindow.AcceptButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                });
            // Assert
            Assert.That(File.Exists(backupFilePath));

            // Clean up
            File.Delete(backupFilePath);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PythonMigration;
using Dynamo.PythonMigration.Controls;
using Dynamo.Utilities;
using Dynamo.Views;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using PythonNodeModels;
using PythonNodeModelsWpf;

namespace IronPythonTests
{
    class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        private readonly PythonMigrationViewExtension viewExtension = new PythonMigrationViewExtension();
        private string CoreTestDirectory { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "core"); } }

        private List<string> raisedEvents = new List<string>();

        private static void SetEngineViaContextMenu(NodeView nodeView, PythonEngineVersion engine)
        {
            var engineSelection = nodeView.MainContextMenu.Items
                      .OfType<MenuItem>()
                      .Where(item => (item.Header as string) == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher).FirstOrDefault();
            switch (engine)
            {
                case PythonEngineVersion.IronPython2:
                    (engineSelection.Items[0] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
                case PythonEngineVersion.CPython3:
                    (engineSelection.Items[1] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
            }
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
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.EditHeader);

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
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.EditHeader);

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

        [Test]
        public void WorkspaceWithMultiplePythonEnginesUpdatesCorrectlyViaContextHandler()
        {
            // open test graph
            Open(@"core\python\WorkspaceWithMultiplePythonEngines.dyn");

            var nodeModels = ViewModel.Model.CurrentWorkspace.Nodes.Where(n => n.NodeType == "PythonScriptNode");
            List<PythonNode> pythonNodes = nodeModels.Cast<PythonNode>().ToList();
            var pynode1 = pythonNodes.ElementAt(0);
            var pynode2 = pythonNodes.ElementAt(1);
            var pynode1view = NodeViewWithGuid("d060e68f-510f-43fe-8990-c2c1ba7e0f80");
            var pynode2view = NodeViewWithGuid("4050d23e-529c-43e9-b614-0506d8adb06b");


            Assert.AreEqual(new List<string> { "2.7.9", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x => x.Data));

            SetEngineViaContextMenu(pynode1view, PythonEngineVersion.CPython3);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "3.9.12", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x => x.Data));

            SetEngineViaContextMenu(pynode2view, PythonEngineVersion.CPython3);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "3.9.12", "3.9.12" }, pynode2.CachedValue.GetElements().Select(x => x.Data));

            SetEngineViaContextMenu(pynode1view, PythonEngineVersion.IronPython2);
            SetEngineViaContextMenu(pynode2view, PythonEngineVersion.IronPython2);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "2.7.9", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x => x.Data));
            DispatcherUtil.DoEvents();

            Model.CurrentWorkspace.Undo();
            Assert.AreEqual(new List<string> { "2.7.9", "3.9.12" }, pynode2.CachedValue.GetElements().Select(x => x.Data));
            DispatcherUtil.DoEvents();
            Model.CurrentWorkspace.Undo();
            Assert.AreEqual(new List<string> { "3.9.12", "3.9.12" }, pynode2.CachedValue.GetElements().Select(x => x.Data));
            DispatcherUtil.DoEvents();

        }
    }
}

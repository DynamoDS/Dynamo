using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.GraphNodeManager;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Graph Node Manager tests for the Autodesk Assistant modification marker.
    /// Kept separate from GraphNodeManagerViewExtensionTests, which is marked [Category("Failure")],
    /// so these tests run with the regular unit/regression selection.
    /// </summary>
    public class GraphNodeManagerAIMarkerTests : DynamoTestUIBase
    {
        private const string AIIconName = "nodeModifiedByAIIcon";

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            // Work on a private copy so the test never rewrites the shared settings file
            string source = Path.Combine(GetTestDirectory(ExecutingDirectory), "settings", "DynamoSettings-ViewExtension.xml");
            string settingsPath = Path.Combine(TempFolder, "DynamoSettings-ViewExtension.xml");
            File.Copy(source, settingsPath, true);
            PreferenceSettings.DynamoTestPath = settingsPath;

            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = PreferenceSettings.Load(settingsPath)
            };
        }

        [Test]
        public void WhenNodeNotModifiedByAIThenIconIsCollapsed()
        {
            // Arrange
            var (node, grid) = OpenGraphInNodeManager();

            // Act
            var icon = GetAIIcon(grid, node.GUID);

            // Assert
            Assert.AreEqual(Visibility.Collapsed, icon.Visibility);
        }

        [Test]
        public void WhenAIIconIsRenderedThenImageSourceResolves()
        {
            // Arrange
            var (node, grid) = OpenGraphInNodeManager();

            // Act
            var bitmap = GetAIIcon(grid, node.GUID).Source as BitmapSource;

            // Assert
            Assert.IsNotNull(bitmap, "AI icon has no image source.");
            Assert.Greater(bitmap.PixelWidth, 0);
        }

        [Test]
        public void WhenNodeModifiedByAIThenIconIsVisible()
        {
            // Arrange
            var (node, grid) = OpenGraphInNodeManager();

            // Act
            node.IsRecentlyModifiedByAI = true;
            DispatcherUtil.DoEvents();

            // Assert
            Assert.AreEqual(Visibility.Visible, GetAIIcon(grid, node.GUID).Visibility);
        }

        [Test]
        public void WhenAIFlagClearedThenIconIsCollapsed()
        {
            // Arrange
            var (node, grid) = OpenGraphInNodeManager();
            node.IsRecentlyModifiedByAI = true;
            DispatcherUtil.DoEvents();

            // Act
            node.IsRecentlyModifiedByAI = false;
            DispatcherUtil.DoEvents();

            // Assert
            Assert.AreEqual(Visibility.Collapsed, GetAIIcon(grid, node.GUID).Visibility);
        }

        /// <summary>
        /// Opens the Graph Node Manager and a sample graph, and returns the first node and the manager's grid.
        /// </summary>
        private (NodeModel node, DataGrid grid) OpenGraphInNodeManager()
        {
            var viewExt = View.viewExtensionManager.ViewExtensions
                .OfType<GraphNodeManagerViewExtension>()
                .First();
            viewExt.graphNodeManagerMenuItem.IsChecked = true;

            Open(@"pkgs\Dynamo Samples\extra\ZoomNodeColorStates.dyn");

            var node = ViewModel.CurrentSpace.Nodes.First();
            return (node, viewExt.ManagerView.NodesInfoDataGrid);
        }

        private static Image GetAIIcon(DataGrid grid, Guid nodeGuid)
        {
            Image icon = null;
            string lastStep = "not started";

            DispatcherUtil.DoEventsLoop(() =>
            {
                var item = grid.Items.OfType<GridNodeViewModel>().FirstOrDefault(n => n.NodeGuid == nodeGuid);
                if (item == null) { lastStep = "node not in grid.Items"; return false; }

                grid.ScrollIntoView(item);
                grid.UpdateLayout();

                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row == null)
                {
                    lastStep = $"row not realized (grid.IsVisible={grid.IsVisible}, ActualHeight={grid.ActualHeight})";
                    return false;
                }

                icon = WpfUtilities.ChildrenOfType<Image>(row).FirstOrDefault(i => i.Name == AIIconName);
                if (icon == null) lastStep = "row found, but no Image named " + AIIconName;
                return icon != null;
            }, timeoutSeconds: 10);

            Assert.IsNotNull(icon, "AI icon was not rendered in the node's row. Last step: " + lastStep);
            return icon;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Graph Node Manager tests for the Autodesk Assistant modification marker (DYN-10990).
    /// Kept separate from GraphNodeManagerViewExtensionTests, which is marked [Category("Failure")],
    /// so these tests run with the regular unit/regression selection.
    /// </summary>
    public class GraphNodeManagerAIMarkerTests : DynamoTestUIBase
    {
        private const string AIIconName = "nodeModifiedByAIIcon";
        private bool oldEnablePersistance = false;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            string settingDirectory = Path.Combine(GetTestDirectory(ExecutingDirectory), "settings");
            string viewExtSettingFilePath = Path.Combine(settingDirectory, "DynamoSettings-ViewExtension.xml");
            PreferenceSettings.DynamoTestPath = viewExtSettingFilePath;

            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = PreferenceSettings.Load(viewExtSettingFilePath)
            };
        }

        [SetUp]
        public void Setup()
        {
            oldEnablePersistance = ViewModel.PreferenceSettings.EnablePersistExtensions;
            ViewModel.PreferenceSettings.EnablePersistExtensions = false;
        }

        [TearDown]
        public void Teardown()
        {
            ViewModel.PreferenceSettings.EnablePersistExtensions = oldEnablePersistance;
        }

        /// <summary>
        /// The rendered AI icon in the Name column follows NodeModel.IsRecentlyModifiedByAI:
        /// collapsed by default, visible when the node is marked, collapsed again when the mark is cleared.
        /// </summary>
        [Test]
        public void RecentlyModifiedByAIIconVisibilityTest()
        {
            // Open the Graph Node Manager
            var viewExt = View.viewExtensionManager.ViewExtensions
                .OfType<GraphNodeManagerViewExtension>()
                .First();
            viewExt.graphNodeManagerMenuItem.IsChecked = true;

            Open(@"pkgs\Dynamo Samples\extra\ZoomNodeColorStates.dyn");

            var hwm = ViewModel.CurrentSpace as HomeWorkspaceModel;
            var node = hwm.Nodes.First();
            var grid = viewExt.ManagerView.NodesInfoDataGrid;

            // Not marked by default: icon is in the row but collapsed
            var icon = GetAIIcon(grid, node.GUID);
            Assert.IsNotNull(icon, "AI icon was not rendered in the node's row.");
            Assert.AreEqual(Visibility.Collapsed, icon.Visibility);

            // The image source resolves to a real bitmap
            var bitmap = icon.Source as BitmapSource;
            Assert.IsNotNull(bitmap, "AI icon has no image source.");
            Assert.Greater(bitmap.PixelWidth, 0);

            // Autodesk Assistant marks the node: icon becomes visible
            node.IsRecentlyModifiedByAI = true;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Visible, GetAIIcon(grid, node.GUID).Visibility);

            // Transient: clearing the mark collapses the icon again
            node.IsRecentlyModifiedByAI = false;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Collapsed, GetAIIcon(grid, node.GUID).Visibility);
        }

        /// <summary>
        /// Finds the AI icon rendered in the DataGrid row of the given node
        /// </summary>
        private static Image GetAIIcon(DataGrid grid, Guid nodeGuid)
        {
            var item = grid.Items.OfType<GridNodeViewModel>().FirstOrDefault(n => n.NodeGuid == nodeGuid);
            if (item == null) return null;

            Image icon = null;
            DispatcherUtil.DoEventsLoop(() =>
            {
                // The grid virtualizes rows, so make sure this one is realized
                grid.ScrollIntoView(item);
                grid.UpdateLayout();

                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                icon = row == null
                    ? null
                    : WpfUtilities.ChildrenOfType<Image>(row).FirstOrDefault(i => i.Name == AIIconName);
                return icon != null;
            });

            return icon;
        }
    }
}

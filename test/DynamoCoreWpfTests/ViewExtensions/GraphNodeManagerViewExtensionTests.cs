using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;

using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class GraphNodeManagerViewExtensionTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings() { CustomPackageFolders = new List<string>() { this.PackagesDirectory } }
            };
        }

        #region Helpers
        /// <summary>
        /// Loads the extension in Dynamo
        /// </summary>
        /// <param name="v"></param>
        private void LoadExtension(GraphNodeManagerViewExtension v)
        {
            MenuItem item = v.graphNodeManagerMenuItem;
            item.IsChecked = true;
        }
        #endregion

        #region Tests
        /// <summary>
        /// Test if the Extension loads correctly
        /// </summary>
        [Test]
        public void ViewExtensionOpenTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            var viewExtension = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            // Open
            LoadExtension(viewExtension);

            Assert.AreEqual(1, View.ExtensionTabItems.Count);

            // Close
            Utility.DispatcherUtil.DoEvents();
            View.CloseExtensionTab(WpfUtilities.ChildrenOfType<Button>(View.ExtensionTabItems.FirstOrDefault()).FirstOrDefault(), null);
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
        }

        /// <summary>
        /// Test if the number of nodes displayed in the extension is equal to current number of nodes
        /// </summary>
        [Test]
        public void CorrectNumberNodeItemsTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            var viewExt = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            var hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;

            // Arrange
            LoadExtension(viewExt);
            
            var view = viewExt.ManagerView;
            var dataGridItems = view.NodesInfoDataGrid.Items;

            int startGraphNodes = hwm.Nodes.Count();
            int startExtensionNodes = dataGridItems.Count;

            Open(@"pkgs\Dynamo Samples\extra\ZoomNodeColorStates.dyn");

            hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;
            Utility.DispatcherUtil.DoEvents();

            int loadedGraphNodes = hwm.Nodes.Count();
            int loadedExtensionNodes = dataGridItems.Count;

            // Deleting a node to check the results
            hwm.RemoveAndDisposeNode(hwm.Nodes.First());

            hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;
            Utility.DispatcherUtil.DoEvents();

            int deleteGraphNodes = hwm.Nodes.Count();
            int deleteExtensionNodes = dataGridItems.Count;

            // Assert
            Assert.AreEqual(startGraphNodes, startExtensionNodes);
            Assert.AreEqual(loadedGraphNodes, loadedExtensionNodes);
            Assert.AreEqual(deleteGraphNodes, deleteExtensionNodes);
        }
        #endregion

    }
}

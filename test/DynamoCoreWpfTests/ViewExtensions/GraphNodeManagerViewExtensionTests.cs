using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using NUnit.Framework;
using ProtoCore.Mirror;

namespace DynamoCoreWpfTests
{
    public class GraphNodeManagerViewExtensionTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

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
        /// <summary>
        /// Test if using the IsFrozen filter yields correct results
        /// </summary>
        [Test]
        public void FilterFrozenItemsTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            var viewExt = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            // Arrange
            LoadExtension(viewExt);

            Open(@"pkgs\Dynamo Samples\extra\ZoomNodeColorStates.dyn");
            Utility.DispatcherUtil.DoEvents();

            // Get number of frozen Nodes in the graph
            var hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;
            int frozenNodes = hwm.Nodes.Count(n => n.IsFrozen);


            // Activate the 'Frozen' filter
            var view = viewExt.ManagerView;
            string frozenFilterTitle = "Frozen";

            foreach (var item in view.FilterItemControl.Items)
            {
                FilterViewModel fvm = item as FilterViewModel;
                if (fvm.Name.Equals(frozenFilterTitle))
                {
                    fvm.Toggle(null);
                    break;
                }
            }

            Utility.DispatcherUtil.DoEvents();

            // Get the number of frozen Nodes in the Extension
            var dataGridItems = view.NodesInfoDataGrid.Items;
            int frozenUINodes = dataGridItems.Count;

            // Assert
            Assert.AreEqual(frozenNodes, frozenUINodes);
        }

        /// <summary>
        /// Test if the number of Nodes containing Null or Empty List matches what is shown on the UI
        /// </summary>
        [Test]
        public void ContainsEmptyListOrNullTest()
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

            Open(@"pkgs\Dynamo Samples\extra\GraphNodeManagerTestGraph_NullsEmptyLists.dyn");

            hwm = this.ViewModel.CurrentSpace as HomeWorkspaceModel;
            hwm.Run();

            Utility.DispatcherUtil.DoEvents();

            var images = WpfUtilities.ChildrenOfType<Image>(view.NodesInfoDataGrid);
            
            int nullNodesImageCount = GetImageCount(images, "Null");
            int emptyListNodesImageCount = GetImageCount(images, "EmptyList"); 

            int nullNodesCount = hwm.Nodes.Count(ContainsAnyNulls);
            int emptyListNodesCount = hwm.Nodes.Count(ContainsAnyEmptyLists);

            // Assert
            Assert.AreEqual(emptyListNodesCount, emptyListNodesImageCount);
            Assert.AreEqual(nullNodesCount, nullNodesImageCount);
        }

        #region EnablePersistExtensions Tests
        /// <summary>
        /// Test if the Extension loads correctly when remembered
        /// </summary>
        [Test]
        public void ViewExtensionOpensWithDynamoWhenRememberedTest()
        {
            RaiseLoadedEvent(this.View);
            ViewModel.PreferenceSettings.EnablePersistExtensions = true;

            //assert that option is enabled
            Assert.IsTrue(ViewModel.PreferenceSettings.EnablePersistExtensions);

            //open extension
            var extensionManager = View.viewExtensionManager;
            var viewExtension = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;
            LoadExtension(viewExtension);

            //confirm that extension was opened
            Assert.AreEqual(1, View.ExtensionTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is reopened after restart
            Assert.AreEqual(1, View.ExtensionTabItems.Count);
            Assert.IsNotNull(View.ExtensionTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
        }

        /// <summary>
        /// Test if the Extension does not open when closed in the last session, and remember setting was enabled.
        /// </summary>
        [Test]
        public void ViewExtensionDoesNotOpensWithDynamoWhenClosedTest()
        {
            RaiseLoadedEvent(this.View);
            ViewModel.PreferenceSettings.EnablePersistExtensions = true;

            //assert that option is enabled
            Assert.IsTrue(ViewModel.PreferenceSettings.EnablePersistExtensions);

            //open extension
            var extensionManager = View.viewExtensionManager;
            var viewExtension = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;
            LoadExtension(viewExtension);

            //close extension
            var loadedParams = new ViewLoadedParams(View, ViewModel);
            loadedParams.CloseExtensioninInSideBar(viewExtension);

            //confirm that extension was closed
            Assert.AreEqual(0, View.ExtensionTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is still closed after restart
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
            Assert.IsNull(View.ExtensionTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
        }

        /// <summary>
        /// Test if the Extension loads correctly when remembered
        /// </summary>
        [Test]
        public void ViewExtensionDoesNotOpenWhenNotRememberedTest()
        {
            RaiseLoadedEvent(this.View);
            ViewModel.PreferenceSettings.EnablePersistExtensions = false;

            //assert that option is disabled
            Assert.IsFalse(ViewModel.PreferenceSettings.EnablePersistExtensions);

            //open extension
            var extensionManager = View.viewExtensionManager;
            var viewExtension = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;
            LoadExtension(viewExtension);

            //confirm that extension was opened
            Assert.AreEqual(1, View.ExtensionTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is still closed after restart
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
            Assert.IsNull(View.ExtensionTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
        }
        #endregion

        #endregion

        #region Utilities

        /// <summary>
        /// Get the number of image elements containing a string
        /// </summary>
        /// <param name="images"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private int GetImageCount(IEnumerable<Image> images, string match)
        {
            int i = 0;
            foreach (var image in images)
            {
                if (image.Source == null) continue;
                if (image.Visibility != Visibility.Visible) continue;
                if (image.Source.ToString().Contains(match))
                {
                    i++;
                }
            }

            return i;
        }

        private bool ContainsAnyEmptyLists(NodeModel nodeModel)
        {
            return IsNodeEmptyList(nodeModel.CachedValue);
        }

        private bool ContainsAnyNulls(NodeModel nodeModel)
        {
            return IsNodeNull(nodeModel.CachedValue);
        }

        /// <summary>
        ///  Returns true only if the node contains ANY (nested) empty lists 
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeEmptyList(MirrorData mirrorData)
        {
            if (mirrorData == null) return false;
            if (mirrorData.IsCollection)
            {
                try
                {
                    var list = mirrorData.GetElements();
                    if (!list.ToList().Any()) return true;

                    foreach (var nested in list)
                    {
                        if (IsNodeEmptyList(nested))
                            return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the Node contains ANY (nested) null values
        /// </summary>
        /// <param name="mirrorData"></param>
        /// <returns></returns>
        private bool IsNodeNull(MirrorData mirrorData)
        {
            if (mirrorData == null) return false;
            if (mirrorData.IsCollection)
            {
                try
                {
                    var list = mirrorData.GetElements();
                    foreach (var nested in list)
                    {
                        if (IsNodeNull(nested))
                            return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            if (mirrorData.IsNull) return true;
            return false;
        }

        public static IEnumerable<T> FindVisualChilds<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChilds<T>(ithChild)) yield return childOfChild;
            }
        }

        #endregion
    }
}

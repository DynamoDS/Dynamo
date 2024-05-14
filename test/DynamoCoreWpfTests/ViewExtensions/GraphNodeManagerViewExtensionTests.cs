using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.GraphNodeManager;
using Dynamo.GraphNodeManager.ViewModels;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.Wpf.Extensions;
using NUnit.Framework;
using ProtoCore.Mirror;

namespace DynamoCoreWpfTests
{
    [Category("Failure")]
    public class GraphNodeManagerViewExtensionTests : DynamoTestUIBase
    {
        private bool oldEnablePersistance = false;

        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

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
        /// Test if the Extension loads correctly
        /// </summary>
        [Test]
        public void ViewExtensionOpenTest()
        {
            var extensionManager = View.viewExtensionManager;
            var viewExtension = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            // Open
            LoadExtension(viewExtension);

            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            // Close
            Utility.DispatcherUtil.DoEvents();
            View.OnCloseRightSideBarTab(WpfUtilities.ChildrenOfType<Button>(ViewModel.SideBarTabItems.FirstOrDefault()).FirstOrDefault(), null);
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
        }

        /// <summary>
        /// Test if the number of nodes displayed in the extension is equal to current number of nodes
        /// </summary>
        [Test]
        public void CorrectNumberNodeItemsTest()
        {
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
            var extensionManager = View.viewExtensionManager;
            var viewExt = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            // Arrange
            LoadExtension(viewExt);

            Open(@"pkgs\Dynamo Samples\extra\ZoomNodeColorStates.dyn");

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
        /// Marked as Failure until we can fix flakyness
        /// </summary>
        //TODO https://jira.autodesk.com/browse/DYN-6973
        [Category("Failure")]
        [Test]
        public void ContainsEmptyListOrNullTest()
        {
            var extensionManager = View.viewExtensionManager;
            var viewExt = extensionManager.ViewExtensions
                    .FirstOrDefault(x => x as GraphNodeManagerViewExtension != null)
                as GraphNodeManagerViewExtension;

            LoadExtension(viewExt);

            OpenAndRun(@"pkgs\Dynamo Samples\extra\GraphNodeManagerTestGraph_NullsEmptyLists.dyn");

            var hwm = this.ViewModel.CurrentSpace;

            int nullNodesCount = hwm.Nodes.Count(ContainsAnyNulls);
            int emptyListNodesCount = hwm.Nodes.Count(ContainsAnyEmptyLists);

            var view = viewExt.ManagerView;
            var images = WpfUtilities.ChildrenOfType<Image>(view.NodesInfoDataGrid);

            Utility.DispatcherUtil.DoEventsLoop(() =>
            {
                int nullNodesImageCount = GetImageCount(images, "Null");
                int emptyListNodesImageCount = GetImageCount(images, "EmptyList");

                return (nullNodesImageCount == nullNodesCount) && (emptyListNodesImageCount == emptyListNodesCount);
            });

            int nullNodesImageCount = GetImageCount(images, "Null");
            int emptyListNodesImageCount = GetImageCount(images, "EmptyList");

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
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is reopened after restart
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsNotNull(ViewModel.SideBarTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
        }

        /// <summary>
        /// Test if the Extension does not open when closed in the last session, and remember setting was enabled.
        /// </summary>
        [Test]
        public void ViewExtensionDoesNotOpensWithDynamoWhenClosedTest()
        {
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
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is still closed after restart
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsNull(ViewModel.SideBarTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
        }

        /// <summary>
        /// Test if the Extension loads correctly when remembered
        /// </summary>
        [Test]
        public void ViewExtensionDoesNotOpenWhenNotRememberedTest()
        {
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
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            //Restart Dynamo
            Exit();
            Start();

            Utility.DispatcherUtil.DoEvents();

            //confirm that extension is still closed after restart
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsNull(ViewModel.SideBarTabItems.FirstOrDefault(x => x.Tag as GraphNodeManagerViewExtension != null));
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

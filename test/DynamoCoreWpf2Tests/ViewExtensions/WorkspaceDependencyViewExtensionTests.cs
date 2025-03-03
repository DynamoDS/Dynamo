using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Dynamo.WorkspaceDependency;
using Dynamo.WorkspaceDependency.Properties;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class WorkspaceDependencyViewExtensionTests : DynamoTestUIBase
    {
        private WorkspaceDependencyViewExtension viewExtension = new WorkspaceDependencyViewExtension();

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

        private WorkspaceDependencyViewExtension WorkspaceReferencesExtension
        {
            get
            {
                return (WorkspaceDependencyViewExtension)View.viewExtensionManager.ViewExtensions.FirstOrDefault(x => x.Name.Equals(Resources.ExtensionName));
            }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            string path = Path.Combine(PackagesDirectory, "Custom Rounding", "extra", "DLL.dll");
            libraries.Add(path);
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void RestartBannerDefaultStateTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.Loaded(loadedParams);

            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;
            viewExtension.DependencyRegen(CurrentWorkspace);
            // Restart banner should not display by default
            Assert.AreEqual(Visibility.Hidden, viewExtension.DependencyView.RestartBanner.Visibility);
        }


        [Test]
        public void DownloadSpecifiedVersionOfPackageTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.Loaded(loadedParams);

            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;

            // This is equivalent to uninstall the package
            var package = viewExtension.pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == "Dynamo Samples").FirstOrDefault();
            package.LoadState.SetScheduledForDeletion();

            // Once choosing to install the specified version, info.State should reflect RequireRestart
            viewExtension.DependencyRegen(CurrentWorkspace);

            // Restart banner should display immediately
            Assert.AreEqual(Visibility.Visible, viewExtension.DependencyView.RestartBanner.Visibility);
            Assert.AreEqual(1, viewExtension.DependencyView.PackageDependencyTable.Items.Count);
            var newInfo = viewExtension.dataRows.FirstOrDefault().DependencyInfo;

            // Local loaded version was 2.0.0, but now will be update to date with dyn
            Assert.AreEqual("2.0.1", newInfo.Version.ToString());
            Assert.AreEqual(1, newInfo.Nodes.Count);
            Assert.AreEqual(newInfo.State, PackageDependencyState.RequiresRestart);
        }

        /// <summary>
        /// This test is created to guard the clicking behavior on tab closing button
        /// and make sure it actually close the corresponding tab
        /// </summary>
        [Test]
        public void ClickingToCloseViewExtensionTabTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            // Open a graph which should bring up the Workspace References view extension window with one tab
            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            DispatcherUtil.DoEvents();
            View.OnCloseRightSideBarTab(WpfUtilities.ChildrenOfType<Button>(ViewModel.SideBarTabItems.FirstOrDefault()).FirstOrDefault(), null);
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
        }

        /// <summary>
        /// This test is created to guard the tab closing API - CloseExtensioninInSideBar()
        /// and make sure it actually close the corresponding tab
        /// </summary>
        [Test]
        public void APItoCloseViewExtensionTabTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            // Open a graph which should bring up the Workspace References view extension window with one tab
            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            loadedParams.CloseExtensioninInSideBar(this.viewExtension);
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
        }

        /// <summary>
        /// This test will verify that the Closed() will be triggered on the extension that is closed. 
        /// </summary>
        [Test]
        public void OnViewExtensionClosedTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;

            // Open a graph which should bring up the Workspace References view extension window with one tab
            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            var loadedParams = new ViewLoadedParams(View, ViewModel);

            // Assert that the workspace references menu item is checked.
            Assert.IsTrue(WorkspaceReferencesExtension.workspaceReferencesMenuItem.IsChecked);

            // Closing the view extension side bar should trigger the Closed() on the workspace dependency view extension.
            // This will un-check the workspace references menu item.
            loadedParams.CloseExtensioninInSideBar(WorkspaceReferencesExtension);

            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);

            // Assert that the workspace references menu item is un-checked.
            Assert.IsFalse(WorkspaceReferencesExtension.workspaceReferencesMenuItem.IsChecked);
        }

        /// <summary>
        /// This test will make sure that the extension tab is closed upon closing the home workspace.
        /// </summary>
        [Test]
        public void CloseViewExtensionTabOnClosingWorkspace()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            // Open a graph which should bring up the Workspace References view extension window with one tab
            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            var homeSpace = Model.Workspaces.First(ws => ws is HomeWorkspaceModel) as HomeWorkspaceModel;
            homeSpace.Clear();
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
        }

        /// <summary>
        /// This test is created to guard a crash happened that while dep viewer is loaded,
        /// opening a dyf directly and closing it to switch to an empty homeworkspace causing a crash
        /// </summary>
        [Test]
        public void DependencyRegenCrashingDynamoTest()
        {
            this.View.WindowState = WindowState.Maximized;
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            // Open a random dyf, as a result two Workspace Model will exist under DynamoModel
            Open(@"pkgs\EvenOdd2\dyf\EvenOdd.dyf");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.Loaded(loadedParams);

            var homeWorkspaceModel = ViewModel.Model.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();

            // This is equivalent to uninstall the package
            var package = viewExtension.pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == "Dynamo Samples").FirstOrDefault();
            package.LoadState.SetScheduledForDeletion();

            // Closing the dyf will trigger DependencyRegen of HomeWorkspaceModel.
            // The HomeWorkspaceModel does not contain any dependency info since it's empty
            // but DependencyRegen() call on it should not crash
            Assert.DoesNotThrow(()=> viewExtension.DependencyRegen(homeWorkspaceModel));
        }

        [Test]
        public void KeepInstalledVersionOfPackageTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.Loaded(loadedParams);

            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;
            var info = CurrentWorkspace.NodeLibraryDependencies.Find(x => x.Name == "Dynamo Samples");

            // Once choosing to keep the installed loaded version, info.Version should reflect the lower version
            viewExtension.DependencyView.UpdateWorkspaceToUseInstalledPackage(info as PackageDependencyInfo);

            // Check results after keeping installed version of package
            var packageDependencies = CurrentWorkspace.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual("2.0.0", package.Version.ToString());
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.AreEqual(package.State, PackageDependencyState.Loaded);

            // Check serialization
            var ToJson = CurrentWorkspace.ToJson(ViewModel.Model.EngineController);
            var jObject = JsonConvert.DeserializeObject(ToJson) as JObject;
            var deserializedPackageDependencies = jObject[WorkspaceReadConverter.NodeLibraryDependenciesPropString];
            Assert.AreEqual(1, deserializedPackageDependencies.Count());
            var name = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NamePropString].Value<string>();
            Assert.AreEqual(package.Name, name);
            var version = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.VersionPropString].Value<string>();
            Assert.AreEqual(package.Version.ToString(), version);
            var nodes = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NodesPropString].Values<string>();
            Assert.AreEqual(package.Nodes.Select(n => n.ToString("N")), nodes);
        }

        [Test]
        public void WillNotAddDupWorkspaceDependencyViewExtensionLoadTest()
        {
            RaiseLoadedEvent(this.View);
            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var extensionManager = View.viewExtensionManager;

            var initialNum = ViewModel.SideBarTabItems.Count;

            // Adding the workspace references extension will 
            // not add a dup tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum, ViewModel.SideBarTabItems.Count);
        }

        [Test]
        public void TestPropertiesWithCodeInIt()
        {
            Assert.AreEqual(Resources.ExtensionName, viewExtension.Name);

            Assert.AreEqual("A6706BF5-11C2-458F-B7C8-B745A77EF7FD", viewExtension.UniqueId);
        }

        [Test]
        public void VerifyDynamoLoadingOnOpeningWorkspaceWithMissingCustomNodes()
        {
            List<string> dependenciesList = new List<string>() { "MeshToolkit", "Clockwork for Dynamo 1.x", "Clockwork for Dynamo 2.x", "Dynamo Samples" };
            DynamoModel.IsTestMode = false;

            var examplePath = Path.Combine(@"core\packageDependencyTests\PackageDependencyStates.dyn");
            Open(examplePath);
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);

            foreach (PackageDependencyRow packageDependencyRow in WorkspaceReferencesExtension.dataRows)
            {
                var dependencyInfo = packageDependencyRow.DependencyInfo;
                Assert.Contains(dependencyInfo.Name, dependenciesList);
            }
        }

        [Test]
        public void VerifyLocalDefinitions()
        {
            List<string> dependenciesList = new List<string>() { "RootNode.dyf"};
            DynamoModel.IsTestMode = false;

            // Load the custom node and the zero touch assembly.
            GetLibrariesToPreload(new List<string>());
            var examplePath = Path.Combine(@"core\custom_node_dep_test\RootNode.dyf");
            Open(examplePath);
            ViewModel.Model.ClearCurrentWorkspace();

            // Open test file to verify the LocalDefinitions list. 
            examplePath = Path.Combine(@"core\LocalDefinitionsTest.dyn");
            Open(examplePath);
           
            Assert.AreEqual(1, WorkspaceReferencesExtension.localDefinitionDataRows.Count());
            DependencyRow localDefinitionRow = WorkspaceReferencesExtension.localDefinitionDataRows.FirstOrDefault();
            var dependencyInfo = localDefinitionRow.DependencyInfo;
            Assert.Contains(dependencyInfo.Name, dependenciesList);
        }

        [Test]
        public void VerifyExternalFileReferences()
        {
            List<string> dependenciesList = new List<string>() { "DynamoTest.xlsx", "Dynamo.png" };

            // Open test file to verify the external file references. 
            var examplePath = Path.Combine(@"core\ExternalReferencesTest.dyn");
            Open(examplePath);

            WorkspaceReferencesExtension.DependencyRegen(Model.CurrentWorkspace, true);

            Assert.AreEqual(2, WorkspaceReferencesExtension.externalFilesDataRows.Count());
            foreach (DependencyRow localDefinitionRow in WorkspaceReferencesExtension.externalFilesDataRows)
            {
                var dependencyInfo = localDefinitionRow.DependencyInfo;
                Assert.Contains(dependencyInfo.Name, dependenciesList);
            }
        }

        [Test]
        public void GetExternalFilesShouldBailIfGraphExecuting()
        {
            // Open test file to verify the external file references are not computed when RunEnabled is false. 
            var examplePath = Path.Combine(@"core\ExternalReferencesTest.dyn");
            Open(examplePath);
            (Model.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunEnabled = false;
            WorkspaceReferencesExtension.DependencyRegen(Model.CurrentWorkspace, true);
            var results = Model.CurrentWorkspace.ExternalFiles;
            Assert.AreEqual(0, results.Count());
            (Model.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunEnabled = true;
            WorkspaceReferencesExtension.DependencyRegen(Model.CurrentWorkspace, true);
            results = Model.CurrentWorkspace.ExternalFiles;
            Assert.AreEqual(2, results.Count());
        }
    }
}

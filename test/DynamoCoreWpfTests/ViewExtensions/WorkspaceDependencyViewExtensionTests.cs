using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Dynamo.Wpf.Extensions;
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

        [Test]
        public void RestartBannerDefaultStateTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);
            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;
            viewExtension.DependencyView.DependencyRegen(CurrentWorkspace);
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
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);

            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;
            var info = CurrentWorkspace.NodeLibraryDependencies.Find(x => x.Name == "Dynamo Samples");

            // This is equivalent to uninstall the package
            var package = viewExtension.pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == "Dynamo Samples").FirstOrDefault();
            package.MarkedForUninstall = true;

            // Once choosing to install the specified version, info.State should reflect RequireRestart
            viewExtension.DependencyView.DependencyRegen(CurrentWorkspace);

            // Restart banner should display immediately
            Assert.AreEqual(Visibility.Visible, viewExtension.DependencyView.RestartBanner.Visibility);
            Assert.AreEqual(1, viewExtension.DependencyView.PackageDependencyTable.Items.Count);
            var newInfo = viewExtension.DependencyView.dataRows.FirstOrDefault().DependencyInfo;

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
            Assert.AreEqual(1, View.ExtensionTabItems.Count);

            Utility.DispatcherUtil.DoEvents();
            View.CloseExtensionTab(WpfUtilities.ChildrenOfType<Button>(View.ExtensionTabItems.FirstOrDefault()).FirstOrDefault(), null);
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
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
            Assert.AreEqual(1, View.ExtensionTabItems.Count);

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            loadedParams.CloseExtensioninInSideBar(this.viewExtension);
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
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
            Assert.AreEqual(1, View.ExtensionTabItems.Count);
            var homeSpace = Model.Workspaces.First(ws => ws is HomeWorkspaceModel) as HomeWorkspaceModel;
            homeSpace.Clear();
            Assert.AreEqual(0, View.ExtensionTabItems.Count);
        }

        /// <summary>
        /// This test is created to guard a crash happened that while dep viewer is loaded,
        /// opening a dyf directly and closing it to switch to an empty homeworkspace causing a crash
        /// </summary>
        [Test]
        public void DependencyRegenCrashingDynamoTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            // Open a random dyf, as a result two Workspace Model will exist under DynamoModel
            Open(@"pkgs\EvenOdd2\dyf\EvenOdd.dyf");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);

            var homeWorkspaceModel = ViewModel.Model.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();

            // This is equivalent to uninstall the package
            var package = viewExtension.pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == "Dynamo Samples").FirstOrDefault();
            package.MarkedForUninstall = true;

            // Closing the dyf will trigger DependencyRegen of HomeWorkspaceModel.
            // The HomeWorkspaceModel does not contain any dependency info since it's empty
            // but DependencyRegen() call on it should not crash
            Assert.DoesNotThrow(()=> viewExtension.DependencyView.DependencyRegen(homeWorkspaceModel));
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
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);

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

            var initialNum = View.ExtensionTabItems.Count;

            // Adding the workspace references extension will 
            // not add a dup tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum, View.ExtensionTabItems.Count);
        }

        public static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
        }
    }
}

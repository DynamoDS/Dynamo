using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.WorkspaceDependency;
using Dynamo.Wpf.Extensions;
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
        public void KeepInstalledVersionOfPackageTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);

            System.Version infoVersion = new System.Version("0.1.3");
            PackageDependencyInfo info = new PackageDependencyInfo("GetHighest", infoVersion);

            // Once choosing to keep the installed loaded version, info.Version should reflect the lower version
            viewExtension.DependencyView.UpdateWorkspaceToUseInstalledPackage(info);
            Assert.AreEqual("0.1.2", info.Version.ToString());
        }

        [Test]
        public void WorkspaceDependencyViewExtensionLoadTest()
        {
            RaiseLoadedEvent(this.View);

            var extensionManager = View.viewExtensionManager;

            var initialNum = View.TabItems.Count;

            // Adding the workspace dependency view extension will add a tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum + 1, View.TabItems.Count);
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

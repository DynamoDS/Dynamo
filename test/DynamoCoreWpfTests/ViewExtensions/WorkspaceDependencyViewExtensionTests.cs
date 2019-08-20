﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
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
        public void KeepInstalledVersionOfPackageTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            viewExtension.DependencyView = new WorkspaceDependencyView(viewExtension, loadedParams);

            var CurrentWorkspace = ViewModel.Model.CurrentWorkspace;
            var info = CurrentWorkspace.NodeLibraryDependencies.Find(x => x.Name == "Dynamo Samples");
            viewExtension.pmExtension = this.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
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

            var initialNum = View.TabItems.Count;

            // Adding the workspace dependency view extension will 
            // not add a dup tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum, View.TabItems.Count);
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

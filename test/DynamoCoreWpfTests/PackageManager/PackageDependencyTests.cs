using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager.UI;
using Dynamo.Tests;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Dynamo.Models;
using Dynamo.Interfaces;
using Dynamo.Scheduler;
using Dynamo.Configuration;

namespace Dynamo.PackageManager.Wpf.Tests
{
    internal class PackageDependencyTests : DynamoModelTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }

        [Test]
        public void CanDiscoverDependenciesForFunctionDefinitionOpenFromFile()
        {
            Open<HomeWorkspaceModel>(TestDirectory, @"core\custom_node_dep_test\", "custom_node_dep_test.dyn");

            var funcRootNode = this.CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Function>("333ed3ad-c786-4064-8203-e79ce7cb109f");

            var dirDeps = funcRootNode.Definition.DirectDependencies;
            Assert.AreEqual(2, dirDeps.Count() );

            var allDeps = funcRootNode.Definition.Dependencies;
            Assert.AreEqual(7, allDeps.Count());

            var packageRoot = new PackageItemRootViewModel(funcRootNode.Definition);
            packageRoot.BuildDependencies(new HashSet<object>());

            Assert.AreEqual(2, packageRoot.Items.Count);

            Assert.AreEqual(2, packageRoot.Items[0].Items.Count);
            Assert.AreEqual(3, packageRoot.Items[1].Items.Count);
        }

        /// <summary>
        /// This test is placed in this class because the DyanmoSample package has wpf dependency.
        /// </summary>
        [Test]
        public void NodeModelPackageDependencyIsCollected()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\OneDependentNode_NodeModel.dyn");

            // Assert package dependency is not already serialized to .dyn
            using (StreamReader file = new StreamReader(path))
            {
                var data = file.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(data);
                Assert.IsNull(json[WorkspaceReadConverter.NodeLibraryDependenciesPropString]);
            }

            // Assert package dependency is collected
            OpenModel(path);

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), package);
            Assert.AreEqual(1, package.Nodes.Count);

            Assert.IsTrue(package.IsLoaded);
            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
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
    }
}

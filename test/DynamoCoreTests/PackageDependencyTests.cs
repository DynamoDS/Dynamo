using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
using Dynamo.Search.SearchElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class PackageDependencyTests : DynamoModelTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(TestDirectory, "pkgs"); } }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private void LoadPackage(string packageDirectory)
        {
            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders.Add(packageDirectory);
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> {pkg});
        }

        private PackageDependencyInfo GetPackageInfo(string packageName)
        {
            var loader = GetPackageLoader();
            var package = loader.LocalPackages.Where(p => p.Name == packageName).FirstOrDefault();
            if (package != null)
            {
                return new PackageDependencyInfo(package.Name, new Version(package.VersionName));
            }
            return null;
        }

        private NodeModel GetNodeInstance(string creationName)
        {
            var searchElementList = CurrentDynamoModel.SearchModel.SearchEntries.OfType<NodeSearchElement>();
            foreach (var element in searchElementList)
            {
                if (element.CreationName == creationName)
                {
                    return (element as NodeSearchElement).CreateNode();
                }
            }
            return null;
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

        [Test]
        public void ZeroTouchPackageDependencyIsCollectedAndSerialized()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\OneDependentNode_ZeroTouch.dyn");

            // Assert package dependency is not already serialized to .dyn
            using (StreamReader file = new StreamReader(path))
            {
                var data = file.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(data);
                Assert.IsNull(json["PackageDependencies"]);
            }

            // Assert package dependency is collected
            OpenModel(path);
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), package);
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.IsTrue(package.IsLoaded);

            // Assert package dependency is serialized
            var ToJson = CurrentDynamoModel.CurrentWorkspace.ToJson(CurrentDynamoModel.EngineController);
            var JObject = (JObject)JsonConvert.DeserializeObject(ToJson);
            var deserializedPackageDependencies = JObject["PackageDependencies"];
            Assert.AreEqual(1, deserializedPackageDependencies.Count());
            var name = deserializedPackageDependencies.First()["Name"].Value<string>();
            Assert.AreEqual(package.Name, name);
            var version = deserializedPackageDependencies.First()["Version"].Value<string>();
            Assert.AreEqual(package.Version.ToString(), version);
            var nodes = deserializedPackageDependencies.First()["Nodes"].Values<string>();
            Assert.AreEqual(package.Nodes.Select(n => n.ToString("N")), nodes);
        }

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
                Assert.IsNull(json["PackageDependencies"]);
            }

            // Assert package dependency is collected
            OpenModel(path);
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), package);
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.IsTrue(package.IsLoaded);
        }

        [Test]
        public void CustomNodePackageDependencyIsCollected()
        {
            // Add "Round Down To Precision" custom node from the "Custom Rounding" package to a new workspace
            var guid = new Guid("6d1e3caa-780d-40fd-a045-766b3170235d");
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(guid);
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, true);

            // Assert package dependency is collected
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")), package);
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.IsTrue(package.IsLoaded);
        }

        [Test]
        public void PackageDependencyIsCollectedForNewWorkspace()
        {
            // Add one node from "Dynamo Samples" package
            var pi = GetPackageInfo("Dynamo Samples");
            var node = GetNodeInstance("Examples.NEWBasicExample.Create@double,double,double");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node, true);

            // Assert that "Dynamo Samples" has been added to the workspace's package dependencies
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(pi, package);
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.IsTrue(package.IsLoaded);
        }
        
        [Test]
        public void PackageDependenciesUpdatedAfterNodesAdded()
        {
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.PackageDependencies.Count, 0);

            // Add one node from "Dynamo Samples" package
            var package1 = GetPackageInfo("Dynamo Samples");
            var node1 = GetNodeInstance("Examples.NEWBasicExample.Create@double,double,double");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node1, true);
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);

            // Add second node from "Dynamo Samples" package
            var node2 = GetNodeInstance("Examples.PeriodicIncrement.Increment");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node2, true);
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;

            // Package dependencies count should still be 1 because both nodes are from the same package
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(package1, packageDependencies.First());
            Assert.AreEqual(2, packageDependencies.First().Nodes.Count);

            // Add third node from a different package ("Custom Rounding")
            var package2 = GetPackageInfo("Custom Rounding");
            var guid = new Guid("6d1e3caa-780d-40fd-a045-766b3170235d");
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(guid);
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, true);

            // There should now be 2 package dependencies and 3 total dependent nodes
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(2, packageDependencies.Count);
            foreach(var package in packageDependencies)
            {
                Assert.IsTrue(package.IsLoaded);
                if (package.Equals(package1))
                {
                    // Package 1 should have two nodes
                    Assert.AreEqual(2, package.Nodes.Count);
                }
                else
                {
                    // Package 2 should have one node
                    Assert.AreEqual(1, package.Nodes.Count);
                }
            }
        }

        [Test]
        public void PackageDependenciesUpdatedAfterNodeRemoved()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\TwoDependentNodes_OnePackage.dyn");
            OpenModel(path);

            // Get the two dependent nodes
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            var node1 = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList()[0];
            var node2 = CurrentDynamoModel.CurrentWorkspace.Nodes.ToList()[1];

            // Verify package dependencies
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(2, packageDependencies.First().Nodes.Count);
            
            // Remove one node and assert is is no longer listed as a dependent node
            CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(node1);
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(1, packageDependencies.First().Nodes.Count);
            Assert.True(!packageDependencies.First().Nodes.Contains(node1.GUID));
            Assert.IsTrue(packageDependencies.First().IsLoaded);

            // Remove te second node and assert package dependencies is now empty
            CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(node2);
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(0, packageDependencies.Count);

            // Add another node from the "Dynamo Samples" package
            // and assert that the two removed nodes do not return
            var node3 = GetNodeInstance("Examples.PeriodicIncrement.Increment");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node3, true);
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(1, packageDependencies.First().Nodes.Count);
            Assert.True(!packageDependencies.First().Nodes.Contains(node1.GUID));
            Assert.True(!packageDependencies.First().Nodes.Contains(node2.GUID));
            Assert.True(packageDependencies.First().Nodes.Contains(node3.GUID));
        }

        [Test]
        public void PackageDependenciesUpdatedAfterPackageAndNodeAdded()
        {
            // Assert ZTTestPackage is not already loaded
            var pi = GetPackageInfo("ZTTestPackage");
            Assert.IsNull(pi);

            // Load package
            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\ZTTestPackage");
            LoadPackage(packageDirectory);
            pi = GetPackageInfo("ZTTestPackage");

            // Add node from package
            var node = GetNodeInstance("ZTTestPackage.RRTestClass.RRTestClass");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node, true);

            // Assert new package dependency is collected
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.Contains(pi, packageDependencies);
            Assert.IsTrue(packageDependencies.First().IsLoaded);
        }

        [Test]
        public void PackageDependenciesClearedAfterWorkspaceCleared()
        {
            // Assert ZTTestPackage is not already loaded
            var pi = GetPackageInfo("ZTTestPackage");
            Assert.IsNull(pi);

            // Load package
            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\ZTTestPackage");
            LoadPackage(packageDirectory);
            pi = GetPackageInfo("ZTTestPackage");

            // Add node from package
            var node = GetNodeInstance("ZTTestPackage.RRTestClass.RRTestClass");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node, true);

            // Assert new package dependency is collected
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.Contains(pi, packageDependencies);

            // Clear current workspace
            CurrentDynamoModel.ClearCurrentWorkspace();

            // Assert package dependency list is cleared
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.PackageDependencies.Count == 0);
        }

        [Test]
        public void PackageDependenciesPreservedWhenPackagesNotLoaded()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\UnloadedPackage.dyn");
            OpenModel(path);

            // Assert ZTTestPackage is not loaded
            var pi = GetPackageInfo("ZTTestPackage");
            Assert.IsNull(pi);

            // Assert ZTTestPackage is still a package dependency
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.Contains(new PackageDependencyInfo("ZTTestPackage", new Version("0.0.1")), packageDependencies);
            Assert.IsTrue(packageDependencies.First().IsLoaded == false);
        }
    }
}

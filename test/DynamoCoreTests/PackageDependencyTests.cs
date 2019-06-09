using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Dynamo.Configuration;
using Dynamo.Extensions;
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

        private PackageLoader GetPackageLoader()
        {
            var extensions = CurrentDynamoModel.ExtensionManager.Extensions.OfType<PackageManagerExtension>();
            if (extensions.Count() > 0)
            {
                return extensions.First().PackageLoader;
            }

            return null;
        }

        internal void LoadPackage(string package)
        {
            var pkgDir = Path.Combine(PackagesDirectory, package);
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(pkgDir);
            loader.Load(pkg);
        }

        internal PackageDependencyInfo GetPackageInfo(string packageName)
        {
            var loader = GetPackageLoader();
            var package = loader.LocalPackages.Where(p => p.Name == packageName).FirstOrDefault();
            return new PackageDependencyInfo(package.Name, new Version(package.VersionName));
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
        }

        [Test]
        public void CustomNodePackageDependencyIsCollected()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\OneDependentNode_CustomNode.dyn");

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
            Assert.AreEqual(new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")), package);
            Assert.AreEqual(1, package.Nodes.Count);
        }

        [Test]
        public void PackageDependencyIsCollectedForNewWorkspace()
        {
            // Add "Round Down To Precision" custom node from the "Custom Rounding" package to a new workspace
            var guid = new Guid("6d1e3caa-780d-40fd-a045-766b3170235d");
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(guid);
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, true);

            // Assert that "Custom Rounding" has been added to the workspace's package dependencies
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.PackageDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")), package);
            Assert.AreEqual(1, package.Nodes.Count);
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
        }

        [Test]
        public void PackageDependenciesUpdatedAfterPackageAndNodeAdded()
        {
        }

        [Test]
        public void PackageDependenciesPreservedWhenPackagesNotLoaded()
        {
        }

        [Test]
        public void PackageDependenciesPreservedAfterPackageRemoved()
        {
        }
    }
}

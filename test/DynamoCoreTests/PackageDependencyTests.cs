using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Dynamo.Configuration;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
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
            Assert.AreEqual(packageDependencies.Count, 1);
            var package = packageDependencies.First();
            Assert.AreEqual(package, new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")));
            Assert.AreEqual(package.Nodes.Count, 1);

            // Assert package dependency is serialized
            var ToJson = CurrentDynamoModel.CurrentWorkspace.ToJson(CurrentDynamoModel.EngineController);
            var JObject = (JObject)JsonConvert.DeserializeObject(ToJson);
            var deserializedPackageDependencies = JObject["PackageDependencies"];
            Assert.AreEqual(deserializedPackageDependencies.Count(), 1);
            var name = deserializedPackageDependencies.First()["Name"].Value<string>();
            Assert.AreEqual(name, package.Name);
            var version = deserializedPackageDependencies.First()["Version"].Value<string>();
            Assert.AreEqual(version, package.Version.ToString());
            var nodes = deserializedPackageDependencies.First()["Nodes"].Values<string>();
            Assert.AreEqual(nodes, package.Nodes.Select(n => n.ToString("N")));
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
            Assert.AreEqual(packageDependencies.Count, 1);
            var package = packageDependencies.First();
            Assert.AreEqual(package, new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")));
            Assert.AreEqual(package.Nodes.Count, 1);
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
            Assert.AreEqual(packageDependencies.Count, 1);
            var package = packageDependencies.First();
            Assert.AreEqual(package, new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")));
            Assert.AreEqual(package.Nodes.Count, 1);
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
            Assert.AreEqual(packageDependencies.Count, 1);
            var package = packageDependencies.First();
            Assert.AreEqual(package, new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")));
            Assert.AreEqual(package.Nodes.Count, 1);
        }

        [Test]
        public void PackageDependenciesUpdatedAfterNodeAdded()
        {
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

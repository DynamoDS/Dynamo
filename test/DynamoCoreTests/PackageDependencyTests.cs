using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
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
 
            // Assert package dependency is serialized
            var ToJson = currentws.ToJson(CurrentDynamoModel.EngineController);
            var JObject = (JObject)JsonConvert.DeserializeObject(ToJson);
            var deserializedPackageDependencies = JObject[WorkspaceReadConverter.NodeLibraryDependenciesPropString];
            Assert.AreEqual(1, deserializedPackageDependencies.Count());
            var name = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NamePropString].Value<string>();
            Assert.AreEqual(package.Name, name);
            var version = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.VersionPropString].Value<string>();
            Assert.AreEqual(package.Version.ToString(), version);
            var nodes = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NodesPropString].Values<string>();
            Assert.AreEqual(package.Nodes.Select(n => n.ToString("N")), nodes);
        }

        [Test,Category("FailureNET6")]
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
            //TODO_NET6 following package cannot be loaded as depends on wpf.
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), package);
            Assert.AreEqual(1, package.Nodes.Count);

            Assert.IsTrue(package.IsLoaded);
            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
        }

        [Test]
        public void CustomNodePackageDependencyIsCollected()
        {
            // Add "Round Down To Precision" custom node from the "Custom Rounding" package to a new workspace
            var guid = new Guid("6d1e3caa-780d-40fd-a045-766b3170235d");
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(guid);
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, true);

            // Assert package dependency is collected
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("Custom Rounding", new Version("0.1.4")), package);
            Assert.AreEqual(1, package.Nodes.Count);

            Assert.IsTrue(package.IsLoaded);
            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
        }

        [Test]
        public void PackageDependencyIsCollectedForNewWorkspace()
        {
            // Add one node from "Dynamo Samples" package
            var pi = GetPackageInfo("Dynamo Samples");
            var node = GetNodeInstance("Examples.BasicExample.Create@double,double,double");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node, true);

            // Assert that "Dynamo Samples" has been added to the workspace's package dependencies
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(pi, package);
            Assert.AreEqual(1, package.Nodes.Count);
            Assert.IsTrue(package.IsLoaded);

            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
        }

        [Test]
        public void PackageDependenciesUpdatedAfterNodesAdded()
        {
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies.Count, 0);

            // Add one node from "Dynamo Samples" package
            var package1 = GetPackageInfo("Dynamo Samples");
            var node1 = GetNodeInstance("Examples.BasicExample.Create@double,double,double");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node1, true);
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);

            // Add second node from "Dynamo Samples" package
            var node2 = GetNodeInstance("Examples.PeriodicIncrement.Increment");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node2, true);
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;

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
            packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;
            Assert.AreEqual(2, packageDependencies.Count);
            foreach (var package in packageDependencies)
            {
                Assert.IsTrue(package.IsLoaded);

                if (package is PackageDependencyInfo)
                {
                    var packageDependencyState = ((PackageDependencyInfo)package).State;
                    Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
                }

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

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Verify package dependencies
            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(2, packageDependencies.First().Nodes.Count);

            // Remove one node and assert is is no longer listed as a dependent node
            CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(node1);
            packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(1, packageDependencies.First().Nodes.Count);
            Assert.True(!packageDependencies.First().Nodes.Contains(node1.GUID));

            var package = packageDependencies.First();
            Assert.IsTrue(package.IsLoaded);

            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }

            // Remove te second node and assert package dependencies is now empty
            CurrentDynamoModel.CurrentWorkspace.RemoveAndDisposeNode(node2);
            packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(0, packageDependencies.Count);

            // Add another node from the "Dynamo Samples" package
            // and assert that the two removed nodes do not return
            var node3 = GetNodeInstance("Examples.PeriodicIncrement.Increment");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node3, true);
            packageDependencies = currentws.NodeLibraryDependencies;
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
            var packageDependencies = CurrentDynamoModel.CurrentWorkspace.NodeLibraryDependencies;
            Assert.Contains(pi, packageDependencies);

            var package = packageDependencies.First();
            Assert.IsTrue(package.IsLoaded);

            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
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

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert new package dependency is collected
            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.Contains(pi, packageDependencies);

            // Clear current workspace
            CurrentDynamoModel.ClearCurrentWorkspace();

            // Assert package dependency list is cleared
            Assert.IsTrue(currentws.NodeLibraryDependencies.Count == 0);
        }

        [Test]
        public void PackageDependenciesPreservedWhenPackagesNotLoaded()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\UnloadedPackage.dyn");
            OpenModel(path);

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert ZTTestPackage is not loaded
            var pi = GetPackageInfo("ZTTestPackage");
            Assert.IsNull(pi);

            // Assert ZTTestPackage is still a package dependency
            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.Contains(new PackageDependencyInfo("ZTTestPackage", new Version("0.0.1")), packageDependencies);

            var package = packageDependencies.First();
            Assert.IsTrue(package.IsLoaded == false);

            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                // Assert that the package is not loaded. 
                Assert.AreNotEqual(PackageDependencyState.Loaded, packageDependencyState);
            }
        }

        [Test]
        public void DependencyWithTypeLoads()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\DependenciesWithType.dyn");
            OpenModel(path);

            // Assert ZTTestPackage is not loaded
            var pi = GetPackageInfo("ZTTestPackage");
            Assert.IsNull(pi);

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert ZTTestPackage is still a package dependency
            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.Contains(new PackageDependencyInfo("ZTTestPackage", new Version("0.0.1")), packageDependencies);

            // Assert that the package is not loaded. 
            var package = packageDependencies.First();
            Assert.IsTrue(package.IsLoaded == false);

            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreNotEqual(PackageDependencyState.Loaded, packageDependencyState);
            }

            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\ZTTestPackage");
            LoadPackage(packageDirectory);

            // Add node from package
            var node = GetNodeInstance("ZTTestPackage.RRTestClass.RRTestClass");
            CurrentDynamoModel.AddNodeToCurrentWorkspace(node, true);

            pi = GetPackageInfo("ZTTestPackage");
            // Assert new package dependency is collected
            Assert.Contains(pi, packageDependencies);
        }


        [Test]
        public void PackageDependenciesUpdatedWhenCustomNodeResolvedByNewPackage()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\CustomNodeContainedInMultiplePackages.dyn");

            // Assert serialized package dependency is Clockwork
            var clockworkInfo = new PackageDependencyInfo("Clockwork for Dynamo 2.x", new Version("2.1.2"));
            using (StreamReader file = new StreamReader(path))
            {
                var data = file.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(data);
                var pd = json[WorkspaceReadConverter.NodeLibraryDependenciesPropString];
                var deserializedPDs = JsonConvert.DeserializeObject<List<INodeLibraryDependencyInfo>>(pd.ToString(),
                    new NodeLibraryDependencyConverter(CurrentDynamoModel.Logger));
                Assert.AreEqual(1, deserializedPDs.Count);
                var package = deserializedPDs.First();
                Assert.AreEqual(clockworkInfo, package);
            }

            OpenModel(path);

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert clockwork is still the only dependency returned
            var packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(clockworkInfo, packageDependencies.First());

            // Void package dependency for node
            CurrentDynamoModel.CurrentWorkspace.VoidNodeDependency(Guid.Parse("d87512f7f69e433d86c729bac18bcfef"));

            // Assert local package dependency overrides deserialized package dependency
            var pi = GetPackageInfo("Custom Rounding");
            packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            Assert.AreEqual(pi, packageDependencies.First());
        }

        [Test,Category("FailureNET6")]
        public void PackageDependencyStatechangeTestAfterLoadingPackage()
        {
            // Before loading the clockworkpackage,veify the package dependency states. 
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\PackageDependencyStates.dyn");
            OpenModel(path);

            var currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert the total number of package dependencies.
            var packageDependenciesList = currentws.NodeLibraryDependencies;
            Assert.AreEqual(4, packageDependenciesList.Count);

            // Check for Missing package state
            PackageDependencyInfo firstPackage = (PackageDependencyInfo) packageDependenciesList[0];
            Assert.AreEqual(new PackageDependencyInfo("MeshToolkit", new Version("2.0.1")), firstPackage);
            Assert.AreEqual(PackageDependencyState.Missing, firstPackage.State);

            PackageDependencyInfo secondPackage = (PackageDependencyInfo) packageDependenciesList[1];
            Assert.AreEqual(new PackageDependencyInfo("Clockwork for Dynamo 2.x", new Version("2.1.2")), secondPackage);
            Assert.AreEqual(PackageDependencyState.Missing, secondPackage.State);

            PackageDependencyInfo thirdPackage = (PackageDependencyInfo) packageDependenciesList[2];
            Assert.AreEqual(new PackageDependencyInfo("Clockwork for Dynamo 1.x", new Version("1.33.0")), thirdPackage);
            Assert.AreEqual(PackageDependencyState.Missing, thirdPackage.State);

            //TODO_NET6 following package cannot be loaded as depends on wpf.

            // Check for Loaded package state
            PackageDependencyInfo lastPackage = (PackageDependencyInfo) packageDependenciesList.Last();
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), lastPackage);
            Assert.AreEqual(PackageDependencyState.Loaded, lastPackage.State);

            CurrentDynamoModel.ClearCurrentWorkspace();

            // Load the clockworkpackage and verify the change in the package dependency state.
            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\ClockworkPackage");
            LoadPackage(packageDirectory);

            // Reload JSON file graph
            path = Path.Combine(TestDirectory, @"core\packageDependencyTests\PackageDependencyStates.dyn");
            OpenModel(path);

            currentws = CurrentDynamoModel.CurrentWorkspace;
            currentws.ForceComputeWorkspaceReferences = true;

            // Assert the total number of package dependencies.
            packageDependenciesList = currentws.NodeLibraryDependencies;
            Assert.AreEqual(4, packageDependenciesList.Count);

            // Check for Missing package state
            firstPackage = (PackageDependencyInfo)packageDependenciesList[0];
            Assert.AreEqual(new PackageDependencyInfo("MeshToolkit", new Version("2.0.1")), firstPackage);
            Assert.AreEqual(PackageDependencyState.Missing, firstPackage.State);

            // Check for Warning package state, where the actually package is missing
            // but the nodes are resolved by a different package.
            secondPackage = (PackageDependencyInfo)packageDependenciesList[1];
            Assert.AreEqual(new PackageDependencyInfo("Clockwork for Dynamo 2.x", new Version("2.1.2")), secondPackage);
            Assert.AreEqual(PackageDependencyState.Warning, secondPackage.State);

            // Check for Incorrect package dependency state
            thirdPackage = (PackageDependencyInfo)packageDependenciesList[2];
            Assert.AreEqual(new PackageDependencyInfo("Clockwork for Dynamo 1.x", new Version("1.33.0")), thirdPackage);
            Assert.AreEqual(PackageDependencyState.IncorrectVersion, thirdPackage.State);

            // Check for Loaded package state
            lastPackage = (PackageDependencyInfo)packageDependenciesList.Last();
            Assert.AreEqual(new PackageDependencyInfo("Dynamo Samples", new Version("2.0.0")), lastPackage);
            Assert.AreEqual(PackageDependencyState.Loaded, lastPackage.State);
        }
    }
}

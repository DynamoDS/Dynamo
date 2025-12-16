using CoreNodeModels;
using Dynamo.Tests;
using Dynamo.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// Confirms that nested custom nodes containing CPython3 Python nodes
        /// are migrated to PythonNet3 and produce expected results.
        /// </summary>
        [Test]
        public void NestedCPythonCustomNodesAreAutoMigratedToPythonNet3()
        {
            Assert.IsTrue(
               View.viewExtensionManager.ViewExtensions.Any(e => e != null && e.Name == "Python Migration"),
               "Python Migration view extension is not loaded.");

            // Load custom node definitions into the manager first
            var pythonDir = Path.Combine(GetTestDirectory(ExecutingDirectory), "core", "python");
            var childPath = Path.Combine(pythonDir, "CNWithCPython_Child.dyf");
            var parentPath = Path.Combine(pythonDir, "CNWithCPython_Parent.dyf");
            var graphPath = Path.Combine(pythonDir, "WithNestedCPythonCustomNodes.dyn");
            var expectedValue = "20";

            Assert.IsTrue(File.Exists(childPath), "Missing test file: " + childPath);
            Assert.IsTrue(File.Exists(parentPath), "Missing test file: " + parentPath);
            Assert.IsTrue(File.Exists(graphPath), "Missing test file: " + graphPath);

            // Assert that both custom nodes are containing CPython3 python nodes before they are loaded
            AssertDyfContainsPythonNodesWithEngine(childPath, "CPython3");
            AssertDyfContainsPythonNodesWithEngine(parentPath, "CPython3");

            Assert.IsTrue(ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(childPath, true, out _));
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(parentPath, true, out _));

            // Open graph and run
            Open(graphPath);
            Run();

            // Assert watch value is expected
            var watch = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("8a7664fa-6764-42a1-b3ed-d9e7535819df");
            Assert.IsNotNull(watch);
            Assert.AreEqual(expectedValue, watch.CachedValue?.ToString());
        }

        /// <summary>
        /// Helper method to assert that a .dyf file contains at least one Python node of a given engine type.
        /// </summary>
        private static void AssertDyfContainsPythonNodesWithEngine(string dyfPath, string expectedEngine)
        {
            Assert.IsTrue(File.Exists(dyfPath), "Missing .dyf file: " + dyfPath);

            var root = JObject.Parse(File.ReadAllText(dyfPath));
            var nodes = root["Nodes"] as JArray;
            Assert.IsNotNull(nodes, "Invalid .dyf JSON: missing 'Nodes' array in " + dyfPath);

            bool match = nodes
                .OfType<JObject>()
                .Where(n => (n.Value<string>("ConcreteType") ?? string.Empty)
                .StartsWith("PythonNodeModels", StringComparison.Ordinal))
                .Select(n => n.Value<string>("Engine") ?? n.Value<string>("EngineName"))
                .Any(engine => string.Equals(engine, expectedEngine, StringComparison.Ordinal));

            Assert.IsTrue(match, $"Expected at least one Python node with engine '{expectedEngine}' in '{Path.GetFileName(dyfPath)}'.");
        }
    }
}


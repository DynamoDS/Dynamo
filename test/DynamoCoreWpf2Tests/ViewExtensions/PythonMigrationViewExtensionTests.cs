using Dynamo.PythonServices;
using Dynamo.Tests;
using Dynamo.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PythonNodeModels;
using System;
using System.IO;
using System.Linq;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        [Test]
        public void OpeningSecondGraphWithSameWorkspaceGuidStillMigratesCPythonToPythonNet3()
        {
            // Ensure that the Python auto migration notifications are disabled for this test
            Model.PreferenceSettings?.ShowPythonAutoMigrationNotifications = false;

            Assert.IsTrue(
               View.viewExtensionManager.ViewExtensions.Any(e => e != null && e.Name == "Python Migration"),
               "Python Migration view extension is not loaded.");

            var testDirectory = Path.GetFullPath(Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\python"));
            var firstGraphPath = Path.Combine(testDirectory, "WithCPython_DuplicateGuid_1.dyn");
            var secondGraphPath = Path.Combine(testDirectory, "WithCPython_DuplicateGuid_2.dyn");

            // Assert that both test graphs contain CPython nodes before opening
            AssertDyfContainsPythonNodesWithEngine(firstGraphPath, PythonEngineManager.CPython3EngineName);
            AssertDyfContainsPythonNodesWithEngine(secondGraphPath, PythonEngineManager.CPython3EngineName);

            Open(firstGraphPath);

            var firstGraphGuid = Model.CurrentWorkspace.Guid;
            var firstGraphFullName = Model.CurrentWorkspace.FileName;
            var pyNode = Model.CurrentWorkspace.NodeFromWorkspace<PythonNode>("14a86cf8-1219-4c0c-b636-cf9b3896984a");

            // Assert that the Python node has been migrated to PythonNet3
            Assert.IsNotNull(pyNode);
            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.PythonNet3EngineName);

            // Ensure the Unsaved Changes notification does not block opening the second graph 
            Model.CurrentWorkspace.HasUnsavedChanges = false;

            Open(secondGraphPath);

            var secondGraphGuid = Model.CurrentWorkspace.Guid;
            var secondGraphFullName = Model.CurrentWorkspace.FileName;
            pyNode = Model.CurrentWorkspace.NodeFromWorkspace<PythonNode>("5b70c276-b7db-4164-9931-74971c57d32c");

            // Assert that the Python node has been migrated to PythonNet3
            Assert.IsNotNull(pyNode);
            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.PythonNet3EngineName);

            // Assert that both graphs have the same Guid but have different files
            Assert.AreEqual(firstGraphGuid, secondGraphGuid, "Test graphs do not share the same Workspace Guid as expected");
            Assert.AreNotEqual(firstGraphFullName, secondGraphFullName, "Test graphs do not have different file paths as expected");
        }

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

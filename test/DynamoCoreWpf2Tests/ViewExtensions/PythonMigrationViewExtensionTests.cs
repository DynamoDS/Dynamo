using Dynamo.Graph.Nodes;
using Dynamo.PythonServices;
using Dynamo.Utilities;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PythonMigrationViewExtensionTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void OpeningSecondGraphWithSameWorkspaceGuidStillMigratesCPythonToPythonNet3()
        {
            // Ensure view extensions have been Loaded() (matches other ViewExtension tests).
            RaiseLoadedEvent(View);

            EnsurePythonMigrationViewExtensionLoaded();

            // NOTE: These two files already share the same top-level "Uuid" in test data.
            OpenAndWait(@"core\python\PythonGeometry.dyn");
            AssertCurrentWorkspaceHasNoCPython3Nodes();

            OpenAndWait(@"core\python\python_check_output.dyn");
            AssertCurrentWorkspaceHasNoCPython3Nodes();
        }

        private void OpenAndWait(string pathInTestsDir)
        {
            var fullPath = Path.GetFullPath(Path.Combine(GetTestDirectory(ExecutingDirectory), pathInTestsDir));

            ViewModel.OpenCommand.Execute(fullPath);

            DispatcherUtil.DoEventsLoop(() =>
            {
                var current = Model?.CurrentWorkspace?.FileName;
                if (string.IsNullOrWhiteSpace(current)) return false;

                try
                {
                    current = Path.GetFullPath(current);
                }
                catch
                {
                    // ignore path normalization failures, just compare raw
                }

                return string.Equals(current, fullPath, StringComparison.OrdinalIgnoreCase);
            });

            DispatcherUtil.DoEvents();
        }

        private void AssertCurrentWorkspaceHasNoCPython3Nodes()
        {
            var pythonNodes = Model.CurrentWorkspace.Nodes.OfType<PythonNodeBase>().ToList();
            Assert.That(pythonNodes.Count, Is.GreaterThan(0), "Expected at least one Python node in the test graph.");

            Assert.That(
                pythonNodes.Any(n => n.EngineName == PythonEngineManager.PythonNet3EngineName),
                Is.True,
                $"Expected at least one Python node to be migrated to {PythonEngineManager.PythonNet3EngineName}.");

            Assert.That(
                pythonNodes.Any(n => n.EngineName == PythonEngineManager.CPython3EngineName),
                Is.False,
                $"Expected no Python nodes to remain on {PythonEngineManager.CPython3EngineName} after migration.");
        }

        private void EnsurePythonMigrationViewExtensionLoaded()
        {
            var extensionManager = View.viewExtensionManager;

            // First try: it may already be loaded as a built-in view extension.
            var existing = extensionManager.ViewExtensions.FirstOrDefault(x =>
                string.Equals(x.Name, "Python Migration", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.GetType().FullName, "Dynamo.PythonMigration.PythonMigrationViewExtension", StringComparison.Ordinal));

            if (existing != null) return;

            // Fallback: load it from the test bin folder via a minimal manifest.
            var asmPath = Path.Combine(ExecutingDirectory, "PythonMigrationViewExtension.dll");
            if (!File.Exists(asmPath))
            {
                Assert.Inconclusive($"PythonMigrationViewExtension.dll was not found at '{asmPath}'. " +
                                    "The test runner output folder may not contain this view extension.");
                return;
            }

            var manifestPath = Path.Combine(TempFolder, "PythonMigration_ViewExtensionDefinition.xml");
            var manifestXml =
                $@"<ViewExtensionDefinition>
  <AssemblyPath>{asmPath}</AssemblyPath>
  <TypeName>Dynamo.PythonMigration.PythonMigrationViewExtension</TypeName>
</ViewExtensionDefinition>";

            File.WriteAllText(manifestPath, manifestXml);

            var loaded = extensionManager.ExtensionLoader.Load(manifestPath);
            Assert.IsNotNull(loaded, "Failed to load Python Migration view extension from manifest.");

            extensionManager.Add(loaded);

            // Let any Loaded()/subscriptions complete.
            DispatcherUtil.DoEvents();
        }
    }
}

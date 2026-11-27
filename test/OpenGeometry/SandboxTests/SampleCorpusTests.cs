using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Applications;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class SampleCorpusTests
    {
        private static readonly string[] CurrentlyPassingSamples =
        {
            @"Basics\Basics_Basic01.dyn",
            @"Basics\Basics_Basic02.dyn",
            @"Basics\Basics_Basic03.dyn",
            @"Core\Core_AttractorPoint.dyn",
            @"Core\Core_CodeBlocks.dyn",
            @"Core\Core_ListAtLevel.dyn",
            @"Core\Core_ListLacing.dyn",
            @"Core\Core_PassingFunctions.dyn",
            @"Core\Core_RangeSyntax.dyn",
            @"Geometry\Geometry_Curves.dyn",
            @"Geometry\Geometry_Points.dyn",
            @"Geometry\Geometry_Solids.dyn",
            @"Geometry\Geometry_Surfaces.dyn"
        };

        // Core_Strings.dyn intentionally passes a non-numeric string to String.ToNumber as part of the tutorial,
        // so it produces one expected DSCore.String.ToNumber warning while resolving all nodes with zero errors.
        private static readonly string[] AllNonExcelGeometrySamples =
        {
            @"Basics\Basics_Basic01.dyn",
            @"Basics\Basics_Basic02.dyn",
            @"Basics\Basics_Basic03.dyn",
            @"Core\Core_AttractorPoint.dyn",
            @"Core\Core_CodeBlocks.dyn",
            @"Core\Core_ListAtLevel.dyn",
            @"Core\Core_ListLacing.dyn",
            @"Core\Core_PassingFunctions.dyn",
            @"Core\Core_RangeSyntax.dyn",
            @"Core\Core_Strings.dyn",
            @"Geometry\Geometry_Curves.dyn",
            @"Geometry\Geometry_Points.dyn",
            @"Geometry\Geometry_Solids.dyn",
            @"Geometry\Geometry_Surfaces.dyn"
        };

        private string tempRoot;
        private DynamoModel model;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "OpenGeometryCorpusTests", Guid.NewGuid().ToString("N"));
            var userData = Path.Combine(tempRoot, "UserData");
            var commonData = Path.Combine(tempRoot, "CommonData");
            Directory.CreateDirectory(userData);
            Directory.CreateDirectory(commonData);

            var args = new StartupUtils.CommandLineArguments
            {
                NoNetworkMode = true,
                DisableAnalytics = true,
                UserDataFolder = userData,
                CommonDataFolder = commonData,
                Locale = "en-US"
            };
            model = StartupUtils.MakeCLIModel(args);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            model?.ShutDown(false);
            model = null;
            if (Directory.Exists(tempRoot))
            {
                try { Directory.Delete(tempRoot, true); }
                catch (IOException) { }
            }
        }

        [TestCaseSource(nameof(CurrentlyPassingSamples))]
        public void WhenCuratedDynamoSampleRunsThenNoDummyNodesOrErrorsOccur(string relativeSamplePath)
        {
            var fullPath = Path.Combine(FindSamplesRoot(), relativeSamplePath);
            Assert.That(File.Exists(fullPath), Is.True, $"Missing Dynamo sample: {fullPath}");

            var result = RunSample(fullPath, relativeSamplePath);
            Assert.Multiple(() =>
            {
                Assert.That(result.DummyNodes, Is.Empty, $"Unresolved DummyNodes in {relativeSamplePath}");
                Assert.That(result.ErrorNodes, Is.Empty, $"Error nodes in {relativeSamplePath}");
                Assert.That(result.WarningNodes, Is.Empty, $"Warning nodes in {relativeSamplePath}");
            });
        }

        [Test]
        public void WhenSampleCorpusIsAuditedThenPassRateAndMissingSignaturesAreReported()
        {
            var samplesRoot = FindSamplesRoot();
            var results = new List<SampleRunResult>();
            var missingSignatures = new SortedSet<string>(StringComparer.Ordinal);

            foreach (var rel in AllNonExcelGeometrySamples)
            {
                var fullPath = Path.Combine(samplesRoot, rel);
                Assert.That(File.Exists(fullPath), Is.True, $"Missing sample: {fullPath}");
                var r = RunSample(fullPath, rel);
                results.Add(r);
                foreach (var sig in r.UnresolvedSignatures)
                    missingSignatures.Add(sig);
            }

            var passed = results.Count(r => r.Passed);
            TestContext.Progress.WriteLine($"OpenGeometry Sample Corpus: {passed}/{results.Count} clean passes");
            foreach (var r in results)
            {
                var status = r.Passed ? "PASS" : "GAP ";
                TestContext.Progress.WriteLine(
                    $"  [{status}] {r.RelativePath} (nodes={r.NodeCount}, dummy={r.DummyNodes.Length}, warn={r.WarningNodes.Length}, err={r.ErrorNodes.Length})");
                foreach (var d in r.DummyNodes)
                    TestContext.Progress.WriteLine($"         dummy: {d}");
                foreach (var w in r.WarningNodes)
                    TestContext.Progress.WriteLine($"         warn:  {w}");
                foreach (var e in r.ErrorNodes)
                    TestContext.Progress.WriteLine($"         err:   {e}");
            }

            if (missingSignatures.Count > 0)
            {
                TestContext.Progress.WriteLine("Missing/unresolved signatures:");
                foreach (var sig in missingSignatures)
                    TestContext.Progress.WriteLine($"  - {sig}");
            }

            Assert.That(passed, Is.GreaterThanOrEqualTo(CurrentlyPassingSamples.Length));
        }

        private SampleRunResult RunSample(string fullPath, string relativePath)
        {
            model.ClearCurrentWorkspace();
            model.OpenFileFromPath(fullPath, forceManualExecutionMode: true);
            var workspace = model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.That(workspace, Is.Not.Null, $"Workspace failed to open for {relativePath}");

            workspace.Run();

            var dummyNodes = workspace.Nodes.OfType<DummyNode>()
                .Select(d => $"{d.Name} ({d.LegacyFullName})")
                .ToArray();

            var unresolvedSigs = new List<string>();
            foreach (var zt in workspace.Nodes.OfType<DSFunctionBase>())
            {
                if (zt.State == ElementState.Error || zt.State == ElementState.Warning)
                {
                    if (!string.IsNullOrEmpty(zt.Controller?.Definition?.MangledName))
                        unresolvedSigs.Add(zt.Controller.Definition.MangledName);
                }
            }
            foreach (var d in workspace.Nodes.OfType<DummyNode>())
            {
                if (!string.IsNullOrEmpty(d.LegacyFullName))
                    unresolvedSigs.Add(d.LegacyFullName);
            }

            var errorNodes = workspace.Nodes
                .Where(n => n.State == ElementState.Error)
                .Select(n => $"{n.Name}: {string.Join("; ", n.NodeInfos.Select(i => i.Message))}")
                .ToArray();

            var warningNodes = workspace.Nodes
                .Where(n => n.State == ElementState.Warning)
                .Select(n => $"{n.Name}: {string.Join("; ", n.NodeInfos.Select(i => i.Message))}")
                .ToArray();

            return new SampleRunResult(
                relativePath,
                workspace.Nodes.Count(),
                dummyNodes,
                warningNodes,
                errorNodes,
                unresolvedSigs.ToArray());
        }

        private static string FindSamplesRoot()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "doc", "distrib", "Samples", "en-US");
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate doc/distrib/Samples/en-US.");
        }

        private sealed record SampleRunResult(
            string RelativePath,
            int NodeCount,
            string[] DummyNodes,
            string[] WarningNodes,
            string[] ErrorNodes,
            string[] UnresolvedSignatures)
        {
            public bool Passed => DummyNodes.Length == 0 && WarningNodes.Length == 0 && ErrorNodes.Length == 0;
        }
    }
}

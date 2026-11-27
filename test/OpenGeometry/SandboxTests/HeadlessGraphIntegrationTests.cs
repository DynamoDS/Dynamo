using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Applications;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Visualization;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class HeadlessGraphIntegrationTests
    {
        private const double CuboidVolume = 1_000.0;
        private const double Tolerance = 1e-6;
        private static readonly double ExpectedCutVolume = CuboidVolume - 40.0 * Math.PI;

        private static readonly Guid CuboidNodeId = Guid.Parse("22222222-2222-4222-8222-222222222201");
        private static readonly Guid CylinderNodeId = Guid.Parse("88888888-8888-4888-8888-888888888801");
        private static readonly Guid DifferenceNodeId = Guid.Parse("99999999-9999-4999-8999-999999999901");
        private static readonly Guid CutVolumeNodeId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa01");
        private static readonly Guid OriginalVolumeNodeId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb01");

        private string tempRoot;
        private DynamoModel model;

        [SetUp]
        public void SetUp()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "OpenGeometryDynamoTests", Guid.NewGuid().ToString("N"));
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

        [TearDown]
        public void TearDown()
        {
            model?.ShutDown(false);
            model = null;
            if (Directory.Exists(tempRoot))
            {
                try { Directory.Delete(tempRoot, true); }
                catch (IOException) { }
            }
        }

        [Test]
        public void WhenSavedDynOpensAndRunsThenVolumesRenderOpenBoreAndSaveRoundTripSurvives()
        {
            var fixturePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "CenteredCuboidCylinderCut.dyn");
            Assert.That(File.Exists(fixturePath), Is.True, $"Missing fixture at {fixturePath}");

            model.OpenFileFromPath(fixturePath, forceManualExecutionMode: false);
            var workspace = model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.That(workspace, Is.Not.Null);

            AssertWorkspaceResolvedAndEvaluated(workspace);
            AssertCutNodeRendersWithOpenBore(workspace);

            var savedPath = Path.Combine(tempRoot, "RoundTrip.dyn");
            workspace.Save(savedPath, isBackup: false, engine: model.EngineController);
            Assert.That(File.Exists(savedPath), Is.True);

            var originalNodeIds = workspace.Nodes.Select(n => n.GUID).OrderBy(id => id).ToArray();
            var originalEdges = workspace.Connectors
                .Select(c => (c.Start.Owner.GUID, c.Start.Index, c.End.Owner.GUID, c.End.Index))
                .OrderBy(edge => edge)
                .ToArray();

            model.ClearCurrentWorkspace();
            model.OpenFileFromPath(savedPath, forceManualExecutionMode: false);

            var reopened = model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.That(reopened, Is.Not.Null);

            var reopenedNodeIds = reopened.Nodes.Select(n => n.GUID).OrderBy(id => id).ToArray();
            var reopenedEdges = reopened.Connectors
                .Select(c => (c.Start.Owner.GUID, c.Start.Index, c.End.Owner.GUID, c.End.Index))
                .OrderBy(edge => edge)
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(reopenedNodeIds, Is.EqualTo(originalNodeIds), "All nodes should survive save and reopen.");
                Assert.That(reopenedEdges, Is.EqualTo(originalEdges), "All connector wiring should survive save and reopen.");
            });

            AssertWorkspaceResolvedAndEvaluated(reopened);
            AssertCutNodeRendersWithOpenBore(reopened);
        }

        private void AssertWorkspaceResolvedAndEvaluated(HomeWorkspaceModel workspace)
        {
            workspace.Run();

            var dummyNodes = workspace.Nodes.OfType<DummyNode>().ToArray();
            var errorNodes = workspace.Nodes.Where(n => n.State == ElementState.Error || n.State == ElementState.Warning)
                .Select(n => $"{n.Name} ({n.GUID}): {n.State} - {string.Join("; ", n.NodeInfos.Select(i => i.Message))}")
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(dummyNodes, Is.Empty, "All Zero-Touch geometry nodes should resolve without DummyNodes.");
                Assert.That(errorNodes, Is.Empty, "No nodes should be in warning or error state.");
                Assert.That(workspace.Nodes.Count(), Is.EqualTo(11));
                Assert.That(workspace.Connectors.Count(), Is.EqualTo(12));
                Assert.That(workspace.Nodes.Any(n => n.GUID == CuboidNodeId), Is.True);
                Assert.That(workspace.Nodes.Any(n => n.GUID == CylinderNodeId), Is.True);
                Assert.That(workspace.Nodes.Any(n => n.GUID == DifferenceNodeId), Is.True);
            });

            var cutVolumeNode = workspace.Nodes.Single(n => n.GUID == CutVolumeNodeId);
            var boxVolumeNode = workspace.Nodes.Single(n => n.GUID == OriginalVolumeNodeId);
            var cutValue = cutVolumeNode.GetValue(0, model.EngineController)?.Data;
            var boxValue = boxVolumeNode.GetValue(0, model.EngineController)?.Data;

            Assert.Multiple(() =>
            {
                Assert.That(cutValue, Is.TypeOf<double>());
                Assert.That(boxValue, Is.TypeOf<double>());
                Assert.That((double)cutValue, Is.EqualTo(ExpectedCutVolume).Within(ExpectedCutVolume * Tolerance));
                Assert.That((double)boxValue, Is.EqualTo(CuboidVolume).Within(CuboidVolume * Tolerance));
            });
        }

        private void AssertCutNodeRendersWithOpenBore(HomeWorkspaceModel workspace)
        {
            var diffNode = workspace.Nodes.Single(n => n.GUID == DifferenceNodeId);
            var captured = new List<IRenderPackage>();

            void Handler(NodeModel _, RenderPackageCache packages)
            {
                captured.AddRange(packages.Packages);
            }

            diffNode.RenderPackagesUpdated += Handler;
            try
            {
                var factory = new DefaultRenderPackageFactory();
                var scheduled = diffNode.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory, forceUpdate: true);
                Assert.That(scheduled, Is.True, "Visual update task should be scheduled for Solid.Difference.");
            }
            finally
            {
                diffNode.RenderPackagesUpdated -= Handler;
            }

            Assert.That(captured, Is.Not.Empty, "Solid.Difference should emit at least one render package.");
            var vertices = captured.SelectMany(p => p.MeshVertices).ToArray();
            var normals = captured.SelectMany(p => p.MeshNormals).ToArray();
            var triangles = vertices.Chunk(9).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(vertices.Length, Is.GreaterThan(0));
                Assert.That(vertices.Length % 9, Is.Zero);
                Assert.That(normals.Length, Is.EqualTo(vertices.Length));
                Assert.That(triangles.Any(CoversBoreCenter), Is.False, "Top/bottom faces must not cover the through-hole center.");
                Assert.That(
                    triangles.Any(t => t.Chunk(3).All(p => Math.Abs(p[0] * p[0] + p[1] * p[1] - 4.0) < Tolerance)),
                    Is.True,
                    "The cylindrical bore wall should be present in the rendered mesh.");
            });
        }

        private static bool CoversBoreCenter(double[] t)
        {
            if (Math.Abs(t[2] - t[5]) > Tolerance || Math.Abs(t[2] - t[8]) > Tolerance)
                return false;

            var a = t[0] * t[4] - t[1] * t[3];
            var b = t[3] * t[7] - t[4] * t[6];
            var c = t[6] * t[1] - t[7] * t[0];
            return (a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Applications;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class JsonParsingCorpusTests
    {
        public sealed record JsonParsingCase(
            string FileName,
            string PrimaryTrueNodeId,
            bool ExpectOrientedBoundingBoxWarning = false,
            string SecondaryNodeId = null,
            bool? ExpectedSecondaryValue = null)
        {
            public override string ToString() => FileName;
        }

        private static IEnumerable<JsonParsingCase> JsonParsingGraphs()
        {
            yield return new("Abstract_BoundingBox_JSONParsing.dyn", "abb39e07-db08-45cf-9438-478defffbf68",
                ExpectOrientedBoundingBoxWarning: true,
                SecondaryNodeId: "9d611e10-bea8-4fbc-9364-8516e9f677f7",
                ExpectedSecondaryValue: true);
            yield return new("Abstract_CoordinateSystem_JSONParsing.dyn", "07366ada-f095-4529-b1ed-39b240192c96");
            yield return new("Abstract_Plane_JSONParsing.dyn", "9754cbd6-6d48-4241-9a68-99f372a80aee");
            yield return new("Abstract_Vector_JSONParsing.dyn", "71efc8c5-c0c7-4189-9017-07c30e6d5903");
            yield return new("Curve_Arc_JSONParsing.dyn", "71efc8c5-c0c7-4189-9017-07c30e6d5903",
                SecondaryNodeId: "82304dd5-0259-48f8-a564-4a84a32d58d4",
                ExpectedSecondaryValue: false);
            yield return new("Curve_Circle_JSONParsing.dyn", "54d56712-f1fa-4194-8a52-62aaf4eba5ba");
            yield return new("Curve_Ellipse_JSONParsing.dyn", "a29aa179-c7ae-4069-a6d9-c6d2055ab845");
            yield return new("Curve_EllipseArc_JSONParsing.dyn", "a29aa179-c7ae-4069-a6d9-c6d2055ab845",
                SecondaryNodeId: "a73925f5-7d2c-44d7-994a-2c4d77bf8581",
                ExpectedSecondaryValue: false);
            yield return new("Curve_Helix_JSONParsing.dyn", "b6a4919b-3dd9-4eb7-9a7f-0435d941d235",
                SecondaryNodeId: "1bbd147b-429c-43ab-8fe4-6a00d691a024",
                ExpectedSecondaryValue: false);
            yield return new("Curve_Line_JSONParsing.dyn", "71efc8c5-c0c7-4189-9017-07c30e6d5903");
            yield return new("Curve_NurbsCurve_JSONParsing.dyn", "423356e2-c8f8-4e00-aa6c-50e9bdb72c98");
            yield return new("Curve_PolyCurve_JSONParsing.dyn", "423356e2-c8f8-4e00-aa6c-50e9bdb72c98");
            yield return new("Curve_Polygon_JSONParsing.dyn", "015f80f9-1737-4031-b345-b46b5a8d54ca");
            yield return new("Curve_Rectangle_JSONParsing.dyn", "9754cbd6-6d48-4241-9a68-99f372a80aee");
            yield return new("Point_JSONParsing.dyn", "71efc8c5-c0c7-4189-9017-07c30e6d5903");
            yield return new("Solid_Cylinder_JSONParsing.dyn", "07366ada-f095-4529-b1ed-39b240192c96");
        }

        private string tempRoot;
        private DynamoModel model;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "OpenGeometryJsonCorpusTests", Guid.NewGuid().ToString("N"));
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

        [TestCaseSource(nameof(JsonParsingGraphs))]
        public void WhenDynamoJsonParsingGraphRunsThenRoundTripChecksPass(JsonParsingCase testCase)
        {
            var fullPath = Path.Combine(FindJsonTestRoot(), testCase.FileName);
            Assert.That(File.Exists(fullPath), Is.True, $"Missing Dynamo JSON test graph: {fullPath}");

            model.ClearCurrentWorkspace();
            model.OpenFileFromPath(fullPath, forceManualExecutionMode: true);
            var workspace = model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.That(workspace, Is.Not.Null, $"Workspace failed to open for {testCase.FileName}");

            workspace.Run();

            var dummyNodes = workspace.Nodes.OfType<DummyNode>()
                .Select(d => $"{d.Name} ({d.LegacyFullName})")
                .ToArray();
            var errorNodes = workspace.Nodes
                .Where(n => n.State == ElementState.Error)
                .Select(n => $"{n.Name} ({n.GUID}): {string.Join("; ", n.NodeInfos.Select(i => i.Message))}")
                .ToArray();
            var warningNodes = workspace.Nodes
                .Where(n => n.State == ElementState.Warning)
                .Select(n => $"{n.Name} ({n.GUID}): {string.Join("; ", n.NodeInfos.Select(i => i.Message))}")
                .ToArray();

            var primaryId = Guid.Parse(testCase.PrimaryTrueNodeId);
            var primaryNode = workspace.Nodes.Single(n => n.GUID == primaryId);
            var primaryValue = primaryNode.GetValue(0, model.EngineController)?.Data;

            if (!Equals(primaryValue, true) || errorNodes.Length > 0 || dummyNodes.Length > 0)
            {
                TestContext.Progress.WriteLine($"Diagnostics for {testCase.FileName}: primary={primaryValue}");
                foreach (var w in warningNodes)
                    TestContext.Progress.WriteLine($"  warn: {w}");
                foreach (var e in errorNodes)
                    TestContext.Progress.WriteLine($"  err:  {e}");
            }

            Assert.Multiple(() =>
            {
                Assert.That(dummyNodes, Is.Empty, $"Unresolved DummyNodes in {testCase.FileName}");
                Assert.That(errorNodes, Is.Empty, $"Error nodes in {testCase.FileName}");
                if (!testCase.ExpectOrientedBoundingBoxWarning)
                    Assert.That(warningNodes, Is.Empty, $"Unexpected warning nodes in {testCase.FileName}");
                Assert.That(primaryValue, Is.EqualTo(true), $"Primary round-trip assertion node {primaryId} failed in {testCase.FileName}");

                if (!string.IsNullOrEmpty(testCase.SecondaryNodeId) && testCase.ExpectedSecondaryValue.HasValue)
                {
                    var secId = Guid.Parse(testCase.SecondaryNodeId);
                    var secNode = workspace.Nodes.Single(n => n.GUID == secId);
                    var secValue = secNode.GetValue(0, model.EngineController)?.Data;
                    Assert.That(secValue, Is.EqualTo(testCase.ExpectedSecondaryValue.Value),
                        $"Secondary assertion node {secId} mismatch in {testCase.FileName}");
                }
            });
        }

        private static string FindJsonTestRoot()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "test", "core", "json");
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate test/core/json.");
        }
    }
}

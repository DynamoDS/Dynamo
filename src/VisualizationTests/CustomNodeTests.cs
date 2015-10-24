using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class CustomNodeHomeWorkspaceTests : VisualizationTest
    {
        [SetUp]
        public void Setup()
        {
            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(
                        GetTestDirectory(ExecutingDirectory),
                        @"core\visualization\custom-nodes\FiveCubes.dyf"),
                    true,
                    out info));

            // Disable edge rendering to ensure that curve counts are correct.
            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;

            OpenVisualizationTest(@"custom-nodes\custom-node-test.dyn");
        }

        [Test]
        public void InHomeWorkspace_HasGeometry()
        {
            Assert.AreEqual(3, BackgroundPreviewGeometry.Meshes().Count());
            Assert.AreEqual(3, BackgroundPreviewGeometry.Curves().Count());
            Assert.AreEqual(3, BackgroundPreviewGeometry.Points().Count());
        }

        [Test]
        public void CreateInstance_InsideInstance_HasGeometry()
        {
            // Regression test for defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5165
            // To verify when some geometry nodes are converted to custom node,
            // their render packages shouldn't be carried over to custom work
            // space.
            var model = ViewModel.Model;

            OpenVisualizationTest("visualize_line_incustom.dyn");

            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalCurves());

            // Convert a DSFunction node Line.ByPointDirectionLength to custom node.
            var workspace = model.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            var customWorkspace = model.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__VisualizationTest__",
                    Success = true
                }) as CustomNodeWorkspaceModel;
            ViewModel.HomeSpace.Run();

            // Switch to custom workspace
            model.OpenCustomNodeWorkspace(customWorkspace.CustomNodeId);
            var customSpace = ViewModel.Model.CurrentWorkspace;

            // Select that node
            DynamoSelection.Instance.Selection.Add(node);

            // No preview in the background
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalCurves());
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalMeshes());
        }
    }

    [TestFixture]
    public class CustomNodeCustomWorkspaceTests : VisualizationTest
    {
        private CustomNodeWorkspaceModel customNodeWorkspace;

        [SetUp]
        public void Setup()
        {
            var customNodePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\custom-nodes\FiveCubes.dyf");

            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    customNodePath,
                    true,
                    out info));

            // Disable edge rendering to ensure that curve counts are correct.
            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;

            OpenVisualizationTest(@"custom-nodes\custom-node-test.dyn");

            Model.OpenCustomNodeWorkspace(info.FunctionId);
            var cws = Model.Workspaces.FirstOrDefault(ws => ws is CustomNodeWorkspaceModel);
            Assert.NotNull(cws);
            customNodeWorkspace = (CustomNodeWorkspaceModel)cws;
        }

        [Test]
        public void InsideInstance_AllGeometrySolid()
        {
            Model.OpenCustomNodeWorkspace(customNodeWorkspace.CustomNodeId);

            Assert.AreEqual(2, BackgroundPreviewGeometry.Points().Count(p => p.IsAlive()));
            Assert.AreEqual(2, BackgroundPreviewGeometry.Curves().Count(p => p.IsAlive()));
            Assert.AreEqual(2, BackgroundPreviewGeometry.Meshes().Count(p => p.IsAlive()));
        }

        [Test]
        public void InsideInstance_OtherGeometryTransparent()
        {
            Model.OpenCustomNodeWorkspace(customNodeWorkspace.CustomNodeId);

            Assert.AreEqual(1, BackgroundPreviewGeometry.Points().Count(p => p.IsDead()));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Curves().Count(p => p.IsDead()));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Meshes().Count(p => p.IsDead()));
        }

        [Test]
        public void InsideInstance_HiddenInHomeSpace_NoGeometry()
        {
            var hws = Model.Workspaces.FirstOrDefault(ws => ws is HomeWorkspaceModel);
            Assert.NotNull(hws);
            var customNode = hws.Nodes.FirstOrDefault(n => n is Function);
            Assert.NotNull(customNode);
            customNode.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Model.OpenCustomNodeWorkspace(customNodeWorkspace.CustomNodeId);

            Assert.AreEqual(2, BackgroundPreviewGeometry.Meshes().Count(m=> m.Visibility == Visibility.Visible));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Meshes().Count(m => m.Visibility == Visibility.Hidden));
        }

        [Test]
        public void InsideInstance_PartiallyApplied_AllGeometrySolid()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        public void InsideNestedInstance_HasOuterGeometryAndOuterInstanceGeometry()
        {
            Assert.Inconclusive("Finish me.");
        }
    }
}

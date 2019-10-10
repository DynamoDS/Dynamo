using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class CustomNodeInsideHomeWorkspaceTests : VisualizationTest
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
        public async void InHomeWorkspace_CustomNodeInstance_HasGeometry()
        {
            Assert.AreEqual(11, BackgroundPreviewGeometry.Meshes().Count());
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
                Enumerable.Empty<Dynamo.Graph.Notes.NoteModel>(),
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

    public class CustomNodeInsideCustomWorkspaceTestBase : VisualizationTest
    {
        protected CustomNodeInfo info;
        protected CustomNodeWorkspaceModel customNodeWorkspace;

        [SetUp]
        public virtual void Setup()
        {
            var customNodePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\custom-nodes\FiveCubes.dyf");

            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    customNodePath,
                    true,
                    out info));

            // Disable edge rendering to ensure that curve counts are correct.
            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
        }
    }

    [TestFixture]
    public class CustomNodeNotInWorkspaceTests : VisualizationTest
    {
        [Test]
        public void InsideCustomNode_NotPlacedInHomeWorkspace_NoGeometry()
        {
            OpenVisualizationTest(@"custom-nodes\FiveCubes.dyf");
            Assert.AreEqual(0, BackgroundPreviewGeometry.Points().Count());
            Assert.AreEqual(0, BackgroundPreviewGeometry.Curves().Count());
            Assert.AreEqual(0, BackgroundPreviewGeometry.Meshes().Count());
        }
    }

    [TestFixture]
    public class CustomNodeInsideCustomWorkspaceTests : CustomNodeInsideCustomWorkspaceTestBase
    {
        public override void Setup()
        {
            base.Setup();

            OpenVisualizationTest(@"custom-nodes\custom-node-test.dyn");

            Model.OpenCustomNodeWorkspace(info.FunctionId);
            var cws = Model.Workspaces.FirstOrDefault(ws => ws is CustomNodeWorkspaceModel);
            Assert.NotNull(cws);
            customNodeWorkspace = (CustomNodeWorkspaceModel)cws;
        }

        [Test]
        public async void InsideInstance_AllGeometryFromInstancesOfThisCustomNode_Alive()
        {
            Assert.AreEqual(2, BackgroundPreviewGeometry.Points().Count(p => p.IsAlive()));
            Assert.AreEqual(2, BackgroundPreviewGeometry.Curves().Count(p => p.IsAlive()));
            Assert.AreEqual(10, BackgroundPreviewGeometry.Meshes().Count(p => p.IsAlive()));
        }

        [Test]
        public async void InsideInstance_OtherGeometryFromOtherNodes_Dead()
        {
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

            Assert.AreEqual(6, BackgroundPreviewGeometry.Meshes().Count(m => m.Visibility == Visibility.Visible));
            Assert.AreEqual(5, BackgroundPreviewGeometry.Meshes().Count(m => m.Visibility == Visibility.Hidden));
        }

        [Test]
        public void InsideNestedInstance_HasOuterGeometryAndOuterInstanceGeometry()
        {
            Assert.Inconclusive("Finish me.");
        }
    }

    [TestFixture]
    public class CustomNodePartialApplicationTests : CustomNodeInsideCustomWorkspaceTestBase
    {
        public override void Setup()
        {
            base.Setup();

            OpenVisualizationTest(@"custom-nodes\custom-node-partial-application-test.dyn");

            Model.OpenCustomNodeWorkspace(info.FunctionId);
            var cws = Model.Workspaces.FirstOrDefault(ws => ws is CustomNodeWorkspaceModel);
            Assert.NotNull(cws);
            customNodeWorkspace = (CustomNodeWorkspaceModel)cws;
        }

        [Test, Category("Failure")]
        public void InsideInstance_PartiallyApplied_AllGeometryFromInstancesOfThisCustomNode_Alive()
        {
            // The mirror data for the output port of List.Map is null during testing.

            Assert.AreEqual(1, BackgroundPreviewGeometry.Points().Count(p => p.IsDead()));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Curves().Count(p => p.IsDead()));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Meshes().Count(p => p.IsDead()));

            Assert.AreEqual(0, BackgroundPreviewGeometry.Points().Count(p => p.IsAlive()));
            Assert.AreEqual(0, BackgroundPreviewGeometry.Curves().Count(p => p.IsAlive()));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Meshes().Count(p => p.IsAlive()));
        }
    }
}

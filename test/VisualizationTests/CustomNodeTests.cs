using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using CoreNodeModels;
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
        public void InHomeWorkspace_CustomNodeInstance_HasGeometry()
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
        public void InsideInstance_AllGeometryFromInstancesOfThisCustomNode_Alive()
        {
            Assert.AreEqual(2, BackgroundPreviewGeometry.Points().Count(p => p.IsAlive()));
            Assert.AreEqual(2, BackgroundPreviewGeometry.Curves().Count(p => p.IsAlive()));
            Assert.AreEqual(2, BackgroundPreviewGeometry.Meshes().Count(p => p.IsAlive()));
        }

        [Test]
        public void InsideInstance_OtherGeometryFromOtherNodes_Dead()
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

            Assert.AreEqual(2, BackgroundPreviewGeometry.Meshes().Count(m => m.Visibility == Visibility.Visible));
            Assert.AreEqual(1, BackgroundPreviewGeometry.Meshes().Count(m => m.Visibility == Visibility.Hidden));
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



    [TestFixture]
    public class CustomNodeDefineDataTests : VisualizationTest
    {
        [Test]
        public void TestDefineDataAutoModeType()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\defineData\defineDataTest.dyn");

            ViewModel.OpenCommand.Execute(openPath);

            var lockedGUID = "e02596f9-9e1f-43a2-9f61-9909ec58ca34";
            var unlockedGUID = "ee557143-64bc-4961-981c-2794af48b79f";

            var lockedNode = Model.CurrentWorkspace.Nodes.First(n => n.GUID.ToString() == lockedGUID) as DefineData;
            var unlockedNode = Model.CurrentWorkspace.Nodes.First(n => n.GUID.ToString() == unlockedGUID) as DefineData;

            RunCurrentModel();

            var nodes = ViewModel.CurrentSpaceViewModel.Nodes;

            Assert.IsFalse(lockedNode.IsAutoMode);
            Assert.IsTrue(lockedNode.DisplayValue == "Boolean");
            Assert.IsTrue(lockedNode.Infos.Count == 1);

            RunCurrentModel();

            Assert.IsTrue(unlockedNode.IsAutoMode);
            Assert.IsTrue(unlockedNode.DisplayValue == "Integer");
            Assert.IsTrue(unlockedNode.Infos.Count == 0, "The AutoMode node should have found the correct type and have no Errors, but it does .. ");
        }

        [Test]
        public void TestDefineDataCorrectInheritanceDisplayedInAutoMode()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\defineData\defineDataTest.dyn");

            ViewModel.OpenCommand.Execute(openPath);

            var listGUID = "0cbb9f47-5a28-4898-8d28-575cb15c4455";  // List inputs are 'Line' and 'Rectangle', we expect a 'Curve' as DisplayValue
            var errorListGUID = "39edfbf6-a83b-4815-9bb0-2d7ebcff39f3"; // List inputs are 'Line', 'Rectangle' and 5 (integer), we expect a '' as DisplayValue

            var listNode = Model.CurrentWorkspace.Nodes.First(n => n.GUID.ToString() == listGUID) as DefineData;
            var errorListNode = Model.CurrentWorkspace.Nodes.First(n => n.GUID.ToString() == errorListGUID) as DefineData;

            RunCurrentModel();

            var nodes = ViewModel.CurrentSpaceViewModel.Nodes;

            Assert.IsTrue(listNode.IsAutoMode);
            Assert.IsTrue(listNode.IsList);
            Assert.AreEqual(listNode.DisplayValue, "Curve", "The correct common ancestor should be Curve, not 'Line' or 'Rectangle'");

            Assert.IsTrue(errorListNode.IsAutoMode);
            Assert.IsFalse(errorListNode.IsList);
            Assert.AreEqual(errorListNode.DisplayValue, string.Empty, "The node displays a type value, but it should not.");
        }
    }
}

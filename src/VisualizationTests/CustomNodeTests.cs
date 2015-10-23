using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class CustomNodeTests : HelixWatch3DViewModelTests
    {
        [Test]
        public void InHomeWorkspace_HasGeometry()
        {
            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(
                        GetTestDirectory(ExecutingDirectory),
                        @"core\visualization\Points.dyf"),
                    true,
                    out info));

            OpenVisualizationTest("ASM_customNode.dyn");

            //ensure that we have some visualizations
            Assert.Greater(BackgroundPreviewGeometry.TotalPoints(), 0);
        }

        [Test]
        public void InsideInstance_HasGeometry()
        {
            // Regression test for defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5165
            // To verify when some geometry nodes are converted to custom node,
            // their render packages shouldn't be carried over to custom work
            // space.
            var model = ViewModel.Model;

            OpenVisualizationTest("visualize_line_incustom.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalPoints());
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalCurves());
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalMeshes());
        }

        [Test]
        public void InsideInstance_PartiallyApplied_AllGeometrySolid()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        public void InsideInstance_OtherGeometryTransparent()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        public void InsideInstance_MultipleInstances_AllGeometrySolid()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        public void InsideInstance_HiddenInHomeSpace_NoGeometry()
        {
            Assert.Inconclusive("Finish me.");
        }
    }
}

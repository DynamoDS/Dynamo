using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class VisualizationManagerUITests : DynamoTestUI
    {
        private Watch3DView BackgroundPreview
        {
            get
            {
                return (Watch3DView)Ui.background_grid.FindName("background_preview");
            }
        }

        [Test, Category("Failing")]
        public void NothingIsVisualizedWhenThereIsNothingToVisualize()
        {
            var viz = dynSettings.Controller.VisualizationManager;

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(0, BackgroundPreview.Points.Count);
            Assert.AreEqual(0, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.Mesh.Positions.Count);
            Assert.AreEqual(0, BackgroundPreview.XAxes.Count);
            Assert.AreEqual(0, BackgroundPreview.YAxes.Count);
            Assert.AreEqual(0, BackgroundPreview.ZAxes.Count);
        }

        [Test, Category("Failing")]
        public void BackgroundPreviewDrawsOnOpen()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            //model.Open(openPath);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////graphics will have been updated at this point
            ////enabled the background preview and ensure that it 

            Assert.Inconclusive("Finish me!");
        }

        [Test, Category("Failing")]
        public void CleansUpGeometryWhenNodeFails()
        {
            Assert.Inconclusive("Can not test post-failure visualization state as we need to " +
                                "throwing testing exception which avoid OnEvaluationComplete being called.");

            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            //model.Open(openPath);

            //// check all the nodes and connectors are loaded
            //Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            //Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////adjust the number node's value - currently set to 0..5 to something that makes the XYZ error
            //var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            //numNode.Value = "blah";

            //// run the expression
            //// it will fail
            //Assert.Throws(typeof(NUnit.Framework.AssertionException), () => dynSettings.Controller.RunExpression(null));
            //var renderables = viz.Visualizations.SelectMany(x => x.Value.Points);
            //Assert.AreEqual(0, renderables.Count());
        }

        [Test, Category("Failing")]
        public void VisualizationInSyncWithPreview()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points_line.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            Assert.AreEqual(7, BackgroundPreview.Points.Count);
            Assert.AreEqual(12, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.MeshCount);

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.Nodes.First(x => x.GUID.ToString() == "a7c70c13-cc62-41a6-85ed-dc42e788181d");
            p1.IsVisible = false;

            Assert.AreEqual(1, BackgroundPreview.Points.Count);
            Assert.AreEqual(12, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.MeshCount);

            //flip off the lines node
            var l1 = model.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.IsVisible = false;

            Assert.AreEqual(1, BackgroundPreview.Points.Count);
            Assert.AreEqual(0, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.MeshCount);

            //flip those back on and ensure the visualization returns
            p1.IsVisible = true;
            l1.IsVisible = true;

            Assert.AreEqual(7, BackgroundPreview.Points.Count);
            Assert.AreEqual(12, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.MeshCount);
        }

        [Test, Category("Failing")]
        public void VisualizationInSyncWithPreviewUpstream()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points_line.dyn");
            model.Open(openPath);

            dynSettings.Controller.DynamoModel.OnRequestLayoutUpdate(this, EventArgs.Empty);

            // run the expression
            dynSettings.Controller.RunExpression(null);
            Thread.Sleep(1000);

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            Assert.AreEqual(7, BackgroundPreview.Points.Count);
            Assert.AreEqual(12, BackgroundPreview.Lines.Count);
            Assert.AreEqual(0, BackgroundPreview.MeshCount);

            //flip off the line node's preview upstream
            var l1 = model.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.IsUpstreamVisible = false;

            //ensure that the watch 3d is not showing the upstream
            //the render descriptions will still be around for those
            //nodes, but watch 3D will not be showing them

            var watch = model.Nodes.First(x => x.GetType().Name == "Watch3D");
            var watchView = watch.GetType().GetProperty("View").GetValue(watch, null);
            var points = watchView.GetType().GetProperty("Points").GetValue(watchView, null) as List<Point3D>;
            Assert.AreEqual(0, points.Count);
        }

        [Test, Category("Failing")]
        public void CanVisualizePoints()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            //model.Open(openPath);

            //// check all the nodes and connectors are loaded
            //Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            //Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////ensure that the number of visualizations matches the 
            ////number of pieces of geometry in the collection
            //Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.Points.Count);

            ////adjust the number node's value - currently set to 0..5 (6 elements)
            //var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            //numNode.Value = "0..10";
            //dynSettings.Controller.RunExpression(null);

            //Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.Points.Count);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test, Category("Failing")]
        public void CleansUpGeometryWhenNodesAreDisconnected()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points_line.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure the correct representations

            //look at the data in the visualization manager
            //ensure that the number of Drawable nodes
            //and the number of entries in the Dictionary match
            Assert.AreEqual(7, BackgroundPreview.Points.Count);
            Assert.AreEqual(6, BackgroundPreview.Lines.Count / 2);
            
            //delete a conector coming into the lines node
            var lineNode = model.Nodes.FirstOrDefault(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            var port = lineNode.InPorts.First();
            port.Disconnect(port.Connectors.First());

            //ensure that the visualization no longer contains
            //the renderables for the line node
            Assert.AreEqual(7, BackgroundPreview.Points.Count);
            Assert.AreEqual(0, BackgroundPreview.Lines.Count);
        }

        [Test, Category("Failing")]
        public void CanVisualizeASMSolids()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_thicken.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.IsTrue(BackgroundPreview.Mesh.TriangleIndices.Count > 0);

            model.HomeSpace.HasUnsavedChanges = false;
        }

        [Test, Category("Failing")]
        public void CanVisualizeASMSurfaces()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_cuboid.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //var meshes = viz.Visualizations.SelectMany(x => x.Value.Meshes);
            Assert.AreEqual(36, BackgroundPreview.Mesh.Positions.Count);
        }

        [Test, Category("Failing")]
        public void CanVisualizeCoordinateSystems()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_coordinateSystem.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(2, BackgroundPreview.XAxes.Count);
            Assert.AreEqual(2, BackgroundPreview.YAxes.Count);
            Assert.AreEqual(2, BackgroundPreview.ZAxes.Count);
        }

        [Test, Category("Failing")]
        public void CanVisualizeGeometryFromPython()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_python.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(1000, BackgroundPreview.Points.Count);
            Assert.AreEqual(1000, BackgroundPreview.MeshCount);

        }

        [Test, Category("Failing")]
        public void VisualizationIsDeletedWhenNodeIsRemoved()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(6, BackgroundPreview.Points.Count);

            //delete a node and ensure that the renderables are cleaned up
            var pointNode = model.Nodes.FirstOrDefault(x => x.GUID.ToString() == "0b472626-e18f-404a-bec4-d84ad7f33011");
            var modelsToDelete = new List<ModelBase> {pointNode};
            model.DeleteModelInternal(modelsToDelete);

            model.HomeSpace.HasUnsavedChanges = false;

            Assert.AreEqual(0, BackgroundPreview.Points.Count);
        }

        [Test, Category("Failing")]
        public void VisualizationsAreClearedWhenWorkspaceIsCleared()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that we have some visualizations
            Assert.Greater(BackgroundPreview.Points.Count, 0);

            //now clear the workspace
            model.Clear(null);

            //ensure that we have no visualizations
            Assert.AreEqual(0, BackgroundPreview.Points.Count);
        }

        [Test, Category("Failing")]
        public void VisualizationsAreCreatedForCustomNodes()
        {
            var model = dynSettings.Controller.DynamoModel;

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\Points.dyf"))
                != null);
            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_customNode.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that we have some visualizations
            Assert.Greater(BackgroundPreview.Points.Count, 0);
        }

        [Test, Category("Failing")]
        public void HonorsPreviewSaveState()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points_line_noPreview.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //all nodes are set to not preview in the file
            //ensure that we have no visualizations
            Assert.AreEqual(0, BackgroundPreview.Lines.Count);
        }

        [Test, Category("Failing")]
        public void CanDrawNodeLabels()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\Labels.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            //before we run the expression, confirm that all nodes
            //have label display set to false - the default
            Assert.IsTrue(model.AllNodes.All(x => x.DisplayLabels != true));

            var cbn = model.Nodes.FirstOrDefault(x => x.GUID.ToString() == "fdec3b9b-56ae-4d01-85c2-47b8425e3130") as CodeBlockNodeModel;
            Assert.IsNotNull(cbn);
            cbn.Code = "Point.ByCoordinates(a<1>,a<1>,a<1>);";
            
            // run the expression
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
            Assert.AreEqual(4, BackgroundPreview.Points.Count());

            cbn.Code = "Point.ByCoordinates(a<1>,a<1>,a<1>);";

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            cbn.DisplayLabels = true;
            Assert.AreEqual(BackgroundPreview.Text.Count(), 4);

            cbn.Code = "Point.ByCoordinates(a<1>,a<2>,a<3>);";

            //change the lacing to cross product 
            //ensure that the labels update to match
            //ptNode.ArgumentLacing = LacingStrategy.CrossProduct;
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
            Assert.AreEqual(64, BackgroundPreview.Points.Count());
            Assert.AreEqual(64, BackgroundPreview.Text.Count());

            cbn.DisplayLabels = false;
            Assert.AreEqual(0, BackgroundPreview.Text.Count());

            model.HomeSpace.HasUnsavedChanges = false;
        }

        [Test, Category("Failing")]
        public void CanDrawNodeLabelsOnCurves()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\GeometryTestFiles\BSplineCurveTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            //before we run the expression, confirm that all nodes
            //have label display set to false - the default
            Assert.IsTrue(model.AllNodes.All(x => x.DisplayLabels != true));

            // run the expression
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            //10 lines segments in this file
            Assert.AreEqual(60, BackgroundPreview.Lines.Count());

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            var crvNode = model.Nodes.FirstOrDefault(x => x.GUID.ToString() == "e9e53fe0-a0b0-4cf7-93d5-5eea8f0428f2");
            Assert.IsNotNull(crvNode);
            crvNode.DisplayLabels = true;

            Assert.AreEqual(6,BackgroundPreview.Text.Count());
        }

        private int GetTotalDrawablesInModel()
        {
            return dynSettings.Controller.DynamoModel.Nodes
                .SelectMany(x=>x.RenderPackages)
                .Cast<RenderPackage>()
                .Where(x=>x.IsNotEmpty())
                .Aggregate(0,(a, b) => a + b.ItemsCount);
        }
    }
}

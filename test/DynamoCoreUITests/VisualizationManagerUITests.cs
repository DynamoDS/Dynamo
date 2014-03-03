using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.UI
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

        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            Controller = DynamoController.MakeSandbox();
            DynamoController.IsTestMode = true;
            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder();
            }
        }

        [TearDown]
        public void Exit()
        {
            if (Ui.IsLoaded)
                Ui.Close();
        }

        [Test]
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

        [Test]
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

        /*
        [Test]
        public void VisualizationInSyncWithPreviewUpstream()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;
            
            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line.dyn");
            model.Open(openPath);

            dynSettings.Controller.DynamoModel.OnRequestLayoutUpdate(this, EventArgs.Empty);

            // run the expression
            dynSettings.Controller.RunExpression(null);
            Thread.Sleep(1000);

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);
            
            Assert.AreEqual(7, pointCount);
            Assert.AreEqual(12, lineCount);
            Assert.AreEqual(0, meshCount);

            //flip off the line node's preview upstream
            var l1 = model.Nodes.First(x => x is LineNode);
            l1.IsUpstreamVisible = false;

            //ensure that the watch 3d is not showing the upstream
            //the render descriptions will still be around for those
            //nodes, but watch 3D will not be showing them

            var watch = model.Nodes.First(x => x.GetType().Name == "Watch3D");
            var watchView = watch.GetType().GetProperty("View").GetValue(watch, null);
            var points = watchView.GetType().GetProperty("Points").GetValue(watchView, null) as ThreadSafeList<Point3D>;
            Assert.AreEqual(0, points.Count);

        }
        */

        [Test]
        public void CanVisualizePoints()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that the number of visualizations matches the 
            //number of pieces of geometry in the collection
            Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.Points.Count);

            //adjust the number node's value - currently set to 0..5 (6 elements)
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..10";
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.Points.Count);
        }

        [Test]
        public void CleansUpGeometryWhenNodesAreDisconnected()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line.dyn");
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

        [Test]
        public void CanVisualizeASMSolids()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_thicken.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.IsTrue(BackgroundPreview.Mesh.TriangleIndices.Count > 0);
        }

        [Test]
        public void CanVisualizeASMSurfaces()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_cuboid.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //var meshes = viz.Visualizations.SelectMany(x => x.Value.Meshes);
            Assert.AreEqual(36, BackgroundPreview.Mesh.Positions.Count);
        }

        [Test]
        public void CanVisualizeCoordinateSystems()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_coordinateSystem.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(2, BackgroundPreview.XAxes.Count);
            Assert.AreEqual(2, BackgroundPreview.YAxes.Count);
            Assert.AreEqual(2, BackgroundPreview.ZAxes.Count);
        }

        [Test]
        public void CanVisualizeGeometryFromPython()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_python.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            ////var drawables = VisualizationManager.GetDrawableNodesInModel();
            ////Assert.AreEqual(drawables.Count(), viz.Visualizations.Count);

            //int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(45, BackgroundPreview.Points.Count);
            Assert.AreEqual(1, BackgroundPreview.MeshCount);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void VisualizationIsDeletedWhenNodeIsRemoved()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            //model.Open(openPath);

            //// check all the nodes and connectors are loaded
            //Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            //Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            //int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(6, pointCount);

            ////delete a node and ensure that the renderables are cleaned up
            //var pointNode = model.Nodes.FirstOrDefault(x => x is Point3DNode);
            //List<ModelBase> modelsToDelete = new List<ModelBase>();
            //modelsToDelete.Add(pointNode);
            //model.DeleteModelInternal(modelsToDelete);

            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(0, pointCount);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void VisualizationsAreClearedWhenWorkspaceIsCleared()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            //model.Open(openPath);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////ensure that we have some visualizations
            //Assert.Greater(viz.Visualizations.Count, 0);

            ////now clear the workspace
            //model.Clear(null);

            ////ensure that we have no visualizations
            //Assert.AreEqual(0, viz.Visualizations.Count);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void VisualizationsAreCreatedForCustomNodes()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //Assert.IsTrue(
            //    Controller.CustomNodeManager.AddFileToPath(Path.Combine(GetTestDirectory(), @"core\visualization\Points.dyf"))
            //    != null);
            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_customNode.dyn");
            //model.Open(openPath);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////ensure that we have some visualizations
            //Assert.Greater(viz.Visualizations.Count, 0);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void HonorsPreviewSaveState()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line_noPreview.dyn");
            //model.Open(openPath);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////all nodes are set to not preview in the file
            ////ensure that we have no visualizations
            //int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(0, lineCount);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void CanDrawNodeLabels()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\Labels.dyn");
            //model.Open(openPath);

            //// check all the nodes and connectors are loaded
            //Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            ////before we run the expression, confirm that all nodes
            ////have label display set to false - the default
            //Assert.IsTrue(model.AllNodes.All(x => x.DisplayLabels != true));

            //// run the expression
            //Assert.DoesNotThrow(()=>dynSettings.Controller.RunExpression(null));

            //Assert.AreEqual(4, viz.Visualizations.SelectMany(x=>x.Value.Points).Count());

            ////label displayed should be possible now because
            ////some nodes have values. toggle on label display
            //var ptNode = model.Nodes.FirstOrDefault(x => x is Point3DNode);
            //Assert.IsNotNull(ptNode);
            //ptNode.DisplayLabels = true;

            //Assert.AreEqual(viz.Visualizations.SelectMany(x=>x.Value.Text).Count(), 4);

            ////change the lacing to cross product 
            ////ensure that the labels update to match
            //ptNode.ArgumentLacing = LacingStrategy.CrossProduct;
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));
            //Assert.AreEqual(64, viz.Visualizations.SelectMany(x => x.Value.Points).Count());
            //Assert.AreEqual(64, viz.Visualizations.SelectMany(x => x.Value.Text).Count());

            //ptNode.DisplayLabels = false;
            //Assert.AreEqual(0, viz.Visualizations.SelectMany(x => x.Value.Text).Count());

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        [Test]
        public void CanDrawNodeLabelsOnCurves()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\BSplineCurveTest.dyn");
            //model.Open(openPath);

            //// check all the nodes and connectors are loaded
            //Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            ////before we run the expression, confirm that all nodes
            ////have label display set to false - the default
            //Assert.IsTrue(model.AllNodes.All(x => x.DisplayLabels != true));

            //// run the expression
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            ////10 lines segments in this file
            //Assert.AreEqual(20, viz.Visualizations.SelectMany(x => x.Value.Lines).Count());

            ////label displayed should be possible now because
            ////some nodes have values. toggle on label display
            //var crvNode = model.Nodes.FirstOrDefault(x => x is BSplineCurveNode);
            //Assert.IsNotNull(crvNode);
            //crvNode.DisplayLabels = true;

            //Assert.AreEqual(viz.Visualizations.SelectMany(x => x.Value.Text).Count(), 5);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        private int GetTotalDrawablesInModel()
        {
            return dynSettings.Controller.DynamoModel.Nodes.Select(x => ((RenderPackage)x.RenderPackage).ItemsCount).Aggregate((a, b) => a + b);
        }
    }
}

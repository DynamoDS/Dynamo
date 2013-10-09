﻿using System.Collections.Generic;
using System.IO;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using System.Linq;

namespace Dynamo.Tests
{
    [TestFixture]
    class 
        VisualizationManagerTests : DynamoUnitTest
    {
        [Test]
        public void CanVisualizeASMPoints()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that the number of visualizations matches the 
            //number of pieces of geometry in the collection
            var drawables = VisualizationManager.GetAllDrawablesInModel();
            var renderables = viz.Visualizations.SelectMany(x => x.Value.Points);
            Assert.AreEqual(drawables.Values.SelectMany(x=>x).Count(), renderables.Count());

            //adjust the number node's value - currently set to 0..5 (6 elements)
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..10";
            dynSettings.Controller.RunExpression(null);

            drawables = VisualizationManager.GetAllDrawablesInModel();
            renderables = viz.Visualizations.SelectMany(x => x.Value.Points);
            Assert.AreEqual(drawables.Values.SelectMany(x=>x).Count(), renderables.Count());
        }

        [Test]
        public void CleansUpGeometryWhenNodeFails()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //adjust the number node's value - currently set to 0..5 to something that makes the XYZ error
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "blah";

            // run the expression
            // it will fail
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => dynSettings.Controller.RunExpression(null));
            var renderables = viz.Visualizations.SelectMany(x => x.Value.Points);
            Assert.AreEqual(0, renderables.Count());
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
            var points = viz.Visualizations.SelectMany(x => x.Value.Points);
            var lines = viz.Visualizations.SelectMany(x => x.Value.Lines);

            Assert.AreEqual(7,points.Count());
            Assert.AreEqual(6, lines.Count()/2);

            //delete a conector coming into the lines node
            var lineNode = model.Nodes.First(x => x is LineNode);
            var port = lineNode.InPorts.First();
            port.Disconnect(port.Connectors.First());

            //ensure that the visualization no longer contains
            //the renderables for the line node
            points = viz.Visualizations.SelectMany(x => x.Value.Points);
            lines = viz.Visualizations.SelectMany(x => x.Value.Lines);
            Assert.AreEqual(7, points.Count());
            Assert.AreEqual(0, lines.Count());
        }

        [Test]
        public void NothingIsVisualizedWhenThereIsNothingToVisualize()
        {
            var viz = dynSettings.Controller.VisualizationManager;

            // run the expression
            dynSettings.Controller.RunExpression(null);

            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(0, pointCount);
            Assert.AreEqual(0, lineCount);
            Assert.AreEqual(0, meshCount);
            Assert.AreEqual(0, xCount);
            Assert.AreEqual(0, yCount);
            Assert.AreEqual(0, zCount);
        }

        [Test]
        public void BackgroundPreviewDrawsOnOpen()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //graphics will have been updated at this point
            //enabled the background preview and ensure that it 
        }
        
        [Test]
        public void VisualizationInSyncWithPreview()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(7, pointCount);
            Assert.AreEqual(12, lineCount);
            Assert.AreEqual(0, meshCount);

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.Nodes.First(x => x is Point3DNode && x.GUID.ToString() == "354ddfd4-76dc-4dca-a622-bac6418393cb");
            p1.IsVisible = false;

            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(1, pointCount);
            Assert.AreEqual(12, lineCount);
            Assert.AreEqual(0, meshCount);

            //flip off the lines node
            var l1 = model.Nodes.First(x => x is LineNode);
            l1.IsVisible = false;

            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(1, pointCount);
            Assert.AreEqual(0, lineCount);
            Assert.AreEqual(0, meshCount);

            //flip those back on and ensure the visualization returns
            p1.IsVisible = true;
            l1.IsVisible = true;

            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(7, pointCount);
            Assert.AreEqual(12, lineCount);
            Assert.AreEqual(0, meshCount);
        }

        [Test]
        public void CanVisualizeASMSolids()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_thicken.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var meshes = viz.Visualizations.SelectMany(x => x.Value.Meshes);
            Assert.AreEqual(1, meshes.Count());
        }

        [Test]
        public void CanVisualizeASMSurfaces()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_cuboid.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var meshes = viz.Visualizations.SelectMany(x => x.Value.Meshes);
            Assert.AreEqual(1, meshes.Count());
        }

        [Test]
        public void CanVisualizeASMCoordinateSystems()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_coordinateSystem.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var xs = viz.Visualizations.SelectMany(x => x.Value.XAxisPoints);
            var ys = viz.Visualizations.SelectMany(x => x.Value.YAxisPoints);
            var zs = viz.Visualizations.SelectMany(x => x.Value.ZAxisPoints);

            Assert.AreEqual(2, xs.Count());
            Assert.AreEqual(2, ys.Count());
            Assert.AreEqual(2, zs.Count());
        }

        [Test]
        public void CanVisualizeGeometryFromPython()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_python.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //var drawables = VisualizationManager.GetDrawableNodesInModel();
            //Assert.AreEqual(drawables.Count(), viz.Visualizations.Count);
            
            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(45, pointCount);
            Assert.AreEqual(1, meshCount);
            
        }

        [Test]
        public void VisualizationIsDeletedWhenNodeIsRemoved()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(6, pointCount);

            //delete a node and ensure that the renderables are cleaned up
            var pointNode = model.Nodes.FirstOrDefault(x => x is Point3DNode);
            model.Delete(pointNode);

            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(0, pointCount);
        }

        [Test]
        public void VisualizationIsCreatedWhenNodeIsAdded()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            var nodeData = new Dictionary<string, object>();
            nodeData["x"] = 100.0;
            nodeData["y"] = 100.0;
            nodeData["name"] = "Dynamo.Nodes.Point3DNode";

            model.CreateNode(nodeData);
            Assert.AreEqual(1, model.Nodes.Count);

            Assert.AreEqual(1, viz.Visualizations.Count);
        }

        [Test]
        public void VisualizationsAreClearedWhenWorkspaceIsCleared()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that we have some visualizations
            Assert.Greater(viz.Visualizations.Count, 0);

            //now clear the workspace
            model.Clear(null);

            //ensure that we have no visualizations
            Assert.AreEqual(0, viz.Visualizations.Count);
        }
    
        [Test]
        public void VisualizationsAreCreatedForCustomNodes()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(GetTestDirectory(), @"core\visualization\Points.dyf"))
                != null);
            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_customNode.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //ensure that we have some visualizations
            Assert.Greater(viz.Visualizations.Count, 0);
        }
    
        [Test]
        public void HonorsPreviewSaveState()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line_noPreview.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //all nodes are set to not preview in the file
            //ensure that we have no visualizations
            int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            viz.GetRenderableCounts(
                out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            Assert.AreEqual(0, lineCount);
        }
    }
}

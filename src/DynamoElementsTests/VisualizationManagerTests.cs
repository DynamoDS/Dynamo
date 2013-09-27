using System.IO;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using System.Linq;

namespace Dynamo.Tests
{
    [TestFixture]
    class VisualizationManagerTests : DynamoUnitTest
    {
        [Test]
        public void CanVisualizeASMPoints()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //look at the data in the visualization manager
            //ensure that the number of IDrawable nodes
            //and the number of entries in the Dictionary match
            var drawables = model.Nodes.Where(x => x is IDrawable);
            Assert.AreEqual(drawables.Count(), viz.Visualizations.Count);

            //ensure that the number of visualizations matches the 
            //number of pieces of geometry in the collection
            var geoms = viz.Visualizations.SelectMany(x => x.Value.Geometry);
            var renderables = viz.Visualizations.SelectMany(x => x.Value.Description.Points);
            Assert.AreEqual(geoms.Count(), renderables.Count());

            //adjust the number node's value - currently set to 0..5 (6 elements)
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..10";
            dynSettings.Controller.RunExpression(null);

            geoms = viz.Visualizations.SelectMany(x => x.Value.Geometry);
            renderables = viz.Visualizations.SelectMany(x => x.Value.Description.Points);
            Assert.AreEqual(geoms.Count(), renderables.Count());
        }

        [Test]
        public void CleansUpGeometryWhenNodeFails()
        {
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //adjust the number node's value - currently set to 0..5 to something that makes the XYZ error
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "blah";

            // run the expression
            // it will fail
            Assert.Throws(typeof(NUnit.Framework.AssertionException), () => dynSettings.Controller.RunExpression(null));
            var renderables = viz.Visualizations.SelectMany(x => x.Value.Description.Points);
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
            //ensure that the number of IDrawable nodes
            //and the number of entries in the Dictionary match
            var drawables = model.Nodes.Where(x => x is IDrawable);
            var points = viz.Visualizations.SelectMany(x => x.Value.Description.Points);
            var lines = viz.Visualizations.SelectMany(x => x.Value.Description.Lines);
            Assert.AreEqual(drawables.Count(), viz.Visualizations.Count);

            Assert.AreEqual(7,points.Count());
            Assert.AreEqual(6, lines.Count()/2);

            //delete a conector coming into the lines node
            var lineNode = model.Nodes.First(x => x is LineNode);
            //var connector = model.CurrentWorkspace.Connectors.First(x => x.End.Owner == lineNode);
            //model.Delete(connector);
            var port = lineNode.InPorts.First();
            port.Disconnect(port.Connectors.First());

            //ensure that the visualization no longer contains
            //the renderables for the line node
            points = viz.Visualizations.SelectMany(x => x.Value.Description.Points);
            lines = viz.Visualizations.SelectMany(x => x.Value.Description.Lines);
            Assert.AreEqual(7, points.Count());
            Assert.AreEqual(0, lines.Count());
        }

        [Test]
        public void NothingIsVisualizedWhenThereIsNothingToVisualize()
        {
            Assert.Inconclusive("Finish me.");
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
        public void CanVisualizeASMSolids()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void CanVisualizeASMSurfaces()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = dynSettings.Controller.DynamoModel;
            var viz = dynSettings.Controller.VisualizationManager;

            string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_loft.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var drawables = model.Nodes.Where(x => x is IDrawable);
            var meshes = viz.Visualizations.SelectMany(x => x.Value.Description.Meshes);
            Assert.AreEqual(drawables.Count(), viz.Visualizations.Count);
            Assert.AreEqual(1, meshes.Count());
        }

        [Test]
        public void CanVisualizeASMCoordinateSystems()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void CanVisualizeGeometryFromPython()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void VisualizationIsDeletedWhenNodeIsRemoved()
        {
            
        }

        [Test]
        public void VisualizationIsUpdatedWhenPreviewStateIsChanged()
        {
            
        }
    }
}

using System;
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

            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void CanVisualizeASMSolids()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void CanVisualizeASMSurfaces()
        {
            Assert.Inconclusive("Finish me!");
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

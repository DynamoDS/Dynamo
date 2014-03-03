using System.Collections.Generic;
using System.IO;
using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using System.Linq;
using Dynamo.Models;

namespace Dynamo.Tests
{
    [TestFixture]
    class VisualizationManagerTests : DynamoUnitTest
    {
        
        [Test]
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
        
        [Test]
        public void VisualizationInSyncWithPreview()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //var viz = dynSettings.Controller.VisualizationManager;

            //string openPath = Path.Combine(GetTestDirectory(), @"core\visualization\ASM_points_line.dyn");
            //model.Open(openPath);

            //// run the expression
            //dynSettings.Controller.RunExpression(null);

            ////we start with all previews disabled
            ////the graph is two points feeding into a line

            ////ensure that visulations match our expectations
            //int pointCount, lineCount, meshCount, xCount, yCount, zCount;
            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(7, pointCount);
            //Assert.AreEqual(12, lineCount);
            //Assert.AreEqual(0, meshCount);

            ////now flip off the preview on one of the points
            ////and ensure that the visualization updates without re-running
            //var p1 = model.Nodes.First(x => x is Point3DNode && x.GUID.ToString() == "354ddfd4-76dc-4dca-a622-bac6418393cb");
            //p1.IsVisible = false;

            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(1, pointCount);
            //Assert.AreEqual(12, lineCount);
            //Assert.AreEqual(0, meshCount);

            ////flip off the lines node
            //var l1 = model.Nodes.First(x => x is LineNode);
            //l1.IsVisible = false;

            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(1, pointCount);
            //Assert.AreEqual(0, lineCount);
            //Assert.AreEqual(0, meshCount);

            ////flip those back on and ensure the visualization returns
            //p1.IsVisible = true;
            //l1.IsVisible = true;

            //viz.GetRenderableCounts(
            //    out pointCount, out lineCount, out meshCount, out xCount, out yCount, out zCount);

            //Assert.AreEqual(7, pointCount);
            //Assert.AreEqual(12, lineCount);
            //Assert.AreEqual(0, meshCount);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        

        

    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.UI.Controls;
using Dynamo.Units;
using Dynamo.Utilities;
using DynamoCore.UI.Controls;
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
            var points = watchView.GetType().GetProperty("HelixPoints").GetValue(watchView, null) as ThreadSafeList<Point3D>;
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
            Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.HelixPoints.Count);

            //adjust the number node's value - currently set to 0..5 (6 elements)
            var numNode = (DoubleInput)model.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..10";
            dynSettings.Controller.RunExpression(null);

            Assert.AreEqual(GetTotalDrawablesInModel(), BackgroundPreview.HelixPoints.Count);
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
            Assert.AreEqual(7, BackgroundPreview.HelixPoints.Count);
            Assert.AreEqual(6, BackgroundPreview.HelixLines.Count / 2);
            
            //delete a conector coming into the lines node
            var lineNode = model.Nodes.FirstOrDefault(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            var port = lineNode.InPorts.First();
            port.Disconnect(port.Connectors.First());

            //ensure that the visualization no longer contains
            //the renderables for the line node
            Assert.AreEqual(7, BackgroundPreview.HelixPoints.Count);
            Assert.AreEqual(0, BackgroundPreview.HelixLines.Count);

            Assert.Inconclusive("Ian to finish after viz manager work.");
        }

        private int GetTotalDrawablesInModel()
        {
            return dynSettings.Controller.DynamoModel.Nodes.Select(x => ((RenderPackage)x.RenderPackage).ItemsCount).Aggregate((a, b) => a + b);
        }
    }
}

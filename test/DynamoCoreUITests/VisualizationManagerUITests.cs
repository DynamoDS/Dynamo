using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class VisualizationManagerUITests : DynamoTestUI
    {
        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            Controller = DynamoController.MakeSandbox();
            Controller.Testing = true;
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
    }
}

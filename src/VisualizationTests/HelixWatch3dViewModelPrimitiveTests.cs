using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SharpDX;
using Watch3DNodeModelsWpf;

namespace WpfVisualizationTests
{
    class HelixWatch3dViewModelPrimitiveTests : WpfVisualizationTests.VisualizationTest
    {
        #region meshes
        [Test]
        public void DisplayByGeometryColorHasColoredSpheres()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLinesSpheres.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all spheres
            var spheres = BackgroundPreviewGeometry.OfType<DynamoGeometryModel3D>();
            var sphereColors = spheres.SelectMany(x => x.Geometry.Colors);
            var blueSphereCount = sphereColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowSphereCount = sphereColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            //multiple verts per sphere.
            Assert.AreEqual(447336, blueSphereCount);
            Assert.AreEqual(439128, yellowSphereCount);
        }

        #endregion

        #region points and lines display colors

        [Test]
        public void DisplayByGeometryColorHasColoredPoints()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);
        }

        [Test]
        public void DisplayByGeometryColorHasColoredPointsPreviewToggled()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");


            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); 
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

            var pointDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("9af882721af64514883ce7390fcfe523")).First();

            //disable preview of the pts node
            pointDisplayNode.UpdateValue(new UpdateValueParams("IsVisible", false.ToString()));
            DispatcherUtil.DoEvents();

            Assert.True(BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>()
                .All(pt => pt.Visibility == System.Windows.Visibility.Hidden));

            pointDisplayNode.UpdateValue(new UpdateValueParams("IsVisible", true.ToString()));
            DispatcherUtil.DoEvents();

            pointColors = points.SelectMany(x => x.Geometry.Colors);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); 
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

        }


        [Test]
        public void DisplayByGeometryColorHasColoredLines()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all lines
            var lines = BackgroundPreviewGeometry.OfType<DynamoLineGeometryModel3D>();
            var lineColors = lines.SelectMany(x => x.Geometry.Colors);
            var blueLineColorsCount = lineColors.Where(x => x == Colors.Blue.ToColor4()).Count();
            //multiply * 2 since each line has two vert colors.
            var yellowLineColorsCount = lineColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14897 * 2, blueLineColorsCount);
            Assert.AreEqual(14895 * 2, yellowLineColorsCount);
        }

        #endregion


        #region special renderpackages
        [Test]
        public void RenderCoreHasFlagsSetCorrectlyForSpecialRenderPackage()
        {
            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            var mockManipulator = new DynamoGeometryModel3D()
            {
                Transform = MatrixTransform3D.Identity,
                Material = PhongMaterials.Red,
                IsHitTestVisible = true,
                RequiresPerVertexColoration = false,
            };
            Assert.False((mockManipulator.SceneNode.RenderCore as DynamoGeometryMeshCore).dataCore.IsSpecialRenderPackageData);
            AttachedProperties.SetIsSpecialRenderPackage(mockManipulator, true);
            //assert that setting this attached property updated the meshcore data.
            Assert.IsTrue((mockManipulator.SceneNode.RenderCore as DynamoGeometryMeshCore).dataCore.IsSpecialRenderPackageData);
          
        }
        #endregion

    }
}

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

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

            var pointDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("9af882721af64514883ce7390fcfe523")).First();

            //select
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(pointDisplayNode);
            DispatcherUtil.DoEvents();

            var selectedColorCount = pointColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            Assert.AreEqual(29791, selectedColorCount);

            //disable preview of the pts node
            pointDisplayNode.UpdateValue(new UpdateValueParams("IsVisible", false.ToString()));
            DispatcherUtil.DoEvents();

            Assert.True(BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>()
                .All(pt => pt.Visibility == System.Windows.Visibility.Hidden));

            pointDisplayNode.UpdateValue(new UpdateValueParams("IsVisible", true.ToString()));
            DispatcherUtil.DoEvents();

            //TODO this is unexpected but matches master behavior.
            //selectedColorCount = pointColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            //Assert.AreEqual(29791, selectedColorCount);

            //deselect
            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();

            pointColors = points.SelectMany(x => x.Geometry.Colors);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

        }

        [Test]
        public void DisplayByGeometryColorHasColoredPointsCanSwitchBetweenMultipleNodesAndSelectionUpdates()
        {
            OpenVisualizationTest("Display.ByGeometryColorPoints_Selection.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var green = pointColors.Where(x => x == new Color3(0, 1, 0).ToColor4()).Count();
            var red = pointColors.Where(x => x == new Color3(1, 0, 0).ToColor4()).Count();
            Assert.AreEqual(1331, green);
            Assert.AreEqual(216, red);

            var redPtsNode = ws.Nodes.Where(x => x.Name == "red").FirstOrDefault();
            var greenPtsNode = ws.Nodes.Where(x => x.Name == "green").FirstOrDefault();
            //select red node
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(redPtsNode);
            DispatcherUtil.DoEvents();

            //assert no red points.
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            green = pointColors.Where(x => x == new Color3(0, 1, 0).ToColor4()).Count();
            red = pointColors.Where(x => x == new Color3(1, 0, 0).ToColor4()).Count();
            Assert.AreEqual(1331, green);
            Assert.AreEqual(0, red);

            //select green node
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(greenPtsNode);
            DispatcherUtil.DoEvents();

            pointColors = points.SelectMany(x => x.Geometry.Colors);
            green = pointColors.Where(x => x == new Color3(0, 1, 0).ToColor4()).Count();
            red = pointColors.Where(x => x == new Color3(1, 0, 0).ToColor4()).Count();
            Assert.AreEqual(0, green);
            Assert.AreEqual(216, red);

            //select both
            DynamoSelection.Instance.Selection.Add(redPtsNode);
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            green = pointColors.Where(x => x == new Color3(0, 1, 0).ToColor4()).Count();
            red = pointColors.Where(x => x == new Color3(1, 0, 0).ToColor4()).Count();
            Assert.AreEqual(0, green);
            Assert.AreEqual(0, red);
        }

        [Test]
        public void DisplayByGeometryColorHasColoredPtSelection()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(pointDisplayNode);
            DispatcherUtil.DoEvents();
            //assert we have all points selected blue
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            var selectedColorCount = pointColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            Assert.AreEqual(29791, selectedColorCount);
            //now deselect, assert 0
            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();

            pointColors = points.SelectMany(x => x.Geometry.Colors);
            selectedColorCount = pointColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            Assert.AreEqual(0, selectedColorCount);
        }


        [Test]
        public void DisplayByGeometryColorHasColoredLineSelection()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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

            var lineDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("09ddd3ecea454984a37349f1266a247d")).First();

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(lineDisplayNode);
            DispatcherUtil.DoEvents();
            //assert we have all lines selected blue
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            var selectedColorCount = lineColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            Assert.AreEqual(59582, selectedColorCount);
            //now deselect, assert 0
            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();

            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            selectedColorCount = lineColors.Where(x => x == HelixWatch3DNodeViewModel.SelectedMaterial.DiffuseColor).Count();
            Assert.AreEqual(0, selectedColorCount);
        }

        #endregion

        #region freeze
        [Test]
        public void PointsFreezeColorIsCorrect()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);
            var pointDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("9af882721af64514883ce7390fcfe523")).First();

            //freeze the point geo node.
            pointDisplayNode.IsFrozen = true;
            DispatcherUtil.DoEvents();
            //assert that point colors have low alpha
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            var frozenColorsCount = pointColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(29791, frozenColorsCount);

            //unfreeze
            pointDisplayNode.IsFrozen = false;
            DispatcherUtil.DoEvents();
            //colors should be as before.
            frozenColorsCount = pointColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(0, frozenColorsCount);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);
        }

        [Test]
        public void LinesFreezeColorIsCorrect()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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

            var lineDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("09ddd3ecea454984a37349f1266a247d")).First();

            //freeze the line geo node.
            lineDisplayNode.IsFrozen = true;
            DispatcherUtil.DoEvents();
            //assert that point colors have low alpha
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            var frozenColorsCount = lineColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(59582, frozenColorsCount);

            //unfreeze
            lineDisplayNode.IsFrozen = false;
            DispatcherUtil.DoEvents();
            //colors should be as they were before freeze
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            blueLineColorsCount = lineColors.Where(x => x == Colors.Blue.ToColor4()).Count();
            //multiply * 2 since each line has two vert colors.
            yellowLineColorsCount = lineColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14897 * 2, blueLineColorsCount);
            Assert.AreEqual(14895 * 2, yellowLineColorsCount);

        }
        #endregion

        #region isolate geometry

        [Test]
        public void IsolatedColoredPtsBecomesTransparent()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

            ViewModel.BackgroundPreviewViewModel.IsolationMode = true;
            DispatcherUtil.DoEvents();
            //everything should be isolated.
            //assert that point colors have low alpha
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            var isolatedColors = pointColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791, isolatedColors);

            //select points
            var pointDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("9af882721af64514883ce7390fcfe523")).First();
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(pointDisplayNode);
            DispatcherUtil.DoEvents();
            //assert points are no longer isolated
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();
            //isolated again.
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            isolatedColors = pointColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791, isolatedColors);

            //unisolate
            ViewModel.BackgroundPreviewViewModel.IsolationMode = false;
            DispatcherUtil.DoEvents();
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

        }

        [Test]
        public void IsolatedColoredLinesBecomesTransparent()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var lines = BackgroundPreviewGeometry.OfType<DynamoLineGeometryModel3D>();
            var lineColors = lines.SelectMany(x => x.Geometry.Colors);
            var blueLinesCount = lineColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowLinesCount = lineColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14897 * 2, blueLinesCount);
            Assert.AreEqual(14895 * 2, yellowLinesCount);

            ViewModel.BackgroundPreviewViewModel.IsolationMode = true;
            DispatcherUtil.DoEvents();
            //everything should be isolated.
            //assert that point colors have low alpha
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            var isolatedColors = lineColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791 * 2, isolatedColors);

            //select points
            var lineDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("09ddd3ecea454984a37349f1266a247d")).First();
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(lineDisplayNode);
            DispatcherUtil.DoEvents();
            //assert points are no longer isolated
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            blueLinesCount = lineColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowLinesCount = lineColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14897 * 2, blueLinesCount);
            Assert.AreEqual(14895 * 2, yellowLinesCount);

            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();
            //isolated again.
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            isolatedColors = lineColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791 * 2, isolatedColors);

            //unisolate
            ViewModel.BackgroundPreviewViewModel.IsolationMode = false;
            DispatcherUtil.DoEvents();
            lineColors = lines.SelectMany(x => x.Geometry.Colors);
            blueLinesCount = lineColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowLinesCount = lineColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14897 * 2, blueLinesCount);
            Assert.AreEqual(14895 * 2, yellowLinesCount);

        }

        [Test]
        public void IsolatedANDFrozenColoredPtsBecomesTransparent()
        {
            OpenVisualizationTest("Display.ByGeometryColorPointsLines.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();
            DispatcherUtil.DoEvents();
            //get all points
            var points = BackgroundPreviewGeometry.OfType<DynamoPointGeometryModel3D>();
            var pointColors = points.SelectMany(x => x.Geometry.Colors);
            var bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            var yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);

            //before activating isolation mode - freeze the point.
            var pointDisplayNode = Model.CurrentWorkspace.Nodes.Where(x => x.GUID == Guid.Parse("9af882721af64514883ce7390fcfe523")).First();
            DynamoSelection.Instance.ClearSelection();
            pointDisplayNode.IsFrozen = true;
            DispatcherUtil.DoEvents();

            //assert frozen colors
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            var frozenColorsCount = pointColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(29791, frozenColorsCount);

            //additionally isolate.
            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();

            ViewModel.BackgroundPreviewViewModel.IsolationMode = true;
            DispatcherUtil.DoEvents();
            //everything should be isolated.
            //assert that point colors have low alpha
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            var isolatedColors = pointColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791, isolatedColors);

            //select points
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(pointDisplayNode);
            DispatcherUtil.DoEvents();
            //assert points are no longer isolated - but they should be frozen.
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            frozenColorsCount = pointColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(29791, frozenColorsCount);

            DynamoSelection.Instance.ClearSelection();
            DispatcherUtil.DoEvents();
            //isolated again.
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            //isolate sets Alpha to .3 currently
            isolatedColors = pointColors.Where(x => x.Alpha < .4f).Count();
            Assert.AreEqual(29791, isolatedColors);

            //unisolate
            ViewModel.BackgroundPreviewViewModel.IsolationMode = false;
            DispatcherUtil.DoEvents();
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            frozenColorsCount = pointColors.Where(x => x.Alpha == .5).Count();
            Assert.AreEqual(29791, frozenColorsCount);

            //back to normal.
            pointDisplayNode.IsFrozen = false;
            DispatcherUtil.DoEvents();
            pointColors = points.SelectMany(x => x.Geometry.Colors);
            bluePtsCount = pointColors.Where(x => x == Colors.Blue.ToColor4()).Count(); ;
            yellowPtsCount = pointColors.Where(x => x == new Color3(1, 1, 0).ToColor4()).Count();
            Assert.AreEqual(14896, bluePtsCount);
            Assert.AreEqual(14895, yellowPtsCount);
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
            Assert.False((mockManipulator.SceneNode.RenderCore as DynamoGeometryMeshCore).IsSpecialRenderPackageData);
            AttachedProperties.SetIsSpecialRenderPackage(mockManipulator, true);
            //assert that setting this attached property updated the meshcore data.
            Assert.IsTrue((mockManipulator.SceneNode.RenderCore as DynamoGeometryMeshCore).IsSpecialRenderPackageData);
          
        }
        #endregion

    }
}

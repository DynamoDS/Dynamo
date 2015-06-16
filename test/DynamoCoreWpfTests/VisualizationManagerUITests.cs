using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using SystemTestServices;
using Autodesk.DesignScript.Interfaces;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SharpDX;
using Color = System.Windows.Media.Color;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class VisualizationManagerUITests : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSIronPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private Watch3DView BackgroundPreview
        {
            get { return (Watch3DView)View.background_grid.FindName("background_preview"); }
        }

        [Test]
        public void VisualizationManager_EmptyGraph_NothingRenders()
        {
            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.False(ws.HasSomethingToRender());
        }

        [Test]
        public void VisualizationManager_NodePreviewToggled_RenderingUpToDate()
        {
            var model = ViewModel.Model;
            var viz = ViewModel.VisualizationManager;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points_line.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //we start with all previews disabled
            //the graph is two points feeding into a line

            Assert.AreEqual(7, ws.TotalPointsToRender());
            Assert.AreEqual(6, ws.TotalLinesToRender());
            Assert.AreEqual(0, ws.TotalMeshesToRender());

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "a7c70c13-cc62-41a6-85ed-dc42e788181d");
            p1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.AreEqual(1, ws.TotalPointsToRender());
            Assert.AreEqual(6, ws.TotalLinesToRender());
            Assert.AreEqual(0, ws.TotalMeshesToRender());

            //flip off the lines node
            var l1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.AreEqual(1, ws.TotalPointsToRender());
            Assert.AreEqual(0, ws.TotalLinesToRender());
            Assert.AreEqual(0, ws.TotalMeshesToRender());

            //flip those back on and ensure the visualization returns
            p1.UpdateValue(new UpdateValueParams("IsVisible", "true"));
            l1.UpdateValue(new UpdateValueParams("IsVisible", "true"));

            Assert.AreEqual(7, ws.TotalPointsToRender());
            Assert.AreEqual(6, ws.TotalLinesToRender());
            Assert.AreEqual(0, ws.TotalMeshesToRender());
        }

        [Test, Category("Failure")]
        public void VisualizationManager_PreviewUpstreamToggled_RenderingUpToDate()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points_line.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            Assert.AreEqual(7, ws.TotalPointsToRender());
            Assert.AreEqual(6, ws.TotalLinesToRender());
            Assert.AreEqual(0, ws.TotalMeshesToRender());

            // listen for the render complete event so
            // we can inspect the packages sent to the watch3d node
            var viz = ViewModel.VisualizationManager;
            viz.ResultsReadyToVisualize += viz_ResultsReadyToVisualize;

            //flip off the line node's preview upstream
            var l1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.UpdateValue(new UpdateValueParams("IsUpstreamVisible", "false"));

            viz.ResultsReadyToVisualize -= viz_ResultsReadyToVisualize;
        }

        void viz_ResultsReadyToVisualize(VisualizationEventArgs args)
        {
            if (args.Id == Guid.Empty)
            {
                return;
            }

            Assert.AreEqual(0, args.Packages.Sum(rp=>rp.PointVertexCount));
        }

        [Test]
        public void VisualizationManager_NodeDisconnected_NodeRendersAreCleared()
        {
            //test to ensure that when nodes are disconnected 
            //their associated geometry is removed
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points_line.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //ensure the correct representations

            //look at the data in the visualization manager
            //ensure that the number of Drawable nodes
            Assert.AreEqual(7, ws.TotalPointsToRender());
            Assert.AreEqual(6, ws.TotalLinesToRender());

            //delete a conector coming into the lines node
            var lineNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            var port = lineNode.InPorts.First();
            port.Connectors.First().Delete();

            //ensure that the visualization no longer contains
            //the renderables for the line node
            Assert.AreEqual(7, ws.TotalPointsToRender());
            Assert.AreEqual(0, ws.TotalLinesToRender());
        }

        [Test]
        public void VisualizationManager_Python_CreatesVisualizations()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_python.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(1000, ws.TotalPointsToRender());
            Assert.AreEqual(1000, ws.TotalMeshesToRender());
        }

        [Test]
        public void VisualizationManager_NodeRemoved_VisualizationsDeleted()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points.dyn");
            Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(6, ws.TotalPointsToRender());

            //delete a node and ensure that the renderables are cleaned up
            var pointNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "0b472626-e18f-404a-bec4-d84ad7f33011");
            var modelsToDelete = new List<ModelBase> { pointNode };
            model.DeleteModelInternal(modelsToDelete);

            ViewModel.HomeSpace.HasUnsavedChanges = false;

            Assert.AreEqual(0, ws.TotalPointsToRender());
        }

        [Test]
        public void VisualizationManager_WorkspaceCleared_RenderingCleared()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //ensure that we have some visualizations
            Assert.Greater(ws.TotalPointsToRender(), 0);

            //now clear the workspace
            model.ClearCurrentWorkspace();

            //ensure that we have no visualizations
            Assert.AreEqual(0, ws.TotalPointsToRender());
        }

        [Test]
        public void VisualizationManager_WorkspaceOpened_RenderingCleared()
        {
            var model = ViewModel.Model;
            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_points.dyn");
            Open(openPath);

            // Make sure the workspace is running automatically.
            // Flipping the mode here will cause it to run.
            var ws = model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            // Ensure we have some geometry
            Assert.Greater(ws.TotalPointsToRender(),0);

            // Open a new file. It doesn't matter if the new file
            // is saved in Manual or Automatic, the act of clearing
            // the home workspace should cause the visualization manager
            // to request all views to clear themselves.

            openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\visualization\ASM_thicken.dyn");
            Open(openPath);

            ws = model.CurrentWorkspace as HomeWorkspaceModel;

            Assert.AreEqual(0, ws.TotalPointsToRender());
        }

        [Test]
        public void VisualizationManager_CustomNodes_Render()
        {
            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(
                        GetTestDirectory(ExecutingDirectory),
                        @"core\visualization\Points.dyf"),
                    true,
                    out info));
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_customNode.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //ensure that we have some visualizations
            Assert.Greater(ws.TotalPointsToRender(), 0);
        }

        [Test]
        public void VisualizationManager_Open_RemembersPreviewSaveState()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points_line_noPreview.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            //all nodes are set to not preview in the file
            //ensure that we have no visualizations
            Assert.AreEqual(0, ws.TotalLinesToRender());
        }

        [Test]
        public void VisualizationManager_Labels_Render()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\Labels.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            //before we run the expression, confirm that all nodes
            //have label display set to false - the default
            Assert.IsTrue(ViewModel.HomeSpace.Nodes.All(x => x.DisplayLabels != true));

            var cbn =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "fdec3b9b-56ae-4d01-85c2-47b8425e3130") as
                    CodeBlockNodeModel;
            Assert.IsNotNull(cbn);

            var elementResolver = model.CurrentWorkspace.ElementResolver;
            cbn.SetCodeContent("Autodesk.Point.ByCoordinates(a<1>,a<1>,a<1>);", elementResolver);

            // run the expression
            Assert.AreEqual(4, ws.TotalPointsToRender());

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            cbn.DisplayLabels = true;
            Assert.AreEqual(4, ws.TotalTextObjectsToRender());

            cbn.SetCodeContent("Autodesk.Point.ByCoordinates(a<1>,a<2>,a<3>);", elementResolver);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
            Assert.AreEqual(64, ws.TotalPointsToRender());
            Assert.AreEqual(64, ws.TotalTextObjectsToRender());

            cbn.DisplayLabels = false;
            Assert.AreEqual(0, ws.TotalTextObjectsToRender());
        }

        [Test]
        public void VisualizationManager_LabelsOnCurves_Render()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_points_line.dyn");
            Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            //before we run the expression, confirm that all nodes
            //have label display set to false - the default
            Assert.IsTrue(ViewModel.HomeSpace.Nodes.All(x => x.DisplayLabels != true));

            // run the expression
            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(6, ws.TotalLinesToRender());

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            var crvNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            Assert.IsNotNull(crvNode);
            crvNode.DisplayLabels = true;

            Assert.AreEqual(6, ws.TotalTextObjectsToRender());
        }

        [Test]
        public void VisualizationManager_CustomNode_DoesNotRender()
        {
            // Regression test for defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5165
            // To verify when some geometry nodes are converted to custom node,
            // their render packages shouldn't be carried over to custom work
            // space.
            var model = ViewModel.Model;
            var examplePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\");
            Open(Path.Combine(examplePath, "visualize_line_incustom.dyn"));

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(1, ws.TotalLinesToRender());

            // Convert a DSFunction node Line.ByPointDirectionLength to custom node.
            var workspace = model.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            var customWorkspace = model.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__VisualizationTest__",
                    Success = true
                }) as CustomNodeWorkspaceModel;
            ViewModel.HomeSpace.Run();

            // Switch to custom workspace
            model.OpenCustomNodeWorkspace(customWorkspace.CustomNodeId);
            var customSpace = Model.CurrentWorkspace;

            // Select that node
            DynamoSelection.Instance.Selection.Add(node);

            // No preview in the background
            Assert.AreEqual(0, customSpace.TotalPointsToRender());
            Assert.AreEqual(0, customSpace.TotalLinesToRender());
            Assert.AreEqual(0, customSpace.TotalMeshesToRender());
        }

        [Test]
        public void VisualizationManager_Solids_Render()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_thicken.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.True(ws.HasAnyMeshes());

            ViewModel.HomeSpace.HasUnsavedChanges = false;
        }

        [Test]
        public void VisualizationManager_Surfaces_Render()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_cuboid.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(36, ws.TotalMeshVerticesToRender());
        }

        [Test]
        public void VisualizationManager_CoordinateSystems_Render()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_coordinateSystem.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(1f, 0f, 0f, 1f)));
            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(0f, 1f, 0f, 1f)));
            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(0f, 0f, 1f, 1f)));
        }

        [Test]
        public void VisualizationManager_Planes_Render()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\Planes.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            var numberOfPlanesNode = ws.Nodes.FirstOrDefault(n => n.NickName == "Number of Planes") as DoubleInput;
            Assert.NotNull(numberOfPlanesNode);

            var numberOfPlanes = int.Parse(numberOfPlanesNode.Value);
            var numberOfTrisPerPlane = 2;
            var numberOfVertsPerTri = 3;

            // 5 planes, each with two triangles:
            // 30 mesh vertices
            //ensure that the number of visualizations matches the 
            //number of pieces of geometry in the collection
            Assert.AreEqual(numberOfPlanes * numberOfVertsPerTri * numberOfTrisPerPlane, ws.TotalMeshVerticesToRender());

            var testColor = new Color4(0, 0, 0, 10.0f/255.0f);
            Assert.True(ws.HasMeshVerticesAllOfColor(testColor));

            // Increase the number of planes
            numberOfPlanes = numberOfPlanes + 5;
            numberOfPlanesNode.Value = numberOfPlanes.ToString();
            Assert.AreEqual(numberOfPlanes * numberOfVertsPerTri * numberOfTrisPerPlane, ws.TotalMeshVerticesToRender());
        }

        [Test]
        [Category("UnitTests")]
        public void Watch3D_Reopened_SizeRemainsTheSame()
        {
            var random = new Random();
            var original = new Watch3D();

            // Update the original node instance.
            var width = original.Width * (1.0 + random.NextDouble());
            var height = original.Height * (1.0 + random.NextDouble());
            original.SetSize(Math.Floor(width), Math.Floor(height));

            original.CameraPosition = new Point3D(10, 20, 30);
            original.LookDirection = new Vector3D(15, 25, 35);

            // Ensure the serialization survives through file, undo, and copy.
            var document = new XmlDocument();
            var fileElement = original.Serialize(document, SaveContext.File);
            var undoElement = original.Serialize(document, SaveContext.Undo);
            var copyElement = original.Serialize(document, SaveContext.Copy);

            // Duplicate the node in various save context.
            var nodeFromFile = new Watch3D();
            var nodeFromUndo = new Watch3D();
            var nodeFromCopy = new Watch3D();
            nodeFromFile.Deserialize(fileElement, SaveContext.File);
            nodeFromUndo.Deserialize(undoElement, SaveContext.Undo);
            nodeFromCopy.Deserialize(copyElement, SaveContext.Copy);

            // Making sure we have properties preserved through file operation.
            Assert.AreEqual(original.WatchWidth, nodeFromFile.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromFile.WatchHeight);
            Assert.AreEqual(original.CameraPosition.X, nodeFromFile.CameraPosition.X);
            Assert.AreEqual(original.CameraPosition.Y, nodeFromFile.CameraPosition.Y);
            Assert.AreEqual(original.CameraPosition.Z, nodeFromFile.CameraPosition.Z);
            Assert.AreEqual(original.LookDirection.X, nodeFromFile.LookDirection.X);
            Assert.AreEqual(original.LookDirection.Y, nodeFromFile.LookDirection.Y);
            Assert.AreEqual(original.LookDirection.Z, nodeFromFile.LookDirection.Z);

            // Making sure we have properties preserved through undo operation.
            Assert.AreEqual(original.WatchWidth, nodeFromUndo.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromUndo.WatchHeight);
            Assert.AreEqual(original.CameraPosition.X, nodeFromUndo.CameraPosition.X);
            Assert.AreEqual(original.CameraPosition.Y, nodeFromUndo.CameraPosition.Y);
            Assert.AreEqual(original.CameraPosition.Z, nodeFromUndo.CameraPosition.Z);
            Assert.AreEqual(original.LookDirection.X, nodeFromUndo.LookDirection.X);
            Assert.AreEqual(original.LookDirection.Y, nodeFromUndo.LookDirection.Y);
            Assert.AreEqual(original.LookDirection.Z, nodeFromUndo.LookDirection.Z);

            // Making sure we have properties preserved through copy operation.
            Assert.AreEqual(original.WatchWidth, nodeFromCopy.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromCopy.WatchHeight);
            Assert.AreEqual(original.CameraPosition.X, nodeFromCopy.CameraPosition.X);
            Assert.AreEqual(original.CameraPosition.Y, nodeFromCopy.CameraPosition.Y);
            Assert.AreEqual(original.CameraPosition.Z, nodeFromCopy.CameraPosition.Z);
            Assert.AreEqual(original.LookDirection.X, nodeFromCopy.LookDirection.X);
            Assert.AreEqual(original.LookDirection.Y, nodeFromCopy.LookDirection.Y);
            Assert.AreEqual(original.LookDirection.Z, nodeFromCopy.LookDirection.Z);
        }

        [Test]
        public void Watch3D_SwitchWorkspaceType_BackgroundColorIsCorrect()
        {
            CustomNodeInfo info;

            var customNodePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\Points.dyf");

            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    customNodePath,
                    true,
                    out info));

            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_customNode.dyn");

            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            var homeColor = (Color) SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundHome"];

            Assert.AreEqual(BackgroundPreview.watch_view.BackgroundColor, homeColor.ToColor4());

            Open(customNodePath);

            var customColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundCustom"];

            Assert.AreEqual(BackgroundPreview.watch_view.BackgroundColor, customColor.ToColor4());
        }

        [Test]
        public void ViewSettings_ShowEdges_Toggled_GeometryIsCorrect()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\visualization\ASM_cuboid.dyn");
            Open(openPath);

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, ws.TotalLinesToRender());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = true;
            Assert.AreEqual(12, ws.TotalLinesToRender());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, ws.TotalLinesToRender());
        }

        private void Open(string relativePath)
        {
            OpenDynamoDefinition(relativePath);
            DispatcherUtil.DoEvents();
        }
    }

    internal static class WorkspaceExtensions
    {
        public static int TotalLinesOfColorToRender(this WorkspaceModel workspace, Color4 color)
        {
            var colorLineCount = workspace.Nodes.
                SelectMany(n=>n.RenderPackages).
                Where(rp=>rp.LineVertexCount > 0).
                Sum(rp => rp.TotalCurvesOfColor(color));

            return colorLineCount;
        }

        public static int TotalPointsToRender(this WorkspaceModel workspace)
        {
            var vertexCount = workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Sum(rp => rp.PointVertexCount);

            return vertexCount;
        }

        public static int TotalTextObjectsToRender(this WorkspaceModel workspace)
        {
            var labelledPackages = workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Where(rp=>rp.DisplayLabels).ToArray();

            int total = 0;

            total += labelledPackages.
                Where(rp=>rp.PointVertexCount > 0).
                Sum(rp => rp.PointVertexCount);

            total += labelledPackages.
                Where(rp=>rp.LineVertexCount > 0).
                Sum(rp => rp.LineStripVertexCounts.Count(c => c >0));

            total += labelledPackages.Count(rp => rp.MeshVertexCount > 0);

            return total;
        }

        public static int TotalMeshesToRender(this WorkspaceModel workspace)
        {
            return workspace.Nodes.SelectMany(n => n.RenderPackages).Count(rp => rp.MeshVertexCount > 0);
        }

        public static bool HasAnyMeshes(this WorkspaceModel workspace)
        {
            return workspace.Nodes.SelectMany(n => n.RenderPackages).Any(rp => rp.MeshVertexCount > 0);
        }

        public static bool HasMeshVerticesAllOfColor(this WorkspaceModel workspace, Color4 color)
        {
            return workspace.Nodes.SelectMany(n => n.RenderPackages)
                .All(rp => rp.MeshVertexColors.IsArrayOfColor(color));
        }

        public static int TotalMeshVerticesToRender(this WorkspaceModel workspace)
        {
            return workspace.Nodes.SelectMany(n => n.RenderPackages).Sum(rp => rp.MeshVertexCount);
        }

        public static int TotalLinesToRender(this WorkspaceModel workspace)
        {
            return workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Where(rp=>rp.LineStripVertexCounts.Any()).
                Sum(rp => rp.LineStripVertexCounts.Count(c => c > 0));
        }

        public static bool HasSomethingToRender(this WorkspaceModel workspace)
        {
            var allPackages = workspace.Nodes.
                SelectMany(n => n.RenderPackages);

            return allPackages.Any();
        }

        private static int TotalCurvesOfColor(this IRenderPackage package, Color4 color)
        {
            var count = 0;
            var idx = 0;
            for (var i = 0; i < package.LineStripVertexCounts.Count(); i++)
            {
                var currCount = package.LineStripVertexCounts.ElementAt(i);
                if (currCount == 0) continue;

                var colorBytes = package.LineStripVertexColors.Skip(idx).Take(currCount*4);
                if (colorBytes.IsArrayOfColor(color))
                {
                    count++;
                }
                idx += currCount*4;
            }

            return count;
        }

        private static bool IsArrayOfColor(this IEnumerable<byte> colorBytes, Color4 color)
        {
            var colorArr = colorBytes.ToArray();

            for (var i = 0; i < colorArr.Count(); i += 4)
            {
                var r = colorArr[i] == (byte)(color.Red * 255);
                var g = colorArr[i+1] == (byte)(color.Green * 255);
                var b = colorArr[i+2] == (byte)(color.Blue * 255);
                var a = colorArr[i+3] == (byte)(color.Alpha * 255);
                if (!a || !r || !g || !b)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;
using SystemTestServices;
using Autodesk.DesignScript.Interfaces;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Tests;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoCoreWpfTests.Utility;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SharpDX;
using Color = System.Windows.Media.Color;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class VisualizationManagerUITests : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("Display.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private Dictionary<string, Model3D> Geometry
        {
            get { return ViewModel.BackgroundPreviewViewModel.Model3DDictionary; }
        } 

        private Watch3DView BackgroundPreview
        {
            get { return (Watch3DView)View.background_grid.FindName("background_preview"); }
        }

        #region node tests

        [Test]
        public void Node_RenderingUpToDate()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "a7c70c13-cc62-41a6-85ed-dc42e788181d");
            p1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(Geometry.NumberOfInvisiblePoints(), 1);

            //flip off the lines node
            var l1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(Geometry.NumberOfInvisibleCurves(), 1);

            //flip those back on and ensure the visualization returns
            p1.UpdateValue(new UpdateValueParams("IsVisible", "true"));
            l1.UpdateValue(new UpdateValueParams("IsVisible", "true"));

            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(Geometry.NumberOfInvisibleCurves(), 0);
        }

        [Test]
        public void Node_PreviewUpstreamToggled_RenderingUpToDate()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            //we start with all previews disabled
            //the graph is two points feeding into a line

            //ensure that visulations match our expectations
            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7,6,0));

            // listen for the render complete event so
            // we can inspect the packages sent to the watch3d node
            var viz = ViewModel.VisualizationManager;
            viz.ResultsReadyToVisualize += viz_ResultsReadyToVisualize;

            //flip off the line node's preview upstream
            var l1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.UpdateValue(new UpdateValueParams("IsUpstreamVisible", "false"));

            viz.ResultsReadyToVisualize -= viz_ResultsReadyToVisualize;
        }

        [Test]
        public void Node_InputDisconnected_NodeRendersAreCleared()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            Assert.True(Geometry.HasNumberOfPointsCurvesAndMeshes(7,6,0));

            var lineNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            var port = lineNode.InPorts.First();
            port.Connectors.First().Delete();

            Assert.AreEqual(Geometry.TotalCurves(), 0);
        }

        [Test]
        public void Node_Removed_VisualizationsDeleted()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points.dyn");

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());

            Assert.AreEqual(Geometry.TotalPoints(), 6);

            var pointNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "0b472626-e18f-404a-bec4-d84ad7f33011");
            var modelsToDelete = new List<ModelBase> { pointNode };
            model.DeleteModelInternal(modelsToDelete);

            ViewModel.HomeSpace.HasUnsavedChanges = false;

            Assert.AreEqual(Geometry.TotalPoints(), 0);
        }

        [Test]
        public void Node_Labels_Render()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("Labels.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count());

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

            Assert.AreEqual(4, Geometry.TotalPoints());

            cbn.DisplayLabels = true;
            Assert.AreEqual(4, Geometry.TotalText());

            cbn.SetCodeContent("Autodesk.Point.ByCoordinates(a<1>,a<2>,a<3>);", elementResolver);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            Assert.AreEqual(64, Geometry.TotalPoints());
            Assert.AreEqual(64, Geometry.TotalText());

            cbn.DisplayLabels = false;

            Assert.AreEqual(0, Geometry.TotalText());
        }

        [Test]
        public void Node_LabelsOnCurves_Render()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count());

            //before we run the expression, confirm that all nodes
            //have label display set to false - the default
            Assert.IsTrue(ViewModel.HomeSpace.Nodes.All(x => x.DisplayLabels != true));

            // run the expression
            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            ws.RunSettings.RunType = RunType.Automatic;

            Assert.AreEqual(6, Geometry.TotalCurves());

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            var crvNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            Assert.IsNotNull(crvNode);
            crvNode.DisplayLabels = true;

            Assert.AreEqual(6, Geometry.TotalText());
        }

        [Test]
        public void Node_MixedGeometryOutput_Deleted_AllGeometryGone()
        {
            Assert.Inconclusive("Finish me!");
        }

        #endregion

        #region workspace tests

        [Test]
        public void Workspace_Empty_NothingRenders()
        {
            Assert.True(Geometry.HasNoUserCreatedModel3DObjects());
        }

        [Test]
        public void Workspace_Cleared_RenderingCleared()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points.dyn");

            //ensure that we have some visualizations
            Assert.Greater(Geometry.TotalPoints(), 0);

            //now clear the workspace
            model.ClearCurrentWorkspace();

            //ensure that we have no visualizations
            Assert.AreEqual(Geometry.TotalPoints(), 0);
        }

        [Test]
        public void Workspace_Opened_RenderingCleared()
        {
            OpenVisualizationTest("ASM_points.dyn");

            // Ensure we have some geometry
            Assert.Greater(Geometry.TotalPoints(), 0);

            // Open an empty file. This will test whether opening a
            // workspace causes any geometry to be left behind.

            OpenVisualizationTest("empty.dyn");

            Assert.AreEqual(Geometry.TotalPoints(), 0);
        }

        [Test]
        public void Workspace_Open_RemembersPreviewSaveState()
        {
            OpenVisualizationTest("ASM_points_line_noPreview.dyn");

            //all nodes are set to not preview in the file
            //ensure that we have no visualizations
            Assert.AreEqual(Geometry.TotalCurves(), 0);
        }

        #endregion

        #region geometry tests

        [Test]
        public void Geometry_Solids_Render()
        {
            OpenVisualizationTest("ASM_thicken.dyn");

            Assert.True(Geometry.HasAnyMeshes());

            ViewModel.HomeSpace.HasUnsavedChanges = false;
        }

        [Test]
        public void Geometry_Surfaces_Render()
        {
            OpenVisualizationTest("ASM_cuboid.dyn");

            Assert.AreEqual(Geometry.TotalMeshVerticesToRender(), 36);
        }

        [Test]
        public void Geometry_CoordinateSystems_Render()
        {
            OpenVisualizationTest("ASM_coordinateSystem.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(1f, 0f, 0f, 1f)));
            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(0f, 1f, 0f, 1f)));
            Assert.AreEqual(1, ws.TotalLinesOfColorToRender(new Color4(0f, 0f, 1f, 1f)));
        }

        [Test]
        public void Geometry_Planes_Render()
        {
            OpenVisualizationTest("Planes.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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

            var testColor = new Color4(0, 0, 0, 10.0f / 255.0f);
            Assert.True(ws.HasMeshVerticesAllOfColor(testColor));

            // Increase the number of planes
            numberOfPlanes = numberOfPlanes + 5;
            numberOfPlanesNode.Value = numberOfPlanes.ToString();
            Assert.AreEqual(numberOfPlanes * numberOfVertsPerTri * numberOfTrisPerPlane, ws.TotalMeshVerticesToRender());
        }

        #endregion

        #region custom node tests

        [Test]
        public void CustomNodes_Render()
        {
            CustomNodeInfo info;
            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(
                        GetTestDirectory(ExecutingDirectory),
                        @"core\visualization\Points.dyf"),
                    true,
                    out info));

            OpenVisualizationTest("ASM_customNode.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            //ensure that we have some visualizations
            Assert.Greater(ws.TotalPointsToRender(), 0);
        }

        [Test]
        public void CustomNode_DoesNotRender()
        {
            // Regression test for defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5165
            // To verify when some geometry nodes are converted to custom node,
            // their render packages shouldn't be carried over to custom work
            // space.
            var model = ViewModel.Model;

            OpenVisualizationTest("visualize_line_incustom.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

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
            var customSpace = ViewModel.Model.CurrentWorkspace;

            // Select that node
            DynamoSelection.Instance.Selection.Add(node);

            // No preview in the background
            Assert.AreEqual(0, customSpace.TotalPointsToRender());
            Assert.AreEqual(0, customSpace.TotalLinesToRender());
            Assert.AreEqual(0, customSpace.TotalMeshesToRender());
        }

        #endregion

        #region watch 3d tests

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

            OpenVisualizationTest("ASM_customNode.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            var homeColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundHome"];

            Assert.AreEqual(BackgroundPreview.watch_view.BackgroundColor, homeColor.ToColor4());

            OpenVisualizationTest("Points.dyf");

            var customColor = (Color)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["WorkspaceBackgroundCustom"];

            Assert.AreEqual(BackgroundPreview.watch_view.BackgroundColor, customColor.ToColor4());
        }

        [Test]
        public void Watch3D_Disconnect_Reconnect_CorrectRenderings()
        {
            OpenVisualizationTest("ASM_points_line.dyn");

            // Clear the dispatcher to ensure that the 
            // view is created.
            DispatcherUtil.DoEvents();

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            var watch3DNode = ws.FirstNodeFromWorkspace<Watch3D>();
            var totalPointsInView = ViewModel.BackgroundPreviewViewModel.Model3DDictionary.TotalPoints();
            Assert.Greater(totalPointsInView, 3);

            // Disconnect the port coming into the watch3d node.
            var connector = watch3DNode.InPorts[0].Connectors.First();
            watch3DNode.InPorts[0].Disconnect(connector);

            // Three items, the grid, the axes, and the light will remain.
            Assert.AreEqual(watch3DNode.View.View.Items.Count, 3);

            var linesNode = ws.Nodes.First(n => n.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");

            var cmd1 = new DynamoModel.MakeConnectionCommand(linesNode.GUID.ToString(), 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin);
            var cmd2 = new DynamoModel.MakeConnectionCommand(watch3DNode.GUID.ToString(), 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End);

            ViewModel.Model.ExecuteCommand(cmd1);
            ViewModel.Model.ExecuteCommand(cmd2);

            Assert.AreEqual(6, watch3DNode.View.View.Items.Count);
        }

        #endregion

        #region dynamo view tests

        [Test]
        public void ViewSettings_ShowEdges_Toggled_GeometryIsCorrect()
        {
            OpenVisualizationTest("ASM_cuboid.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, ws.TotalLinesToRender());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = true;
            Assert.AreEqual(12, ws.TotalLinesToRender());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, ws.TotalLinesToRender());
        }

        #endregion

        [Test]
        public void Python_CreatesVisualizations()
        {
            OpenVisualizationTest("ASM_python.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(1000, ws.TotalPointsToRender());
            Assert.AreEqual(1000, ws.TotalMeshesToRender());
        }

        [Test]
        public void Display_ByGeometryColor_HasColoredMesh()
        {
            OpenVisualizationTest("Display.ByGeometryColor.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();

            Assert.True(ws.HasAnyVertexColoredMeshesOfColor(new Color4(new Color3(1.0f,0,1.0f))));
        }

        [Test]
        public void Display_BySurfaceColors_HasColoredMesh()
        {
            OpenVisualizationTest("Display.BySurfaceColors.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();

            Assert.True(ws.HasAnyColorMappedMeshes());
        }

        private void viz_ResultsReadyToVisualize(VisualizationEventArgs args)
        {
            if (args.Id == Guid.Empty)
            {
                return;
            }

            Assert.AreEqual(Geometry.TotalPoints(), 0);
        }

        private void OpenVisualizationTest(string fileName)
        {
            string relativePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                string.Format(@"core\visualization\{0}",fileName));

            if (!File.Exists(relativePath))
            {
                throw new FileNotFoundException("The specified .dyn file could not be found.");
            }

            ViewModel.OpenCommand.Execute(relativePath);
        }

        private static void CompareRenderPackagesAndModel3DCounts(HomeWorkspaceModel ws, Dictionary<string, Model3D> geometryDictionary)
        {
            Assert.AreEqual(ws.TotalPointsToRender(), geometryDictionary.TotalPoints());
            Assert.AreEqual(ws.TotalLinesToRender(), geometryDictionary.TotalCurves());
            Assert.AreEqual(ws.TotalMeshesToRender(), geometryDictionary.TotalMeshes());
        }
    }

    internal static class GeometryDictionaryExtensions
    {
        private static List<string> keyList = new List<string>()
            {
                Watch3DViewModel.axesKey,
                Watch3DViewModel.gridKey,
                Watch3DViewModel.directionalLightKey
            };

        public static int TotalPoints(this Dictionary<string, Model3D> dictionary)
        {
            var points = dictionary.Where(kvp => kvp.Value is PointGeometryModel3D && !keyList.Contains(kvp.Key)).ToArray();

            return points.Any()
                ? points.SelectMany(p => ((PointGeometryModel3D)p.Value).Geometry.Positions).Count()
                : 0;
        }

        public static int TotalMeshes(this Dictionary<string, Model3D> dictionary)
        {
            return dictionary.Count(kvp => kvp.Value is DynamoGeometryModel3D && !keyList.Contains(kvp.Key));
        }

        public static int TotalCurves(this Dictionary<string, Model3D> dictionary)
        {
            var lines = dictionary.Where(kvp => kvp.Value is LineGeometryModel3D && ! keyList.Contains(kvp.Key)).ToArray();

            return lines.Any()
                ? lines.SelectMany(p => ((LineGeometryModel3D)p.Value).Geometry.Positions).Count()/2
                : 0;
        }

        public static int TotalText(this Dictionary<string, Model3D> dictionary)
        {
            var text = dictionary
                .Where(kvp => kvp.Value is BillboardTextModel3D && !keyList.Contains(kvp.Key))
                .Select(kvp=>kvp.Value).Cast<BillboardTextModel3D>()
                .Select(bb=>bb.Geometry).Cast<BillboardText3D>()
                .ToArray();

            return text.Any()
                ? text.Where(t=>t != null).SelectMany(t=>t.TextInfo).Count()
                : 0;
        }

        public static bool HasNumberOfPointsCurvesAndMeshes(this Dictionary<string, Model3D> dictionary, int numberOfPoints, int numberOfCurves, int numberOfMeshes)
        {
            return dictionary.TotalPoints() == numberOfPoints &&
                   dictionary.TotalCurves() == numberOfCurves &&
                   dictionary.TotalMeshes() == numberOfMeshes;
        }

        public static int NumberOfInvisiblePoints(this Dictionary<string, Model3D> dictionary)
        {
            var geoms = dictionary.Where(kvp => kvp.Value is PointGeometryModel3D).ToArray();
            return geoms.Any()? geoms.Count(kvp => kvp.Value.Visibility == Visibility.Hidden) : 0;
        }

        public static int NumberOfInvisibleCurves(this Dictionary<string, Model3D> dictionary)
        {
            var geoms = dictionary.Where(kvp => kvp.Value is LineGeometryModel3D).ToArray();
            return geoms.Any() ? geoms.Count(kvp => kvp.Value.Visibility == Visibility.Hidden) : 0;
        }

        public static int NumberOfInvisibleMeshes(this Dictionary<string, Model3D> dictionary)
        {
            var geoms = dictionary.Where(kvp => kvp.Value is DynamoGeometryModel3D).ToArray();
            return geoms.Any() ? geoms.Count(kvp => kvp.Value.Visibility == Visibility.Hidden) : 0;
        }

        public static bool HasVisibleObjects(this Dictionary<string, Model3D> dictionary)
        {
            return dictionary.Any(kvp => kvp.Value.Visibility == Visibility.Visible);
        }

        public static bool HasNoUserCreatedModel3DObjects(this Dictionary<string, Model3D> dictionary)
        {
            return dictionary.All(kvp => keyList.Contains(kvp.Key));
        }

        public static bool HasAnyMeshes(this Dictionary<string, Model3D> dictionary)
        {
            return dictionary.TotalMeshes() > 0;
        }

        public static int TotalMeshVerticesToRender(this Dictionary<string, Model3D> dictionary)
        {
            var geoms = dictionary.Where(kvp => kvp.Value is DynamoGeometryModel3D && !keyList.Contains(kvp.Key))
                .Select(kvp=>kvp.Value).Cast<DynamoGeometryModel3D>();

            return geoms.Any()? geoms.SelectMany(g=>g.Geometry.Positions).Count() : 0;
        }
    }

    internal static class WorkspaceExtensions
    {
        public static bool HasAnyVertexColoredMeshesOfColor(this WorkspaceModel workspace, Color4 color)
        {
            var colorMeshCount = workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Where(rp => rp.RequiresPerVertexColoration).
                Where(rp => rp.MeshVertexColors.IsArrayOfColor(color));

            return colorMeshCount.Any();
        }

        public static bool HasAnyColorMappedMeshes(this WorkspaceModel workspace)
        {
            var colorMeshCount = workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Where(rp => rp.Colors != null);

            return colorMeshCount.Any();
        }

        public static int TotalLinesOfColorToRender(this WorkspaceModel workspace, Color4 color)
        {
            var colorLineCount = workspace.Nodes.
                SelectMany(n => n.RenderPackages).
                Where(rp => rp.LineVertexCount > 0).
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
                Where(rp => rp.DisplayLabels).ToArray();

            int total = 0;

            total += labelledPackages.
                Where(rp => rp.PointVertexCount > 0).
                Sum(rp => rp.PointVertexCount);

            total += labelledPackages.
                Where(rp => rp.LineVertexCount > 0).
                Sum(rp => rp.LineStripVertexCounts.Count(c => c > 0));

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
                Where(rp => rp.LineStripVertexCounts.Any()).
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

                var colorBytes = package.LineStripVertexColors.Skip(idx).Take(currCount * 4);
                if (colorBytes.IsArrayOfColor(color))
                {
                    count++;
                }
                idx += currCount * 4;
            }

            return count;
        }

        private static bool IsArrayOfColor(this IEnumerable<byte> colorBytes, Color4 color)
        {
            var colorArr = colorBytes.ToArray();

            for (var i = 0; i < colorArr.Count(); i += 4)
            {
                var r = colorArr[i] == (byte)(color.Red * 255);
                var g = colorArr[i + 1] == (byte)(color.Green * 255);
                var b = colorArr[i + 2] == (byte)(color.Blue * 255);
                var a = colorArr[i + 3] == (byte)(color.Alpha * 255);
                if (!a || !r || !g || !b)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

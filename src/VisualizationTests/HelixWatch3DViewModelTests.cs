using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Xml;
using CoreNodeModels.Input;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Tests;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoCoreWpfTests.Utility;
using DynamoShapeManager;
using HelixToolkit.Wpf.SharpDX;
using NUnit.Framework;
using SharpDX;
using SystemTestServices;
using TestServices;
using Watch3DNodeModels;
using Watch3DNodeModelsWpf;
using Color = System.Windows.Media.Color;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;

namespace WpfVisualizationTests
{
    /// <summary>
    /// The standard SystemTestBase uses a DefaultWatch3DViewModel.
    /// In order to test visualizations, the VisualizationTest class,
    /// uses a HelixWatch3DViewModel supplied as part of the 
    /// DynamoViewModel's start configuration.
    /// </summary>
    public class VisualizationTest : SystemTestBase
    {
        protected IEnumerable<Model3D> BackgroundPreviewGeometry
        {
            get { return ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).SceneItems; }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            libraries.Add("VMDataBridge.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override void StartDynamo(TestSessionConfiguration testConfig)
        {
            // Add Dynamo Core location to the PATH system environment variable.
            // This is to make sure dependencies(e.g.Helix assemblies) can be located.
            var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process) + ";" + testConfig.DynamoCorePath;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);

            var preloader = new Preloader(testConfig.DynamoCorePath, new[] { testConfig.RequestedLibraryVersion2 });
            preloader.Preload();

            var preloadedLibraries = new List<string>();
            GetLibrariesToPreload(preloadedLibraries);

            if (preloadedLibraries.Any())
            {
                if (pathResolver == null)
                    pathResolver = new TestPathResolver();

                var pr = pathResolver as TestPathResolver;
                foreach (var preloadedLibrary in preloadedLibraries.Distinct())
                {
                    pr.AddPreloadLibraryPath(preloadedLibrary);
                }
            }

            Model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    PathResolver = pathResolver,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    UpdateManager = this.UpdateManager,
                    ProcessMode = TaskProcessMode.Synchronous
                });

            Model.EvaluationCompleted += Model_EvaluationCompleted;

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model,
                    Watch3DViewModel = 
                        HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                            null,
                            new Watch3DViewModelStartupParams(Model), 
                            Model.Logger)
                });

            //create the view
            View = new DynamoView(ViewModel);
            View.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        private async void Model_EvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            DispatcherUtil.DoEvents();
        }

        protected void OpenVisualizationTest(string fileName)
        {
            string relativePath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                string.Format(@"core\visualization\{0}", fileName));

            if (!File.Exists(relativePath))
            {
                throw new FileNotFoundException("The specified .dyn file could not be found.");
            }

            ViewModel.OpenCommand.Execute(relativePath);
        }

        // With version 2.5 NUnit will call base class TearDown methods after those in the derived classes
        [TearDown]
        private void CleanUp()
        {
            Model.EvaluationCompleted -= Model_EvaluationCompleted;
        }
    }

    [TestFixture]
    public class HelixWatch3DViewModelTests : VisualizationTest
    {
        protected Watch3DView BackgroundPreview
        {
            get
            {
                return (Watch3DView) View.background_grid.Children().
                    FirstOrDefault(c => c is Watch3DView);
            }
        }

        #region node tests

        [Test]
        public void Node_RenderingUpToDate()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "a7c70c13-cc62-41a6-85ed-dc42e788181d");
            p1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(1, BackgroundPreviewGeometry.NumberOfInvisiblePoints());

            //flip off the lines node
            var l1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            l1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(1, BackgroundPreviewGeometry.NumberOfInvisibleCurves());

            //flip those back on and ensure the visualization returns
            p1.UpdateValue(new UpdateValueParams("IsVisible", "true"));
            l1.UpdateValue(new UpdateValueParams("IsVisible", "true"));

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(BackgroundPreviewGeometry.NumberOfInvisibleCurves(), 0);
        }

        [Test]
        public void Node_PreviewToggled_RenderingUpToDate()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            // Verify that visualizations match our expectations
            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));

            var watch3D = Model.CurrentWorkspace.FirstNodeFromWorkspace<Watch3D>();
            Assert.NotNull(watch3D);

            var view = FindFirstWatch3DNodeView();
            var vm = view.ViewModel as HelixWatch3DNodeViewModel;
            Assert.NotNull(vm);
   
            Assert.True(vm.SceneItems.HasNumberOfPointsCurvesAndMeshes(0,6,0));
        }

        [Test]
        public void Node_InputDisconnected_NodeRendersAreCleared()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7,6,0));

            var lineNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            var port = lineNode.InPorts.First();
            port.Connectors.First().Delete();

            Assert.AreEqual(BackgroundPreviewGeometry.TotalCurves(), 0);
        }

        [Test]
        public void Node_Removed_VisualizationsDeleted()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points.dyn");

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());

            Assert.AreEqual(BackgroundPreviewGeometry.TotalPoints(), 6);

            var pointNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "0b472626-e18f-404a-bec4-d84ad7f33011");
            var modelsToDelete = new List<ModelBase> { pointNode };
            model.DeleteModelInternal(modelsToDelete);

            ViewModel.HomeSpace.HasUnsavedChanges = false;

            Assert.AreEqual(BackgroundPreviewGeometry.TotalPoints(), 0);
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

            Assert.AreEqual(4, BackgroundPreviewGeometry.TotalPoints());

            cbn.DisplayLabels = true;
            Assert.AreEqual(4, BackgroundPreviewGeometry.TotalText());

            cbn.SetCodeContent("Autodesk.Point.ByCoordinates(a<1>,a<2>,a<3>);", elementResolver);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            Assert.AreEqual(64, BackgroundPreviewGeometry.TotalPoints());
            Assert.AreEqual(64, BackgroundPreviewGeometry.TotalText());

            cbn.DisplayLabels = false;

            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalText());
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

            Assert.AreEqual(6, BackgroundPreviewGeometry.TotalCurves());

            //label displayed should be possible now because
            //some nodes have values. toggle on label display
            var crvNode =
                model.CurrentWorkspace.Nodes.FirstOrDefault(
                    x => x.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");
            Assert.IsNotNull(crvNode);
            crvNode.DisplayLabels = true;

            Assert.AreEqual(6, BackgroundPreviewGeometry.TotalText());
        }

        [Test]
        public void Node_VisibilityOff_CachedValueUpdated_VisibilityOn_RenderDataCorrect()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points_line.dyn");

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));

            //now flip off the preview on one of the points
            //and ensure that the visualization updates without re-running
            var p1 = model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "a7c70c13-cc62-41a6-85ed-dc42e788181d");
            p1.UpdateValue(new UpdateValueParams("IsVisible", "false"));

            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(7, 6, 0));
            Assert.AreEqual(1, BackgroundPreviewGeometry.NumberOfInvisiblePoints());

            // Now change the number of points
            var cbn =
                Model.CurrentWorkspace.Nodes.First(x => x.GUID.ToString() == "cbc582bf-1040-4184-9b28-2d8d5419e411") as CodeBlockNodeModel;
            Assert.NotNull(cbn);
            cbn.SetCodeContent("0..2", Model.CurrentWorkspace.ElementResolver);

            // Flip the point visibility back on and ensure the visualization returns
            p1.UpdateValue(new UpdateValueParams("IsVisible", "true"));

            // Ensure that the new visualization matches the updated values.
            Assert.True(BackgroundPreviewGeometry.HasNumberOfPointsCurvesAndMeshes(4, 3, 0));
            Assert.AreEqual(BackgroundPreviewGeometry.NumberOfInvisiblePoints(), 0);
        }

        #endregion

        #region workspace tests

        [Test]
        public void Workspace_Empty_NothingRenders()
        {
            Assert.True(BackgroundPreviewGeometry.HasNoUserCreatedModel3DObjects());
        }

        [Test]
        public void Workspace_Cleared_RenderingCleared()
        {
            var model = ViewModel.Model;

            OpenVisualizationTest("ASM_points.dyn");

            //ensure that we have some visualizations
            Assert.Greater(BackgroundPreviewGeometry.TotalPoints(), 0);

            //now clear the workspace
            model.ClearCurrentWorkspace();

            //ensure that we have no visualizations
            Assert.AreEqual(BackgroundPreviewGeometry.TotalPoints(), 0);
        }

        [Test]
        public void Workspace_Opened_RenderingCleared()
        {
            OpenVisualizationTest("ASM_points.dyn");

            // Ensure we have some geometry
            Assert.Greater(BackgroundPreviewGeometry.TotalPoints(), 0);

            // Open an empty file. This will test whether opening a
            // workspace causes any geometry to be left behind.

            OpenVisualizationTest("empty.dyn");

            Assert.AreEqual(BackgroundPreviewGeometry.TotalPoints(), 0);
        }

        [Test]
        public void Workspace_Open_RemembersPreviewSaveState()
        {
            OpenVisualizationTest("ASM_points_line_noPreview.dyn");

            //all nodes are set to not preview in the file
            //ensure that we have no visualizations
            Assert.AreEqual(BackgroundPreviewGeometry.TotalCurves(), 0);
        }

        #endregion

        #region geometry tests

        [Test]
        public void Geometry_Solids_Render()
        {
            OpenVisualizationTest("ASM_thicken.dyn");

            Assert.True(BackgroundPreviewGeometry.HasAnyMeshes());

            ViewModel.HomeSpace.HasUnsavedChanges = false;
        }

        [Test]
        public void Geometry_Surfaces_Render()
        {
            OpenVisualizationTest("ASM_cuboid.dyn");

            Assert.AreEqual(BackgroundPreviewGeometry.TotalMeshVerticesToRender(), 36);
        }

        [Test]
        public void Geometry_CoordinateSystems_Render()
        {
            OpenVisualizationTest("ASM_coordinateSystem.dyn");

            Assert.AreEqual(2, BackgroundPreviewGeometry.TotalCurveVerticesOfColor(new Color4(1f, 0f, 0f, 1f)));
            Assert.AreEqual(2, BackgroundPreviewGeometry.TotalCurveVerticesOfColor(new Color4(0f, 1f, 0f, 1f)));
            Assert.AreEqual(2, BackgroundPreviewGeometry.TotalCurveVerticesOfColor(new Color4(0f, 0f, 1f, 1f)));
        }

        [Test]
        public void Geometry_Planes_Render()
        {
            OpenVisualizationTest("Planes.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            var numberOfPlanesNode = ws.Nodes.FirstOrDefault(n => n.Name == "Number of Planes") as DoubleInput;
            Assert.NotNull(numberOfPlanesNode);

            var numberOfPlanes = int.Parse(numberOfPlanesNode.Value);
            var numberOfTrisPerPlane = 2;
            var numberOfVertsPerTri = 3;

            // 5 planes, each with two triangles:
            // 30 mesh vertices
            //ensure that the number of visualizations matches the 
            //number of pieces of geometry in the collection
            Assert.AreEqual(numberOfPlanes * numberOfVertsPerTri * numberOfTrisPerPlane, 
                BackgroundPreviewGeometry.TotalMeshVerticesToRender());

            var testColor = new Color4(0, 0, 0, 10.0f / 255.0f);
            Assert.True(BackgroundPreviewGeometry.HasMeshVerticesAllOfColor(testColor));

            // Increase the number of planes
            numberOfPlanes = numberOfPlanes + 5;
            numberOfPlanesNode.Value = numberOfPlanes.ToString();
            Assert.AreEqual(numberOfPlanes * numberOfVertsPerTri * numberOfTrisPerPlane, 
                BackgroundPreviewGeometry.TotalMeshVerticesToRender());
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

            var vmParams = new Watch3DViewModelStartupParams(ViewModel.Model);
            var vm1 = new HelixWatch3DNodeViewModel(original, vmParams);
            var cam = vm1.Camera;

            cam.Position = new Point3D(10, 20, 30);
            cam.LookDirection = new Vector3D(15, 25, 35);

            // Ensure the serialization survives through file, undo, and copy.
            var document = new XmlDocument();
            var fileElement = original.Serialize(document, SaveContext.File);
            var undoElement = original.Serialize(document, SaveContext.Undo);
            var copyElement = original.Serialize(document, SaveContext.Copy);

            // Duplicate the node in various save context.
            var nodeFromFile = new Watch3D();
            var vmFile = new HelixWatch3DNodeViewModel(nodeFromFile, vmParams);

            var nodeFromUndo = new Watch3D();
            var vmUndo = new HelixWatch3DNodeViewModel(nodeFromUndo, vmParams);

            var nodeFromCopy = new Watch3D();
            var vmCopy = new HelixWatch3DNodeViewModel(nodeFromCopy, vmParams);

            nodeFromFile.Deserialize(fileElement, SaveContext.File);
            nodeFromUndo.Deserialize(undoElement, SaveContext.Undo);
            nodeFromCopy.Deserialize(copyElement, SaveContext.Copy);

            var newCam = vmFile.Camera;

            // Making sure we have properties preserved through file operation.
            Assert.AreEqual(original.WatchWidth, nodeFromFile.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromFile.WatchHeight);
            Assert.AreEqual(cam.Position.X, newCam.Position.X);
            Assert.AreEqual(cam.Position.Y, newCam.Position.Y);
            Assert.AreEqual(cam.Position.Z, newCam.Position.Z);
            Assert.AreEqual(cam.LookDirection.X, newCam.LookDirection.X);
            Assert.AreEqual(cam.LookDirection.Y, newCam.LookDirection.Y);
            Assert.AreEqual(cam.LookDirection.Z, newCam.LookDirection.Z);

            newCam = vmUndo.Camera;

            // Making sure we have properties preserved through undo operation.
            Assert.AreEqual(original.WatchWidth, nodeFromUndo.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromUndo.WatchHeight);
            Assert.AreEqual(cam.Position.X, newCam.Position.X);
            Assert.AreEqual(cam.Position.Y, newCam.Position.Y);
            Assert.AreEqual(cam.Position.Z, newCam.Position.Z);
            Assert.AreEqual(cam.LookDirection.X, newCam.LookDirection.X);
            Assert.AreEqual(cam.LookDirection.Y, newCam.LookDirection.Y);
            Assert.AreEqual(cam.LookDirection.Z, newCam.LookDirection.Z);

            newCam = vmCopy.Camera;

            // Making sure we have properties preserved through copy operation.
            Assert.AreEqual(original.WatchWidth, nodeFromCopy.WatchWidth);
            Assert.AreEqual(original.WatchHeight, nodeFromCopy.WatchHeight);
            Assert.AreEqual(cam.Position.X, newCam.Position.X);
            Assert.AreEqual(cam.Position.Y, newCam.Position.Y);
            Assert.AreEqual(cam.Position.Z, newCam.Position.Z);
            Assert.AreEqual(cam.LookDirection.X, newCam.LookDirection.X);
            Assert.AreEqual(cam.LookDirection.Y, newCam.LookDirection.Y);
            Assert.AreEqual(cam.LookDirection.Z, newCam.LookDirection.Z);
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
        public void Watch3D_FirstRun()
        {
            OpenVisualizationTest("FirstRunWatch3D.dyn");

            var view = FindFirstWatch3DNodeView();
            var vm = view.ViewModel as HelixWatch3DNodeViewModel;

            Assert.AreEqual(vm.SceneItems.Count(), 3);
        }

        [Test]
        public void Watch3D_Disconnect_Reconnect_CorrectRenderings()
        {
            OpenVisualizationTest("ASM_points_line.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            var watch3DNode = ws.FirstNodeFromWorkspace<Watch3D>();
            var totalPointsInView = BackgroundPreviewGeometry.TotalPoints();
            Assert.Greater(totalPointsInView, 3);

            // Disconnect the port coming into the watch3d node.
            var connector = watch3DNode.InPorts[0].Connectors.First();
            watch3DNode.InPorts[0].Connectors.Remove(connector);

            // Three items, the grid, the axes, and the light will remain.
            var view = FindFirstWatch3DNodeView();
            Assert.AreEqual(view.View.Items.Count, 3);

            var linesNode = ws.Nodes.First(n => n.GUID.ToString() == "7c1cecee-43ed-43b5-a4bb-5f71c50341b2");

            var cmd1 = new DynamoModel.MakeConnectionCommand(linesNode.GUID.ToString(), 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin);
            var cmd2 = new DynamoModel.MakeConnectionCommand(watch3DNode.GUID.ToString(), 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End);

            ViewModel.Model.ExecuteCommand(cmd1);
            ViewModel.Model.ExecuteCommand(cmd2);

            // View contains 3 default items and a collection of lines from the node connected to the Watch3D node
            Assert.AreEqual(4, view.View.Items.Count);
        }

        [Test]
        public void HelixWatch3DViewModel_DisableGrid_GridDoesNotDraw()
        {
            var bPreviewVm = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            Assert.IsNotNull(bPreviewVm, "HelixWatch3D has not been loaded");
            bPreviewVm.Active = true;
            bPreviewVm.IsGridVisible = false;

            // check if grid has not redraw
            Assert.IsTrue(bPreviewVm.Active, "Background has become inactive");
            Assert.IsFalse(bPreviewVm.IsGridVisible, "Background grid has not been hidden");
            var grid = bPreviewVm.Model3DDictionary[HelixWatch3DViewModel.DefaultGridName];
            Assert.AreEqual(Visibility.Hidden, grid.Visibility, "Background grid has not been hidden");
        }

        [Test]
        public void HelixWatch3DViewModel_OpenFileWithGridDisabled_GridDoesNotDraw()
        {
            HelixWatch3DViewModel_DisableGrid_GridDoesNotDraw();

            OpenVisualizationTest("CBN.dyn");

            // check if grid has not redraw after opening a file
            var bPreviewVm = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            Assert.IsTrue(bPreviewVm.Active, "Background has become inactive");
            Assert.IsFalse(bPreviewVm.IsGridVisible, "Background grid has become visible");
            var grid = bPreviewVm.Model3DDictionary[HelixWatch3DViewModel.DefaultGridName];
            Assert.AreEqual(Visibility.Hidden, grid.Visibility, "Background grid has become visible");
        }

        [Test]
        public void HelixWatch3DViewModel_OpenFileWithGridDisabled_EnableGrid_GridDraws()
        {
            HelixWatch3DViewModel_OpenFileWithGridDisabled_GridDoesNotDraw();

            // turn on grid
            ViewModel.ToggleBackgroundGridVisibilityCommand.Execute(null);

            // check if grid has appeared
            var bPreviewVm = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            Assert.IsTrue(bPreviewVm.Active, "Background has become inactive");
            Assert.IsTrue(bPreviewVm.IsGridVisible, "Background grid has not appeared");
            var grid = bPreviewVm.Model3DDictionary[HelixWatch3DViewModel.DefaultGridName];
            Assert.AreEqual(Visibility.Visible, grid.Visibility, "Background grid has not appeared");
        }

        [Test]
        public void HelixWatch3DViewModel_ChangeBackgroundVisibility_CanNavigateButtonsAreCorrect()
        {
            var bPreviewVm = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            Assert.IsNotNull(bPreviewVm, "HelixWatch3D has not been loaded");
            bPreviewVm.Active = false;

            Assert.IsFalse(bPreviewVm.Active, "Background has not been turned off");
            var currentWorkspace = View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First();
            Assert.AreEqual(Visibility.Hidden, currentWorkspace.statusBarPanel.Visibility, "Navigation buttons were not hidden");

            // turn on background
            ViewModel.ToggleFullscreenWatchShowingCommand.Execute(null);

            Assert.IsTrue(bPreviewVm.Active, "Background has not been turned on");
            Assert.AreEqual(Visibility.Visible, currentWorkspace.statusBarPanel.Visibility, "Navigation buttons did not appear");
        }

        #endregion

        #region dynamo view tests

        [Test]
        public void ViewSettings_ShowEdges_Toggled_GeometryIsCorrect()
        {
            OpenVisualizationTest("ASM_cuboid.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalCurves());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = true;
            Assert.AreEqual(12, BackgroundPreviewGeometry.TotalCurves());

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalCurves());
        }

        #endregion

        [Test]
        public void Python_CreatesVisualizations()
        {
            OpenVisualizationTest("ASM_python.dyn");

            //total points are the two strips of points at the top and
            //bottom of the mesh, duplicated 11x2x2 plus the one mesh
            Assert.AreEqual(1000, BackgroundPreviewGeometry.TotalPoints());
            Assert.AreEqual(1000 * 36, BackgroundPreviewGeometry.TotalMeshVerticesToRender());
        }

        [Test]
        public void Display_ByGeometryColor_HasColoredMesh()
        {
            OpenVisualizationTest("Display.ByGeometryColor.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();

            Assert.True(BackgroundPreviewGeometry.HasAnyMeshVerticesOfColor(new Color4(new Color3(1.0f, 0, 1.0f))));
        }

        [Test]
        public void Display_BySurfaceColors_HasColoredMesh()
        {
            OpenVisualizationTest("Display.BySurfaceColors.dyn");

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;

            RunCurrentModel();

            Assert.True(BackgroundPreviewGeometry.HasAnyColorMappedMeshes());
        }

        [Test]
        public void TurnGlobalPreviewOnAndOff()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\visualization\PreviewGlobal.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var workspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            workspace.RunSettings.RunType = RunType.Automatic;

            // Identifiers for all nodes in the workspace.
            var nodeIds = new[]
            {
                Guid.Parse("0750d269-26f3-47d7-bb25-79c762a540ff"), // point0 ( visible )
                Guid.Parse("33b03fa4-50e7-4d08-a5d8-ae7f168c2624"), // point1 ( invisible )
                Guid.Parse("fa226a63-1a31-4f13-8b93-1286dac2c695"), // point2 ( visible )
                Guid.Parse("e7b0f340-a0bc-46b2-a05d-7dd71c72e27c"), // originCodeBlock (visible)
            };

            // Ensure we do have all the nodes we expected here.
            var nodes = nodeIds.Select(workspace.NodeFromWorkspace).ToList();
            Assert.IsFalse(nodes.Any(n => n == null)); // Nothing should be null.

            // Ensure that the initial visibility is correct
            Assert.IsTrue(nodes.Select(n => n.IsVisible).SequenceEqual(new[]
            {
                true, false, true, true
            }));
            // Ensure that visulations match our expectations
            Assert.AreEqual(2, BackgroundPreviewGeometry.TotalPoints());

            // Now turn off the preview of all the nodes
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.ShowHideAllGeometryPreviewCommand.Execute("false");

            // Check that all node visibility is off and nothing is shown
            Assert.IsTrue(nodes.Select(n => n.IsVisible).SequenceEqual(new[]
            {
                false, false, false, false
            }));
            Assert.AreEqual(2, BackgroundPreviewGeometry.NumberOfInvisiblePoints());// There should be no point left.

            // Now turn on the preview of all the nodes
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.ShowHideAllGeometryPreviewCommand.Execute("true");
            Assert.AreEqual(3, BackgroundPreviewGeometry.TotalPoints());
        }

        [Test]
        public void SettingGlobalLacingStrategy()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\visualization\LacingStrategyGlobal.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var workspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            workspace.RunSettings.RunType = RunType.Automatic;

            // Identifiers for all nodes in the workspace.
            var nodeIds = new[]
            {
                Guid.Parse("0750d269-26f3-47d7-bb25-79c762a540ff"), // point0
                Guid.Parse("33b03fa4-50e7-4d08-a5d8-ae7f168c2624"), // point1
                Guid.Parse("fa226a63-1a31-4f13-8b93-1286dac2c695"), // point2
                Guid.Parse("e7b0f340-a0bc-46b2-a05d-7dd71c72e27c"), // arrays codeblock [1..5] and [11..12]
            };

            // Ensure we do have all the nodes we expected here.
            var nodes = nodeIds.Select(workspace.NodeFromWorkspace).ToList();
            Assert.IsFalse(nodes.Any(n => n == null)); // Nothing should be null.

            // Ensure that visulations match our expectations
            // Initially, all nodes has shortest lacing strategy
            // 5 points for point0 node, 2 points for point1 and point2 node
            Assert.AreEqual(9, BackgroundPreviewGeometry.TotalPoints());

            // Modify lacing strategy to Longest
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.Longest.ToString());

            Assert.AreEqual(12, BackgroundPreviewGeometry.TotalPoints());

            // Modify lacing strategy to Auto
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.Auto.ToString());

            Assert.AreEqual(9, BackgroundPreviewGeometry.TotalPoints());

            // Modify lacing strategy to CrossProduct
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.CrossProduct.ToString());

            Assert.AreEqual(17, BackgroundPreviewGeometry.TotalPoints());

            // Change lacing back to Shortest
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.Shortest.ToString());
            Assert.AreEqual(9, BackgroundPreviewGeometry.TotalPoints());
        }

        [Test]
        [Category("RegressionTests")]
        public void TypedIdentifierInCodeBlockNode()
        {
            // Regression test for MAGN-7518 that expression "x : Point = Point.ByCoordinate()" in CBN
            // doesn't generate background preview.
            OpenVisualizationTest("TypedIdentifierInCBN.dyn");
            Assert.AreEqual(2, BackgroundPreviewGeometry.TotalPoints());
        }

        [Test]
        [Category("RegressionTests")]
        public void Switch3DBackgroundPreview()
        {
            // Regression test for MAGN-9140 that all geometries that created
            // when the background preview is off should display when the
            // background preview is switched to on.
            ViewModel.Watch3DViewModels.First().Active = false;
            OpenVisualizationTest("OnePoint.dyn");
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalPoints());
            ViewModel.Watch3DViewModels.First().Active = true;
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());
        }

        [Test]
        [Category("RegressionTests")]
        public void MAGN9434_DeleteCodeBlockNode()
        {
            OpenVisualizationTest("CreatePoint.dyn");
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());
            var codeBlockNode = ViewModel.CurrentSpace.NodeFromWorkspace<CodeBlockNodeModel>(Guid.Parse("7883d92a-ef8b-4e05-8c7d-46cfc627c994"));

            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty, codeBlockNode.GUID, "Code", "p2 = Point.ByCoordinates();");
            ViewModel.Model.ExecuteCommand(command);
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());

            var deleteCommand = new DynamoModel.DeleteModelCommand(codeBlockNode.GUID);
            ViewModel.Model.ExecuteCommand(deleteCommand);
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalPoints());
        }

        [Test]
        [Category("RegressionTests")]
        public void MAGN9434_DisablePreview()
        {
            OpenVisualizationTest("CreatePoint.dyn");
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());
            var codeBlockNode = ViewModel.CurrentSpace.NodeFromWorkspace<CodeBlockNodeModel>(Guid.Parse("7883d92a-ef8b-4e05-8c7d-46cfc627c994"));

            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty, codeBlockNode.GUID, "Code", "p2 = Point.ByCoordinates();");
            ViewModel.Model.ExecuteCommand(command);
            Assert.AreEqual(1, BackgroundPreviewGeometry.TotalPoints());

            var disableCommand = new DynamoModel.UpdateModelValueCommand(Guid.Empty, codeBlockNode.GUID, "IsVisible", "false");
            ViewModel.Model.ExecuteCommand(disableCommand);
            Assert.AreEqual(0, BackgroundPreviewGeometry.TotalPoints() - BackgroundPreviewGeometry.NumberOfInvisiblePoints());
        }

        [Test]
        [Category("RegressionTests")]
        public void CanTagGeometryWhenClickingSingleItemInPreviewBubble()
        {
            tagGeometryWhenClickingItem(new[] {0, 0}, 1, "Point.ByCoordinates", 
                n => n.ViewModel.NodeModel, true);
        }

        [Test]
        [Category("RegressionTests")]
        public void CanTagGeometryWhenClickingArrayItemInPreviewBubble()
        {
            tagGeometryWhenClickingItem(new[] {0}, 11, "Point.ByCoordinates",
                n => n.ViewModel.NodeModel, true);
        }

        [Test]
        [Category("RegressionTests")]
        public void CanTagGeometryWhenClickingSingleItemInWatchNode()
        {
            tagGeometryWhenClickingItem(new[] { 0, 0 }, 1, "Watch",
                n => n.ViewModel.NodeModel.InPorts[0].Connectors[0].Start.Owner);
        }

        [Test]
        [Category("RegressionTests")]
        public void CanTagGeometryWhenClickingArrayItemInWatchNode()
        {
            tagGeometryWhenClickingItem(new[] { 0 }, 11, "Watch", 
                n => n.ViewModel.NodeModel.InPorts[0].Connectors[0].Start.Owner);
        }
        
        private async void tagGeometryWhenClickingItem(int[] indexes, int expectedNumberOfLabels, 
            string nodeName, Func<NodeView,NodeModel> getGeometryOwnerNode, bool expandPreviewBubble = false)
        {
            OpenVisualizationTest("MAGN_3815.dyn");
            RunCurrentModel();
            Assert.AreEqual(3, Model.CurrentWorkspace.Nodes.Count());
            var nodeView = View.ChildrenOfType<NodeView>().First(nv => nv.ViewModel.Name == nodeName);
            Assert.IsNotNull(nodeView, "NodeView has not been found by given name: " + nodeName);

            if (expandPreviewBubble)
            {
                nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                View.Dispatcher.Invoke(() =>
                {
                    nodeView.PreviewControl.BindToDataSource();
                    nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                    nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
                });

                DispatcherUtil.DoEvents();
            }

            var treeViewItem = nodeView.ChildOfType<TreeViewItem>();
            // find TreeViewItem by given index in multi-dimentional array
            foreach (var index in indexes)
            {
                treeViewItem = treeViewItem.ChildrenOfType<TreeViewItem>().ElementAt(index);
            }

            // click on the found TreeViewItem
            View.Dispatcher.Invoke(() =>
            {
                treeViewItem.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent
                });
            });

            DispatcherUtil.DoEvents();

            // check if label has been added to corresponding geometry
            var helix = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            var labelKey = getGeometryOwnerNode(nodeView).AstIdentifierForPreview.Name + ":text";
            Assert.IsTrue(helix.Model3DDictionary.ContainsKey(labelKey), "Label has not been added to selected geometry item");
            var geometry = (helix.Model3DDictionary[labelKey] as GeometryModel3D).Geometry as BillboardText3D;
            Assert.AreEqual(expectedNumberOfLabels, geometry.TextInfo.Count);
        }

        private Watch3DView FindFirstWatch3DNodeView()
        {
            var views = View.ChildrenOfType<Watch3DView>();
            return views.Last();
        }

        [Test]
        public void GeometryPreviewWhenClickingArrayItemInPreview()
        {
            OpenVisualizationTest("magn_10809.dyn");
            RunCurrentModel();
            DispatcherUtil.DoEvents();

            var vm = ViewModel.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            Assert.NotNull(vm);

            // select original node
            vm.AddLabelForPath("var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0:0");
            var originalNode = vm.Model3DDictionary["var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0:0" + ":mesh"];
            var otherNode = vm.Model3DDictionary["var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0:1" + ":mesh"];
            Assert.IsTrue((bool)originalNode.GetValue(AttachedProperties.ShowSelectedProperty)); 
            Assert.IsFalse((bool)otherNode.GetValue(AttachedProperties.ShowSelectedProperty));

            // deselect original node
            vm.AddLabelForPath("var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0:0");
            Assert.IsFalse((bool)originalNode.GetValue(AttachedProperties.ShowSelectedProperty));
            Assert.IsFalse((bool)otherNode.GetValue(AttachedProperties.ShowSelectedProperty));

            // select another node
            vm.AddLabelForPath("var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0:1");
            Assert.IsFalse((bool)originalNode.GetValue(AttachedProperties.ShowSelectedProperty));
            Assert.IsTrue((bool)otherNode.GetValue(AttachedProperties.ShowSelectedProperty));

            // Select node one level up
            vm.AddLabelForPath("var_88f09e6c057f4a5c95a7f4f0b25161e2:0:0");
            Assert.IsTrue((bool)originalNode.GetValue(AttachedProperties.ShowSelectedProperty));
            Assert.IsTrue((bool)otherNode.GetValue(AttachedProperties.ShowSelectedProperty));
        }

    }

    internal static class GeometryDictionaryExtensions
    {
        private static List<string> keyList = new List<string>()
            {
                HelixWatch3DViewModel.DefaultAxesName,
                HelixWatch3DViewModel.DefaultGridName,
                HelixWatch3DViewModel.DefaultLightName
            };

        public static int TotalPoints(this IEnumerable<Model3D> dictionary)
        {
            var points = dictionary.Where(g => g is PointGeometryModel3D && !keyList.Contains(g.Name)).ToArray();

            return points.Any()
                ? points.SelectMany(g => ((PointGeometryModel3D)g).Geometry.Positions).Count()
                : 0;
        }

        /// <summary>
        /// Returns the total number of DynamoGeometryModel3D objects.
        /// 
        /// Each DynamoGeometryModel3D object may contain more than one mesh.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static int TotalMeshes(this IEnumerable<Model3D> dictionary)
        {
            return dictionary.Count(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name));
        }

        public static IEnumerable<DynamoGeometryModel3D> Meshes(this IEnumerable<Model3D> geometry)
        {
            var candidates = geometry.Where(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name));
            return candidates.Cast<DynamoGeometryModel3D>();
        }

        public static IEnumerable<LineGeometryModel3D> Curves(this IEnumerable<Model3D> geometry)
        {
            var candidates = geometry.Where(g => g is LineGeometryModel3D && !keyList.Contains(g.Name));
            return candidates.Cast<LineGeometryModel3D>();
        }

        public static IEnumerable<PointGeometryModel3D> Points(this IEnumerable<Model3D> geometry)
        {
            var candidates = geometry.Where(g => g is PointGeometryModel3D && !keyList.Contains(g.Name));
            return candidates.Cast<PointGeometryModel3D>();
        }

        public static bool IsDead(this GeometryModel3D geometry)
        {
            if (geometry is PointGeometryModel3D || geometry is LineGeometryModel3D)
            {
                return geometry.Geometry.Colors.All(c => c == HelixWatch3DViewModel.DefaultDeadColor);
            }

            if (geometry is DynamoGeometryModel3D)
            {
                return geometry.Geometry.Colors.All(c => c.Alpha < 1.0f);
            }

            return false;
        }

        public static bool IsAlive(this GeometryModel3D geometry)
        {
            if (geometry is PointGeometryModel3D)
            {
                return geometry.Geometry.Colors.All(c => c == HelixWatch3DViewModel.DefaultPointColor);
            }

            if (geometry is LineGeometryModel3D)
            {
                return geometry.Geometry.Colors.All(c => c == HelixWatch3DViewModel.DefaultLineColor);
            }

            if (geometry is DynamoGeometryModel3D)
            {
                return geometry.Geometry.Colors.All(c => c.Alpha == 1.0f);
            }

            return false;
        }

        public static int TotalCurves(this IEnumerable<Model3D> dictionary)
        {
            var lines = dictionary.Where(g => g is LineGeometryModel3D && ! keyList.Contains(g.Name)).ToArray();

            return lines.Any()
                ? lines.SelectMany(g => ((LineGeometryModel3D)g).Geometry.Positions).Count()/2
                : 0;
        }

        public static int TotalText(this IEnumerable<Model3D> dictionary)
        {
            var text = dictionary
                .Where(g => g is BillboardTextModel3D && !keyList.Contains(g.Name))
                .Cast<BillboardTextModel3D>()
                .Select(bb=>bb.Geometry).Cast<BillboardText3D>()
                .ToArray();

            return text.Any()
                ? text.Where(t=>t != null).SelectMany(t=>t.TextInfo).Count()
                : 0;
        }

        public static int TotalCurveVerticesOfColor(this IEnumerable<Model3D> dictionary, Color4 color)
        {
            var geoms = dictionary.Where(g => g is LineGeometryModel3D && !keyList.Contains(g.Name)).Cast<LineGeometryModel3D>();

            return geoms.Sum(g => g.Geometry.Colors.Count(c => c == color));
        }

        public static bool HasNumberOfPointsCurvesAndMeshes(this IEnumerable<Model3D> geometry, int numberOfPoints, 
            int numberOfCurves, int numberOfMeshes)
        {
            return geometry.TotalPoints() == numberOfPoints &&
                   geometry.TotalCurves() == numberOfCurves &&
                   geometry.TotalMeshes() == numberOfMeshes;
        }

        public static int NumberOfInvisiblePoints(this IEnumerable<Model3D> dictionary)
        {
            var points = dictionary.Where(g => g is PointGeometryModel3D && !keyList.Contains(g.Name)).ToArray();

            return points.Any() ? points.Count(g => g.Visibility == Visibility.Hidden) : 0;
        }

        public static int NumberOfInvisibleCurves(this IEnumerable<Model3D> dictionary)
        {
            var lines = dictionary.Where(g => g is LineGeometryModel3D && !keyList.Contains(g.Name)).ToArray();

            return lines.Any() ? lines.Count(g => g.Visibility == Visibility.Hidden) : 0;
        }

        public static int NumberOfInvisibleMeshes(this IEnumerable<Model3D> dictionary)
        {
            var geoms = dictionary.Where(g => g is DynamoGeometryModel3D).ToArray();
            return geoms.Any() ? geoms.Count(g => g.Visibility == Visibility.Hidden) : 0;
        }

        public static bool HasVisibleObjects(this IEnumerable<Model3D> dictionary)
        {
            return dictionary.Any(g => g.Visibility == Visibility.Visible);
        }

        public static bool HasNoUserCreatedModel3DObjects(this IEnumerable<Model3D> dictionary)
        {
            return dictionary.All(g => keyList.Contains(g.Name));
        }

        public static bool HasAnyMeshes(this IEnumerable<Model3D> dictionary)
        {
            return dictionary.TotalMeshes() > 0;
        }

        public static int TotalMeshVerticesToRender(this IEnumerable<Model3D> dictionary)
        {
            var geoms = dictionary.Where(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name)).Cast<DynamoGeometryModel3D>();

            return geoms.Any()? geoms.SelectMany(g=>g.Geometry.Positions).Count() : 0;
        }

        public static bool HasAnyMeshVerticesOfColor(this IEnumerable<Model3D> dictionary, Color4 color)
        {
            var geoms = dictionary.Where(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name)).Cast<DynamoGeometryModel3D>();

            return geoms.Any(g => g.Geometry.Colors.Any(c => c == color));
        }

        public static bool HasMeshVerticesAllOfColor(this IEnumerable<Model3D> dictionary, Color4 color)
        {
            var geoms = dictionary.Where(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name)).Cast<DynamoGeometryModel3D>();

            return geoms.All(g => g.Geometry.Colors.All(c => c == color));
        }

        public static bool HasAnyColorMappedMeshes(this IEnumerable<Model3D> dictionary)
        {
            var geoms = dictionary.Where(g => g is DynamoGeometryModel3D && !keyList.Contains(g.Name)).Cast<DynamoGeometryModel3D>();

            return geoms.Any(g => ((PhongMaterial)g.Material).DiffuseMap != null);
        }
    }
}

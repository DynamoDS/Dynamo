using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class CoreUserInterfaceTests : SystemTestBase
    {
        #region SaveImageCommand

        [Test]
        [Category("DynamoUI")]
        public void CanSaveImage()
        {
            // Save image command now requires the workspace to be not empty.
            var testPath = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testPath, @"core\nodeLocationTest.dyn");

            OpenDynamoDefinition(openPath);
            DispatcherUtil.DoEvents(); // Allows visual tree to be reconstructed.

            string path = Path.Combine(TempFolder, "output.png");
            ViewModel.SaveImageCommand.Execute(path);

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Test]
        [Category("DynamoUI")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            ViewModel.SaveImageCommand.Execute(path);

            Assert.False(File.Exists(path));
        }

        #endregion

        #region ToggleConsoleShowingCommand

        [Test]
        [Category("DynamoUI")]
        public void CanHideConsoleWhenShown()
        {
            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.True(ViewModel.ConsoleHeight > 0);
        }

        [Test]
        [Category("DynamoUI")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(ViewModel.ConsoleHeight > 0);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanShowConsoleWhenHidden()
        {
            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.True(ViewModel.ConsoleHeight > 0);

            ViewModel.ToggleConsoleShowingCommand.Execute(null);
            Assert.False(ViewModel.ConsoleHeight > 0);
        }

        #endregion

        #region Zoom In and Out canvas

        [Test]
        [Category("DynamoUI")]
        public void CanZoom()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;
            double zoom;
            
            // Test Zoom in
            zoom = workspaceModel.Zoom;
            if ( ViewModel.ZoomInCommand.CanExecute(null) )
                ViewModel.ZoomInCommand.Execute(null);
            Assert.Greater(workspaceModel.Zoom, zoom);

            // Test Zoom out
            zoom = workspaceModel.Zoom;
            if (ViewModel.ZoomOutCommand.CanExecute(null))
                ViewModel.ZoomOutCommand.Execute(null);
            Assert.Greater(zoom, workspaceModel.Zoom);

            // Test can set zoom (at random zoom for 10 times)
            int testLoop = 10;
            for (int i = 0; i < testLoop; i++)
            {
                // Get random number for the zoom
                double upperBound = WorkspaceModel.ZOOM_MAXIMUM;
                double lowerBound = WorkspaceModel.ZOOM_MINIMUM;
                Random random = new Random();
                double randomNumber = random.NextDouble() * (upperBound - lowerBound) + lowerBound;

                if (ViewModel.CurrentSpaceViewModel.SetZoomCommand.CanExecute(randomNumber))
                    ViewModel.CurrentSpaceViewModel.SetZoomCommand.Execute(randomNumber);

                // Check Zoom is correct
                Assert.AreEqual(randomNumber, workspaceModel.Zoom);
            }

            // Border Test for Set Zoom
            // Min zoom
            zoom = WorkspaceModel.ZOOM_MINIMUM;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreEqual(zoom, workspaceModel.Zoom);
            // Zoom out over limit (check that it does not zoom out)
            if (ViewModel.ZoomOutCommand.CanExecute(null))
                ViewModel.ZoomOutCommand.Execute(null);
            Assert.AreEqual(zoom, workspaceModel.Zoom);
            
            // Max zoom
            zoom = WorkspaceModel.ZOOM_MAXIMUM;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreEqual(zoom, workspaceModel.Zoom);
            // Zoom in over limit (check that it does not zoom in)
            if (ViewModel.ZoomInCommand.CanExecute(null))
                ViewModel.ZoomInCommand.Execute(null);
            Assert.AreEqual(zoom, workspaceModel.Zoom);

            // Above Max Limit Test
            zoom = WorkspaceModel.ZOOM_MAXIMUM + 0.1;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreNotEqual(zoom, workspaceModel.Zoom);

            // Below Min Limit Test
            zoom = WorkspaceModel.ZOOM_MINIMUM - 0.1;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreNotEqual(zoom, workspaceModel.Zoom);

            // Stress Test
            // Zoom in and out repeatly
            for (int i = 0; i < 20; i++)
            {
                for (int stepIn = 0; stepIn < 20; stepIn++)
                {
                    if (ViewModel.ZoomInCommand.CanExecute(null))
                        ViewModel.ZoomInCommand.Execute(null);
                }
                for (int stepOut = 0; stepOut < 20; stepOut++)
                {
                    if (ViewModel.ZoomOutCommand.CanExecute(null))
                        ViewModel.ZoomOutCommand.Execute(null);
                }
            }
            // Doesn't crash the system
            Assert.True(true);
        }

        #endregion

        #region Pan Left, Right, Top, Down Canvas

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanLeft()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Left"))
                    ViewModel.PanCommand.Execute("Left");
            }

            Assert.Greater(workspaceModel.X, posX);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanRight()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Right"))
                    ViewModel.PanCommand.Execute("Right");
            }

            Assert.Greater(posX, workspaceModel.X);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanUp()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Up"))
                    ViewModel.PanCommand.Execute("Up");
            }

            Assert.AreEqual(posX, workspaceModel.X);
            Assert.Greater(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanDown()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (ViewModel.PanCommand.CanExecute("Down"))
                    ViewModel.PanCommand.Execute("Down");
            }

            Assert.AreEqual(posX, workspaceModel.X);
            Assert.Greater(posY, workspaceModel.Y);
        }

        #endregion

        #region Fit to View

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void FitViewWithNoNodes()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();
            
            // Check for no changes
            Assert.AreEqual(workspaceModel.Zoom, initZoom);
            Assert.AreEqual(workspaceModel.X, initX);
            Assert.AreEqual(workspaceModel.Y, initY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitView()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();

            // Check for no changes
            Assert.AreNotEqual(workspaceModel.Zoom, initZoom);
            Assert.AreNotEqual(workspaceModel.X, initX);
            Assert.AreNotEqual(workspaceModel.Y, initY);

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewTwiceForActualZoom()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            workspaceVM.FitViewInternal();

            // Check for no changes
            Assert.AreNotEqual(workspaceModel.Zoom, initZoom);
            Assert.AreNotEqual(workspaceModel.X, initX);
            Assert.AreNotEqual(workspaceModel.Y, initY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void FitViewStressTest()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;
            
            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            for (int i = 0; i < 100; i++)
            {
                // Zoom to max zoom value
                workspaceVM.FitViewInternal();
            }

            // Not crashed
            Assert.True(true);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByZoom()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            workspaceVM.FitViewInternal();

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some zoom action before FitView again
            ViewModel.ZoomIn(null);

            workspaceVM.FitViewInternal();

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByPan()
        {
            WorkspaceModel workspaceModel = ViewModel.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = ViewModel.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            workspaceVM.FitViewInternal();

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some pan action before FitView again
            ViewModel.Pan("Up" as object);

            workspaceVM.FitViewInternal();

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            ViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        // Add a number node on workspace
        private void CreateNodeOnCurrentWorkspace()
        {
            // Create number node
            var numNode = new DoubleInput { X = 100, Y = 100 };
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(numNode, true);
        }

        #endregion

        [Test,RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting_BackgroundPreview_1_0API()
        {
            bool expectedValue = !ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive;
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);
          

            expectedValue = !ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive;
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.IsBackgroundPreviewActive);

            #region Save And Load of PreferenceSettings

            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.IsBackgroundPreviewActive = true;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.IsBackgroundPreviewActive, initalSetting.IsBackgroundPreviewActive);
            #endregion

            #region Second Test
            initalSetting.IsBackgroundPreviewActive = false;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.IsBackgroundPreviewActive, initalSetting.IsBackgroundPreviewActive);
            #endregion
        }

        #region PreferenceSettings
        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void PreferenceSetting()
        {
            // Test Case to ensure that the link for these persistent variable
            // between DynamoViewModel, Model is not broken or replaced.
            #region BackgroundPreviewActive

            var backgroundPreviewName = ViewModel.BackgroundPreviewViewModel.PreferenceWatchName;
            bool expectedValue = !ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName);
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));

            expectedValue = !ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName);
            ViewModel.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, ViewModel.Model.PreferenceSettings.GetIsBackgroundPreviewActive(backgroundPreviewName));
            #endregion

            #region ConsoleHeight
            int expectedHeight = 100;
            ViewModel.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedHeight, ViewModel.Model.PreferenceSettings.ConsoleHeight);

            expectedHeight = 0;
            ViewModel.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedHeight, ViewModel.Model.PreferenceSettings.ConsoleHeight);
            #endregion

            #region ConnectorType
            ConnectorType expectedConnector = ConnectorType.BEZIER;
            ViewModel.SetConnectorType("BEZIER");
            Assert.AreEqual(expectedConnector, ViewModel.Model.PreferenceSettings.ConnectorType);

            expectedConnector = ConnectorType.POLYLINE;
            ViewModel.SetConnectorType("POLYLINE");
            Assert.AreEqual(expectedConnector, ViewModel.Model.PreferenceSettings.ConnectorType);
            #endregion

            #region Collect Information Option
            {
                // Backup the value of Dynamo.IsTestMode and restore it later. The 
                // reason for this is 'IsUsageReportingApproved' only returns the 
                // actual value when not running in test mode.
                var isTestMode = DynamoModel.IsTestMode;

                // First time run, check if dynamo did set it back to false after running
                Assert.AreEqual(false, UsageReportingManager.Instance.FirstRun);

                // CollectionInfoOption To TRUE
                UsageReportingManager.Instance.SetUsageReportingAgreement(true);
                RestartTestSetup(startInTestMode: false);
                Assert.AreEqual(true, UsageReportingManager.Instance.IsUsageReportingApproved);

                // CollectionInfoOption To FALSE
                UsageReportingManager.Instance.SetUsageReportingAgreement(false);
                RestartTestSetup(startInTestMode: false);
                Assert.AreEqual(false, UsageReportingManager.Instance.IsUsageReportingApproved);

                DynamoModel.IsTestMode = isTestMode; // Restore the orignal value.
            }
            #endregion

            #region Save And Load of PreferenceSettings
            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            #region First Test

            initalSetting.ConnectorType = ConnectorType.BEZIER;
            initalSetting.ConsoleHeight = 100;
            initalSetting.SetIsBackgroundPreviewActive(backgroundPreviewName, true);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.GetIsBackgroundPreviewActive(backgroundPreviewName),
                initalSetting.GetIsBackgroundPreviewActive(backgroundPreviewName));
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ConsoleHeight, initalSetting.ConsoleHeight);
            #endregion

            #region Second Test
            initalSetting.ConnectorType = ConnectorType.POLYLINE;
            initalSetting.ConsoleHeight = 0;
            initalSetting.SetIsBackgroundPreviewActive(backgroundPreviewName, false);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.GetIsBackgroundPreviewActive(backgroundPreviewName),
                initalSetting.GetIsBackgroundPreviewActive(backgroundPreviewName));
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ConsoleHeight, initalSetting.ConsoleHeight);
            #endregion

            #endregion

            View.Close();
        }

        [Test]
        public void PreferenceSettings_ShowEdges_DefaultFalse()
        {
            var settings = new PreferenceSettings();
            Assert.False(settings.ShowEdges);
        }

        [Test]
        public void PreferenceSettings_ShowEdges_Toggle()
        {
            ViewModel.RenderPackageFactoryViewModel.ShowEdges = false;
            Assert.False(Model.PreferenceSettings.ShowEdges);

            ViewModel.RenderPackageFactoryViewModel.ShowEdges = true;
            Assert.True(Model.PreferenceSettings.ShowEdges);
        }

        [Test]
        public void PreferenceSettings_ShowEdges_Save()
        {
            // Test if variable can be serialize and deserialize without any issue
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            var initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.ShowEdges = true;
            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);
            Assert.True(resultSetting.ShowEdges);
        }

        private void RestartTestSetup(bool startInTestMode)
        {
            // Shutdown Dynamo and restart it
            View.Close();
            View = null;

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            Model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = startInTestMode,
                    ProcessMode = startInTestMode 
                        ? TaskProcessMode.Synchronous 
                        : TaskProcessMode.Asynchronous
                });

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            //create the view
            View = new DynamoView(ViewModel);

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
        #endregion

        #region InfoBubble

        [Test]
        [Category("DynamoUI")]
        public void UpdateInfoBubble_ErrorBubble()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel(this.ViewModel);
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_ErrorBubble = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Error,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Bottom);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_ErrorBubble);
                Assert.AreEqual(content, infoBubble.Content);
                Assert.AreEqual(InfoBubbleViewModel.Style.Error, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Bottom, infoBubble.ConnectingDirection);
            }
        }

        [Test, Ignore]
        // Opacity is no longer affecting the visibility of infobubble. This requires opacity of UIElement
        // This test is no longer feasible. Keeping it for future reference
        [Category("DynamoUI")]
        public void Collapse()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel(this.ViewModel);
            infoBubble.OnRequestAction(new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));
            //Assert.AreEqual(0, infoBubble.Opacity);
        }

        #endregion

        #region Notes

        [Test]
        [Category("DynamoUI")]
        public void CanCreateANote()
        {
            ViewModel.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);
            
            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count());

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test]
        [Category("DynamoUI")]
        public void CanDeleteANote()
        {
            ViewModel.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count());

            //select the note for deletion
            DynamoSelection.Instance.Selection.Add(note);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            //delete the note
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0,Model.CurrentWorkspace.Notes.Count());

            ViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
           
        }


        #endregion

        [Test]
        [Category("UnitTests")]
        public void TestDraggedNode()
        {
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+")) { X = 16, Y = 32 };
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            NodeModel locatable = ViewModel.Model.CurrentWorkspace.Nodes.First();

            var startPoint = new Point2D(8, 64);
            var dn = new WorkspaceViewModel.DraggedNode(locatable, startPoint);

            // Initial node position.
            Assert.AreEqual(16, locatable.X);
            Assert.AreEqual(32, locatable.Y);

            // Move the mouse cursor to move node.
            dn.Update(new Point2D(-16, 72));
            Assert.AreEqual(-8, locatable.X);
            Assert.AreEqual(40, locatable.Y);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfOpenOnRightClick()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");
            RightClick(currentWs.zoomBorder);

            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfNotOpenOnNodeRightClick()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsNotNull(currentWs, "DynamoView does not have any WorkspaceView");
            CreateNodeOnCurrentWorkspace();

            DispatcherUtil.DoEvents();
            var node = currentWs.ChildOfType<NodeView>();
            RightClick(node);

            // workspace context menu shouldn't be open
            Assert.IsFalse(currentWs.ContextMenuPopup.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfInCanvasSearchHidesOnOpeningContextMenu()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();

            // show in-canvas search
            ViewModel.CurrentSpaceViewModel.ShowInCanvasSearchCommand.Execute(ShowHideFlags.Show);
            Assert.IsTrue(currentWs.InCanvasSearchBar.IsOpen);

            // open context menu
            RightClick(currentWs.zoomBorder);

            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
            Assert.IsFalse(currentWs.InCanvasSearchBar.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_TestIfSearchTextClearsOnOpeningContextMenu()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();

            // open context menu
            RightClick(currentWs.zoomBorder);

            // set dummy content for search text
            currentWs.ViewModel.InCanvasSearchViewModel.SearchText = "dummy";
            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);
            Assert.IsFalse(currentWs.InCanvasSearchBar.IsOpen);

            // show in-canvas search
            ViewModel.CurrentSpaceViewModel.ShowInCanvasSearchCommand.Execute(ShowHideFlags.Show);
            Assert.IsTrue(currentWs.InCanvasSearchBar.IsOpen);

            // check if search text is still empty
            Assert.IsTrue(currentWs.ViewModel.InCanvasSearchViewModel.SearchText.Equals(string.Empty));
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceContextMenu_IfSubmenuOpenOnMouseHover()
        {
            var currentWs = View.ChildOfType<WorkspaceView>();
            RightClick(currentWs.zoomBorder);
            Assert.IsTrue(currentWs.ContextMenuPopup.IsOpen);

            currentWs.WorkspaceLacingMenu.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = Mouse.MouseEnterEvent
            });

            DispatcherUtil.DoEvents();

            Assert.IsTrue(currentWs.WorkspaceLacingMenu.IsSubmenuOpen);
        }

        private void RightClick(IInputElement element)
        {
            element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });

            element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent
            });

            DispatcherUtil.DoEvents();
        }
    }
}
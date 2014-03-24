using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class CoreUserInterfaceTests : DynamoTestUI
    {
        #region SaveImageCommand

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CanSaveImage()
        {
            string path = Path.Combine(TempFolder, "output.png");

            Vm.SaveImageCommand.Execute(path);

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            Vm.SaveImageCommand.Execute(path);

            Assert.False(File.Exists(path));
        }

        #endregion

        #region ToggleConsoleShowingCommand

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CanShowConsoleWhenHidden()
        {
            Vm.ToggleConsoleShowingCommand.Execute(null);
            Ui.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => Assert.False(Ui.ConsoleShowing)));
            Assert.Inconclusive("Binding is not being updated in time for the test to complete correctly");
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(Ui.ConsoleShowing);
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CanHideConsoleWhenShown()
        {
            //Vm.ToggleConsoleShowingCommand.Execute(null);
            //Assert.True(ui.ConsoleShowing);

            //Vm.ToggleConsoleShowingCommand.Execute(null);
            //Assert.False(ui.ConsoleShowing); 
            
            Assert.Inconclusive("Binding is not being updated in time for the test to complete correctly");
        }

        #endregion

         //THIS WILL ALWAYS FAIL 

        //[Test, RequiresSTA]
        //[Category("DynamoUI")]
        //public void CanOpenAllSampleFilesWithoutError()
        //{
        //    var di = new DirectoryInfo(@"..\..\doc\Distrib\Samples\");
        //    int failCount = 0;

        //    foreach (DirectoryInfo d in di.GetDirectories())
        //    {

        //        foreach (FileInfo fi in d.GetFiles())
        //        {
        //            try
        //            {
        //                dynSettings.Bench.Dispatcher.Invoke(new Action(delegate
        //                {
        //                    dynSettings.Controller.CommandQueue.Enqueue(
        //                        Tuple.Create<object, object>(_vm.OpenCommand, fi.FullName));
        //                    dynSettings.Controller.ProcessCommandQueue();
        //                }));
        //            }
        //            catch(Exception e)
        //            {
        //                failCount++;
        //                Console.WriteLine(string.Format("Could not open {0}", fi.FullName));
        //                Console.WriteLine(string.Format("Could not open {0}", e.Message));
        //                Console.WriteLine(string.Format("Could not open {0}", e.StackTrace));
        //            }
        //        }
        //    }
        //    Assert.AreEqual(failCount, 0);
        //}

        #region Zoom In and Out canvas

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CanZoom()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;
            double zoom;
            
            // Test Zoom in
            zoom = workspaceModel.Zoom;
            if ( Vm.ZoomInCommand.CanExecute(null) )
                Vm.ZoomInCommand.Execute(null);
            Assert.Greater(workspaceModel.Zoom, zoom);

            // Test Zoom out
            zoom = workspaceModel.Zoom;
            if (Vm.ZoomOutCommand.CanExecute(null))
                Vm.ZoomOutCommand.Execute(null);
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

                if (Vm.CurrentSpaceViewModel.SetZoomCommand.CanExecute(randomNumber))
                    Vm.CurrentSpaceViewModel.SetZoomCommand.Execute(randomNumber);

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
            if (Vm.ZoomOutCommand.CanExecute(null))
                Vm.ZoomOutCommand.Execute(null);
            Assert.AreEqual(zoom, workspaceModel.Zoom);
            
            // Max zoom
            zoom = WorkspaceModel.ZOOM_MAXIMUM;
            if (workspaceVM.SetZoomCommand.CanExecute(zoom))
                workspaceVM.SetZoomCommand.Execute(zoom);
            Assert.AreEqual(zoom, workspaceModel.Zoom);
            // Zoom in over limit (check that it does not zoom in)
            if (Vm.ZoomInCommand.CanExecute(null))
                Vm.ZoomInCommand.Execute(null);
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
                    if (Vm.ZoomInCommand.CanExecute(null))
                        Vm.ZoomInCommand.Execute(null);
                }
                for (int stepOut = 0; stepOut < 20; stepOut++)
                {
                    if (Vm.ZoomOutCommand.CanExecute(null))
                        Vm.ZoomOutCommand.Execute(null);
                }
            }
            // Doesn't crash the system
            Assert.True(true);
        }

        #endregion

        #region Pan Left, Right, Top, Down Canvas

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanPanLeft()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (Vm.PanCommand.CanExecute("Left"))
                    Vm.PanCommand.Execute("Left");
            }

            Assert.Greater(workspaceModel.X, posX);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanPanRight()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (Vm.PanCommand.CanExecute("Right"))
                    Vm.PanCommand.Execute("Right");
            }

            Assert.Greater(posX, workspaceModel.X);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanPanUp()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (Vm.PanCommand.CanExecute("Up"))
                    Vm.PanCommand.Execute("Up");
            }

            Assert.AreEqual(posX, workspaceModel.X);
            Assert.Greater(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanPanDown()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (Vm.PanCommand.CanExecute("Down"))
                    Vm.PanCommand.Execute("Down");
            }

            Assert.AreEqual(posX, workspaceModel.X);
            Assert.Greater(posY, workspaceModel.Y);
        }

        #endregion

        #region Fit to View

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void FitViewWithNoNodes()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            // Zoom to max zoom value
            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);
            
            // Check for no changes
            Assert.AreEqual(workspaceModel.Zoom, initZoom);
            Assert.AreEqual(workspaceModel.X, initX);
            Assert.AreEqual(workspaceModel.Y, initY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanFitView()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check for no changes
            Assert.AreNotEqual(workspaceModel.Zoom, initZoom);
            Assert.AreNotEqual(workspaceModel.X, initX);
            Assert.AreNotEqual(workspaceModel.Y, initY);

            Controller.DynamoViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanFitViewTwiceForActualZoom()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            // Zoom to max zoom value
            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check for no changes
            Assert.AreNotEqual(workspaceModel.Zoom, initZoom);
            Assert.AreNotEqual(workspaceModel.X, initX);
            Assert.AreNotEqual(workspaceModel.Y, initY);

            Controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void FitViewStressTest()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;
            
            double initZoom = workspaceModel.Zoom;
            double initX = workspaceModel.X;
            double initY = workspaceModel.Y;

            CreateNodeOnCurrentWorkspace();

            for (int i = 0; i < 100; i++)
            {
                // Zoom to max zoom value
                if (workspaceVM.FitViewCommand.CanExecute(null))
                    workspaceVM.FitViewCommand.Execute(null);
            }

            // Not crashed
            Assert.True(true);

            Controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanFitViewResetByZoom()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some zoom action before FitView again
            Vm.ZoomIn(null);

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            Controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void CanFitViewResetByPan()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some pan action before FitView again
            Vm.Pan("Up" as object);

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            Controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        // Add a number node on workspace
        private void CreateNodeOnCurrentWorkspace()
        {
            // Create number node
            Controller.DynamoModel.CreateNode(Guid.NewGuid(), "Number", 0, 0, true, false);
            var numNode = Controller.DynamoViewModel.Model.Nodes[0];

            // Add to current workspace
            Vm.CurrentSpaceViewModel.Model.Nodes.Add(numNode);
            int nodeIndex = Vm.CurrentSpaceViewModel.Model.Nodes.Count - 1;
            Vm.CurrentSpaceViewModel.Model.Nodes[nodeIndex].X = 100;
            Vm.CurrentSpaceViewModel.Model.Nodes[nodeIndex].Y = 100;
        }

        #endregion

        #region PreferenceSettings
        [Test, RequiresSTA]
        [Category("DynamoUI"), Category("Failing")]
        public void PreferenceSetting()
        {
            // Test Case to ensure that the link for these persistent variable
            // between DynamoViewModel, Controller is not broken or replaced.
            #region FullscreenWatchShowing
            bool expectedValue = !Controller.PreferenceSettings.FullscreenWatchShowing;
            Vm.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, Controller.PreferenceSettings.FullscreenWatchShowing);

            expectedValue = !Controller.PreferenceSettings.FullscreenWatchShowing;
            Vm.ToggleFullscreenWatchShowing(null);
            Assert.AreEqual(expectedValue, Controller.PreferenceSettings.FullscreenWatchShowing);
            #endregion

            #region ShowConsole
            expectedValue = !Controller.PreferenceSettings.ShowConsole;
            Vm.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedValue, Controller.PreferenceSettings.ShowConsole);

            expectedValue = !Controller.PreferenceSettings.ShowConsole;
            Vm.ToggleConsoleShowing(null);
            Assert.AreEqual(expectedValue, Controller.PreferenceSettings.ShowConsole);
            #endregion

            #region ConnectorType
            ConnectorType expectedConnector = ConnectorType.BEZIER;
            Vm.SetConnectorType("BEZIER");
            Assert.AreEqual(expectedConnector, Controller.PreferenceSettings.ConnectorType);

            expectedConnector = ConnectorType.POLYLINE;
            Vm.SetConnectorType("POLYLINE");
            Assert.AreEqual(expectedConnector, Controller.PreferenceSettings.ConnectorType);
            #endregion

            #region Collect Information Option
            // First time run, check if dynamo did set it back to false after running
            Assert.AreEqual(false, UsageReportingManager.Instance.FirstRun);

            // CollectionInfoOption To TRUE
            UsageReportingManager.Instance.SetUsageReportingAgreement(true);
            RestartTestSetup();
            Assert.AreEqual(true, UsageReportingManager.Instance.IsUsageReportingApproved);

            // CollectionInfoOption To FALSE
            UsageReportingManager.Instance.SetUsageReportingAgreement(false);
            RestartTestSetup();
            Assert.AreEqual(false, UsageReportingManager.Instance.IsUsageReportingApproved);
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
            initalSetting.ShowConsole = true;
            initalSetting.FullscreenWatchShowing = true;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.FullscreenWatchShowing, initalSetting.FullscreenWatchShowing);
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ShowConsole, initalSetting.ShowConsole);
            #endregion

            #region Second Test
            initalSetting.ConnectorType = ConnectorType.POLYLINE;
            initalSetting.ShowConsole = false;
            initalSetting.FullscreenWatchShowing = false;

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.FullscreenWatchShowing, initalSetting.FullscreenWatchShowing);
            Assert.AreEqual(resultSetting.ConnectorType, initalSetting.ConnectorType);
            Assert.AreEqual(resultSetting.ShowConsole, initalSetting.ShowConsole);
            #endregion

            #endregion
            
            Ui.Close();
        }

        private void RestartTestSetup()
        {
            // Shutdown Dynamo and restart it
            Ui.Close();
            if (Controller != null)
            {
                Controller.ShutDown(false);
                Controller = null;
            }

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DYNAMO_TEST_PATH = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            Controller = DynamoController.MakeSandbox();
            DynamoController.IsTestMode = true;

            //create the view
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
        #endregion

        #region InfoBubble

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void UpdateInfoBubble_LibItem()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_LibItem = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.LibraryItemPreview, 
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Left);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_LibItem);
                Assert.AreEqual(content, infoBubble.Content);
                Assert.AreEqual(InfoBubbleViewModel.Style.LibraryItemPreview, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Left, infoBubble.ConnectingDirection);
            }
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void UpdateInfoBubble_NodeTooltip()
        {
            var infoBubble = new InfoBubbleViewModel();
            string content = "This is the test infoBubble";
            var inputData_NodeTooltip = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Right);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_NodeTooltip);
                Assert.AreEqual(content, infoBubble.Content);
                Assert.AreEqual(InfoBubbleViewModel.Style.NodeTooltip, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Right, infoBubble.ConnectingDirection);
            }
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void UpdateInfoBubble_ErrorBubble()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
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
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            infoBubble.OnRequestAction(new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));
            //Assert.AreEqual(0, infoBubble.Opacity);
        }

	    #endregion

        #region Notes

        [Test]
        [Category("DynamoUI")]
        public void CanCreateANote()
        {
            Vm.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);
            
            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count);

            Vm.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test]
        [Category("DynamoUI"), Category("Failing")]
        public void CanDeleteANote()
        {
            Vm.AddNoteCommand.Execute(null);
            var note = Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, Model.CurrentWorkspace.Notes.Count);

            //select the note for deletion
            DynamoSelection.Instance.Selection.Add(note);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            //delete the note
            Vm.DeleteCommand.Execute(null);
            Assert.AreEqual(0,Model.CurrentWorkspace.Notes.Count);

            Vm.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }


        #endregion

        //[Test]
        //public void CrashPresentsSaveAs()
        //{
        //    dynSettings.Controller.IsCrashing = true;
        //    dynSettings.Controller.DynamoModel.HomeSpace.HasUnsavedChanges = true;
        //    dynSettings.Controller.DynamoViewModel.Exit(false);
        //}
    }
}
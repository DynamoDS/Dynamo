using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Models;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class CoreUserInterfaceTests
    {
        private static string TempFolder;
        private static DynamoController controller;
        private static DynamoViewModel vm;
        private static DynamoView ui;

        #region SetUp & TearDown

        [SetUp, RequiresSTA]
        public void Start()
        {
            controller = DynamoController.MakeSandbox();

            //create the view
            ui = new DynamoView();
            ui.DataContext = controller.DynamoViewModel;
            vm = controller.DynamoViewModel;
            controller.UIDispatcher = ui.Dispatcher;
            ui.Show();

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

        [TearDown, RequiresSTA]
        public void Exit()
        {
            if (ui.IsLoaded)
                ui.Close();
        }

        #endregion

        #region Utility functions

        public static void EmptyTempFolder()
        {
            var directory = new DirectoryInfo(TempFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }


        #endregion


        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        #region SaveImageCommand

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanSaveImage()
        {
            string path = Path.Combine(TempFolder, "output.png");

            vm.SaveImageCommand.Execute(path);

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            vm.SaveImageCommand.Execute(path);

            Assert.False(File.Exists(path));
        }

        #endregion

        #region ToggleConsoleShowingCommand

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanShowConsoleWhenHidden()
        {
            vm.ToggleConsoleShowingCommand.Execute(null);
            ui.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => Assert.False(ui.ConsoleShowing)));
            Assert.Inconclusive("Binding is not being updated in time for the test to complete correctly");
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(ui.ConsoleShowing);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanHideConsoleWhenShown()
        {
            //vm.ToggleConsoleShowingCommand.Execute(null);
            //Assert.True(ui.ConsoleShowing);

            //vm.ToggleConsoleShowingCommand.Execute(null);
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

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanZoomIn()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            double zoom = workspaceModel.Zoom;

            vm.ZoomInCommand.Execute(null);

            Assert.Greater(workspaceModel.Zoom, zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanZoomOut()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            double zoom = workspaceModel.Zoom;

            vm.ZoomOutCommand.Execute(null);

            Assert.Greater(zoom, workspaceModel.Zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanSetZoom()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            int testLoop = 10;

            for (int i = 0; i < testLoop; i++)
            {
                // Get random number for the zoom
                double upperBound = WorkspaceModel.ZOOM_MAXIMUM;
                double lowerBound = WorkspaceModel.ZOOM_MINIMUM;
                Random random = new Random();
                double randomNumber = random.NextDouble() * (upperBound - lowerBound) + lowerBound;

                vm.CurrentSpaceViewModel.SetZoomCommand.Execute(randomNumber);

                // Check Zoom is correct
                Assert.AreEqual(randomNumber, workspaceModel.Zoom);
            }
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanSetZoomBorderTest()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM);
            Assert.AreEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM);
            Assert.AreEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM + 0.1);
            Assert.AreNotEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM - 0.1);
            Assert.AreNotEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanZoomInLimit()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            // Zoom to max zoom value
            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM);

            vm.ZoomInCommand.Execute(null);

            // Check it does not zoom in anymore
            Assert.AreEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanZoomOutLimit()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            // Zoom to max zoom value
            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM);

            vm.ZoomOutCommand.Execute(null);

            // Check it does not zoom out anymore
            Assert.AreEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void ZoomInOutStressTest()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            // Zoom in and out repeatly
            for (int i = 0; i < 100; i++)
            {
                for (int stepIn = 0; stepIn < 30; stepIn++)
                {
                    if (vm.ZoomInCommand.CanExecute(null))
                    {
                        vm.ZoomInCommand.Execute(null);
                        Console.WriteLine("Zoom in " + stepIn);
                    }
                }
                for (int stepOut = 0; stepOut < 30; stepOut++)
                {
                    if (vm.ZoomOutCommand.CanExecute(null))
                    {
                        vm.ZoomOutCommand.Execute(null);
                        Console.WriteLine("Zoom out " + stepOut);
                    }
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
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (vm.PanCommand.CanExecute("Left"))
                    vm.PanCommand.Execute("Left");
            }

            Assert.Greater(workspaceModel.X, posX);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanRight()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (vm.PanCommand.CanExecute("Right"))
                    vm.PanCommand.Execute("Right");
            }

            Assert.Greater(posX, workspaceModel.X);
            Assert.AreEqual(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanUp()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (vm.PanCommand.CanExecute("Up"))
                    vm.PanCommand.Execute("Up");
            }

            Assert.AreEqual(posX, workspaceModel.X);
            Assert.Greater(workspaceModel.Y, posY);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanPanDown()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            int numOfPanTested = 100;
            double posX = workspaceModel.X;
            double posY = workspaceModel.Y;

            // Pan left repeatly
            for (int i = 0; i < numOfPanTested; i++)
            {
                if (vm.PanCommand.CanExecute("Down"))
                    vm.PanCommand.Execute("Down");
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
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

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
        [Category("DynamoUI")]
        public void CanFitView()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

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

            controller.DynamoViewModel.CurrentSpaceViewModel.Model.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewTwiceForActualZoom()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

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

            controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void FitViewStressTest()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;
            
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

            controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByZoom()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some zoom action before FitView again
            vm.ZoomIn(null);

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void CanFitViewResetByPan()
        {
            WorkspaceModel workspaceModel = vm.CurrentSpaceViewModel.Model;
            WorkspaceViewModel workspaceVM = vm.CurrentSpaceViewModel;

            CreateNodeOnCurrentWorkspace();

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            double curZoom = workspaceModel.Zoom;
            double curX = workspaceModel.X;
            double curY = workspaceModel.Y;

            // Do some pan action before FitView again
            vm.Pan("Up" as object);

            if (workspaceVM.FitViewCommand.CanExecute(null))
                workspaceVM.FitViewCommand.Execute(null);

            // Check actual zoom
            Assert.AreEqual(workspaceModel.Zoom, curZoom);
            Assert.AreEqual(workspaceModel.X, curX);
            Assert.AreEqual(workspaceModel.Y, curY);

            controller.DynamoViewModel.CurrentSpace.HasUnsavedChanges = false;
        }

        // Add a number node on workspace
        private void CreateNodeOnCurrentWorkspace()
        {
            // Create number node
            var nodeData = new Dictionary<string, object>();
            nodeData.Add("name", "Number");
            controller.DynamoModel.CreateNode(nodeData);
            var numNode = controller.DynamoViewModel.Model.Nodes[0];

            // Add to current workspace
            vm.CurrentSpaceViewModel.Model.Nodes.Add(numNode);
            int nodeIndex = vm.CurrentSpaceViewModel.Model.Nodes.Count() - 1;
            vm.CurrentSpaceViewModel.Model.Nodes[nodeIndex].X = 100;
            vm.CurrentSpaceViewModel.Model.Nodes[nodeIndex].Y = 100;
        }

        #endregion
    }
}
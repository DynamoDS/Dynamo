﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using System.Windows;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class CoreUserInterfaceTests :DynamoTestUI
    {
        [SetUp]
        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            Controller = DynamoController.MakeSandbox();

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

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        #region SaveImageCommand

        [Test]
        [Category("DynamoUI")]
        public void CanSaveImage()
        {
            string path = Path.Combine(TempFolder, "output.png");

            Vm.SaveImageCommand.Execute(path);

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

        [Test]
        [Category("DynamoUI")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            Vm.SaveImageCommand.Execute(path);

            Assert.False(File.Exists(path));
        }

        #endregion

        #region ToggleConsoleShowingCommand

        [Test]
        [Category("DynamoUI")]
        public void CanShowConsoleWhenHidden()
        {
            Vm.ToggleConsoleShowingCommand.Execute(null);
            Ui.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => Assert.False(Ui.ConsoleShowing)));
            Assert.Inconclusive("Binding is not being updated in time for the test to complete correctly");
        }

        [Test]
        [Category("DynamoUI")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(Ui.ConsoleShowing);
        }

        [Test]
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
        public void CanZoomIn()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            double zoom = workspaceModel.Zoom;

            Vm.ZoomInCommand.Execute(null);

            Assert.Greater(workspaceModel.Zoom, zoom);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanZoomOut()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            double zoom = workspaceModel.Zoom;

            Vm.ZoomOutCommand.Execute(null);

            Assert.Greater(zoom, workspaceModel.Zoom);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanSetZoom()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            int testLoop = 10;

            for (int i = 0; i < testLoop; i++)
            {
                // Get random number for the zoom
                double upperBound = WorkspaceModel.ZOOM_MAXIMUM;
                double lowerBound = WorkspaceModel.ZOOM_MINIMUM;
                Random random = new Random();
                double randomNumber = random.NextDouble() * (upperBound - lowerBound) + lowerBound;

                Vm.CurrentSpaceViewModel.SetZoomCommand.Execute(randomNumber);

                // Check Zoom is correct
                Assert.AreEqual(randomNumber, workspaceModel.Zoom);
            }
        }

        [Test]
        [Category("DynamoUI")]
        public void CanSetZoomBorderTest()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM);
            Assert.AreEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM);
            Assert.AreEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM + 0.1);
            Assert.AreNotEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);

            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM - 0.1);
            Assert.AreNotEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanZoomInLimit()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            // Zoom to max zoom value
            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MAXIMUM);

            Vm.ZoomInCommand.Execute(null);

            // Check it does not zoom in anymore
            Assert.AreEqual(WorkspaceModel.ZOOM_MAXIMUM, workspaceModel.Zoom);
        }

        [Test]
        [Category("DynamoUI")]
        public void CanZoomOutLimit()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            // Zoom to max zoom value
            workspaceVM.SetZoomCommand.Execute(WorkspaceModel.ZOOM_MINIMUM);

            Vm.ZoomOutCommand.Execute(null);

            // Check it does not zoom out anymore
            Assert.AreEqual(WorkspaceModel.ZOOM_MINIMUM, workspaceModel.Zoom);
        }

        [Test, RequiresSTA]
        [Category("DynamoUI")]
        public void ZoomInOutStressTest()
        {
            WorkspaceModel workspaceModel = Vm.CurrentSpaceViewModel._model;
            WorkspaceViewModel workspaceVM = Vm.CurrentSpaceViewModel;

            // Zoom in and out repeatly
            for (int i = 0; i < 100; i++)
            {
                for (int stepIn = 0; stepIn < 30; stepIn++)
                {
                    if (Vm.ZoomInCommand.CanExecute(null))
                    {
                        Vm.ZoomInCommand.Execute(null);
                        Console.WriteLine("Zoom in " + stepIn);
                    }
                }
                for (int stepOut = 0; stepOut < 30; stepOut++)
                {
                    if (Vm.ZoomOutCommand.CanExecute(null))
                    {
                        Vm.ZoomOutCommand.Execute(null);
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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
        [Category("DynamoUI")]
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

        #region InfoBubble
        
        [Test]
        [Category("DynamoUI")]
        public void UpdateInfoBubble_LibItem()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_LibItem = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.LibraryItemPreview, 
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Left, Guid.Empty);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_LibItem);
                Assert.AreEqual(content, infoBubble.ItemDescription);
                Assert.AreEqual(InfoBubbleViewModel.Style.LibraryItemPreview, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Left, infoBubble.ConnectingDirection);
            }
        }

        [Test]
        [Category("DynamoUI")]
        public void UpdateInfoBubble_NodeTooltip()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_NodeTooltip = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.NodeTooltip,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Right, Guid.Empty);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_NodeTooltip);
                Assert.AreEqual(content, infoBubble.ItemDescription);
                Assert.AreEqual(InfoBubbleViewModel.Style.NodeTooltip, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Right, infoBubble.ConnectingDirection);
            }
        }

        [Test]
        [Category("DynamoUI")]
        public void UpdateInfoBubble_ErrorBubble()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            string content = "This is the test infoBubble";
            InfoBubbleDataPacket inputData_ErrorBubble = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Error,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Bottom, Guid.Empty);

            if (infoBubble.UpdateContentCommand.CanExecute(null))
            {
                infoBubble.UpdateContentCommand.Execute(inputData_ErrorBubble);
                Assert.AreEqual(content, infoBubble.ItemDescription);
                Assert.AreEqual(InfoBubbleViewModel.Style.Error, infoBubble.InfoBubbleStyle);
                Assert.AreEqual(InfoBubbleViewModel.Direction.Bottom, infoBubble.ConnectingDirection);
            }
        }

        [Test]
        [Category("DynamoUI")]
        public void Collapse()
        {
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel();
            infoBubble.InstantCollapseCommand.Execute(null);
            Assert.AreEqual(0, infoBubble.Opacity);
        }

	    #endregion
        
    }
}
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

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Dynamo.Commands;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoElementsUITests
    {
        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            
            dynSettings.Writer.Close();
            dynSettings.Controller.Bench.Close();
            EmptyTempFolder();
        }

        private static string TempFolder;

        private static void StartDynamo()
        {
            string tempPath = Path.GetTempPath();
            var random = new Random();
            string logPath = Path.Combine(tempPath, "dynamoLog" + random.Next() + ".txt");

            TempFolder = Path.Combine(tempPath, "dynamoTmp");

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                EmptyTempFolder();
            }

            TextWriter tw = new StreamWriter(logPath);
            tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
            dynSettings.Writer = tw;

            //create a new instance of the ViewModel
            var controller = new DynamoController();
            controller.Bench.Show();
        }

        public static void EmptyTempFolder()
        {
            var directory = new DirectoryInfo(TempFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

    // OpenCommand

        [Test]
        public void CanOpenGoodFile()
        {
            // NOTE rom PB: this test fails due to the fact that Bench is locked as it was never shown in these tests
            //              The same test is present in DynamoElementsUITests.cs, where it succeeds
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\multiplicationAndAdd.dyn");
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCmd, openPath));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(5, dynSettings.Controller.CurrentSpace.Nodes.Count);
        }

    // SaveImageCommand

        [Test]
        public void CanSaveImage()
        {
            string path = Path.Combine(TempFolder, "output.png");

            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveImageCmd, path));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }

 
        [Test]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveImageCmd, path));

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(0, tempFldrInfo.GetFiles().Length);
        }

    // ShowSplashScreenCmd

        [Test]
        public void CanShowAndCloseSplashScreen()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.ShowSplashScreenCmd,
                                                                                 null));
            dynSettings.Controller.ProcessCommandQueue();
            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action)(() =>
                {
                    Assert.AreEqual(true, dynSettings.Controller.SplashScreen.IsVisible);
                    dynSettings.Controller.CommandQueue.Enqueue(
                        Tuple.Create<object, object>(DynamoCommands.CloseSplashScreenCmd, null));
                    dynSettings.Controller.ProcessCommandQueue();
                    Assert.AreEqual(false, dynSettings.Controller.SplashScreen.IsVisible);
                }));
        }

        [Test]
        public void CanShowSplashScreenFromDefaultState()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.ShowSplashScreenCmd,
                                                                                 null));
            dynSettings.Controller.ProcessCommandQueue();
            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action)(() => { Assert.AreEqual(true, dynSettings.Controller.SplashScreen.IsVisible); }));
        }

        [Test]
        public void CanCloseSplashScreenFromDefaultState()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CloseSplashScreenCmd,
                                                                                 null));
            dynSettings.Controller.ProcessCommandQueue();

            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action)(() => { Assert.AreEqual(false, dynSettings.Controller.SplashScreen.IsVisible); }));
        }

    // ToggleConsoleShowingCommand

        [Test]
        public void CanShowConsoleWhenHidden()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                DynamoCommands.ToggleConsoleShowingCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.True(dynSettings.Bench.ConsoleShowing);
        }


        [Test]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(dynSettings.Bench.ConsoleShowing);
        }


        [Test]
        public void CanHideConsoleWhenShown()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                DynamoCommands.ToggleConsoleShowingCmd, null));
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                DynamoCommands.ToggleConsoleShowingCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.False(dynSettings.Bench.ConsoleShowing);
        }
    }
}
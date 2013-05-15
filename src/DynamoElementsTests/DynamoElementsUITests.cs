using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Commands;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoElementsUiTests
    {
        private DynamoViewModel _vm;

        [SetUp]
        [Category("DynamoUI")]
        public void Init()
        {
            StartDynamo();
            _vm = dynSettings.Controller.DynamoViewModel;

        }

        [TearDown]
        [Category("DynamoUI")]
        public void Cleanup()
        {
            //dynSettings.Writer.Close();
            //dynSettings.Controller.Bench.Close();
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

            //create a new instance of the ViewModel
            //var controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment());
            //controller.Bench.Show();
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
        [Category("DynamoUI")]
        public void CanOpenGoodFile()
        {
            // NOTE rom PB: this test fails due to the fact that Bench is locked as it was never shown in these tests
            //              The same test is present in DynamoElementsUITests.cs, where it succeeds
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\multiplicationAndAdd.dyn");
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(_vm.OpenCommand, openPath));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(5, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        // SaveImageCommand

        [Test]
        [Category("DynamoUI")]
        public void CanSaveImage()
        {
            string path = Path.Combine(TempFolder, "output.png");

            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(_vm.SaveImageCommand, path));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.True(File.Exists(path));
            File.Delete(path);
            Assert.False(File.Exists(path));
        }


        [Test]
        [Category("DynamoUI")]
        public void CannotSaveImageWithBadPath()
        {
            string path = "W;\aelout put.png";

            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(_vm.SaveImageCommand, path));

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(0, tempFldrInfo.GetFiles().Length);
        }

        // ToggleConsoleShowingCommand
        [Test]
        [Category("DynamoUI")]
        public void CanShowConsoleWhenHidden()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                _vm.ToggleConsoleShowingCommand, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.True(dynSettings.Bench.ConsoleShowing);
        }


        [Test]
        [Category("DynamoUI")]
        public void ConsoleIsHiddenOnOpen()
        {
            Assert.False(dynSettings.Bench.ConsoleShowing);
        }


        [Test]
        [Category("DynamoUI")]
        public void CanHideConsoleWhenShown()
        {
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
               _vm.ToggleConsoleShowingCommand, null));
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                _vm.ToggleConsoleShowingCommand, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.False(dynSettings.Bench.ConsoleShowing);
        }


        [Test]
        [Category("DynamoUI")]
        public void CanOpenAllSampleFilesWithoutError()
        {
            var di = new DirectoryInfo(@"..\..\doc\Distrib\Samples\");
            int failCount = 0;

            foreach (DirectoryInfo d in di.GetDirectories())
            {

                foreach (FileInfo fi in d.GetFiles())
                {
                    try
                    {
                        dynSettings.Controller.CommandQueue.Enqueue(
                            Tuple.Create<object, object>(_vm.OpenCommand, fi.FullName));
                        dynSettings.Controller.ProcessCommandQueue();
                    }
                    catch
                    {
                        failCount++;
                        Console.WriteLine(string.Format("Could not open {0}", fi.FullName));
                    }
                }
            }
            Assert.AreEqual(failCount, 0);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;

using Dynamo.Controls;
using Dynamo.Commands;
using Dynamo.Utilities;
using Dynamo.Nodes;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoElementsTests
    {
        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        private static string TempFolder;

        private static void StartDynamo()
        {
            string tempPath = Path.GetTempPath();
            string logPath = Path.Combine(tempPath, "dynamoLog.txt");

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
            DynamoController controller = new DynamoController();
            controller.Bench.Show();
        }

        public static void EmptyTempFolder()
        {
            var directory = new DirectoryInfo(TempFolder);
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        [TearDown]
        public void Cleanup()
        {
            EmptyTempFolder();
            dynSettings.Controller.Bench.Close();
        }

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        [Test]
        public void CanAddANote()
        {
            //create some test note data
            Dictionary<string, object> inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", dynSettings.Controller.CurrentSpace);

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.AddNoteCmd, inputs));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddANodeByName()
        {
            Dictionary<string, object> sumData = new Dictionary<string, object>();
            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "+");
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        public void CanSumTwoNumbers()
        {

            Dictionary<string, object> sumData = new Dictionary<string, object>();
            Dictionary<string, object> numData1 = new Dictionary<string, object>();
            Dictionary<string, object> numData2 = new Dictionary<string, object>();

            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "+");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData1));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData2));
            dynSettings.Controller.ProcessCommandQueue();

            //update the layout so the following
            //connectors have visuals to transform to
            //we were experiencing a problem in tests with TransfromToAncestor
            //calls not being valid because entities weren't in the tree yet.
            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);


            dynDoubleInput num1 = dynSettings.Controller.Nodes[1] as dynDoubleInput;
            num1.Value = 2;
            dynDoubleInput num2 = dynSettings.Controller.Nodes[2] as dynDoubleInput;
            num2.Value = 2;

            Dictionary<string, object> cd1 = new Dictionary<string, object>();
            cd1.Add("start", dynSettings.Controller.Nodes[1].NodeUI);
            cd1.Add("end", dynSettings.Controller.Nodes[0].NodeUI);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, cd1));
            Dictionary<string,object> cd2 = new Dictionary<string,object>();
            cd2.Add("start", dynSettings.Controller.Nodes[2].NodeUI);    //first number node
            cd2.Add("end", dynSettings.Controller.Nodes[0].NodeUI);    //+ node
            cd2.Add("port_start",0);  //first output
            cd2.Add("port_end",1);  //second input

            dynSettings.Bench.LogText = "";

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, cd2));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.RunExpressionCmd, null));
            dynSettings.Controller.ProcessCommandQueue();

            //validate that the expression for addition is as expected
            Assert.AreEqual((dynSettings.Controller.Nodes[0] as dynNode).PrintExpression(), "(+ 2 2)");

            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);
            
            Assert.AreEqual(dynSettings.Controller.Nodes.Count, 3);
        }

    // Splash screen

        [Test]
        public void CanShowSplashScreenFromDefaultState()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.ShowSplashScreenCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action)(() =>
                {
                    Assert.AreEqual(true, dynSettings.Controller.SplashScreen.IsVisible);
                }));

        }

        [Test]
        public void CanCloseSplashScreenFromDefaultState()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CloseSplashScreenCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action)(() =>
                {
                    Assert.AreEqual(false, dynSettings.Controller.SplashScreen.IsVisible);
                }));

        }

        [Test]
        public void CanShowAndCloseSplashScreen()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.ShowSplashScreenCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            dynSettings.Controller.SplashScreen.Dispatcher.Invoke(
                (Action) (() =>
                    {
                        Assert.AreEqual(true, dynSettings.Controller.SplashScreen.IsVisible);
                        dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CloseSplashScreenCmd, null));
                        dynSettings.Controller.ProcessCommandQueue();
                        Assert.AreEqual(false, dynSettings.Controller.SplashScreen.IsVisible);
                    }));
            
        }

    // Log

        [Test]
        public void CanClearLog()
        {
            
            Assert.AreNotEqual( 0, dynSettings.Bench.LogText.Length );
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.ClearLogCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.AreEqual(0, dynSettings.Bench.LogText.Length);
            
        }

    // Clearworkspace 

        [Test]
        public void CanClearWorkspaceWithNodes()
        {

            Dictionary<string, object> sumData = new Dictionary<string, object>();
            Dictionary<string, object> numData1 = new Dictionary<string, object>();
            Dictionary<string, object> numData2 = new Dictionary<string, object>();

            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "+");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            Assert.AreEqual(0, dynSettings.Controller.Nodes.Count());

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData1));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData2));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(3, dynSettings.Controller.Nodes.Count());

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.ClearCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.AreEqual(0, dynSettings.Controller.Nodes.Count());

        }

        [Test]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.ClearCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.AreEqual(0, dynSettings.Controller.Nodes.Count());
            
        }

        // LayoutAll

        [Test]
        public void CanLayoutAll()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.LayoutAllCmd, null));
            dynSettings.Controller.ProcessCommandQueue();
            Assert.AreNotEqual(0, dynSettings.Controller.Nodes.Count() );
        }

    // SaveImage

        [Test]
        public void CanSaveImage()
        {
            var path = Path.Combine(TempFolder, "output.png");

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveImageCmd, path));
            dynSettings.Controller.ProcessCommandQueue();
            
            Assert.AreEqual( true, File.Exists(path) );
            File.Delete(path); 
            Assert.AreEqual( false, File.Exists(path) );
            
        }

        [Test]
        public void CannotSaveImageWithBadPath()
        {
            var path = "W;\aelout put.png";

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveImageCmd, path));

            DirectoryInfo tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(0, tempFldrInfo.GetFiles().Length );

        }

    // HomeCommand

        [Test]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            for (int i = 0; i < 20; i++)
            {
                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.HomeCmd, null));
                Assert.AreEqual(true, dynSettings.Controller.ViewingHomespace);
            }
        }

        //[Test]
        //public void CanGoHomeWhenInDifferentWorkspace()
        //{
        //    // move to different workspace
        //    // go home
        //    // need to create new function via command
        //    //TODO: loadWorkspaceFromFileCommand
        //}


    // OpenCommand

        [Test]
        public void CanOpenGoodFile()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\multiplicationAndAdd.dyn");
            
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.OpenCmd, openPath));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(5, dynSettings.Controller.Nodes.Count());
        }

        //[Test]
        //public void CanHandleBadFileWhenOpening()
        //{
        // TODO: create bad file for opening
        //}

    // SaveAsCommand

        [Test]
        public void CanSaveAsEmptyFile()
        {
            var fn = "ruthlessTurtles.dyn";
            var path = Path.Combine(TempFolder, fn);
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveAsCmd, path ));
            dynSettings.Controller.ProcessCommandQueue();

            DirectoryInfo tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            var numNodes = 100;

            for (var i = 0; i < numNodes; i++)
            {
                Dictionary<string, object> sumData = new Dictionary<string, object>();
                sumData.Add("name", "+");
                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
                dynSettings.Controller.ProcessCommandQueue();

                Assert.AreEqual( i+1, dynSettings.Controller.CurrentSpace.Nodes.Count );
            }

            var fn = "ruthlessTurtles.dyn";
            var path = Path.Combine(TempFolder, fn);
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveAsCmd, path));
            dynSettings.Controller.ProcessCommandQueue();

            DirectoryInfo tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);

        }

    // SaveCommand

        [Test]
        public void CanSaveEmptyFile()
        {
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveCmd, null));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual( true, File.Exists(dynSettings.Controller.CurrentSpace.FilePath) );

        }

        [Test]
        public void CanSaveFileWithNodesInIt()
        {
            var numNodes = 100;

            for (var i = 0; i < numNodes; i++)
            {
                Dictionary<string, object> sumData = new Dictionary<string, object>();
                sumData.Add("name", "+");
                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
                dynSettings.Controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, dynSettings.Controller.CurrentSpace.Nodes.Count);
            }

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.SaveCmd, null));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(true, File.Exists(dynSettings.Controller.CurrentSpace.FilePath));
            File.Delete(dynSettings.Controller.CurrentSpace.FilePath);
            Assert.AreEqual(false, File.Exists(dynSettings.Controller.CurrentSpace.FilePath));
        }

        //// CancelRunCommand

        //[Test]
        //public void CanCancelRun()
        //{


        //}

        // ToggleConsoleShowingCommand

        //[Test]
        //public void CanShowConsoleWhenHidden()
        //{
            
        //}

        //[Test]
        //public void CanHideConsoleWhenShown()
        //{


        //}

        //// AddToSelectionCommand

        //[Test]
        //public void CanAddToSelectionCommand()
        //{


        //}

        //[Test]
        //public void CanMaintainSelectionWhenNodesAlreadySelected()
        //{


        //}

        //// SelectCommand

        //[Test]
        //public void CanSelectNodes()
        //{
            
        //}

        //[Test]
        //public void CanSelectNothingWhenNoNodesPresent()
        //{
            
        //}

        //// CopyCommand
        //// PasteCommand

        //[Test]
        //public void CanCopyNodes()
        //{

        //}

        //[Test]
        //public void CanCopyAndPasteNodes()
        //{

        //}

        //// RunExpressionCommand

        //[Test]
        //public void CanRunExpression()
        //{

        //}

        //// CreateConnectionCommand

        //[Test]
        //public void CanCreateConnection()
        //{

        //}

        
    }
}

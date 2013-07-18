using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using System.Xml;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoCoreTests
    {
        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                controller.ShutDown();

                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static DynamoController controller;
        private static string TempFolder;

        private static void StartDynamo()
        {
            try
            {

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

                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        // TODO: create set of sample files with no Revit dependencies
        //[Test]
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
        //                controller.CommandQueue.Enqueue(
        //                    Tuple.Create<object, object>(controller.DynamoViewModel.OpenCommand, fi.FullName));
        //                controller.ProcessCommandQueue();
        //            }
        //            catch (Exception e)
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

        // OpenCommand
        [Test]
        public void CanOpenGoodFile()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\multiplicationAndAdd\multiplicationAndAdd.dyn");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCommand, openPath));
            //DynamoCommands.ProcessCommandQueue();
            vm.Open(openPath);

            Assert.AreEqual(5, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }


        [Test]
        public void CanAddANodeByName()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            var sumData = new Dictionary<string, object>();
            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);

            sumData.Add("name", "Add");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
            //DynamoCommands.ProcessCommandQueue();
            vm.CreateNode(sumData);

            Assert.AreEqual(controller.DynamoViewModel.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        public void CanAddANote()
        {
            //create some test note data
            var inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", controller.DynamoViewModel.CurrentSpace);

            DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddNoteCommand, inputs));
            DynamoCommands.ProcessCommandQueue();

            Assert.AreEqual(controller.DynamoViewModel.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                                                                                     null));
                Assert.DoesNotThrow(() => DynamoCommands.ProcessCommandQueue());

                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand, 5));
                Assert.DoesNotThrow(() => DynamoCommands.ProcessCommandQueue());

                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                                                                                     "noodle"));
                Assert.DoesNotThrow(() => DynamoCommands.ProcessCommandQueue());

                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                                                                                     new StringBuilder()));
                Assert.DoesNotThrow(() => DynamoCommands.ProcessCommandQueue());
            }
        }

        [Test]
        public void CanAddToSelectionCommand()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        public void CanClearLog()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            Assert.AreNotEqual(0, controller.DynamoViewModel.LogText.Length);
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.ClearLogCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.ClearLog();

            Assert.AreEqual(0, controller.DynamoViewModel.LogText.Length);
        }

        // Clearworkspace 

        [Test]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.ClearCommand, null));
            DynamoCommands.ProcessCommandQueue();
            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        [Test]
        public void CanClearWorkspaceWithNodes()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            var sumData = new Dictionary<string, object>();
            var numData1 = new Dictionary<string, object>();
            var numData2 = new Dictionary<string, object>();

            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "Add");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, numData1));
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, numData2));
            //DynamoCommands.ProcessCommandQueue();
            vm.CreateNode(sumData);
            vm.CreateNode(numData1);
            vm.CreateNode(numData2);

            Assert.AreEqual(3, controller.DynamoViewModel.Model.Nodes.Count());

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.ClearCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.Clear();

            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        [Test]
        public void CanAdd100NodesToClipboard()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                DynamoCommands.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                DynamoCommands.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            DynamoCommands.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }

        [Test]
        public void CanAdd1NodeToClipboardAndPaste()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");

                dynSettings.Controller.DynamoViewModel.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                DynamoCommands.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            vm.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
            vm.Paste(null);

            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.PasteCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.Paste(null);
            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste3Times()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);
                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);


                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.PasteCommand, null));
                //DynamoCommands.ProcessCommandQueue();
                vm.Paste(null);

                Assert.AreEqual(numNodes, controller.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }
        }

        [Test]
        public void CanAddOneNodeToClipboard()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }


        // LayoutAll

        [Test]
        public void CanLayoutAll()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.LayoutAllCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.LayoutAll();

            Assert.AreNotEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        // SaveImage

        //[Test]
        //public void CanGoHomeWhenInDifferentWorkspace()
        //{
        //    // move to different workspace
        //    // go home
        //    // need to create new function via command
        //    //TODO: loadWorkspaceFromFileCommand
        //}

        // SaveAsCommand

        [Test]
        public void CanSaveAsEmptyFile()
        {
            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveAsCommand, path));
            DynamoCommands.ProcessCommandQueue();

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveAsCommand, path));
            //DynamoCommands.ProcessCommandQueue();
            vm.SaveAs(path);

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        // SaveCommand

        [Test]
        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveCommand, null));
            DynamoCommands.ProcessCommandQueue();

            Assert.IsNull(controller.DynamoViewModel.CurrentSpace.FilePath);
        }

        [Test]
        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();

                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveCommand, null));
            //DynamoCommands.ProcessCommandQueue();

            vm.Save();

            Assert.IsNull(controller.DynamoViewModel.CurrentSpace.FilePath);
        }



        [Test]
        public void CanSelectAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                dynSettings.Controller.OnRequestSelect(this, new ModelEventArgs(null, null));
            }
        }

        [Test]
        public void CanSelectNodeAndTheRestAreDeslected()
        {
            Assert.Inconclusive("Test not valid after move of selection handling logic to view.");

            //var vm = dynSettings.Controller.DynamoViewModel;

            //int numNodes = 100;

            //// create 100 nodes, and select them as you go
            //for (int i = 0; i < numNodes; i++)
            //{
            //    var sumData = new Dictionary<string, object>();
            //    sumData.Add("name", "Add");
            //    //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
            //    //DynamoCommands.ProcessCommandQueue();
            //    vm.CreateNode(sumData);

            //    Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            //    DynamoSelection.Instance.Selection.Add();
            //    controller.OnRequestSelect(null, new ModelEventArgs( controller.DynamoViewModel.Model.Nodes[i], null) );
            //    Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);
            //}
        }

        [Test]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            for (int i = 0; i < 20; i++)
            {
                DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.HomeCommand, null));
                Assert.AreEqual(true, controller.DynamoViewModel.ViewingHomespace);
            }
        }

        [Test]
        public void CanSumTwoNumbers()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            var sumData = new Dictionary<string, object>();
            var numData1 = new Dictionary<string, object>();
            var numData2 = new Dictionary<string, object>();

            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "Dynamo.Nodes.dynAddition");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            var watch = new Dictionary<string, object>();
            watch.Add("x", 100.0);
            watch.Add("y", 300.0);
            watch.Add("name", "Dynamo.Nodes.dynWatch");

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, numData1));
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, numData2));
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, watch));
            //DynamoCommands.ProcessCommandQueue();

            vm.CreateNode(sumData);
            vm.CreateNode(numData1);
            vm.CreateNode(numData2);
            vm.CreateNode(watch);

            //update the layout so the following
            //connectors have visuals to transform to
            //we were experiencing a problem in tests with TransfromToAncestor
            //calls not being valid because entities weren't in the tree yet.
            //dynSettings.Bench.Dispatcher.Invoke(
            //    new Action(delegate { controller.Bench.UpdateLayout(); }), DispatcherPriority.Render, null);


            var num1 = controller.DynamoViewModel.Model.Nodes[1] as dynDoubleInput;
            num1.Value = 2;
            var num2 = controller.DynamoViewModel.Model.Nodes[2] as dynDoubleInput;
            num2.Value = 2;

            var cd1 = new Dictionary<string, object>();
            cd1.Add("start", controller.DynamoViewModel.Model.Nodes[1]);
            cd1.Add("end", controller.DynamoViewModel.Model.Nodes[0]);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCommand, cd1));
            vm.CreateConnection(cd1);

            var cd2 = new Dictionary<string, object>();
            cd2.Add("start", controller.DynamoViewModel.Model.Nodes[2]); //first number node
            cd2.Add("end", controller.DynamoViewModel.Model.Nodes[0]); //+ node
            cd2.Add("port_start", 0); //first output
            cd2.Add("port_end", 1); //second input

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCommand, cd2));
            vm.CreateConnection(cd2);

            var cd3 = new Dictionary<string, object>();
            cd3.Add("start", controller.DynamoViewModel.Model.Nodes[0]); // add
            cd3.Add("end", controller.DynamoViewModel.Model.Nodes[3]); // watch
            cd3.Add("port_start", 0); //first output
            cd3.Add("port_end", 0); //second input

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCommand, cd3));
            vm.CreateConnection(cd3);

            controller.DynamoViewModel.LogText = "";

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.RunExpressionCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            vm.RunExpression(null);

            Thread.Sleep(250);

            Assert.AreEqual((controller.DynamoViewModel.Model.Nodes[0]).PrintExpression(), "(Add 2 2)");
            Assert.AreEqual((controller.DynamoViewModel.Model.Nodes[3]).PrintExpression(), "(Watch (Add 2 2))");
            Assert.AreEqual(controller.DynamoViewModel.Model.Nodes[3] is dynWatch, true);

            var w = (dynWatch)controller.DynamoViewModel.Model.Nodes[3];
            double val = 0.0;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(w.OldValue, ref val) );
            Assert.AreEqual(4.0, val);

        }
        
        [Test]
        public void SelectionDoesNotChangeWhenAddingAlreadySelectedNode()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                vm.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                vm.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);
                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\nodeLocationTest.dyn");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCommand, openPath));
            //DynamoCommands.ProcessCommandQueue();
            vm.Open(openPath);

            Assert.AreEqual(1, dynSettings.Controller.DynamoModel.Nodes.Count);
            var node = dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zu-ZA");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCommand, openPath));
            //DynamoCommands.ProcessCommandQueue();
            vm.Open(openPath);

            Assert.AreEqual(1, dynSettings.Controller.DynamoModel.Nodes.Count);
            node = dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCommand, openPath));
            //DynamoCommands.ProcessCommandQueue();
            vm.Open(openPath);

            Assert.AreEqual(1, dynSettings.Controller.DynamoModel.Nodes.Count);
            node = dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Test]
        public void AngleConverter()
        {
            RadianToDegreesConverter converter = new RadianToDegreesConverter();
            double radians = Convert.ToDouble(converter.ConvertBack(90.0, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(1.57, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack(180.0, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(3.14, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack(360.0, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(6.28, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack(-90.0, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(-1.57, radians, 0.01);

            double degrees = Convert.ToDouble(converter.Convert(-1.570795, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(-90.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert(6.28318, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(360.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert(3.14159, typeof(string), null, new System.Globalization.CultureInfo("en-US")));
            Assert.AreEqual(180.0, degrees, 0.01);
        }

        //// CancelRunCommand

        //[Test]
        //public void CanCancelRun()
        //{
        //    // TODO: need an expensive operation to run
        //    DynamoCommands.RunExpressionCmd.Execute();
        //    DynamoCommands.CancelRunCmd.Execute(null);

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
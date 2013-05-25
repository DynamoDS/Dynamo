using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoElementsTests
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
                controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment(), false, typeof(DynamoViewModel), Context.NONE);
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
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\multiplicationAndAdd\multiplicationAndAdd.dyn");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.OpenCommand, openPath));
            controller.ProcessCommandQueue();

            Assert.AreEqual(5, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }


        [Test]
        public void CanAddANodeByName()
        {
            var sumData = new Dictionary<string, object>();
            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);

            sumData.Add("name", "Add");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));

            controller.ProcessCommandQueue();

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

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddNoteCommand, inputs));
            controller.ProcessCommandQueue();

            Assert.AreEqual(controller.DynamoViewModel.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     null));
                Assert.DoesNotThrow(() => controller.ProcessCommandQueue());

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand, 5));
                Assert.DoesNotThrow(() => controller.ProcessCommandQueue());

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     "noodle"));
                Assert.DoesNotThrow(() => controller.ProcessCommandQueue());

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     new StringBuilder()));
                Assert.DoesNotThrow(() => controller.ProcessCommandQueue());
            }
        }

        [Test]
        public void CanAddToSelectionCommand()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        public void CanClearLog()
        {
            Assert.AreNotEqual(0, controller.DynamoViewModel.LogText.Length);
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.ClearLogCommand, null));
            controller.ProcessCommandQueue();
            Assert.AreEqual(0, controller.DynamoViewModel.LogText.Length);
        }

        // Clearworkspace 

        [Test]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.ClearCommand, null));
            controller.ProcessCommandQueue();
            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        [Test]
        public void CanClearWorkspaceWithNodes()
        {
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

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, numData1));
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, numData2));
            controller.ProcessCommandQueue();

            Assert.AreEqual(3, controller.DynamoViewModel.Model.Nodes.Count());

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.ClearCommand, null));
            controller.ProcessCommandQueue();
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
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CopyCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }

        [Test]
        public void CanAdd1NodeToClipboardAndPaste()
        {
            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand,
                                                                                     sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CopyCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.PasteCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CopyCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.PasteCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste3Times()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CopyCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.PasteCommand, null));
                controller.ProcessCommandQueue();

                Assert.AreEqual(numNodes, controller.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }
        }

        [Test]
        public void CanAddOneNodeToClipboard()
        {
            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CopyCommand, null));
            controller.ProcessCommandQueue();

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }


        // LayoutAll

        [Test]
        public void CanLayoutAll()
        {
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.LayoutAllCommand, null));
            controller.ProcessCommandQueue();
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
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.SaveAsCommand, path));
            controller.ProcessCommandQueue();

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.SaveAsCommand, path));
            controller.ProcessCommandQueue();

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        // SaveCommand

        [Test]
        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.SaveCommand, null));
            controller.ProcessCommandQueue();

            Assert.IsNull(controller.DynamoViewModel.CurrentSpace.FilePath);
        }

        [Test]
        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.SaveCommand, null));
            controller.ProcessCommandQueue();

            Assert.IsNull(controller.DynamoViewModel.CurrentSpace.FilePath);
        }



        [Test]
        public void CanSelectAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                dynSettings.Controller.OnRequestSelect(this, new NodeEventArgs(null, null));
            }
        }

        [Test]
        public void CanSelectNodeAndTheRestAreDeslected()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.OnRequestSelect(null, new NodeEventArgs( controller.DynamoViewModel.Model.Nodes[i], null) );
                Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            for (int i = 0; i < 20; i++)
            {
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.HomeCommand, null));
                Assert.AreEqual(true, controller.DynamoViewModel.ViewingHomespace);
            }
        }

        [Test]
        public void CanSumTwoNumbers()
        {
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

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, numData1));
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, numData2));
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, watch));
            controller.ProcessCommandQueue();

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

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateConnectionCommand, cd1));

            var cd2 = new Dictionary<string, object>();
            cd2.Add("start", controller.DynamoViewModel.Model.Nodes[2]); //first number node
            cd2.Add("end", controller.DynamoViewModel.Model.Nodes[0]); //+ node
            cd2.Add("port_start", 0); //first output
            cd2.Add("port_end", 1); //second input

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateConnectionCommand, cd2));

            var cd3 = new Dictionary<string, object>();
            cd3.Add("start", controller.DynamoViewModel.Model.Nodes[0]); // add
            cd3.Add("end", controller.DynamoViewModel.Model.Nodes[3]); // watch
            cd3.Add("port_start", 0); //first output
            cd3.Add("port_end", 0); //second input

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateConnectionCommand, cd3));

            controller.DynamoViewModel.LogText = "";

            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.RunExpressionCommand, null));
            controller.ProcessCommandQueue();

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
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, sumData));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.AddToSelectionCommand,
                                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                controller.ProcessCommandQueue();

                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
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
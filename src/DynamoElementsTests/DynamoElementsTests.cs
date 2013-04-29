//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Windows.Threading;
//using Dynamo.Commands;
//using Dynamo.Nodes;
//using Dynamo.Utilities;
//using Dynamo.Selection;
//using NUnit.Framework;

//namespace Dynamo.Tests
//{
//    [TestFixture]
//    internal class DynamoElementsTests
//    {
//        [SetUp]
//        public void Init()
//        {
//            StartDynamo();
            
//        }

//        [TearDown]
//        public void Cleanup()
//        {
//            try
//            {
//                DynamoLogger.Instance.FinishLogging();
//                EmptyTempFolder();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.StackTrace);
//            }
//        }

//        private static string TempFolder;

//        private static void StartDynamo()
//        {
//            try
//            {

//                string tempPath = Path.GetTempPath();

//                TempFolder = Path.Combine(tempPath, "dynamoTmp");

//                if (!Directory.Exists(TempFolder))
//                {
//                    Directory.CreateDirectory(TempFolder);
//                }
//                else
//                {
//                    EmptyTempFolder();
//                }

//                DynamoLogger.Instance.StartLogging();

//                //create a new instance of the ViewModel
//                new DynamoController( new FSchemeInterop.ExecutionEnvironment(), false );
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.StackTrace);
//            }
//        }

//        public static void EmptyTempFolder()
//        {
//            try
//            {
//                var directory = new DirectoryInfo(TempFolder);
//                foreach (FileInfo file in directory.GetFiles()) file.Delete();
//                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.StackTrace);
//            }
//        }

//        // OpenCommand

//        [Test]
//        public void CanOpenGoodFile()
//        {
//            // NOTE rom PB: this test fails due to the fact that Bench is locked as it was never shown in these tests
//            //              The same test is present in DynamoElementsUITests.cs, where it succeeds
//            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\multiplicationAndAdd.dyn");
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.OpenCommand, openPath));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(5, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//        }


//        [Test]
//        public void CanAddANodeByName()
//        {
//            var sumData = new Dictionary<string, object>();
//            sumData.Add("x", 400.0);
//            sumData.Add("y", 100.0);

//            sumData.Add("name", "+");
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));

//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count, 1);
//        }

//        [Test]
//        public void CanAddANote()
//        {
//            //create some test note data
//            var inputs = new Dictionary<string, object>();
//            inputs.Add("x", 200.0);
//            inputs.Add("y", 200.0);
//            inputs.Add("text", "This is a test note.");
//            inputs.Add("workspace", dynSettings.Controller.DynamoViewModel.CurrentSpace);

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddNoteCommand, inputs));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(dynSettings.Controller.DynamoViewModel.CurrentSpace.Notes.Count, 1);
//        }

//        [Test]
//        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
//        {
//            int numNodes = 100;

//            // select all of them one by one
//            for (int i = 0; i < numNodes; i++)
//            {
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     null));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand, 5));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     "noodle"));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     new StringBuilder()));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());
//            }
//        }

//        [Test]
//        public void CanAddToSelectionCommand()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();

//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }
//        }

//        // Log

//        [Test]
//        public void CanClearLog()
//        {
//            Assert.AreNotEqual(0, dynSettings.Controller.DynamoViewModel.LogText.Length);
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.ClearLogCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();
//            Assert.AreEqual(0, dynSettings.Controller.DynamoViewModel.LogText.Length);
//        }

//        // Clearworkspace 

//        [Test]
//        public void CanClearWorkspaceWithEmptyWorkspace()
//        {
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.ClearCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();
//            Assert.AreEqual(0, dynSettings.Controller.DynamoViewModel.Model.Nodes.Count());
//        }

//        [Test]
//        public void CanClearWorkspaceWithNodes()
//        {
//            var sumData = new Dictionary<string, object>();
//            var numData1 = new Dictionary<string, object>();
//            var numData2 = new Dictionary<string, object>();

//            sumData.Add("x", 400.0);
//            sumData.Add("y", 100.0);
//            sumData.Add("name", "Add");

//            numData1.Add("x", 100.0);
//            numData1.Add("y", 100.0);
//            numData1.Add("name", "Number");

//            numData2.Add("x", 100.0);
//            numData2.Add("y", 300.0);
//            numData2.Add("name", "Number");

//            Assert.AreEqual(0, dynSettings.Controller.DynamoViewModel.Model.Nodes.Count());

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, numData1));
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, numData2));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(3, dynSettings.Controller.DynamoViewModel.Model.Nodes.Count());

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.ClearCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();
//            Assert.AreEqual(0, dynSettings.Controller.DynamoViewModel.Model.Nodes.Count());
//        }

//        [Test]
//        public void CanAdd100NodesToClipboard()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand,sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CopyCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);
//        }

//        [Test]
//        public void CanAdd1NodeToClipboardAndPaste()
//        {
//            int numNodes = 1;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                sumData.Add("guid", Guid.NewGuid());
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand,
//                                                                                     sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CopyCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.PasteCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes * 2, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//        }

//        [Test]
//        public void CanAdd100NodesToClipboardAndPaste()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();

//                sumData.Add("name", "+");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand,sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CopyCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.PasteCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes * 2, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//        }

//        [Test]
//        public void CanAdd100NodesToClipboardAndPaste3Times()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();

//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CopyCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);

//            int numPastes = 3;
//            for (int i = 1; i <= numPastes; i++)
//            {
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.PasteCommand, null));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);
//                Assert.AreEqual(numNodes * (i+1) , dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//            }
//        }

//        [Test]
//        public void CanAddOneNodeToClipboard()
//        {
//            int numNodes = 1;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CopyCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.AreEqual(numNodes, dynSettings.Controller.ClipBoard.Count);
//        }


//        // LayoutAll

//        [Test]
//        public void CanLayoutAll()
//        {
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.LayoutAllCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();
//            Assert.AreNotEqual(0, dynSettings.Controller.DynamoViewModel.Model.Nodes.Count());
//        }

//        // SaveImage

//        //[Test]
//        //public void CanGoHomeWhenInDifferentWorkspace()
//        //{
//        //    // move to different workspace
//        //    // go home
//        //    // need to create new function via command
//        //    //TODO: loadWorkspaceFromFileCommand
//        //}

//    // SaveAsCommand

//        [Test]
//        public void CanSaveAsEmptyFile()
//        {
//            string fn = "ruthlessTurtles.dyn";
//            string path = Path.Combine(TempFolder, fn);
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SaveAsCommand, path));
//            dynSettings.Controller.ProcessCommandQueue();

//            var tempFldrInfo = new DirectoryInfo(TempFolder);
//            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
//            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
//        }

//        [Test]
//        public void CanSaveAsFileWithNodesInIt()
//        {
//            int numNodes = 100;

//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//            }

//            string fn = "ruthlessTurtles.dyn";
//            string path = Path.Combine(TempFolder, fn);
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SaveAsCommand, path));
//            dynSettings.Controller.ProcessCommandQueue();

//            var tempFldrInfo = new DirectoryInfo(TempFolder);
//            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
//            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
//        }

//    // SaveCommand

//        [Test]
//        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
//        {
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SaveCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.IsNull(dynSettings.Controller.DynamoViewModel.CurrentSpace.FilePath);
//        }

//        [Test]
//        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
//        {
//            int numNodes = 100;

//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
//            }

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SaveCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            Assert.IsNull(dynSettings.Controller.DynamoViewModel.CurrentSpace.FilePath);
//        }



//        [Test]
//        public void CanSelectAndNotThrowExceptionWhenPassedIncorrectType()
//        {
//            int numNodes = 100;

//            // select all of them one by one
//            for (int i = 0; i < numNodes; i++)
//            {
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SelectCommand, null));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SelectCommand, 5));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SelectCommand, "noodle"));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SelectCommand,
//                                                                                     new StringBuilder()));
//                Assert.DoesNotThrow(() => dynSettings.Controller.ProcessCommandQueue());
//            }
//        }

//        [Test]
//        public void CanSelectNodeAndTheRestAreDeslected()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.SelectCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);
//            }
//        }

//        [Test]
//        public void CanStayHomeWhenInHomeWorkspace()
//        {
//            for (int i = 0; i < 20; i++)
//            {
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.HomeCommand, null));
//                Assert.AreEqual(true, dynSettings.Controller.DynamoViewModel.ViewingHomespace);
//            }
//        }

//        [Test]
//        public void CanSumTwoNumbers()
//        {
//            var sumData = new Dictionary<string, object>();
//            var numData1 = new Dictionary<string, object>();
//            var numData2 = new Dictionary<string, object>();

//            sumData.Add("x", 400.0);
//            sumData.Add("y", 100.0);
//            sumData.Add("name", "Add");

//            numData1.Add("x", 100.0);
//            numData1.Add("y", 100.0);
//            numData1.Add("name", "Number");

//            numData2.Add("x", 100.0);
//            numData2.Add("y", 300.0);
//            numData2.Add("name", "Number");

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, numData1));
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, numData2));
//            dynSettings.Controller.ProcessCommandQueue();

//            //update the layout so the following
//            //connectors have visuals to transform to
//            //we were experiencing a problem in tests with TransfromToAncestor
//            //calls not being valid because entities weren't in the tree yet.
//            dynSettings.Bench.Dispatcher.Invoke(
//                new Action(delegate { dynSettings.Controller.Bench.UpdateLayout(); }), DispatcherPriority.Render, null);


//            var num1 = dynSettings.Controller.DynamoViewModel.Model.Nodes[1] as dynDoubleInput;
//            num1.Value = 2;
//            var num2 = dynSettings.Controller.DynamoViewModel.Model.Nodes[2] as dynDoubleInput;
//            num2.Value = 2;

//            var cd1 = new Dictionary<string, object>();
//            cd1.Add("start", dynSettings.Controller.DynamoViewModel.Model.Nodes[1]);
//            cd1.Add("end", dynSettings.Controller.DynamoViewModel.Model.Nodes[0]);
//            cd1.Add("port_start", 0);
//            cd1.Add("port_end", 0);

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateConnectionCommand, cd1));
//            var cd2 = new Dictionary<string, object>();
//            cd2.Add("start", dynSettings.Controller.DynamoViewModel.Model.Nodes[2]); //first number node
//            cd2.Add("end", dynSettings.Controller.DynamoViewModel.Model.Nodes[0]); //+ node
//            cd2.Add("port_start", 0); //first output
//            cd2.Add("port_end", 1); //second input

//            dynSettings.Controller.DynamoViewModel.LogText = "";

//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateConnectionCommand, cd2));
//            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.RunExpressionCommand, null));
//            dynSettings.Controller.ProcessCommandQueue();

//            //validate that the expression for addition is as expected
//            Assert.AreEqual((dynSettings.Controller.DynamoViewModel.Model.Nodes[0]).PrintExpression(), "(+ 2 2)");

//            dynSettings.Bench.Dispatcher.Invoke(
//                new Action(delegate { dynSettings.Controller.Bench.UpdateLayout(); }), DispatcherPriority.Render, null);

//            Assert.AreEqual(dynSettings.Controller.DynamoViewModel.Model.Nodes.Count, 3);
//        }



//        [Test]
//        public void SelectionDoesNotChangeWhenAddingAlreadySelectedNode()
//        {
//            int numNodes = 100;

//            // create 100 nodes, and select them as you go
//            for (int i = 0; i < numNodes; i++)
//            {
//                var sumData = new Dictionary<string, object>();
//                sumData.Add("name", "Add");
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, sumData));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, dynSettings.Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
//            }

//            // the number selected stays the same
//            for (int i = 0; i < numNodes; i++)
//            {
//                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.AddToSelectionCommand,
//                                                                                     dynSettings.Controller.DynamoViewModel.Model.Nodes[i]));
//                dynSettings.Controller.ProcessCommandQueue();

//                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
//            }
//        }

//        //// CancelRunCommand

//        //[Test]
//        //public void CanCancelRun()
//        //{
//        //    // TODO: need an expensive operation to run
//        //    DynamoCommands.RunExpressionCmd.Execute();
//        //    DynamoCommands.CancelRunCmd.Execute(null);

//        //}

//        //// RunExpressionCommand

//        //[Test]
//        //public void CanRunExpression()
//        //{

//        //}

//        //// CreateConnectionCommand

//        //[Test]
//        //public void CanCreateConnection()
//        //{

//        //}
//    }
//}
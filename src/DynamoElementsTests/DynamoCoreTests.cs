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
using NUnit.Framework;

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

        #region utility methods

        public double ConvertToDouble(dynNodeModel node)
        {
            //dynDoubleInput n = node as dynDoubleInput;
            Assert.AreNotEqual(null, node);
            Assert.AreNotEqual(null, node.OldValue);
            Assert.AreEqual(true, node.OldValue.IsNumber);
            return (node.OldValue as FScheme.Value.Number).Item;
        }

        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(vm, guid);
        }

        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, Guid guid)
        {
            return vm.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public dynWatch GetWatchNodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(vm, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
        }

        public double GetDoubleFromFSchemeValue(FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        public FSharpList<FScheme.Value> GetListFromFSchemeValue(FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        #endregion

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
            var model = dynSettings.Controller.DynamoModel;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\multiplicationAndAdd\multiplicationAndAdd.dyn");
            model.Open(openPath);

            Assert.AreEqual(5, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAddANodeByName()
        {
            var model = dynSettings.Controller.DynamoModel;

            var sumData = new Dictionary<string, object>();
            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);

            sumData.Add("name", "Add");
            model.CreateNode(sumData);

            Assert.AreEqual(controller.DynamoModel.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        public void CanAddANote()
        {
            var model = dynSettings.Controller.DynamoModel;

            //create some test note data
            var inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", controller.DynamoViewModel.CurrentSpace);

            model.AddNote(inputs);

            Assert.AreEqual(controller.DynamoViewModel.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
        {
            var model = dynSettings.Controller.DynamoModel;
            
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                Assert.DoesNotThrow(() => model.AddToSelection(null));

                Assert.DoesNotThrow(() => model.AddToSelection(5));

                Assert.DoesNotThrow(() => model.AddToSelection("noodle"));

                Assert.DoesNotThrow(() => model.AddToSelection(new StringBuilder()));
            }
        }

        [Test]
        public void CanAddToSelectionCommand()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        public void CanClearLog()
        {
            var model = dynSettings.Controller.DynamoModel;

            Assert.AreNotEqual(0, controller.DynamoViewModel.LogText.Length);
            dynSettings.Controller.ClearLog(null);

            Assert.AreEqual(0, controller.DynamoViewModel.LogText.Length);
        }

        // Clearworkspace 

        [Test]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            dynSettings.Controller.DynamoModel.Clear(null);
            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        [Test]
        public void CanClearWorkspaceWithNodes()
        {
            var model = dynSettings.Controller.DynamoModel;

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

            model.CreateNode(sumData);
            model.CreateNode(numData1);
            model.CreateNode(numData2);

            Assert.AreEqual(3, controller.DynamoViewModel.Model.Nodes.Count());

            model.Clear(null);

            Assert.AreEqual(0, controller.DynamoViewModel.Model.Nodes.Count());
        }

        [Test]
        public void CanAdd100NodesToClipboard()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");

                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }

        [Test]
        public void CanAdd1NodeToClipboardAndPaste()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");

                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
            model.Paste(null);

            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");
                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            model.Paste(null);
            Assert.AreEqual(numNodes * 2, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        public void CanAdd100NodesToClipboardAndPaste3Times()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();

                sumData.Add("name", "Add");

                model.CreateNode(sumData);
                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                model.Paste(null);

                Assert.AreEqual(numNodes, controller.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }
        }

        [Test]
        public void CanAddOneNodeToClipboard()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            model.Copy(null);

            Assert.AreEqual(numNodes, controller.ClipBoard.Count);
        }


        // LayoutAll

        [Test]
        public void CanLayoutAll()
        {
            var model = dynSettings.Controller.DynamoModel;

            model.LayoutAll(null);

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
            var model = dynSettings.Controller.DynamoModel;

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            model.SaveAs(path);
            
            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
                //DynamoCommands.ProcessCommandQueue();
                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.SaveAsCommand, path));
            //DynamoCommands.ProcessCommandQueue();
            model.SaveAs(path);

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        // SaveCommand

        [Test]
        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            var model = dynSettings.Controller.DynamoModel;

            model.SaveAs(null);

            Assert.IsNull(controller.DynamoViewModel.CurrentSpace.FilePath);
        }

        [Test]
        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");

                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            model.Save(null);

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

            //var model = dynSettings.Controller.DynamoModel;

            //int numNodes = 100;

            //// create 100 nodes, and select them as you go
            //for (int i = 0; i < numNodes; i++)
            //{
            //    var sumData = new Dictionary<string, object>();
            //    sumData.Add("name", "Add");
            //    //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCommand, sumData));
            //    //DynamoCommands.ProcessCommandQueue();
            //    model.CreateNode(sumData);

            //    Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

            //    DynamoSelection.Instance.Selection.Add();
            //    controller.OnRequestSelect(null, new ModelEventArgs( controller.DynamoViewModel.Model.Nodes[i], null) );
            //    Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);
            //}
        }

        [Test]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            var model = dynSettings.Controller.DynamoModel;

            for (int i = 0; i < 20; i++)
            {
                model.Home(null);
                Assert.AreEqual(true, controller.DynamoViewModel.ViewingHomespace);
            }
        }

        [Test]
        public void CanSumTwoNumbers()
        {
            var model = dynSettings.Controller.DynamoModel;

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

            model.CreateNode(sumData);
            model.CreateNode(numData1);
            model.CreateNode(numData2);
            model.CreateNode(watch);

            //update the layout so the following
            //connectors have visuals to transform to
            //we were experiencing a problem in tests with TransfromToAncestor
            //calls not being valid because entities weren't in the tree yet.
            //dynSettings.Bench.Dispatcher.Invoke(
            //    new Action(delegate { controller.Bench.UpdateLayout(); }), DispatcherPriority.Render, null);

            var num1 = controller.DynamoViewModel.Model.Nodes[1] as dynDoubleInput;
            num1.Value = "2";
            var num2 = controller.DynamoViewModel.Model.Nodes[2] as dynDoubleInput;
            num2.Value = "2";

            var cd1 = new Dictionary<string, object>();
            cd1.Add("start", controller.DynamoViewModel.Model.Nodes[1]);
            cd1.Add("end", controller.DynamoViewModel.Model.Nodes[0]);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            model.CreateConnection(cd1);

            var cd2 = new Dictionary<string, object>();
            cd2.Add("start", controller.DynamoViewModel.Model.Nodes[2]); //first number node
            cd2.Add("end", controller.DynamoViewModel.Model.Nodes[0]); //+ node
            cd2.Add("port_start", 0); //first output
            cd2.Add("port_end", 1); //second input

            model.CreateConnection(cd2);

            var cd3 = new Dictionary<string, object>();
            cd3.Add("start", controller.DynamoViewModel.Model.Nodes[0]); // add
            cd3.Add("end", controller.DynamoViewModel.Model.Nodes[3]); // watch
            cd3.Add("port_start", 0); //first output
            cd3.Add("port_end", 0); //second input

            model.CreateConnection(cd3);

            //controller.DynamoViewModel.LogText = "";

            dynSettings.Controller.RunExpression(null);

            Thread.Sleep(250);

            Assert.AreEqual(controller.DynamoViewModel.Model.Nodes[3] is dynWatch, true);

            var w = (dynWatch)controller.DynamoViewModel.Model.Nodes[3];
            double val = 0.0;
            Assert.AreEqual(true, Utils.Convert(w.OldValue, ref val) );
            Assert.AreEqual(4.0, val);

        }
        
        [Test]
        public void SelectionDoesNotChangeWhenAddingAlreadySelectedNode()
        {
            var model = dynSettings.Controller.DynamoModel;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var sumData = new Dictionary<string, object>();
                sumData.Add("name", "Add");
                model.CreateNode(sumData);

                Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                model.AddToSelection(controller.DynamoViewModel.Model.Nodes[i]);
                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            var model = dynSettings.Controller.DynamoModel;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\good_dyns\nodeLocationTest.dyn");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.OpenCommand, openPath));
            //DynamoCommands.ProcessCommandQueue();
            model.Open(openPath);

            Assert.AreEqual(1, dynSettings.Controller.DynamoModel.Nodes.Count);
            var node = dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zu-ZA");
            model.Open(openPath);

            Assert.AreEqual(1, dynSettings.Controller.DynamoModel.Nodes.Count);
            node = dynSettings.Controller.DynamoModel.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            model.Open(openPath);

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

        [Test]
        public void AdditionOfTwoArray()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\trial\AdditionOfTwoArray.dyn");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.OpenCommand, openPath));
            controller.ProcessCommandQueue();

            //var vm = controller.DynamoViewModel;

            //controller.RunCommand(vm.RunExpressionCommand);

            //var node1 = NodeFromCurrentSpace(vm, "75a16152-334d-49f1-af4b-82718b3c6dd1");
            //dynAddition n = node1 as dynAddition;
            //Assert.AreEqual(0.0, (n.OldValue as FScheme.Value.Number).Item);

            Assert.AreEqual(4, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(3, controller.DynamoViewModel.CurrentSpace.Connectors.Count);

        }

        [Test]
        public void Logic_Comparison_Test()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\trial\LogicComparisonTest.dyn");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.OpenCommand, openPath));
            controller.ProcessCommandQueue();

            var vm = controller.DynamoViewModel;

            // check an input value of Slider node
            var node1 = NodeFromCurrentSpace(vm, "7ecd2ad1-830c-4610-a37e-426a40bc9cc4");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(dynDoubleSliderInput), node1);
            Assert.AreEqual(9.0, ((dynDoubleSliderInput)node1).Value);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check an input value of number node
            var node2 = NodeFromCurrentSpace(vm, "33f2f9bb-2033-44f8-bf5f-abf1bf29778b");
            Assert.AreEqual(9.0, ConvertToDouble(node2));
            Assert.IsTrue(node2.IsVisible);

            // check the output value of less than node.
            var node3 = NodeFromCurrentSpace(vm, "2c8997c1-6e91-4072-bc12-8de9f1be68b1");
            dynLessThan n = node3 as dynLessThan;
            Assert.AreEqual(0.0, (n.OldValue as FScheme.Value.Number).Item);

            // Check watch value for all operations
            var watch = GetWatchNodeFromCurrentSpace(vm, "a0350fe2-ec68-4a39-820a-8594fc98a9cb");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal);

            var watch1 = GetWatchNodeFromCurrentSpace(vm, "381f050e-bf88-447a-9197-aa7059ba3e25");
            var doubleWatchVal1 = GetDoubleFromFSchemeValue(watch1.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal1);

            var watch2 = GetWatchNodeFromCurrentSpace(vm, "b4b72295-7e3a-4257-a162-27313f8367ac");
            var doubleWatchVal2 = GetDoubleFromFSchemeValue(watch2.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal2);

            var watch3 = GetWatchNodeFromCurrentSpace(vm, "d7c2cdcd-7229-4327-8aa7-a6892dd01cd7");
            var doubleWatchVal3 = GetDoubleFromFSchemeValue(watch3.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal3);

            var watch4 = GetWatchNodeFromCurrentSpace(vm, "a3310e87-90ff-4ec9-95b8-96091434f905");
            var doubleWatchVal4 = GetDoubleFromFSchemeValue(watch4.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal4);

            // check number of nodes and connectors
            Assert.AreEqual(12, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(15, controller.DynamoViewModel.CurrentSpace.Connectors.Count);

        }

        [Test]
        public void AdditionTest()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string openPath = Path.Combine(directory, @"..\..\test\dynamo_elements_samples\working\trial\AdditionTest.dyn");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.OpenCommand, openPath));
            controller.ProcessCommandQueue();

            var vm = controller.DynamoViewModel;
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check an input value of number node
            var node1 = NodeFromCurrentSpace(vm, "9f330501-79c4-4ed6-b4b6-957775d8d3cc");
            //dynDoubleInput n = node1 as dynDoubleInput;
            //Assert.AreEqual(10.0, (n.OldValue as FScheme.Value.Number).Item);
            //Another way of writing verification points...
            Assert.AreEqual(10.0, ConvertToDouble(node1));
            Assert.IsTrue(node1.IsVisible);

            // check an input value of number node
            var node2 = NodeFromCurrentSpace(vm, "a12cbcf0-6ffc-4bc4-ab15-44cdef2b3799");
            dynDoubleInput n1 = node2 as dynDoubleInput;
            Assert.AreEqual(20.0, (n1.OldValue as FScheme.Value.Number).Item);

            // check the output value of Add node.
            var node3 = NodeFromCurrentSpace(vm, "a897df66-c910-4dd2-9960-bdbbb787caf8");
            //dynAddition n2 = node3 as dynAddition;
            //Assert.AreEqual(30.0, (n2.OldValue as FScheme.Value.Number).Item);

            Assert.AreEqual(30.0, ConvertToDouble(node3));
            Assert.IsTrue(node3.IsVisible);

            // check number of nodes and connectors
            Assert.AreEqual(3, controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(2, controller.DynamoViewModel.CurrentSpace.Connectors.Count);

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
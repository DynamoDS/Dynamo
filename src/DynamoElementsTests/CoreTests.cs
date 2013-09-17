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
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class CoreTests : DynamoUnitTest
    {
        
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

            string openPath = Path.Combine(GetTestDirectory(), @"core\multiplicationAndAdd\multiplicationAndAdd.dyn");
            model.Open(openPath);

            Assert.AreEqual(5, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
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

            Assert.AreEqual(Controller.DynamoViewModel.CurrentSpace.Nodes.Count, 1);
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
            inputs.Add("workspace", Controller.DynamoViewModel.CurrentSpace);

            model.AddNote(inputs);

            Assert.AreEqual(Controller.DynamoViewModel.CurrentSpace.Notes.Count, 1);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        public void CanClearLog()
        {
            var model = dynSettings.Controller.DynamoModel;

            Assert.AreNotEqual(0, Controller.DynamoViewModel.LogText.Length);
            dynSettings.Controller.ClearLog(null);

            Assert.AreEqual(0, Controller.DynamoViewModel.LogText.Length);
        }

        // Clearworkspace 

        [Test]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            dynSettings.Controller.DynamoModel.Clear(null);
            Assert.AreEqual(0, Controller.DynamoViewModel.Model.Nodes.Count());
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

            Assert.AreEqual(0, Controller.DynamoViewModel.Model.Nodes.Count());

            model.CreateNode(sumData);
            model.CreateNode(numData1);
            model.CreateNode(numData2);

            Assert.AreEqual(3, Controller.DynamoViewModel.Model.Nodes.Count());

            model.Clear(null);

            Assert.AreEqual(0, Controller.DynamoViewModel.Model.Nodes.Count());
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, Controller.ClipBoard.Count);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, Controller.ClipBoard.Count);
            model.Paste(null);

            Assert.AreEqual(numNodes * 2, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, Controller.ClipBoard.Count);

            model.Paste(null);
            Assert.AreEqual(numNodes * 2, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
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
                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy(null);

            Assert.AreEqual(numNodes, Controller.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                model.Paste(null);

                Assert.AreEqual(numNodes, Controller.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCommand,
                //                                                                     controller.DynamoViewModel.Model.Nodes[i]));
                //DynamoCommands.ProcessCommandQueue();
                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            //DynamoCommands.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CopyCommand, null));
            //DynamoCommands.ProcessCommandQueue();
            model.Copy(null);

            Assert.AreEqual(numNodes, Controller.ClipBoard.Count);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
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

            Assert.IsNull(Controller.DynamoViewModel.CurrentSpace.FilePath);
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);
            }

            model.Save(null);

            Assert.IsNull(Controller.DynamoViewModel.CurrentSpace.FilePath);
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

            //    Assert.AreEqual(i + 1, controller.DynamoViewModel.CurrentWorkspace.Nodes.Count);

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
                Assert.AreEqual(true, Controller.DynamoViewModel.ViewingHomespace);
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
            sumData.Add("name", "Dynamo.Nodes.Addition");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            var watch = new Dictionary<string, object>();
            watch.Add("x", 100.0);
            watch.Add("y", 300.0);
            watch.Add("name", "Dynamo.Nodes.Watch");

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

            var num1 = Controller.DynamoViewModel.Model.Nodes[1] as DoubleInput;
            num1.Value = "2";
            var num2 = Controller.DynamoViewModel.Model.Nodes[2] as DoubleInput;
            num2.Value = "2";

            var cd1 = new Dictionary<string, object>();
            cd1.Add("start", Controller.DynamoViewModel.Model.Nodes[1]);
            cd1.Add("end", Controller.DynamoViewModel.Model.Nodes[0]);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            model.CreateConnection(cd1);

            var cd2 = new Dictionary<string, object>();
            cd2.Add("start", Controller.DynamoViewModel.Model.Nodes[2]); //first number node
            cd2.Add("end", Controller.DynamoViewModel.Model.Nodes[0]); //+ node
            cd2.Add("port_start", 0); //first output
            cd2.Add("port_end", 1); //second input

            model.CreateConnection(cd2);

            var cd3 = new Dictionary<string, object>();
            cd3.Add("start", Controller.DynamoViewModel.Model.Nodes[0]); // add
            cd3.Add("end", Controller.DynamoViewModel.Model.Nodes[3]); // watch
            cd3.Add("port_start", 0); //first output
            cd3.Add("port_end", 0); //second input

            model.CreateConnection(cd3);

            //controller.DynamoViewModel.LogText = "";

            dynSettings.Controller.RunExpression(null);

            Thread.Sleep(250);

            Assert.AreEqual(Controller.DynamoViewModel.Model.Nodes[3] is Watch, true);

            var w = (Watch)Controller.DynamoViewModel.Model.Nodes[3];
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

                Assert.AreEqual(i + 1, Controller.DynamoViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                model.AddToSelection(Controller.DynamoViewModel.Model.Nodes[i]);
                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\nodeLocationTest.dyn");

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
    }
}
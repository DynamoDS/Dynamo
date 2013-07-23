using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoSampleTests
    {

        #region startup and shutdown

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
        private static string ExecutingDirectory { get; set; }

        private static void StartDynamo()
        {
            try
            {
                ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
                controller.Testing = true;
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

        #endregion

        #region utility methods

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

        [Test]
        public void AddSubtractMapReduceFilterBasic()
        {
            var vm = controller.DynamoViewModel;

            string openPath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\map_reduce_filter\map_reduce_filter.dyn");
            controller.RunCommand( vm.OpenCommand, openPath );

            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(28, vm.CurrentSpace.Nodes.Count);

            // check an input value
            var node1 = NodeFromCurrentSpace(vm, "51ed7fed-99fa-46c3-a03c-2c076f2d0538");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(dynDoubleInput), node1);
            Assert.AreEqual(2.0, ((dynDoubleInput)node1).Value);
            
            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

            // add-subtract -3.0
            var watch = GetWatchNodeFromCurrentSpace(vm, "4a2363b6-ef64-44f5-be64-18832586e574");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(-3.0, doubleWatchVal);

            // map - list of three 6's 
            watch = GetWatchNodeFromCurrentSpace(vm,  "fcad8d7a-1c9f-4604-a03b-53393e36ea0b");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[2]));

            // reduce - 6.0
            watch = GetWatchNodeFromCurrentSpace(vm, "e892c469-47e6-4006-baea-ec4afea5a04e");
            doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6.0, doubleWatchVal);

            // filter - list of 6-10
            watch = GetWatchNodeFromCurrentSpace(vm, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(7, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(8, GetDoubleFromFSchemeValue(listWatchVal[2]));
            Assert.AreEqual(9, GetDoubleFromFSchemeValue(listWatchVal[3]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal[4]));

        }

        [Test]
        public void Sequence()
        {
            var vm = controller.DynamoViewModel;

            string openPath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\sequence\sequence.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(5, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        public void CombineWithCustomNodes()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\combine\");

            Assert.IsTrue( controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "combine2.dyf")));
            Assert.IsTrue( controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "Sequence2.dyf")));

            string openPath = Path.Combine(examplePath, "combine-with-three.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(10, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            Assert.Inconclusive("Finish me!");

        }

        [Test]
        public void ReduceAndRecursion()
        {
            var vm = controller.DynamoViewModel;

            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\reduce_and_recursion\");

            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "MyReduce.dyf")));
            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "Sum Numbers.dyf")));

            string openPath = Path.Combine(examplePath, "reduce-example.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            Assert.Inconclusive("Finish me!");

        }

        [Test]
        public void FilterWithCustomNode()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\filter\");

            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "IsOdd.dyf")));

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(6, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            Assert.Inconclusive("Finish me!");

        }

        [Test]
        public void Sorting()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\sorting\");

            string openPath = Path.Combine(examplePath, "sorting.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            Assert.Inconclusive("Finish me!");

        }

        [Test]
        public void Add()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Add.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4c5889ac-7b91-4fb5-aaad-a2128b533279");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Subtract()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Subtract.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "a574df4e-2dff-4c06-bbb6-e9467060085f");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Multiply()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Multiply.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Divide()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Divide.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Modulo()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Modulo.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4a780dfb-74b1-453a-86ef-2f4a5c46792e");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Ceiling()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Ceiling.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "97e58c7f-9082-4980-997a-d290cf8055e1");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(2.0, doubleWatchVal);
        }

        [Test]
        public void Floor()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Floor.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "fb52d286-ebcc-449c-989e-e4ea94831125");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Power()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Power.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "6a7b150e-f053-4b29-b672-007aa1acde24");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Round()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Round.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "430e086e-8cf0-4e89-abba-69dc1cd94058");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Sine()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Sine.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Cosine()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Cosine.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(-1.0, doubleWatchVal);
        }

        [Test]
        public void OpeningDynWithDyfMissingIsOkayAndRunsOkay()
        {
            Assert.DoesNotThrow(delegate
                {
                    var vm = controller.DynamoViewModel;
                    var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\CASE");
                    string openPath = Path.Combine(examplePath, "case_flip_matrix.dyn");

                    controller.RunCommand(vm.OpenCommand, openPath);

                    Assert.AreEqual( controller.DynamoModel.CurrentSpace.Nodes.Count, 11);

                    controller.RunCommand(vm.RunExpressionCommand);
                    Thread.Sleep(100);
                });
        }

        [Test]
        public void Tangent()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns\math");

            string openPath = Path.Combine(examplePath, "Tangent.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(100);

            var watch = GetWatchNodeFromCurrentSpace(vm, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal, 0.00001);
        }

        [Test]
        public void StringInputNodeWorksWithSpecialCharacters()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns");
            string openPath = Path.Combine(examplePath, "StringInputTest.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            dynStringInput strNode = (dynStringInput)controller.DynamoModel.Nodes.First(x => x is dynStringInput);
            string expected =
                "A node\twith tabs, and\r\ncarriage returns,\r\nand !@#$%^&* characters, and also something \"in quotes\".";

            Assert.AreEqual(expected, strNode.Value.ToString());
            
        }

        [Test]
        public void Repeat()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\good_dyns");
            string openPath = Path.Combine(examplePath, "RepeatTest.dyn");

            //open and run the expression
            controller.RunCommand(vm.OpenCommand, openPath);
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(300);

            var watch = (dynWatch)controller.DynamoModel.Nodes.First(x => x is dynWatch);
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);

            //change the value of the list
            var numNode = (dynDoubleInput) controller.DynamoModel.Nodes.Last(x => x is dynDoubleInput);
            numNode.Value = "3";
            controller.RunCommand(vm.RunExpressionCommand);
            Thread.Sleep(300);

            listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);

            //test the negative case to make sure it throws an error
            numNode.Value = "-1";
            Assert.Throws<NUnit.Framework.AssertionException>(() => controller.RunCommand(vm.RunExpressionCommand));

        }

        [Test]
        public void SliceList()
        {
            var data = new Dictionary<string, object>();
            data.Add("name", "Slice List");
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, data));
            controller.ProcessCommandQueue();

            //Create a List
            //For a list of 0..20, this will have 21 elements
            //Slicing by 5 should return 6 lists, the last containing one element
            var list = Utils.MakeFSharpList(Enumerable.Range(0, 21).Select(x => FScheme.Value.NewNumber(x)).ToArray());

            var sliceNode = (dynSlice)controller.DynamoModel.Nodes.First(x => x is dynSlice);
            var args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            var res = sliceNode.Evaluate(args);

            //confirm we have a list
            Assert.IsTrue(res.IsList);

            //confirm the correct number of sublists
            Assert.AreEqual(5, ((FScheme.Value.List)res).Item.Count());

            //test if you pass in an empty list
            //should return just one list - the original
            args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty), args);
            res = sliceNode.Evaluate(args);
            Assert.AreEqual(0, ((FScheme.Value.List)res).Item.Count());

            //test if you pass in a list wwith less elements than the
            //slice, you should just get back the same list
            list = Utils.MakeFSharpList(Enumerable.Range(0, 1).Select(x => FScheme.Value.NewNumber(x)).ToArray());
            args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            res = sliceNode.Evaluate(args);
            Assert.AreEqual(1, ((FScheme.Value.List)res).Item.Count());
        }

        [Test]
        public void Diagonals()
        {
            //0   1   2   3   4
            //5   6   7   8   9
            //10  11  12  13  14
            //15  16  17  18  19
          
            //diagonal left
            //should yield the following sublists
            //0
            //1,5
            //2,6,10
            //3,7,11,15
            //4,8,12,16
            //9,13,17
            //14,18
            //19

            var list = Utils.MakeFSharpList(Enumerable.Range(0, 20).Select(x => FScheme.Value.NewNumber(x)).ToArray());

            var data = new Dictionary<string, object> {{"name", "Diagonal Left List"}};
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, data));
            controller.ProcessCommandQueue();

            var leftNode = (dynDiagonalLeftList)controller.DynamoModel.Nodes.First(x => x is dynDiagonalLeftList);
            var args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            var res = leftNode.Evaluate(args);

            Assert.AreEqual(8, ((FScheme.Value.List)res).Item.Count());

            controller.CommandQueue.Enqueue(Tuple.Create<object,object>(controller.DynamoViewModel.ClearCommand, null));

            //diagonal right
            //diagonal left
            //should yield the following sublists
            //15
            //10,16
            //5,11,17
            //0,6,12,18
            //1,7,13,19
            //2,8,14
            //3,9
            //4

            data = new Dictionary<string, object> {{"name", "Diagonal Right List"}};
            controller.CommandQueue.Enqueue(Tuple.Create<object, object>(controller.DynamoViewModel.CreateNodeCommand, data));
            controller.ProcessCommandQueue();

            var rightNode = (dynDiagonalRightList)controller.DynamoModel.Nodes.First(x => x is dynDiagonalRightList);
            args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            res = rightNode.Evaluate(args);

            Assert.AreEqual(8, ((FScheme.Value.List)res).Item.Count());
        }

    }
}
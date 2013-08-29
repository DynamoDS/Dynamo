using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class CoreDynTests
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
                controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE)
                {
                    Testing = true
                };
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

        public dynNodeModel NodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(model, guid);
        }

        public string GetTestDirectory()
        {
            var directory = new DirectoryInfo(ExecutingDirectory);
            return Path.Combine(directory.Parent.Parent.FullName, "test");
        }
 
        public dynNodeModel NodeFromCurrentSpace(DynamoModel model, Guid guid)
        {
            return model.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public dynWatch GetWatchNodeFromCurrentSpace(DynamoModel model, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(model, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
        }

        public dynWatch GetFirstWatchNodeFromCurrentSpace(DynamoModel model)
        {
            return (dynWatch) model.CurrentSpace.Nodes.FirstOrDefault(x => x is dynWatch);
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
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\map_reduce_filter\map_reduce_filter.dyn");
            model.Open(openPath);


            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(28, model.CurrentSpace.Nodes.Count);

            // check an input value
            var node1 = NodeFromCurrentSpace(model, "51ed7fed-99fa-46c3-a03c-2c076f2d0538");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(dynDoubleInput), node1);
            Assert.AreEqual("2", ((dynDoubleInput)node1).Value);
            
            // run the expression
            //DynamoCommands.RunCommand(DynamoCommands.RunExpressionCommand);
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

            // add-subtract -3.0
            var watch = GetWatchNodeFromCurrentSpace(model, "4a2363b6-ef64-44f5-be64-18832586e574");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(-3.0, doubleWatchVal);

            // map - list of three 6's 
            watch = GetWatchNodeFromCurrentSpace(model,  "fcad8d7a-1c9f-4604-a03b-53393e36ea0b");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[2]));

            // reduce - 6.0
            watch = GetWatchNodeFromCurrentSpace(model, "e892c469-47e6-4006-baea-ec4afea5a04e");
            doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6.0, doubleWatchVal);

            // filter - list of 6-10
            watch = GetWatchNodeFromCurrentSpace(model, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
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
            var model = controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\sequence\sequence.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(5, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode = GetFirstWatchNodeFromCurrentSpace(model);
            Assert.IsNotNull(watchNode);

            // 50 elements between -1 and 1
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode.OldValue);
            var list = (watchNode.OldValue as FScheme.Value.List).Item;

            Assert.AreEqual(50, list.Count());
            list.ToList().ForEach(x =>
                {
                    Assert.IsAssignableFrom(typeof(FScheme.Value.Number), x);
                    var val = (x as FScheme.Value.Number).Item;
                    Assert.IsTrue((val < 1.0));
                    Assert.IsTrue((val > -1.0));
                });

        }

        [Test]
        public void CombineWithCustomNodes()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine\");

            string openPath = Path.Combine(examplePath, "combine-with-three.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // [[0,3,6], [2,5,4], [0,5,8]]

            // check the output values are correctly computed
            var watchNode = GetFirstWatchNodeFromCurrentSpace(model);
            Assert.IsNotNull(watchNode);

            var expected = new List<List<double>>()
                {
                    new List<double>() {0, 3, 6},
                    new List<double>() {1, 4, 7},
                    new List<double>() {2, 5, 8},
                };

            // 50 elements between -1 and 1
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode.OldValue);
            var outerList = (watchNode.OldValue as FScheme.Value.List).Item;

            Assert.AreEqual(3, outerList.Count());
            int i = 0;
            foreach (var innerList in outerList)
            {
                var fList = GetListFromFSchemeValue(innerList);
                int j = 0;
                foreach (var ele in fList)
                {
                    var num = (ele as FScheme.Value.Number).Item;
                    Assert.AreEqual(num, expected[i][j], 0.001);
                    j++;
                }
                i++;
            }

        }

        [Test]
        public void ReduceAndRecursion()
        {
            var model = controller.DynamoModel;

            var examplePath = Path.Combine(GetTestDirectory(), @"core\reduce_and_recursion\");

            string openPath = Path.Combine(examplePath, "reduce-example.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watch = GetWatchNodeFromCurrentSpace(model, "157557d2-2452-413a-9944-1df3df793cee");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(doubleWatchVal, 15.0, 0.001);

            var watch2 = GetWatchNodeFromCurrentSpace(model, "068dd555-a5d5-4f11-af05-e4fa0cc015c9");
            var doubleWatchVal1 = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(doubleWatchVal1, 15.0, 0.001);

            var watch3 = GetWatchNodeFromCurrentSpace(model, "1aca382d-ca81-4955-a6c1-0f549df19fd7");
            var doubleWatchVal2 = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(doubleWatchVal2, 15.0, 0.001);

        }

        [Test]
        public void FilterWithCustomNode()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\filter\");

            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "IsOdd.dyf")) != null);

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode = GetFirstWatchNodeFromCurrentSpace(model);
            Assert.IsNotNull(watchNode);

            // odd numbers between 0 and 5
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode.OldValue);
            var list = (watchNode.OldValue as FScheme.Value.List).Item;

            Assert.AreEqual(3, list.Count());
            var count = 1;
            list.ToList().ForEach(x =>
            {
                Assert.IsAssignableFrom(typeof(FScheme.Value.Number), x);
                var val = (x as FScheme.Value.Number).Item;
                Assert.AreEqual(count, val, 0.0001);
                count += 2;
            });

        }

        [Test]
        public void Sorting()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\sorting\");

            string openPath = Path.Combine(examplePath, "sorting.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, model.CurrentSpace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode1 = GetWatchNodeFromCurrentSpace(model, "d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            var watchNode2 = GetWatchNodeFromCurrentSpace(model, "c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
            Assert.IsNotNull(watchNode1);
            Assert.IsNotNull(watchNode2);

            // odd numbers between 0 and 5
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode1.OldValue);
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode2.OldValue);
            var list1 =
                (watchNode1.OldValue as FScheme.Value.List).Item.Select(x => (x as FScheme.Value.String).Item).ToList();
            var list2 =
                (watchNode2.OldValue as FScheme.Value.List).Item.Select(x => (x as FScheme.Value.String).Item).ToList();

            Assert.AreEqual(5, list1.Count);
            Assert.AreEqual(5, list2.Count);

            var values = new List<string>(){"aaaaa", "bbb", "aa", "c", "dddd"};

            values.Sort((e1, e2) => String.Compare(e1, e2));
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(list1[i], values[i]);
            }

            values.Sort((e1, e2) => e1.Count().CompareTo(e2.Count()));
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(list2[i], values[i]);
            }

        }

        [Test]
        public void Add()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Add.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4c5889ac-7b91-4fb5-aaad-a2128b533279");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Subtract()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Subtract.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "a574df4e-2dff-4c06-bbb6-e9467060085f");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Multiply()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Multiply.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Divide()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Divide.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Modulo()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Modulo.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4a780dfb-74b1-453a-86ef-2f4a5c46792e");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Ceiling()
        {
            var model = controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Ceiling.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "97e58c7f-9082-4980-997a-d290cf8055e1");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(2.0, doubleWatchVal);
        }

        [Test]
        public void Floor()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Floor.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "fb52d286-ebcc-449c-989e-e4ea94831125");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Power()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Power.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "6a7b150e-f053-4b29-b672-007aa1acde24");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Round()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Round.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "430e086e-8cf0-4e89-abba-69dc1cd94058");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Sine()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Sine.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Cosine()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Cosine.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(-1.0, doubleWatchVal);
        }

        [Test]
        public void OpeningDynWithDyfMissingIsOkayAndRunsOkay()
        {
            Assert.DoesNotThrow(delegate
                {
                    var model = dynSettings.Controller.DynamoModel;
                    var examplePath = Path.Combine(GetTestDirectory(), @"core\CASE");
                    string openPath = Path.Combine(examplePath, "case_flip_matrix.dyn");

                    model.Open(openPath);

                    Assert.AreEqual(dynSettings.Controller.DynamoModel.CurrentSpace.Nodes.Count, 11);

                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void Tangent()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Tangent.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(0.0, doubleWatchVal, 0.00001);
        }

        [Test]
        public void StringInputNodeWorksWithSpecialCharacters()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core");
            string openPath = Path.Combine(examplePath, "StringInputTest.dyn");
            model.Open(openPath);

            var strNode = (dynStringInput)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynStringInput);
            const string expected =
                "A node\twith tabs, and\r\ncarriage returns,\r\nand !@#$%^&* characters, and also something \"in quotes\".";

            Assert.AreEqual(expected, strNode.Value);
            
        }

        [Test]
        public void Repeat()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core");
            string openPath = Path.Combine(examplePath, "RepeatTest.dyn");

            //open and run the expression
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = (dynWatch)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynWatch);
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);

            //change the value of the list
            var numNode = (dynDoubleInput) controller.DynamoModel.Nodes.Last(x => x is dynDoubleInput);
            numNode.Value = "3";
            dynSettings.Controller.RunExpression(null);
            Thread.Sleep(300);

            listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);

            //test the negative case to make sure it throws an error
            numNode.Value = "-1";
            Assert.Throws<AssertionException>(() => dynSettings.Controller.RunExpression(null));

        }

        [Test]
        public void SliceList()
        {
            var model = dynSettings.Controller.DynamoModel;

            var data = new Dictionary<string, object> {{"name", "Partition List"}};
            model.CreateNode(data);

            //Create a List
            //For a list of 0..20, this will have 21 elements
            //Slicing by 5 should return 6 lists, the last containing one element
            var list = Utils.SequenceToFSharpList(Enumerable.Range(0, 21).Select(x => FScheme.Value.NewNumber(x)));

            var sliceNode = (dynSlice)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynSlice);
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
            list = Utils.SequenceToFSharpList(Enumerable.Range(0, 1).Select(x => FScheme.Value.NewNumber(x)));
            args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            res = sliceNode.Evaluate(args);
            Assert.AreEqual(1, ((FScheme.Value.List)res).Item.Count());
        }

        [Test]
        public void Diagonals()
        {
            var model = dynSettings.Controller.DynamoModel;

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

            var list = Utils.SequenceToFSharpList(Enumerable.Range(0, 20).Select(x => FScheme.Value.NewNumber(x)));

            var data = new Dictionary<string, object> {{"name", "Diagonal Left List"}};
            model.CreateNode(data);

            var leftNode = (dynDiagonalLeftList)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynDiagonalLeftList);
            var args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            var res = leftNode.Evaluate(args);

            Assert.AreEqual(8, ((FScheme.Value.List)res).Item.Count());

            model.Clear(null);

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
            model.CreateNode(data);

            var rightNode = (dynDiagonalRightList)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynDiagonalRightList);
            args = FSharpList<FScheme.Value>.Empty;
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(5), args);
            args = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewList(list), args);
            res = rightNode.Evaluate(args);

            Assert.AreEqual(8, ((FScheme.Value.List)res).Item.Count());
        }

        [Test]
        public void ReadImageFile()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\files");

            string openPath = Path.Combine(examplePath, "readImageFileTest.dyn");
            model.Open(openPath);

            //set the path to the image file
            var pathNode = (dynStringFilename)model.Nodes.First(x => x is dynStringFilename);
            pathNode.Value = Path.Combine(examplePath,"honey-badger.jpg");

            dynSettings.Controller.RunExpression(null);

            var watch = GetWatchNodeFromCurrentSpace(model, "4744f516-c6b5-421c-b7f1-1731610667bb");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(25, doubleWatchVal, 0.00001);
        }

        [Test]
        public void UsingDefaultValue()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\default_values");

            string openPath = Path.Combine(examplePath, "take-every-default.dyn");
            model.Open(openPath);

            var watch = GetWatchNodeFromCurrentSpace(model, "360f3b50-5f27-460a-a57a-bb6338064d98");

            // Run once
            dynSettings.Controller.RunExpression(null);

            var oldVal = watch.OldValue;
            Assert.IsNotNull(oldVal);
            Assert.IsTrue(oldVal.IsList);

            // Pretend we never ran
            model.Nodes.ForEach(
                x =>
                {
                    x.RequiresRecalc = true;
                    x.ResetOldValue();
                });

            // Make sure results are still consistent
            dynSettings.Controller.RunExpression(null);
            
            var newVal = watch.OldValue;
            Assert.IsNotNull(newVal);
            Assert.IsTrue(newVal.IsList);

            Assert.IsTrue(oldVal.Print() == newVal.Print());
        }
    }
}
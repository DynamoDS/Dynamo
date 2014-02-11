using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.FSchemeInterop;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using String = System.String;

namespace Dynamo.Tests
{
    internal class CoreDynTests : DynamoUnitTest
    {
        [Test]
        public void AddSubtractMapReduceFilterBasic()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\map_reduce_filter\map_reduce_filter.dyn");
            model.Open(openPath);


            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(28, model.CurrentWorkspace.Nodes.Count);

            // check an input value
            var node1 = model.CurrentWorkspace.NodeFromWorkspace("51ed7fed-99fa-46c3-a03c-2c076f2d0538");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(DoubleInput), node1);
            Assert.AreEqual("2", ((DoubleInput)node1).Value);
            
            // run the expression
            //DynamoCommands.RunCommand(DynamoCommands.RunExpressionCommand);
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

            // add-subtract -3.0
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4a2363b6-ef64-44f5-be64-18832586e574");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(-3.0, doubleWatchVal);

            // map - list of three 6's 
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fcad8d7a-1c9f-4604-a03b-53393e36ea0b");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(6, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(6, listWatchVal[2].GetDoubleFromFSchemeValue());

            // reduce - 6.0
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("e892c469-47e6-4006-baea-ec4afea5a04e");
            doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(6.0, doubleWatchVal);

            // filter - list of 6-10
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(8, listWatchVal[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(9, listWatchVal[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal[4].GetDoubleFromFSchemeValue());

        }

        /// <summary>
        /// Confirm that a node with multiple outputs evaluates successfully.
        /// </summary>
        [Test]
        public void MultipleOutputs()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "multi.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression();

            var splitListVal = model.CurrentWorkspace.FirstNodeFromWorkspace<DeCons>().OldValue;

            Assert.IsInstanceOf<FScheme.Value.List>(splitListVal);

            var outs = (splitListVal as FScheme.Value.List).Item;

            Assert.AreEqual(2, outs.Length);

            var out1 = outs[0];
            Assert.IsInstanceOf<FScheme.Value.Number>(out1);
            Assert.AreEqual(0, (out1 as FScheme.Value.Number).Item);

            var out2 = outs[1];
            Assert.IsInstanceOf<FScheme.Value.List>(out2);
            Assert.IsTrue((out2 as FScheme.Value.List).Item.IsEmpty);
        }

        [Test]
        public void PartialApplicationWithMultipleOutputs()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "partial-multi.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression();

            var firstWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("3005609b-ceaa-451f-9b6c-6ca957358ad6");

            Assert.IsInstanceOf<FScheme.Value.List>(firstWatch.OldValue);
            Assert.IsInstanceOf<FScheme.Value.Number>((firstWatch.OldValue as FScheme.Value.List).Item[0]);
            Assert.AreEqual(0, ((firstWatch.OldValue as FScheme.Value.List).Item[0] as FScheme.Value.Number).Item);

            var restWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("2787f566-7612-41d1-8cec-8212fea58c8b");

            Assert.IsInstanceOf<FScheme.Value.List>(restWatch.OldValue);
            Assert.IsInstanceOf<FScheme.Value.List>((restWatch.OldValue as FScheme.Value.List).Item[0]);
            Assert.IsTrue(((restWatch.OldValue as FScheme.Value.List).Item[0] as FScheme.Value.List).Item.IsEmpty);
        }

        [Test]
        public void Sequence()
        {
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\sequence\sequence.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
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
        public void Sorting()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\sorting\");

            string openPath = Path.Combine(examplePath, "sorting.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            var watchNode2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
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

            var values = new List<string> {"aaaaa", "bbb", "aa", "c", "dddd"};

            values.Sort(String.Compare);
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
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Add.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4c5889ac-7b91-4fb5-aaad-a2128b533279");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Subtract()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Subtract.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("a574df4e-2dff-4c06-bbb6-e9467060085f");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Multiply()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Multiply.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(4.0, doubleWatchVal);
        }

        [Test]
        public void Divide()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Divide.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4c650bcc-9f18-4d23-a769-34845fd50fab");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(1.0, doubleWatchVal);
        }

        [Test]
        public void Modulo()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Modulo.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4a780dfb-74b1-453a-86ef-2f4a5c46792e");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(0.0, doubleWatchVal);
        }

        [Test]
        public void Ceiling()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Ceiling.dyn");
            model.Open(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("97e58c7f-9082-4980-997a-d290cf8055e1");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();  
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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fb52d286-ebcc-449c-989e-e4ea94831125");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6a7b150e-f053-4b29-b672-007aa1acde24");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("430e086e-8cf0-4e89-abba-69dc1cd94058");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
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

                    Assert.AreEqual(dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes.Count, 11);

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

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4d9fb747-2e90-4571-9c8f-7d59ad14a939");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(0.0, doubleWatchVal, 0.00001);
        }

        [Test]
        public void StringInputNodeWorksWithSpecialCharacters()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core");
            string openPath = Path.Combine(examplePath, "StringInputTest.dyn");
            model.Open(openPath);

            var strNode = (StringInput)dynSettings.Controller.DynamoModel.Nodes.First(x => x is StringInput);
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

            var watch = (Watch)dynSettings.Controller.DynamoModel.Nodes.First(x => x is Watch);
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);

            //change the value of the list
            var numNode = (DoubleInput) Controller.DynamoModel.Nodes.Last(x => x is DoubleInput);
            numNode.Value = "3";
            dynSettings.Controller.RunExpression(null);
            Thread.Sleep(300);

            listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);

            //test the negative case to make sure it throws an error
            numNode.Value = "-1";
            Assert.Throws<AssertionException>(() => dynSettings.Controller.RunExpression(null));

        }

        [Test]
        public void SliceList()
        {
            dynSettings.Controller.DynamoModel.CreateNode(0, 0, "Partition List");

            //Create a List
            //For a list of 0..20, this will have 21 elements
            //Slicing by 5 should return 6 lists, the last containing one element
            var list = Utils.ToFSharpList(Enumerable.Range(0, 21).Select(x => FScheme.Value.NewNumber(x)));

            var sliceNode = (Slice)dynSettings.Controller.DynamoModel.Nodes.First(x => x is Slice);
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
            list = Utils.ToFSharpList(Enumerable.Range(0, 1).Select(x => FScheme.Value.NewNumber(x)));
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

            var list = Utils.ToFSharpList(Enumerable.Range(0, 20).Select(x => FScheme.Value.NewNumber(x)));

            model.CreateNode(0, 0, "Diagonal Left List");

            var leftNode = (DiagonalLeftList)dynSettings.Controller.DynamoModel.Nodes.First(x => x is DiagonalLeftList);
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

            model.CreateNode(0, 0, "Diagonal Right List");

            var rightNode = (DiagonalRightList)dynSettings.Controller.DynamoModel.Nodes.First(x => x is DiagonalRightList);
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
            var pathNode = (StringFilename)model.Nodes.First(x => x is StringFilename);
            pathNode.Value = Path.Combine(examplePath,"honey-badger.jpg");

            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("4744f516-c6b5-421c-b7f1-1731610667bb");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(25, doubleWatchVal, 0.00001);
        }

        [Test]
        public void UsingDefaultValue()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\default_values");

            string openPath = Path.Combine(examplePath, "take-every-default.dyn");
            model.Open(openPath);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("360f3b50-5f27-460a-a57a-bb6338064d98");

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

        [Test]
        public void Formula()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\formula");

            model.Open(Path.Combine(exPath, "formula-test.dyn"));

            var watches = new[]
            {
                "2a8f6086-dd36-49f6-b9c1-dfd5dbc683ea", 
                "226f0d3a-7578-46f8-9f60-9fc24dd82c48",
                "af0ccd4f-9fae-4f66-85eb-e5d58eb15fd8"
            }.Select(guid => model.CurrentWorkspace.NodeFromWorkspace<Watch>(guid));

            dynSettings.Controller.RunExpression(null);

            foreach (var watch in watches)
            {
                Assert.AreEqual((watch.OldValue as FScheme.Value.Number).Item, 19);   
            }
        }

        [Test]
        public void AndNode()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"and-test.dyn"));

            dynSettings.Controller.RunExpression();

            var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            Assert.IsAssignableFrom<FScheme.Value.Number>(watchValue);
            Assert.AreEqual(0, (watchValue as FScheme.Value.Number).Item);
        }

        [Test]
        public void OrNode()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"or-test.dyn"));

            dynSettings.Controller.RunExpression();

            var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            Assert.IsAssignableFrom<FScheme.Value.Number>(watchValue);
            Assert.AreEqual(1, (watchValue as FScheme.Value.Number).Item);
        }

        [Test]
        public void IfNode()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"if-test.dyn"));

            dynSettings.Controller.RunExpression();

            var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            Assert.IsAssignableFrom<FScheme.Value.String>(watchValue);
            Assert.AreEqual("can't divide by 0", (watchValue as FScheme.Value.String).Item);
        }

        [Test]
        public void PerformAllNode()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"begin-test.dyn"));

            var textAndFileName = @"test.txt";

            model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>().Value = textAndFileName;

            dynSettings.Controller.RunExpression();

            File.Delete(textAndFileName);

            var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            Assert.IsAssignableFrom<FScheme.Value.String>(watchValue);
            Assert.AreEqual(textAndFileName, (watchValue as FScheme.Value.String).Item);
        }

        [Test]
        public void Constants()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"constants-test.dyn"));

            dynSettings.Controller.RunExpression();

            Assert.Pass();
        }

        [Test]
        public void Thunks()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"thunk-test.dyn"));

            dynSettings.Controller.RunExpression();

            Assert.Pass();
        }

        [Test]
        public void MultithreadingWithFutureAndNow()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\multithreading");

            model.Open(Path.Combine(exPath, @"multithread-test.dyn"));

            dynSettings.Controller.RunExpression();

            Assert.Pass();
        }
    }
}
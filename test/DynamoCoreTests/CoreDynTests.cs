using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using String = System.String;
using System;


namespace Dynamo.Tests
{
    internal class CoreDynTests : DSEvaluationUnitTest
    {
        [Test]
        public void AddSubtractMapReduceFilterBasic()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\map_reduce_filter\map_reduce_filter.dyn");
            RunModel(openPath);


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
            AssertPreviewValue("4a2363b6-ef64-44f5-be64-18832586e574", -3.0);

            // map - list of three 6's 
            AssertPreviewValue("fcad8d7a-1c9f-4604-a03b-53393e36ea0b", new int[] {6,6,6});

            // reduce - 6.0
            AssertPreviewValue("e892c469-47e6-4006-baea-ec4afea5a04e", 6.0);

            // filter - list of 6-10
            AssertPreviewValue("41279a88-2f0b-4bd3-bef1-1be693df5c7e", new int[] {  6, 7, 8, 9, 10 });


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
            RunModel(openPath);

            Dictionary<int, object> validationData = new Dictionary<int,object>()
            {

                {0,0},
            };

            SelectivelyAssertPreviewValues("a4d6ecce-0fe7-483d-a4f2-cd8cddefa25c", validationData);

        }

        [Test]
		[Category("Failing")]
        public void PartialApplicationWithMultipleOutputs()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "partial-multi.dyn");
            RunModel(openPath);

            AssertPreviewValue("3005609b-ceaa-451f-9b6c-6ca957358ad6", new int[]{0});

            AssertPreviewValue("2787f566-7612-41d1-8cec-8212fea58c8b", new int[]{});
        }

        [Test]
        public void Sequence()
        {
            //TODO: cannot finish test until migration is completed for Sequence and Formula nodes
            Assert.Inconclusive("Deprecated: Sequence, Formula");

            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\sequence\sequence.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

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

            AssertPreviewValue(watchNode.GUID.ToString(), new int[]{ });
        }

        [Test]
        public void Sorting()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\sorting\");

            string openPath = Path.Combine(examplePath, "sorting.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

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

            AssertPreviewValue(watchNode1.GUID.ToString(), new List<string> { "aa", "aaaaa", "bbb", "c", "dddd" });
            AssertPreviewValue(watchNode2.GUID.ToString(), new List<string> { "c", "aa", "bbb", "dddd", "aaaaa" });
        }

        [Test]
        public void Add()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Add.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279" , 4.0);

        }

        [Test]
        public void Subtract()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Subtract.dyn");
            RunModel(openPath);

            AssertPreviewValue("a574df4e-2dff-4c06-bbb6-e9467060085f", 0.0);

        }

        [Test]
        public void Multiply()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Multiply.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c650bcc-9f18-4d23-a769-34845fd50fab", 4.0);

        }

        [Test]
        public void Divide()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Divide.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c650bcc-9f18-4d23-a769-34845fd50fab", 1.0);

        }

        [Test]
        public void Modulo()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Modulo.dyn");
            RunModel(openPath);

            AssertPreviewValue("4a780dfb-74b1-453a-86ef-2f4a5c46792e", 0.0);

        }

        [Test]
        public void Ceiling()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Ceiling.dyn");
            RunModel(openPath);

            AssertPreviewValue("97e58c7f-9082-4980-997a-d290cf8055e1", 2.0);

        }

        [Test]
        public void Floor()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Floor.dyn");
            RunModel(openPath);

            AssertPreviewValue("fb52d286-ebcc-449c-989e-e4ea94831125", 1.0);

        }

        [Test]
        public void Power()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Power.dyn");
            RunModel(openPath);

            AssertPreviewValue("6a7b150e-f053-4b29-b672-007aa1acde24", 4.0);

        }

        [Test]
        public void Round()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Round.dyn");
            RunModel(openPath);

            AssertPreviewValue("430e086e-8cf0-4e89-abba-69dc1cd94058", 1.0);

        }

        [Test]
        public void Sine()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Sine.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", 1.0);

        }

        [Test]
        public void Cosine()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Cosine.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", -1.0);

        }

        [Test]
        public void OpeningDynWithDyfMissingIsOkayAndRunsOkay()
        {
            Assert.DoesNotThrow(delegate
                {
                    var model = dynSettings.Controller.DynamoModel;
                    var examplePath = Path.Combine(GetTestDirectory(), @"core\CASE");
                    string openPath = Path.Combine(examplePath, "case_flip_matrix.dyn");

                    RunModel(openPath);

                    Assert.AreEqual(dynSettings.Controller.DynamoModel.CurrentWorkspace.Nodes.Count, 11);

                });
        }

        [Test]
        public void Tangent()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Tangent.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", 0.0);

        }

        [Test]
        public void StringInputNodeWorksWithSpecialCharacters()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core");
            string openPath = Path.Combine(examplePath, "StringInputTest.dyn");
            RunModel(openPath);

            AssertPreviewValue("a6e316b4-7054-42cd-a901-7bc6d4045c23",
                "A node\twith tabs, and\r\ncarriage returns,\r\nand !@#$%^&* characters, and also something \"in quotes\".");
        }

        [Test]
        public void Repeat()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core");
            string openPath = Path.Combine(examplePath, "RepeatTest.dyn");

            //open and run the expression
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);
            dynSettings.Controller.RunExpression(null);

            var watch = (Watch)dynSettings.Controller.DynamoModel.Nodes.First(x => x is Watch);
            var watchData = watch.GetValue(0);
            Assert.IsTrue(watchData.IsCollection);
            Assert.AreEqual(5, watchData.GetElements().Count);

            //change the value of the list
            var numNode = (DoubleInput)Controller.DynamoModel.Nodes.Last(x => x is DoubleInput);
            numNode.Value = "3";
            dynSettings.Controller.RunExpression(null);

            watchData = watch.GetValue(0);
            Assert.IsTrue(watchData.IsCollection);
            Assert.AreEqual(3, watchData.GetElements().Count);

            //test the negative case
            numNode.Value = "-1";
            dynSettings.Controller.RunExpression(null);
            Assert.IsNull(watch.GetValue(0).GetElements());
        }

        [Test]
        public void ReadImageFile()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\files");

            string openPath = Path.Combine(examplePath, "readImageFileTest.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            //set the path to the image file
            var pathNode = (DSCore.File.Filename)model.Nodes.First(x => x is DSCore.File.Filename);
            pathNode.Value = Path.Combine(examplePath,"honey-badger.jpg");

            RunCurrentModel();

            AssertPreviewValue("4744f516-c6b5-421c-b7f1-1731610667bb", 25);
        }

        [Test]
        public void TestExportToCSVFile()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\files");

            string openPath = Path.Combine(examplePath, "TestExportToCSVFile.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            //set the path to the csv file
            var pathNode = (DSCore.File.Filename)model.Nodes.First(x => x is DSCore.File.Filename);
            pathNode.Value = Path.Combine(examplePath, "TestExportToCSV.txt");

            //clean up the text file
            File.WriteAllText(pathNode.Value, String.Empty);

            RunCurrentModel();

            AssertPreviewValue("6cf3efb3-127f-4bbd-9008-25cc1ba15bd8", true);

            StreamReader sr = new StreamReader(pathNode.Value);
            String line = sr.ReadToEnd();

            StringAssert.AreEqualIgnoringCase("1, 2, 3, 4, 5\r\n-2, 2.6, 9\r\n0\r\n", line);
        }

        [Test]
        public void TestExportToCSVFile_Negativ()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\files");

            string openPath = Path.Combine(examplePath, "TestExportToCSVFile_Negative.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            RunCurrentModel();

            AssertPreviewValue("906cfd65-37fc-4f54-ac21-3d59a32feb5a", false);
        }



        [Test]
        public void UsingDefaultValue()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\default_values");

            string openPath = Path.Combine(examplePath, "take-every-default.dyn");
            RunModel(openPath);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("360f3b50-5f27-460a-a57a-bb6338064d98");
            var expectedValue = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
            var oldVal = watch.CachedValue;
            Assert.IsTrue(oldVal.IsCollection);
            AssertValue(oldVal, expectedValue);

            // Pretend we never ran
            model.Nodes.ForEach(
                x =>
                {
                    x.RequiresRecalc = true;
                });

            // Make sure results are still consistent
            dynSettings.Controller.RunExpression(null);

            var newVal = watch.CachedValue;
            Assert.IsTrue(newVal.IsCollection);
            AssertValue(newVal, expectedValue);
        }

        [Test]
        public void Formula()
        {
            Assert.Inconclusive();

            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\formula");

            Controller.DynamoViewModel.OpenCommand.Execute(Path.Combine(exPath, "formula-test.dyn"));

            var watches = new[]
            {
                "2a8f6086-dd36-49f6-b9c1-dfd5dbc683ea", 
                "226f0d3a-7578-46f8-9f60-9fc24dd82c48",
                "af0ccd4f-9fae-4f66-85eb-e5d58eb15fd8"
            }.Select(guid => model.CurrentWorkspace.NodeFromWorkspace<Watch>(guid));

            dynSettings.Controller.RunExpression(null);

            foreach (var watch in watches)
            {
                Assert.AreEqual(19, watch.CachedValue.Data);
            }
        }

        [Test]
        public void AndNode()
        {
            Assert.Inconclusive("Porting : Formula");

            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"and-test.dyn"));

            AssertPreviewValue("a3d8097e-1eb9-4ed0-8d48-9c14cdfb0340", 0.0);
        }

        [Test]
        public void OrNode()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"or-test.dyn"));

            AssertPreviewValue("a3d8097e-1eb9-4ed0-8d48-9c14cdfb0340", true);
        }

        [Test]
        public void IfNode()
        {
            Assert.Inconclusive("Porting : Formula");

            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"if-test.dyn"));

            AssertPreviewValue("317384f2-7921-49cb-b1d9-be8b2718bde1", "can't divide by 0");

        }

        [Test]
        public void PerformAllNode()
        {
            Assert.Inconclusive("Porting : FileWriter");
            
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            Controller.DynamoViewModel.OpenCommand.Execute(Path.Combine(exPath, @"begin-test.dyn"));

            var dummy = model.CurrentWorkspace.FirstNodeFromWorkspace<DSCoreNodesUI.DummyNode>();
            Assert.IsNotNull(dummy);

            Assert.Inconclusive("Test inconclusive due to Deprecated node");

            //const string textAndFileName = @"test.txt";
            //model.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>().Value = textAndFileName;

            //dynSettings.Controller.RunExpression();

            //File.Delete(textAndFileName);

            //var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            //Assert.IsAssignableFrom<string>(watchValue.Data);
            //Assert.AreEqual(textAndFileName, watchValue.Data);
        }

        [Test]
        public void Constants()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"constants-test.dyn"));

            Assert.Pass();
        }

        [Test]
        public void Thunks()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"thunk-test.dyn"));

            Assert.Pass();
        }

        [Test]
        public void MultithreadingWithFutureAndNow()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\multithreading");

            RunModel(Path.Combine(exPath, @"multithread-test.dyn"));

            Assert.Pass();
        }

        [Test]
        public void TestNumber_RangeExpr01()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\number");

            RunModel(Path.Combine(exPath, @"TestNumber_RangeExpr01.dyn"));

            AssertPreviewValue("572c5ff9-1b83-4c58-986f-f8f4453a6d09", new[] {1, 7, 13, 19});
            AssertPreviewValue("1f62b414-7118-4606-9924-32a4b09c32a9", new[] {1, -2, -5, -8, -11});
            AssertPreviewValue("2bed6a11-aceb-469b-ba59-79a1ac7b7396", new double[]{});
        }

        [Test]
        public void TestNumber_RangeExpr02()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\number");

            RunModel(Path.Combine(exPath, @"TestNumber_RangeExpr02.dyn"));

            AssertPreviewValue("d358da0e-9cfa-4562-b110-726133cc1be4", new[] { 5, 4, 3, 2, 1 });
            AssertPreviewValue("d3885e46-af86-4296-b76e-f736fb0613f4", new[] { 5, 4, 3, 2, 1 });
            AssertPreviewValue("efc44473-0708-4bc6-a0ea-e9e6eeed097b", null);
            AssertPreviewValue("34e34e69-1d42-45d7-8970-d3fc9ebf0ed7", new[] { 1, 2, 3, 4, 5 });
            AssertPreviewValue("7f62e8ca-d248-40bb-9139-7f421c4b44e6", new[] { 1, 2, 3, 4, 5 });
            AssertPreviewValue("5b3926a5-b920-4524-9031-f2c711523275", null);

            AssertPreviewValue("580f2bef-9ecb-45ae-b2cb-2701a125f546", new[] { 5, 4, 3, 2, 1 });
            AssertPreviewValue("5a766000-00ae-49a4-a1ad-0a528e7ddd8f", null);
            AssertPreviewValue("b827d3ef-9f50-4333-97de-380873017ffa", new[] { 5, 4, 3, 2, 1 });
            AssertPreviewValue("621ca605-c0b8-4350-99b0-01e48b56931b", new[] { 1, 2, 3, 4, 5 });
            AssertPreviewValue("18f6fa4f-beaa-4904-a78c-30411a6f8391", new[] { 1, 2, 3, 4, 5 });
            AssertPreviewValue("87eaf2fe-6a70-4f21-bf17-e33c286809d4", null);
        }

        [Test]
        public void TestNumber_RangeExpr03()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\number");

            RunModel(Path.Combine(exPath, @"TestNumber_RangeExpr03.dyn"));

            AssertPreviewValue("d358da0e-9cfa-4562-b110-726133cc1be4", new[] { 5, 4, 3, 2, 1 });

            AssertPreviewValue("580f2bef-9ecb-45ae-b2cb-2701a125f546", new[] { 5, 4, 3, 2, 1 });
        }

        [Test]
        public void TestNumber_RangeExpr04()
        {
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\number");

            RunModel(Path.Combine(exPath, @"TestNumber_RangeExpr04.dyn"));

            AssertPreviewValue("e9ad17aa-e30f-4fcb-9d43-71ec2ab027f4", new[] { 5, 4, 3, 2, 1 });
        }
    }
}
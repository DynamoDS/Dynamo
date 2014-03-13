using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using String = System.String;


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
            Assert.IsAssignableFrom(typeof(CodeBlockNodeModel), node1);
            Assert.AreEqual("2;", ((CodeBlockNodeModel)node1).Code);
            
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

                {1,0},
            };

            SelectivelyAssertPreviewValues("a4d6ecce-0fe7-483d-a4f2-cd8cddefa25c", validationData);

        }

        [Test]
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

            //TODO: cannot finish test until migration is completed for Sequence and Formula nodes
            Assert.Inconclusive("Deprecated: Sequence, Formula");

            AssertPreviewValue(watchNode.GUID.ToString(), new int[]{ });
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
                "A node\twith tabs, and\ncarriage returns,\nand !@#$%^&amp;* characters, and also something &quot;in quotes&quot;.");
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
            var watchData = watch.GetValue(0);
            Assert.IsTrue(watchData.IsCollection);
            Assert.AreEqual(5, watchData.GetElements().Count);

            //change the value of the list
            var numNode = (DoubleInput)Controller.DynamoModel.Nodes.Last(x => x is DoubleInput);
            numNode.Value = "3";
            dynSettings.Controller.RunExpression(null);
            Thread.Sleep(300);

            watchData = watch.GetValue(0);
            Assert.IsTrue(watchData.IsCollection);
            Assert.AreEqual(3, watchData.GetElements().Count);

            //test the negative case to make sure it throws an error
            numNode.Value = "-1";
            Assert.Throws<AssertionException>(() => dynSettings.Controller.RunExpression(null));
        }

        [Test]
        public void ReadImageFile()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\files");

            string openPath = Path.Combine(examplePath, "readImageFileTest.dyn");
            model.Open(openPath);

            //set the path to the image file
            var pathNode = (DSCore.File.Filename)model.Nodes.First(x => x is DSCore.File.Filename);
            pathNode.Value = Path.Combine(examplePath,"honey-badger.jpg");

            RunCurrentModel();

            AssertPreviewValue("4744f516-c6b5-421c-b7f1-1731610667bb", 25);
        }

        [Test]
        public void UsingDefaultValue()
        {
            var model = dynSettings.Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\default_values");

            string openPath = Path.Combine(examplePath, "take-every-default.dyn");
            RunModel(openPath);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("360f3b50-5f27-460a-a57a-bb6338064d98");

            var oldVal = watch.OldValue;
            Assert.IsNotNull(oldVal.Data);
            Assert.IsTrue(oldVal.IsCollection);

            // Pretend we never ran
            model.Nodes.ForEach(
                x =>
                {
                    x.RequiresRecalc = true;
                });

            // Make sure results are still consistent
            dynSettings.Controller.RunExpression(null);

            var newVal = watch.OldValue;
            Assert.IsNotNull(newVal.Data);
            Assert.IsTrue(newVal.IsCollection);

            Assert.AreEqual(oldVal, newVal);
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
                Assert.AreEqual(19, watch.OldValue.Data);
            }
        }

        [Test]
        public void AndNode()
        {
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
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            RunModel(Path.Combine(exPath, @"if-test.dyn"));

            AssertPreviewValue("317384f2-7921-49cb-b1d9-be8b2718bde1", "can't divide by 0");

        }

        [Test]
        public void PerformAllNode()
        {
            
            var model = dynSettings.Controller.DynamoModel;
            var exPath = Path.Combine(GetTestDirectory(), @"core\customast");

            model.Open(Path.Combine(exPath, @"begin-test.dyn"));

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
    }
}
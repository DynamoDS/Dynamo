using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using String = System.String;


namespace Dynamo.Tests
{
    internal class CoreDynTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DSOffice.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void AddSubtractMapReduceFilterBasic()
        {
            string openPath = Path.Combine(TestDirectory, @"core\map_reduce_filter\map_reduce_filter.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(28, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // check an input value
            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("51ed7fed-99fa-46c3-a03c-2c076f2d0538");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(DoubleInput), node1);
            Assert.AreEqual("2", ((DoubleInput)node1).Value);

            // run the expression
            //DynamoCommands.RunCommand(DynamoCommands.RunExpressionCommand);
            BeginRun();

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

            // add-subtract -3.0
            AssertPreviewValue("4a2363b6-ef64-44f5-be64-18832586e574", -3.0);

            // map - list of three 6's 
            AssertPreviewValue("fcad8d7a-1c9f-4604-a03b-53393e36ea0b", new[] { 6, 6, 6 });

            // reduce - 6.0
            AssertPreviewValue("e892c469-47e6-4006-baea-ec4afea5a04e", 6.0);

            // filter - list of 6-10
            AssertPreviewValue("41279a88-2f0b-4bd3-bef1-1be693df5c7e", new[] { 6, 7, 8, 9, 10 });
        }

        /// <summary>
        /// Confirm that node model derived nodes deserialize into correct node state.
        /// </summary>
        [Test]
        public void verifyNodeStates()
        {
            // Open/Run XML test graph
            string openPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            RunModel(openPath);

            // Check dead node XML
            var deadNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("1237a148-7a90-489d-b677-11038072c288");
            Assert.AreEqual(ElementState.Dead, deadNode.State);
            // Check warning node XML
            var warningNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("50219c24-e583-4b85-887c-409fb062da6e");
            Assert.AreEqual(ElementState.Warning, warningNode.State);
            // Check active node XML
            var activeNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("72136fa9-7aec-4ed5-a23b-1ee1c13294a6");
            Assert.AreEqual(ElementState.Active, activeNode.State);

            // Save/Open/Run JSON graph
            string tempPath = Path.Combine(Path.GetTempPath(), "NodeStates.dyn");
            CurrentDynamoModel.CurrentWorkspace.Save(tempPath);
            CurrentDynamoModel.OpenFileFromPath(tempPath);
            CurrentDynamoModel.CurrentWorkspace.RequestRun();

            // Check dead node JSON
            deadNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("1237a148-7a90-489d-b677-11038072c288");
            Assert.AreEqual(ElementState.Dead, deadNode.State);
            // Check warning node JSON
            warningNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("50219c24-e583-4b85-887c-409fb062da6e");
            Assert.AreEqual(ElementState.Warning, warningNode.State);
            // Check active node JSON
            activeNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("72136fa9-7aec-4ed5-a23b-1ee1c13294a6");
            Assert.AreEqual(ElementState.Active, activeNode.State);

            // Delete temp graph file
            File.Delete(tempPath);
        }


        /// <summary>
        /// Confirm that a node with multiple outputs evaluates successfully.
        /// </summary>
        [Test]
        public void MultipleOutputs()
        {
            string openPath = Path.Combine(TestDirectory, @"core\multiout\multi.dyn");
            RunModel(openPath);

            var empty = new List<object>();
            var validationData = DesignScript.Builtin.Dictionary.ByKeysValues(
                new[] {"first", "rest"}, new object[] {0, empty});

            AssertPreviewValue("a4d6ecce-0fe7-483d-a4f2-cd8cddefa25c", validationData);
        }

        [Test]
        public void PartialApplicationWithMultipleOutputs()
        {
            string openPath = Path.Combine(TestDirectory, @"core\multiout\partial-multi.dyn");
            RunModel(openPath);

            AssertPreviewValue("3005609b-ceaa-451f-9b6c-6ca957358ad6", new int[] { 0 });

            AssertPreviewValue("2787f566-7612-41d1-8cec-8212fea58c8b", new int[] { });
        }

        [Test]
        public void Sequence()
        {
            string openPath = Path.Combine(TestDirectory, @"core\sequence\sequence.dyn");
            OpenModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // run the expression
            BeginRun();

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Assert.IsNotNull(watchNode);

            AssertPreviewValue(watchNode.GUID.ToString(), new int[] { });
        }

        [Test]
        public void DefaultSequence()
        {
            RunModel(@"core\sequence\DefaultSequence.dyn");
            AssertPreviewValue("6d7f8652-6cf2-4749-b398-922992fa484b",
                "Function");
        }

        [Test]
        public void DefaultRange()
        {
            RunModel(@"core\range\DefaultRange.dyn");
            AssertPreviewValue("24323e5c-6d36-4b18-b99d-fa953eafeb73",
                "Function");
        }

        [Test]
        public void DefaultNumberSlider()
        {
            RunModel(@"core\slider\DefaultNumberSlider.dyn");
            AssertPreviewValue("120c4ade-a49c-4aac-b3ff-02e41562d3ad", 1.0);
        }

        [Test]
        public void DefaultIntegerSlider()
        {
            RunModel(@"core\slider\DefaultIntegerSlider.dyn");
            AssertPreviewValue("35e5118e-c118-4690-bcef-ca5e601eac72", 1.0);
        }

        [Test]
        public void TestRangeMap()
        {
            RunModel(@"core\range\RangeMap.dyn");
            AssertPreviewValue("251ec88d-200d-4b75-95c8-1f9dfa540eba",
                new[] { new[] { 5, 6, 7, 8, 9, 10 }, new[] { 5, 7, 9 } });
            AssertPreviewValue("54a3f2af-e286-4a5b-8b04-977d47b6cfe3",
                new[] { new[] { 1, 2, 3, 4, 5 }, new[] { 2, 3, 4, 5 } });
            AssertPreviewValue("462fc8d4-1261-4251-8149-3ae9f4807591",
                new[] { new[] { 5 }, new[] { 5, 6, 7, 8, 9, 10 } });
        }

        [Test]
        public void TestSequenceMap()
        {
            RunModel(@"core\sequence\SequenceMap.dyn");
            AssertPreviewValue("251ec88d-200d-4b75-95c8-1f9dfa540eba",
                new[] { new[] { 5, 6, 7 }, new[] { 5, 7, 9 } });
            AssertPreviewValue("54a3f2af-e286-4a5b-8b04-977d47b6cfe3",
                new[] { new[] { 1, 3, 5, 7, 9 }, new[] { 2, 4, 6, 8, 10 } });
            AssertPreviewValue("462fc8d4-1261-4251-8149-3ae9f4807591",
                new[] { new[] { 5, 6, 7, 8, 9 }, new[] { 5, 6, 7 } });
        }

        [Test]
        public void Sorting()
        {
            string openPath = Path.Combine(TestDirectory, @"core\sorting\sorting.dyn");
            OpenModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // run the expression
            BeginRun();

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            var watchNode2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
            Assert.IsNotNull(watchNode1);
            Assert.IsNotNull(watchNode2);

            AssertPreviewValue(watchNode1.GUID.ToString(), new List<string> { "aa", "aaaaa", "bbb", "c", "dddd" });
            AssertPreviewValue(watchNode2.GUID.ToString(), new List<string> { "c", "aa", "bbb", "dddd", "aaaaa" });
        }

        [Test]
        public void Add()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Add.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void Subtract()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Subtract.dyn");
            RunModel(openPath);

            AssertPreviewValue("a574df4e-2dff-4c06-bbb6-e9467060085f", 0.0);
        }

        [Test]
        public void Multiply()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Multiply.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c650bcc-9f18-4d23-a769-34845fd50fab", 4.0);
        }

        [Test]
        public void Divide()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Divide.dyn");
            RunModel(openPath);

            AssertPreviewValue("4c650bcc-9f18-4d23-a769-34845fd50fab", 1.0);
        }

        [Test]
        public void Modulo()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Modulo.dyn");
            RunModel(openPath);

            AssertPreviewValue("4a780dfb-74b1-453a-86ef-2f4a5c46792e", 0.0);
            AssertPreviewValue("4839daee-5add-4686-a89d-dd36ec868993", 3);
            AssertPreviewValue("2d1dd88e-6d47-48c5-9b79-560a6bf406ee", 3.0);
        }

        [Test]
        public void ModuloDivisionByZero()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\ModuloZero.dyn");
            RunModel(openPath);

            AssertPreviewValue("75647d42-ff81-4ae0-9f44-2e68c9942633", new object[] { null, 0, 1, 0, 3 });
        }


        [Test]
        public void Ceiling()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Ceiling.dyn");
            RunModel(openPath);

            AssertPreviewValue("97e58c7f-9082-4980-997a-d290cf8055e1", 2.0);
        }

        [Test]
        public void Floor()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Floor.dyn");
            RunModel(openPath);

            AssertPreviewValue("fb52d286-ebcc-449c-989e-e4ea94831125", 1.0);
        }

        [Test]
        public void Power()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Power.dyn");
            RunModel(openPath);

            AssertPreviewValue("6a7b150e-f053-4b29-b672-007aa1acde24", 4.0);
        }

        [Test]
        public void Round()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Round.dyn");
            RunModel(openPath);

            AssertPreviewValue("430e086e-8cf0-4e89-abba-69dc1cd94058", 1.0);
        }

        [Test]
        public void Sine()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Sine.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", 1.0);
        }

        [Test]
        public void Cosine()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Cosine.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", -1.0);
        }

        [Test]
        public void OpeningDynWithDyfMissingIsOkayAndRunsOkay()
        {
            string openPath = Path.Combine(TestDirectory, @"core\CASE\case_flip_matrix.dyn");
            RunModel(openPath);

            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void Tangent()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Tangent.dyn");
            RunModel(openPath);

            AssertPreviewValue("4d9fb747-2e90-4571-9c8f-7d59ad14a939", 0.0);
        }

        [Test]
        public void StringInputNodeWorksWithSpecialCharacters()
        {
            string openPath = Path.Combine(TestDirectory, @"core\StringInputTest.dyn");
            RunModel(openPath);

            var watch = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            AssertPreviewValue(watch.GUID.ToString(),
                "A node\twith tabs, and\r\ncarriage returns,\r\nand !@#$%^&* characters, and also something \"in quotes\".");
        }

        [Test]
        public void Repeat()
        {
            string openPath = Path.Combine(TestDirectory, @"core\RepeatTest.dyn");

            //open and run the expression
            OpenModel(openPath);
            BeginRun();

            var watch = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            AssertPreviewValue(watch.GUID.ToString(), new[] { 5, 5, 5, 5, 5 });

            //change the value of the list
            var numNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DoubleInput>().Last();
            numNode.Value = "3";
            BeginRun();

            AssertPreviewValue(watch.GUID.ToString(), new[] { 5, 5, 5 });
        }

        [Test]
        public void RepeatFail()
        {
            string openPath = Path.Combine(TestDirectory, @"core\RepeatTest.dyn");

            //open and run the expression
            OpenModel(openPath);
            BeginRun();

            var watch = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            var numNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DoubleInput>().Last();

            //test the negative case
            numNode.Value = "-1";
            BeginRun();
            AssertPreviewValue(watch.GUID.ToString(), null);
        }

        [Test]
        public void ReadImageFile()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\files");
            string openPath = Path.Combine(examplePath, "readImageFileTest.dyn");
            OpenModel(openPath);

            //set the path to the image file
            var pathNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();
            pathNode.Value = Path.Combine(examplePath, "honey-badger.jpg");

            BeginRun();

            AssertPreviewValue("4744f516-c6b5-421c-b7f1-1731610667bb", 25);
        }

        [Test]
        public void CanOpenBadFile()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\files");
            string openPath = Path.Combine(examplePath, "Dummy.dyn");
            try
            {
                OpenModel(openPath);
            }
            catch (System.Exception e)
            {
                Assert.IsTrue(e is System.Xml.XmlException || e is Newtonsoft.Json.JsonReaderException);
            }
        }

        [Test]
        public void CanOpenGoodFileWithoutExtension()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\files");
            string openPath = Path.Combine(examplePath, "Sphere");
            OpenModel(openPath);
            BeginRun();
        }

        [Test]
        public void TestExportToCSVFile()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\files");
            string openPath = Path.Combine(examplePath, "TestExportToCSVFile.dyn");
            OpenModel(openPath);

            //set the path to the csv file
            var pathNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();
            pathNode.Value = Path.Combine(examplePath, "TestExportToCSV.txt");

            //clean up the text file
            File.WriteAllText(pathNode.Value, String.Empty);

            BeginRun();

            using (var sr = new StreamReader(pathNode.Value))
            {
                String line = sr.ReadToEnd();
                StringAssert.AreEqualIgnoringCase("1,2,3,4,5\r\n-2,2.6,9\r\n0\r\n", line);
            }
        }

        [Test]
        public void TestExportToCSVFile_Negativ()
        {
            string openPath = Path.Combine(TestDirectory, @"core\files\TestExportToCSVFile_Negative.dyn");
            OpenModel(openPath);

            BeginRun();
        }

        [Test]
        public void UsingDefaultValue()
        {
            string openPath = Path.Combine(TestDirectory, @"core\default_values\take-every-default.dyn");
            RunModel(openPath);

            var watch = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("360f3b50-5f27-460a-a57a-bb6338064d98");
            var expectedValue = new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
            var oldVal = watch.CachedValue;
            Assert.IsTrue(oldVal is ICollection);
            Assert.AreEqual(oldVal, expectedValue);

            // Pretend we never ran
            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                node.MarkNodeAsModified(true);
            }

            // Make sure results are still consistent
            BeginRun();

            var newVal = watch.CachedValue;
            Assert.IsTrue(newVal is ICollection);
            Assert.AreEqual(newVal, expectedValue);
        }

        [Test]
        public void Formula()
        {
            OpenModel(Path.Combine(TestDirectory, @"core\formula\formula-test.dyn"));

            var watches = new[]
            {
                "2a8f6086-dd36-49f6-b9c1-dfd5dbc683ea", 
                "226f0d3a-7578-46f8-9f60-9fc24dd82c48"
                //"af0ccd4f-9fae-4f66-85eb-e5d58eb15fd8"
            }.Select(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>);

            BeginRun();

            foreach (var watch in watches)
            {
                Assert.AreEqual(19, watch.CachedValue);
            }
        }

        [Test]
        public void AndNode()
        {
            RunModel(Path.Combine(TestDirectory, @"core\customast\and-test.dyn"));

            AssertPreviewValue("a3d8097e-1eb9-4ed0-8d48-9c14cdfb0340", 0.0);
        }

        [Test]
        public void OrNode()
        {
            RunModel(Path.Combine(TestDirectory, @"core\customast\or-test.dyn"));

            AssertPreviewValue("a3d8097e-1eb9-4ed0-8d48-9c14cdfb0340", true);
        }

        [Test]
        public void IfNode()
        {
            RunModel(Path.Combine(TestDirectory, @"core\customast\if-test.dyn"));

            AssertPreviewValue("317384f2-7921-49cb-b1d9-be8b2718bde1", "can't divide by 0");
        }

        [Test]
        public void PerformAllNode()
        {
            OpenModel(Path.Combine(TestDirectory, @"core\customast\begin-test.dyn"));

            var dummy = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DummyNode>();
            Assert.IsNotNull(dummy);

            const string textAndFileName = @"test.txt";
            CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<StringInput>().Value = textAndFileName;

            BeginRun();

            File.Delete(textAndFileName);

            //var watchValue = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>().OldValue;

            //Assert.IsAssignableFrom<string>(watchValue.Data);
            //Assert.AreEqual(textAndFileName, watchValue.Data);
        }

        [Test]
        public void Constants()
        {
            RunModel(Path.Combine(TestDirectory, @"core\customast\constants-test.dyn"));
        }

        [Test]
        public void Thunks()
        {
            RunModel(Path.Combine(TestDirectory, @"core\customast\thunk-test.dyn"));
        }

        [Test]
        public void MultithreadingWithFutureAndNow()
        {
            RunModel(Path.Combine(TestDirectory, @"core\multithreading\multithread-test.dyn"));
        }

        [Test]
        public void TestNumber_RangeExpr01()
        {
            RunModel(Path.Combine(TestDirectory, @"core\number\TestNumber_RangeExpr01.dyn"));

            AssertPreviewValue("572c5ff9-1b83-4c58-986f-f8f4453a6d09", new[] { 1, 7, 13, 19 });
            AssertPreviewValue("1f62b414-7118-4606-9924-32a4b09c32a9", new[] { 1, -2, -5, -8, -11 });
            AssertPreviewValue("2bed6a11-aceb-469b-ba59-79a1ac7b7396", new double[] { });
        }

        [Test]
        public void TestNumber_RangeExpr02()
        {
            RunModel(Path.Combine(TestDirectory, @"core\number\TestNumber_RangeExpr02.dyn"));

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
            RunModel(Path.Combine(TestDirectory, @"core\number\TestNumber_RangeExpr03.dyn"));

            AssertPreviewValue("d358da0e-9cfa-4562-b110-726133cc1be4", new[] { 5, 4, 3, 2, 1 });
            AssertPreviewValue("580f2bef-9ecb-45ae-b2cb-2701a125f546", new[] { 5, 4, 3, 2, 1 });
        }

        [Test]
        public void TestNumber_RangeExpr04()
        {
            RunModel(Path.Combine(TestDirectory, @"core\number\TestNumber_RangeExpr04.dyn"));

            AssertPreviewValue("e9ad17aa-e30f-4fcb-9d43-71ec2ab027f4", new[] { 5, 4, 3, 2, 1 });
        }

        [Test]
        public void TestBigNumber()
        {
            RunModel(Path.Combine(TestDirectory, @"core\number\TestBigNumber.dyn"));
            long value = 6640000000;
            AssertPreviewValue("b64d00bd-695b-496f-91e2-45caadd56535", value);
            AssertPreviewValue("8540896e-90c8-45aa-a1b3-b2d93d98668d", value);
        }
    }
}

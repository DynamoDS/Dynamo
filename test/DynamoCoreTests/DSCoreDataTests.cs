using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class DSCoreDataTests : DynamoModelTestBase
    {
        // Preload required libraries
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void LoadJSONFromFile()
        {
            // Load JSON file 
            string jsonFilePath = Path.Combine(TestDirectory, @"core\json\JSONFile.json");
            JObject jsonObject = JObject.Parse(File.ReadAllText(jsonFilePath));

            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);
            AssertNoDummyNodes();

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var engine = CurrentDynamoModel.EngineController;
            // Get Dictionary Components node
            Guid testNodeGuid = Guid.Parse("aa367b7b-22c5-492e-be30-9690c8a45960");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Get test node data
            var rawVal = testNode.GetValue(0, engine).Data;

            // Verify the test node is returning a valid dictionary
            Assert.AreEqual(typeof(DesignScript.Builtin.Dictionary).FullName, rawVal.GetType().FullName);

            // Compare the keys and values of the node result against the test loaded result
            var dictionary = (rawVal as DesignScript.Builtin.Dictionary);

            var nodeKeys = dictionary.Keys.ToList();
            var nodeValues = dictionary.Values.ToList();
            var localKeys = jsonObject.Properties().Select(p => p.Name).ToList();
            var localValues = jsonObject.Properties().Select(p => p.Value).ToList();

            // Sort
            nodeKeys.Sort();
            localKeys.Sort();

            // Verify list lengths
            Assert.AreEqual(nodeKeys.Count, localKeys.Count);
            Assert.AreEqual(nodeValues.Count, nodeValues.Count);

            // Verify keys
            Assert.AreEqual(nodeKeys, localKeys);
        }

        [Test]
        [Category("UnitTests")]
        public void CompareDictionaryAndJSON()
        {
            // Load JSON file 
            string jsonFilePath = Path.Combine(TestDirectory, @"core\json\JSONFile.json");
            JObject jsonObject = JObject.Parse(File.ReadAllText(jsonFilePath));

            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Assert all keys match between Dynamo Dictionary and parsed JSON
            AssertPreviewValue("5b0b1aba-ee6b-420e-aa12-e54270c00718", true);

            // Assert all values match between Dynamo Dictionary and parsed JSON
            AssertPreviewValue("5b0b1aba-ee6b-420e-aa12-e54270c00718", true);
        }

        [Test]
        [Category("UnitTests")]
        public void ParseJSON()
        {
            // Open the test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Guid testNodeGuid = Guid.Parse("9dca6adc-dcf2-436a-9317-43a0af5195bc");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Expected parsed types
            string[] expectedOutputs = new string[]
            {
                    typeof(System.Boolean).FullName,
                    typeof(System.Boolean).FullName,
                    typeof(DesignScript.Builtin.Dictionary).FullName,
                    typeof(System.String).FullName,
                    typeof(System.Int64).FullName,
                    typeof(System.DateTime).FullName,
                    typeof(System.Double).FullName,
                    typeof(DesignScript.Builtin.Dictionary).FullName
            };

            // Verify node output types match expected output
            AssertPreviewValue(testNode.GUID.ToString(), expectedOutputs);
        }

        [Test]
        [Category("UnitTests")]
        public void StringifyJSON()
        {
            // Open the test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Test.dyn");
            OpenModel(path);

            // Get node data
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Guid testNodeGuid = Guid.Parse("31f973f9-0764-47e1-b770-7b6b97d8803e");
            NodeModel testNode = workspace.NodeFromWorkspace(testNodeGuid);

            // Expected parsed types
            var expectedString = typeof(System.String).FullName;
            var expectedChar = typeof(System.Char).FullName;

            // Verify node output types match expected output
            AssertPreviewValue(testNode.GUID.ToString(), new[]
            {
                expectedString, expectedString, expectedChar, expectedString, expectedString, expectedString,
                expectedString, expectedString

            });
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyStringifyReplication()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_Replication.dyn");
            OpenModel(path);

            // Verify applying replication produces different result
            AssertPreviewValue("b69f2b6e-a328-43cf-a7a3-db42576ce814", true);

            // Verify DesignScript and node stringify functionality produces the same results - no replication.
            AssertPreviewValue("e81d053f-dbc5-4b1e-86df-075c9b279aa3", true);

            // Verify DesignScript and node stringify functionality produces the same results - with replication.
            AssertPreviewValue("a0af3136-aaaf-40c2-b2ef-cee522f9ea45", true);
        }

        [Test]
        [Category("UnitTests")]
        public void VerifyDesignScriptMatchesNodes()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_DesignScript.dyn");
            OpenModel(path);
            AssertNoDummyNodes();

            // Verify DesignScript usage of ParseJSON and StringifyJSON results match nodes
            AssertPreviewValue("5df5ed6d-6270-4018-a479-4f7cefcf7fe8", true);
        }

        [Test]
        [Category("UnitTests")]
        public void ParsingJSONInPythonReturnsSameResult()
        {
            // Load test graph
            string path = Path.Combine(TestDirectory, @"core\json\JSON_Nodes_PythonJSONParsing.dyn");
            OpenModel(path);

            // Verify keys match when parsing JSON via Python
            AssertPreviewValue("e4e600d9-12a6-400e-adb3-02c1ad26cddf", true);

            // Verify values match when parsing JSON via Python
            AssertPreviewValue("cdad5bf1-f5f7-47f4-a119-ad42e5084cfa", true);
        }
    }
}

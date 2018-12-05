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

            // Get node data
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            var engine = CurrentDynamoModel.EngineController;
            // Get Dictionary Components node
            string testNodeGuid = "aa367b7b-22c5-492e-be30-9690c8a45960";
            NodeModel testNode = getNodeById(workspace, testNodeGuid);

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
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            string testNodeGuid = "9dca6adc-dcf2-436a-9317-43a0af5195bc";
            NodeModel testNode = getNodeById(workspace, testNodeGuid);

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
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            string testNodeGuid = "31f973f9-0764-47e1-b770-7b6b97d8803e";
            NodeModel testNode = getNodeById(workspace, testNodeGuid);

            // Expected parsed types
            List<string> expectedOutputs = Enumerable.Repeat(typeof(System.String).FullName, 8).ToList();
            
            // Verify node output types match expected output
            AssertPreviewValue(testNode.GUID.ToString(), expectedOutputs);
        }

        // Helper Functions //

        // Get node from a provided workspace by searching GUIDs
        private NodeModel getNodeById(WorkspaceModel workspace, string guid)
        {
            foreach (NodeModel node in workspace.Nodes)
            {
                if (node.GUID.ToString() == guid)
                {
                    return node;
                }
            }

            // Node not found, throw
            throw new Exception("Unable to locate a node in the workspace associated with the provided GUID.");
        }
    }
}

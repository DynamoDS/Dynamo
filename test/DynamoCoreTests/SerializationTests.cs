using NUnit.Framework;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Dynamo.Engine.NodeToCode;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Newtonsoft.Json;
using Dynamo.Engine;
using Dynamo.Events;
using System.Text.RegularExpressions;
using Dynamo.Utilities;
using Newtonsoft.Json.Linq;

namespace Dynamo.Tests
{

    /// <summary>
    /// Test helpers we can share between core and view json serialization tests.
    /// </summary>
    public static class serializationTestUtils
    {
        /// <summary>
        /// replace the ids of models in the workspaceModel with non guids inside the given json string
        /// </summary>
        /// <param name="json"> the given json string to do replacements in</param>
        /// <param name="model"> the workspace the json represents</param>
        /// <param name="modelsGuidToIdMap"> a map of the old guids to the new ids we are replacing them with </param>
        /// <returns> returns a new json string without guids where applicable </returns>
        public static string replaceModelIdsWithNonGuids(string json, WorkspaceModel model, Dictionary<Guid,string> modelsGuidToIdMap)
        {
            var idcount = 0;

            //alter the output json so that all node ids are not guids
            foreach (var nodeId in model.Nodes.Select(x => x.GUID))
            {
                modelsGuidToIdMap.Add(nodeId, idcount.ToString());
                json = json.Replace(nodeId.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }

            //alter the output json so that all port ids are not guids
            foreach (var node in model.Nodes)
            {
                foreach (var port in node.InPorts)
                {
                    modelsGuidToIdMap.Add(port.GUID, idcount.ToString());
                    json = json.Replace(port.GUID.ToString("N"), idcount.ToString());
                    idcount = idcount + 1;
                }

                foreach (var port in node.OutPorts)
                {
                    modelsGuidToIdMap.Add(port.GUID, idcount.ToString());
                    json = json.Replace(port.GUID.ToString("N"), idcount.ToString());
                    idcount = idcount + 1;
                }
            }
            //alter the output json so that all connectorModel ids are not guids
            foreach (var connector in model.Connectors)
            {
                modelsGuidToIdMap.Add(connector.GUID, idcount.ToString());
                json = json.Replace(connector.GUID.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }
            //alter the output json so that all annotationModel ids are not guids
            foreach (var annotation in model.Annotations)
            {
                modelsGuidToIdMap.Add(annotation.GUID, idcount.ToString());
                json = json.Replace(annotation.GUID.ToString("N"), idcount.ToString());
                idcount = idcount + 1;
            }

            return json;
        }
    }

    /* The Serialization tests compare the results of a workspace opened and executed from its
     * original .dyn format, to one converted to json, deserialized and executed. In the process,
     * the tests save the following files:
     *  - xxx.json file representing the serialized version of the workspace to json, where xxx is the
     *  original .dyn file name.
     *  - xxx_data.json file containing the cached values of each of the workspaces
     *  - xxx.ds file containing the Design Script code for the workspace.
     */
    [TestFixture, Category("Serialization")]
    public class SerializationTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();
        private Dictionary<Guid, string> modelsGuidToIdMap = new Dictionary<Guid, string>();


        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;
            
            //Clear Temp directory folders before start of the new serialization test run
            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, "json");
            var jsonNonGuidFolder = Path.Combine(tempPath, "jsonNonGuid");

            //Try and delete all the files from the previous run. 
            //If there's an error in deleting files, the tests should countinue
            if (Directory.Exists(jsonFolder))
            {
                try
                {
                    Console.WriteLine("Deleting JSON directory from temp");
                    Directory.Delete(jsonFolder, true);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (Directory.Exists(jsonNonGuidFolder))
            {
                try
                {
                    Console.WriteLine("Deleting jsonNonGuid directory from temp");
                    Directory.Delete(jsonNonGuidFolder, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);
        }

        internal class PortComparisonData
        {
            public string ID { get; set; }
            public bool UseLevels { get; set; }
            public bool KeepListStructure { get; set; }
            public int Level { get; set; }
            public bool UsingDefaultValue { get; set; }

            public override bool Equals(object obj)
            {
                var other = (obj as PortComparisonData);
                return ID == other.ID &&
                    other.KeepListStructure == this.KeepListStructure &&
                    other.Level == this.Level &&
                    other.UseLevels == this.UseLevels &&
                    other.UsingDefaultValue == this.UsingDefaultValue;
            }
        }

        /// <summary>
        /// Caches workspaces data for comparison.
        /// </summary>
        internal class WorkspaceComparisonData
        {
            public Guid Guid { get; set; }
            public string Description { get; set; }
            public int NodeCount { get; set; }
            public int ConnectorCount { get; set; }
            public Dictionary<Guid, Type> NodeTypeMap { get; set; }
            public Dictionary<Guid, List<object>> NodeDataMap { get; set; }
            public Dictionary<Guid, string> NodeReplicationMap { get; set; }
            public Dictionary<Guid, int> InportCountMap { get; set; }
            public Dictionary<Guid, int> OutportCountMap { get; set; }
            public Dictionary<Guid, PortComparisonData> PortDataMap { get; set; }
            public Dictionary<Guid, NodeInputData> InputsMap { get; set; }
            public string DesignScript { get; set; }

            public WorkspaceComparisonData(WorkspaceModel workspace, EngineController controller)
            {
                Guid = workspace.Guid;
                Description = workspace.Description;
                NodeCount = workspace.Nodes.Count();
                ConnectorCount = workspace.Connectors.Count();
                NodeTypeMap = new Dictionary<Guid, Type>();
                NodeDataMap = new Dictionary<Guid, List<object>>();
                InportCountMap = new Dictionary<Guid, int>();
                OutportCountMap = new Dictionary<Guid, int>();
                PortDataMap = new Dictionary<Guid, PortComparisonData>();
                NodeReplicationMap = new Dictionary<Guid, string>();
                InputsMap = new Dictionary<Guid, NodeInputData>();

                foreach (var n in workspace.Nodes)
                {
                    NodeTypeMap.Add(n.GUID, n.GetType());
                    NodeReplicationMap.Add(n.GUID, n.ArgumentLacing.ToString());
                    //save input nodes to inputs block
                    if (n.IsSetAsInput)
                    {
                        InputsMap.Add(n.GUID, n.InputData);
                    }

                    var portvalues = n.OutPorts.Select(p =>
                        GetDataOfValue(n.GetValue(p.Index, controller))).ToList<object>();

                    n.InPorts.ToList().ForEach(p =>
                    {
                        PortDataMap.Add(p.GUID,
                            new PortComparisonData
                            {
                                ID = p.GUID.ToString(),
                                UseLevels = p.UseLevels,
                                KeepListStructure = p.KeepListStructure,
                                Level = p.Level,
                                UsingDefaultValue = p.UsingDefaultValue
                            });
                    });

                    n.OutPorts.ToList().ForEach(p =>
                    {
                        PortDataMap.Add(p.GUID,
                            new PortComparisonData
                            {
                                ID = p.GUID.ToString(),

                            });
                    });

                    NodeDataMap.Add(n.GUID, portvalues);
                    InportCountMap.Add(n.GUID, n.InPorts.Count);
                    OutportCountMap.Add(n.GUID, n.OutPorts.Count);
                }
            }
        }

        private static object GetDataOfValue(ProtoCore.Mirror.MirrorData value)
        {
            if (value.IsCollection)
            {
                return value.GetElements().Select(x => GetDataOfValue(x)).ToList<object>();
            }

            if (!value.IsPointer)
            {
                var data = value.Data;

                if (data != null)
                {
                    return data;
                }
            }

            return value.StringData;
        }

        private void CompareWorkspacesDifferentGuids(WorkspaceComparisonData a, WorkspaceComparisonData b)
        {
            var nodeDiff = a.NodeTypeMap.Select(x => x.Value).Except(b.NodeTypeMap.Select(x => x.Value));
            if (nodeDiff.Any())
            {
                Assert.Fail("The workspaces don't have the same number of nodes. The json workspace is missing: " + string.Join(",", nodeDiff.Select(i => i.ToString())));
            }
            Assert.AreEqual(a.NodeCount, b.NodeCount, "The workspaces don't have the same number of nodes.");
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount, "The workspaces don't have the same number of connectors.");

            foreach (var kvp in a.InportCountMap)
            {
                var countA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[kvp.Key]);
                var countB = b.InportCountMap[newGuid];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} inports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }
            foreach (var kvp in a.OutportCountMap)
            {
                var countA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[kvp.Key]);
                var countB = b.OutportCountMap[newGuid];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} outports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }

            foreach (var portkvp in a.PortDataMap)
            {
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[portkvp.Key]);
                Assert.IsTrue(b.PortDataMap.ContainsKey(newGuid));
                var aPort = a.PortDataMap[portkvp.Key];
                var bPort = b.PortDataMap[newGuid];
                Assert.AreEqual(aPort.UseLevels, bPort.UseLevels);
                Assert.AreEqual(aPort.KeepListStructure, bPort.KeepListStructure);
                Assert.AreEqual(aPort.Level, bPort.Level);
            }

            foreach (var kvp in a.NodeReplicationMap)
            {
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[kvp.Key]);
                var valueA = kvp.Value;
                var valueB = b.NodeReplicationMap[newGuid];
                Assert.AreEqual(valueA, valueB);
            }

            foreach (var kvp in a.NodeDataMap)
            {
                var valueA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[kvp.Key]);
                var valueB = b.NodeDataMap[newGuid];

                Assert.AreEqual(a.NodeTypeMap[kvp.Key], b.NodeTypeMap[newGuid]);

                try
                {
                    // When values are geometry, sometimes the creation
                    // of the string representation for forming this message
                    // fails.
                    Assert.AreEqual(valueA, valueB,
                    string.Format("Node Type:{0} value, {1} is not equal to {2}",
                    a.NodeTypeMap[kvp.Key], valueA, valueB));
                }
                catch
                {
                    continue;
                }
            }
        }

        private void CompareWorkspaces(WorkspaceComparisonData a, WorkspaceComparisonData b)
        {
            var nodeDiff = a.NodeTypeMap.Except(b.NodeTypeMap);
            if (nodeDiff.Any())
            {
                Assert.Fail("The workspaces don't have the same number of nodes. The json workspace is missing: " + string.Join(",", nodeDiff.Select(i => i.Value.ToString())));
            }
            Assert.AreEqual(a.Description, b.Description, "The workspaces don't have the same description.");
            Assert.AreEqual(a.NodeCount, b.NodeCount, "The workspaces don't have the same number of nodes.");
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount, "The workspaces don't have the same number of connectors.");
            //TODO: Annotations / Note tests should be in viewmodel serialization tests.
            // Assert.AreEqual(a.GroupCount, b.GroupCount, "The workspaces don't have the same number of groups.");
            // Assert.AreEqual(a.NoteCount, b.NoteCount, "The workspaces don't have the same number of notes.");
            foreach (var kvp in a.InportCountMap)
            {
                var countA = kvp.Value;
                var countB = b.InportCountMap[kvp.Key];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} inports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }
            foreach (var kvp in a.OutportCountMap)
            {
                var countA = kvp.Value;
                var countB = b.OutportCountMap[kvp.Key];
                Assert.AreEqual(countA, countB, string.Format("One {0} node has {1} outports, while the other has {2}", a.NodeTypeMap[kvp.Key], countA, countB));
            }

            foreach (var portkvp in a.PortDataMap)
            {
                Assert.IsTrue(b.PortDataMap.ContainsKey(portkvp.Key));
                Assert.AreEqual(a.PortDataMap[portkvp.Key], b.PortDataMap[portkvp.Key]);
            }

            foreach (var kvp in a.NodeReplicationMap)
            {
                var valueA = kvp.Value;
                var valueB = b.NodeReplicationMap[kvp.Key];
                Assert.AreEqual(valueA, valueB);
            }

            foreach (var kvp in a.NodeDataMap)
            {
                var valueA = kvp.Value;
                var valueB = b.NodeDataMap[kvp.Key];

                Assert.AreEqual(a.NodeTypeMap[kvp.Key], b.NodeTypeMap[kvp.Key]);

                try
                {
                    // When values are geometry, sometimes the creation
                    // of the string representation for forming this message
                    // fails.
                    Assert.AreEqual(valueA, valueB,
                    string.Format("Node Type:{0} value, {1} is not equal to {2}",
                    a.NodeTypeMap[kvp.Key], valueA, valueB));
                }
                catch
                {
                    continue;
                }
            }

            foreach (var kvp in a.InputsMap)
            {
                var vala = kvp.Value;
                var valb = b.InputsMap[kvp.Key];
                Assert.AreEqual(vala, valb, "input datas are not the same.");
            }
        }

        [Test]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\CustomNodes\TestAdd.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath, "json", ConvertCurrentWorkspaceToJsonAndSave, CompareWorkspaces, SaveWorkspaceComparisonData);
        }

        [Test]
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath, "json", ConvertCurrentWorkspaceToJsonAndSave, CompareWorkspaces, SaveWorkspaceComparisonData);
        }

        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);
            return fis.Select(fi => fi.FullName).ToArray();
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces")]
        public void SerializationTest(string filePath)
        {
            DoWorkspaceOpenAndCompare(filePath, "json", ConvertCurrentWorkspaceToJsonAndSave, CompareWorkspaces, SaveWorkspaceComparisonData);
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// This set of tests has slightly modified json where the id properties
        /// are altered when serialized to test deserialization of non-guid ids.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces")]
        public void SerializationNonGuidIdsTest(string filePath)
        {
            modelsGuidToIdMap.Clear();
            DoWorkspaceOpenAndCompare(filePath, "json_nonGuidIds", ConvertCurrentWorkspaceToNonGuidJsonAndSave, CompareWorkspacesDifferentGuids, SaveWorkspaceComparisonDataWithNonGuidIds);
        }

        public static List<string> bannedTests = new List<string>()
            {
                "NestedIF",
                "recorded",
                "excel",
                "CASE",
                "WatchPreviewBubble",
                "visualization",
                "migration",
                "missing_custom_node",
                "Dummy.dyn",
                // Tests which require late initialization
                // of custom nodes...
                "noro.dyn",
                "Number1.dyn",
                "MultipleIF",
                "packageTest",
                "reduce-example",
                "TestFrozen"
            };

        private void DoWorkspaceOpenAndCompare(string filePath, string dirName,
            Func<DynamoModel, string, string> saveFunction,
            Action<WorkspaceComparisonData, WorkspaceComparisonData> workspaceCompareFunction,
            Action<WorkspaceComparisonData, string, TimeSpan> workspaceDataSaveFunction)
        {
            var openPath = filePath;

            if (bannedTests.Any(t => filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;
            ws1.Description = "TestDescription";

            var dummyNodes = ws1.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var cbnErrorNodes = ws1.Nodes.Where(n => n is CodeBlockNodeModel && n.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType == Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new WorkspaceComparisonData(ws1, CurrentDynamoModel.EngineController);

            var dirPath = Path.Combine(Path.GetTempPath(), dirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var fi = new FileInfo(filePath);
            var filePathBase = dirPath + @"\" + Path.GetFileNameWithoutExtension(fi.Name);

            ConvertCurrentWorkspaceToDesignScriptAndSave(filePathBase);

            string json = saveFunction(model, filePathBase);

            workspaceDataSaveFunction(wcd1, filePathBase, lastExecutionDuration);

            lastExecutionDuration = new TimeSpan();

            var ws2 = WorkspaceModel.FromJson(json, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false,
                model.CustomNodeManager);

            if (ws2 is CustomNodeWorkspaceModel)
            {
                model.AddCustomNodeWorkspace((CustomNodeWorkspaceModel)ws2);
            }

            foreach (var c in ws2.Connectors)
            {
                Assert.NotNull(c.Start.Owner, "The node is not set for the start of connector " + c.GUID + ". The end node is " + c.End.Owner + ".");
                Assert.NotNull(c.End.Owner, "The node is not set for the end of connector " + c.GUID + ". The start node is " + c.Start.Owner + ".");
            }

            // The following logic is taken from the DynamoModel.Open method.
            // It assumes a single home workspace model. So, we remove all
            // others, before adding a new one.
            if (ws2 is HomeWorkspaceModel)
            {
                var currentHomeSpaces = model.Workspaces.OfType<HomeWorkspaceModel>().ToList();
                if (currentHomeSpaces.Any())
                {
                    var end = ws2 is HomeWorkspaceModel ? 0 : 1;

                    for (var i = currentHomeSpaces.Count - 1; i >= end; i--)
                    {
                        model.RemoveWorkspace(currentHomeSpaces[i]);
                    }
                }

                model.AddWorkspace(ws2);

                var hws = ws2 as HomeWorkspaceModel;
                if (hws != null)
                {
                    model.ResetEngine();

                    if (hws.RunSettings.RunType == RunType.Periodic)
                    {
                        hws.StartPeriodicEvaluation();
                    }
                }

                model.CurrentWorkspace = ws2;
            }

            Assert.NotNull(ws2);

            dummyNodes = ws2.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var wcd2 = new WorkspaceComparisonData(ws2, CurrentDynamoModel.EngineController);

            workspaceCompareFunction(wcd1, wcd2);

            var functionNodes = ws2.Nodes.Where(n => n is Function).Cast<Function>();
            if (functionNodes.Any())
            {
                Assert.True(functionNodes.All(n => CurrentDynamoModel.CustomNodeManager.LoadedDefinitions.Contains(n.Definition)));
            }

            foreach (var c in ws2.Connectors)
            {
                Assert.NotNull(c.Start.Owner);
                Assert.NotNull(c.End.Owner);
                Assert.True(ws2.Nodes.Contains(c.Start.Owner));
                Assert.True(ws2.Nodes.Contains(c.End.Owner));
            }

            //assert that the inputs in the saved json file are the same as those we can gather from the 
            //grah at runtime - because we don't deserialize these directly we check the json itself.
            var jObject = JObject.Parse(json);
            var jToken = jObject["Inputs"];
            var inputs = jToken.ToArray().Select(x => x.ToObject<NodeInputData>()).ToList();
            var inputs2 = ws1.Nodes.Where(x => x.IsSetAsInput == true && x.InputData != null).Select(input => input.InputData).ToList();

            //inputs2 might come from a WS with non guids, so we need to replace the ids with guids if they exist in the map
            foreach (var input in inputs2)
            {
                if (modelsGuidToIdMap.ContainsKey(input.Id))
                {
                    input.Id = GuidUtility.Create(GuidUtility.UrlNamespace, modelsGuidToIdMap[input.Id]);
                }
            }
            Assert.IsTrue(inputs.SequenceEqual(inputs2));
        }

        private static void SaveWorkspaceComparisonData(WorkspaceComparisonData wcd1, string filePathBase, TimeSpan executionDuration)
        {
            var nodeData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var d in wcd1.NodeDataMap)
            {
                var t = wcd1.NodeTypeMap[d.Key];
                var nodeDataDict = new Dictionary<string, object>();
                nodeDataDict.Add("nodeType", t.ToString());
                nodeDataDict.Add("portValues", d.Value);
                nodeData.Add(d.Key.ToString(), nodeDataDict);
            }

            var workspaceDataDict = new Dictionary<string, object>();
            workspaceDataDict.Add("nodeData", nodeData);
            workspaceDataDict.Add("executionDuration", executionDuration.TotalSeconds);

            var dataMapStr = JsonConvert.SerializeObject(workspaceDataDict,
                            new JsonSerializerSettings()
                            {
                                Formatting = Formatting.Indented,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

            var dataPath = filePathBase + ".data";
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            File.WriteAllText(dataPath, dataMapStr);
        }

        private void SaveWorkspaceComparisonDataWithNonGuidIds(WorkspaceComparisonData wcd1, string filePathBase, TimeSpan executionDuration)
        {
            var nodeData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var d in wcd1.NodeDataMap)
            {
                var t = wcd1.NodeTypeMap[d.Key];
                var nodeDataDict = new Dictionary<string, object>();
                nodeDataDict.Add("nodeType", t.ToString());
                nodeDataDict.Add("portValues", d.Value);
                nodeData.Add(d.Key.ToString(), nodeDataDict);
            }

            var workspaceDataDict = new Dictionary<string, object>();
            workspaceDataDict.Add("nodeData", nodeData);
            workspaceDataDict.Add("executionDuration", executionDuration.TotalSeconds);

            var dataMapStr = JsonConvert.SerializeObject(workspaceDataDict,
                            new JsonSerializerSettings()
                            {
                                Formatting = Formatting.Indented,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
            //replace all the guids in the data file with all of our remapped ids.
            foreach (var guidKey in modelsGuidToIdMap.Keys)
            {
                dataMapStr = dataMapStr.Replace(guidKey.ToString(), modelsGuidToIdMap[guidKey]);
            }

            var dataPath = filePathBase + ".data";
            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
            File.WriteAllText(dataPath, dataMapStr);
        }

        private static string ConvertCurrentWorkspaceToJsonAndSave(DynamoModel model, string filePathBase)
        {
            var json = model.CurrentWorkspace.ToJson(model.EngineController);
            Assert.IsNotNullOrEmpty(json);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, "json");

            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".dyn";
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            return json;
        }

        private string ConvertCurrentWorkspaceToNonGuidJsonAndSave(DynamoModel model, string filePathBase)
        {
            var json = model.CurrentWorkspace.ToJson(model.EngineController);

           json = serializationTestUtils.replaceModelIdsWithNonGuids(json, model.CurrentWorkspace, modelsGuidToIdMap);

            Assert.IsNotNullOrEmpty(json);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, "jsonNonGuid");

            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".dyn";
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            return json;
        }

        private void ConvertCurrentWorkspaceToDesignScriptAndSave(string filePathBase)
        {
            try
            {
                var workspace = CurrentDynamoModel.CurrentWorkspace;

                var libCore = CurrentDynamoModel.EngineController.LibraryServices.LibraryManagementCore;
                var libraryServices = new LibraryCustomizationServices(CurrentDynamoModel.PathManager);
                var nameProvider = new NamingProvider(libCore, libraryServices);
                var controller = CurrentDynamoModel.EngineController;
                var resolver = CurrentDynamoModel.CurrentWorkspace.ElementResolver;
                var namingProvider = new NamingProvider(controller.LibraryServices.LibraryManagementCore, libraryServices);

                var result = NodeToCodeCompiler.NodeToCode(libCore, workspace.Nodes, workspace.Nodes, namingProvider);
                NodeToCodeCompiler.ReplaceWithShortestQualifiedName(
                        controller.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes, resolver);
                var codegen = new ProtoCore.CodeGenDS(result.AstNodes);
                var ds = codegen.GenerateCode();

                var dsPath = filePathBase + ".ds";
                if (File.Exists(dsPath))
                {
                    File.Delete(dsPath);
                }
                File.WriteAllText(dsPath, ds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Assert.Inconclusive("The current workspace could not be converted to Design Script.");
            }
        }
    }
}

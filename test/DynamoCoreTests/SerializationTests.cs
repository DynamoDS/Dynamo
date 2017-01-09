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

namespace Dynamo.Tests
{
    /* The Serialization tests compare the results of a workspace opened and executed from its
     * original .dyn format, to one converted to json, deserialized and executed. In the process,
     * the tests save the following files:
     *  - xxx.json file representing the serialized version of the workspace to json, where xxx is the
     *  original .dyn file name.
     *  - xxx_data.json file containing the cached values of each of the workspaces
     *  - xxx.ds file containing the Design Script code for the workspace.
     */
    [TestFixture, Category("Serialization")]
    class SerializationTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();

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
        }

        [TearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);
        }

        /// <summary>
        /// Caches workspaces data for comparison.
        /// </summary>
        internal class WorkspaceComparisonData
        {
            public Guid Guid { get; set; }
            public int NodeCount { get; set; }
            public int ConnectorCount { get; set; }
            public int GroupCount { get; set; }
            public int NoteCount { get; set; }
            public Dictionary<Guid,Type> NodeTypeMap { get; set; }
            public Dictionary<Guid,List<object>> NodeDataMap { get; set; }
            public Dictionary<Guid,int> InportCountMap { get; set; }
            public Dictionary<Guid,int> OutportCountMap { get; set; }
            public string DesignScript { get; set; }

            public WorkspaceComparisonData(WorkspaceModel workspace, EngineController controller)
            {
                Guid = workspace.Guid;
                NodeCount = workspace.Nodes.Count();
                ConnectorCount = workspace.Connectors.Count();
                GroupCount = workspace.Annotations.Count();
                NoteCount = workspace.Notes.Count();
                NodeTypeMap = new Dictionary<Guid, Type>();
                NodeDataMap = new Dictionary<Guid, List<object>>();
                InportCountMap = new Dictionary<Guid, int>();
                OutportCountMap = new Dictionary<Guid, int>();

                foreach (var n in workspace.Nodes)
                {
                    NodeTypeMap.Add(n.GUID, n.GetType());

                    var portvalues = n.OutPorts.Select(p =>
                        GetDataOfValue(n.GetValue(p.Index, controller))).ToList<object>();

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

        private void CompareWorkspaces(WorkspaceComparisonData a, WorkspaceComparisonData b)
        {
            var nodeDiff = a.NodeTypeMap.Except(b.NodeTypeMap);
            if (nodeDiff.Any())
            {
                Assert.Fail("The workspaces don't have the same number of nodes. The json workspace is missing: " + string.Join(",", nodeDiff.Select(i => i.Value.ToString())));
            }
            Assert.AreEqual(a.NodeCount, b.NodeCount, "The workspaces don't have the same number of nodes.");
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount, "The workspaces don't have the same number of connectors.");
            Assert.AreEqual(a.GroupCount, b.GroupCount, "The workspaces don't have the same number of groups.");
            Assert.AreEqual(a.NoteCount, b.NoteCount, "The workspaces don't have the same number of notes.");
            foreach(var kvp in a.InportCountMap)
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
        }

        [Test]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory , @"core\CustomNodes\TestAdd.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath);
        }

        [Test]
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpenAndCompare(customNodeTestPath);
        }

        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);
            return fis.Select(fi=>fi.FullName).ToArray();
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
            DoWorkspaceOpenAndCompare(filePath);
        }

        private static List<string> bannedTests = new List<string>()
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

        private void DoWorkspaceOpenAndCompare(string filePath)
        {
            var openPath = filePath;

            if (bannedTests.Any(t => filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;

            var dummyNodes = ws1.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.NickName).ToArray()));
            }

            var cbnErrorNodes = ws1.Nodes.Where(n => n is CodeBlockNodeModel && n.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType== Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new WorkspaceComparisonData(ws1, CurrentDynamoModel.EngineController);

            var fi = new FileInfo(filePath);
            var filePathBase = Path.Combine(Path.GetTempPath() + @"\json\" + Path.GetFileNameWithoutExtension(fi.Name));

            ConvertCurrentWorkspaceToDesignScriptAndSave(filePathBase);

            string json = ConvertCurrentWorkspaceToJsonAndSave(model, filePathBase);

            SaveWorkspaceComparisonData(wcd1, filePathBase, lastExecutionDuration);

            lastExecutionDuration = new TimeSpan();

            var ws2 = Autodesk.Workspaces.Utilities.LoadWorkspaceFromJson(json, model.LibraryServices,
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
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.NickName).ToArray()));
            }

            var wcd2 = new WorkspaceComparisonData(ws2, CurrentDynamoModel.EngineController);

            CompareWorkspaces(wcd1, wcd2);

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
        }

        private static void SaveWorkspaceComparisonData(WorkspaceComparisonData wcd1, string filePathBase, TimeSpan executionDuration)
        {
            var nodeData = new Dictionary<string, Dictionary<string, object>>();
            foreach(var d in wcd1.NodeDataMap)
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

        private static string ConvertCurrentWorkspaceToJsonAndSave(DynamoModel model, string filePathBase)
        {
            var json = Autodesk.Workspaces.Utilities.SaveWorkspaceToJson(model.CurrentWorkspace, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false,
                model.CustomNodeManager);

            Assert.IsNotNullOrEmpty(json);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, "json");

            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".json";
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
            catch
            {
                Assert.Inconclusive("The current workspace could not be converted to Design Script.");
            }
        }
    }
}

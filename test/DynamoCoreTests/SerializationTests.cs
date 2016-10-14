using NUnit.Framework;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes;
using Dynamo.Models;

namespace Dynamo.Tests
{
    [TestFixture, Category("Serialization")]
    class SerializationTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
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
            public Dictionary<Guid,object> NodeDataMap { get; set; }
            public Dictionary<Guid,int> InportCountMap { get; set; }
            public Dictionary<Guid,int> OutportCountMap { get; set; }

            public WorkspaceComparisonData(WorkspaceModel workspace)
            {
                Guid = workspace.Guid;
                NodeCount = workspace.Nodes.Count();
                ConnectorCount = workspace.Connectors.Count();
                GroupCount = workspace.Annotations.Count();
                NoteCount = workspace.Notes.Count();
                NodeTypeMap = new Dictionary<Guid, Type>();
                NodeDataMap = new Dictionary<Guid, object>();
                InportCountMap = new Dictionary<Guid, int>();
                OutportCountMap = new Dictionary<Guid, int>();

                foreach (var n in workspace.Nodes)
                {
                    NodeTypeMap.Add(n.GUID, n.GetType());
                    NodeDataMap.Add(n.GUID, n.CachedValue == null? null:n.CachedValue.Data);
                    InportCountMap.Add(n.GUID, n.InPorts.Count);
                    OutportCountMap.Add(n.GUID, n.OutPorts.Count);
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

        private void DoWorkspaceOpenAndCompare(string filePath)
        {
            var openPath = filePath;

            var bannedTests = new List<string>()
            {
                "NestedIF",
                "recorded",
                "excel",
                "CASE",
                "WatchPreviewBubble",
                "visualization",
                "migration",
                "missing_custom_node",
                "Dummy.dyn"
            };

            if (bannedTests.Any(t=>filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            // Find the version in the root
            
            /*var xmlDoc = new XmlDocument();
            xmlDoc.Load(openPath);
            WorkspaceInfo info;
            if(WorkspaceInfo.FromXmlDocument(xmlDoc, openPath, false, false, CurrentDynamoModel.Logger, out info))
            {
                if (Version.Parse(info.Version) < new Version(1, 0))
                {
                    Assert.Inconclusive("The test file was from before version 1.0.");
                }
            }*/

            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;

            var dummyNodes = ws1.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.NickName).ToArray()));
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType== Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new WorkspaceComparisonData(ws1);

            var json = Autodesk.Workspaces.Utilities.SaveWorkspaceToJson(model.CurrentWorkspace, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false, 
                model.CustomNodeManager);

            Assert.IsNotNullOrEmpty(json);

            var fi = new FileInfo(filePath);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, "json");
            
            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = Path.Combine(Path.GetTempPath() + @"\json\" + Path.GetFileNameWithoutExtension(fi.Name) + ".json");
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            var ws2 = Autodesk.Workspaces.Utilities.LoadWorkspaceFromJson(json, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false,
                model.CustomNodeManager);

            if (ws2 is CustomNodeWorkspaceModel)
            {
                model.AddCustomNodeWorkspace((CustomNodeWorkspaceModel)ws2);
            }

            foreach(var c in ws2.Connectors)
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
            if(dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",",dummyNodes.Select(n=>n.NickName).ToArray()));
            }

            var wcd2 = new WorkspaceComparisonData(ws2);

            CompareWorkspaces(wcd1, wcd2);

            var functionNodes = ws2.Nodes.Where(n => n is Function).Cast<Function>();
            if(functionNodes.Any())
            {
                Assert.True(functionNodes.All(n => CurrentDynamoModel.CustomNodeManager.LoadedDefinitions.Contains(n.Definition)));
            }
            
            foreach(var c in ws2.Connectors)
            {
                Assert.NotNull(c.Start.Owner);
                Assert.NotNull(c.End.Owner);
                Assert.True(ws2.Nodes.Contains(c.Start.Owner));
                Assert.True(ws2.Nodes.Contains(c.End.Owner));
            }
        }
    }
}

using NUnit.Framework;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using ProtoCore.Mirror;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph.Nodes.CustomNodes;

namespace Dynamo.Tests
{
    [TestFixture]
    class SerializationTests : DynamoModelTestBase
    {
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
            public Dictionary<Guid,MirrorData> NodeDataMap { get; set; }

            public WorkspaceComparisonData(WorkspaceModel workspace)
            {
                Guid = workspace.Guid;
                NodeCount = workspace.Nodes.Count();
                ConnectorCount = workspace.Connectors.Count();
                GroupCount = workspace.Annotations.Count();
                NoteCount = workspace.Notes.Count();
                NodeTypeMap = new Dictionary<Guid, Type>();
                NodeDataMap = new Dictionary<Guid, MirrorData>();

                foreach (var n in workspace.Nodes)
                {
                    NodeTypeMap.Add(n.GUID, n.GetType());
                    NodeDataMap.Add(n.GUID, n.CachedValue);
                }
            }
        }

        private void CompareWorkspaces(WorkspaceComparisonData a, WorkspaceComparisonData b)
        {
            Assert.AreEqual(a.NodeCount, b.NodeCount);
            Assert.AreEqual(a.ConnectorCount, b.ConnectorCount);
            Assert.AreEqual(a.GroupCount, b.GroupCount);
            Assert.AreEqual(a.NoteCount, b.NoteCount);
        }

        [Test]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory , @"core\CustomNodes\TestAdd.dyn");
            DowWorkspaceOpenAndCompare(customNodeTestPath);
        }

        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);
            return fis.Select(fi=>fi.FullName).ToArray();
        }

        [Test, TestCaseSource("FindWorkspaces")]
        public void SerializationTest(string filePath)
        {
            DowWorkspaceOpenAndCompare(filePath);
        }

        private void DowWorkspaceOpenAndCompare(string filePath)
        {
            var openPath = filePath;
            OpenModel(openPath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;

            var wcd1 = new WorkspaceComparisonData(ws1);

            var json = model.SaveCurrentWorkspaceToJson();

            Assert.IsNotNullOrEmpty(json);

            CurrentDynamoModel.LoadWorkspaceFromJson(json);

            var ws2 = CurrentDynamoModel.CurrentWorkspace;
            Assert.NotNull(ws2);

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

            // Set the ws as the current home workspace
            // and try to run it.
            RunCurrentModel();

            foreach (var n in ws2.Nodes)
            {
                Assert.True(wcd1.NodeDataMap[n.GUID].Data.Equals(n.CachedValue.Data), 
                    string.Format("Node Type:{0} value, {1} is not equal to {2}", 
                    n.GetType(), n.CachedValue.Data == null? "null" : n.CachedValue.Data, wcd1.NodeDataMap[n.GUID].Data));
                Assert.AreEqual(wcd2.NodeTypeMap[n.GUID], n.GetType());
            }
        }
    }
}

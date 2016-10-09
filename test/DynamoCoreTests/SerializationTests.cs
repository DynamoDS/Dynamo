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
        }

        [Test]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory , @"core\CustomNodes\TestAdd.dyn");
            DowWorkspaceOpenAndCompare(customNodeTestPath);
        }

        [Test]
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
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

            if (ws1.WorkspaceVersion < new Version(1, 0))
            {
                Assert.Inconclusive("The test file was from before version 1.0.");
            }

            if(((HomeWorkspaceModel)ws1).RunSettings.RunType== Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new WorkspaceComparisonData(ws1);

            var json = Workspaces.Serialization.Workspaces.SaveWorkspaceToJson(model.CurrentWorkspace, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false, 
                model.CustomNodeManager, new ProtoCore.Namespace.ElementResolver());

            Assert.IsNotNullOrEmpty(json);

            var ws2 = Workspaces.Serialization.Workspaces.LoadWorkspaceFromJson(json, model.LibraryServices,
                model.EngineController, model.Scheduler, model.NodeFactory, DynamoModel.IsTestMode, false,
                model.CustomNodeManager, new ProtoCore.Namespace.ElementResolver());

            if (ws2 is CustomNodeWorkspaceModel)
            {
                model.AddCustomNodeWorkspace((CustomNodeWorkspaceModel)ws2);
            }

            if (ws2 is HomeWorkspaceModel)
            {
                // TODO: #4258
                // The logic to remove all other home workspaces from the model
                // was moved from the ViewModel. When #4258 is implemented, we will need to
                // remove this step.
                var currentHomeSpaces = model.Workspaces.OfType<HomeWorkspaceModel>().ToList();
                if (currentHomeSpaces.Any())
                {
                    // If the workspace we're opening is a home workspace,
                    // then remove all the other home workspaces. Otherwise,
                    // Remove all but the first home workspace.
                    var end = ws2 is HomeWorkspaceModel ? 0 : 1;

                    for (var i = currentHomeSpaces.Count - 1; i >= end; i--)
                    {
                        model.RemoveWorkspace(currentHomeSpaces[i]);
                    }
                }

                model.AddWorkspace(ws2);

                // TODO: #4258
                // The following logic to start periodic evaluation will need to be moved
                // inside of the HomeWorkspaceModel's constructor.  It cannot be there today
                // as it causes an immediate crash due to the above ResetEngine call.
                var hws = ws2 as HomeWorkspaceModel;
                if (hws != null)
                {
                    // TODO: #4258
                    // Remove this ResetEngine call when multiple home workspaces is supported.
                    // This call formerly lived in DynamoViewModel
                    model.ResetEngine();

                    if (hws.RunSettings.RunType == RunType.Periodic)
                    {
                        hws.StartPeriodicEvaluation();
                    }
                }

                model.CurrentWorkspace = ws2;
            }

            Assert.NotNull(ws2);

            if(ws2.Nodes.Any(n=>n is DummyNode))
            {
                Assert.Inconclusive("Workspace contains Dummy Nodes.");
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

            // Set the ws as the current home workspace
            // and try to run it.
            //RunCurrentModel();

            foreach (var n in ws2.Nodes)
            {
                if(wcd1.NodeDataMap[n.GUID] == null && n.CachedValue == null)
                {
                    continue;
                }
                Assert.True(wcd1.NodeDataMap[n.GUID].Equals(n.CachedValue.IsNull? null : n.CachedValue.Data), 
                    string.Format("Node Type:{0} value, {1} is not equal to {2}", 
                    n.GetType(), n.CachedValue == null? "null" : n.CachedValue.Data, wcd1.NodeDataMap[n.GUID]));
                Assert.AreEqual(wcd2.NodeTypeMap[n.GUID], n.GetType());
            }
        }
    }
}

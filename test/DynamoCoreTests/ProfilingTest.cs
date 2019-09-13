using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using System.Threading;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class ProfilingTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("VMDataBridge.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestProfilingSingleNode()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            // Assert that profiling is disabled by default
            var engineController = CurrentDynamoModel.EngineController;
            Assert.IsNull(engineController.ProfilingSession);
            AssertNullValues();

            // Assert that no profiling data is created after a run when profiling is disabled
            BeginRun();
            Assert.IsNull(engineController.ProfilingSession);
            var nodeGuid = "3016738fece14964876e0acfdb09811a";
            AssertNonNull(nodeGuid);

            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            // Assert that profiling can be enabled
            engineController.EnableProfiling(true, homeWorkspace, nodes);
            Assert.IsNotNull(engineController.ProfilingSession);
            AssertNonNull(nodeGuid);

            // Assert that there is no profiling data before the graph is run
            var profilingData = engineController.ProfilingSession.ProfilingData;
            var node = nodes.FirstOrDefault();
            Assert.IsNull(profilingData.TotalExecutionTime);
            Assert.IsNull(profilingData.NodeExecutionTime(node));

            BeginRun();

            // Assert that profiling data exists after a run occurs
            Assert.IsNotNull(profilingData.TotalExecutionTime);
            Assert.Greater(profilingData.TotalExecutionTime?.Ticks, 0);
            Assert.IsNotNull(profilingData.NodeExecutionTime(node));
            Assert.Greater(profilingData.NodeExecutionTime(node)?.Ticks, 0);

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.DeleteModelCommand(node.GUID));
            BeginRun();

            // Assert that per-node profiling data is deleted after a node is deleted
            Assert.IsNotNull(profilingData.TotalExecutionTime);
            Assert.Greater(profilingData.TotalExecutionTime?.Ticks, 0);
            Assert.IsNull(profilingData.NodeExecutionTime(node));

            // Assert that profiling can be disabled
            engineController.EnableProfiling(false, homeWorkspace, nodes);
            Assert.IsNull(engineController.ProfilingSession);
        }
 
        [Test]
        public void TestProfilingSingleNodePublicMethodsOnly()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            // Assert that profiling is disabled by default
            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            engineController.EnableProfiling(true, homeWorkspace, nodes);
            BeginRun();

            // Assert that execution time data exists after a run occurs
            var profilingData = engineController.ExecutionTimeData;
            Assert.IsNotNull(profilingData.TotalExecutionTime);
            Assert.Greater(profilingData.TotalExecutionTime?.Ticks, 0);
            var node = nodes.FirstOrDefault();
            Assert.IsNotNull(profilingData.NodeExecutionTime(node));
            Assert.Greater(profilingData.NodeExecutionTime(node)?.Ticks, 0);
        }

        private static List<Guid> executingNodes = new List<Guid>();
        private static int nodeExecutionBeginCount = 0;
        private static int nodeExecutionEndCount = 0;

        private static void onNodeExectuionBegin(NodeModel model)
        {
            // Assert that the node has ot been executed more than once
            CollectionAssert.DoesNotContain(executingNodes, model.GUID);

            executingNodes.Add(model.GUID);
            nodeExecutionBeginCount++;
        }

        private static void onNodeExectuionEnd(NodeModel model)
        {
            // Assert that the node ending execution had the begin exectuion event fired previously
            CollectionAssert.Contains(executingNodes, model.GUID);

            nodeExecutionEndCount++;
        }

       [Test]
        public void TestNodeExecutionEvents()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            foreach(var node in nodes)
            {
                node.NodeExecutionBegin += onNodeExectuionBegin;
                node.NodeExecutionEnd += onNodeExectuionEnd;
            }

            // Currently the node execution begin/end events are only enabled when profiling is enabled
            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            engineController.EnableProfiling(true, homeWorkspace, nodes);

            BeginRun();

            // Assert that all nodes were executed and the begin and end events were fired as expectd
            Assert.AreEqual(4, nodeExecutionBeginCount);
            Assert.AreEqual(4, nodeExecutionEndCount);

            foreach (var node in nodes)
            {
                node.NodeExecutionBegin -= onNodeExectuionBegin;
                node.NodeExecutionEnd -= onNodeExectuionEnd;
            }
        }

        [Test]
        public void StartTimeResetsWhenNodeIsReExecuted()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            // Enable profiling
            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            engineController.EnableProfiling(true, homeWorkspace, nodes);
            var profilingData = engineController.ProfilingSession.ProfilingData;

            // Assert no profiling data has been created for any node yet
            foreach (var node in nodes)
            {
                var time = profilingData.NodeExecutionTime(node);
                Assert.IsNull(time);
            }

            // First run
            BeginRun();

            // Gather first run data and assert it is not null
            var firstRunTimes = new List<TimeSpan>();
            foreach (var node in nodes)
            {
                var time = profilingData.NodeExecutionTime(node);
                Assert.IsNotNull(time);
                firstRunTimes.Add((TimeSpan)time);
            }

            // Wait a few seconds so we can later confirm that the start time gets reset after this pause
            var pauseTime = new TimeSpan(0, 0, 0, 5); // 5 seconds
            Thread.Sleep(pauseTime);

            // Mark ONLY the most upstream node as modified
            var codeBlockGuid = "87794137f78d4d809814e73d977e46fb";
            foreach (var node in nodes)
            {
                if (node.GUID == Guid.Parse(codeBlockGuid))
                {
                    node.MarkNodeAsModified();
                }
            }

            // Second run / Re-execute
            BeginRun();

            // Gather second run data
            var secondRunTimes = new List<TimeSpan>();
            foreach (var node in nodes)
            {
                var executionTime = (TimeSpan)profilingData.NodeExecutionTime(node);
                Assert.IsNotNull(executionTime);
                secondRunTimes.Add(executionTime);
                // Assert the execution time is less than the 5 second pause time
                // i.e. The start time has been reset since before we paused for 5 seconds
                Assert.Less(executionTime.TotalMilliseconds, pauseTime.TotalMilliseconds);
            }
        }
    }
}

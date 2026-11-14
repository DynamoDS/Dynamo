using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using FFITarget;
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
            libraries.Add("FFITarget.dll");
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

        [Test]
        public void WhenProfilingHasNotBeenEnabledThenIsProfilingEnabledIsFalse()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var engineController = CurrentDynamoModel.EngineController;

            // Reading the property must not throw, unlike ExecutionTimeData, when there is no session
            Assert.IsFalse(engineController.IsProfilingEnabled);
            Assert.IsNull(engineController.ProfilingSession);
        }

        [Test]
        public void WhenProfilingIsEnabledThenIsProfilingEnabledIsTrue()
        {
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            engineController.EnableProfiling(true, homeWorkspace, nodes);

            Assert.IsTrue(engineController.IsProfilingEnabled);
            // The execution time data can be read once the property is true
            Assert.IsNotNull(engineController.ExecutionTimeData);
        }

        [Test]
        public void WhenProfilingIsDisabledThenIsProfilingEnabledIsFalse()
        {
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            engineController.EnableProfiling(true, homeWorkspace, nodes);
            Assert.IsTrue(engineController.IsProfilingEnabled);

            engineController.EnableProfiling(false, homeWorkspace, nodes);

            Assert.IsFalse(engineController.IsProfilingEnabled);
        }

        [Test]
        public void WhenGraphRunsWithProfilingEnabledThenIsProfilingEnabledStaysTrue()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            engineController.EnableProfiling(true, homeWorkspace, nodes);

            BeginRun();

            // Running does not end the profiling session, so a caller can still read the data afterwards
            Assert.IsTrue(engineController.IsProfilingEnabled);
            Assert.IsNotNull(engineController.ExecutionTimeData.TotalExecutionTime);
        }

        [Test]
        public void WhenProfilingIsEnabledTwiceThenTheSameSessionIsKept()
        {
            string openPath = Path.Combine(TestDirectory, @"core\profiling\createSomePoints.dyn");
            OpenModel(openPath);

            var engineController = CurrentDynamoModel.EngineController;
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            engineController.EnableProfiling(true, homeWorkspace, nodes);
            var firstSession = engineController.ProfilingSession;

            // A second caller turning profiling on (for example TuneUp after another extension) shares the session
            engineController.EnableProfiling(true, homeWorkspace, nodes);

            Assert.IsTrue(engineController.IsProfilingEnabled);
            Assert.AreSame(firstSession, engineController.ProfilingSession);
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

        #region DYN-11168: a node must execute between its own NodeExecutionBegin and NodeExecutionEnd

        /// <summary>
        /// The ProfilingCounter value seen when a node's first NodeExecutionBegin and the first
        /// NodeExecutionEnd after it fired. A node that executed between the two sees End = Begin + 1.
        /// </summary>
        private sealed class CounterAtEvents
        {
            public int? AtBegin;
            public int? AtEnd;
        }

        private static Dictionary<Guid, CounterAtEvents> RecordCounterAtEvents(IEnumerable<NodeModel> nodes)
        {
            var seen = nodes.ToDictionary(n => n.GUID, _ => new CounterAtEvents());
            foreach (var node in nodes)
            {
                node.NodeExecutionBegin += n =>
                {
                    var s = seen[n.GUID];
                    if (s.AtBegin == null) s.AtBegin = ProfilingCounter.Count;
                };
                node.NodeExecutionEnd += n =>
                {
                    var s = seen[n.GUID];
                    if (s.AtBegin != null && s.AtEnd == null) s.AtEnd = ProfilingCounter.Count;
                };
            }
            return seen;
        }

        /// <summary>
        /// Resets the counter and puts the workspace in Manual, so nodes run only on BeginRun. In Automatic,
        /// creating a node or enabling profiling would run the graph straight away.
        /// </summary>
        private void StartManualCounterTest()
        {
            CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().First().RunSettings.RunType = RunType.Manual;
            ProfilingCounter.Reset();
        }

        private DSFunction AddCounterNode(string mangledName)
        {
            var node = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor(mangledName));
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, false));
            return node;
        }

        private void EnableProfiling()
        {
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().First();
            CurrentDynamoModel.EngineController.EnableProfiling(true, homeWorkspace, homeWorkspace.Nodes);
        }

        private static void AssertExecutedBetweenBeginAndEnd(CounterAtEvents seen, string what)
        {
            Assert.IsNotNull(seen.AtBegin, $"{what}: NodeExecutionBegin did not fire");
            Assert.IsNotNull(seen.AtEnd, $"{what}: NodeExecutionEnd did not fire after NodeExecutionBegin");
            Assert.AreEqual(seen.AtBegin + 1, seen.AtEnd,
                $"{what}: the node must execute exactly once between its NodeExecutionBegin (counter {seen.AtBegin}) " +
                $"and NodeExecutionEnd (counter {seen.AtEnd}); otherwise its time is credited to other nodes");
        }

        [Test]
        public void ProfiledRun_OfNeverRunGraph_NodeExecutesBetweenItsBeginAndEnd()
        {
            // Control case: profiling a graph that has never run attributes the node correctly.
            StartManualCounterTest();
            var node = AddCounterNode("FFITarget.ProfilingCounter.Increment");

            EnableProfiling();
            var seen = RecordCounterAtEvents(new[] { node });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[node.GUID], "no-input node, profiled first run");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_NoInputNodeExecutesBetweenItsBeginAndEnd()
        {
            // DYN-11168: one run without profiling, then a profiled run. The node's statement re-executes
            // before its begin marker, so its time lands outside its own begin/end window.
            StartManualCounterTest();
            var node = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            BeginRun();
            Assert.AreEqual(1, ProfilingCounter.Count, "the unprofiled run should execute the node once");

            EnableProfiling();
            var seen = RecordCounterAtEvents(new[] { node });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[node.GUID], "no-input node, profiled run after an unprofiled run");
            Assert.AreEqual(2, ProfilingCounter.Count, "the profiled run should execute the node exactly once");
        }

        [Test]
        public void ProfiledRun_RepeatedAfterUnprofiledRun_NoInputNodeStillExecutesBetweenItsBeginAndEnd()
        {
            // DYN-11168: once triggered, the bad order persists for every later profiled run.
            StartManualCounterTest();
            var node = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            BeginRun();

            EnableProfiling();
            BeginRun();

            node.MarkNodeAsModified(true);
            var seen = RecordCounterAtEvents(new[] { node });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[node.GUID], "no-input node, second profiled run after an unprofiled run");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_NodeWithInputExecutesBetweenItsBeginAndEnd()
        {
            // DYN-11168: same sequence for a node with an input, wired downstream of the no-input node.
            StartManualCounterTest();
            var upstream = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            var downstream = AddCounterNode("FFITarget.ProfilingCounter.IncrementAfter@int");
            ConnectorModel.Make(upstream, downstream, 0, 0);
            BeginRun();
            Assert.AreEqual(2, ProfilingCounter.Count, "the unprofiled run should execute both nodes once");

            EnableProfiling();
            var seen = RecordCounterAtEvents(new NodeModel[] { upstream, downstream });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[downstream.GUID], "node with an input, profiled run after an unprofiled run");
            Assert.AreEqual(4, ProfilingCounter.Count, "the profiled run should execute each node exactly once");
        }

        private DoubleInput AddNumberNode(string value)
        {
            var node = new DoubleInput { Value = value };
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, false));
            return node;
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_NodeFedByNumberInputExecutesOnceBetweenItsBeginAndEnd()
        {
            // DYN-11168: an input node (Number) upstream must not make the downstream node run twice.
            StartManualCounterTest();
            var number = AddNumberNode("42");
            var downstream = AddCounterNode("FFITarget.ProfilingCounter.IncrementAfter@int");
            ConnectorModel.Make(number, downstream, 0, 0);
            BeginRun();
            Assert.AreEqual(1, ProfilingCounter.Count, "the unprofiled run should execute the node once");

            EnableProfiling();
            var seen = RecordCounterAtEvents(new NodeModel[] { downstream });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[downstream.GUID], "node fed by a Number node, profiled run after an unprofiled run");
            Assert.AreEqual(2, ProfilingCounter.Count, "the profiled run should execute the node exactly once");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRunThenConnect_NodeFedByNumberInputExecutesOnceBetweenItsBeginAndEnd()
        {
            // DYN-11168: same, but the wire is added after the unprofiled run (as the MCP profile_workspace test does).
            StartManualCounterTest();
            var number = AddNumberNode("42");
            var downstream = AddCounterNode("FFITarget.ProfilingCounter.IncrementAfter@int");
            BeginRun();
            var countAfterUnprofiledRun = ProfilingCounter.Count;
            ConnectorModel.Make(number, downstream, 0, 0);

            EnableProfiling();
            var seen = RecordCounterAtEvents(new NodeModel[] { downstream });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[downstream.GUID], "node fed by a Number node connected after an unprofiled run");
            Assert.AreEqual(countAfterUnprofiledRun + 1, ProfilingCounter.Count, "the profiled run should execute the node exactly once");
        }

        #endregion
    }
}

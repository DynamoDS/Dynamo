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

        [Test]
        public void ProfiledRun_AfterUnprofiledRunThenAddDownstreamNode_NewNodeExecutesOnceBetweenItsBeginAndEnd()
        {
            // DYN-11168: a node added after the unprofiled run is compiled as an added subtree, ahead of
            // the recompiled upstream node. It must still run once, after its upstream node.
            StartManualCounterTest();
            var upstream = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            BeginRun();
            Assert.AreEqual(1, ProfilingCounter.Count, "the unprofiled run should execute the node once");

            var downstream = AddCounterNode("FFITarget.ProfilingCounter.IncrementAfter@int");
            ConnectorModel.Make(upstream, downstream, 0, 0);
            EnableProfiling();
            var seen = RecordCounterAtEvents(new NodeModel[] { upstream, downstream });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[upstream.GUID], "upstream node, profiled run after adding a downstream node");
            AssertExecutedBetweenBeginAndEnd(seen[downstream.GUID], "node added after an unprofiled run");
            Assert.AreEqual(3, ProfilingCounter.Count, "the profiled run should execute each node exactly once");
        }

        [Test]
        public void ProfiledRun_AfterReconnectFollowingAProfiledRun_UnchangedNodeExecutesBetweenItsBeginAndEnd()
        {
            // DYN-11168: after a profiled run, reconnecting another node recompiles the unchanged nodes from
            // their cached statements. The cache must keep each node's statement between its markers.
            StartManualCounterTest();
            var first = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            var second = AddCounterNode("FFITarget.ProfilingCounter.Increment");
            var downstream = AddCounterNode("FFITarget.ProfilingCounter.IncrementAfter@int");
            ConnectorModel.Make(first, downstream, 0, 0);
            BeginRun();

            EnableProfiling();
            BeginRun();

            downstream.InPorts[0].Connectors.First().Delete();
            ConnectorModel.Make(second, downstream, 0, 0);
            first.MarkNodeAsModified(true);
            var seen = RecordCounterAtEvents(new NodeModel[] { first, downstream });
            BeginRun();

            AssertExecutedBetweenBeginAndEnd(seen[first.GUID], "unchanged node recompiled after a reconnect");
            AssertExecutedBetweenBeginAndEnd(seen[downstream.GUID], "reconnected node");
        }

        #endregion

        #region DYN-11176: profiling must not change a node's callsite IDs or lose its trace data

        // Koey Lai's two tests below (ForceReexecuteWithoutProfiling_... and
        // ProfiledRun_AfterUnprofiledRun_TracedNodeKeepsItsTraceData) found the regression from the
        // DYN-11168 fix. The rest cover the other ways profiling and a node's compile can be combined.
        //
        // IncrementerTracedClass mimics host element binding: with trace data it reuses its ID,
        // without trace data it takes a new ID (as Revit would create a new element).
        private DSFunction AddTracedNode()
        {
            const string mangledName = "FFITarget.IncrementerTracedClass.IncrementerTracedClass@int";
            var descriptor = CurrentDynamoModel.LibraryServices.GetFunctionDescriptor(mangledName);
            Assert.IsNotNull(descriptor, $"function descriptor not found: {mangledName}");
            var node = new DSFunction(descriptor);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, false));
            return node;
        }

        private IncrementerTracedClass GetTracedObject(NodeModel node)
        {
            var value = GetPreviewValue(node.GUID);
            Assert.IsInstanceOf<IncrementerTracedClass>(value, "node output should be an IncrementerTracedClass");
            return (IncrementerTracedClass)value;
        }

        // Callsite IDs of the traced constructor for this node, as cached by the VM.
        private List<string> TracedCallsiteIds(NodeModel node)
        {
            var guid = node.GUID.ToString();
            return CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Keys
                .Where(k => k.StartsWith("IncrementerTracedClass") && k.Contains(guid))
                .OrderBy(k => k)
                .ToList();
        }

        [Test]
        public void ForceReexecuteWithoutProfiling_TracedNodeKeepsItsTraceData()
        {
            // Control: proves the check below can see trace reuse at all.
            StartManualCounterTest();
            IncrementerTracedClass.ResetForNextTest();
            var number = AddNumberNode("1");
            var traced = AddTracedNode();
            ConnectorModel.Make(number, traced, 0, 0);
            BeginRun();
            var firstId = GetTracedObject(traced).ID;

            traced.MarkNodeAsModified(true);
            BeginRun();

            var second = GetTracedObject(traced);
            Assert.IsTrue(second.WasCreatedWithTrace(), "force re-execution should reuse the callsite's trace data");
            Assert.AreEqual(firstId, second.ID, "force re-execution should rebind to the same element");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_TracedNodeKeepsItsTraceData()
        {
            // Enabling profiling after a plain run must not lose trace data: a host would otherwise
            // create a new element instead of updating the one from the plain run.
            StartManualCounterTest();
            IncrementerTracedClass.ResetForNextTest();
            var number = AddNumberNode("1");
            var traced = AddTracedNode();
            ConnectorModel.Make(number, traced, 0, 0);
            BeginRun();
            var firstId = GetTracedObject(traced).ID;
            var callsitesBefore = TracedCallsiteIds(traced);

            EnableProfiling();
            BeginRun();

            var second = GetTracedObject(traced);
            var callsitesAfter = TracedCallsiteIds(traced);
            TestContext.WriteLine("Callsites after plain run:    " + string.Join(", ", callsitesBefore));
            TestContext.WriteLine("Callsites after profiled run: " + string.Join(", ", callsitesAfter));
            Assert.IsTrue(second.WasCreatedWithTrace(), "the profiled run should reuse the callsite's trace data");
            Assert.AreEqual(firstId, second.ID, "the profiled run should rebind to the same element");
        }

        private void DisableProfiling()
        {
            var homeWorkspace = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().First();
            CurrentDynamoModel.EngineController.EnableProfiling(false, homeWorkspace, homeWorkspace.Nodes);
        }

        // Callsite IDs the VM has cached for this node's profiling markers.
        private List<string> MarkerCallsiteIds(NodeModel node)
        {
            var guid = node.GUID.ToString();
            return CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Keys
                .Where(k => k.Contains("_ProfilingMarker") && k.Contains(guid))
                .OrderBy(k => k)
                .ToList();
        }

        private DSFunction BuildNumberFedTracedNode()
        {
            StartManualCounterTest();
            IncrementerTracedClass.ResetForNextTest();
            var number = AddNumberNode("1");
            var traced = AddTracedNode();
            ConnectorModel.Make(number, traced, 0, 0);
            return traced;
        }

        // Active graph nodes compiled from this node's profiling markers.
        private int ActiveMarkerGraphNodeCount(NodeModel node)
        {
            var runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            return runtimeCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList
                .Count(g => g.guid == node.GUID && g.isActive && g.IsProfilingMarker);
        }

        // Callsite keys that combine more than one callsite. A run that left a profiling marker as
        // its last executed statement made the next run's keys combine with it.
        private List<string> CompositeCallsiteKeys()
        {
            return CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Keys
                .Where(k => k.Contains(";"))
                .ToList();
        }

        private void AssertRebound(NodeModel traced, int expectedId, string when)
        {
            var obj = GetTracedObject(traced);
            Assert.IsTrue(obj.WasCreatedWithTrace(), $"{when}: the node should reuse its callsite's trace data");
            Assert.AreEqual(expectedId, obj.ID, $"{when}: the node should rebind to the same element");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_TracedNodeKeepsItsCallsiteIds()
        {
            // The node's own callsite IDs must be identical with and without the markers; the
            // markers' calls get IDs of their own that never use the "_Instance" form.
            var traced = BuildNumberFedTracedNode();
            BeginRun();
            var callsitesBefore = TracedCallsiteIds(traced);
            Assert.AreEqual(1, callsitesBefore.Count, "the plain run should create one callsite for the node");
            StringAssert.Contains("_Instance0_", callsitesBefore[0]);

            EnableProfiling();
            BeginRun();

            CollectionAssert.AreEqual(callsitesBefore, TracedCallsiteIds(traced),
                "enabling profiling must not create new callsites for the node's own call");
            var markerCallsites = MarkerCallsiteIds(traced);
            Assert.AreEqual(2, markerCallsites.Count, "the begin and end markers should each have a callsite");
            foreach (var id in markerCallsites)
            {
                StringAssert.DoesNotContain("_Instance", id, "marker callsites must not use the node's numbering");
            }
        }

        [Test]
        public void ProfilingDisabledAfterProfiledRun_TracedNodeKeepsItsTraceData()
        {
            var traced = BuildNumberFedTracedNode();
            BeginRun();
            var firstId = GetTracedObject(traced).ID;

            EnableProfiling();
            BeginRun();
            AssertRebound(traced, firstId, "profiled run");

            DisableProfiling();
            BeginRun();
            AssertRebound(traced, firstId, "run after profiling was turned off");
        }

        [Test]
        public void ProfilingToggledRepeatedly_TracedNodeKeepsItsTraceDataAndCallsites()
        {
            var traced = BuildNumberFedTracedNode();
            BeginRun();
            var firstId = GetTracedObject(traced).ID;
            var callsites = TracedCallsiteIds(traced);
            int markerGraphNodes = -1;

            for (int i = 1; i <= 3; i++)
            {
                EnableProfiling();
                BeginRun();
                AssertRebound(traced, firstId, $"profiled run {i}");
                // Each marker compiles to several graph nodes; the count must not grow between cycles.
                if (i == 1)
                {
                    markerGraphNodes = ActiveMarkerGraphNodeCount(traced);
                    Assert.Greater(markerGraphNodes, 0, "the profiled run should compile the markers");
                }
                Assert.AreEqual(markerGraphNodes, ActiveMarkerGraphNodeCount(traced),
                    $"profiled run {i}: only the current markers should be active");

                DisableProfiling();
                BeginRun();
                AssertRebound(traced, firstId, $"unprofiled run {i}");
                Assert.AreEqual(0, ActiveMarkerGraphNodeCount(traced),
                    $"unprofiled run {i}: turning profiling off should deactivate the markers");
                CollectionAssert.IsEmpty(CompositeCallsiteKeys(),
                    $"unprofiled run {i}: no callsite should be keyed under a leftover marker");
            }

            CollectionAssert.AreEqual(callsites, TracedCallsiteIds(traced),
                "toggling profiling must never add callsites for the node's own call");
        }

        [Test]
        public void ProfiledFirstRun_ThenProfilingDisabledAndNodeRecompiled_TracedNodeKeepsItsTraceData()
        {
            // The reverse order: the node's first compile includes the markers. Before DYN-11176 the
            // node's call was then numbered Instance1, and any later compile without markers (here a
            // reconnected input) renumbered it Instance0 and lost the trace data.
            var traced = BuildNumberFedTracedNode();
            EnableProfiling();
            BeginRun();
            var firstId = GetTracedObject(traced).ID;

            DisableProfiling();
            var newNumber = AddNumberNode("1");
            ConnectorModel.Make(newNumber, traced, 0, 0);
            BeginRun();

            AssertRebound(traced, firstId, "run without profiling after a reconnect");
        }

        [Test]
        public void ReconnectInputWithoutProfiling_TracedNodeKeepsItsTraceData()
        {
            // Control for the next test: reconnecting an input recompiles the node and keeps its trace.
            var traced = BuildNumberFedTracedNode();
            BeginRun();
            var firstId = GetTracedObject(traced).ID;

            var newNumber = AddNumberNode("1");
            ConnectorModel.Make(newNumber, traced, 0, 0);
            BeginRun();

            AssertRebound(traced, firstId, "run after a reconnect");
        }

        [Test]
        public void ReconnectInputWithProfilingOn_TracedNodeKeepsItsTraceData()
        {
            // Koey's pre-existing case: a node first compiled without markers, then recompiled with
            // them because an input was reconnected while profiling was on.
            var traced = BuildNumberFedTracedNode();
            BeginRun();
            var firstId = GetTracedObject(traced).ID;

            EnableProfiling();
            BeginRun();

            var newNumber = AddNumberNode("1");
            ConnectorModel.Make(newNumber, traced, 0, 0);
            BeginRun();

            AssertRebound(traced, firstId, "profiled run after a reconnect");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_ReplicatedTracedNodeKeepsEveryElement()
        {
            // One callsite holding several elements (replication over a list), as a node that
            // creates a list of Revit elements does.
            StartManualCounterTest();
            IncrementerTracedClass.ResetForNextTest();
            var list = new CodeBlockNodeModel("[1, 2, 3];", 0, 0,
                CurrentDynamoModel.LibraryServices, CurrentDynamoModel.CurrentWorkspace.ElementResolver);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(list, 0, 0, true, false));
            var traced = AddTracedNode();
            ConnectorModel.Make(list, traced, 0, 0);
            BeginRun();
            var firstIds = GetTracedObjects(traced).Select(o => o.ID).ToList();
            Assert.AreEqual(3, firstIds.Count);

            EnableProfiling();
            BeginRun();

            var second = GetTracedObjects(traced);
            Assert.IsTrue(second.All(o => o.WasCreatedWithTrace()), "every element should reuse its trace data");
            CollectionAssert.AreEqual(firstIds, second.Select(o => o.ID).ToList(),
                "every element should rebind to the same element");
        }

        [Test]
        public void ProfiledRun_AfterUnprofiledRun_CodeBlockWithTwoTracedCallsKeepsBoth()
        {
            // Two traced calls in one node are numbered Instance0 and Instance1 within its guid; the
            // markers must not shift either of them.
            StartManualCounterTest();
            IncrementerTracedClass.ResetForNextTest();
            var cbn = new CodeBlockNodeModel(
                "a = FFITarget.IncrementerTracedClass(1);\nb = FFITarget.IncrementerTracedClass(2);", 0, 0,
                CurrentDynamoModel.LibraryServices, CurrentDynamoModel.CurrentWorkspace.ElementResolver);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false));
            BeginRun();
            var firstIds = new[] { GetTracedOutput(cbn, 0).ID, GetTracedOutput(cbn, 1).ID };
            Assert.AreNotEqual(firstIds[0], firstIds[1]);

            EnableProfiling();
            BeginRun();

            for (int i = 0; i < 2; i++)
            {
                var obj = GetTracedOutput(cbn, i);
                Assert.IsTrue(obj.WasCreatedWithTrace(), $"output {i} should reuse its trace data");
                Assert.AreEqual(firstIds[i], obj.ID, $"output {i} should rebind to the same element");
            }
        }

        [Test]
        public void SavedAndReopenedWithoutProfiling_TracedNodeRebinds()
        {
            // Control for the next two tests: trace data saved in the .dyn rebinds on reopen.
            AssertRebindsAfterReopen(profileBeforeSave: false, profileAfterReopen: false);
        }

        [Test]
        public void SavedWithoutProfiling_ReopenedWithProfiling_TracedNodeRebinds()
        {
            // For example: a graph run and saved, then reopened in Revit while TuneUp is open.
            AssertRebindsAfterReopen(profileBeforeSave: false, profileAfterReopen: true);
        }

        [Test]
        public void SavedWithProfiling_ReopenedWithoutProfiling_TracedNodeRebinds()
        {
            AssertRebindsAfterReopen(profileBeforeSave: true, profileAfterReopen: false);
        }

        private void AssertRebindsAfterReopen(bool profileBeforeSave, bool profileAfterReopen)
        {
            var traced = BuildNumberFedTracedNode();
            var tracedGuid = traced.GUID;
            BeginRun();
            var firstId = GetTracedObject(traced).ID;
            if (profileBeforeSave)
            {
                EnableProfiling();
                BeginRun();
                AssertRebound(traced, firstId, "profiled run before saving");
            }

            var path = GetNewFileNameOnTempPath("dyn");
            CurrentDynamoModel.CurrentWorkspace.Save(path, false, CurrentDynamoModel.EngineController);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(path, true));

            var reopened = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(tracedGuid);
            Assert.IsNotNull(reopened, "the traced node should be in the reopened graph");
            if (profileAfterReopen)
            {
                EnableProfiling();
            }
            BeginRun();

            AssertRebound(reopened, firstId, "first run after reopening");
        }

        private List<IncrementerTracedClass> GetTracedObjects(NodeModel node)
        {
            var mirror = GetRuntimeMirror(GetVarName(node.GUID));
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            Assert.IsTrue(data.IsCollection, "node output should be a list");
            return data.GetElements().Select(e =>
            {
                Assert.IsInstanceOf<IncrementerTracedClass>(e.Data, "list items should be IncrementerTracedClass");
                return (IncrementerTracedClass)e.Data;
            }).ToList();
        }

        private IncrementerTracedClass GetTracedOutput(NodeModel node, int outputIndex)
        {
            var mirror = GetRuntimeMirror(node.GetAstIdentifierForOutputIndex(outputIndex).Value);
            Assert.IsNotNull(mirror);
            var value = mirror.GetData().Data;
            Assert.IsInstanceOf<IncrementerTracedClass>(value, $"output {outputIndex} should be an IncrementerTracedClass");
            return (IncrementerTracedClass)value;
        }

        #endregion
    }
}

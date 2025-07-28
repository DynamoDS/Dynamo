using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CoreNodeModels.Input;
using DSCore;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using FFITarget;
using NUnit.Framework;
using ProtoCore;
using static ProtoCore.CallSite;

namespace Dynamo.Tests
{
    internal class TestTraceReconciliationProcessor : ITraceReconciliationProcessor
    {
        public int ExpectedOrphanCount { get; internal set; }

        public TestTraceReconciliationProcessor(int expectedOrphanCount)
        {
            ExpectedOrphanCount = expectedOrphanCount;
        }

        public void PostTraceReconciliation(Dictionary<Guid, List<string>> orphanedSerializables)
        {
            Assert.AreEqual(orphanedSerializables.SelectMany(kvp=>kvp.Value).Count(), ExpectedOrphanCount);
        }
    }

    [TestFixture]
    public sealed class CallsiteTests : DynamoModelTestBase
    {
        private const string callsiteDir = @"core\callsite";

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// These tests depend on nodes in the Dynamo Samples package, which are copied
        /// at build time into a packages folder in the build directory. We
        /// override the UserDataRootFolder on the IPathResolver to have the tests look in 
        /// the packages folder built into the build directory.
        /// </summary>
        protected override string GetUserUserDataRootFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /* Multi-dimension tests
         * This graph contains:
         * One list: {"Tywin","Cersei","Hodor"}
         * One double input: 0..2
         * A + node with the list and the double connected and CARTESIAN PRODUCT lacing.
         */

        [Test]
        public void Callsite_MultiDimensionDecreaseDimensionOnOpenAndRun_OrphanCountCorrect()
        {
            OpenChangeAndCheckOrphans("RebindingMultiDimension.dyn", "0..1", 3);    
        }

        [Test]
        public void CallSite_MultiDimensionIncreaseDimensionOnOpenAndRun()
        {
            OpenChangeAndCheckOrphans("RebindingMultiDimension.dyn", "0..3", 0);
        }


        /* Single-dimension tests
         * This graph contains:
         * One list: {"Tywin","Cersei","Hodor"}
         * One double input: 0..2
         * A + node with the list and the double connected and SINGLE lacing.
         */

        [Test]
        public void Callsite_SingleDimensionDecreaseDimensionOnOpenAndRun()
        {
            OpenChangeAndCheckOrphans("RebindingSingleDimension.dyn", "0..1", 1);
        }

        [Test]
        public void Callsite_SingleDimensionIncreaseDimensionOnOpenAndRun()
        {
            OpenChangeAndCheckOrphans("RebindingSingleDimension.dyn", "0..3", 0);
        }

        private void OpenChangeAndCheckOrphans(string testFileName, string numNodeValue, int expectedOrphanCount)
        {
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, testFileName);

            CurrentDynamoModel.TraceReconciliationProcessor = new TestTraceReconciliationProcessor(expectedOrphanCount);

            var numNode = ws.FirstNodeFromWorkspace<CodeBlockNodeModel>();

            // Increase the number values.
            numNode.SetCodeContent(numNodeValue + ";", new ProtoCore.Namespace.ElementResolver());

            BeginRun();
        }

        [Test]
        public void Callsite_DeleteNodeBeforeRun()
        {

            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "RebindingSingleDimension.dyn");

            CurrentDynamoModel.TraceReconciliationProcessor = new TestTraceReconciliationProcessor(3);

            var traceNode = ws.Nodes.Where(n => n is DSFunction).FirstOrDefault(f => f.Name == "WrapperObject.WrapperObject");
            Assert.NotNull(traceNode);

            ws.RemoveAndDisposeNode(traceNode);

            BeginRun();
        }

        [Test]
        public void Callsite_RunWithTraceDataFromUnresolvedNodes_DoesNotCrash()
        {
            var ws = Open<HomeWorkspaceModel>(SampleDirectory, @"en-US\Revit", "Revit_GeometryCreation_Surfaces.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(42, ws.Nodes.Count());
            Assert.AreEqual(49, ws.Connectors.Count());

            // The guard added around deserialization of types that
            // can't be resolved will prevent a crash. This test
            // passes if the workspace runs.

            BeginRun();

            Assert.Pass();
        }

        [Test]
        public void Callsite_ElementBinding()
        {
            // This graph has 2 "WrapperObject" creation nodes that is defined in FFITarget.
            // The node wraps a static string ID that increments each time you create a new node.
            // One of the nodes in the graph is created the 2nd time and the other the 3rd time
            // in the same session. Therefore the ID of the first is 2 and the second is 3 and the graph
            // stores the trace data for each node. This test tests that on reopening the graph the ID's
            // of each of these nodes do not change and remain 2 and 3 respectively due to element binding.
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding.dyn");

            BeginRun();

            AssertPreviewValue("c760af7e-042c-4722-a834-3445bf41f549", 2);
            AssertPreviewValue("5f277520-13aa-4833-aa82-b17a822e6d8c", 3);
        }

        // This test dyn contains a manually edited callsite data with modified function scopes that do not exist
        // this ensures function scope is not used during trace reconciliation for custom nodes.

        [Test]
        public void Callsite_ElementBinding_CustomNodes()
        {
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_customNodes_modified.dyn");
            CurrentDynamoModel.TraceReconciliationProcessor = new TestTraceReconciliationProcessor(0);
            Assert.AreEqual(6, ws.Nodes.Count());
            var dummyNodes = ws.Nodes.OfType<DummyNode>();
            Assert.AreEqual(0, dummyNodes.Count());
            Assert.IsFalse(ws.Nodes.OfType<Function>().First().IsInErrorState);

            BeginRun();
            AssertPreviewValue("da39dbe5f59649b18c2fb6ca54acba7b", 5);
            AssertPreviewValue("2366239164a9441a8c4dcd981d9cf542", 4);
            AssertPreviewValue("342f96575f8942c890867d88495fb0db",1);

        }

        // This test dyn contains a manually edited callsite data with modified function scopes that do not exist
        // this ensures function scope is not used during trace reconciliation for custom nodes.

        [Test]
        public void Callsite_ElementBinding_CustomNodes_multiple()
        {
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_customNodes_modified_multiple.dyn");
            CurrentDynamoModel.TraceReconciliationProcessor = new TestTraceReconciliationProcessor(0);
            Assert.AreEqual(9, ws.Nodes.Count());
            var dummyNodes = ws.Nodes.OfType<DummyNode>();
            Assert.AreEqual(0, dummyNodes.Count());
            Assert.IsFalse(ws.Nodes.OfType<Function>().First().IsInErrorState);

            BeginRun();
            AssertPreviewValue("da39dbe5f59649b18c2fb6ca54acba7b", 6);
            AssertPreviewValue("2366239164a9441a8c4dcd981d9cf542", 7);
            AssertPreviewValue("8cfce012280342f3bd688520d68a7f66", 10);
            AssertPreviewValue("08448232ee094aad8280e9a99ed44f46", 11);
        }

        [Test]
        public void Callsite_ElementBinding_CustomNodes_ShouldReturnUniqueIds()
        {
            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_customNodes_replication.dyn");
            BeginRun();
            AssertPreviewValue("3cab31e7c7e646cfb11f6145edf1d8c3", Enumerable.Range(1, 6).ToList());
        }

        [Test]
        public void Single_CallSite_Should_Rebind()
        {

            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "single_callsite.dyn");
            //assert there is only one callsite
            Assert.AreEqual(1, this.CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Keys.Count);
            var callsite = this.CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.FirstOrDefault().Value;
            Assert.AreEqual(1, callsite.invokeCount);
            var traceid = Guid.Parse("08278f9e8ae64c72b86313e04cdde709");
            var traceNode = CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == traceid).FirstOrDefault();
            var firstresult = (traceNode.CachedValue.Data as WrapperObject).ID;

            //run the graph again.
            ws.Nodes.OfType<CodeBlockNodeModel>().First().SetCodeContent("2", ws.ElementResolver);
            Assert.AreEqual(1, this.CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.Keys.Count);
            Assert.AreEqual(1, callsite.invokeCount);
            var secondResult = (traceNode.CachedValue.Data as WrapperObject).ID;
            Assert.AreEqual(firstresult,secondResult);

        }

        [Test]
        public void Callsite_ElementBinding_CustomNodes2dReplication_ShouldReturnUniqueIds()
        {
            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_customNodes_replication2d.dyn");
            BeginRun();
            AssertPreviewValue("3cab31e7c7e646cfb11f6145edf1d8c3", new int[][] {
               new int[] {1},
               new int[] {2,3 },
               new int[]{4,5,6 },
               new int[]{7,8,9,10 },
               new int[]{11,12,13,14,15 },
               new int[]{16,17,18,19,20,21 }
            });
        }

        [Test]
        public void Callsite_ElementBinding_CustomNodes_MultipleRunsShouldResetInvocationCount()
        {
            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_customNodes_replication.dyn");
            BeginRun();
            AssertPreviewValue("3cab31e7c7e646cfb11f6145edf1d8c3", Enumerable.Range(1, 6).ToList());
            
            //grab the inner callsite inside the custom node
            var callsite = this.CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallsiteCache.
                Where(kv => kv.Key.Contains("WrapperObject")).FirstOrDefault().Value;
            //should have executed 6 times
            Assert.AreEqual(callsite.invokeCount, 6);

            //force a re execution and if binding succeeds then data should be unchanged.
            ws.Nodes.OfType<CodeBlockNodeModel>().First().SetCodeContent("5..10", ws.ElementResolver);
            AssertPreviewValue("3cab31e7c7e646cfb11f6145edf1d8c3", Enumerable.Range(1, 6).ToList());
            //count should have been reset and invoked 6 more times
            Assert.AreEqual(callsite.invokeCount,6);
        }

        [Test]
        [Category("TechDebt")]
        public void Callsite_ElementBinding_Functions_UniqueIds_ForReplicationOfInnerAndOuterFunction()
        {
            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "func_nested_replication.dyn");
            BeginRun();
            AssertPreviewValue("22e0f3229b314aa48914e8f6b925872c", Enumerable.Range(1, 3).ToList());
            // this node currently rebinds to its inner callsites so there are repeated values being returned.
            // it's not clear this behavior is correct, but it matches expected results with zeroTouch nodes nested in
            // other zero touch nodes which access trace. This test is created to note the current behavior and to 
            // alert us if it changes.
            AssertPreviewValue("74cd0ca6d4964ec2b500fbe96139d28c", new int[][] {
               new int[] {4},
               new int[] {4,5 },
               new int[]{4,5,6 },
               new int[]{4,5,6,7 }
            });
            /*
            AssertPreviewValue("74cd0ca6d4964ec2b500fbe96139d28c", new int[][] {
               new int[] {4},
               new int[] {5,6 },
               new int[]{7,8,9 },
               new int[]{10,11,12,13 }
            });
            */
        }

        //TODO add previous test with multiple executions

       

        [Test]
        public void Callsite_ElementBinding_ShouldReturnUniqueIds()
        {
            WrapperObject.ResetNextID();
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "nonNestedWorking_replication.dyn");
            BeginRun();
            AssertPreviewValue("a74679f905fc4883bb017851d94ac074", Enumerable.Range(1, 6).ToList());
        }

        [Test]
        public void Callsite_ElementBinding_Timing()
        {
            //This graph loads trace data for 1500 "WrapperObjects" in Manual run mode.
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding_large.dyn");
            var sw = new Stopwatch();
            sw.Start(); 

            BeginRun();
            sw.Stop();
            Assert.Less(sw.Elapsed.Milliseconds, 20000);
            Console.WriteLine(sw.Elapsed);
            AssertPreviewValue("056d9c584f3b42acabec727e64188fae", Enumerable.Range(1502,1500).ToList());
        }

        [Test]
        public void CanDetectLegacyTraceFormat()
        {
            var legacyTraceData =
                "PFNPQVAtRU5WOkVudmVsb3BlIHhtbG5zOnhzaT0iaHR0cDovL3d3dyY2hlbWFzLm1pY3Jvc29mdC5jb20vc29hcC9lbmNvZGluZy";
            Assert.True(CheckIfTraceDataIsLegacySOAPFormat(legacyTraceData));
        }

        [Test]
        public void CanWarnAboutLegacyTraceData()
        {
            DynamoModel.IsTestMode = false;
            var counter = 99;
            CurrentDynamoModel.RequestNotification += (_, _) => { counter++; };

            // Dyn file contains SOAP formatted trace data.
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir,
                "element_binding_customNodes_modified_multiple_pre3.0.dyn");

            DynamoModel.IsTestMode = true;
            Assert.AreEqual(100, counter);
        }

        [Test]
        public void JsonTraceSerializationTest()
        {
            // Contains trace data in old SOAP format
            var filePath = Path.Combine(TestDirectory,
                @"core\callsite\element_binding_customNodes_modified_multiple_pre3.0.dyn");

            OpenModel(filePath);
            var model = CurrentDynamoModel;

            if (((HomeWorkspaceModel)model.CurrentWorkspace).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }
            var json = model.CurrentWorkspace.ToJson(model.EngineController);
            Assert.That(json, Is.Not.Null.Or.Empty);

            var obj = Data.ParseJSON(json) as Dictionary<string, object>;
            var bindings = obj["Bindings"];

            var list = (bindings as IEnumerable<object>).ToList();
            Assert.AreEqual(3, list.Count);
            int i = 0;
            foreach (var elem in list)
            {
                var traces = ((elem as Dictionary<string, object>)["Binding"] as Dictionary<string, object>).Values;
                if (i == 0 || i == 2)
                {
                    Assert.AreEqual(2, traces.Count);
                }
                else Assert.AreEqual(1, traces.Count);

                foreach (var trace in traces)
                {
                    // Assert that new bindings are not in SOAP format.
                    Assert.False(CallSite.CheckIfTraceDataIsLegacySOAPFormat(trace as string));
                }
                ++i;
            }
        }
    }
}

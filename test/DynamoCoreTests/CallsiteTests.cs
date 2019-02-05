using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CoreNodeModels.Input;
using Dynamo.Engine;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;
using Dynamo.Graph.Nodes;

namespace Dynamo.Tests
{
    internal class TestTraceReconciliationProcessor : ITraceReconciliationProcessor
    {
        public int ExpectedOrphanCount { get; internal set; }

        public TestTraceReconciliationProcessor(int expectedOrphanCount)
        {
            ExpectedOrphanCount = expectedOrphanCount;
        }

        public void PostTraceReconciliation(Dictionary<Guid, List<ISerializable>> orphanedSerializables)
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

        [Test, Category("Failure")]
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

        [Test, Category("Failure")]
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

        [Test, Category("Failure")]
        public void Callsite_DeleteNodeBeforeRun()
        {
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "RebindingSingleDimension.dyn");

            CurrentDynamoModel.TraceReconciliationProcessor = new TestTraceReconciliationProcessor(3);

            var traceNode = ws.Nodes.Where(n=>n is DSFunction).FirstOrDefault(f=>f.Name == "TraceExampleWrapper.ByString");
            Assert.NotNull(traceNode);

            ws.RemoveAndDisposeNode(traceNode);

            BeginRun();
        }

        [Test]
        public void Callsite_RunWithTraceDataFromUnresolvedNodes_DoesNotCrash()
        {
            var ws = Open<HomeWorkspaceModel>(SampleDirectory, @"en-US\Geometry", "Geometry_Surfaces.dyn");

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
            // The node wraps a static ISerializable ID that increments each time you create a new node.
            // One of the nodes in the graph is created the 2nd time and the other the 3rd time
            // in the same session. Therefore the ID of the first is 2 and the second is 3 and the graph
            // stores the trace data for each node. This test tests that on reopening the graph the ID's
            // of each of these nodes do not change and remain 2 and 3 respectively due to element binding.
            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "element_binding.dyn");

            BeginRun();

            AssertPreviewValue("c760af7e-042c-4722-a834-3445bf41f549", 2);
            AssertPreviewValue("5f277520-13aa-4833-aa82-b17a822e6d8c", 3);
        }
    }
}

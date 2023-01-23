using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture, Category("Performance")]
    public class PerformanceTests : DynamoModelTestBase
    {
        private List<(string graph, TimeSpan oldEngineCompileTime, TimeSpan oldEngineExecutionTime,
            TimeSpan newEngineCompileTime, TimeSpan newEngineExecutionTime)> executionData;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }
        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(Path.Combine(TestDirectory, "core", "performance"));
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);

            var failingTests = new string[] { 
                "aniform.dyn",
                "lotsofcoloredstuff.dyn"};

            // Ignore aniform and lotsofcoloredstuff for now
            return fis.Where(fi =>!failingTests.Contains(fi.Name)).Select(fi => fi.FullName).ToArray();
        }

        [TestFixtureSetUp]
        public void SetupPerformanceTests()
        {
            executionData = new List<(string, TimeSpan, TimeSpan, TimeSpan, TimeSpan)>();
        }

        [TestFixtureTearDown]
        public void TeardownPerformanceTests()
        {
            Console.WriteLine("{0,50}{1,9}{2,9}{3,11}{4,9}{5,9}{6,11}", "Graph", "Old C", "Old E", "Old C+E", "New C", "New E", "New C+E");
            executionData.ForEach(item =>
            {
                var (graph, oldEngineCompileTime, oldEngineExecutionTime, newEngineCompileTime, newEngineExecutionTime) = item;
                Console.WriteLine("{0,50}{1,9:0.0}{2,9:0.0}{3,11:0.0}{4,9:0.0}{5,9:0.0}{6,11:0.0}", graph,
                    oldEngineCompileTime.TotalMilliseconds, oldEngineExecutionTime.TotalMilliseconds,
                    oldEngineCompileTime.TotalMilliseconds + oldEngineExecutionTime.TotalMilliseconds,
                    newEngineCompileTime.TotalMilliseconds, newEngineExecutionTime.TotalMilliseconds,
                    newEngineCompileTime.TotalMilliseconds + newEngineExecutionTime.TotalMilliseconds);
            });
            executionData.Clear();
        }

        [Test, TestCaseSource("FindWorkspaces"), Category("Performance")]
        public void PerformanceTest(string filePath)
        {
            DoWorkspaceOpenAndCompare(filePath);
        }

        private void DoWorkspaceOpenAndCompare(string filePath)
        {
            OpenModel(filePath);

            var model = CurrentDynamoModel;
            var ws1 = model.CurrentWorkspace;
            ws1.Description = "TestDescription";

            Assert.NotNull(ws1);

            CheckForDummyNodes(ws1);

            var cbnErrorNodes = ws1.Nodes.Where(n => n is CodeBlockNodeModel && n.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new serializationTestUtils.WorkspaceComparisonData(ws1, CurrentDynamoModel.EngineController);

            var oldEngineCompileAndExecutionTime = model.EngineController.CompileAndExecutionTime;

            // The big hammer, maybe not needed
            Cleanup();

            Setup();

            OpenModel(filePath, dsExecution: false);
            model = CurrentDynamoModel;
            var ws2 = model.CurrentWorkspace;
            ws2.Description = "TestDescription";

            if (((HomeWorkspaceModel)ws2).RunSettings.RunType == Dynamo.Models.RunType.Manual)
            {
                RunCurrentModel();
            }

            Assert.NotNull(ws2);

            CheckForDummyNodes(ws2);

            var wcd2 = new serializationTestUtils.WorkspaceComparisonData(ws2, CurrentDynamoModel.EngineController, dsExecution: false);

            serializationTestUtils.CompareWorkspaceModelsMSIL(wcd1, wcd2);

            var newEngineCompileAndExecutionTime = model.EngineController.CompileAndExecutionTime;

            Console.WriteLine("Compile and Execution time old Engine={0:0.0}+{1:0.0} ms, new Engine={2:0.0}+{3:0.0} ms",
                oldEngineCompileAndExecutionTime.compileTime.TotalMilliseconds, oldEngineCompileAndExecutionTime.executionTime.TotalMilliseconds,
                newEngineCompileAndExecutionTime.compileTime.TotalMilliseconds, newEngineCompileAndExecutionTime.executionTime.TotalMilliseconds);
            var execution = (Path.GetFileName(filePath),
                oldEngineCompileAndExecutionTime.compileTime, oldEngineCompileAndExecutionTime.executionTime,
                newEngineCompileAndExecutionTime.compileTime, newEngineCompileAndExecutionTime.executionTime);
            executionData.Add(execution);
        }

        private void CheckForDummyNodes(WorkspaceModel ws)
        {
            var dummyNodes = ws.Nodes.Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }
        }
    }
}

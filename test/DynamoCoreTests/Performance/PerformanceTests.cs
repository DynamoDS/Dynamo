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
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            base.GetLibrariesToPreload(libraries);
        }
        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(Path.Combine(TestDirectory, "core", "performance"));
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);

            // Ignore aniform and lotsofcoloredstuff for now
            return fis.Where(
                    fi=>fi.Name != "aniform.dyn" 
                    && fi.Name != "lotsofcoloredstuff.dyn").Select(fi => fi.FullName).ToArray();
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
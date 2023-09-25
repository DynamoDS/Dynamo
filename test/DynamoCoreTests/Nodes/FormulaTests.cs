using System.Collections.Generic;
using System.IO;
using CoreNodeModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class FormulaTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void FormulaWithIf()
        {
            string path = Path.Combine(TestDirectory, "core", "formula", "formula-if.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("a9a9e7fa-abda-4f0b-8a9e-d7f8070b6b99");
            Assert.AreEqual(3, node.InPorts.Count);
            
            AssertPreviewValue("a9a9e7fa-abda-4f0b-8a9e-d7f8070b6b99", 2);
        }

        [Test]
        public void FormulaMigration()
        {
            string path = Path.Combine(TestDirectory, "core", "formula", "formula4.dyn");
            RunModel(path);

            var trigNode = "b052363d-1713-4735-9568-5d3a3e98a740";
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(trigNode);
            //Assert.AreEqual(3, node.InPorts.Count);

            AssertPreviewValue(trigNode, -1.9952);

            var expNode = "cc59376e-25a1-4ac1-b0ec-f525260c645c";
            node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(expNode);
            AssertPreviewValue(expNode, 22026.465794);

            var errNode = "5d2f36ea-e692-436c-9a3b-f1d15b093c0b";
            node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(errNode);
            Assert.True(node.ToolTipText.Contains(Properties.Resources.FormulaDSConversionFailure));

            var numNode = "a418958a-990b-4b9a-998a-bcfc581f4ff4";
            node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(numNode);
            AssertPreviewValue(numNode, 400);

            var addNode = "98fe8b4b-c82e-4015-a978-fa632ce0c2b0";
            node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(addNode);
            AssertPreviewValue(addNode, 6);
        }
    }
}

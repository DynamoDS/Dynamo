using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class NodeMigrationTests : Dynamo.Tests.DSEvaluationUnitTest
    {
        #region Dynamo Core Node Migration Tests

        [Test]
        public void Dynamo_Nodes_StringInput_0_6_3_27649()
        {
            OpenModel(GetDynPath("Dynamo.Nodes.StringInput-0.6.3.27649.dyn"));

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var cbn = workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                "dc27fc31-fdad-40b5-906e-bbba9caf43a6");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            Assert.NotNull(cbn); // Ensure the StringInput node is migrated.
            Assert.AreEqual("\"First line\\nSecond line with\\ttab\\nThird line with \\\"quotes\\\"\";", cbn.Code);

            RunCurrentModel(); // Execute the opened file.
            AssertPreviewValue("f6d7a6c3-5df4-45c0-911b-04d39b4c1959", 56);
        }

        [Test]
        public void Dynamo_Nodes_NumberInput_0_6_3_25334()
        {
            OpenModel(GetDynPath("Dynamo.Nodes.NumberInput-0.6.3.25334.dyn"));

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            Assert.AreEqual(12, workspace.Nodes.Count);
            Assert.AreEqual(16, workspace.Connectors.Count);

            // All 8 NumberInput nodes should have been migrated into code blocks.
            var cbn5 = GetCodeBlockNode("ddf4b266-29b6-4609-b1fe-dba814d4babd");
            var cbn10 = GetCodeBlockNode("27d6c83d-602c-4d44-a9b6-ab229cb2143d");
            var cbn50 = GetCodeBlockNode("ffd50d99-b51d-4104-9e19-041219ca5740");
            var cbnRange = GetCodeBlockNode("eb6aa95d-8be4-4ca7-95a6-e696904a71fa");
            var cbnStep = GetCodeBlockNode("5fde015f-8a95-4b46-ba64-29de06850938");
            var cbnCount = GetCodeBlockNode("ace69b4e-5092-42cf-9fba-9aee6729509d");
            var cbnApprox = GetCodeBlockNode("30e9b7fd-fc09-4243-a34a-146ad841868a");
            var cbnIncr = GetCodeBlockNode("bede0d80-6382-4430-9403-a14c3916e041");

            Assert.NotNull(cbn5);
            Assert.NotNull(cbn10);
            Assert.NotNull(cbn50);
            Assert.NotNull(cbnRange);
            Assert.NotNull(cbnStep);
            Assert.NotNull(cbnCount);
            Assert.NotNull(cbnApprox);
            Assert.NotNull(cbnIncr);

            RunCurrentModel(); // Execute the migrated graph.

            AssertPreviewValue("ddf4b266-29b6-4609-b1fe-dba814d4babd", 5);
            AssertPreviewValue("27d6c83d-602c-4d44-a9b6-ab229cb2143d", 10);
            AssertPreviewValue("ffd50d99-b51d-4104-9e19-041219ca5740", 50);

            AssertPreviewValue("eb6aa95d-8be4-4ca7-95a6-e696904a71fa",
                new int[] {
                    10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                    20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                    30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                    40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                    50 });

            AssertPreviewValue("5fde015f-8a95-4b46-ba64-29de06850938",
                new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 });

            AssertPreviewValue("ace69b4e-5092-42cf-9fba-9aee6729509d",
                new int[] { 10, 20, 30, 40, 50 });

            AssertPreviewValue("30e9b7fd-fc09-4243-a34a-146ad841868a",
                new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 });

            AssertPreviewValue("bede0d80-6382-4430-9403-a14c3916e041", 5);
        }

        #endregion

        #region Revit Node Migration Tests

        #endregion

        #region Private Helper Methods

        private string GetDynPath(string sourceDynFile)
        {
            string sourceDynPath = this.GetTestDirectory();
            sourceDynPath = Path.Combine(sourceDynPath, @"core\migration\");
            return Path.Combine(sourceDynPath, sourceDynFile);
        }

        private CodeBlockNodeModel GetCodeBlockNode(string nodeGuid)
        {
            var workspace = Controller.DynamoModel.CurrentWorkspace;
            return workspace.NodeFromWorkspace<CodeBlockNodeModel>(
                System.Guid.Parse(nodeGuid));
        }

        #endregion
    }
}

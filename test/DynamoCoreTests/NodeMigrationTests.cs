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
            var cbn = workspace.FirstNodeFromWorkspace<CodeBlockNodeModel>();
            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            Assert.NotNull(cbn); // Ensure the StringInput node is migrated.
            Assert.AreEqual("\"First line\\nSecond line with\\ttab\\nThird line with \\\"quotes\\\"\";", cbn.Code);
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

        #endregion
    }
}

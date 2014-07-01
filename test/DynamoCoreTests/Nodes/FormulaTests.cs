using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DSCoreNodesUI;

using Dynamo.Tests;

using NUnit.Framework;

namespace Dynamo.Nodes
{
    [TestFixture]
    class FormulaTests : DSEvaluationUnitTest
    {
        [Test]
        public void FormulaWithIf()
        {
            string path = Path.Combine(GetTestDirectory(), "core", "formula", "formula-if.dyn");
            RunModel(path);

            var node = Controller.DynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Formula>();
            Assert.AreEqual(3, node.InPorts.Count);
            
            AssertPreviewValue(node.GUID.ToString(), 2);
        }
    }
}

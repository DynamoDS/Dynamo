using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DSCoreNodesUI;

using Dynamo.Tests;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class FormulaTests : DSEvaluationViewModelUnitTest
    {
        public override void Setup()
        {
            PreloadLibraries(new[] { "DSCoreNodes.dll" });
            base.Setup(); // Setup DynamoModel in this call.
        }

        [Test]
        public void FormulaWithIf()
        {
            string path = Path.Combine(GetTestDirectory(), "core", "formula", "formula-if.dyn");
            RunModel(path);

            var node = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Formula>();
            Assert.AreEqual(3, node.InPorts.Count);
            
            AssertPreviewValue(node.GUID.ToString(), 2);
        }
    }
}

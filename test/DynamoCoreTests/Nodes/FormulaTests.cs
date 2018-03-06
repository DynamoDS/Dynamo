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

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Formula>();
            Assert.AreEqual(3, node.InPorts.Count);
            
            AssertPreviewValue(node.GUID.ToString(), 2);
        }
    }
}

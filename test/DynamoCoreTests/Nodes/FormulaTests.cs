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
    }
}

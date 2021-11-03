using System.Collections.Generic;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class AtLevelTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestAtLevelForTranspose()
        {
            RunModel(@"core\atlevel\atlevel_transpose.dyn");
            AssertPreviewValue("2c65b3e2-17cb-4847-b11d-3c390f06dd0f", 
                new object[] 
                {
                    new object[] { 0, 3 },
                    new object[] { 1, 4 },
                    new object[] { 2, 5 }
                });
        }
    }
}

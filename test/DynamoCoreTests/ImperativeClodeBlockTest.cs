using System.Collections.Generic;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("ImperativeCode")]
    class ImperativeClodeBlockTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestCallStaticFunction()
        {
            RunModel(@"core\imperative\call_static_function.dyn");
            AssertPreviewValue("250880ed-5d34-49e7-b92b-2a7f9336f62b", new object[] { 3, 5 });
        }
    }
}

using System.Collections.Generic;
using Dynamo;
using NUnit.Framework;

namespace DisplayTests
{
    [TestFixture]
    class DisplayTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("GeometryColor.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void RegressMagn9666_DisplayCrash()
        {
            RunModel(@"core\library\DisplayReturn.dyn");
            var preview1 = GetPreviewValueInString("dc46358c-d71e-4764-9113-c7bc0dae8909");
            Assert.IsNotNullOrEmpty(preview1);
            Assert.IsTrue(preview1.Contains("Sphere"));

            var preview2 = GetPreviewValueInString("79bc4023-4f01-4b11-823e-75e615ad534c");
            Assert.IsNotNullOrEmpty(preview2);
            Assert.IsTrue(preview2.Contains("Point"));
        }
    }
}

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class ListAtLevelTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        string listTestFolder { get { return Path.Combine(TestDirectory, "core", "ListAtLevel"); } }

        [Test]
        public void TestAdd()
        {
            string testFilePath = Path.Combine(listTestFolder, "testadd.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("59483091-2910-423e-b0df-0263c604a717", new[] { "1foo", "2bar", "3baz", "4qux" });
        }

        [Test]
        public void TestUseListStructure1()
        {
            string testFilePath = Path.Combine(listTestFolder, "testdom.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("b71f4b57-9346-4411-ac67-f291d075909a", new object[] { new[] { new[] { 1, 2 }, new[] { 3, 4 } } });
        }

        [Test]
        public void TestNotUseListStructure()
        {
            string testFilePath = Path.Combine(listTestFolder, "testnotdom.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("b71f4b57-9346-4411-ac67-f291d075909a", new[] {1, 2, 3, 4});
        }

        [Test]
        public void TestUseListStructure2()
        {
            string testFilePath = Path.Combine(listTestFolder, "testdom2.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("ded17ade-881a-443c-b87a-8e2ca9b7e922", new [] { new [] { "3a", "7b" }, new [] { "11c" } });
        }

        [Test]
        public void TestUseListStructure3()
        {
            string testFilePath = Path.Combine(listTestFolder, "testdom3.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("ded17ade-881a-443c-b87a-8e2ca9b7e922", new[] { "3a", "7b", "11c" });
        }

        [Test]
        public void TestUseListStructureAndLacing1()
        {
            string testFilePath = Path.Combine(listTestFolder, "testdomlacing1.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("ae89387d-77b8-49e2-b59a-26a4b0a2ae9b", new[] { "3a", "7b", "11c", "11d" });
        }

        [Test]
        public void TestUseListStructureAndLacing2()
        {
            string testFilePath = Path.Combine(listTestFolder, "testdomlacing2.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("ae89387d-77b8-49e2-b59a-26a4b0a2ae9b", new[] { new[] { "3a", "7b" }, new[] { "11c", "15c" } });
        }
    }
}

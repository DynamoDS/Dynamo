using System.Collections.Generic;
using System.IO;
using DesignScript.Builtin;
using NUnit.Framework;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    public class DictionaryTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FFITarget.dll");
            
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestPassingNestedDictionary()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dictionary\AcceptNestedDictionary.dyn");
            OpenModel(dynFilePath);

            var validationData1 = Dictionary.ByKeysValues(new[] { "E", "G", "F", "H" },
                new object[] { 1, Dictionary.ByKeysValues(new[] { "A", "B", "C", "D" }, new object[] { 1, 2, 3, 4 }), 2, false });
            AssertPreviewValue("e1bd17cc-9f3d-437b-93bf-a9bc049318a2", validationData1);

        }

        [Test]
        public void TestPassingTypeInDictionary()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dictionary\AcceptDictWithType.dyn");
            OpenModel(dynFilePath);

            var validationData1 = Dictionary.ByKeysValues(new[] { "type", "list" },
                  new object[] { new FFITarget.DummyCollection(), new[] { 1 } });

            AssertPreviewValue("490bab9d-9155-4012-8ec4-9a66052eb8b3", validationData1);

        }

        [Test]
        public void TestPassingArbitraryDictionary()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dictionary\AcceptArbitraryDict.dyn");
            OpenModel(dynFilePath);

            var dict = Dictionary.ByKeysValues(new string[] { "A", "B", "C", "D" }, new object[] { 1, 2, 3, 4 });
            var validationData1 = Dictionary.ByKeysValues(new[] { "E", "F", "G", "H", "I" },
                  new object[] { 1, dict, new FFITarget.Dummy(), new object[] { 1, 2, 3, 4, new FFITarget.DummyCollection() }, false });

            AssertPreviewValue("756519ff963e4353a68535047fd50756", validationData1);
            AssertPreviewValue("48a84d7d9af14873a76a9e76df4f6058", validationData1);
        }

        [Test]
        public void DictionaryToString()
        {
            var dict = Dictionary.ByKeysValues(new string[] { "A", "B", "C", "D" }, new object[] { 1, 2, 3, 4 });
            var dictAsString = dict.ToString();
            // Entries can be in any order, so this is a safe way to assert the result.
            StringAssert.Contains("A:1", dictAsString);
            StringAssert.Contains("B:2", dictAsString);
            StringAssert.Contains("C:3", dictAsString);
            StringAssert.Contains("D:4", dictAsString);
            StringAssert.IsMatch(@"\{\w:\d,\w:\d,\w:\d,\w:\d\}", dictAsString);
        }

        [Test]
        public void EmptyDictionaryToString()
        {
            var dict = Dictionary.ByKeysValues(new string[0], new object[0]);
            Assert.AreEqual(dict.ToString(), "{}");
        }

        [Test,Category("FailureNET6")]
        public void Dictionary_ListOfValues_ReturnedAsExpected()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dictionary\Dictionary.Values2.dyn");
            OpenModel(dynFilePath);

            var validationData1 = new List<string> { "foo" };
            var validationData2 = new object[] { 1, new[] { 1, 2 } };

            //TODO NET6 unsure if we can control this - NET6 sometimes returns dictionary values in different orders
            //we may need to sort dictionary values to retain runtime behavior with net48.
            AssertPreviewValue("f64c1972520144d7a6d342937584c47e", validationData2);
            AssertPreviewValue("bc957838571c4c56af2bb714f9f40bd7", validationData1);
        }
    }
}

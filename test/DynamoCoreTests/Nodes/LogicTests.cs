using System.IO;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ComparisonTests : DSEvaluationViewModelUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "comparison"); } }

        [Test]
        public void testLessThan_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("fee9b04f-420f-4e2e-8dc1-20b7732d038c", 1);
            AssertPreviewValue("9093b858-7e36-4cc9-b665-3297bbabd280", 0);

        }

        [Test]
        public void testLessThan_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanStringInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("b300d0f8-dee2-4eb8-ac13-e77e337ebbf2", 1);
            AssertPreviewValue("f4e793a3-f01f-42d8-b084-884e4155dbd8", 0);

        }

        [Test]
        public void testLessThan_InvalidInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanInvalidInput.dyn");

            RunModel(testFilePath);
        }

        [Test]
        public void testEqual_InvalidInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testEqualInvalidInput.dyn");

            RunModel(testFilePath);
        }

        [Test]
        public void testEqual_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testEqualNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("75e739ed-082f-4eaa-8fd4-d0b88f44eaf4", 1);
            AssertPreviewValue("40e72290-42ce-457b-9153-23f4d63b7a9b", 0);

        }

        [Test]
        public void testEqual_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testEqualStringInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("171ec867-1434-444c-aa3a-8e61e167c477", 1);
            AssertPreviewValue("ce42fdfb-6fca-4da5-a8a5-fb65dd03567d", 0);

        }

        [Test]
        public void testGreaterThan_InvalidInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanInvalidInput.dyn");

            RunModel(testFilePath);
            //AssertPreviewValue("e52633e8-d520-473a-a0fb-9c835cf633dc", null);
        }

        [Test]
        public void testGreaterThan_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("97646425-6c25-4692-87c8-23c0a1aeda09", 0);
            AssertPreviewValue("3483359c-8fb4-4c37-be95-e56827920430", 1);

        }

        [Test]
        public void testGreaterThan_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanStringInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("bc06bc35-51f7-4db4-bbb5-f687449ec87b", 0);
            AssertPreviewValue("8c4d02fe-e6cb-4ff7-9093-82f246e1a88d", 1);

        }

        [Test]
        public void testGreaterThanOrEqual_InvalidInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualInvalidInput.dyn");

            RunModel(testFilePath);
        }

        [Test]
        public void testGreaterThanOrEqual_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("40101492-816b-4e68-9a14-d29f99239542", 0);
            AssertPreviewValue("e598cfcd-227c-4a7b-b26e-e7de6fbb6e2b", 1);
            AssertPreviewValue("c8962c58-3bfa-424e-baa3-f8c18a0a563f", 1);

        }

        [Test]
        public void testGreaterThanOrEqual_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualStringInput.dyn");

            RunModel(testFilePath);
            ViewModel.HomeSpace.Run();
            AssertPreviewValue("b5b07b97-bd27-4a31-bca1-59d791150b4b", 0);
            AssertPreviewValue("370abb34-9866-465e-98f0-8df73cad39ba", 1);
            AssertPreviewValue("6b703733-fb5a-431d-9180-08538dffbd8c", 1);

        }

        [Test]
        public void testLessThanOrEqual_InvalidInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualInvalidInput.dyn");

            RunModel(testFilePath);

        }

        [Test]
        public void testLessThanOrEqual_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualNumberInput.dyn");

            RunModel(testFilePath);
            ViewModel.HomeSpace.Run();
            AssertPreviewValue("49723f58-2a48-4cf8-815d-899bf3691938", 1);
            AssertPreviewValue("cfd23808-b7da-46f5-acb7-ffe9bd80da53", 1);
            AssertPreviewValue("0643bd3b-8d20-4300-aa96-1c1789b90303", 0);

        }

        [Test]
        public void testLessThanOrEqual_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualStringInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("05e8d59e-e183-4c20-a37f-03b6e97e465a", 1);
            AssertPreviewValue("8e37eefc-8555-417c-a2af-bf75e6d986db", 1);
            AssertPreviewValue("90bb3906-b6fe-4be5-b3cb-a97d92409a70", 0);

        }

        [Test]
        public void testCompare_DoubleInteger()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testCompare_DoubleInteger.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("604e36a9-df28-43ac-b86e-11f932a9f6e4", false);
            AssertPreviewValue("20a2f416-2a13-4afe-af00-c041d5997f40", true);
            AssertPreviewValue("a4c69409-8430-42a4-9769-2817a507b1b7", true);
            AssertPreviewValue("5801abfd-e00f-408a-8bb1-8289b795686f", false);
        }

        [Test]
        public void testCompare_BooleanInteger()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testCompare_BooleanInteger.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("604e36a9-df28-43ac-b86e-11f932a9f6e4", null);
            AssertPreviewValue("20a2f416-2a13-4afe-af00-c041d5997f40", null);
            AssertPreviewValue("a4c69409-8430-42a4-9769-2817a507b1b7", null);
            AssertPreviewValue("5801abfd-e00f-408a-8bb1-8289b795686f", null);
        }

        [Test]
        public void testCompare_BooleanBoolean()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testCompare_BooleanBoolean.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("604e36a9-df28-43ac-b86e-11f932a9f6e4", false);
            AssertPreviewValue("20a2f416-2a13-4afe-af00-c041d5997f40", true);
            AssertPreviewValue("a4c69409-8430-42a4-9769-2817a507b1b7", true);
            AssertPreviewValue("5801abfd-e00f-408a-8bb1-8289b795686f", false);
        }

        [Test]
        public void testCompare_StringInteger()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testCompare_StringInteger.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("604e36a9-df28-43ac-b86e-11f932a9f6e4", null);
            AssertPreviewValue("20a2f416-2a13-4afe-af00-c041d5997f40", null);
            AssertPreviewValue("a4c69409-8430-42a4-9769-2817a507b1b7", null);
            AssertPreviewValue("5801abfd-e00f-408a-8bb1-8289b795686f", null);
        }

        [Test]
        public void testCompare_StringDouble()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testCompare_StringDouble.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("604e36a9-df28-43ac-b86e-11f932a9f6e4", null);
            AssertPreviewValue("20a2f416-2a13-4afe-af00-c041d5997f40", null);
            AssertPreviewValue("a4c69409-8430-42a4-9769-2817a507b1b7", null);
            AssertPreviewValue("5801abfd-e00f-408a-8bb1-8289b795686f", null);
        }
    }

    [TestFixture]
    class ConditionalTest : DSEvaluationViewModelUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "conditional"); } }

        [Test]
        public void testAnd_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testAndNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("893a8746-b74f-4078-a125-8b96a48ec782", 0);
            AssertPreviewValue("6fa95218-d960-4069-ab38-0fec7c815e06", 1);
            AssertPreviewValue("aa4b3295-6c95-4a0a-b848-69bf72353c36", 0);

        }

        [Test]
        public void testIf_StringInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testIfNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("1e99e389-04ba-4a9f-9ef4-d188058f734f", 1);
            AssertPreviewValue("ae08cfd2-3fb3-41e0-94f4-a70ead3e7466", 0);
            AssertPreviewValue("75da9972-b458-45d7-8673-42e7c74e42b6", 0);
            AssertPreviewValue("1faadb07-62e9-42f1-9c01-18b6673e53cf", 0);
            AssertPreviewValue("a1cb3c11-4939-40fb-ac7b-ad80c9e3c576", 1);
            AssertPreviewValue("6a3cc1a4-a353-4870-8c1c-848298ebe050", 3);
        }

        [Test]
        public void testNot_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testNotNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("893a8746-b74f-4078-a125-8b96a48ec782", 0);
            AssertPreviewValue("6fa95218-d960-4069-ab38-0fec7c815e06", 1);

        }

        [Test]
        public void testOr_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testOrNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("3892c87e-ee6b-4e57-9132-85ac4512f676", 1);
            AssertPreviewValue("d133fab7-91d3-4e1e-ada5-69a32def1bb5", 1);
            AssertPreviewValue("bbbd263e-f424-400c-839e-34c86b6e4c64", 0);

        }

        [Test]
        public void testXor_NumberInput()
        {

            string testFilePath = Path.Combine(logicTestFolder, "testXorNumberInput.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("893a8746-b74f-4078-a125-8b96a48ec782", 1);
            AssertPreviewValue("6fa95218-d960-4069-ab38-0fec7c815e06", 0);
            AssertPreviewValue("aa4b3295-6c95-4a0a-b848-69bf72353c36", 0);

        }

    }
}
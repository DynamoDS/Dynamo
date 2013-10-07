using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Dynamo.ViewModels;
using System.IO;
using System.Reflection;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using String = System.String;

namespace Dynamo.Tests
{
    [TestFixture]
    class ComparisonTests : DynamoUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "comparison"); } }

        [Test]
        public void testLessThan_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fee9b04f-420f-4e2e-8dc1-20b7732d038c");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("9093b858-7e36-4cc9-b665-3297bbabd280");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testLessThan_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("b300d0f8-dee2-4eb8-ac13-e77e337ebbf2");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f4e793a3-f01f-42d8-b084-884e4155dbd8");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testLessThan_InvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanInvalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void testEqual_InvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testEqualInvalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void testEqual_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testEqualNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("75e739ed-082f-4eaa-8fd4-d0b88f44eaf4");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("40e72290-42ce-457b-9153-23f4d63b7a9b");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testEqual_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testEqualStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("171ec867-1434-444c-aa3a-8e61e167c477");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("ce42fdfb-6fca-4da5-a8a5-fb65dd03567d");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testGreaterThan_InvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanInvalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void testGreaterThan_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("97646425-6c25-4692-87c8-23c0a1aeda09");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("3483359c-8fb4-4c37-be95-e56827920430");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testGreaterThan_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("bc06bc35-51f7-4db4-bbb5-f687449ec87b");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("8c4d02fe-e6cb-4ff7-9093-82f246e1a88d");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testGreaterThanOrEqual_InvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualInvalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void testGreaterThanOrEqual_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("40101492-816b-4e68-9a14-d29f99239542");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("e598cfcd-227c-4a7b-b26e-e7de6fbb6e2b");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("c8962c58-3bfa-424e-baa3-f8c18a0a563f");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            double expectedResult3 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

        [Test]
        public void testGreaterThanOrEqual_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testGreaterThanOrEqualStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("b5b07b97-bd27-4a31-bca1-59d791150b4b");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("370abb34-9866-465e-98f0-8df73cad39ba");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6b703733-fb5a-431d-9180-08538dffbd8c");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            double expectedResult3 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

        [Test]
        public void testLessThanOrEqual_InvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualInvalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void testLessThanOrEqual_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("49723f58-2a48-4cf8-815d-899bf3691938");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("cfd23808-b7da-46f5-acb7-ffe9bd80da53");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("0643bd3b-8d20-4300-aa96-1c1789b90303");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 1;
            double expectedResult3 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

        [Test]
        public void testLessThanOrEqual_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("05e8d59e-e183-4c20-a37f-03b6e97e465a");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("8e37eefc-8555-417c-a2af-bf75e6d986db");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("90bb3906-b6fe-4be5-b3cb-a97d92409a70");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 1;
            double expectedResult3 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }
    }

    [TestFixture]
    class ConditionalTest : DynamoUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "conditional"); } }

        [Test]
        public void testAnd_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testAndNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("893a8746-b74f-4078-a125-8b96a48ec782");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6fa95218-d960-4069-ab38-0fec7c815e06");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("aa4b3295-6c95-4a0a-b848-69bf72353c36");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            double expectedResult3 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

        [Test]
        public void testAnd_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testAndStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("893a8746-b74f-4078-a125-8b96a48ec782");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6fa95218-d960-4069-ab38-0fec7c815e06");

            String actualResult1 = watch1.GetValue(0).GetStringFromFSchemeValue();
            String actualResult2 = watch2.GetValue(0).GetStringFromFSchemeValue();
            String expectedResult1 = "b";
            String expectedResult2 = "a";
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testIf_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testIfNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1e99e389-04ba-4a9f-9ef4-d188058f734f");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("ae08cfd2-3fb3-41e0-94f4-a70ead3e7466");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("75da9972-b458-45d7-8673-42e7c74e42b6");
            Watch watch4 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1faadb07-62e9-42f1-9c01-18b6673e53cf");
            Watch watch5 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("a1cb3c11-4939-40fb-ac7b-ad80c9e3c576");
            Watch watch6 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6a3cc1a4-a353-4870-8c1c-848298ebe050");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult4 = watch4.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult5 = watch5.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult6 = watch6.GetValue(0).GetDoubleFromFSchemeValue();

            double expectedResult1 = 1;
            double expectedResult2 = 0;
            double expectedResult3 = 0;
            double expectedResult4 = 0;
            double expectedResult5 = 1;
            double expectedResult6 = 3;

            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
            Assert.AreEqual(expectedResult4, actualResult4);
            Assert.AreEqual(expectedResult5, actualResult5);
            Assert.AreEqual(expectedResult6, actualResult6);
        }

        [Test]
        public void testNot_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testNotNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("893a8746-b74f-4078-a125-8b96a48ec782");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6fa95218-d960-4069-ab38-0fec7c815e06");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Test]
        public void testOr_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testOrNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("3892c87e-ee6b-4e57-9132-85ac4512f676");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d133fab7-91d3-4e1e-ada5-69a32def1bb5");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("bbbd263e-f424-400c-839e-34c86b6e4c64");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 1;
            double expectedResult3 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

        [Test]
        public void testXor_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testXorNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("893a8746-b74f-4078-a125-8b96a48ec782");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6fa95218-d960-4069-ab38-0fec7c815e06");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("aa4b3295-6c95-4a0a-b848-69bf72353c36");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult3 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 1;
            double expectedResult2 = 0;
            double expectedResult3 = 0;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
            Assert.AreEqual(expectedResult3, actualResult3);
        }

    }
}
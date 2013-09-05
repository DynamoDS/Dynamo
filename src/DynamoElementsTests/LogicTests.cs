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
    class LogicTests : DynamoUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "comparison"); } }

        [Test]
        public void testLessThanNumberInput()
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
        public void testLessThanStringInput()
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
        public void testLessThanInvalidInput()
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
        public void testEqualInvalidInput()
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
        public void testEqualNumberInput()
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
        public void testEqualStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testEqualNumberInput.dyn");

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
        public void testGreaterThanInvalidInput()
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
        public void testGreaterThanNumberInput()
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
        public void testGreaterThanStringInput()
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
        public void testGreaterThanOrEqualInvalidInput()
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
        public void testGreaterThanOrEqualNumberInput()
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
        public void testGreaterThanOrEqualStringInput()
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
        public void testLessThanOrEqualInvalidInput()
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
        public void testLessThanOrEqualNumberInput()
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
        public void testLessThanOrEqualStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanOrEqualStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Watch watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fd0fc05c-299c-48f6-9aba-42f2448c72ee");
            Watch watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("e3bd792e-5f7d-4326-b10f-6096c7092d79");
            Watch watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("84ea405b-4f66-4b7f-9054-74cc7f686adb");

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
}
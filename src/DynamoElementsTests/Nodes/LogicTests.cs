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
    public class ComparisonTests : DynamoUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "comparison"); } }

        [Test]
        public void testLessThan_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testLessThanNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            LessThan watch1 = model.CurrentWorkspace.NodeFromWorkspace<LessThan>("604e36a9-df28-43ac-b86e-11f932a9f6e4");
            LessThan watch2 = model.CurrentWorkspace.NodeFromWorkspace<LessThan>("20a2f416-2a13-4afe-af00-c041d5997f40");

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
            LessThan watch1 = model.CurrentWorkspace.NodeFromWorkspace<LessThan>("604e36a9-df28-43ac-b86e-11f932a9f6e4");
            LessThan watch2 = model.CurrentWorkspace.NodeFromWorkspace<LessThan>("20a2f416-2a13-4afe-af00-c041d5997f40");

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
            
            dynSettings.Controller.RunExpression();

            Assert.AreEqual(0, model.CurrentWorkspace.FirstNodeFromWorkspace<Equal>().OldValue.GetDoubleFromFSchemeValue());
        }

        [Test]
        public void testEqual_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testEqualNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Equal watch1 = model.CurrentWorkspace.NodeFromWorkspace<Equal>("8c3ffac4-60e3-4898-9c8d-ffb5ccee2211");
            Equal watch2 = model.CurrentWorkspace.NodeFromWorkspace<Equal>("f935bd04-cade-4b63-9d4c-a85c778cafea");

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
            Equal watch1 = model.CurrentWorkspace.NodeFromWorkspace<Equal>("8c3ffac4-60e3-4898-9c8d-ffb5ccee2211");
            Equal watch2 = model.CurrentWorkspace.NodeFromWorkspace<Equal>("f935bd04-cade-4b63-9d4c-a85c778cafea");

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
            GreaterThan watch1 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThan>("e52633e8-d520-473a-a0fb-9c835cf633dc");
            GreaterThan watch2 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThan>("4fa1ffcf-4e8a-49c6-9917-9353caa9305c");

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
            GreaterThan watch1 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThan>("e52633e8-d520-473a-a0fb-9c835cf633dc");
            GreaterThan watch2 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThan>("4fa1ffcf-4e8a-49c6-9917-9353caa9305c");

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
            GreaterThanEquals watch1 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("a212d397-5c07-48da-9321-9df27bddb2a4");
            GreaterThanEquals watch2 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("24f8d658-86f4-4b62-92e6-1cf3868f53f7");
            GreaterThanEquals watch3 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("19028dd3-7a41-45be-ac99-8d5de14cd590");

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
            GreaterThanEquals watch1 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("a212d397-5c07-48da-9321-9df27bddb2a4");
            GreaterThanEquals watch2 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("19028dd3-7a41-45be-ac99-8d5de14cd590");
            GreaterThanEquals watch3 = model.CurrentWorkspace.NodeFromWorkspace<GreaterThanEquals>("24f8d658-86f4-4b62-92e6-1cf3868f53f7");

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
            LessThanEquals watch1 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("8ed276d0-d1e0-4e38-bcaa-77c2c3b4819f");
            LessThanEquals watch2 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("4f115ff1-17b2-4386-a38b-4546e7bf39b6");
            LessThanEquals watch3 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("c3167b61-df46-4034-8319-aefc9e7565d4");

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
            LessThanEquals watch1 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("8ed276d0-d1e0-4e38-bcaa-77c2c3b4819f");
            LessThanEquals watch2 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("4f115ff1-17b2-4386-a38b-4546e7bf39b6");
            LessThanEquals watch3 = model.CurrentWorkspace.NodeFromWorkspace<LessThanEquals>("c3167b61-df46-4034-8319-aefc9e7565d4");

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
    public class ConditionalTest : DynamoUnitTest
    {
        private string logicTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "logic", "conditional"); } }

        [Ignore]
        public void testAnd_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testAndNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            And watch1 = model.CurrentWorkspace.NodeFromWorkspace<And>("24a185da-6176-4b08-bba3-214ccc379dc7");
            And watch2 = model.CurrentWorkspace.NodeFromWorkspace<And>("1db98a6e-0fde-4911-9da5-800dc820fab3");
            And watch3 = model.CurrentWorkspace.NodeFromWorkspace<And>("be726ccd-686c-40d9-a9fa-ed4990434906");

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

        [Ignore]
        public void testAnd_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testAndStringInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            And watch1 = model.CurrentWorkspace.NodeFromWorkspace<And>("9d18c5d9-7678-4818-b4f6-474c29c358ff");
            And watch2 = model.CurrentWorkspace.NodeFromWorkspace<And>("2e685247-b78a-4f0d-8e43-57ca740857c8");

            String actualResult1 = watch1.GetValue(0).GetStringFromFSchemeValue();
            String actualResult2 = watch2.GetValue(0).GetStringFromFSchemeValue();
            String expectedResult1 = "b";
            String expectedResult2 = "a";
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Ignore]
        public void testIf_StringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testIfNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Conditional watch1 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("a00a70d6-6806-46d3-9bbc-52e30108499b");
            Conditional watch2 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("f1c1e1bb-c112-41ad-b38e-67e2512d2c97");
            Conditional watch3 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("66361adf-57db-4bcf-9bec-3e7c07bf025b");
            Conditional watch4 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("8f4b6aa0-5c1d-4dd3-90cb-bd66e4d921ac");
            Conditional watch5 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("38f7e43f-9361-45de-9ef1-cd51af9afe6c");
            Conditional watch6 = model.CurrentWorkspace.NodeFromWorkspace<Conditional>("b6f5ae7a-cb3b-4eed-88a5-259ea2a12302");

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
            Not watch1 = model.CurrentWorkspace.NodeFromWorkspace<Not>("40a668a4-f2db-424e-bfea-0e380920dd9f");
            Not watch2 = model.CurrentWorkspace.NodeFromWorkspace<Not>("cad4dc45-b62c-4f0b-b091-fb290efb14eb");

            double actualResult1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actualResult2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double expectedResult1 = 0;
            double expectedResult2 = 1;
            Assert.AreEqual(expectedResult1, actualResult1);
            Assert.AreEqual(expectedResult2, actualResult2);
        }

        [Ignore]
        public void testOr_NumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(logicTestFolder, "testOrNumberInput.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            Or watch1 = model.CurrentWorkspace.NodeFromWorkspace<Or>("ae73676a-b5fb-4745-b1d1-2b757d659a7e");
            Or watch2 = model.CurrentWorkspace.NodeFromWorkspace<Or>("b148eef2-3bfc-4b00-a3ab-837f0c83e0ba");
            Or watch3 = model.CurrentWorkspace.NodeFromWorkspace<Or>("5932a88d-c798-4bdb-8a0e-d58b08c71134");

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
            Xor watch1 = model.CurrentWorkspace.NodeFromWorkspace<Xor>("3dd18676-abe9-49db-a60c-badecf2322fd");
            Xor watch2 = model.CurrentWorkspace.NodeFromWorkspace<Xor>("29930505-2c52-40a0-a37c-9be2d383b4b5");
            Xor watch3 = model.CurrentWorkspace.NodeFromWorkspace<Xor>("06f44e0b-824b-46a9-9e39-efa4500640b6");

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
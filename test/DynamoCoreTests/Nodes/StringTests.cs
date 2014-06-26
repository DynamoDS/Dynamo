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
using Dynamo.DSEngine;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;
using String = System.String;

namespace Dynamo.Tests
{
    class StringTests : DSEvaluationUnitTest
    {
        string localDynamoStringTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "string");}}

        #region concat string test cases  

        [Test]
        public void TestConcatStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("8c7c1a80-021b-4064-b9d1-873a0538bb0b", "123abc	    !@#    ");

        }

        [Test]
        public void TestConcatStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("cc16be22-af85-4626-b759-4a82e10bf1b0", "");

        }

        [Test]
        public void TestConcatStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("8c7c1a80-021b-4064-b9d1-873a0538bb0b",
            "Don't feel like picking up my phone, so leave a message at the tone Don't feel like picking up my phone, so leave a message at the tone ");

        }

        [Test]
        public void TestConcatStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("8c7c1a80-021b-4064-b9d1-873a0538bb0b", "yesterday today.tomorrow");
        }

        [Test]
        public void TestConcatStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_invalidInput.dyn");

            RunModel(testFilePath); //later will add node and connector count verification.

            Assert.Inconclusive("Need behavior confirmation");
        }

        [Test]
        public void TestConcatStringMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("fbc947fb-460b-49b9-8460-b223bffb63d5", new string[] { "abcd", "efgh" });
        }

        [Test]
        public void TestConcatStringInListMap()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatStringInListMap.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("a105ad39-9b1c-44aa-a2cb-37866ea48dd0", new string[] { "0a", "10a", "20a", "30a", "40a", "50a" });
        }

        #endregion

        #region substring test cases  

        [Test]
        public void TestSubStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "");

        }

        [Test]
        public void TestSubStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "rainbow");

        }

        [Test]
        public void TestSubStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "rainbow");
        }

        [Test]
        public void TestSubStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("aa03e3b7-066b-4564-91bc-69c247bc8bdb");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        [Test]
        public void TestSubStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "rainbow");

        }

        #endregion

        #region join string test cases  

        [Test]
        public void TestJoinStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", ".");

        }

        [Test]
        public void TestJoinStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "y.x");

        }

        [Test]
        public void TestJoinStringSingleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_singleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "a");
        }

        [Test]
        public void TestJoinStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "first.second");

        }

        #endregion

        #region number to string test cases  

        [Test]
        public void TestNumberToStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f8767579-f7c1-475f-980e-7cd6a42684c8", "25");

        }

        [Test]
        public void TestNumberToStringInvalidInput()
        {
            Assert.Inconclusive("ToString node can accept a function object, and convert it to a string. "
            +"This test case will fail because of this");
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_invalidInput.dyn");

            RunModel(testFilePath);

            // The input is a function object "String Length" unconnected to any input
            // To assert that the watch node is not displaying something such as "Pointer, opdata = 10, metaData = 58"
            StringAssert.DoesNotContain("ointer", model.CurrentWorkspace.NodeFromWorkspace<Watch>(
                "f8767579-f7c1-475f-980e-7cd6a42684c8").CachedValue.Data.ToString());
        }

        [Test]
        public void TestNumberToStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f8767579-f7c1-475f-980e-7cd6a42684c8", 123456789);
            AssertPreviewValue("5a974eeb-6bca-4029-9948-c6af1c9fe913", -123456789);
            AssertPreviewValue("ce2c9ef8-8fac-427a-b550-ecec8f66aacf", 3.456000);
            AssertPreviewValue("bd14730f-fddc-4301-9d63-7b1e77eeb72a", -3.456000);

        }

        #endregion

        #region split string test cases  

        [Test]
        public void TestSplitStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "");

        }

        [Test]
        public void TestSplitStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_fromFile.dyn");

            RunModel(testFilePath);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,"today"},
                {1,"yesterday"},
                {2,"tomorrow"},
            };

            SelectivelyAssertPreviewValues("f72f6210-b32f-4dc4-9b2a-61f0144a0109", validationData);

        }

        [Test]
        public void TestSplitStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", new string[] { "1", "2" });

        }

        [Test]
        public void TestSplitStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("d36759fb-da7a-475a-a43a-9c85996ad55d");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        [Test]
        public void TestSplitStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", new string[]{"today","yesterday"});
        }

        [Test]
        public void TestSplitStringMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_multipleInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109",
                new string[][]
                {
                    new string[] { "today", "yesterday" },
                    new string[] { "tomorrow", "and forever" }
                });
        }
 
        #endregion

        #region string length test cases  

        [Test]
        public void TestStringLengthEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", 0);

        }

        [Test]
        public void TestStringLengthFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", 16);

        }

        [Test]
        public void TestStringLengthFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", 3);

        }

        [Test]
        public void TestStringLengthMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", new object[] { 2, 3 });
        }

        [Test]
        public void TestStringLengthNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", 15);

        }

        #endregion

        #region string to number  

        [Test]
        public void TestStringToNumberEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_empltyString.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("0f912454-b278-499f-b15f-c42c039a5453");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        [Test]
        public void TestStringToNumberFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f8767579-f7c1-475f-980e-7cd6a42684c8", 123521);
        }

        [Test]
        public void TestStringToNumberFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f8767579-f7c1-475f-980e-7cd6a42684c8", 12);
        }

        [Test]
        public void TestStringToNumberInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("0f912454-b278-499f-b15f-c42c039a5453");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        [Test]
        public void TestStringToNumberNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("ca09bc3a-35c3-488f-a013-c05a5b7733c5", 12);
            AssertPreviewValue("251210e5-2e04-4e81-b11d-39a8aff10887", 12.3);
            AssertPreviewValue("898ee89d-a934-4b43-a051-da3459be329a", 1000);
            AssertPreviewValue("0afc0a8f-3d8a-4d7c-a2ec-d868cbb29b5f", 123456789);
        }

        #endregion

        #region string case test cases  

        [Test]
        public void TestStringCaseEmptyInput()
        {
            //DynamoModel model = Controller.DynamoModel;
            //string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_emptyString.dyn");

            //RunModel(testFilePath);
            //AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "");

            Assert.Inconclusive("String.StringCase node not migrated");
        }

        [Test]
        public void TestStringCaseFileInput()
        {
            //DynamoModel model = Controller.DynamoModel;
            //string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFile.dyn");

            //RunModel(testFilePath);
            //AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "RAINY DAY");

            Assert.Inconclusive("String.StringCase node not migrated");

        }

        [Test]
        public void TestStringCaseFunctionInput()
        {
            //DynamoModel model = Controller.DynamoModel;
            //string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFunction.dyn");

            //RunModel(testFilePath);
            //AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "SUNNYDAY");

            Assert.Inconclusive("String.StringCase node not migrated");

        }

        [Test]
        public void TestStringCaseInvalidInput()
        {
            //DynamoModel model = Controller.DynamoModel;
            //string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_invalidInput.dyn");

            //RunModel(testFilePath);

            //Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            //Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            //NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("294a2376-6751-43c3-a8b1-5492fa942dbe");
            //Assert.AreEqual(ElementState.Warning, nodeModel.State);

            Assert.Inconclusive("String.StringCase node not migrated");

        }

        [Test]
        public void TestStringCaseNormalInput()
        {
            //DynamoModel model = Controller.DynamoModel;
            //string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_normal.dyn");

            //RunModel(testFilePath);
            //AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "RAINY DAY");
            //AssertPreviewValue("77a8c84b-b5bb-46f1-a550-7b3d5441c0a1", "rainy day");

            Assert.Inconclusive("String.StringCase node not migrated");

        }

        #endregion

        #region to string test cases  

        [Ignore]
        public void TestToStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", @"""\n");

        }

        [Ignore]
        public void TestToStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "can you read this\n");

        }

        [Ignore]
        public void TestToStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", new string[] { "1\n", "2\n" });

        }

        [Ignore]
        public void TestToStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "123456\n");
        }

        #endregion

        #region String.Contains test cases

        [Test]
        public void TestStringContainsNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringContains_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", true);
            AssertPreviewValue("2cfb7395-1ee2-4b01-823a-3d17e6b2b776", false);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", false);
            AssertPreviewValue("cc7f1487-f493-409e-96f8-6ab9890184b6", false);
        }

        [Test]
        public void TestStringContainsMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringContains_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", new object[] { true, false, true });
            AssertPreviewValue("a1e5f019-4dea-48c2-a30a-0491549bcd74", new object[] { false, true });
        }

        [Test]
        public void TestStringContainsEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringContains_emptyString.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", false);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", true);
        }

        [Test]
        public void TestStringContainsInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringContains_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("9e19aed1-90ec-4de3-bb5d-e0b547f69138");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        #endregion

        #region String.StartsWith and String.EndsWith test cases

        [Test]
        public void TestStringStartsWithNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringStartsWith_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", true);
            AssertPreviewValue("2cfb7395-1ee2-4b01-823a-3d17e6b2b776", false);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", false);
            AssertPreviewValue("cc7f1487-f493-409e-96f8-6ab9890184b6", false);
        }

        [Test]
        public void TestStringEndsWithNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringEndsWith_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", false);
            AssertPreviewValue("2cfb7395-1ee2-4b01-823a-3d17e6b2b776", false);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", true);
            AssertPreviewValue("cc7f1487-f493-409e-96f8-6ab9890184b6", false);
        }

        #endregion

        #region String.IndexOf test cases

        [Test]
        public void TestStringIndexOfNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringIndexOf_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", 0);
            AssertPreviewValue("2cfb7395-1ee2-4b01-823a-3d17e6b2b776", -1);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", 16);
            AssertPreviewValue("cc7f1487-f493-409e-96f8-6ab9890184b6", -1);
        }

        [Test]
        public void TestStringIndexOfMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringIndexOf_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289",
                new object[] { 2, 6, -1 });

            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56",
                new object[] { 18, 10, 2, 12, -1 });
        }

        [Test]
        public void TestStringIndexOfEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringIndexOf_emptyString.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("4e7c00fe-120f-4779-957f-a1e909a20289", -1);
            AssertPreviewValue("a983daca-7755-49f1-8a15-061691172e56", 0);
        }

        #endregion

        #region String.Insert test cases

        [Test]
        public void TestStringInsertNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringInsert_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("2b1997c5-d72d-443f-9df8-29a33cc0fbd0", "This is a string.");
            AssertPreviewValue("36774b24-e6dd-41b1-889b-3847aa0ad6ee", "This is another string.");
        }

        [Test]
        public void TestStringInsertMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringInsert_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("2b1997c5-d72d-443f-9df8-29a33cc0fbd0",
                new string[] {
                   "My name is Jude.",
                   "My name is Bryan.",
                   "My name is Dave."
                });

            AssertPreviewValue("36774b24-e6dd-41b1-889b-3847aa0ad6ee",
                new string[] { "Arrraagh", "Aarrragh", "Aaarrrgh" });
        }

        [Test]
        public void TestStringInsertInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringInsert_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("a20ad56b-cd14-4aa3-b39c-d9bd0ac0e9f8");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        #endregion

        #region String.Replace test cases

        [Test]
        public void TestStringReplaceNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringReplace_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("36774b24-e6dd-41b1-889b-3847aa0ad6ee",
                "A String object's text content can be replaced by another text.");
        }

        [Test]
        public void TestStringReplaceMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringReplace_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("36774b24-e6dd-41b1-889b-3847aa0ad6ee",
                new string[] {
                    "I am going to meet him and Dave.",
                    "I am going to meet Bryan and him."
                });

            AssertPreviewValue("b61394da-efa0-4e7e-aec7-9f55a90e8449",
                new string[] {
                    "My name is Dave.",
                    "Dave is my name."
                });
        }

        [Test]
        public void TestStringReplaceEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringReplace_emptyString.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("36774b24-e6dd-41b1-889b-3847aa0ad6ee",
                "A String object's content can be replaced by another.");
        }

        [Test]
        public void TestStringReplaceInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringReplace_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("6b6593df-5c71-4472-a91b-7fc69feb14d4");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        #endregion

        #region String.TrimWhitespace test cases

        [Test]
        public void TestTrimWhitespaceNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestTrimWhitespace_normal.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("6a233902-f291-4cd3-aa46-de4fa36ecac7", "123abc");
            AssertPreviewValue("cc4c02a1-1438-4d7d-b62c-08d4b31f9a77", "abc234");
            AssertPreviewValue("bd08fd1e-7b3b-4a27-a7aa-38f3b30ae5f2", "345abc");
            AssertPreviewValue("9bd75396-03b6-4ded-8695-eb64510c4ab4", "456   abc789");
        }

        [Test]
        public void TestSTrimWhitespaceMultipleInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestTrimWhitespace_multipleInput.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("6a233902-f291-4cd3-aa46-de4fa36ecac7",
                new string[] {
                    "123abc",
                    "abc234",
                    "345abc",
                    "456   abc789"
                });
        }

        [Test]
        public void TestTrimWhitespaceEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestTrimWhitespace_emptyString.dyn");

            RunModel(testFilePath);

            AssertPreviewValue("6a233902-f291-4cd3-aa46-de4fa36ecac7", 0);
            AssertPreviewValue("cc4c02a1-1438-4d7d-b62c-08d4b31f9a77", 0);
            AssertPreviewValue("bd08fd1e-7b3b-4a27-a7aa-38f3b30ae5f2", 0);
            AssertPreviewValue("9bd75396-03b6-4ded-8695-eb64510c4ab4", 0);
        }

        [Test]
        public void TestTrimWhitespaceInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestTrimWhitespace_invalidInput.dyn");

            RunModel(testFilePath);

            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            NodeModel nodeModel = model.CurrentWorkspace.NodeFromWorkspace("6a6a3d81-57bc-44ae-af37-9dabcf25b8e4");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);
        }

        #endregion

    }
}

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

            RunModel(testFilePath);//later will add node and connector count verification.
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
        public void TestJoinStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_invalidInput.dyn");

            RunModel(testFilePath);//later will add node and connector count verification.

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
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_invalidInput.dyn");

            RunModel(testFilePath);//later will add node and connector count verification.

        }

        [Test]
        public void TestNumberToStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f8767579-f7c1-475f-980e-7cd6a42684c8", "123456789");
            AssertPreviewValue("5a974eeb-6bca-4029-9948-c6af1c9fe913", "-123456789");
            AssertPreviewValue("ce2c9ef8-8fac-427a-b550-ecec8f66aacf", "3.456");
            AssertPreviewValue("bd14730f-fddc-4301-9d63-7b1e77eeb72a", "-3.456");

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

            RunModel(testFilePath);//later will add node and connector count verification.

        }

        [Test]
        public void TestSplitStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", new string[]{"today","yesterday"});
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
        public void TestStringLengthInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_invalidInput.dyn");

            RunModel(testFilePath);//later will add node and connector count verification.

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

            RunModel(testFilePath);//later will add node and connector count verification.

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

            RunModel(testFilePath);//later will add node and connector count verification.

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
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_emptyString.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "");

        }

        [Test]
        public void TestStringCaseFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFile.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "RAINY DAY");

        }

        [Test]
        public void TestStringCaseFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFunction.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "SUNNYDAY");
        }

        [Test]
        public void TestStringCaseInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_invalidInput.dyn");

            RunModel(testFilePath);//later will add node and connector count verification.

        }

        [Test]
        public void TestStringCaseNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_normal.dyn");

            RunModel(testFilePath);
            AssertPreviewValue("f72f6210-b32f-4dc4-9b2a-61f0144a0109", "RAINY DAY");
            AssertPreviewValue("77a8c84b-b5bb-46f1-a550-7b3d5441c0a1", "rainy day");

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

    }
}

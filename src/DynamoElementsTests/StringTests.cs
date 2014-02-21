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
    class StringTests : DynamoUnitTest
    {
        string localDynamoStringTestFloder { get { return Path.Combine(GetTestDirectory(), "core", "string");}}

        #region concat string test cases

        [Test]
        public void TestConcatStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("8c7c1a80-021b-4064-b9d1-873a0538bb0b");

            String actual = string.Empty;
            String expected = "123abc	    !@#    ";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestConcatStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("cc16be22-af85-4626-b759-4a82e10bf1b0");

            String actual = string.Empty;
            String expected = "";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestConcatStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("8c7c1a80-021b-4064-b9d1-873a0538bb0b");

            String actual = string.Empty;
            String expected = "Don't feel like picking up my phone, so leave a message at the tone Don't feel like picking up my phone, so leave a message at the tone ";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestConcatStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("8c7c1a80-021b-4064-b9d1-873a0538bb0b");

            String actual = string.Empty;
            String expected = "yesterday today.tomorrow";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestConcatStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestConcatString_invalidInput.dyn");

            model.Open(testFilePath);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        #endregion

        #region substring test cases

        [Test]
        public void TestSubStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestSubStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "rainbow";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestSubStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "rainbow";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestSubStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestSubStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSubstring_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "rainbow";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region join string test cases

        [Test]
        public void TestJoinStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = ".";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestJoinStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "y.x";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestJoinStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestJoinStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestJoinString_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "first.second";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region number to string test cases

        [Test]
        public void TestNumberToStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f8767579-f7c1-475f-980e-7cd6a42684c8");

            String actual = string.Empty;
            String expected = "25";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestNumberToStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestNumberToStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestNumberToString_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);

            var watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f8767579-f7c1-475f-980e-7cd6a42684c8");
            var watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("5a974eeb-6bca-4029-9948-c6af1c9fe913");
            var watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("ce2c9ef8-8fac-427a-b550-ecec8f66aacf");
            var watch4 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("bd14730f-fddc-4301-9d63-7b1e77eeb72a");
            
            String actual1 = string.Empty;
            String actual2 = string.Empty;
            String actual3 = string.Empty;
            String actual4 = string.Empty;

            String expected1 = "123456789";
            String expected2 = "-123456789";
            String expected3 = "3.456";
            String expected4 = "-3.456";

            FSchemeInterop.Utils.Convert(watch1.GetValue(0), ref actual1);
            FSchemeInterop.Utils.Convert(watch2.GetValue(0), ref actual2);
            FSchemeInterop.Utils.Convert(watch3.GetValue(0), ref actual3);
            FSchemeInterop.Utils.Convert(watch4.GetValue(0), ref actual4);

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        #endregion

        #region split string test cases

        [Test]
        public void TestSplitStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestSplitStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String expected1 = "today";
            String expected2 = "yesterday";
            String expected3 = "tomorrow";

            FSharpList<FScheme.Value> splitedStrings = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(expected1, splitedStrings[0].GetStringFromFSchemeValue());
            Assert.AreEqual(expected2, splitedStrings[1].GetStringFromFSchemeValue());
            Assert.AreEqual(expected3, splitedStrings[2].GetStringFromFSchemeValue());
        }

        [Test]
        public void TestSplitStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String expected1 = "1";
            String expected2 = "2";

            FSharpList<FScheme.Value> splitedStrings = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(expected1, splitedStrings[0].GetStringFromFSchemeValue());
            Assert.AreEqual(expected2, splitedStrings[1].GetStringFromFSchemeValue());
        }

        [Test]
        public void TestSplitStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestSplitStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestSplitString_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String expected1 = "today";
            String expected2 = "yesterday";

            FSharpList<FScheme.Value> splitedStrings = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(expected1, splitedStrings[0].GetStringFromFSchemeValue());
            Assert.AreEqual(expected2, splitedStrings[1].GetStringFromFSchemeValue());
        }

        #endregion

        #region string length test cases

        [Test]
        public void TestStringLengthEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 16;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 3;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringLengthNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringLength_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 15;

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region string to number

        [Test]
        public void TestStringToNumberEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_empltyString.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringToNumberFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f8767579-f7c1-475f-980e-7cd6a42684c8");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 123521;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringToNumberFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f8767579-f7c1-475f-980e-7cd6a42684c8");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 12;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringToNumberInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringToNumberNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringToNumber_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("ca09bc3a-35c3-488f-a013-c05a5b7733c5");
            var watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("251210e5-2e04-4e81-b11d-39a8aff10887");
            var watch3 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("898ee89d-a934-4b43-a051-da3459be329a");
            var watch4 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("0afc0a8f-3d8a-4d7c-a2ec-d868cbb29b5f");

            double actual1 = watch1.GetValue(0).GetDoubleFromFSchemeValue();
            double actual2 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            double actual3 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            double actual4 = watch4.GetValue(0).GetDoubleFromFSchemeValue();

            double expected1 = 12;
            double expected2 = 12.3;
            double expected3 = 1000;
            double expected4 = 123456789;

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
            Assert.AreEqual(expected3, actual3);
            Assert.AreEqual(expected4, actual4);
        }

        #endregion

        #region string case test cases

        [Test]
        public void TestStringCaseEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringCaseFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "RAINY DAY";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringCaseFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "SUNNYDAY";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringCaseInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_invalidInput.dyn");

            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringCaseNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestStringCase_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch1 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");
            var watch2 = model.CurrentWorkspace.NodeFromWorkspace<Watch>("77a8c84b-b5bb-46f1-a550-7b3d5441c0a1");

            String actual1 = watch1.GetValue(0).GetStringFromFSchemeValue();
            String actual2 = watch2.GetValue(0).GetStringFromFSchemeValue();
            
            String expected1 = "RAINY DAY";
            String expected2 = "rainy day";

            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
        }

        #endregion

        #region to string test cases

        [Ignore]
        public void TestToStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_emptyString.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = @"""\n";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Ignore]
        public void TestToStringFileInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_fromFile.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "can you read this\n";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Ignore]
        public void TestToStringFunctionInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_fromFunction.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            FSharpList<FScheme.Value> resultList = watch.GetValue(0).GetListFromFSchemeValue();

            String actual1 = resultList[0].GetStringFromFSchemeValue();
            String actual2 = resultList[1].GetStringFromFSchemeValue();

            String expected1 = "1\n";
            String expected2 = "2\n";
            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
        }

        [Ignore]
        public void TestToStringNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(localDynamoStringTestFloder, "TestToString_normal.dyn");

            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("f72f6210-b32f-4dc4-9b2a-61f0144a0109");

            String actual = string.Empty;
            String expected = "123456\n";
            FSchemeInterop.Utils.Convert(watch.GetValue(0), ref actual);
            Assert.AreEqual(expected, actual);
        }

        #endregion

    }
}

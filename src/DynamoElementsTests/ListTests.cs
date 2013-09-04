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
    class ListTests : DynamoUnitTest
    {
        string listTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "list"); } }

        [Test]
        public void TestBuildSublistsEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_emptyInput.dyn");
            model.Open(testFilePath);

            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");
            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestBuildSublistsInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_invalidInput.dyn");
            model.Open(testFilePath);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestBuildSublistsNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(1, actualChild1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual(3, actualChild2[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestBuildSublistsStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual("b", actualChild1[0].getStringFromFSchemeValue());
            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual("d", actualChild2[0].getStringFromFSchemeValue());
        }

        [Test]
        public void TestConcatenateListsEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestConcatenateListsInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestConcatenateListsNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(9, actual.Length);
            Assert.AreEqual(10, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(20, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(20, actual[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, actual[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual("a", actual[5].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[6].getStringFromFSchemeValue());
            Assert.AreEqual("a", actual[7].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[8].getStringFromFSchemeValue());
        }

        [Test]
        public void TestDiagonalLeftListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestDiagonalLeftListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestDiagonalLeftListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild3 = actual[2].GetListFromFSchemeValue();

            Assert.AreEqual(3, actual.Length);

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(1, actualChild1[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual(2, actualChild2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, actualChild2[1].GetDoubleFromFSchemeValue());

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual(4, actualChild3[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(5, actualChild3[1].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestDiagonalLeftListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, actual.Length);
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild3 = actual[2].GetListFromFSchemeValue();

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual("a", actualChild1[0].getStringFromFSchemeValue());

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual("b", actualChild2[0].getStringFromFSchemeValue());
            Assert.AreEqual("a", actualChild2[1].getStringFromFSchemeValue());

            Assert.AreEqual(1, actualChild3.Length);
            Assert.AreEqual("b", actualChild3[0].getStringFromFSchemeValue());
        }

        [Test]
        public void TestDiagonalRightListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestDiagonalRightListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_invalidInput.dyn");
            model.Open(testFilePath);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestDiagonalRightListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, actual.Length);
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild3 = actual[2].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild4 = actual[3].GetListFromFSchemeValue();

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(5, actualChild1[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual(3, actualChild2[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(2, actualChild3.Length);
            Assert.AreEqual(1, actualChild3[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, actualChild3[1].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, actualChild4.Length);
            Assert.AreEqual(2, actualChild4[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestFirstOfListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFirstOfListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFirstOfListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 10;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestFirstOfListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            string actual = watch.GetValue(0).getStringFromFSchemeValue();
            string expected = "a";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 1;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestIsEmptyListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestIsEmptyListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 0;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestStringLengthNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 5;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringLengthStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            double actual = watch.GetValue(0).GetDoubleFromFSchemeValue();
            double expected = 4;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestPartitionStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            double expected = 0;
            Assert.AreEqual(expected, actual.Length);
        }

        [Test]
        public void TestPartitionStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestPartitionStringNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(3, childList1.Length);
            Assert.AreEqual(1, childList1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, childList1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, childList1[2].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList2.Length);
            Assert.AreEqual(4, childList2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(5, childList2[1].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestPartitionStringStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(3, childList1.Length);
            Assert.AreEqual("a", childList1[0].getStringFromFSchemeValue());
            Assert.AreEqual("b", childList1[1].getStringFromFSchemeValue());
            Assert.AreEqual("a", childList1[2].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(1, childList2.Length);
            Assert.AreEqual("b", childList2[0].getStringFromFSchemeValue());
        }

        [Test]
        public void TestFlattenEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            double expectedLength = 0;
            Assert.AreEqual(expectedLength, actual.Length);
        }

        [Test]
        public void TestFlattenInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFlattenNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(5, childList1.Length);
            Assert.AreEqual(0, childList1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, childList1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, childList1[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, childList1[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, childList1[4].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(4, childList2.Length);
            Assert.AreEqual("a", childList2[0].getStringFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].getStringFromFSchemeValue());
            Assert.AreEqual("c", childList2[2].getStringFromFSchemeValue());
            Assert.AreEqual("d", childList2[3].getStringFromFSchemeValue());

            Assert.AreEqual("a", actual[2].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[3].getStringFromFSchemeValue());
            Assert.AreEqual("c", actual[4].getStringFromFSchemeValue());
            Assert.AreEqual("d", actual[5].getStringFromFSchemeValue());
        }

        [Test]
        public void TestFlattenCompletlyEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            double expectedLength = 0;
            Assert.AreEqual(expectedLength, actual.Length);
        }

        [Test]
        public void TestFlattenCompletlyInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestFlattenCompletlyNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(13, actual.Length);

            Assert.AreEqual(0, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, actual[3].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, actual[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual("a", actual[5].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[6].getStringFromFSchemeValue());
            Assert.AreEqual("c", actual[7].getStringFromFSchemeValue());
            Assert.AreEqual("d", actual[8].getStringFromFSchemeValue());
            Assert.AreEqual("a", actual[9].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[10].getStringFromFSchemeValue());
            Assert.AreEqual("c", actual[11].getStringFromFSchemeValue());
            Assert.AreEqual("d", actual[12].getStringFromFSchemeValue());
        }

        [Test]
        public void TestRepeatEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRepeatNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(0, childList1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, childList1[1].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(1, childList2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, childList2[1].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(2, childList3[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, childList3[1].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList4 = actual[3].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(3, childList4[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, childList4[1].GetDoubleFromFSchemeValue());

            FSharpList<FScheme.Value> childList5 = actual[4].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(4, childList5[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, childList5[1].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestRepeatStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("a", childList1[0].getStringFromFSchemeValue());
            Assert.AreEqual("a", childList1[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("b", childList2[0].getStringFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("c", childList3[0].getStringFromFSchemeValue());
            Assert.AreEqual("c", childList3[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList4 = actual[3].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("d", childList4[0].getStringFromFSchemeValue());
            Assert.AreEqual("d", childList4[1].getStringFromFSchemeValue());
        }

        [Test]
        public void TestRestOfListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_emptyInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRestOfListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestRestOfListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_numberInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, actual.Length);
            Assert.AreEqual(20, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, actual[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(20, actual[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, actual[3].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TestRestOfListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_stringInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual("b", actual[0].getStringFromFSchemeValue());
            Assert.AreEqual("a", actual[1].getStringFromFSchemeValue());
            Assert.AreEqual("b", actual[2].getStringFromFSchemeValue());
        }

        [Test]
        public void TestTransposeEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_emptyInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, actual.Length);
        }

        [Test]
        public void TestTransposeInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_invalidInput.dyn");
            model.Open(testFilePath);
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void TestTransposeNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_normalInput.dyn");
            model.Open(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("789c1592-b64c-4a97-8f1a-8cef3d0cc2d0");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, actual.Length);

            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(1, childList1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("a", childList1[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(2, childList2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(3, childList3[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("a", childList3[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList4 = actual[3].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(4, childList4[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("b", childList4[1].getStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList5 = actual[4].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(5, childList5[0].GetDoubleFromFSchemeValue());
        }
    }
}

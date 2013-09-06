using System;
using System.IO;
using System.Threading;
using Dynamo.FSchemeInterop;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Dynamo.ViewModels;
using System.Reflection;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Dynamo.Tests;
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

        #region Sort Test Cases

        [Test]
        public void Sort_NumbersfFromDiffInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_NumbersfFromDiffInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // fourth and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(1, listWatchVal[4].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(-3.76498800959146, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal2[7].GetDoubleFromFSchemeValue());
        }


        [Test]
        public void Sort_SimpleNumbers()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_SimpleNumbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("de6bd134-55d1-4fb8-a605-1c486b5acb5f");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(2, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1.7, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("25ee495f-2d8e-4fa5-8180-6d0e45eb4675");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal2.Length);
            Assert.AreEqual(0, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal2[7].GetDoubleFromFSchemeValue());
        }


        [Test]
        public void Sort_StringsAndNumbers_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_Strings&Numbers.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void Sort_Strings()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_Strings.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("aa64651f-29cb-4008-b199-ec2f4ab3a1f7");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual("dddd", listWatchVal[0].getStringFromFSchemeValue());
            Assert.AreEqual("bbbbbbbbbbbbb", listWatchVal[4].getStringFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual("a", listWatchVal2[0].getStringFromFSchemeValue());
            Assert.AreEqual("rrrrrrrrr", listWatchVal2[4].getStringFromFSchemeValue());
        }
        #endregion

        #region SortBy Test Cases
        [Test]
        public void SortBy_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SortBy_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("3cf42e26-c178-4cc4-81a5-38b1c7867f5e");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(10.23, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(8, listWatchVal[4].GetDoubleFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("c966ac1d-5caa-4cfe-bb0c-f6db9e5697c4");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual(10.23, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(0.45, listWatchVal2[4].GetDoubleFromFSchemeValue());
        }
        #endregion

        #region Reverse Test Cases

        [Test]
        public void Reverse_ListWithOneNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_ListWithOneNumber.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(1, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Reverse_MixedList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_MixedList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(54.5, listWatchVal[0].GetDoubleFromFSchemeValue());

            // First element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].getStringFromFSchemeValue());

        }

        [Test]
        public void Reverse_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_NumberRange.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(-1, listWatchVal[7].GetDoubleFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listWatchVal1.Length);
            Assert.AreEqual(-1, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(6, listWatchVal1[7].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Reverse_UsingStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_UsingStringList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal.Length);
            Assert.AreEqual("Script", listWatchVal[0].getStringFromFSchemeValue());
            Assert.AreEqual("Dynamo", listWatchVal[3].getStringFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].getStringFromFSchemeValue());
            Assert.AreEqual("Script", listWatchVal1[3].getStringFromFSchemeValue());

        }

        [Test]
        public void Reverse_WithArrayInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_WithArrayInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);
            //Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
            //Assert.AreEqual("Dynamo", GetDoubleFromFSchemeValue(listWatchVal[3]));

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal1.Length);
            //Assert.AreEqual("Dynamo", getStringFromFSchemeValue(listWatchVal1[0]));
            //Assert.AreEqual("Script", getStringFromFSchemeValue(listWatchVal1[3]));

        }

        [Test]
        public void Reverse_WithSingletonInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_WithSingletonInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(10, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, listWatchVal[2].GetDoubleFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("2e8a3965-c908-4358-b7fc-331d0f3109ac");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listWatchVal1.Length);
            Assert.AreEqual(3, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal1[2].GetDoubleFromFSchemeValue());

        }

        #endregion

        #region Filter Tests

        [Test]
        public void Filter_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, Second and last element in the list before Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("a54127b5-decb-4750-aaf3-1b895be73984");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(11, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal[10].GetDoubleFromFSchemeValue());

            // First, Second and last element in the list after Filter
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal1.Length);
            Assert.AreEqual(6, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal1[4].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Filter_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, second and last element in the list before Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1327061f-b25d-4e91-9df7-a79850cb59e0");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(0, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(5, listWatchVal[5].GetDoubleFromFSchemeValue());

            // After filter there should not
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, listWatchVal1.Length);

        }

        [Test]
        public void Filter_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // First, second and last element in the first list after Filter
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("fce51e58-10e1-46b4-bc4c-756dfde00de7");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(6, listWatchVal[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listWatchVal[4].GetDoubleFromFSchemeValue());

            // First, second and last element in the second list after Filter
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("412526ae-d86c-491c-a587-d43598fa9c93");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal1.Length);
            Assert.AreEqual(0, listWatchVal1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listWatchVal1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, listWatchVal1[4].GetDoubleFromFSchemeValue());

            // First, second and last elements in the list after combining above two filtered list
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("dc27f671-4cef-480f-9ddc-218d61db7e52");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual(double.PositiveInfinity, listWatchVal2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, listWatchVal2[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2.5, listWatchVal2[4].GetDoubleFromFSchemeValue());


        }

        #endregion

    }
}

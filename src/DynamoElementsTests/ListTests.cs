using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

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
            Assert.AreEqual("b", actualChild1[0].GetStringFromFSchemeValue());
            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual("d", actualChild2[0].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", actual[5].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[6].GetStringFromFSchemeValue());
            Assert.AreEqual("a", actual[7].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[8].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", actualChild1[0].GetStringFromFSchemeValue());

            Assert.AreEqual(2, actualChild2.Length);
            Assert.AreEqual("b", actualChild2[0].GetStringFromFSchemeValue());
            Assert.AreEqual("a", actualChild2[1].GetStringFromFSchemeValue());

            Assert.AreEqual(1, actualChild3.Length);
            Assert.AreEqual("b", actualChild3[0].GetStringFromFSchemeValue());
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

            string actual = watch.GetValue(0).GetStringFromFSchemeValue();
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
            Assert.AreEqual("a", childList1[0].GetStringFromFSchemeValue());
            Assert.AreEqual("b", childList1[1].GetStringFromFSchemeValue());
            Assert.AreEqual("a", childList1[2].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(1, childList2.Length);
            Assert.AreEqual("b", childList2[0].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", childList2[0].GetStringFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].GetStringFromFSchemeValue());
            Assert.AreEqual("c", childList2[2].GetStringFromFSchemeValue());
            Assert.AreEqual("d", childList2[3].GetStringFromFSchemeValue());

            Assert.AreEqual("a", actual[2].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[3].GetStringFromFSchemeValue());
            Assert.AreEqual("c", actual[4].GetStringFromFSchemeValue());
            Assert.AreEqual("d", actual[5].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", actual[5].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[6].GetStringFromFSchemeValue());
            Assert.AreEqual("c", actual[7].GetStringFromFSchemeValue());
            Assert.AreEqual("d", actual[8].GetStringFromFSchemeValue());
            Assert.AreEqual("a", actual[9].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[10].GetStringFromFSchemeValue());
            Assert.AreEqual("c", actual[11].GetStringFromFSchemeValue());
            Assert.AreEqual("d", actual[12].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", childList1[0].GetStringFromFSchemeValue());
            Assert.AreEqual("a", childList1[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("b", childList2[0].GetStringFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("c", childList3[0].GetStringFromFSchemeValue());
            Assert.AreEqual("c", childList3[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList4 = actual[3].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual("d", childList4[0].GetStringFromFSchemeValue());
            Assert.AreEqual("d", childList4[1].GetStringFromFSchemeValue());
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
            Assert.AreEqual("b", actual[0].GetStringFromFSchemeValue());
            Assert.AreEqual("a", actual[1].GetStringFromFSchemeValue());
            Assert.AreEqual("b", actual[2].GetStringFromFSchemeValue());
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
            Assert.AreEqual("a", childList1[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(2, childList2[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("b", childList2[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(3, childList3[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("a", childList3[1].GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> childList4 = actual[3].GetListFromFSchemeValue();
            Assert.AreEqual(2, childList1.Length);
            Assert.AreEqual(4, childList4[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual("b", childList4[1].GetStringFromFSchemeValue());

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

            // First and last element in the list before sorting
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("aa64651f-29cb-4008-b199-ec2f4ab3a1f7");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual("dddd", listWatchVal[0].GetStringFromFSchemeValue());
            Assert.AreEqual("bbbbbbbbbbbbb", listWatchVal[4].GetStringFromFSchemeValue());

            // First and last element in the list after sorting
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d8ee9c7c-c456-4a38-a5d8-07eca624ebfe");
            FSharpList<FScheme.Value> listWatchVal2 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(5, listWatchVal2.Length);
            Assert.AreEqual("a", listWatchVal2[0].GetStringFromFSchemeValue());
            Assert.AreEqual("rrrrrrrrr", listWatchVal2[4].GetStringFromFSchemeValue());
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

            // First element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal.Length);
            Assert.AreEqual(54.5, listWatchVal[0].GetDoubleFromFSchemeValue());

            // First element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(6, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].GetStringFromFSchemeValue());

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

            // First and last element in the list before Reversing
            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("44505507-11d2-4792-b785-039304cadf89");
            FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal.Length);
            Assert.AreEqual("Script", listWatchVal[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Dynamo", listWatchVal[3].GetStringFromFSchemeValue());

            // First and last element in the list after Reversing
            watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("6dc62b9d-6045-4b68-a34c-2d5da999958b");
            FSharpList<FScheme.Value> listWatchVal1 = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(4, listWatchVal1.Length);
            Assert.AreEqual("Dynamo", listWatchVal1[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Script", listWatchVal1[3].GetStringFromFSchemeValue());

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

        #region LaceShortest test cases

        [Test]
        public void LaceShortest_Simple()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_Simple.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Element from the Reverse list
            var reverse = model.CurrentWorkspace.NodeFromWorkspace<Reverse>("c3d629f7-76a0-40bc-bf39-da45d8b8ea7a");
            FSharpList<FScheme.Value> listReverseValue = reverse.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, listReverseValue.Length);
            Assert.AreEqual(4, listReverseValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listReverseValue[1].GetDoubleFromFSchemeValue());

            // Elements from the Combine list
            var combine = model.CurrentWorkspace.NodeFromWorkspace<Combine>("cc23b43e-3709-4ed1-bedb-f903e4ea7d75");
            FSharpList<FScheme.Value> listCombineValue = combine.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, listCombineValue.Length);
            Assert.AreEqual(-0.5, listCombineValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(-1, listCombineValue[1].GetDoubleFromFSchemeValue());

            // Elements from first LaceShortest list
            var shortest = model.CurrentWorkspace.NodeFromWorkspace<LaceShortest>("10005d3c-3bbf-4690-b658-37b11c8402b1");
            FSharpList<FScheme.Value> listShotestValue = shortest.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, listShotestValue.Length);
            Assert.AreEqual(2, listShotestValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, listShotestValue[1].GetDoubleFromFSchemeValue());

            // Elements from second LaceShortest list
            var shortest1 = model.CurrentWorkspace.NodeFromWorkspace<LaceShortest>("ce7bf465-0f93-4e5a-8bc9-9960cd077f25");
            FSharpList<FScheme.Value> listShotestValue1 = shortest1.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(2, listShotestValue1.Length);
            Assert.AreEqual(-4, listShotestValue1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(-4, listShotestValue1[1].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void LaceShortest_NegativeInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_NegativeInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

        }

        [Test]
        public void LaceShortest_StringInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_StringInput.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Element from the Reverse list
            var reverse = model.CurrentWorkspace.NodeFromWorkspace<ConcatStrings>("1c4c75ff-735d-4431-9df3-2b187c469b3a");
            string actual = reverse.GetValue(0).GetStringFromFSchemeValue();
            string expected = "1Design";
            Assert.AreEqual(expected, actual);

            // Elements from first LaceShortest list
            var shortest = model.CurrentWorkspace.NodeFromWorkspace<LaceShortest>("10005d3c-3bbf-4690-b658-37b11c8402b1");
            FSharpList<FScheme.Value> listShotestValue = shortest.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listShotestValue.Length);
            Assert.AreEqual(1, listShotestValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listShotestValue[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, listShotestValue[2].GetDoubleFromFSchemeValue());

            // Elements from second LaceShortest list
            var shortest1 = model.CurrentWorkspace.NodeFromWorkspace<LaceShortest>("c19f09a1-6132-4c9c-8f37-5f138e1a3067");
            FSharpList<FScheme.Value> listShotestValue1 = shortest1.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listShotestValue1.Length);
            Assert.AreEqual("Dynamo", listShotestValue1[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Design", listShotestValue1[1].GetStringFromFSchemeValue());
            Assert.AreEqual("Script", listShotestValue1[2].GetStringFromFSchemeValue());

        }

        #endregion

        #region LaceLongest test cases

        [Test]
        public void LaceLongest_Simple()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_Simple.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("5da40769-ffc8-408b-94bb-8c5dff31132e");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild3 = actual[2].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild4 = actual[3].GetListFromFSchemeValue();

            Assert.AreEqual(4, actual.Length);

            Assert.AreEqual(1, actualChild1.Length);
            Assert.AreEqual(2, actualChild1[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, actualChild2.Length);
            Assert.AreEqual(8, actualChild2[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, actualChild3.Length);
            Assert.AreEqual(14, actualChild3[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, actualChild4.Length);
            Assert.AreEqual(19, actualChild4[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void LaceLongest_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_Negative.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

        }

        [Test]
        public void LaceLongest_ListWith10000Element()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_ListWith10000Element.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<LaceLongest>("25daa241-d8a4-4e74-aec1-6068358babf7");
            FSharpList<FScheme.Value> listWatchValue = watch.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(10000, listWatchValue.Length);
            Assert.AreEqual(2001, listWatchValue[1000].GetDoubleFromFSchemeValue());

        }

        #endregion

        #region FilterOut test cases

        [Test]
        public void FilterOut_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Element from the Number node
            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("b6571eb6-b1c2-4874-8749-b783176dc039");
            FSharpList<FScheme.Value> listAllNumbers = numberRange.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(10, listAllNumbers.Length);
            Assert.AreEqual(1, listAllNumbers[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listAllNumbers[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, listAllNumbers[2].GetDoubleFromFSchemeValue());

            // Elements from first FilterOut list
            var filterOut = model.CurrentWorkspace.NodeFromWorkspace<FilterOut>("53ec97e2-d860-4fdc-8ea5-2288bf39bcfc");
            FSharpList<FScheme.Value> listFilteredValue = filterOut.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listFilteredValue.Length);
            Assert.AreEqual(3, listFilteredValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, listFilteredValue[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listFilteredValue[7].GetDoubleFromFSchemeValue());

            // Elements from second FilterOut list
            var filterOut1 = model.CurrentWorkspace.NodeFromWorkspace<FilterOut>("0af3f566-1b05-4578-9fb0-297ca98d6d8c");
            FSharpList<FScheme.Value> listFilteredValue1 = filterOut1.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, listFilteredValue1.Length);
            Assert.AreEqual(1, listFilteredValue1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listFilteredValue1[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, listFilteredValue1[2].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void FilterOut_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Element from the Number node
            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("b6571eb6-b1c2-4874-8749-b783176dc039");
            FSharpList<FScheme.Value> listAllNumbers = numberRange.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(10, listAllNumbers.Length);
            Assert.AreEqual(1, listAllNumbers[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(2, listAllNumbers[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(3, listAllNumbers[2].GetDoubleFromFSchemeValue());

            // Elements from FilterOut list
            var filterOut = model.CurrentWorkspace.NodeFromWorkspace<FilterOut>("53ec97e2-d860-4fdc-8ea5-2288bf39bcfc");
            FSharpList<FScheme.Value> listFilteredValue = filterOut.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(8, listFilteredValue.Length);
            Assert.AreEqual(3, listFilteredValue[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, listFilteredValue[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, listFilteredValue[7].GetDoubleFromFSchemeValue());

            // Elements from Take from List
            var takeFromList = model.CurrentWorkspace.NodeFromWorkspace<TakeList>("6921b2ef-fc5c-44b4-992f-9421c267d9ef");
            FSharpList<FScheme.Value> takenFromList = takeFromList.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(3, takenFromList.Length);
            Assert.AreEqual(3, takenFromList[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(4, takenFromList[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(5, takenFromList[2].GetDoubleFromFSchemeValue());

            // Elements from Drop from List 
            var dropFromList = model.CurrentWorkspace.NodeFromWorkspace<DropList>("57a41c41-fa71-41dd-aa25-ca2156f2ba0b");
            FSharpList<FScheme.Value> droppedFromList = dropFromList.GetValue(0).GetListFromFSchemeValue();
            Assert.AreEqual(0, droppedFromList.Length); // As there where only three element in the input list so after droppping the list should be empty.

        }

        [Test]
        public void FilterOut_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

        }


        #endregion

        #region NumberRange test cases
        
        [Test]
        public void NumberRange_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(51, actual.Length);
            Assert.AreEqual(0, actual[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(50, actual[50].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void NumberRange_LargeNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LargeNumber.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var watch = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            FSharpList<FScheme.Value> actual = watch.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(1000001, actual.Length);

            Assert.AreEqual(500, actual[500].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1000000, actual[1000000].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void NumberRange_LacingShortest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingShortest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            FSharpList<FScheme.Value> actual = numberRange.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();

            Assert.AreEqual(1, actual.Length);

            Assert.AreEqual(10, actualChild1.Length);
            Assert.AreEqual(1, actualChild1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(10, actualChild1[9].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void NumberRange_LacingLongest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingLongest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            FSharpList<FScheme.Value> actual = numberRange.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, actual.Length);

            Assert.AreEqual(10, actualChild1.Length);
            Assert.AreEqual(10, actualChild1[9].GetDoubleFromFSchemeValue());

            Assert.AreEqual(5, actualChild2.Length);
            Assert.AreEqual(10, actualChild2[4].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void NumberRange_LacingCrossProduct()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingCrossProduct.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            FSharpList<FScheme.Value> actual = numberRange.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild3 = actual[2].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> actualChild4 = actual[3].GetListFromFSchemeValue();

            Assert.AreEqual(4, actual.Length);

            Assert.AreEqual(10, actualChild1.Length);
            Assert.AreEqual(10, actualChild1[9].GetDoubleFromFSchemeValue());

            Assert.AreEqual(5, actualChild2.Length);
            Assert.AreEqual(9, actualChild2[4].GetDoubleFromFSchemeValue());

            Assert.AreEqual(9, actualChild3.Length);
            Assert.AreEqual(10, actualChild3[8].GetDoubleFromFSchemeValue());

            Assert.AreEqual(5, actualChild4.Length);
            Assert.AreEqual(10, actualChild4[4].GetDoubleFromFSchemeValue());

        }
        
        #endregion

        #region ListMinimum test cases

        [Test]
        public void ListMinimum_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ListMinimum_NumberRange.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var listMin = model.CurrentWorkspace.NodeFromWorkspace<ListMin>("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6");

            Assert.AreEqual(-1, listMin.GetValue(0).GetDoubleFromFSchemeValue());

        }

        [Test]
        public void ListMinimum_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ListMinimum_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var listMin = model.CurrentWorkspace.NodeFromWorkspace<ListMin>("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6");

            Assert.AreEqual(5, listMin.GetValue(0).GetDoubleFromFSchemeValue());

        }

        #endregion

        #region AddToList test cases

        [Test]
        public void AddToList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var addToList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.List>("31d0eb4e-8657-4eb1-a852-5e9b766eddd7");

            FSharpList<FScheme.Value> actual = addToList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList = actual[2].GetListFromFSchemeValue();

            Assert.AreEqual(6, actual.Length);
            Assert.AreEqual("Design", actual[0].GetStringFromFSchemeValue());
            Assert.AreEqual(10, actual[5].GetDoubleFromFSchemeValue());

            Assert.AreEqual(4, childList.Length);
            Assert.AreEqual(-10, childList[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void AddToList_EmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_EmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var addToList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.List>("1976caa7-d45e-4a44-9faf-345d98337bbb");

            FSharpList<FScheme.Value> actual = addToList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList = actual[0].GetListFromFSchemeValue();

            Assert.AreEqual(1, actual.Length);

            Assert.AreEqual(2, childList.Length);
            Assert.IsEmpty(childList[0].GetStringFromFSchemeValue());
            Assert.AreEqual(0, childList[1].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void AddToList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var addToList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.List>("cfdfc020-05d0-4442-96df-8d97aad9c38c");

            FSharpList<FScheme.Value> actual = addToList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList1 = actual[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList2 = actual[1].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList3 = actual[2].GetListFromFSchemeValue();

            Assert.AreEqual(3, actual.Length);

            Assert.AreEqual(1, childList1.Length);
            Assert.AreEqual(3, childList1[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, childList2.Length);
            Assert.AreEqual(6, childList2[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, childList3.Length);
            Assert.AreEqual(9, childList3[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void AddToList_GeometryToList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_GeometryToList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var addToList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.List>("31d0eb4e-8657-4eb1-a852-5e9b766eddd7");

            FSharpList<FScheme.Value> actual = addToList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList1 = actual[2].GetListFromFSchemeValue();

            Assert.AreEqual(6, actual.Length);

            Assert.AreEqual(4, childList1.Length);
            Assert.AreEqual(-10, childList1[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void AddToList_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_Negative.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

        }

        #endregion

        #region SplitList test cases

        [Test]
        public void SplitList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var splitList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DeCons>("8226a43b-fd5e-45f6-a5f7-32815c12084a");

            Assert.AreEqual("Dynamo", splitList.GetValue(0).GetStringFromFSchemeValue());

            FSharpList<FScheme.Value> secondOutput = splitList.GetValue(1).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> childList = secondOutput[0].GetListFromFSchemeValue();

            Assert.AreEqual(1, secondOutput.Length);

            Assert.AreEqual(2, childList.Length);
            Assert.AreEqual(0, childList[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void SplitList_FirstElementAsList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_FirstElementAsList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var splitList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DeCons>("8226a43b-fd5e-45f6-a5f7-32815c12084a");

            FSharpList<FScheme.Value> firstOutput = splitList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> secondOutput = splitList.GetValue(1).GetListFromFSchemeValue();

            Assert.AreEqual(2, firstOutput.Length);
            Assert.AreEqual(0, firstOutput[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, secondOutput.Length);
            Assert.AreEqual("Dynamo", secondOutput[0].GetStringFromFSchemeValue());

        }

        [Test]
        public void SplitList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var splitList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DeCons>("8226a43b-fd5e-45f6-a5f7-32815c12084a");

            FSharpList<FScheme.Value> firstOutput = splitList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> secondOutput = splitList.GetValue(1).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = secondOutput[0].GetListFromFSchemeValue();

            Assert.AreEqual(1, firstOutput.Length);
            Assert.AreEqual(3, firstOutput[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(2, secondOutput.Length);

            Assert.AreEqual(1, child.Length);
            Assert.AreEqual(6, child[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void SplitList_ComplexAnotherExample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_ComplexAnotherExample.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var splitList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DeCons>("66e94123-deaf-4bc8-8c5f-b3bc0996a57e");

            FSharpList<FScheme.Value> firstOutput = splitList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> secondOutput = splitList.GetValue(1).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = secondOutput[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child1 = secondOutput[1].GetListFromFSchemeValue();

            Assert.AreEqual(12, firstOutput.Length);
            Assert.AreEqual("x", firstOutput[0].GetStringFromFSchemeValue());
            Assert.AreEqual("z", firstOutput[11].GetStringFromFSchemeValue());

            Assert.AreEqual(2, secondOutput.Length);

            Assert.AreEqual(12, child.Length);
            Assert.AreEqual(19.35, child[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(12, child1.Length);
            Assert.AreEqual(32.85, child1[0].GetDoubleFromFSchemeValue());

        }
        #endregion

        #region TakeFromList test cases
        
        [Test]
        public void TakeFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var takeFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeList>("14cb6593-24d8-4ffc-8ee5-9f4247449fc2");

            FSharpList<FScheme.Value> firstOutput = takeFromList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = firstOutput[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child1 = firstOutput[4].GetListFromFSchemeValue();

            Assert.AreEqual(5, firstOutput.Length);

            Assert.AreEqual(1, child.Length);
            Assert.AreEqual(3, child[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, child1.Length);
            Assert.AreEqual(15, child1[0].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void TakeFromList_WithStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_WithStringList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var takeFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeList>("14cb6593-24d8-4ffc-8ee5-9f4247449fc2");

            FSharpList<FScheme.Value> firstOutput = takeFromList.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(4, firstOutput.Length);

            Assert.AreEqual("Test", firstOutput[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Take", firstOutput[1].GetStringFromFSchemeValue());
            Assert.AreEqual("From", firstOutput[2].GetStringFromFSchemeValue());
            Assert.AreEqual("List", firstOutput[3].GetStringFromFSchemeValue());

        }

        [Test]
        public void TakeFromList_NegativeIntValue()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_NegativeIntValue.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var takeFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeList>("14cb6593-24d8-4ffc-8ee5-9f4247449fc2");

            FSharpList<FScheme.Value> firstOutput = takeFromList.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(1, firstOutput.Length);

            Assert.AreEqual("List", firstOutput[0].GetStringFromFSchemeValue());

        }

        [Test]
        public void TakeFromList_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });

        }

        [Test]
        public void TakeFromList_AmtAsRangeExpn()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_AmtAsRangeExpn.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        #endregion

        #region DropFromList test cases
        [Test]
        public void DropFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropFromList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            
            // run expression
            dynSettings.Controller.RunExpression(null);

            var dropFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DropList>("e2d27010-b8fc-4eb8-8703-63bab5ce6e85");
            FSharpList<FScheme.Value> output = dropFromList.GetValue(0).GetListFromFSchemeValue();

            var dropFromList1 = model.CurrentWorkspace.NodeFromWorkspace<DropList>("097e0b4b-4cbb-43b1-a21c-77a619ad1050");
            FSharpList<FScheme.Value> secondOutput = dropFromList1.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(6, output.Length);
            Assert.AreEqual(0, output[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(6, secondOutput.Length);
            Assert.AreEqual(5, secondOutput[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void DropFromList_InputEmptyList()
        {

            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropFromList_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        #endregion

        #region ShiftListIndeces test cases

        [Test]
        public void ShiftListIndeces_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var shiftListIndeces = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.ShiftList>("7f6cbd60-b9fb-4b16-81d3-4fab26790446");
            FSharpList<FScheme.Value> output = shiftListIndeces.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(10, output.Length);
            Assert.AreEqual(7, output[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, output[4].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void ShiftListIndeces_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(21, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var shiftListIndeces = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.ShiftList>("492db019-4807-4810-8919-10b94e8ca083");
            FSharpList<FScheme.Value> output = shiftListIndeces.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = output[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child1 = output[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, output.Length);

            Assert.AreEqual(12, child.Length);
            Assert.AreEqual(32.85, child[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(50.2275, child[11].GetDoubleFromFSchemeValue());

            Assert.AreEqual(12, child1.Length);
            Assert.AreEqual(19.35, child1[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(108.7275, child1[11].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void ShiftListIndeces_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var shiftListIndeces = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.ShiftList>("7f6cbd60-b9fb-4b16-81d3-4fab26790446");
            FSharpList<FScheme.Value> output = shiftListIndeces.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, output.Length);

        }

        [Test]
        public void ShiftListIndeces_InputStringAsAmt()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_InputStringAsAmt.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void ShiftListIndeces_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }
        #endregion

        #region GetFromList test cases
        
        [Test]
        public void GetFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var getFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.GetFromList>("332093dc-4551-4c82-9f6b-061c7945211b");
            FSharpList<FScheme.Value> output = getFromList.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(1, output.Length);
            Assert.AreEqual(9, output[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void GetFromList_WithStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_WithStringList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var getFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.GetFromList>("58d35bfa-4435-44f0-a322-c6f7350f0220");
            FSharpList<FScheme.Value> output = getFromList.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(2, output.Length);
            Assert.AreEqual("Get", output[0].GetStringFromFSchemeValue());
            Assert.AreEqual("From ", output[1].GetStringFromFSchemeValue());
        }

        [Test]
        public void GetFromList_AmtAsRangeExpn()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_AmtAsRangeExpn.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var getFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.GetFromList>("d2f1c900-99ce-40a5-ae4d-bbac1fe96cfd");
            FSharpList<FScheme.Value> output = getFromList.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = output[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child1 = output[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, output.Length);

            Assert.AreEqual(1, child.Length);
            Assert.AreEqual(6, child[0].GetDoubleFromFSchemeValue());

            Assert.AreEqual(1, child1.Length);
            Assert.AreEqual(9, child1[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void GetFromList_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void GetFromList_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_Negative.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }
        [Test]
        public void GetFromList_NegativeIntValue()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_NegativeIntValue.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

            // This test case need to change because -ve index is valid in DesignScript context.
        }

        #endregion

        #region TakeEveryNth test case

        [Test]
        public void TakeEveryNth_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeEveryNth>("b18e5ac3-5732-4c78-9a3b-56b375c9beee");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(4, output.Length);

            Assert.AreEqual(10, output[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(13, output[1].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TakeEveryNth_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeEveryNth>("b18e5ac3-5732-4c78-9a3b-56b375c9beee");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(1, output.Length);

            Assert.AreEqual(2.3, output[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void TakeEveryNth_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeEveryNth>("b18e5ac3-5732-4c78-9a3b-56b375c9beee");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, output.Length);

        }

        [Test]
        public void TakeEveryNth_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }
        #endregion

        #region DropEveryNth

        [Test]
        public void DropEveryNth_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveEveryNth>("96a1ca07-83eb-4459-981e-7daed6d1d4b3");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(4, output.Length);

            Assert.AreEqual(6, output[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(7, output[1].GetDoubleFromFSchemeValue());
            Assert.AreEqual(8, output[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual(9, output[3].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void DropEveryNth_ComplexTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_ComplexTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(19, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveEveryNth>("4bd0ced4-29ee-4f4e-95af-d0573e04731a");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child = output[0].GetListFromFSchemeValue();
            FSharpList<FScheme.Value> child1 = output[1].GetListFromFSchemeValue();

            Assert.AreEqual(2, output.Length);

            Assert.AreEqual(12, child.Length);
            Assert.AreEqual("x", child[0].GetStringFromFSchemeValue());

            Assert.AreEqual(12, child1.Length);
            Assert.AreEqual(32.85, child1[0].GetDoubleFromFSchemeValue());
        }

        [Test]
        public void DropEveryNth_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_InputEmptyList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveEveryNth>("a0304232-ad3a-4518-92ff-4b8893297ce4");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, output.Length);

        }

        [Test]
        public void DropEveryNth_InputStringForNth()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_InputStringForNth.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }
        #endregion

        #region RemoveFromList test cases
        
        [Test]
        public void RemoveFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveFromList>("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(5, output.Length);

            Assert.AreEqual("testRemoveFromList", output[0].GetStringFromFSchemeValue());
            Assert.AreEqual("Dynamo", output[1].GetStringFromFSchemeValue());
            Assert.AreEqual(10.689, output[2].GetDoubleFromFSchemeValue());
            Assert.AreEqual("Design", output[3].GetStringFromFSchemeValue());
            Assert.AreEqual("Script", output[4].GetStringFromFSchemeValue());

        }

        [Test]
        public void RemoveFromList_StringAsList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_StringAsList.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void RemoveFromList_StringAsIndex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_StringAsIndex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            // run expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void RemoveFromList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_Complex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveFromList>("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(2, output.Length);

            Assert.AreEqual(7, output[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(8, output[1].GetDoubleFromFSchemeValue());

        }

        [Test]
        public void RemoveFromList_RangeExpnAsIndex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_RangeExpnAsIndex.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveFromList>("dc581841-0221-4bc3-9010-3d2f0081a169");
            FSharpList<FScheme.Value> output = takeEveryNth.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(5, output.Length);
            Assert.AreEqual(10, output[0].GetDoubleFromFSchemeValue());
            Assert.AreEqual(1, output[1].GetDoubleFromFSchemeValue());

            var takeEveryNth1 = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveFromList>("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6");
            FSharpList<FScheme.Value> output1 = takeEveryNth1.GetValue(0).GetListFromFSchemeValue();

            Assert.AreEqual(0, output1.Length);

        }
        #endregion
    }
}

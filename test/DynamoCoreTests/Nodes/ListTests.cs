using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Models;
using System.Collections.Generic;


namespace Dynamo.Tests
{
    class ListTests : DSEvaluationUnitTest
    {
        string listTestFolder { get { return Path.Combine(GetTestDirectory(), "core", "list"); } }

        #region Test Build Sublist  

        [Test]
        public void TestBuildSublistsEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_emptyInput.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("9375d612-cccb-4ba2-96e1-4d1497c6234b", new int[] { });
        }

        [Test]
        public void TestBuildSublistsInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_invalidInput.dyn");
            RunModel(testFilePath);

        }

        [Test]
        public void TestBuildSublistsNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("9240cdc9-5bbf-4579-930c-ef742a91d798", new int[][] { new int[] { 1 }, new int[] { 3 } });

        }

        [Test]
        public void TestBuildSublistsStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_stringInput.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("9240cdc9-5bbf-4579-930c-ef742a91d798", new string[][] { new string[] { "b" }, new string[] { "d" } });

        }

        #endregion

        #region Test Concatenate List  

        [Test]
        public void TestConcatenateListsEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("760c9f00-e12c-4db9-bbdf-a19562efdd09", new int[]{});

        }

        [Test]
        public void TestConcatenateListsInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestConcatenateListsNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_normalInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("364b303f-8f0b-4964-b333-e937299c8352", new object[] { 10, 20, 10, 20, 10, "a", "b", "a", "b" });

        }

        #endregion

        #region Test DiagonalLeftList  

        [Test]
        public void TestDiagonalLeftListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("a54ad1f8-9b02-4ebf-9d4e-a53608906145", new int[]{});
        }

        [Test]
        public void TestDiagonalLeftListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestDiagonalLeftListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("87345663-8421-46f0-acd2-051e4ec5ff88", new int[][]{new int[]{1}, new int[]{2,3}, new int[]{4,5}});
        }

        [Test]
        public void TestDiagonalLeftListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("87345663-8421-46f0-acd2-051e4ec5ff88", new string[][] { new string[] { "a" }, new string[] { "b", "a" }, new string[] { "b" } });

        }

        #endregion

        #region Test DiagonalRightList  

        [Test]
        public void TestDiagonalRightListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("49f4ebe5-fd49-462b-9896-fe1244f66486", new int[]{});

        }

        [Test]
        public void TestDiagonalRightListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_invalidInput.dyn");
            RunModel(testFilePath);

        }

        [Test]
        public void TestDiagonalRightListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("e84bf89e-e7a0-427c-adae-adcd61646e4e", new int[][] { new int[] { 5 }, new int[] { 3 }, new int[] { 1, 4 }, new int[] { 2 } });

        }

        #endregion

        #region Test First Of List  

        [Test]
        public void TestFirstOfListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_emptyInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestFirstOfListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestFirstOfListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("879fda8f-b9f4-453b-bf4d-faeb76ce5ffc", 10);
        }

        [Test]
        public void TestFirstOfListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("879fda8f-b9f4-453b-bf4d-faeb76ce5ffc", "a");
        }

        #endregion

        #region Test Empty List  

        [Test]
        public void TestIsEmptyListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 1);

        }

        [Test]
        public void TestIsEmptyListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestIsEmptyListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 0);

        }

        [Test]
        public void TestIsEmptyListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 0);

        }

        #endregion

        #region Test List Length  

        [Test]
        public void TestStringLengthEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("8ab87f7a-2577-46b9-bee6-512b1678b028", new int[] { });

        }

        [Test]
        public void TestStringLengthInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestStringLengthNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("18473048-4a5f-4b23-8578-d9b8c0f32c0f", 5);

        }

        [Test]
        public void TestStringLengthStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testListLength_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("18473048-4a5f-4b23-8578-d9b8c0f32c0f", 4);

        }

        #endregion

        #region Test Partition List  

        [Test]
        public void TestPartitionStringEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("6cad28c0-605a-4b58-84a2-87939f81f61e", new int[] { });

        }

        [Test]
        public void TestPartitionStringInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestPartitionStringNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("a3cdc54a-5965-47ea-b294-f893b1b64ae2", new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5 } });

        }

        [Test]
        public void TestPartitionStringStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("a3cdc54a-5965-47ea-b294-f893b1b64ae2", new string[][] { new string[] { "a", "b", "a" }, new string[] { "b" } });

        }

        #endregion

        #region Test Flatten

        [Test]
        public void TestFlattenEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("4cc4e5f0-4338-43bb-911e-d7c10ea2b53c", new int[] { });

        }

        [Test]
        public void TestFlattenInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestFlattenNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlatten_normalInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("6e46f30d-2214-4ff7-a666-0516d2af7c64", new object[] 
            { 
                new object[] { 0, 1, 2, 3, 4 }, new object[] { "a", "b", "c", "d" }, "a", "b", "c", "d" 
            });
        }

        #endregion

        #region Test FlattenCompletely  

        [Test]
        public void TestFlattenCompletlyEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_emptyInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("641db696-5626-4af0-b07e-6335c6dc4bc9", new int[] { });
        }

        [Test]
        public void TestFlattenCompletlyInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestFlattenCompletlyNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_normalInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("76609452-9c1d-4d71-9223-bb13c323f3a6", new object[] { 0, 1, 2, 3, 4, "a", "b", "c", "d", "a", "b", "c", "d" });

        }

        #endregion

        #region Test Repeat  

        [Test]
        public void TestRepeatEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_emptyInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestRepeatNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_numberInput.dyn");
            RunModel(testFilePath);
            var a1 = new int[] { 0, 0 };
            var a2 = new int[] { 1, 1 };
            var a3 = new int[] { 2, 2 };
            var a4 = new int[] { 3, 3 };
            var a5 = new int[] { 4, 4 };
            AssertPreviewValue("72dddbc8-0a6b-431d-a185-8ec62a8b79dd", new int[][] { a1, a2, a3, a4, a5 });

        }

        [Test]
        public void TestRepeatStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRepeat_stringInput.dyn");
            RunModel(testFilePath);

            var a1 = new string[] { "a", "a" };
            var a2 = new string[] { "b", "b" };
            var a3 = new string[] { "c", "c" };
            var a4 = new string[] { "d", "d" };
            AssertPreviewValue("72dddbc8-0a6b-431d-a185-8ec62a8b79dd", new string[][] { a1, a2, a3, a4 });

        }

        #endregion

        #region Test RestOfList  

        [Test]
        public void TestRestOfListEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_emptyInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestRestOfListInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestRestOfListNumberInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_numberInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("3d3e481b-16ef-4837-b94e-7922f9e42029", new int[] { 20, 10, 20, 10 });

        }

        [Test]
        public void TestRestOfListStringInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testRestOfList_stringInput.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("3d3e481b-16ef-4837-b94e-7922f9e42029", new string[] { "b", "a", "b" });

        }

        #endregion

        #region Test Transpose List  
        [Test]
        public void TestTransposeEmptyInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_emptyInput.dyn");
            RunModel(testFilePath);
            dynSettings.Controller.RunExpression(null);
            var watch = model.CurrentWorkspace.NodeFromWorkspace<DSFunction>("df181bd7-3f1f-4195-93af-c0b846f6c8ce");

            var actual = watch.GetValue(0).GetElements();
            Assert.AreEqual(0, actual.Count);
        }

        [Test]
        public void TestTransposeInvalidInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_invalidInput.dyn");
            RunModel(testFilePath);
        }

        [Test]
        public void TestTransposeNormalInput()
        {
            DynamoModel model = Controller.DynamoModel;
            string testFilePath = Path.Combine(listTestFolder, "testTransposeList_normalInput.dyn");
            RunModel(testFilePath);

            var a1 = new object[] { 1, "a" };
            var a2 = new object[] { 2, "b" };
            var a3 = new object[] { 3, "a" };
            var a4 = new object[] { 4, "b" };
            var a5 = new object[] { 5, "a" };
            AssertPreviewValue("e639bc66-6dec-4a0a-bae2-9bac7dab59dc", new object[][] { a1, a2, a3, a4, a5 });

        }
        #endregion

        #region Sort Test Cases  

        [Test]
        public void Sort_NumbersfFromDiffInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_NumbersfFromDiffInput.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);


            // fourth and last element in the list before sorting
            Dictionary<int, object> validationData = new Dictionary<int,object>()
            {
                {4,1},
                {7,0},
            };
            SelectivelyAssertPreviewValues("de6bd134-55d1-4fb8-a605-1c486b5acb5f", validationData);

            // First and last element in the list after sorting
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,-3.76498800959146},
                {7,1},
            };
            SelectivelyAssertPreviewValues("dd7d0508-d5d4-4990-a6f7-6cfc02b0f2f4", validationData1);

        }

        [Test]
        public void Sort_SimpleNumbers()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_SimpleNumbers.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);

            // First and last element in the list before sorting
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,2},
                {7,1.7},
            };
            SelectivelyAssertPreviewValues("de6bd134-55d1-4fb8-a605-1c486b5acb5f", validationData);

            // First and last element in the list after sorting
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,0},
                {7,10},
            };
            SelectivelyAssertPreviewValues("dd7d0508-d5d4-4990-a6f7-6cfc02b0f2f4", validationData1);

        }


        [Test]
        public void Sort_StringsAndNumbers_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_Strings&Numbers.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);

        }

        [Test]
        public void Sort_Strings()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Sort_Strings.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);

            // First and last element in the list before sorting
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,"dddd"},
                {4,"bbbbbbbbbbbbb"},
            };
            SelectivelyAssertPreviewValues("aa64651f-29cb-4008-b199-ec2f4ab3a1f7", validationData);

            // First and last element in the list after sorting
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,"a"},
                {4,"rrrrrrrrr"},
            };
            SelectivelyAssertPreviewValues("14fae78b-b009-4503-afe9-b714e08db1ec", validationData1);

        }
        #endregion

        #region SortBy Test Cases  
        [Test]
        public void SortBy_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SortBy_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);


            // First and last element in the list before sorting
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,10.23},
                {4,8},
            };
            SelectivelyAssertPreviewValues("3cf42e26-c178-4cc4-81a5-38b1c7867f5e", validationData);

            // First and last element in the list after sorting
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,10.23},
                {4,0.45},
            };
            SelectivelyAssertPreviewValues("9e2c84e6-b9b8-4bdf-b82e-868b2436b865", validationData1);

        }
        #endregion

        #region Reverse Test Cases  

        [Test]
        public void Reverse_ListWithOneNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_ListWithOneNumber.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);


            // First element in the list before Reversing
            AssertPreviewValue("cd36fac7-d9eb-47ea-a73d-ad1bb4bbe54a", new int[] { 0 });
        }

        [Test]
        public void Reverse_MixedList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_MixedList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);


            // First element in the list before Reversing
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,"Dynamo"},
            };
            SelectivelyAssertPreviewValues("6dc62b9d-6045-4b68-a34c-2d5da999958b", validationData);

            // First element in the list after Reversing
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,54.5},
            };
            SelectivelyAssertPreviewValues("cd36fac7-d9eb-47ea-a73d-ad1bb4bbe54a", validationData1);

        }

        [Test]
        public void Reverse_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_NumberRange.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);


            // First and Last element in the list before Reversing
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,-1},
                {7,8},

            };
            SelectivelyAssertPreviewValues("6dc62b9d-6045-4b68-a34c-2d5da999958b", validationData);

            // First and last element in the list after Reversing
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,6},
                {7,-1},
            };
            SelectivelyAssertPreviewValues("cd36fac7-d9eb-47ea-a73d-ad1bb4bbe54a", validationData1);

        }

        [Test]
        public void Reverse_UsingStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_UsingStringList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);


            // First and Last element in the list before Reversing
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,"Dynamo"},
                {3,"Script"},

            };
            SelectivelyAssertPreviewValues("6dc62b9d-6045-4b68-a34c-2d5da999958b", validationData);

            // First and last element in the list after Reversing
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,"Script"},
                {3,"Dynamo"},
            };
            SelectivelyAssertPreviewValues("cd36fac7-d9eb-47ea-a73d-ad1bb4bbe54a", validationData1);

        }

        [Test]
        public void Reverse_WithArrayInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_WithArrayInput.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);

            //// First and last element in the list before Reversing
            //var watch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
            //FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
            //Assert.AreEqual(3, listWatchVal.Length);
            ////Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
            ////Assert.AreEqual("Dynamo", GetDoubleFromFSchemeValue(listWatchVal[3]));

            //// First and last element in the list after Reversing
            //var reverse = model.CurrentWorkspace.NodeFromWorkspace<Reverse>("18352d04-273b-4821-8819-bd7676dc4374");
            //FSharpList<FScheme.Value> listWatchVal1 = reverse.GetValue(0).GetListFromFSchemeValue();
            //Assert.AreEqual(3, listWatchVal1.Length);
            ////Assert.AreEqual("Dynamo", getStringFromFSchemeValue(listWatchVal1[0]));
            ////Assert.AreEqual("Script", getStringFromFSchemeValue(listWatchVal1[3]));

        }

        [Test]
        public void Reverse_WithSingletonInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Reverse_WithSingletonInput.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //  before Reversing
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,10},
                {1,2},
                {2,3},
            };
            SelectivelyAssertPreviewValues("1c9d53b6-b5e0-4282-9768-a6c53115aba4", validationData);

            // after Reversing
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,3},
                {1,2},
                {2,10},

            };
            SelectivelyAssertPreviewValues("18352d04-273b-4821-8819-bd7676dc4374", validationData1);
            
        }

        #endregion

        #region Filter Tests  

        [Test]
        public void Filter_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            //  before Filter
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {1,1},
                {10,10},
            };
            SelectivelyAssertPreviewValues("a54127b5-decb-4750-aaf3-1b895be73984", validationData);

            // after Filter
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,new int[]{6,7,8,9,10}},
                {1,new int[]{0,1,2,3,4,5}},
            };
            SelectivelyAssertPreviewValues("b03dcac5-14f1-46b8-bcb8-398561d28b83", validationData1);

        }

        [Test]
        public void Filter_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            //  before Filter
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {1,1},
                {5,5},
            };
            SelectivelyAssertPreviewValues("1327061f-b25d-4e91-9df7-a79850cb59e0", validationData);
           
            AssertPreviewValue("b03dcac5-14f1-46b8-bcb8-398561d28b83", new int[]{});
        }

        [Test]
        public void Filter_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Filter_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            //  before Filter
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,new int[]{6,7,8,9,10}},
                {1,new int[]{0,1,2,3,4,5}},
            };
            SelectivelyAssertPreviewValues("d957655d-57d0-4445-a5a8-c730a3cb8d28", validationData);

            // after Filter
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,new int[]{0,1,2,3,4}},
                {1,new int[]{5,6,7,8,9,10}},
            };
            SelectivelyAssertPreviewValues("32e204af-cc73-486c-9add-9215f2688b98", validationData1);

            // after Filter and Combine
            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {1,7},
                {4,2.5},

            };
            SelectivelyAssertPreviewValues("dc27f671-4cef-480f-9ddc-218d61db7e52", validationData2);

        }

        #endregion

        #region LaceShortest test cases  

        [Test]
        public void LaceShortest_Simple()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_Simple.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // Element from the Reverse list
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,4},
                {1,2},
            };
            SelectivelyAssertPreviewValues("c3d629f7-76a0-40bc-bf39-da45d8b8ea7a", validationData);

            // Elements from the Combine list
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,-0.5},
                {1,-1},

            };
            SelectivelyAssertPreviewValues("cc23b43e-3709-4ed1-bedb-f903e4ea7d75", validationData1);

            // Elements from first LaceShortest list
            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {0,2},
                {1,4},

            };
            SelectivelyAssertPreviewValues("10005d3c-3bbf-4690-b658-37b11c8402b1", validationData2);

            // Elements from second LaceShortest list
            Dictionary<int, object> validationData3 = new Dictionary<int, object>()
            {
                {0,-4},
                {1,-4},

            };
            SelectivelyAssertPreviewValues("ce7bf465-0f93-4e5a-8bc9-9960cd077f25", validationData3);

        }

        [Test]
        public void LaceShortest_NegativeInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_NegativeInput.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);
        }

        [Test]
        public void LaceShortest_StringInput()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceShortest_StringInput.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            // Element from the Reverse list
            AssertPreviewValue("1c4c75ff-735d-4431-9df3-2b187c469b3a", "1Design");

            // Elements from first LaceShortest list
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,1},
                {1,1},
                {2,1},
            };
            SelectivelyAssertPreviewValues("10005d3c-3bbf-4690-b658-37b11c8402b1", validationData1);

            // Elements from second LaceShortest list
            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {0,"Dynamo"},
                {1,"Design"},
                {2,"Script"},
            };
            SelectivelyAssertPreviewValues("c19f09a1-6132-4c9c-8f37-5f138e1a3067", validationData2);

        }

        #endregion

        #region LaceLongest test cases  

        [Test]
        public void LaceLongest_Simple()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_Simple.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("5da40769-ffc8-408b-94bb-8c5dff31132e", new int[][]
            {
                new int[] { 2 }, new int[] { 8 }, new int[] { 14 }, new int[] { 19 } 
            });

        }

        [Test]
        public void LaceLongest_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_Negative.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }

        [Test]
        public void LaceLongest_ListWith10000Element()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\LaceLongest_ListWith10000Element.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {1000,2000},
            };
            SelectivelyAssertPreviewValues("25daa241-d8a4-4e74-aec1-6068358babf7", validationData2);

        }

        #endregion

        #region FilterOut test cases  

        [Test]
        public void FilterOut_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // Element from the Number node
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,1},
                {1,2},
                {2,3},
            };
            SelectivelyAssertPreviewValues("b6571eb6-b1c2-4874-8749-b783176dc039", validationData1);

            // Elements from first FilterOut list
            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {0,new int[]{1,2}},
                {1,new int[]{3,4,5,6,7,8,9,10}},
            };
            SelectivelyAssertPreviewValues("53ec97e2-d860-4fdc-8ea5-2288bf39bcfc", validationData2);

            // Elements from second FilterOut list
            Dictionary<int, object> validationData3 = new Dictionary<int, object>()
            {
                {0,new int[]{4,5,6,7,8,9,10}},
                {1,new int[]{1,2,3}},
            };
            SelectivelyAssertPreviewValues("0af3f566-1b05-4578-9fb0-297ca98d6d8c", validationData3);

        }

        [Test]
        public void FilterOut_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            // Element from the Number node
            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,1},
                {1,2},
                {2,3},
            };
            SelectivelyAssertPreviewValues("b6571eb6-b1c2-4874-8749-b783176dc039", validationData1);

            // Elements from FilterOut list
            Dictionary<int, object> validationData2 = new Dictionary<int, object>()
            {
                {0,new int[]{1,2}},
                {1,new int[]{3,4,5,6,7,8,9,10}},
            };
            SelectivelyAssertPreviewValues("53ec97e2-d860-4fdc-8ea5-2288bf39bcfc", validationData2);

            // Elements from Take from List
            Dictionary<int, object> validationData3 = new Dictionary<int, object>()
            {
                {0,3},
                {1,4},
                {2,5},
            };
            SelectivelyAssertPreviewValues("6921b2ef-fc5c-44b4-992f-9421c267d9ef", validationData3);


            // Elements from Drop from List 
            AssertPreviewValue("57a41c41-fa71-41dd-aa25-ca2156f2ba0b", new int[] { });

        }

        [Test]
        public void FilterOut_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\FilterOut_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }


        #endregion

        #region NumberRange test cases -PartiallyDone

        [Test]
        public void NumberRange_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
                {50,50},
            
            };
            
            SelectivelyAssertPreviewValues("4e781f03-5b48-4d58-a511-8c732665e961", validationData);

        } 

        [Test]
        public void NumberRange_LargeNumber()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LargeNumber.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {1000000,1000000},
            
            };

            SelectivelyAssertPreviewValues("4e781f03-5b48-4d58-a511-8c732665e961", validationData);

        }

        [Test]
        public void NumberRange_LacingShortest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingShortest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,1},
                {8,9},
            };

            //this is the case of 2D array. Need to change verification
            SelectivelyAssertPreviewValues("798bc857-f36e-44df-97cc-6e878aef5b72", validationData);

            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,2},
                {3,8},
            };

            //this is the case of 2D array. Need to change verification
            SelectivelyAssertPreviewValues("2f5277db-4656-4014-8d85-8e4f51e5c2b1", validationData1);


        }

        [Test]
        public void NumberRange_LacingLongest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingLongest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            //Dictionary<int, object> validationData = new Dictionary<int, object>()
            //{
            //    {9,10},
            //};

            //Dictionary<int, object> validationData1 = new Dictionary<int[], object>()
            //{
            //    {[1][9],10},
            //};

            AssertPreviewValue("4e781f03-5b48-4d58-a511-8c732665e961", new int[][] { new int[] { }, new int[] { } });

        }

        [Test, Category("Not Migrated")]
        public void NumberRange_LacingCrossProduct()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\NumberRange_LacingCrossProduct.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var numberRange = model.CurrentWorkspace.NodeFromWorkspace<NumberRange>("4e781f03-5b48-4d58-a511-8c732665e961");

            var actual = numberRange.GetValue(0).GetElements();
            var innerList1 = actual[0].GetElements();
            var innerList2 = actual[1].GetElements();
            var actualChild1 = innerList1[0].GetElements();
            var actualChild2 = innerList1[1].GetElements();
            var actualChild3 = innerList2[0].GetElements();
            var actualChild4 = innerList2[1].GetElements();

            Assert.AreEqual(2, actual.Count);

            Assert.AreEqual(10, actualChild1.Count);
            Assert.AreEqual(10, actualChild1[9].Data);

            Assert.AreEqual(5, actualChild2.Count);
            Assert.AreEqual(9, actualChild2[4].Data);

            Assert.AreEqual(9, actualChild3.Count);
            Assert.AreEqual(10, actualChild3[8].Data);

            Assert.AreEqual(5, actualChild4.Count);
            Assert.AreEqual(10, actualChild4[4].Data);
        }

        #endregion

        #region ListMinimum test cases  

        [Test]
        public void ListMinimum_NumberRange()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ListMinimum_NumberRange.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6", -1);

        }

        [Test]
        public void ListMinimum_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ListMinimum_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6", 5);

        }

        #endregion

        #region AddToList test cases -PartiallyDone

        [Test, Category("Not Migrated")]
        public void AddToList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var addToList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.List>("31d0eb4e-8657-4eb1-a852-5e9b766eddd7");

            var actual = addToList.GetValue(0).GetElements();
            var childList = actual[2].GetElements();

            Assert.AreEqual(6, actual.Count);
            Assert.AreEqual("Design", actual[0].Data);
            Assert.AreEqual(10, actual[5].Data);

            Assert.AreEqual(4, childList.Count);
            Assert.AreEqual(-10, childList[0].Data);
        }

        [Test]
        public void AddToList_EmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_EmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            //AssertPreviewValue("1976caa7-d45e-4a44-9faf-345d98337bbb", new int[]{new int[]{null,0}});

        }

        [Test]
        public void AddToList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("cfdfc020-05d0-4442-96df-8d97aad9c38c", new int[][]
                {
                    new int[]{3}, new int[]{6}, new int[]{9}
                });

        }

        [Test]
        public void AddToList_GeometryToList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_GeometryToList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
            {3, "Design"},
            };

            SelectivelyAssertPreviewValues("31d0eb4e-8657-4eb1-a852-5e9b766eddd7", validationData);

        }

        [Test]
        public void AddToList_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\AddToList_Negative.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

        }

        #endregion

        #region SplitList test cases -PartiallyDone

        [Test]
        public void SplitList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", "Dynamo");

            AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", new int[][]{new int[]{0,1}});
        }

        [Test]
        public void SplitList_FirstElementAsList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_FirstElementAsList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", "Dynamo");

            AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", new int[][] { new int[] { 0, 1 } });


        }

        [Test]
        public void SplitList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", new int[] { 3 });

            AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", new int[][] { new int[] { 6 }, new int[] { 9 } });


        }

        [Test, Category("Not Migrated")]
        public void SplitList_ComplexAnotherExample()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SplitList_ComplexAnotherExample.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var splitList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.DeCons>("66e94123-deaf-4bc8-8c5f-b3bc0996a57e");

            var firstOutput = splitList.GetValue(0).GetElements();
            var secondOutput = splitList.GetValue(1).GetElements();
            var child = secondOutput[0].GetElements();
            var child1 = secondOutput[1].GetElements();

            Assert.AreEqual(12, firstOutput.Count);
            Assert.AreEqual("x", firstOutput[0].Data);
            Assert.AreEqual("z", firstOutput[11].Data);

            Assert.AreEqual(2, secondOutput.Count);

            Assert.AreEqual(12, child.Count);
            Assert.AreEqual(19.35, child[0].Data);

            Assert.AreEqual(12, child1.Count);
            Assert.AreEqual(32.85, child1[0].Data);
        }
        #endregion

        #region TakeFromList test cases -PartiallyDone

        [Test, Category("Not Migrated")]
        public void TakeFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            var takeFromList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.TakeList>("14cb6593-24d8-4ffc-8ee5-9f4247449fc2");

            var firstOutput = takeFromList.GetValue(0).GetElements();
            var child = firstOutput[0].GetElements();
            var child1 = firstOutput[4].GetElements();

            Assert.AreEqual(5, firstOutput.Count);

            Assert.AreEqual(1, child.Count);
            Assert.AreEqual(3, child[0].GetElements());

            Assert.AreEqual(1, child1.Count);
            Assert.AreEqual(15, child1[0].GetElements());
        }

        [Test]
        public void TakeFromList_WithStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_WithStringList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("14cb6593-24d8-4ffc-8ee5-9f4247449fc2", new string[] 
            { 
                "Test", "Take", "From", "List" 
            });
        }

        [Test]
        public void TakeFromList_NegativeIntValue()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_NegativeIntValue.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("14cb6593-24d8-4ffc-8ee5-9f4247449fc2", new string[] { "List" });

        }

        [Test]
        public void TakeFromList_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }

        [Test]
        public void TakeFromList_AmtAsRangeExpn()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeFromList_AmtAsRangeExpn.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

        }

        #endregion

        #region DropFromList test cases  
        [Test]
        public void DropFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropFromList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,0},
            };

            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,5},
            };
            SelectivelyAssertPreviewValues("e2d27010-b8fc-4eb8-8703-63bab5ce6e85", validationData);

            SelectivelyAssertPreviewValues("097e0b4b-4cbb-43b1-a21c-77a619ad1050", validationData1);
        }

        [Test]
        public void DropFromList_InputEmptyList()
        {

            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropFromList_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);
        }

        #endregion

        #region ShiftListIndices test cases -PartiallyDone

        [Test]
        public void ShiftListIndices_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,7},
                {4,1},
            };
            SelectivelyAssertPreviewValues("7f6cbd60-b9fb-4b16-81d3-4fab26790446", validationData);
        }

        [Test, Category("Not Migrated")]
        public void ShiftListIndices_Complex()
        {
            Assert.Inconclusive("String To Number node had been deprecated, cannot run this TestCase");
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(21, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var shiftListIndeces = model.CurrentWorkspace.NodeFromWorkspace<ShiftList>("492db019-4807-4810-8919-10b94e8ca083");
            var output = shiftListIndeces.GetValue(0).GetElements();
            var child = output[0].GetElements();
            var child1 = output[1].GetElements();

            Assert.AreEqual(2, output.Count);

            Assert.AreEqual(12, child.Count);
            Assert.AreEqual(32.85, child[0].Data);
            Assert.AreEqual(50.2275, child[11].Data);

            Assert.AreEqual(12, child1.Count);
            Assert.AreEqual(19.35, child1[0].Data);
            Assert.AreEqual(108.7275, child1[11].Data);
        }

        [Test]
        public void ShiftListIndices_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("7f6cbd60-b9fb-4b16-81d3-4fab26790446", new int[] { });
        }

        [Test]
        public void ShiftListIndices_InputStringAsAmt()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_InputStringAsAmt.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }

        [Test]
        public void ShiftListIndices_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\ShiftListIndeces_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }
        #endregion

        #region GetFromList test cases PartiallyDone

        [Test]
        public void GetFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("332093dc-4551-4c82-9f6b-061c7945211b", new int[] { 9 });
        }

        [Test]
        public void GetFromList_WithStringList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_WithStringList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("58d35bfa-4435-44f0-a322-c6f7350f0220", new string[] { "Get", "From " });
        }

        [Test, Category("Not Migrated")]
        public void GetFromList_AmtAsRangeExpn()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_AmtAsRangeExpn.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var getFromList = model.CurrentWorkspace.NodeFromWorkspace("d2f1c900-99ce-40a5-ae4d-bbac1fe96cfd");
            var output = getFromList.GetValue(0).GetElements();

            Assert.AreEqual(3, output.Count);
            Assert.AreEqual(14, output[0].Data);
            Assert.AreEqual(2, output[1].Data);
            Assert.AreEqual(3, output[2].Data);

        }

        [Test]
        public void GetFromList_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

        }

        [Test]
        public void GetFromList_Negative()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_Negative.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
        }

        [Test]
        public void GetFromList_NegativeIntValue()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\GetFromList_NegativeIntValue.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

        }

        #endregion

        #region TakeEveryNth test case  

        [Test]
        public void TakeEveryNth_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,10},
                {1,13},
            };
            SelectivelyAssertPreviewValues("b18e5ac3-5732-4c78-9a3b-56b375c9beee", validationData);
        }

        [Test]
        public void TakeEveryNth_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("b18e5ac3-5732-4c78-9a3b-56b375c9beee", new double[] { 2.3 });
        }

        [Test]
        public void TakeEveryNth_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("b18e5ac3-5732-4c78-9a3b-56b375c9beee", new int[] { });
        }

        [Test]
        public void TakeEveryNth_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TakeEveryNth_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
        }
        #endregion

        #region DropEveryNth -PartiallyDone

        [Test]
        public void DropEveryNth_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("96a1ca07-83eb-4459-981e-7daed6d1d4b3", new int[] { 6, 7, 8, 9 });
        }

        [Test, Category("Not Migrated")]
        public void DropEveryNth_ComplexTest()
        {
            Assert.Inconclusive("String To Number node had been deprecated, cannot run this TestCase");

            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_ComplexTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(19, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var takeEveryNth = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.RemoveEveryNth>("4bd0ced4-29ee-4f4e-95af-d0573e04731a");
            var output = takeEveryNth.GetValue(0).GetElements();
            var child = output[0].GetElements();
            var child1 = output[1].GetElements();

            Assert.AreEqual(2, output.Count);

            Assert.AreEqual(12, child.Count);
            Assert.AreEqual("x", child[0].Data);

            Assert.AreEqual(12, child1.Count);
            Assert.AreEqual(32.85, child1[0].Data);
        }

        [Test]
        public void DropEveryNth_InputEmptyList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_InputEmptyList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("a0304232-ad3a-4518-92ff-4b8893297ce4", new int[] { });
        }

        [Test]
        public void DropEveryNth_InputStringForNth()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\DropEveryNth_InputStringForNth.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
        }
        #endregion

        #region RemoveFromList test cases  

        [Test]
        public void RemoveFromList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0, "testRemoveFromList"},
                {1,"Dynamo"},
                {2,10.689},
                {3,"Design"},
                {4,"Script"},
            };

            SelectivelyAssertPreviewValues("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6", validationData);
            
        }

        [Test]
        public void RemoveFromList_StringAsList()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_StringAsList.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

        }

        [Test]
        public void RemoveFromList_StringAsIndex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_StringAsIndex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);
        }

        [Test]
        public void RemoveFromList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,7},
                {1,8},
            };
            SelectivelyAssertPreviewValues("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6", validationData);

        }

        [Test]
        public void RemoveFromList_RangeExpnAsIndex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\RemoveFromList_RangeExpnAsIndex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,10},
                {1,1},

            };

            SelectivelyAssertPreviewValues("dc581841-0221-4bc3-9010-3d2f0081a169", validationData);
            AssertPreviewValue("396d2bbe-7b84-4a7a-a1f0-b7a438b56ca6", new int[] { });

        }
        #endregion

        #region Slice test cases  

        [Test]
        public void SliceList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SliceList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4 + 1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3 + 1, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,1},
                {1,4},
            };
            SelectivelyAssertPreviewValues("df64e6c8-3a73-40b3-b63c-34cb5936b848", validationData);
        }

        [Test]
        public void SliceList_Complex()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SliceList_Complex.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11 + 1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12 + 1, model.CurrentWorkspace.Connectors.Count);
            
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,"Design"},
                {1,4.5},
                {2,"Dynamo"},
            };
            SelectivelyAssertPreviewValues("cc3ae092-8644-4a36-ad38-12ffa15cebda", validationData);

        }

        [Test]
        public void SliceList_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\SliceList_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11 + 1, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12 + 1, model.CurrentWorkspace.Connectors.Count);
        }
        #endregion

        #region Test Average  

        [Test]
        public void Average_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Average_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("cafe8fae-55f0-4e58-977e-80edcbc90d2b", 6);
        }

        [Test]
        public void Average_NegativeInputTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Average_NegativeInputTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

        }

        #endregion

        #region Test TrueForAny  

        [Test]
        public void TrueForAny_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TrueForAny_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("3960776a-4c6c-40d8-8b7e-dbe5db38d75b", true);
        }

        #endregion

        #region Test TrueForAll  

        [Test]
        public void TrueForAll_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\TrueForAll_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            AssertPreviewValue("6434eb4f-89d9-4b11-8b9b-79ed937e4b24", 1);
        }

        #endregion

        #region Test Smooth  

        [Test]
        public void Smooth_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Smooth_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {4,14.242000000000001},
                {5,15.240000000000002},
            };

            SelectivelyAssertPreviewValues("e367bd22-e0ef-402e-b6c0-3a7aaee2be63", validationData);
        }

        [Test]
        public void Smooth_InputListNode()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Smooth_InputListNode.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0, 54.36},
                {2,21.816733333333332},
                {3,37.426796749999994},
            };
            SelectivelyAssertPreviewValues("ae41ff44-6f8c-4037-86ad-5ec3b22956a6", validationData);
        }

        [Test]
        public void Smooth_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Smooth_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
        }
        #endregion

        #region Test Join List -PartiallyDone

        [Test]
        public void JoinList_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\JoinList_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0,10},
                {3,77.5},
                {5,"Dynamo"},
                {6,10},

            };
            SelectivelyAssertPreviewValues("1304807f-6d18-4aef-b4cb-9cb8f469993e", validationData);

        }

        [Test, Category("Not Migrated")]
        public void JoinList_MoreLists()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\JoinList_MoreLists.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            var joinList = model.CurrentWorkspace.NodeFromWorkspace<Dynamo.Nodes.Append>("1304807f-6d18-4aef-b4cb-9cb8f469993e");
            var actual = joinList.GetValue(0).GetElements();
            var actualChild1 = actual[5].GetElements();
            var actualChild2 = actual[6].GetElements();

            Assert.AreEqual(7, actual.Count);

            Assert.AreEqual(10, actual[0].Data);
            Assert.AreEqual(77.5, actual[3].Data);

            Assert.AreEqual(2, actualChild1.Count);
            Assert.AreEqual("Dynamo", actualChild1[0].Data);
            Assert.AreEqual(10, actualChild1[1].Data);

            Assert.AreEqual(3, actualChild2.Count);
            Assert.AreEqual("DS", actualChild2[0].Data);
            Assert.AreEqual(0.256, actualChild2[2].Data);
        }

        #endregion

        #region Test Combine  

        [Test]
        public void Combine_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Combine_SimpleTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData1 = new Dictionary<int, object>()
            {
                {0,4},
                {7,7.1929824561403501},
                {14,8.0332409972299157},
            };
            SelectivelyAssertPreviewValues("e0cbb116-6c81-4e74-90c6-a5235cfb9eea", validationData1);

        }


        [Test]
        public void Combine_ComplexTest()
        {
            Assert.Inconclusive("String To Number node had been deprecated, cannot run this TestCase");

            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Combine_ComplexTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(25, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(28, model.CurrentWorkspace.Connectors.Count);

            Dictionary<int, object> validationData = new Dictionary<int,object>()
            {
                {0,56.7},
                {4,6},
                {8,182.355},
            };
            SelectivelyAssertPreviewValues("f5b7116b-e926-499d-b784-f47c55e01e34", validationData);

        }

        [Test]
        public void Combine_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\list\Combine_NegativeTest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
        }

        #endregion

    }
}

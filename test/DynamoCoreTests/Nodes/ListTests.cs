using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesignScript.Builtin;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;


namespace Dynamo.Tests
{
    class ListTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        string listTestFolder { get { return Path.Combine(TestDirectory, "core", "list"); } }

        #region Test Build Sublist  

        [Test]
        public void TestListMaxMinItem()
        {
            string testFilePath = Path.Combine(listTestFolder, "ListMaximumItem.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("a3ddba22-39bf-4bd7-9e61-d6119603f5b1", 12.5);
            AssertPreviewValue("234f6570-a327-44ad-9e27-38e8dbc9f3c9", 3);

            AssertPreviewValue("65286923-306d-4478-854e-d39a4ca3d1b8", 12.5);
            AssertPreviewValue("b7f5258f-ae1b-4300-bffd-d40d7bfc2ce9", 3);
        }

        [Test]
		public void TestBuildSublistsEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_emptyInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("9375d612-cccb-4ba2-96e1-4d1497c6234b", new int[] { });
		}

		[Test]
		public void TestBuildSublistsInvalidInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_invalidInput.dyn");
			RunModel(testFilePath);

            AssertPreviewValue("516c4904-38d8-40d8-b247-7133f33836ce", new int[] { });
		}

		[Test]
		public void TestBuildSublistsNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("9240cdc9-5bbf-4579-930c-ef742a91d798", new int[][] { new int[] { 1 }, new int[] { 3 } });
		}

		[Test]
		public void TestBuildSublistsStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testBuildSubLists_stringInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("9240cdc9-5bbf-4579-930c-ef742a91d798", new string[][] { new string[] { "b" }, new string[] { "d" } });
		}

		#endregion

		#region Test Concatenate List  

		[Test]
		public void TestConcatenateListsEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("760c9f00-e12c-4db9-bbdf-a19562efdd09", new int[]{});

		}

		[Test]
		public void TestConcatenateListsSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("c03730e8-320d-4da4-b54d-1dc990dd5def", new object[] { 20, 10 });
		}

		[Test]
		public void TestConcatenateListsNormalInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testConcatenateLists_normalInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("364b303f-8f0b-4964-b333-e937299c8352", new object[] { 10, 20, 10, 20, 10, "a", "b", "a", "b" });

		}

		#endregion

		#region Test DiagonalLeftList  

		[Test]
		public void TestDiagonalLeftListEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("a54ad1f8-9b02-4ebf-9d4e-a53608906145", new int[]{});
		}

		[Test]
		public void TestDiagonalLeftListInvalidInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_invalidInput.dyn");
			RunModel(testFilePath);

            AssertPreviewValue("a3f8b65f-2e02-480a-9d41-1139f0b40f07", new int[] {20});
		}

		[Test]
		public void TestDiagonalLeftListNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("87345663-8421-46f0-acd2-051e4ec5ff88", new int[][]{new int[]{1}, new int[]{2,3}, new int[]{4,5}});
		}

		[Test]
		public void TestDiagonalLeftListStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaLeftList_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("87345663-8421-46f0-acd2-051e4ec5ff88", new string[][] { new string[] { "a" }, new string[] { "b", "a" }, new string[] { "b" } });
		}

		#endregion

		#region Test DiagonalRightList  

		[Test]
		public void TestDiagonalRightListEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("49f4ebe5-fd49-462b-9896-fe1244f66486", new int[]{});

		}

		[Test]
		public void TestDiagonalRightListInvalidInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_invalidInput.dyn");
			RunModel(testFilePath);

		}

		[Test]
		public void TestDiagonalRightListNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testDiagonaRightList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("e84bf89e-e7a0-427c-adae-adcd61646e4e", new int[][] { new int[] { 5 }, new int[] { 3 }, new int[] { 1, 4 }, new int[] { 2 } });

		}

		#endregion

		#region Test First Of List  

		[Test]
		public void TestFirstOfListEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_emptyInput.dyn");
			RunModel(testFilePath);
		}

		[Test]
		public void TestFirstOfListSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("3d5f481b-0701-4948-bf84-134bbe360449", 20);
		}

		[Test]
		public void TestFirstOfListNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("879fda8f-b9f4-453b-bf4d-faeb76ce5ffc", 10);
		}

		[Test]
		public void TestFirstOfListStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testFirstOfList_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("879fda8f-b9f4-453b-bf4d-faeb76ce5ffc", "a");
		}

		#endregion

		#region Test Empty List  

		[Test]
		public void TestIsEmptyListEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 1);

		}

		[Test]
		public void TestIsEmptyListSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", false);
		}

		[Test]
		public void TestIsEmptyListNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 0);

		}

		[Test]
		public void TestIsEmptyListStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testIsEmptyList_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("d98b4671-fa55-4303-a9a2-e1b383d737da", 0);

		}

		#endregion

		#region Test List Length  

		[Test]
		public void TestStringLengthEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testListLength_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("8ab87f7a-2577-46b9-bee6-512b1678b028", 0);

		}

		[Test]
		public void TestStringLengthSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testListLength_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("bb376d8d-ce77-46a4-822a-b8136a56287a", 1);
		}

		[Test]
		public void TestStringLengthNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testListLength_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("18473048-4a5f-4b23-8578-d9b8c0f32c0f", 5);

		}

		[Test]
		public void TestStringLengthStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testListLength_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("18473048-4a5f-4b23-8578-d9b8c0f32c0f", 4);

		}

		#endregion

		#region Test Partition List  

		[Test]
		public void TestPartitionStringEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPartitionList_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("6cad28c0-605a-4b58-84a2-87939f81f61e", new int[] { });

		}

        [Test]
        public void TestPartitionStringSingleInput()
        {
            string testFilePath = Path.Combine(listTestFolder, "testPartitionList_singleInput.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("4070b941-f4ef-4fc0-bced-2eb469fcd5f8", new object[] { new object[] { 20 } });
        }

        [Test]
        public void TestPartitionStringNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPartitionList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("a3cdc54a-5965-47ea-b294-f893b1b64ae2", new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5 } });
		}

		[Test]
		public void TestPartitionStringStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPartitionList_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("a3cdc54a-5965-47ea-b294-f893b1b64ae2", new string[][] { new string[] { "a", "b", "a" }, new string[] { "b" } });
		}

		#endregion

		#region Test Flatten

		[Test]
		public void TestFlattenEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPlatten_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("4cc4e5f0-4338-43bb-911e-d7c10ea2b53c", new int[] { });

		}

		[Test]
		public void TestFlattenSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPlatten_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("a5cd432d-28d7-4978-901e-c529f8e65dd4", new int[] { 20 });
		}

		[Test]
		public void TestFlattenNormalInput()
		{
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
			string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_emptyInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("641db696-5626-4af0-b07e-6335c6dc4bc9", new int[] { });
		}

		[Test]
		public void TestFlattenCompletlySingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("6595fa34-fc78-4995-8efb-9fd7e73cbd8a", new int[] { 20 });
		}

		[Test]
		public void TestFlattenCompletlyNormalInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testPlattenCompletely_normalInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("76609452-9c1d-4d71-9223-bb13c323f3a6", new object[] { 0, 1, 2, 3, 4, "a", "b", "c", "d", "a", "b", "c", "d" });

		}

		#endregion

		#region Test Repeat  

		[Test]
		public void TestRepeatEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRepeat_emptyInput.dyn");
			RunModel(testFilePath);
		}

		[Test]
		public void TestRepeatNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRepeat_numberInput.dyn");
			RunModel(testFilePath);
			var a1 = new[] { 0, 1, 2, 3, 4 };
			AssertPreviewValue("72dddbc8-0a6b-431d-a185-8ec62a8b79dd", new[] { a1, a1 });
		}

		[Test]
		public void TestRepeatStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRepeat_stringInput.dyn");
			RunModel(testFilePath);

			var a1 = new[] { "a", "b", "c", "d" };
			AssertPreviewValue("72dddbc8-0a6b-431d-a185-8ec62a8b79dd", new[] { a1, a1 });
		}

		#endregion

		#region Test RestOfList

		[Test]
		public void TestRestOfListSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRestOfList_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("25af2003-1b6e-4a97-9727-a7583119271d", new object[] { });
		}

		[Test]
		public void TestRestOfListNumberInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRestOfList_numberInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("3d3e481b-16ef-4837-b94e-7922f9e42029", new int[] { 20, 10, 20, 10 });

		}

		[Test]
		public void TestRestOfListStringInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testRestOfList_stringInput.dyn");
			RunModel(testFilePath);
			AssertPreviewValue("3d3e481b-16ef-4837-b94e-7922f9e42029", new string[] { "b", "a", "b" });
		}

		#endregion

		#region Test Transpose List  
		[Test]
		public void TestTransposeEmptyInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testTransposeList_emptyInput.dyn");
			RunModel(testFilePath);

			var watch = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("df181bd7-3f1f-4195-93af-c0b846f6c8ce");

			var actual = watch.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			Assert.AreEqual(0, actual.Count);
		}

		[Test]
		public void TestTransposeSingleInput()
		{
			string testFilePath = Path.Combine(listTestFolder, "testTransposeList_singleInput.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("d37a5827-d7d3-41d4-bd68-8ab6666aed39", new object[] {20});
		}

		[Test]
		public void TestTransposeNormalInput()
		{
			// Input array                  Expected output array
			// {                            {
			//     { "AB", "CD" }               { "AB", 21, 31 }
			//     { 21, 22, 23, 24 }           { "CD", 22, 32 }
			//     { 31, 32, 33 }               { null, 23, 33 }
			// }                                { null, 24, null }
			//                              }

			string testFilePath = Path.Combine(listTestFolder, "testTransposeList_normalInput.dyn");
			RunModel(testFilePath);

			string guid = "e639bc66-6dec-4a0a-bae2-9bac7dab59dc";
			var nodeTranspose = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(guid);
			var elements = nodeTranspose.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();

			Assert.AreEqual(4, elements.Count);
			Assert.AreEqual(3, elements[0].GetElements().ToList().Count);
			Assert.AreEqual(3, elements[1].GetElements().ToList().Count);
			Assert.AreEqual(3, elements[2].GetElements().ToList().Count);
			Assert.AreEqual(3, elements[3].GetElements().ToList().Count);

			AssertPreviewValue(guid,
				new object[][]
				{
					new object[] { "AB", 21, 31 },
					new object[] { "CD", 22, 32 },
					new object[] { null, 23, 33 },
					new object[] { null, 24, null }
				});
		}

		[Test]
		public void TestTranspose()
		{
			string testFilePath = Path.Combine(listTestFolder, "transpose.dyn");
			RunModel(testFilePath);

			AssertPreviewValue("b51e05aa-e37b-46f0-9bc1-9c5042b3f07e",
				new object[] { new object[] {1,3 }, new object[] {2, 4}, });

			AssertPreviewValue("919d4d0d-f4e3-4a3c-87c8-dd4466a8ca87",
				new object[] { new object[] {1,4,6}, new object[] {2, 5,7}, new object[] {3, null, 8}, new object[] {null, null, 9}});

			AssertPreviewValue("bcf696d1-43e7-4633-8eb9-d5cb64dde939",
				new object[] { new object[] { new object[] { 1 }, 3 }, new object[] { 2, 4 } });

			AssertPreviewValue("83892f27-5d86-447f-a96c-121cac42a54b",
				new object[] { });

			AssertPreviewValue("35a16bab-e7dd-4e04-9724-0344152ab0f3",
				new object[] { 1, 2});

			AssertPreviewValue("f619b5c1-debc-48b4-a396-577545388bc1",
				new object[] { 1});
		}
		#endregion

		#region Sort Test Cases  

		[Test]
		public void Sort_NumbersfFromDiffInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Sort_NumbersfFromDiffInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.LessOrEqual(18, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.LessOrEqual(15, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Sort_SimpleNumbers.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

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
		public void Sort_StringsAndNumbers()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Sort_Strings&Numbers.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

			AssertPreviewValue("14fae78b-b009-4503-afe9-b714e08db1ec",
				new object[] { 0, "a", "aa", "bbbbbbbbbbbbb", "dddd" });
		}

		[Test]
		public void Sort_Strings()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Sort_Strings.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\SortBy_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());


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
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_ListWithOneNumber.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());


			// First element in the list before Reversing
			AssertPreviewValue("cd36fac7-d9eb-47ea-a73d-ad1bb4bbe54a", new int[] { 0 });
		}

		[Test]
		public void Reverse_MixedList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_MixedList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());


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
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_NumberRange.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());


			// First and Last element in the list before Reversing
			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{0,-1},
				{7,6},

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_UsingStringList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());


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
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_WithArrayInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Assert.AreEqual(16, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

			//// First and last element in the list before Reversing
			//var watch = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("1c9d53b6-b5e0-4282-9768-a6c53115aba4");
			//FSharpList<FScheme.Value> listWatchVal = watch.GetValue(0).GetListFromFSchemeValue();
			//Assert.AreEqual(3, listWatchVal.Length);
			////Assert.AreEqual(2, GetDoubleFromFSchemeValue(listWatchVal[0]));
			////Assert.AreEqual("Dynamo", GetDoubleFromFSchemeValue(listWatchVal[3]));

			//// First and last element in the list after Reversing
			//var reverse = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Reverse>("18352d04-273b-4821-8819-bd7676dc4374");
			//FSharpList<FScheme.Value> listWatchVal1 = reverse.GetValue(0).GetListFromFSchemeValue();
			//Assert.AreEqual(3, listWatchVal1.Length);
			////Assert.AreEqual("Dynamo", getStringFromFSchemeValue(listWatchVal1[0]));
			////Assert.AreEqual("Script", getStringFromFSchemeValue(listWatchVal1[3]));

		}

		[Test]
		public void Reverse_WithSingleInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Reverse_WithSingleInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Filter_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			//  before Filter
		    var validationData = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            AssertPreviewValue("a54127b5-decb-4750-aaf3-1b895be73984", validationData);

			// after Filter
		    var validationData1 = Dictionary.ByKeysValues(new[] {"in", "out"},
		        new object[] {new int[] {6, 7, 8, 9, 10}, new int[] {0, 1, 2, 3, 4, 5}});
            AssertPreviewValue("b03dcac5-14f1-46b8-bcb8-398561d28b83", validationData1);
		}

		[Test]
		public void Filter_NegativeTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Filter_NegativeTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			//  before Filter
		    var validationData = new[] {0, 1, 2, 3, 4, 5};
            AssertPreviewValue("1327061f-b25d-4e91-9df7-a79850cb59e0", validationData);

		    var validationData1 = Dictionary.ByKeysValues(new[] {"in", "out"},
		        new object[] {new object[] {}, new[] {0, 1, 2, 3, 4, 5}});
		    
			AssertPreviewValue("b03dcac5-14f1-46b8-bcb8-398561d28b83", validationData1);
		}

		[Test]
		public void Filter_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Filter_Complex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			//  before Filter
		    var validationData = Dictionary.ByKeysValues(new[] {"in", "out"},
		        new object[] {new int[] {6, 7, 8, 9, 10}, new int[] {0, 1, 2, 3, 4, 5}});
            AssertPreviewValue("d957655d-57d0-4445-a5a8-c730a3cb8d28", validationData);

            // after Filter
            var validationData1 = Dictionary.ByKeysValues(new[] { "in", "out" },
                new object[] { new int[] { 0, 1, 2, 3, 4 }, new int[] { 5, 6, 7, 8, 9, 10 } });
            AssertPreviewValue("32e204af-cc73-486c-9add-9215f2688b98", validationData1);
        }

		#endregion

		#region LaceShortest test cases  

		[Test]
		public void LaceShortest_Simple()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceShortest_Simple.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
        public void LaceCartesian_Simple()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\LaceCartesian_Simple.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Element from the Reverse list
            Dictionary<int, object> validationData = new Dictionary<int, object>()
            {
                {0, new[] {2, 3, 4}},
                {1, new[] {3, 4, 5}},
                {2, new[] {4, 5, 6}}
            };
            

            // Elements from List.CartesianProduct list
            SelectivelyAssertPreviewValues("c0616589-1bc1-45b7-9e81-4d48c0c8f4ad", validationData);

            // Elements from Add node list
            SelectivelyAssertPreviewValues("17e7aa2c-2159-42a8-9c82-ea8594e410b9", validationData);

        }

        [Test]
		public void LaceShortest_NegativeInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceShortest_NegativeInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		[Test]
		public void LaceShortest_StringInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceShortest_StringInput.dyn");
			RunModel(openPath);

			// Elements from first LaceShortest list
			Dictionary<int, object> validationData1 = new Dictionary<int, object>()
			{
				{0,"DynamoDynamo"},
				{1,"DesignDesign"},
				{2,"ScriptScript"},
			};
            SelectivelyAssertPreviewValues("10005d3c-3bbf-4690-b658-37b11c8402b1", validationData1);
		}

		[Test]
        [Category("RegressionTests")]
		public void LaceShortest_WithSingleValueInput()
		{
			// details are given in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2464
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceShortest_WithSingleValueInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			// input to + nodes are 1..2 and {1}.
			AssertPreviewValue("ad4325ac-075f-4e2a-93ee-c41c4cfa6a2b", new int[] { 2 });

			// intput to + nodes are 1..2 and 1.
			AssertPreviewValue("80fbd9cd-42b6-4669-b41b-e5c525c77c52", new int[] { 2,3 });

		}


		#endregion

		#region LaceLongest test cases  

		[Test]
		public void LaceLongest_Simple()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceLongest_Simple.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("5da40769-ffc8-408b-94bb-8c5dff31132e", new int[][]
			{
				new int[] { 2 }, new int[] { 8 }, new int[] { 14 }, new int[] { 19 } 
			});

		}

		[Test]
		public void LaceLongest_Negative()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\LaceLongest_Negative.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

		}

        [Test]
        public void DSCore_LaceLongest()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\DSCore_LaceLongest.dyn");
            RunModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var validationData2 = new Dictionary<int, object>() { { 10, 21 } };
            SelectivelyAssertPreviewValues("25daa241-d8a4-4e74-aec1-6068358babf7", validationData2);
        }

		#endregion

		#region FilterOut test cases  

		[Test]
		public void FilterOut_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\FilterOut_SimpleTest.dyn");
			RunModel(openPath);

			// Elements from first FilterOut list

		    var validationData2 = DesignScript.Builtin.Dictionary.ByKeysValues(
		        new[] {"in", "out"}, new object[] {new[] {1, 2}, new[] {3, 4, 5, 6, 7, 8, 9, 10}});
            AssertPreviewValue("53ec97e2-d860-4fdc-8ea5-2288bf39bcfc", validationData2);

			// Elements from second FilterOut list
		    var validationData3 = DesignScript.Builtin.Dictionary.ByKeysValues(
		        new[] {"in", "out"}, new object[] {new[] {4, 5, 6, 7, 8, 9, 10}, new[] {1, 2, 3}});
            AssertPreviewValue("0af3f566-1b05-4578-9fb0-297ca98d6d8c", validationData3);

		}

		[Test]
		public void FilterOut_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\FilterOut_Complex.dyn");
			RunModel(openPath);

			// Elements from Take from List
		    var validationData3 = new[] {3, 4, 5};
            AssertPreviewValue("6921b2ef-fc5c-44b4-992f-9421c267d9ef", validationData3);


			// Elements from Drop from List 
		    AssertPreviewValue("57a41c41-fa71-41dd-aa25-ca2156f2ba0b", new int[] {});

		}

		[Test]
		public void FilterOut_NegativeTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\FilterOut_NegativeTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

		}


		#endregion

		#region NumberRange test cases -PartiallyDone

		[Test]
		public void NumberRange_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\NumberRange_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\NumberRange_LargeNumber.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{1000000,1000000},
			
			};

			SelectivelyAssertPreviewValues("4e781f03-5b48-4d58-a511-8c732665e961", validationData);

		}

		[Test]
		public void NumberRange_LacingShortest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\NumberRange_LacingShortest.dyn");
			RunModel(openPath);

            AssertPreviewValue("4e781f03-5b48-4d58-a511-8c732665e961",
                new[]
                {
                    new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new[] { 2, 4, 6, 8, 10 }
                });
		}

		[Test]
		public void NumberRange_LacingLongest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\NumberRange_LacingLongest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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

        [Test, Category("Failure")]
		public void NumberRange_LacingCrossProduct()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\NumberRange_LacingCrossProduct.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			var workspace = CurrentDynamoModel.CurrentWorkspace;
			Assert.AreEqual(6, workspace.Nodes.Count());
			Assert.AreEqual(5, workspace.Connectors.Count());

            var numberRange = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CoreNodeModels.Range>("4e781f03-5b48-4d58-a511-8c732665e961");

			var actual = numberRange.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var innerList1 = actual[0].GetElements().ToList();
			var innerList2 = actual[1].GetElements().ToList();
			var actualChild1 = innerList1[0].GetElements().ToList();
			var actualChild2 = innerList1[1].GetElements().ToList();
			var actualChild3 = innerList2[0].GetElements().ToList();
			var actualChild4 = innerList2[1].GetElements().ToList();

			Assert.AreEqual(2, actual.Count);
			Assert.IsNotNull(actualChild1);
			Assert.IsNotNull(actualChild2);
			Assert.IsNotNull(actualChild3);
			Assert.IsNotNull(actualChild4);

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
		public void ListMaximumMinimum_KeyTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ListMaximumMinimum_WithAndWithoutKey.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			// check that max & min nodes are loaded as DSFunction
			var maxNoKey = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("3db0d46d-fea6-4fc9-8c51-a8110f919c5f");
			var minNoKey = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("ef9a3ab0-b4c2-440d-9291-5807bc92e26f");
			var maxWithKey = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("a8ad0bfb-25f2-4ddc-aea6-927bdc739753");
			var minWithKey = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("2b2b1e9c-2ae1-4ba2-8b82-e01311df5429");

			// check that the nodes are migrated based on whether the key is connected or not
			Assert.AreEqual(1, maxNoKey.InPorts.Count);
			Assert.AreEqual(1, minNoKey.InPorts.Count);
			Assert.AreEqual(2, maxWithKey.InPorts.Count);
			Assert.AreEqual(2, minWithKey.InPorts.Count);
			
			// check output values
            Assert.AreEqual("eeee", maxNoKey.GetValue(0, CurrentDynamoModel.EngineController).Data);
            Assert.AreEqual("aaa", minNoKey.GetValue(0, CurrentDynamoModel.EngineController).Data);
            Assert.AreEqual("ccccc", maxWithKey.GetValue(0, CurrentDynamoModel.EngineController).Data);
            Assert.AreEqual("b", minWithKey.GetValue(0, CurrentDynamoModel.EngineController).Data);
		}

		[Test]
		public void ListMinimum_NumberRange()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ListMinimum_NumberRange.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6", -1);

		}

		[Test]
		public void ListMinimum_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ListMinimum_Complex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("aa8b8f1e-e8c4-4ced-bbb2-8ee43d7bb4f6", 5);

		}

		#endregion

		#region AddToList test cases -PartiallyDone

		[Test]
		public void AddToList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			var workspace = CurrentDynamoModel.CurrentWorkspace;
			Assert.AreEqual(8, workspace.Nodes.Count());
			Assert.AreEqual(8, workspace.Connectors.Count());

			var addToList = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("31d0eb4e-8657-4eb1-a852-5e9b766eddd7");
            var actual = addToList.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var childList = actual[2].GetElements().ToList();

			Assert.AreEqual(6, actual.Count);
			Assert.AreEqual("Design", actual[0].Data);
			Assert.AreEqual(10, actual[5].Data);

			Assert.IsNotNull(childList);
			Assert.AreEqual(4, childList.Count);
			Assert.AreEqual(-10, childList[0].Data);
		}
        
        [Test]
        public void AddToList_EmptyList()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_EmptyList.dyn");
            RunModel(openPath);

            AssertPreviewValue("1976caa7-d45e-4a44-9faf-345d98337bbb", new[] { new object[] { string.Empty, 0 } });
        }

        [Test]
        public void AddToList_Complex()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_Complex.dyn");
            RunModel(openPath);

            AssertPreviewValue(
                "cfdfc020-05d0-4442-96df-8d97aad9c38c",
                new[] { new[] { 3 }, new[] { 6 }, new[] { 9 } });
        }

		[Test]
		public void AddToList_GeometryToList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_GeometryToList.dyn");
			RunModel(openPath);

			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
			    {3, "Design"},
			};

			SelectivelyAssertPreviewValues("31d0eb4e-8657-4eb1-a852-5e9b766eddd7", validationData);
		}

		[Test]
		public void AddToList_Negative()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_Negative.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		[Test]
		public void AddToList_ContainingNull()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\AddToList_ContainingNull.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("9a187115-ba09-411e-a836-473aeec4493c", new object[] { 2, 3, 4, null, 6 });
		}

		#endregion

		#region SplitList test cases -PartiallyDone

		[Test]
		public void SplitList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\SplitList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", "Dynamo");

			AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", new int[][] { new int[] { 0, 1 } });
		}

		[Test]
		public void SplitList_FirstElementAsList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\SplitList_FirstElementAsList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", new object[] { "Dynamo" });

			AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", new int[] {0, 1});
		}

		[Test]
		public void SplitList_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\SplitList_Complex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9 + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(8 + 1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("abb3429a-1650-4e1e-a1fc-2ae237ad4f62", new int[] { 3 });

			AssertPreviewValue("223d2c7f-e56d-433a-aa14-7c53db009ce3", new int[][] { new int[] { 6 }, new int[] { 9 } });
		}

		[Test]
        [Category("Failure")]
		public void SplitList_ComplexAnotherExample()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\SplitList_ComplexAnotherExample.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(18, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(19, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			var guid = "66e94123-deaf-4bc8-8c5f-b3bc0996a57e";
			var splitList = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(guid);

            var output = splitList.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var firstOutput = output[0].GetElements().ToList();
			var secondOutput = output[1].GetElements().ToList();
			
			var child = secondOutput[0].GetElements().ToList();
			var child1 = secondOutput[1].GetElements().ToList();

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
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeFromList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			var takeFromList = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("14cb6593-24d8-4ffc-8ee5-9f4247449fc2");
            var firstOutput = takeFromList.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var child = firstOutput[0].GetElements().ToList();
			var child1 = firstOutput[4].GetElements().ToList();

			Assert.AreEqual(5, firstOutput.Count);

			Assert.AreEqual(1, child.Count);
			Assert.AreEqual(3, child[0].Data);

			Assert.AreEqual(1, child1.Count);
			Assert.AreEqual(15, child1[0].Data);
		}

		[Test]
		public void TakeFromList_WithStringList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeFromList_WithStringList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("14cb6593-24d8-4ffc-8ee5-9f4247449fc2", new string[] 
			{ 
				"Test", "Take", "From", "List" 
			});
		}

		[Test]
		public void TakeFromList_NegativeIntValue()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeFromList_NegativeIntValue.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("14cb6593-24d8-4ffc-8ee5-9f4247449fc2", new string[] { "List" });
		}

		[Test]
		public void TakeFromList_InputEmptyList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeFromList_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

		}

		[Test]
		public void TakeFromList_AmtAsRangeExpn()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeFromList_AmtAsRangeExpn.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		#endregion

		#region DropFromList test cases  
		[Test]
		public void DropFromList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\DropFromList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\DropFromList_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		#endregion

		#region ShiftListIndices test cases -PartiallyDone

		[Test]
		public void ShiftListIndices_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ShiftListIndeces_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{0,7},
				{4,1},
			};
			SelectivelyAssertPreviewValues("7f6cbd60-b9fb-4b16-81d3-4fab26790446", validationData);
		}

		[Test]
		public void ShiftListIndices_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ShiftListIndeces_Complex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			var workspace = CurrentDynamoModel.CurrentWorkspace;
			Assert.AreEqual(21, workspace.Nodes.Count());
			Assert.AreEqual(22, workspace.Connectors.Count());

			var guid = "492db019-4807-4810-8919-10b94e8ca083";
			var shiftListIndeces = workspace.NodeFromWorkspace<DSFunction>(guid);
            var output = shiftListIndeces.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var child = output[0].GetElements().ToList();
			var child1 = output[1].GetElements().ToList();

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
			string openPath = Path.Combine(TestDirectory, @"core\list\ShiftListIndeces_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("7f6cbd60-b9fb-4b16-81d3-4fab26790446", new int[] { });
		}

		[Test]
		public void ShiftListIndices_InputStringAsAmt()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ShiftListIndeces_InputStringAsAmt.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

		}

		[Test]
		public void ShiftListIndices_MultipleInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\ShiftListIndeces_MultipleInput.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("7f6cbd60-b9fb-4b16-81d3-4fab26790446",
				new object[][]
				{
					new object[] { 10, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
					new object[] { 9, 10, 1, 2, 3, 4, 5, 6, 7, 8 },
					new object[] { 8, 9, 10, 1, 2, 3, 4, 5, 6, 7 },
					new object[] { 7, 8, 9, 10, 1, 2, 3, 4, 5, 6 }
				});
		}
		#endregion

		#region GetFromList test cases PartiallyDone

		[Test]
		public void GetFromList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("332093dc-4551-4c82-9f6b-061c7945211b", new int[] { 9 });
		}

		[Test]
		public void GetFromList_WithStringList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_WithStringList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("58d35bfa-4435-44f0-a322-c6f7350f0220", new string[] { "Get", "From " });
		}

		[Test]
		public void GetFromList_AmtAsRangeExpn()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_AmtAsRangeExpn.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			var getFromList = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("d2f1c900-99ce-40a5-ae4d-bbac1fe96cfd");
            var output = getFromList.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(14, output[0].Data);
			Assert.AreEqual(2, output[1].Data);
			Assert.AreEqual(3, output[2].Data);

		}

		[Test]
		public void GetFromList_InputEmptyList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		[Test]
		public void GetFromList_Negative()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_Negative.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		[Test]
		public void GetFromList_NegativeIntValue()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\GetFromList_NegativeIntValue.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		#endregion

		#region TakeEveryNth test case  

		[Test]
		public void TakeEveryNth_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeEveryNth_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("b18e5ac3-5732-4c78-9a3b-56b375c9beee", new[] { 7, 10, 13, 16, 19 });
		}

		[Test]
		public void TakeEveryNth_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeEveryNth_Complex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(18, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(17, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("b18e5ac3-5732-4c78-9a3b-56b375c9beee", new[] { 1.0, 2.3 });
		}

		[Test]
		public void TakeEveryNth_InputEmptyList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeEveryNth_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("b18e5ac3-5732-4c78-9a3b-56b375c9beee", new int[] { });
		}

		[Test]
		public void TakeEveryNth_NegativeTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TakeEveryNth_NegativeTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			NodeModel nodeModel = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("b18e5ac3-5732-4c78-9a3b-56b375c9beee");
			Assert.AreEqual(ElementState.Warning, nodeModel.State);
		}
		#endregion

		#region DropEveryNth -PartiallyDone

		[Test]
		public void DropEveryNth_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\DropEveryNth_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("96a1ca07-83eb-4459-981e-7daed6d1d4b3", new int[] { 1, 2, 3, 4, 6, 7, 8, 9 });
		}

		[Test]
		public void DropEveryNth_ComplexTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\DropEveryNth_ComplexTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			var workspace = CurrentDynamoModel.CurrentWorkspace;
			Assert.AreEqual(19, workspace.Nodes.Count());
			Assert.AreEqual(20, workspace.Connectors.Count());

			var guid = "4bd0ced4-29ee-4f4e-95af-d0573e04731a";
			var takeEveryNth = workspace.NodeFromWorkspace<DSFunction>(guid);
            var output = takeEveryNth.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var child = output[0].GetElements().ToList();
			var child1 = output[1].GetElements().ToList();

			Assert.AreEqual(2, output.Count);

			Assert.AreEqual(12, child.Count);
			Assert.AreEqual("x", child[0].Data);

			Assert.AreEqual(12, child1.Count);
			Assert.AreEqual(32.85, child1[0].Data);
		}

		[Test]
		public void DropEveryNth_InputEmptyList()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\DropEveryNth_InputEmptyList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("a0304232-ad3a-4518-92ff-4b8893297ce4", new int[] { });
		}

		[Test]
		public void DropEveryNth_InputStringForNth()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\DropEveryNth_InputStringForNth.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}
		#endregion

		#region RemoveFromList test cases  

		[Test]
		public void RemoveFromList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\RemoveFromList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\RemoveFromList_StringAsList.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

		}

		[Test]
		public void RemoveFromList_StringAsIndex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\RemoveFromList_StringAsIndex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		[Test]
		public void RemoveFromList_Complex()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\RemoveFromList_Complex.dyn");
			RunModel(openPath);

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
			string openPath = Path.Combine(TestDirectory, @"core\list\RemoveFromList_RangeExpnAsIndex.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\SliceList_SimpleTest.dyn");
			RunModel(openPath);

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
			string openPath = Path.Combine(TestDirectory, @"core\list\SliceList_Complex.dyn");
			RunModel(openPath);
			
			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{0,"Design"},
				{1,4.5},
				{2,"Dynamo"},
			};
			SelectivelyAssertPreviewValues("cc3ae092-8644-4a36-ad38-12ffa15cebda", validationData);
		}

		[Test]
		public void SliceList_MultipleInput()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\SliceList_MultipleInput.dyn");
			RunModel(openPath);

			AssertPreviewValue("cc3ae092-8644-4a36-ad38-12ffa15cebda",
				new object[][]
				{
					new object[] { },
					new object[] { "Design", 4.5 },
					new object[] { "Design", 4.5, "Dynamo" }
				});
		}
		#endregion

		#region Test Average  

		[Test]
		public void Average_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Average_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("cafe8fae-55f0-4e58-977e-80edcbc90d2b", 6);
		}

		[Test]
		public void Average_NegativeInputTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\Average_NegativeInputTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		#endregion

		#region Test TrueForAny  

		[Test]
		public void TrueForAny_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TrueForAny_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			AssertPreviewValue("3960776a-4c6c-40d8-8b7e-dbe5db38d75b", true);
		}

		#endregion

		#region Test TrueForAll  

		[Test]
		public void TrueForAll_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\TrueForAll_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
			AssertPreviewValue("6434eb4f-89d9-4b11-8b9b-79ed937e4b24", 1);
		}

		#endregion

		#region Test Join List -PartiallyDone

		[Test]
		public void JoinList_SimpleTest()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\JoinList_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{0,10},
				{3,77.5},
				{5,"Dynamo"},
				{6,10},

			};
			SelectivelyAssertPreviewValues("1304807f-6d18-4aef-b4cb-9cb8f469993e", validationData);
		}

		[Test]
		public void JoinList_MoreLists()
		{
			string openPath = Path.Combine(TestDirectory, @"core\list\JoinList_MoreLists.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

			var joinList = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSVarArgFunction>("1304807f-6d18-4aef-b4cb-9cb8f469993e");
            var actual = joinList.GetValue(0, CurrentDynamoModel.EngineController).GetElements().ToList();
			var actualChild1 = actual[5].GetElements().ToList();
			var actualChild2 = actual[6].GetElements().ToList();

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Combine_SimpleTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Combine_ComplexTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(26, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(29, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

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
			string openPath = Path.Combine(TestDirectory, @"core\list\Combine_NegativeTest.dyn");
			RunModel(openPath);

			// check all the nodes and connectors are loaded
			Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
			Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
		}

		#endregion

		#region Test Create List
        [Test]
        [Category("Failure")]
		public void TestCreateList()
		{
			// Test partially applied Create List node.
			string openPath = Path.Combine(TestDirectory, @"core\list\createList.dyn");
			RunModel(openPath);

			AssertPreviewValue("0f306478-5a96-4276-baac-0d08e12fe872", new object[] { 1.0, 2.0, 3.0, 4.0 }); 
		}
		#endregion

		#region Test List.Replace
	    [Test]
	    public void TestListReplace()
	    {
	        var openPath = Path.Combine(TestDirectory, @"core\list\listreplace.dyn");
            RunModel(openPath);

            AssertPreviewValue("13f697db-85b8-4b93-859c-63f2b66c6b72", new object[] { 0.0, "no value", 2.0, "no value", "no value", 5.0 });
	    }
		#endregion

        #region Regressions
        [Test]
        public void RegressMagn4838_01()
        {
	        var openPath = Path.Combine(TestDirectory, @"core\list\RegressMagn4838_1.dyn");
            RunModel(openPath);

            AssertPreviewValue("8dd745bf-220c-49f7-80e0-3f1783bb33a4", new object[] { null });
        }

        [Test]
        public void RegressMagn4838_02()
        {
	        var openPath = Path.Combine(TestDirectory, @"core\list\RegressMagn4838_2.dyn");
            RunModel(openPath);
        }

        [Test]
        public void TestFilterOnEmptyList()
        {
	        var openPath = Path.Combine(TestDirectory, @"core\list\testFilterOnEmptyInput.dyn");
            RunModel(openPath);
            AssertPreviewValue("068bec9b-6da0-4379-af7c-5062f4fb0f92", new object[] { });
            AssertPreviewValue("fe77da6f-71db-4712-bffb-27d5acc86e0b", new object[] { });
        }
        #endregion

        [Test]
        public void FirstIndexOf()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\FirstIndexOf.dyn");
            RunModel(openPath);
            AssertPreviewValue("04a0347f-b931-40ff-81c9-7c0e5c61d051", 0); 
        }

        [Test]
        public void AllIndicesOf()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\AllIndicesOf.dyn");
            RunModel(openPath);
            AssertPreviewValue("404af9cd-3668-4aa8-aea1-314a228bd6e1", new object[] { 0, 2 });
        }

        [Test]
        [Category("RegressionTests")]
        public void TestListChop()
        {
            string openPath = Path.Combine(TestDirectory, @"core\list\testListChop.dyn");
            RunModel(openPath);
            AssertPreviewValue("f805b9b9-1a1f-4a63-ad9e-e4c9722ef1c7", 
                new object[] {
                    new object[] {1, 2 },
                    new object[] {3, 4 },
                    new object[] {5 } });
        }
    }
}

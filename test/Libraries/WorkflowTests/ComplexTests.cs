using System.IO;

using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.DesignScript.Geometry;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ComplexTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");  // Required for Watch node.
            libraries.Add("ProtoGeometry.dll"); // Required for Surface.
            libraries.Add("DSCoreNodes.dll");   // Required for built-in nodes.
            libraries.Add("DSIronPython.dll");  // Required for Python tests.
            libraries.Add("FunctionObject.ds"); // Required for partially applied nodes.
            base.GetLibrariesToPreload(libraries);
            
        }

        // Note: Only add test cases those are related to ASM Geometry.
        // Always create a region while creating tests for new Geometry type.

        #region Solid Tests
        [Test]
        public void TestSolidSweep()
        {
            // This will test user workflow which contains many nodes, final output is Solid using
            // sweep.
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\RandomModel_V3.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            AssertNoDummyNodes();

            var solidNodeId = "ff0b24b2-73e5-4da6-8081-a1883ff9ad72";
            AssertPreviewCount(solidNodeId, 66);

            // output will be 66 Solids, so putting verification for all Solid creation
            for (int i = 0; i <= 65; i++)
            {
                var sweptSolid = GetPreviewValueAtIndex(solidNodeId, i) as Solid;
                Assert.IsNotNull(sweptSolid);
            }
        }


        [Test]
        public void ListLacing()
        {
            // Create automation for Dynamo file: 02 List Lacing.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\02 List Lacing.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            // check list lacing 
            var shortesListLacing = "e93fee37-1901-4162-8f73-6b5e98c1167f";
            AssertPreviewCount(shortesListLacing, 6);
            var longestListLacing = "de2b1391-95e0-4b3d-b7f0-43d03d5c5b5a";
            AssertPreviewCount(longestListLacing, 10);
            var crossProduct = "c16658b3-42dc-4a45-b58a-10e3f6bb2f67";
            AssertPreviewCount(crossProduct, 10);
        }


        [Test]
        public void AttractorPoint()
        {
            // Create automation for Dynamo file : 03 Attractor Point.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\03 Attractor Point.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();
            // check Point.X
            var pointX = "d16627d1-695f-4997-a7d2-3f8754f19589";
            AssertPreviewCount(pointX, 100);

            // check Cylinder.ByPointsRadius
            var cylinders = "ef3eaed0-7a8e-47a9-b06e-416bb30ec72f";
            AssertPreviewCount(cylinders, 100);
            for (int i = 0; i < 100; i++)
            {
                var cylinder = GetPreviewValueAtIndex(cylinders, i) as Cylinder;
                Assert.IsNotNull(cylinder);
            }
        }


        [Test]
        public void RangeSyntax()
        {
            // Create automation for Dynamo file : 04 RnageSyntax.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\04 Range Syntax.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            // check the preview value of Number Range node
            var numberRange = "5892dcad-9e46-46b5-97f2-5e18e17ca7db";
            AssertPreviewCount(numberRange, 6);
            AssertPreviewValue(numberRange, new double[] { 0, 2, 4, 6, 8, 10 });

            // check the preview value of Number Sequence node
            var numberSequence = "e29704c3-dbc2-4eb8-bd2c-f671c4629051";
            AssertPreviewValue(numberSequence, new object[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 });

            // check the previe value of CBNS
            var cbn1 = "b00ccf43-2f2e-49fe-aa81-d072a3598834";
            AssertPreviewValue(cbn1, new object[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 });
            var cbn2 = "a6e062d2-abba-427f-84a6-e2492db791f2";
            AssertPreviewValue(cbn2, new object[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27 });
            var cbn3 = "a9250aa2-f84e-4ae6-a2dc-1dedba3371ec";
            AssertPreviewValue(cbn3, new object[] { 0, 5, 10 });
            var cbn4 = "99552b5c-caf6-4ae4-85ab-9e8660643726";
            AssertPreviewValue(cbn4, new object[] { 0, 3, 6, 9, 12 });

        }


        [Test]
        public void WriteToExcel()
        {
            // Create automation for Dynamo file : 05 Write to Excel.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\05 Write To Excel.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check List.Transpose
            var listTranspose = "53ee9988-6ed7-497f-92d8-2132f1a76ae3";
            AssertPreviewCount(listTranspose, 65);
        }


        [Test]
        public void ReadFromCSV()
        {
            // Create automation for Dynamo file : 06 Read from CSV.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\06 Read from CSV.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check Points
            var points = "8980c6be-1e52-43ee-86aa-45c467ca5530";
            AssertPreviewCount(points, 65);
            for (int i = 0; i < 65; i++)
            {
                var point = GetPreviewValueAtIndex(points, i) as Point;
                Assert.IsNotNull(point);
            }
        }


        [Test]
        public void PassingFunctions()
        {
            // Create automation for Dynamo file : 07 Passing Functions.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\07 Passing Functions.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(16, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(17, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //Check curve.Extrude that linked with CBN which has value 3
            var curveExtrude = "2abb7d97-6b23-4b26-91af-c11407503a66";
            AssertPreviewCount(curveExtrude, 2);
            //flat the two dimensional list
            var flat = GetFlattenedPreviewValues(curveExtrude);
            Assert.AreEqual(flat.Count, 10);
        }


        [Test]
        public void Math()
        {
            // Create automation for Dynamo file : 08 Math.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\08 Math.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(201, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(199, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check minus -
            string minus = "7355f590-ddb8-45d2-b6fb-155f3f3c4f00";
            AssertPreviewValue(minus, 3);

            //check formula
            string formula = "526a2d12-1976-47b0-9c11-dcffe8ac01ee";
            AssertPreviewValue(formula, 3);

            //check remainder
            string remainder = "5ae3c356-3e83-43a9-a126-95b82cdcacc1";
            AssertPreviewValue(remainder, 1);

            //check Surface.Byloft
            var surface = "019f2710-869d-4f26-ac77-3e8332b78bb6";
            var value = GetPreviewValue(surface) as Surface;
            Assert.IsNotNull(value);

            //check NurbsCurve value inside CodeBlockNode
            var nurb = "288d67b1-9d89-48bd-9dcc-15f7f443b09c";
            Assert.IsNotNull(GetPreviewValue(nurb) as NurbsCurve);

            //check Line.ByStartPointEndPoint
            var line = "ccf76cd8-73c3-486a-a4e2-9493c9bc1f3f";
            Assert.IsNotNull(GetPreviewValue(line) as Line);
        }


        [Test]
        public void Combine()
        {
            // Create automation for Dynamo file : Combine.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Combine.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(44, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(36, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check List.Map
            var map1 = "34986114-3561-4feb-993b-3c53c9ef352f";
            AssertPreviewCount(map1, 15);
            for (int i = 0; i < 15; i++)
            {
                var nurbscurve = GetPreviewValueAtIndex(map1, i) as NurbsCurve;
                Assert.IsNotNull(nurbscurve);
            }
            var map2 = "7ec7b343-6e0c-46ba-b89a-7df17e6a0833";
            for (int i = 0; i < 15; i++)
            {
                var point = GetPreviewValueAtIndex(map2, i) as Point;
                Assert.IsNotNull(point);
                Assert.AreEqual(point.X, 0);
            }

            //check List.Combine
            var combine = "ce5a6237-f69b-4a84-9838-245272c8fccf";
            AssertPreviewCount(combine, 15);
            var flatCombine = GetFlattenedPreviewValues(combine);
            foreach (var element in flatCombine)
            {
                Assert.IsNotNull(element);
            }

            // check Surface.ByLoft
            var surface = "cd8135dc-2d29-4466-bc0d-476016f811cb";
            AssertPreviewCount(surface, 15);
            for (int i = 0; i < 15; i++)
            {
                var sur = GetPreviewValueAtIndex(surface, i) as Surface;
                Assert.IsNotNull(sur);
            }
        }


        [Test]
        public void Count()
        {
            // Create automation for Dynamo file : Count.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Count.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Count
            var count = "6e2bc5c1-a774-4c54-b7e4-9f1dc87360d1";
            AssertPreviewValue(count, 43);

            // check List.Combine
            var combine = "2595c54d-2d72-4050-93ff-457ad3003bab";
            for (int i = 0; i < 43; i++)
            {
                var value = GetPreviewValueAtIndex(combine, i) as string;
                Assert.AreEqual(value, "foo" + i.ToString());
            }
        }


        [Test]
        public void Flatten()
        {
            // Create automation for Dynamo file : Flatten.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Flatten.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Flatten
            var count = "9348354d-1aae-49bf-b1cd-33f650dd42dd";
            AssertPreviewCount(count, 18);
            int[] list = new int[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5 };
            AssertPreviewValue(count, list);
        }


        [Test]
        public void CreateList()
        {
            // Create automation for Dynamo file : CreateList.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\CreateList.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check List.Create
            var count = "cd7d56c0-3e0d-46c7-9dad-777008030045";
            AssertPreviewCount(count, 3);
            var flatList = GetFlattenedPreviewValues(count);
            Assert.AreEqual(flatList.Count, 4);
            flatList[0].ToString().Equals("42");
            flatList[1].ToString().Equals("The answer to everything");
            flatList[2].ToString().Equals("foo");
            flatList[3].ToString().Equals("bar");
        }


        [Test]
        public void IndexOutsideBounds_3399()
        {
            // This will test user workflow which contains many nodes.
            // Crash with "Index was outside the bounds of the array"

            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\20140418_buildingSetback_standalone.dyn");

            var FARId = "c03065ec-fe54-40de-8c27-8089c7fe1b73";
            Assert.DoesNotThrow(() => RunModel(openPath));

        }
        #endregion


        //add test cases that are related to CodeBlocks.
        #region CodeBlocks test
        [Test]
        public void Test_CodeBlocksReference()
        {
            // Create automation for Dynamo file : Code Blocks Reference.dyn
            // This is a training file for demonstrating capabilities of Codeblock
            // To test various types supported in codeblock.  
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\04 Code blocks\Code Blocks Reference.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(68, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(89, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of CBNs
            var cbn = "27c2f333-f51f-4a0c-9f71-70dc64f2ecbe";
            AssertPreviewValue(cbn, 3.142);
            var strCbn = "4c8ddee8-e2b1-4472-9470-a4142f56ac97";
            string str = "Less is more.";
            AssertPreviewValue(strCbn, str);
            var pointCbn = "4ea1d0d8-4882-4f6f-b659-2dcb297db34e";
            var x = GetPreviewValue(pointCbn) as Point;
            Assert.AreEqual(0, x.X);
            Assert.AreEqual(0, x.Y);
            Assert.AreEqual(0, x.Z);
            var ptCBN = "613c6d53-0eab-414f-9aeb-fc7c56de6900";
            AssertPreviewValue(ptCBN, 10);
            
            //check preview values of code block nodes
            var CBN1 = "34a4b2fc-a1c5-4c18-a0fe-48b1c3b8f711";
            var CBN1Val = GetPreviewValue(CBN1) as Point;
            var CBN2 = "a1142e89-89e4-4fa2-a63d-d58489af044d";
            var CBN2Val = GetPreviewValue(CBN2);
            Assert.AreEqual(CBN1Val, CBN2Val);

            //check preview value of code block node, which contains a list
            var listCBN = "4c049b29-2792-4a66-ab89-d8fac3161e7d";
            AssertPreviewCount(listCBN, 4);

            //check preview value of CBN, which is a list and not null
            var notNullCBN = "d71e5f04-9928-437a-863c-5bf34666370b";
            AssertPreviewCount(notNullCBN, 5);
            for (int i = 0; i < 5; i++)
            {
                var element = GetPreviewValueAtIndex(notNullCBN, i) as string;
                Assert.IsNotNull(element);
            }

            //check the preview value of CBN, which value is null
            var nullCBN = "f973a3ec-70ee-475b-ad7d-50e5f21c1e04";
            AssertPreviewValue(nullCBN, null);

            //check the CBN which contains a list of nurbsCurve
            var nurbCBN = "8dce8e89-1d9d-4ffe-b6ea-e7251ab1368b";
            AssertPreviewCount(nurbCBN, 24);
            for (int i = 0; i<24; i++)
            {
                var nurbCBNVal = GetPreviewValueAtIndex(nurbCBN, i) as NurbsCurve;
                Assert.IsNotNull(nurbCBNVal);
            }
            
            //check function call in CBN
            var func = "cba93904-d0da-4452-9864-0d1c02706e95";
            AssertPreviewValue(func, 3);
            var funcStr = "9256e209-fd59-492c-81c8-65cb84552ef5";
            AssertPreviewValue(funcStr, "catdog");

            //check point.Add
            var pAdd = "9aa85384-3767-46da-b524-e0b969d9420a";
            var pValue = GetPreviewValue(pAdd) as Point;
            Assert.IsNotNull(GetPreviewValue(pAdd) as Point);
            Assert.AreEqual(pValue.X, 0);
            Assert.AreEqual(pValue.Y, 0);
            Assert.AreEqual(pValue.Z, 1);

            
        }
        #endregion

        //add test cases that are related to Geometry.
        #region Geometry test
        [Test]
        public void Test_Basket1()
        {
            // Create automation for Dynamo file : Basket1.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\05 Geometry\Basket1.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(31, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(25, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check Geometry.Rotate
            var rotate = "dfdfa7b1-533b-4dca-afa2-1d9d62233b7f";
            AssertPreviewCount(rotate, 12);
            for (int i = 0; i < 12; i++)
            {
                var element = GetPreviewValueAtIndex(rotate, i) as Solid;
                Assert.IsNotNull(element);
            }
        }


        [Test]
        public void Test_Basket2()
        {
            // Create automation for Dynamo file : Basket2.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\05 Geometry\Basket2.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(53, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(45, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check Curve.ExtrudeAsSolid
            var solid = "9349f9d5-aa7c-43d9-a336-5610004ed7ef";
            AssertPreviewCount(solid, 63);
            for (int i = 0; i < 63; i++)
            {
                var element = GetPreviewValueAtIndex(solid, i) as Solid;
                Assert.IsNotNull(element);
            }
        }


        [Test]
        public void Test_Basket3()
        {
            // Create automation for Dynamo file : Basket3.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\05 Geometry\Basket3.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(199, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(141, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            // check Surface.Thicken
            var solid1 = "db78b128-fec5-40b9-9e59-0ca371a4de43";
            AssertPreviewCount(solid1, 4);
            for (int i = 0; i < 4; i++)
            {
                var element = GetPreviewValueAtIndex(solid1, i) as Solid;
                Assert.IsNotNull(element);
            }

            // check Surface.Thicken
            var solid2 = "bd7ffff8-b6dd-4a01-8caf-9a59682a3288";
            AssertPreviewCount(solid2, 5);
            for (int i = 0; i < 5; i++)
            {
                var element = GetPreviewValueAtIndex(solid2, i) as Solid;
                Assert.IsNotNull(element);
            }
        }
        #endregion

        //add test cases that are related to Python.
        #region Python test
        [Test]
        public void Test_Core_Python()
        {
            // Create automation for Dynamo file : Core_Python.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\06 Python\Core_Python.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check List.GetItemAtIndex
            var getItemAtIndex1 = "b1be50ed-f93c-4d9c-b7c4-55d97e820672";
            AssertPreviewValue(getItemAtIndex1, 55);   

            //check List.GetItemAtIndex
            var getItemAtIndex2 = "e14a4953-9115-4344-ac65-e7243e4975e3";
            AssertPreviewValue(getItemAtIndex2, new int[] { 0, 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 });    
        }
        #endregion

        #region File test

        [Test]
        public void Test_File_Migration()
        {
            string directory = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\File Migration");
            string openPath = Path.Combine(directory,"File.dyn");
            OpenModel(openPath);

            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, workspace.Connectors.Count());
            Assert.AreEqual(11, workspace.Nodes.Count);

            // give absolute path
            var textFileName = workspace.NodeFromWorkspace<DSCore.File.Filename>("545b092b-8b2c-4cd4-b15e-9e4162dd4579");
            textFileName.Value = Path.Combine(directory, textFileName.Value);

            textFileName = workspace.NodeFromWorkspace<DSCore.File.Filename>("23f8f80b-eca7-4651-a082-7e2cce93c88d");
            textFileName.Value = Path.Combine(directory, textFileName.Value);

            var imgFileName = workspace.NodeFromWorkspace<DSCore.File.Filename>("c20d8f16-4bf7-465f-93ca-6afd05fe02a2");
            imgFileName.Value = Path.Combine(directory, imgFileName.Value);

            var imgDirectory = workspace.NodeFromWorkspace<DSCore.File.Directory>("729b5ffd-7813-4e09-94a2-74a1e4619c5f");
            imgDirectory.Value = directory;

            var xlsxFileName = workspace.NodeFromWorkspace<DSCore.File.Filename>("b8f8f48b-c546-4ae8-b7f6-45e52310e361");
            xlsxFileName.Value = Path.Combine(directory, xlsxFileName.Value);

            // run the graph
            BeginRun();

            // test reading
            var readText = "b6e77130-3c2e-4d6b-ae5b-2137c3d3a51b";
            var text = GetPreviewValue(readText) as string;
            Assert.AreEqual(text,"1234");

            // test writting. Write text is not obsolete but the
            // namespace needs to be migrated, no warning should
            // be shown in this node
            var writeText = "ac1e30c2-27c5-46ef-88c9-52f0a2c0d1d9";
            var nodeModel = workspace.NodeFromWorkspace(writeText);
            Assert.AreEqual(ElementState.Active, nodeModel.State);

            // test reading image. There are 100 Colors object that is generated since 
            // the sampling in x and y direction is 10.
            var readImage = "7fba95f1-fd7a-4033-a7a9-5dfb91c7e886";
            Assert.AreEqual(100,GetFlattenedPreviewValues(readImage).Count);

            // test load image from path and writing image in a CBN
            var codeBlock = "75b9d0a3-e954-42b5-8ccf-66845b122e3f";
            AssertPreviewValue(codeBlock,true);

            // text writing to csv, persistent warning should be shown
            // since the node is obsolete
            var writeCSV = "3ddc75fb-e607-4932-8e08-f215ee86211e";
            nodeModel = workspace.NodeFromWorkspace(writeCSV);
            Assert.AreEqual(ElementState.PersistentWarning, nodeModel.State);
        }

        #endregion

        [Test]
        public void Test_Lacing()
        {
            // Create automation for Dynamo file : Lacing.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Lacing.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check List.ByStartPointEndPoint
            var list = "5c02325a-f5fc-403c-b46f-0a492cbce5cf";
            AssertPreviewCount(list, 4);
            for (int i = 0; i < 4; i++)
            {
                var line = GetPreviewValueAtIndex(list, i) as Line;
                Assert.IsNotNull(line);
            }
        }


        [Test]
        public void Test_Map()
        {
            // Create automation for Dynamo file : Map.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Map.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            // check the number of nodes and connectors
            Assert.AreEqual(32, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(26, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check List.Map
            var list = "34986114-3561-4feb-993b-3c53c9ef352f";
            AssertPreviewCount(list, 36);
            for (int i = 0; i < 36; i++)
            {
                var nurb = GetPreviewValueAtIndex(list, i) as NurbsCurve;
                Assert.IsNotNull(nurb);
            }
        }


        [Test]
        public void Test_MinMax()
        {
            // Create automation for Dynamo file : MinMax.dyn
            // This is a training file for demonstrating List.MinimumItem and List.MaximumItem
            // To test various types supported in List.  
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\MinMax.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of CBNs
            var cbn = "1db3330e-bd1b-4116-b5ca-df4a18e78960";
            AssertPreviewValue(cbn, new string[]{"Tywin","Cersei","Hodor","Tyrian"});
           
            //check preview value of List.MaximumItem
            var maxNumber = "9e48b2a9-82fe-445f-bfb1-cee9d6e591cd";
            AssertPreviewValue(maxNumber, 42);
          
            //check preview value of List.MinmumItem
            var minNumber = "93a0599d-2456-41e9-a36b-3022cf9c733b";
            AssertPreviewValue(minNumber, 0);

            //check preview value of List.MaximumItem
            var maxString = "c4f3d21a-aed2-4ebd-aaf6-a49358174544";
            AssertPreviewValue(maxString, "Tywin");

            //check preview value of List.MinmumItem
            var minString = "b9a7de54-e4b6-4fcc-acfb-30331b0e519a";           
            AssertPreviewValue(minString, "Cersei");

            //check List.Create
            var listNumber = "259de8c8-25b8-4441-b3fd-c2d1caa5a2ab";
            var flatListNumber = GetFlattenedPreviewValues(listNumber);
            flatListNumber[0].ToString().Equals("0");
            flatListNumber[1].ToString().Equals("42");

            //check List.Create
            var listString = "d6813751-ae9b-4866-b1ab-8062d3062843";
            var flatListString = GetFlattenedPreviewValues(listString);
            Assert.AreEqual(flatListString, new string[] { "Cersei", "Tywin" });          
        }


        [Test]
        public void Test_PassingFunctions()
        {
            // Create automation for Dynamo file : PassingFunctions.dyn
            // This is a training file for demonstrating surface   
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\PassingFunctions.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(16, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(17, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of Curve.Extrude
            var curve = "2abb7d97-6b23-4b26-91af-c11407503a66";
            AssertPreviewCount(curve, 2);
            var surfaces = GetFlattenedPreviewValues(curve);
            foreach (var element in surfaces)
            {
                Assert.IsNotNull(element);
            }

            //check preview values of Curve.Extrude
            var curve2 = "44b197d9-25e2-4afe-96d2-badc82b0e37d";
            AssertPreviewCount(curve2, 8);
            var surfaces2 = GetFlattenedPreviewValues(curve2);
            foreach (var element in surfaces2)
            {
                Assert.IsNotNull(element);
            }
        }


        [Test]
        public void Test_Reverse()
        {
            // Create automation for Dynamo file : Reverse.dyn
            // This is a training file for demonstrating reverse functions   
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Reverse.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of List.Map, List.Reverse
            var map = "45ea27b6-a33c-429d-bf20-6d147d3c5893";
            var flatMap = GetFlattenedPreviewValues(map);
            var reverse = "10be37ae-4a54-49a2-aaf3-60c7c14ddac5";
            var flatReverse = GetFlattenedPreviewValues(reverse);
            var size = (flatMap.Count)/2;
            for (int i = 0; i <= size; i++)
            {
                Assert.AreEqual(flatMap[i], flatReverse[size * 2 - 1 - i]);
            }

            //check preview value of List.Reverse
            var listreverse = "4b56a76f-01a1-48e2-8f33-256219f9ddc7";
            AssertPreviewValue(listreverse, new int[] { 5, 4, 3, 2, 1, 0 });
        }


        [Test]
        public void Test_Sort()
        {
            // Create automation for Dynamo file : Sort.dyn
            // This is a training file for demonstrating sort function  
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Sort.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of List.Sort
            var sort = "e537e6b8-aa95-42ec-b33b-95cda3d1f20e";
            var flat = GetFlattenedPreviewValues(sort);
            Assert.AreEqual(flat, new object[] { -2, 42, "cat", "cheese", "dog" });         
        }


        [Test]
        public void Test_SortGeometry()
        {
            // Create automation for Dynamo file : SortGeometry.dyn
            // This is a training file for demonstrating sort function 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\SortGeometry.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check preview values of PolyCurve.ByPoint
            var sort = "7c636447-6099-48ab-8a1c-758ee8feecaf";
            Assert.IsNotNull(GetPreviewValue(sort) as PolyCurve);

            //check preview vlaues of SortByKey
            var sortByKey = "29df5b54-4e2c-4f5d-8584-bd59df05e2e8";
            for (int i = 0; i < 16; i++)
            {
                var point = GetPreviewValueAtIndex(sortByKey, i) as Point;
                Assert.IsNotNull(point);
            }
        }


        [Test]
        public void Test_Surface()
        {
            // Create automation for Dynamo file : Surface.dyn
            // This is a training file for demonstrating surface
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Surface.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(26, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(21, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check CBN
            var cbn = "9ab004ca-0917-4858-85bc-57ae681e33e8";
            for (int i = 0; i < 3; i++)
            {
                Assert.IsNotNull(GetPreviewValueAtIndex(cbn,i) as NurbsCurve);
            }          
            //check Surface.ByLoft
            var surface = "b5cb4be4-d90d-4783-8b0b-c44c6e18b327";           
            Assert.IsNotNull(GetPreviewValue(surface) as Surface);          
        }


        [Test]
        public void Test_Transpose()
        {
            // Create automation for Dynamo file : Transpose.dyn
            // This is a training file for demonstrating transpose function
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Transpose.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check List.Transpose
            var transpose = "5fd2a365-4831-4400-abbe-b8bc04cdfe7a";
            var flattranspose = GetFlattenedPreviewValues(transpose);
            Assert.AreEqual(flattranspose.Count, 100);
            for (int i = 0; i < 100; i++)
            {
                var point = flattranspose[i] as Point;
                Assert.IsNotNull(point);
            }

            //check the number of nodes and connectors
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Point.ByCoordinates
            var points = "5462da5b-1473-44a6-a3f8-9f5098b4675a";
            var flatpoints = GetFlattenedPreviewValues(points);
            Assert.AreEqual(flatpoints.Count, 100);
            for (int i = 0; i < 100; i++) 
            {
                var point = flatpoints[i] as Point;
                Assert.IsNotNull(point);
            }     
            
            //compare Point.ByCoordinates and List.Transpose
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var point1 = flattranspose[i * 10 + j] as Point;
                    var point2 = flatpoints[i + j * 10] as Point;
                    Assert.AreEqual(point1.ToString(), point2.ToString());
                }
            }
        }


        [Test]
        public void Test_buckyballInCodeBlock()
        {
            //Create automation for Dynamo file : buckyball in a code block.dyn
            //This is a training file for demostrating code block function
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\MiscDefinitions\buckyball in a code block.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check CBN that contains 84 lines
            var lineID = "93a30c71-6a3d-4cf3-8140-b5acc1d33cd6";
            AssertPreviewCount(lineID, 84);
            for (int i = 0; i < 84; i++)
            {
                var line = GetPreviewValueAtIndex(lineID, i) as Line;
                Assert.IsNotNull(line);
            }

            //check CBN tht contains lists of points
            var p1ID = "d1069b0a-eee2-4784-8793-bbbf5791f52f";
            var p1List = GetFlattenedPreviewValues(p1ID);
            Assert.AreEqual(p1List.Count, 4);
            var point0 = p1List[0] as Point;
            var point1 = p1List[1] as Point;
            
            Assert.AreEqual(point0.X, -point1.X);
            Assert.AreEqual(System.Math.Floor(point0.Y), 0);
            Assert.AreEqual(System.Math.Floor(point0.Z), -2);
            Assert.AreEqual(p1List[2].ToString(), "Point(X = -7.835, Y = 0.000, Z = 1.614)");
            Assert.AreEqual(p1List[3].ToString(), "Point(X = 7.835, Y = 0.000, Z = 1.614)");      
        }


        [Test]
        public void Test_SmileyFace()
        {
            //Create automation for Dynamo file : SmileyFace.dyn
            //This is a training file for demostrating cylinder,solid
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\MiscDefinitions\SmileyFace.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(40, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(25, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Cylinder.ByPointsRadius
            var lineID = "c8aaaf1b-975a-4075-99e0-fb0092a232fb";
            Assert.IsNotNull(lineID);
            var cylinder = GetPreviewValue(lineID);
            Assert.AreEqual(cylinder.ToString(), "Cylinder(Radius = 6.000)");
            
            //check CBN which contain a function
            var cbnId = "18e3cdff-8932-4e31-ae82-98c3beee8b08";
            var cbnValue = GetPreviewValue(cbnId) as Point;
            Assert.AreEqual(cbnValue.ToString(), "Point(X = 10.000, Y = -19.972, Z = 33.511)");
            
            // check Solid.UnionAll
            var solidId = "0150d478-21a8-472d-a444-6976a3f1079b";
            var solidValue = GetPreviewValue(solidId) as Solid;
            Assert.IsNotNull(solidValue);
            Assert.AreEqual(solidValue.ToString(), "Solid");
        }


        [Test]
        public void Test_WovenSurface()
        {
            //Create automation for Dynamo file : Woven Surface.dyn
            //This is a training file for demostrating surface, CBN with function
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\MiscDefinitions\Woven Surface.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(64, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(40, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Surface.Thicken
            var surfaceID = "a7b4e678-3278-4554-8ce2-7c76faca79d7";
            AssertPreviewCount(surfaceID, 4);
            for (int i = 0; i < 4; i++)
            {
                var solid = GetPreviewValueAtIndex(surfaceID, i) as Solid;
                Assert.AreEqual(solid.ToString(), "Solid");
            }
           
            //check CBN which contain a function
            var cbnId = "73aa7872-fd75-4542-9270-80daa02de33f";
            AssertPreviewValue(cbnId,new object[]{0.125,0.375,0.625,0.875});        
        }



        [Test]
        public void Test_Mobius()
        {
            //Create automation for Dynamo file : mobius.dyn
            //This is a training file for demostrating surfaces
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\MiscDefinitions\mobius.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(38, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(29, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Surface.ByLoft
            var surface1ID = "3675e40d-1d1d-4869-b3e1-f8ea67286486";
            var surface1 = GetPreviewValue(surface1ID) as Surface;
            Assert.AreEqual(surface1.ToString(), "Surface");
            var surface2ID = "bca2356e-9225-43d1-a4c1-79f9b9c7e52d";
            var surface2 = GetPreviewValue(surface2ID) as Surface;
            Assert.AreEqual(surface2.ToString(), "Surface");
          
            //check CBN, which includes a list of Vector
            var cbnId = "e14439c9-b377-4653-bb63-4edd9a4f90a0";
            AssertPreviewCount(cbnId, 18);
            for (int i = 0; i < 18; i++)
            {
                var vector = GetPreviewValueAtIndex(cbnId, i) as Vector;
                Assert.IsNotNull(vector);
            }

            //check List.Sublist
            var listID = "07ad5b99-cbb0-481c-b1cc-0b569a88ff2e";
            var flatList = GetFlattenedPreviewValues(listID);
            Assert.AreEqual(flatList.Count, 19);
            foreach (var sublist in flatList)
            {
                Assert.IsNotNull(sublist);
                sublist.GetType().ToString().Equals("Point");
            }
               
        }


        #region Vignette Test
        [Test]
        public void Test_Vignette_01Plane_Offset()
        {
            //Create automation for Dynamo file : Vignette-01-Plane-Offset.dyn
            //This is a training file for demostrating Plane and Rectangle
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\Vignette\Vignette-01-Plane-Offset.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Plane.XY
            var planeID = "80f6e70a-b6e4-452e-9ec3-dffa1106ee28";
            var plane = GetPreviewValue(planeID) as Plane;
            Assert.IsNotNull(plane);
            Assert.AreEqual(plane.ToString(), "Plane(Origin = Point(X = 0.000, Y = 0.000, Z = 0.000), Normal = Vector(X = 0.000, Y = 0.000, Z = 1.000, Length = 1.000), XAxis = Vector(X = 1.000, Y = 0.000, Z = 0.000, Length = 1.000), YAxis = Vector(X = 0.000, Y = 1.000, Z = 0.000, Length = 1.000))");

            // check Plane.Offset
            var planeOffsetID = "a0db9245-1b0d-4ea2-a125-ec63c69705d5";
            var planeOffset = GetPreviewValue(planeOffsetID) as Plane;
            Assert.IsNotNull(plane);
            Assert.AreEqual(planeOffset.ToString(), "Plane(Origin = Point(X = 0.000, Y = 0.000, Z = 20.000), Normal = Vector(X = 0.000, Y = 0.000, Z = 1.000, Length = 1.000), XAxis = Vector(X = 1.000, Y = 0.000, Z = 0.000, Length = 1.000), YAxis = Vector(X = 0.000, Y = 1.000, Z = 0.000, Length = 1.000))");

            //check Rectangle.ByWidthHeight
            var rectangle1ID = "49f7b7e2-2e41-463f-9a2e-c77d902d5d97";
            var rectangle2ID = "2e436c2f-88b6-461f-9208-9852ccf6e731";
            var rectangle1 = GetPreviewValue(rectangle1ID) as Rectangle;
            var rectangle2 = GetPreviewValue(rectangle2ID) as Rectangle;
            Assert.AreEqual(rectangle1.ToString(), rectangle2.ToString());  
        }


        [Test]
        public void Test_Vignette_01WireFrame_Section()
        {
            //Create automation for Dynamo file : Vignette-01-Wireframe-Section.dyn
            //This is a training file for demostrating Lines and Rectangles
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\Vignette\Vignette-01-Wireframe-Section.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(48, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(33, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            //check Line.ByStartPointEndPoint
            var lineID = "ead8e061-d570-436b-aece-c66c2cd02326";
            var line = GetPreviewValueAtIndex(lineID, 0);
            Assert.IsNotNull(line);
            Assert.AreEqual(line.ToString(), "Line(StartPoint = Point(X = 7.967, Y = 0.000, Z = 18.800), EndPoint = Point(X = -7.967, Y = 0.000, Z = 18.800), Direction = Vector(X = -15.934, Y = 0.000, Z = 0.000, Length = 15.934))");

            // check Line.ByStartPointEndPoint
            var line2ID = "75480d24-9c8d-46ba-991b-e983cd0d58d3";
            var line2 = GetPreviewValueAtIndex(line2ID, 0);
            Assert.IsNotNull(line2);
            Assert.AreEqual(line2.ToString(), "Line(StartPoint = Point(X = 12.713, Y = 0.000, Z = 30.000), EndPoint = Point(X = -12.713, Y = 0.000, Z = 30.000), Direction = Vector(X = -25.426, Y = 0.000, Z = 0.000, Length = 25.426))");

            //check Rectangular.ByWidthHeight
            var rectangular1ID = "bc931a2b-bfed-4953-890a-81e672631348";
            var rectangular1 = GetPreviewValue(rectangular1ID) as Rectangle;
            Assert.IsNotNull(rectangular1);
   
            // check Rectangular.ByWidthHeight
            var rectangular2ID = "b80fd260-243f-4475-96ec-5542ff0f1d75";
            var rectangular2 = GetPreviewValue(rectangular2ID) as Rectangle;
            Assert.AreEqual(rectangular1.ToString(), rectangular2.ToString());
        }
        #endregion
    }
}

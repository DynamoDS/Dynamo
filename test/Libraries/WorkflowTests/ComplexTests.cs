using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Models;
using System.Linq;

namespace Dynamo.Tests
{
    [TestFixture]
    class ComplexTests : DSEvaluationViewModelUnitTest
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

            var solidNodeId = "ff0b24b2-73e5-4da6-8081-a1883ff9ad72";
            AssertPreviewCount(solidNodeId, 66);

            // output will be 66 Solids, so putting verification for all Solid creation
            for (int i = 0; i<=65; i++)
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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\02 List Lacing.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            // check list lacing 
            var shortesListLacing = "e93fee37-1901-4162-8f73-6b5e98c1167f";
            AssertPreviewCount(shortesListLacing, 6);
            var longestListLacing = "de2b1391-95e0-4b3d-b7f0-43d03d5c5b5a";
            AssertPreviewCount(longestListLacing,10);
            var crossProduct = "c16658b3-42dc-4a45-b58a-10e3f6bb2f67";
            AssertPreviewCount(crossProduct, 10);

        }


         [Test]
        public void AttractorPoint()
        {
            // Create automation for Dynamo file : 03 Attractor Point.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\04 Range Syntax.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            // check the preview value of Number Range node
            var numberRange = "5892dcad-9e46-46b5-97f2-5e18e17ca7db";
            AssertPreviewCount(numberRange, 6);
            AssertPreviewValue(numberRange,new double[] {0,2,4,6,8,10});

            // check the preview value of Number Sequence node
            var numberSequence = "e29704c3-dbc2-4eb8-bd2c-f671c4629051";
            AssertPreviewValue(numberSequence, new object[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 });

            // check the previe value of CBNS
            var cbn1 = "b00ccf43-2f2e-49fe-aa81-d072a3598834";
            AssertPreviewValue(cbn1, new object[] { 0, 2, 4, 6, 8, 10,12,14,16,18 });
            var cbn2 = "a6e062d2-abba-427f-84a6-e2492db791f2";
            AssertPreviewValue(cbn2, new object[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27 });
            var cbn3 = "a9250aa2-f84e-4ae6-a2dc-1dedba3371ec";
            AssertPreviewValue(cbn3, new object[] { 0, 5, 10 });
            var cbn4 = "99552b5c-caf6-4ae4-85ab-9e8660643726";
            AssertPreviewValue(cbn4, new object[] { 0, 3, 6, 9, 12});
  
        }


         [Test]
        public void WriteToExcel()
        {
            // Create automation for Dynamo file : 05 Write to Excel.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\05 Write To Excel.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);

            // check List.Transpose
            var listTranspose = "53ee9988-6ed7-497f-92d8-2132f1a76ae3";
            AssertPreviewCount(listTranspose, 65);

        }


        [Test]
        public void ReadFromCSV ()
        {
            // Create automation for Dynamo file : 06 Read from CSV.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\06 Read from CSV.dyn");
            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\07 Passing Functions.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);

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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\08 Math.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(201, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(199, model.CurrentWorkspace.Nodes.Count);

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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Combine.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(44, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(36, model.CurrentWorkspace.Nodes.Count);

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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Count.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            //check Count
            var count = "6e2bc5c1-a774-4c54-b7e4-9f1dc87360d1";
            AssertPreviewValue(count, 43);
           
            // check List.Combine
            var combine = "2595c54d-2d72-4050-93ff-457ad3003bab";
            for (int i = 0; i < 43; i++)
            {
                var value = GetPreviewValueAtIndex(combine, i) as string;
                Assert.AreEqual(value, "foo"+i.ToString());
            }     
        }


        [Test]
        public void Flatten()
        {
            // Create automation for Dynamo file : Flatten.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\Flatten.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

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
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ListManagementMisc\CreateList.dyn");

            RunModel(openPath);
            AssertNoDummyNodes();

            //check the number of nodes and connectors
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

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
            Assert.DoesNotThrow(()=>RunModel(openPath));
            

        }
        #endregion
    }
}

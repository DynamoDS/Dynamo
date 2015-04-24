using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Models;
using System.Linq;

namespace Dynamo.Tests
{
    [TestFixture]
    class WorkflowTests : DSEvaluationViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
           
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
       public void IndexOutsideBounds_3399()
        {
             // This will test user workflow which contains many nodes.
             // Crash with "Index was outside the bounds of the array"
 
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\20140418_buildingSetback_standalone.dyn");

            

            var FARId = "c03065ec-fe54-40de-8c27-8089c7fe1b73";
            Assert.DoesNotThrow(()=>RunModel(openPath));
            

        }
        #endregion
        [Test]
        public void Truss()
        {
            // Create automation for Demo file : 01 Truss.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\01 Truss.dyn");

            RunModel(openPath);
            //Check the connectors
            Assert.AreEqual(29, model.CurrentWorkspace.Connectors.Count());
            var fatten = "c69ccae8-0c0c-4f85-bfba-68234c4417c4";
            AssertPreviewCount(fatten, 23);
           
            // output will be 23 lines.
            for (int i = 0; i <23; i++)
            {
                var line = GetPreviewValueAtIndex(fatten, i) as Line;
                Assert.IsNotNull(line);
            }

        }

        [Test]
        public void ListLacing()
        {
            // Create automation for Demo file : 02 List Lacing.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\02 List Lacing.dyn");

            RunModel(openPath);
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
            // Create automation for Demo file : 03 Attractor Point.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\03 Attractor Point.dyn");

            RunModel(openPath);
            // check the number of cylinders
            var cylinders = "ef3eaed0-7a8e-47a9-b06e-416bb30ec72f";
            AssertPreviewCount(cylinders, 100);
           

        }

        [Test]
        public void RangeSyntax()
        {
            // Create automation for Demo file : 04 RnageSyntax.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\04 Range Syntax.dyn");

            RunModel(openPath);
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
            // Create automation for Demo file : 05 Write to Excel.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\05 Write To Excel.dyn");

            RunModel(openPath);
           
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
            // Create automation for Demo file : 06 Read from CSV.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\06 Read from CSV.dyn");

            RunModel(openPath);

            //check the number of nodes and connectors
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);

            // check Points
            var points = "8980c6be-1e52-43ee-86aa-45c467ca5530";
            AssertPreviewCount(points, 130);
            for (int i = 0; i < 130; i++)
            {
                var point = GetPreviewValueAtIndex(points, i) as Point;
                Assert.IsNotNull(point);
            }

        }

        [Test]
        public void PassingFunctions()
        {
            // Create automation for Demo file : 07 Passing Functions.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\07 Passing Functions.dyn");

            RunModel(openPath);

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
            // Create automation for Demo file : 08 Math.dyn
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7214
            DynamoModel model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\01 Dynamo Basics\08 Math.dyn");

            RunModel(openPath);

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

        }

    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Dynamo.Models;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;

namespace Dynamo.Tests
{   
    [TestFixture]
    class GeometryDefectTests : DynamoModelTestBase 
    {
		private const double Epsilon = 1e-6;
		
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        // Note: This will contain purely ASM Geometry based test cases generated from Defects.

        [Test]
        public void MAGN_3996_InputAsPolyCurvetoJoinCurves()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3996
            // PolyCurve.ByJoinedCurves should be able to accept PolyCurves as an input
            
            string openPath = Path.Combine(TestDirectory,
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_3996_InputAsPolyCurvetoJoinCurves.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var polyCurveNodeID = GetPreviewValue("0d02fdfe-adda-404e-958e-e950742a8f1c") as PolyCurve;
            Assert.IsNotNull(polyCurveNodeID);

            var polyCurveNodeID1 = GetPreviewValue("61930ace-bb7c-4687-9873-d64cfbd6e2f1") as PolyCurve;
            Assert.IsNotNull(polyCurveNodeID1);

            Assert.AreEqual(polyCurveNodeID.NumberOfCurves, 3);
            Assert.AreEqual(polyCurveNodeID1.NumberOfCurves, 3);
        }

        [Test]
        public void MAGN_4578_CCSForTransformedCuboid()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4578
            // Context CoordinateSystem is incorrect for Cuboids after a transform

            string openPath = Path.Combine(TestDirectory,
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_4578_CCSForTransformedCuboid.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var cCsNodeID = GetPreviewValue("5991cebf-b559-44a9-8d93-de0800386f89") as CoordinateSystem;
            Assert.IsNotNull(cCsNodeID);

            var cCsNodeID1 = GetPreviewValue("5d162a32-026c-4ae5-9c5d-1b81d437fe49") as CoordinateSystem;
            Assert.IsNotNull(cCsNodeID1);

            Assert.AreEqual(cCsNodeID.Origin.X, 0);
            Assert.AreEqual(cCsNodeID1.Origin.X, 5);
            Assert.AreEqual(cCsNodeID1.Origin.Y, 5);
        }

        [Test]
        public void MAGN_4924_CurveExtractionFromSurface()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4924
            // Curves cannot be extracted from many surfaces

            string openPath = Path.Combine(TestDirectory,
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_4924_CurveExtractionFromSurface.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(46, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(56, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var curvesFromSurface = "754f9095-08dc-4cde-aa68-a0605c441d9e";
            AssertPreviewCount(curvesFromSurface, 48);

            // output will be 47 Curves, so putting verification 
            // for all Extracted Curve to make sure there is no missing Curves or Null
            for (int i = 0; i <= 47; i++)
            {
                var extractedCurves = GetPreviewValueAtIndex(curvesFromSurface, i) as Curve;
                Assert.IsNotNull(extractedCurves);
            }
        }

        [Test]
        public void MAGN_5029_CopyPasteWarning()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5029
            // Cutting and pasting Curve.PointAtParameter in run automatically 
            // causes "variable has not yet been defined" warning message 

            string openPath = Path.Combine(TestDirectory,
                @"core\WorkflowTestFiles\\GeometryDefects\MAGN_5029_CopyPasteWarning.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //CGet Curve.PointAtParameter node and copy paste it.
            string nodeID = "de3e5067-d7e2-4e47-aca3-7f2531614892";
            var pointAtParameterNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(nodeID);

            // Copy and paste the PointAtParameter Node
            CurrentDynamoModel.AddToSelection(pointAtParameterNode);
            CurrentDynamoModel.Copy(); // Copy the selected node.
            CurrentDynamoModel.Paste(); // Paste the copied node.

            RunCurrentModel();

            // check all the nodes and connectors are updated
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Make sure we are able to get copy pasted PointAtParameter node.
            var newPointAtPArameterNode = CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(4);
            var guid = newPointAtPArameterNode.GUID.ToString();

            // Checking there is no Warning or Error on node after copy paste.
            Assert.AreNotEqual(ElementState.Error, newPointAtPArameterNode.State);
            Assert.AreNotEqual(ElementState.Warning, newPointAtPArameterNode.State);

            AssertPreviewCount(guid, 10);

            for (int i = 0; i <= 9; i++)
            {
                var extractedCurves = GetPreviewValueAtIndex(guid, i) as Point;
                Assert.IsNotNull(extractedCurves);
            }
        }

        [Test]
        public void MAGN_5041_NurbsCurveExtend()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5041
            // Curve.Extend, Curve.ExtendEnd, Curve.ExtendStart returns null for 
            // distance = 0 on NurbsCurves


            string openPath = Path.Combine(TestDirectory,
                @"core\WorkflowTestFiles\\GeometryDefects\MAGN_5041_NurbsCurveExtend.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var nurbsCurve = GetPreviewValue("0cd25749-5f49-4e2c-82f7-ac278e35ac7f") as NurbsCurve;
            Assert.IsNotNull(nurbsCurve);

            var curveExtend = GetPreviewValue("be028256-7186-4e96-9977-38981feed66b") as Curve;
            Assert.IsNotNull(curveExtend);

            var curveExtendEnd = GetPreviewValue("e09faec4-65c4-4d1f-b769-ceb5e10c47a0") as Curve;
            Assert.IsNotNull(curveExtendEnd);

            var curveExtendStart = GetPreviewValue("1da11460-3a04-4d1a-beb8-a434c0fd206c") as Curve;
            Assert.IsNotNull(curveExtendStart);

            // Checking length of Extended Curve is same as 
            // original Nurbs Curve as extend distance is 0 

            Assert.AreEqual(curveExtend.Length, nurbsCurve.Length);
            Assert.AreEqual(curveExtendEnd.Length, nurbsCurve.Length);
            Assert.AreEqual(curveExtendStart.Length, nurbsCurve.Length);
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
        [Test]
        public void Recursion_CustomNode_5176()
        {

            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5176
            // Unhandled Exception in Dynamo Engine on second run of recursive custom node

            string openPath = Path.Combine(TestDirectory, @"core\WorkflowTestFiles\ChordMarching_customNode02.dyn");
            Assert.DoesNotThrow(() => RunModel(openPath));
            var watchVal = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("d70522b3-b5e0-4ce4-a765-9daf1bd05b44");
            Assert.IsNotNull(watchVal);

        }
        [Test]
        public void MAGN_5155_CrashCurveDivideByLengthFromParameter()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5155
            // Crash passing polycurve to Curve.DivideByLengthFromParameter

            string openPath = Path.Combine(TestDirectory,
    @"core\WorkflowTestFiles\GeometryDefects\MAGN_5155_CrashCurveDivideByLengthFromParameter.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(17, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var curves = "0503d40e-812b-47ae-8198-d4ed0ee91c91";
            AssertPreviewCount(curves, 41);

            // output will be 41 Curves, so putting verification 
            // for all Extracted Curve to make sure there is no missing Curves or Null
            for (int i = 0; i <= 40; i++)
            {
                var allCurves = GetPreviewValueAtIndex(curves, i) as Curve;
                Assert.IsNotNull(allCurves);
            }
        }

        [Test]
        public void MAGN_5177_LofByGuideCurvesForSurfaceAndSolid()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5177
            // Surface and Solid.byLoft ignores guide curve

            string openPath = Path.Combine(TestDirectory,
        @"core\WorkflowTestFiles\GeometryDefects\MAGN_5177_LofByGuideCurvesForSurfaceAndSolid.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(25, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(31, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var surface = GetPreviewValue("3d0b5d5b-4100-442f-b94d-b33184e4829d") as Surface;
            Assert.IsNotNull(surface);

            var polySurface = GetPreviewValue("ddaf06ec-8db4-48f1-b7a0-e44322aaeafa") as PolySurface;
            Assert.IsNotNull(polySurface);

            var solid = GetPreviewValue("7b3339c2-9de7-4330-ae0a-de23b3891524") as Solid;
            Assert.IsNotNull(solid);

            // Surface and Poly Surface area should be same as they are generating same Surface
            // after lofting.

            Assert.AreEqual(surface.Area, polySurface.Area, 0.000001);
        }

        [Test]
        public void MAGN_5323_ListUniqueNotWorkingWithNull()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5323
            // List.UniqueItems will not work on lists with null values

            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\GeometryDefects\MAGN_5323_ListUniqueNotWorkingWithNull.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var point = GetPreviewValueAtIndex("7f3fe860-9f4b-4bbf-848f-1a18606eb5f8", 0) as Point;
            // below count will confirm that List.Unique performed and returnd correct number.
            AssertPreviewCount("7f3fe860-9f4b-4bbf-848f-1a18606eb5f8", 5);
            Assert.IsNotNull(point);
            Assert.AreEqual(0, point.X, 0.000001);
        }

        [Test]
        public void MAGN_5365_WrongFunctionPassingToWatchCrashingDynamo()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5365
            // Crash with either Point.Origin or Vector.ZAxis wired to Watch node

            string openPath = Path.Combine(TestDirectory, 
            @"core\WorkflowTestFiles\GeometryDefects\MAGN_5365_WrongFunctionPassingToWatchCrashingDynamo.dyn");

            RunCurrentModel();

            AssertNoDummyNodes();

            // As test cases reaches here are Asserts are validated, it means there is no
            // crash while running the graph. No more verification needed.
        }

        [Test]
        public void MAGN_5397_ListScanWithPolygon()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5397
            // List.Scan is not working in attached example. 
            // This is a regression from last release.

            string openPath = Path.Combine(TestDirectory,
                @"core\WorkflowTestFiles\GeometryDefects\MAGN_5397_ListScanWithPolygon.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var finalSolid = GetPreviewValue("7ad36ecb-21f3-41f0-acb9-15017c48a19d") as Solid;
            Assert.IsNotNull(finalSolid);
        }

        [Test]
        public void MAGN_5407_GroupByKeyWithListOfPoints()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5407
            // Crash passing polycurve to Curve.DivideByLengthFromParameter

            string openPath = Path.Combine(TestDirectory, 
            @"core\WorkflowTestFiles\GeometryDefects\MAGN_5407_GroupByKeyWithListOfPoints.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var groupedObjects = "3c2f5adb-7967-47ca-962a-a4d24aaea8a9";
            AssertPreviewCount(groupedObjects, 2);

            for (int i = 0; i <= 1; i++)
            {
                var allPoints = GetPreviewValueAtIndex(groupedObjects, i) as Point;
                Assert.IsNotNull(allPoints);
            }

            var groupedObjects1 = "ca293efd-5e4d-47f4-ab70-1278876dea36";
            AssertPreviewCount(groupedObjects1, 2);

            for (int i = 0; i <= 1; i++)
            {
                var allPoints1 = GetPreviewValueAtIndex(groupedObjects1, i) as Point;
                Assert.IsNotNull(allPoints1);
            }
        }

        [Test]
        public void MAGN_5408_ListUniqueOnGeometryObjects()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5408
            // Few things from Defects are working and few are not, adding tests for Point and Line.

            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\GeometryDefects\MAGN_5408_ListUniqueOnGeometryObjects.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var groupPoints = "de1cd6bd-6afd-4fc5-aefe-ecec6be87a7c";
            AssertPreviewCount(groupPoints, 2);

            for (int i = 0; i <= 1; i++)
            {
                var allPOints = GetPreviewValueAtIndex(groupPoints, i) as Point;
                Assert.IsNotNull(allPOints);
            }

            var groupLines = "ca34f933-8431-4c52-b2db-a3e096a6269e";
            AssertPreviewCount(groupLines, 2);

            for (int i = 0; i <= 1; i++)
            {
                var allLines = GetPreviewValueAtIndex(groupLines, i) as Line;
                Assert.IsNotNull(allLines);
            }
        }


        
        [Test]
        public void MAGN_6799_ListMapDoesntWorkWithFlatten()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6799
            // Flatten Does Not work With List.Map.

            string openPath = Path.Combine(TestDirectory, @"core\list\Listmap.dyn");
            RunModel(openPath);

            //check the point.x , point.y and point.z
            //get Point.X guid
            var pointX = GetFlattenedPreviewValues("b63b850f-a9cc-4c5d-9bbd-ad144d74e227");
            Assert.AreEqual(pointX,new object [] {1,2,3,4,10,20,30,40});

            //get Point.y guid
            var pointY = GetFlattenedPreviewValues("2a5daf0c-1316-4ff0-be16-74e3241eff58");
            Assert.AreEqual(pointY, new object[] { 1, 2, 3, 4, 10, 20, 30, 40 });

            //get Point.z guid
            var pointZ = GetFlattenedPreviewValues("24b75bda-4e39-48d1-98ec-de103f739567");
            Assert.AreEqual(pointZ, new object[] { 1, 2, 3, 4, 10, 20, 30, 40 });

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //get List.Map guid
            string ListMapGuid = "0af8a082-0d22-476f-bc28-e61b4ce01170";  
            
            //check the dimension of list
            var levelCount = 2;
            AssertPreviewCount(ListMapGuid, levelCount);
            
            //flatten the list
            var levelList = GetFlattenedPreviewValues(ListMapGuid);
            Assert.AreEqual(levelList.Count, levelCount * 4);
            
            //check the first parameter is not null
            Assert.IsNotNull(levelList[0]);

        }

        [Test]
        public void TestExportWithUnits()
        {
            string openPath = Path.Combine(
                TestDirectory,
                @"core\geometryui\export_units_one_cuboid.dyn");

            RunModel(openPath);

            var exportPreview = GetPreviewValue("71e5eea4-63ea-4c97-9d8d-aa9c8a2c420a") as string;

            Assert.IsNotNull(exportPreview);

            Assert.IsTrue(exportPreview.Contains("exported.sat"));
        }

        [Test]
        public void TestCoordinateSystem()
        {
            //This test is use to test coordinate system
            //Newly added Default arguments are missing for entire CoordinateSystem class.
            //http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7517
            string openPath = Path.Combine(
                TestDirectory,
                @"core\geometryui\coordinateSystem.dyn");

            RunModel(openPath);

            var csPreview = GetPreviewValue("c7a317fc-c980-43f9-90c5-df50c9703f95") as CoordinateSystem;
            Assert.AreEqual(0,csPreview.Origin.X );
            Assert.AreEqual(0,csPreview.Origin.Y);
            Assert.AreEqual(0, csPreview.Origin.Z); 
        }


        [Test]
        public void VoronoiByParameterOnSurface_MAGN_8039()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-8039

            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\GeometryDefects\Voronoi.ByParameterOnSurface_MAGN_8039.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var VoronoiNodeID = "2fdd3ad8-dc1e-4087-be18-196732d41d7a";

            for (int i = 0; i <= 10; i++)
            {
                var allLines = GetPreviewValueAtIndex(VoronoiNodeID, i) as Line;
                Assert.IsNotNull(allLines);
            }
        }

        [Test]
        public void NurbsSurfaceWeights_MAGN_7970()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7970

            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\GeometryDefects\NurbsSurfaceWeights_MAGN_7970.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(20, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Surface created using NurbsSurface.ByControlPointsWeightsKnots
            var nurbsSurfaceWeights = GetFlattenedPreviewValues("92c2dd46-c73f-4e73-868f-19ae619eab26");
            Assert.AreEqual(nurbsSurfaceWeights, new object[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            // Surface created using NurbsSurface.ByControlPoints
            var nurbsSurfaceWeights1 = GetFlattenedPreviewValues("51525d19-3de9-4329-8249-6ddfb45aa8ac");
            Assert.AreEqual(nurbsSurfaceWeights1, new object[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

        }

        [Test]
        [Category("Failure")]
        public void LargeModel_Intersection_MAGN_9320()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-9320

            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\GeometryDefects\LargeModel_Intersection_MAGN_9320.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // Surface created using NurbsSurface.ByControlPointsWeightsKnots
            var intersectAllResults = GetFlattenedPreviewValues("ecc1a8ed-7f70-45b6-9e57-d72c6774b2fe");
            Assert.AreEqual(42, intersectAllResults.Count);
            foreach (var x in intersectAllResults)
            {
                Assert.IsTrue(x.GetType() == typeof(Point));
            }
            
            var doesIntersectResult = GetFlattenedPreviewValues("be1b4b66-fc41-4240-a79f-f2345439ee33");
            Assert.AreEqual(42, doesIntersectResult.Count);
            foreach (var x in doesIntersectResult)
            {
                Assert.IsTrue((bool)x);
            }
        }
        #region Test Node Changes
        [Test]
        public void TestNodeChange_BoundingBox_ByGeometry()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestBoundingBox.ByGeometry.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            Assert.AreEqual(GetPreviewValue("f03e7c8a-1ab3-49d3-be23-d0f47fca0742"), GetPreviewValue("eb5288e9-4ea0-4b5e-afcc-ef78779d8589"));
        }

        [Test]
        public void TestNodeChange_Solid_ByLoft()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestSolid.ByLoft.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            var val = GetPreviewValue("68bca29e-cf59-4dfb-89a2-1183895a6cac") as Solid;
            var val2 = GetPreviewValue("253e626f-afd8-4407-90ca-3e2b853cb572") as Solid;
            var val3 = GetPreviewValue("1ce322b0-a5a6-4874-a0c1-2ce5ca8aa3b3") as Solid;
            ShouldBeApproximate(val.Area, val2.Area);
            ShouldBeApproximate(val2.Area, val3.Area);
            ShouldBeApproximate(val.Area, 69.11503837);
        }
        [Test]
        public void TestNodeChange_Surface_ByLoft()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestSurface.ByLoft.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            var val = GetPreviewValue("411aeb4d-560f-4676-a93d-ee8d41170dcf") as Surface;
            var val2 = GetPreviewValue("9d2b0d76-c44b-4bbd-b513-555e61d246a8") as Surface;
            var val3 = GetPreviewValue("5ce5db0c-fdc8-445b-b923-144eda1a33e2") as Surface;
            ShouldBeApproximate(val.Area, val2.Area);
            ShouldBeApproximate(val2.Area, val3.Area);
            ShouldBeApproximate(val.Area, 62.831853071);
        }
        [Test]
        public void TestNodeChange_Surface_ByJoin()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestPolySurface.ByJoinedSurfaces.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            var val = GetPreviewValue("ac351e9f-01a4-42b3-9b49-6d764d0224b8") as Surface;
            var val2 = GetPreviewValue("3f3efcee-e954-4a71-818c-c06301ed4e57") as Surface;
            ShouldBeApproximate(val.Area, 2);
            ShouldBeApproximate(val2.Area, 3);
        }

        [Test]
        public void TestNodeChange_ByUnion()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestByUnion.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            var val = GetPreviewValue("ec6a82c4-ee31-4c72-a5ef-58dc1bec0fe9") as Solid;
            var val2 = GetPreviewValue("9f2ea7e8-51e8-4361-a337-9f7849a00d29") as Solid;
            ShouldBeApproximate(val.Area, val2.Area);
            ShouldBeApproximate(val.Area, 45.83992284);
        }
        [Test]
        public void TestNodeChange_ExportToSaT()
        {
            string openPath = Path.Combine(TestDirectory,
            @"core\WorkflowTestFiles\TestGeometryNodeChanges\TestGeometry.ExportToSAT.dyn");
            RunModel(openPath);

            AssertNoDummyNodes();
            Assert.NotNull( GetPreviewValue("9d0457db-65e4-43c9-a7c7-752b79f73216"));
            Assert.NotNull(GetPreviewValue("998d5d0f-1d8f-48df-9f99-696613fb549f"));
            Assert.NotNull(GetPreviewValue("9cefb50b-c331-4af3-b076-e4b3d264e26d"));
            Assert.NotNull(GetPreviewValue("11314f90-fcb8-4493-90ee-4051c3a9ff34"));
            Assert.NotNull(GetPreviewValue("5b5c0108-5e74-4ff1-a435-cf86c219ae73"));
        }
        #endregion
        #region Helper Methods
        private static void ShouldBeApproximate(double x, double y, double epsilon = Epsilon)
        {
            Assert.AreEqual(x, y, epsilon);
        }
        #endregion
    }
}

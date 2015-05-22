using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Dynamo.Models;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{   
    [TestFixture]
    class GeometryDefectTests : DynamoModelTestBase 
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
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
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(46, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Make sure we are able to get copy pasted PointAtParameter node.
            var newPointAtPArameterNode = CurrentDynamoModel.CurrentWorkspace.Nodes[4];
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
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(25, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);
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
     

        
    }
}

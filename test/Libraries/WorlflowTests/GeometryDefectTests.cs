using System.IO;
using NUnit.Framework;
using Dynamo.Models;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class GeometryDefectTests : DSEvaluationViewModelUnitTest
    {
        // Note: This will contain purely ASM Geometry based test cases generated from Defects.

        [Test]
        public void MAGN_3996_InputAsPolyCurvetoJoinCurves()
        {
            // Details are available in defect
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3996
            // PolyCurve.ByJoinedCurves should be able to accept PolyCurves as an input
            
            DynamoModel model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(),
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_3996_InputAsPolyCurvetoJoinCurves.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

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

            DynamoModel model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(),
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_4578_CCSForTransformedCuboid.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

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

            DynamoModel model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(),
        @"core\WorkflowTestFiles\\GeometryDefects\MAGN_4924_CurveExtractionFromSurface.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(46, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(56, model.CurrentWorkspace.Connectors.Count);

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
            /* Cutting and pasting Curve.PointAtParameter in run automatically 
             * causes "variable has not yet been defined" warning message */

            DynamoModel model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(),
                @"core\WorkflowTestFiles\\GeometryDefects\MAGN_5029_CopyPasteWarning.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            //CGet Curve.PointAtParameter node and copy paste it.
            string nodeID = "de3e5067-d7e2-4e47-aca3-7f2531614892";
            var pointAtParameterNode = model.CurrentWorkspace.NodeFromWorkspace(nodeID);

            // Copy and paste the PointAtParameter Node
            model.AddToSelection(pointAtParameterNode);
            model.Copy(null); // Copy the selected node.
            model.Paste(null); // Paste the copied node.

            RunCurrentModel();

            // check all the nodes and connectors are updated
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            // Make sure we are able to get copy pasted PointAtParameter node.
            var newPointAtPArameterNode = model.Nodes[4];
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

            DynamoModel model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(),
                @"core\WorkflowTestFiles\\GeometryDefects\MAGN_5041_NurbsCurveExtend.dyn");

            RunModel(openPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            var nurbsCurve = GetPreviewValue("0cd25749-5f49-4e2c-82f7-ac278e35ac7f") as NurbsCurve;
            Assert.IsNotNull(nurbsCurve);

            var curveExtend = GetPreviewValue("be028256-7186-4e96-9977-38981feed66b") as Curve;
            Assert.IsNotNull(curveExtend);

            var curveExtendEnd = GetPreviewValue("e09faec4-65c4-4d1f-b769-ceb5e10c47a0") as Curve;
            Assert.IsNotNull(curveExtendEnd);

            var curveExtendStart = GetPreviewValue("1da11460-3a04-4d1a-beb8-a434c0fd206c") as Curve;
            Assert.IsNotNull(curveExtendStart);

            /* Checking length of Extended Curve is same as 
                original Nurbs Curve as extend distance is 0 */

            Assert.AreEqual(curveExtend.Length, nurbsCurve.Length);
            Assert.AreEqual(curveExtendEnd.Length, nurbsCurve.Length);
            Assert.AreEqual(curveExtendStart.Length, nurbsCurve.Length);
        }
    }
}

using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using CurveByPoints = Autodesk.Revit.DB.CurveByPoints;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace Dynamo.Tests
{
    [TestFixture]
    class CurveTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void CurveByPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\CurveByPoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //cerate some points and wire them
            //to the selections
            ReferencePoint p1, p2, p3, p4;

            using (_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create reference points for testing.");

                p1 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(1, 5, 12));
                p2 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 1, 12));
                p3 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(12, 1, 5));
                p4 = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ(5, 12, 1));

                _trans.Commit();
            }

            var ptSelectNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is PointBySelection);
            if (!ptSelectNodes.Any())
                Assert.Fail("Could not find point selection nodes in dynamo graph.");

            ((PointBySelection)ptSelectNodes.ElementAt(0)).SelectedElement = p1;
            ((PointBySelection)ptSelectNodes.ElementAt(1)).SelectedElement = p2;
            ((PointBySelection)ptSelectNodes.ElementAt(2)).SelectedElement = p3;
            ((PointBySelection)ptSelectNodes.ElementAt(3)).SelectedElement = p4;

            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsTrue(mc.IsReferenceLine);

            //now flip the switch for creating a reference curve
            var boolNode = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is BoolSelector).First();

            ((BasicInteractive<bool>)boolNode).Value = false;

            dynSettings.Controller.RunExpression(true);
            Assert.AreEqual(fec.ToElements().Count(), 1);

            mc = (CurveByPoints)fec.ToElements().ElementAt(0);
            Assert.IsFalse(mc.IsReferenceLine);
        }

        [Test]
        public void CurveLoop()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\CurveLoop.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurvebyPointsArc()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\CurvebyPointsArc.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
        }

        [Test]
        public void OffsetCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\OffsetCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void ThickenCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\ThickenCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CurveByPointsByLineNode()
        {
            //this sample creates a geometric line
            //then creates a curve by points from that line

            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\CurveByPointsByLine.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));

            Assert.AreEqual(2, fec.ToElements().Count());

            //now change one of the number inputs and rerun
            //verify that there are still only two reference points in
            //the model
            var node = dynSettings.Controller.DynamoModel.Nodes.OfType<DoubleInput>().First();
            node.Value = "12.0";

            dynSettings.Controller.RunExpression(true);

            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(2, fec.ToElements().Count);
        }

        /*
        [Test]
        public void ClosedCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\ClosedCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var extrudeNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is CreateExtrusionGeometry);

            var result = (Solid)VisualizationManager.GetDrawablesFromNode(extrudeNode).Values.First();
            double volumeMin = 3850;
            double volumeMax = 4050;
            double actualVolume = result.Volume;
            Assert.Greater(actualVolume, volumeMin);
            Assert.Less(actualVolume, volumeMax);
        }

         * */

        [Test]
        public void CurvebyPointsEllipse()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\CurvebyPointsEllipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));


            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
        }

        [Test]
        public void GetCurveDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Curve\GetCurveDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}

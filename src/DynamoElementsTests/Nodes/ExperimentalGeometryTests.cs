using System.IO;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Nodes;

namespace Dynamo.Tests
{
    [TestFixture]
    class ExperimentalGeometryTests : DynamoUnitTest
    {
        [Test]
        public void LineTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\LineTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for X coordinate of Point node.
            Autodesk.LibG.Geometry geometry = null;
            var getGeometry = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("18dce051-d02c-4e24-b852-8626af14b2ea");
            Assert.IsTrue(Utils.Convert(getGeometry.GetValue(0), ref geometry));

            Autodesk.LibG.Point point = geometry as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            Assert.AreEqual(0, point.x());

            // Verification for Y coordinate of Point node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("c231bff3-36d7-4171-bd28-21d5730ab230");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point1 = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point1);
            Assert.AreEqual(10, point1.x());

            Autodesk.LibG.Geometry line = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<LineNode>("6e53d1b5-d85a-47d7-a299-7bd031281363");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref line));

            // Verification for "Length" , "StartPoint" and "EndPoint" of Line.
            Autodesk.LibG.Line line1 = line as Autodesk.LibG.Line;
            Assert.AreEqual(0, line1.start_point().x());
            Assert.AreEqual(10, line1.end_point().x());
            Assert.AreEqual(10, line1.length());
        }

        [Test]
        public void DoesNotBarfOnZeroLengthLine()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ZeroLengthLine.dyn");
            model.Open(openPath);

            // run the expression
            dynSettings.Controller.RunExpression(null);
        }

        [Test]
        public void ArrayOfLines_WithSliceNode()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ArrayOfLines_WithSliceNode.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            Autodesk.LibG.Geometry line = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("b906acc6-cf91-48b5-95ce-25a6e13ce983");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref line));

            // Verification for "Length" , "StartPoint" and "EndPoint" of Line.
            Autodesk.LibG.Line line1 = line as Autodesk.LibG.Line;
            Assert.AreEqual(4, line1.start_point().x());
            Assert.AreEqual(4, line1.end_point().x());
            Assert.AreEqual(10.770329614269007, line1.length(), 0.000000001);
        }
        [Test]
        public void CircleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\CircleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for X coordinate of CenterPoint node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("753356e5-4025-46b9-a688-8a2e20e482fc");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point1 = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point1);
            Assert.AreEqual(0, point1.x());

            // Verification for Circle node.
            Autodesk.LibG.Geometry geometry = null;
            var getGeometry = model.CurrentWorkspace.NodeFromWorkspace<CircleNode>("3382f192-30e7-49bf-92f0-c24504d498b5");
            Assert.IsTrue(Utils.Convert(getGeometry.GetValue(0), ref geometry));

            Autodesk.LibG.Circle circle = geometry as Autodesk.LibG.Circle;
            Assert.AreNotEqual(null, circle);
            //Assert.AreEqual(point1, circle.center_point());
            Assert.AreEqual(0, circle.center_point().x());

        }

        [Test]
        public void BSplineCurveTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\BSplineCurveTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for BSplineCurve node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<BSplineCurveNode>("78c56e90-0bc2-4629-8335-fef14ba958ed");
            FSharpList<FScheme.Value> innerList1 = getGeometry1.GetValue(0).GetListFromFSchemeValue();
            Assert.IsTrue(Utils.Convert(innerList1[0], ref geometry1));

            Autodesk.LibG.BSplineCurve bSCurve = geometry1 as Autodesk.LibG.BSplineCurve;
            Assert.AreNotEqual(null, bSCurve);
            Assert.IsFalse(bSCurve.is_closed());
            Assert.IsTrue(bSCurve.is_planar());
            Assert.AreEqual(1, bSCurve.start_point().x());
            Assert.AreEqual(5, bSCurve.end_point().y());

        }

        [Test]
        public void BSplineCurve_AllPointsAreArSamePosition()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\BSplineCurve_AllPointsAreArSamePosition.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void ClosedBSplineCurve_WhenAllPointsAreLinear()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ClosedBSplineCurve_WhenAllPointsAreLinear.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for BSplineCurve node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<ClosedBSplineCurveNode>("ba9c10bc-a932-43e8-8741-f5d79e7966ac");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.BSplineCurve bSCurve = geometry1 as Autodesk.LibG.BSplineCurve;
            Assert.AreNotEqual(null, bSCurve);
            Assert.IsTrue(bSCurve.is_closed());
            Assert.IsTrue(bSCurve.is_planar());
            //Assert.AreEqual(0, bSCurve.start_point().x());
            //Assert.AreEqual(10, bSCurve.end_point().y());

        }


        [Test]
        public void CuboidTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\CuboidTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Cuboid node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("26d1df9e-17d3-4d4a-9dea-4a2934026b47");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Cuboid cuboid = geometry1 as Autodesk.LibG.Cuboid;
            Assert.AreNotEqual(null, cuboid);
            Assert.AreEqual(150.0, cuboid.area());
            Assert.AreEqual(125.0, cuboid.volume());
            Assert.AreEqual(55.555555555555557, cuboid.center_of_gravity().x());

        }

        [Test]
        public void LineArray()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\LineArray.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Line node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("0f8214b5-b112-4056-b1a2-7ad0de5261e0");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Line line = geometry1 as Autodesk.LibG.Line;
            Assert.AreNotEqual(null, line);
            Assert.AreEqual(60, line.start_point().x());
            Assert.AreEqual(10, line.end_point().z());
        }

        [Test]
        public void LoftTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\LoftTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(27, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(34, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for loft created using ClosedCurves.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<LoftNode>("b381dd87-683a-4452-9fe2-522cfbc92179");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface loftedSruface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, loftedSruface);
            Assert.AreEqual(35.391268214374563, loftedSruface.area());
            Assert.IsTrue(loftedSruface.closed_u());
            Assert.IsFalse(loftedSruface.closed_v());

            // Verification for loft created using OpenCurves.
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<LoftNode>("033bb339-bc85-4d1a-ac32-1b1ddf954fe1");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Surface loftedSruface1 = geometry2 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, loftedSruface1);
            Assert.AreEqual(16.887305503979327, loftedSruface1.area());
            Assert.IsFalse(loftedSruface1.closed_u());
            Assert.IsFalse(loftedSruface1.closed_v());

        }

        [Test]
        public void PatchSurface_AnotherTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PatchSurface_AnotherTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(33, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface created using two planar curve
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<PatchNode>("d2b074a2-c7a9-46be-8a89-cd01ae00368a");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface patchedsurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, patchedsurface);
            Assert.AreEqual(16.290817227238776, patchedsurface.area());
            Assert.IsFalse(patchedsurface.closed_u());
            Assert.IsFalse(patchedsurface.closed_v());

            // Verification for loft created using OpenCurves.
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<PatchNode>("6ff8ce0d-7cd4-4414-b575-044a6b5c54bc");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Surface patchedSurface1 = geometry2 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, patchedSurface1);
            Assert.AreEqual(113.4724961861788, patchedSurface1.area());
            Assert.IsFalse(patchedSurface1.closed_u());
            Assert.IsFalse(patchedSurface1.closed_v());

        }

        [Test]
        public void PatchSurfaceTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PatchSurfaceTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface created using two planar curve
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<PatchNode>("ac0450a7-e71d-490d-a07a-55fb28c01596");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface patchedsurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, patchedsurface);
            Assert.AreEqual(2.0321279999999815, patchedsurface.area());
            Assert.IsFalse(patchedsurface.closed_u());
            Assert.IsFalse(patchedsurface.closed_v());

        }

        [Test]
        public void Point3DTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Point3DTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Point
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<Point3DNode>("89653c21-d76b-45c6-ab5e-9160ea0a0e06");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            double xValue = point.x();
            double yValue = point.y();
            double zValue = point.z();
            Assert.AreEqual(0, xValue);
            Assert.AreEqual(3.14159265358979, yValue);
            Assert.AreEqual(3.14159265358979, zValue);

            // Property Node tests.
            var xProperty = model.CurrentWorkspace.NodeFromWorkspace<PointXNode>("935b53bb-535a-4b32-aff8-3b9dbf72869d");
            Assert.AreEqual(xValue, xProperty.GetValue(0).GetDoubleFromFSchemeValue());

            var yProperty = model.CurrentWorkspace.NodeFromWorkspace<PointYNode>("36de2412-ad63-465d-b19a-719a0eb7f648");
            Assert.AreEqual(yValue, yProperty.GetValue(0).GetDoubleFromFSchemeValue());

            var zProperty = model.CurrentWorkspace.NodeFromWorkspace<PointZNode>("175727bd-d9f4-4b54-bb2a-f608967e203d");
            Assert.AreEqual(zValue, zProperty.GetValue(0).GetDoubleFromFSchemeValue());

        }

        [Test]
        public void Point2D_ArrayOfPoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Point2D_ArrayOfPoint.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Point
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("b33aad81-94cc-4f8f-9580-51870b7e9363");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            double xValue = point.x();
            double yValue = point.y();
            Assert.AreEqual(10, xValue);
            Assert.AreEqual(10, yValue);
        }

        [Test]
        public void Point_WithInvalidInputType()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Point_WithInvalidInputType.dyn");
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
        public void PolygonTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PolygonTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Polygon
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<PolygonNode>("0474ec0b-6b18-4123-b054-4fef1f8612ee");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Polygon polygon = geometry1 as Autodesk.LibG.Polygon;
            Assert.AreNotEqual(null, polygon);
            Assert.AreEqual(4, polygon.vertices().Count);

        }


        [Test]
        public void Polygon_AllPointsAreLinear()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Polygon_AllPointsAreLinear.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });

        }
        [Test]
        public void PropertycheckTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PropertycheckTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(31, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Area Node
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSurfaceNode>("a61da19c-b8cc-429d-867f-46e38c89829a");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface sweptSurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, sweptSurface);

            double sweptSurfaceArea = sweptSurface.area();
            Assert.AreEqual(182.24579120015983, sweptSurfaceArea);

            var areFromNode = model.CurrentWorkspace.NodeFromWorkspace<AreaNode>("9ef163dd-e0e0-4acc-a07b-b901e41f1512");
            Assert.AreEqual(sweptSurfaceArea, areFromNode.GetValue(0).GetDoubleFromFSchemeValue());

            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<BSplineCurveNode>("064e5f0a-62a4-405c-ba3f-cde337afaa9c");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            //Verification for Length Node.
            Autodesk.LibG.BSplineCurve bSCurve = geometry2 as Autodesk.LibG.BSplineCurve;
            Assert.AreNotEqual(null, bSCurve);

            double curveLength = bSCurve.length();

            var lengthNode = model.CurrentWorkspace.NodeFromWorkspace<LengthNode>("f3d354ad-7e1e-4b2e-8401-3f534ee4e2f0");
            Assert.AreEqual(curveLength, lengthNode.GetValue(0).GetDoubleFromFSchemeValue());
        }

        [Test]
        public void Surface_UsingBSplineCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Surface_UsingBSplineCurve.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(44, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(60, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface created using BSpline curve.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSurfaceNode>("de19856b-cb60-4cc4-b616-c8cf38b0efd0");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface patchedsurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, patchedsurface);
            Assert.AreEqual(975.75638786954767, patchedsurface.area(), 0.000000001);
            Assert.IsFalse(patchedsurface.closed_u());
            Assert.IsTrue(patchedsurface.closed_v());

        }

        [Test]
        public void SurfaceTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\SurfaceTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(44, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(59, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("da3de021-8b3d-4430-8801-b8191d27f50e");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface bSplineSurface1 = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, bSplineSurface1);
            Assert.AreEqual(38.202015536467101, bSplineSurface1.area(), 0.000000001);
            Assert.IsFalse(bSplineSurface1.closed_u());
            Assert.IsTrue(bSplineSurface1.closed_v());


            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("da0f72f7-6719-44e9-8708-5ad817c0747f");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Surface bSplineSurface2 = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, bSplineSurface2);
            Assert.AreEqual(38.202015536467101, bSplineSurface2.area(), 0.000000001);
            Assert.IsFalse(bSplineSurface2.closed_u());
            Assert.IsTrue(bSplineSurface2.closed_v());
        }

        [Test]
        public void SweepAsSolidTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\SweepAsSolidTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(21, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(33, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using "Sweep as Solid" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSolidNode>("398d1047-da7d-4ab4-a661-4e686a7949a2");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid sweepedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, sweepedSolid);
            Assert.AreEqual(194.26617152241661, sweepedSolid.area(), 0.000000001);
            Assert.AreEqual(93.991683898354964, sweepedSolid.volume(), 0.000000001);
            Assert.AreEqual(10.000000000000005, sweepedSolid.center_of_gravity().x());

        }

        [Test]
        public void SweepAsSurfaceTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\SweepAsSurfaceTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(32, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface created using "Sweep as Surface" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSurfaceNode>("a61da19c-b8cc-429d-867f-46e38c89829a");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface sweptSurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, sweptSurface);
            Assert.AreEqual(182.24579120015983, sweptSurface.area(), 0.000000001);
            Assert.IsTrue(sweptSurface.closed_u());
            Assert.IsFalse(sweptSurface.closed_v());

        }

        [Test]
        public void ThickenSurfaceTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ThickenSurfaceTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(22, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(24, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using "Thicken" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<ThickenSurfaceNode>("424090c9-f36c-4aaf-8f46-1fae5bb87f41");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid thickenSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, thickenSolid);
            Assert.AreEqual(217.12607751804401, thickenSolid.area(), 0.000000001);
            Assert.AreEqual(42.74408242616218, thickenSolid.volume(), 0.000000001);
            Assert.AreEqual(-4.4489040318494073, thickenSolid.center_of_gravity().x(), 0.000000001);

        }

        [Test]
        public void ThickenSurface_InputIncorrectGeometry()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ThickenSurface_InputIncorrectGeometry.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });

        }

        [Test]
        public void TranslateTest_SingleObject()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\TranslateTest_SingleObject.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(22, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(35, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using "Translate" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<TranslateNode>("e24de1c7-9c5f-460c-975b-1c418bd05db6");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid translatedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, translatedSolid);
            Assert.AreEqual(194.26617152241661, translatedSolid.area(), 0.000000001);
            Assert.AreEqual(93.991683898354964, translatedSolid.volume(), 0.000000001);
            Assert.AreEqual(10.000000000000005, translatedSolid.center_of_gravity().x());


            // Verification for Solid created using "Sweep as Solid" node.
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSolidNode>("398d1047-da7d-4ab4-a661-4e686a7949a2");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Solid sweepedSolid = geometry2 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, sweepedSolid);
            Assert.AreEqual(194.26617152241661, sweepedSolid.area(), 0.000000001);
            Assert.AreEqual(93.991683898354964, sweepedSolid.volume(), 0.000000001);
            Assert.AreEqual(10.000000000000005, sweepedSolid.center_of_gravity().x());

        }

        [Test]
        public void TrimTest_UsingTwoSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\TrimTest_UsingTwoSurface.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(31, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(35, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface created using "Trim" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<TrimNode>("97ba959c-ad9f-4358-84db-8bedd9020069");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface sweptSurface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, sweptSurface);
            Assert.AreEqual(88.30416734641004, sweptSurface.area(), 0.000000001);
            Assert.IsFalse(sweptSurface.closed_u());
            Assert.IsFalse(sweptSurface.closed_v());

        }

        [Test]
        public void VectorTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\VectorTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //// Verification for Vector node.
            //Autodesk.LibG.Geometry geometry1 = null;
            //var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<VectorNode>("c55236e7-e844-409c-bb99-510681aa6dba");
            //Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            //Autodesk.LibG.Vector vector = geometry1 as Autodesk.LibG.Vector;
            //Assert.AreNotEqual(null, vector);

            //double xValue = vector.x();
            //double yValue = vector.y();
            //double zValue = vector.z();

            //Assert.AreEqual(88.30416734641004, xValue);
            //Assert.AreEqual(1, yValue);
            //Assert.AreEqual(1, zValue);

        }

        [Test]
        public void Vector_UsingCrossProduct()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Vector_UsingCrossProduct.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //// Verification for Vector node.
            //Autodesk.LibG.Geometry geometry1 = null;
            //var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<VectorNode>("c55236e7-e844-409c-bb99-510681aa6dba");
            //Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            //Autodesk.LibG.Vector vector = geometry1 as Autodesk.LibG.Vector;
            //Assert.AreNotEqual(null, vector);

            //double xValue = vector.x();
            //double yValue = vector.y();
            //double zValue = vector.z();

            //Assert.AreEqual(88.30416734641004, xValue);
            //Assert.AreEqual(1, yValue);
            //Assert.AreEqual(1, zValue);

        }

        [Test]
        public void Vector_PropertiesTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Vector_PropertiesTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            //// Verification for Vector node.
            //Autodesk.LibG.Geometry geometry1 = null;
            //var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<VectorNode>("c55236e7-e844-409c-bb99-510681aa6dba");
            //Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            //Autodesk.LibG.Vector vector = geometry1 as Autodesk.LibG.Vector;
            //Assert.AreNotEqual(null, vector);

            //double xValue = vector.x();
            //double yValue = vector.y();
            //double zValue = vector.z();

            //Assert.AreEqual(88.30416734641004, xValue);
            //Assert.AreEqual(1, yValue);
            //Assert.AreEqual(1, zValue);

        }

        [Test]
        public void PlaneTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PlaneTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Plane node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("6b3c6fc6-52f5-4e0d-99fa-fc4cb8314b7d");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Plane plane = geometry1 as Autodesk.LibG.Plane;
            Assert.AreNotEqual(null, plane);
            Assert.AreEqual(2, plane.origin().x());
            Assert.AreEqual(1, plane.normal().x());

        }

        [Test]
        public void PointAtDistanceTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PointAtDistanceTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(29, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(36, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Point created using PointAtDistance node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("5f48e595-d2cd-485f-99a1-dc0abe364ed3");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            double xValue = point.x();
            double yValue = point.y();
            double zValue = point.z();
            Assert.AreEqual(10, xValue);
            Assert.AreEqual(5.5978213925456739, yValue, 0.000000001);
            Assert.AreEqual(-2.506439789083756, zValue, 0.000000001);

            //Autodesk.LibG.Geometry pointAtDistance = null;
            //var pointAtDistance1 = model.CurrentWorkspace.NodeFromWorkspace<PointAtDistanceNode>("877369eb-1b97-4d05-a4d4-61d37b803562");
            //Assert.IsTrue(Utils.Convert(pointAtDistance1.GetValue(0), ref pointAtDistance));

            //Autodesk.LibG.Point point1 = pointAtDistance as Autodesk.LibG.Point;
        }

        [Test]
        public void PointAtParameter_SimpleTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PointAtParameter_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(21, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(22, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Line created using output of PointAtParameter node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<LineNode>("9629d1a1-f1b5-41a8-9086-90c09b2477b7");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Line line = geometry1 as Autodesk.LibG.Line;
            Assert.AreNotEqual(null, line);

            Assert.AreEqual(284.39273535941936, line.length(), 0.000000001);

        }

        [Test]
        public void PointAtUVParameterTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PointAtUVParameterTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(51, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(69, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Point which is created using PointAtUVParameter node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("f467baf4-2bac-4c6f-a31e-d33016571bf5");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Point point = geometry1 as Autodesk.LibG.Point;
            Assert.AreNotEqual(null, point);
            double xValue = point.x();
            double yValue = point.y();
            double zValue = point.z();
            Assert.AreEqual(-6.3977743135825484, xValue, 0.000000001);
            Assert.AreEqual(-16.869544438947674, yValue, 0.000000001);
            Assert.AreEqual(-10.633850246828377, zValue, 0.000000001);

        }

        [Test]
        public void PointAtUVParameter_AnotherScenarioTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\PointAtUVParameter_SimpleTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(54, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(72, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Line created using output of PointAtUVParameter node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<LineNode>("bf3b9aca-78c5-4f1a-a426-eda2b6cc7f73");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Line line = geometry1 as Autodesk.LibG.Line;
            Assert.AreNotEqual(null, line);

            Assert.AreEqual(13.620728681577987, line.length(), 0.000000001);

        }


        [Test]
        public void SweeAsSurface_AnotherTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\SweeAsSurface_AnotherTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<SweepAsSurfaceNode>("a19f5539-3397-4975-ae4b-a0a2c45a3489");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface surface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, surface);
            Assert.AreEqual(66.591667947439021, surface.area(), 0.000000001);
            Assert.IsFalse(surface.closed_u());
            Assert.IsTrue(surface.closed_v());

        }


        [Test]
        public void ExtrudCurveTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ExtrudCurveTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(24, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(27, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<ExtrudeNode>("5c61d60b-23d8-444b-98b8-5f7bf75d5694");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface surface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, surface);
            Assert.AreEqual(1.280984238447507, surface.area(), 0.000000001);
            Assert.IsFalse(surface.closed_u());
            Assert.IsFalse(surface.closed_v());

        }

        [Test]
        public void ExtrudeArrayOfClosedCurves()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ExtrudeArrayOfClosedCurves.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<GetFromList>("d0aabae2-386f-4757-9fcb-124b36c82b69");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface surface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, surface);
            Assert.AreEqual(125.66370614359172, surface.area(), 0.000000001);
            Assert.IsFalse(surface.closed_u());
            Assert.IsTrue(surface.closed_v());

        }

        [Test]
        public void ExtrudeArrayOfOpenCurves()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\ExtrudeArrayOfOpenCurves.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<ExtrudeNode>("d6c89a2b-d91b-49ce-a57f-8c33c34c29b5");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Surface surface = geometry1 as Autodesk.LibG.Surface;
            Assert.AreNotEqual(null, surface);
            Assert.AreEqual(11.188272457045143, surface.area(), 0.000000001);
            Assert.IsFalse(surface.closed_u());
            Assert.IsFalse(surface.closed_v());

        }

        [Test]
        public void Circle_NegativeTest()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Circle_NegativeTest.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            // run expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Circle with CircleWithZeroVectorLength which is invalid
            // though this test case is now passing but after fixing defect this test case will fail and need to update this test case.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<CircleNode>("b1d4583f-fac2-4459-b266-9f69dd538d8c");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Circle circle = geometry1 as Autodesk.LibG.Circle;
            Assert.AreNotEqual(null, circle);

            // Verification for Circle with CircleWithRadiusZero which is invalid
            // though this test case is now passing but after fixing defect this test case will fail and need to update this test case.
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<CircleNode>("2094fe23-9f14-4fa4-ab2a-5dfc66dda6ef");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Circle circle2 = geometry1 as Autodesk.LibG.Circle;
            Assert.AreNotEqual(null, circle);

            //// run the expression
            //Assert.Throws<AssertionException>(() =>
            //    {
            //        dynSettings.Controller.RunExpression(null);
            //    });
        }

        [Test]
        public void Sweep_NegativeScenario()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Sweep_NegativeScenario.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
                {
                    dynSettings.Controller.RunExpression(null);
                });
        }

        [Test]
        public void Loft_NegativeScenario()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Loft_NegativeScenario.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            Assert.Throws<AssertionException>(() =>
            {
                dynSettings.Controller.RunExpression(null);
            });
        }

        [Test]
        public void LoftUsingCircles()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\LoftUsingCircles.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(33, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(46, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using Thinking Lofted surface.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<ThickenSurfaceNode>("b6952095-fc70-4b11-b0b6-b399714c0460");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid thikenSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, thikenSolid);
            Assert.AreEqual(5468.2219186769089, thikenSolid.area(), 0.000000001);
            Assert.AreEqual(24.601521194889756, thikenSolid.volume(), 0.000000001);
            Assert.AreEqual(9.4206816962888951, thikenSolid.center_of_gravity().x());

        }

        [Test]
        public void Intersect_CuboidAndCuboid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Intersect_CuboidAndCuboid.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(29, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(40, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using "Intersect" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<IntersectNode>("6c707733-7616-47b3-9240-eb42ddc66def");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid sweepedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, sweepedSolid);
            Assert.AreEqual(24, sweepedSolid.area());
            Assert.AreEqual(8, sweepedSolid.volume());
            Assert.AreEqual(1.0, sweepedSolid.center_of_gravity().x());

            // Verification for translated Solid
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<TranslateNode>("e87d2dee-bf39-46ea-bcd9-b688ab85f821");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Solid translatedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, translatedSolid);
            Assert.AreEqual(24, translatedSolid.area());
            Assert.AreEqual(8, translatedSolid.volume());
            Assert.AreEqual(1.0, translatedSolid.center_of_gravity().x());

        }

        [Test]
        public void Intersect_SurfaceAndCuboid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Intersect_SurfaceAndCuboid.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(25, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(34, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Verification for Solid created using "Intersect" node.
            Autodesk.LibG.Geometry geometry1 = null;
            var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<IntersectNode>("24e632b7-bfe5-4b91-be80-f78afa8e11c6");
            Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            Autodesk.LibG.Solid sweepedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, sweepedSolid);
            Assert.AreEqual(8, sweepedSolid.area());
            Assert.AreEqual(0, sweepedSolid.volume()); // need to revisit this test case, because Volume cannot be zero for Solid.
            Assert.AreEqual(0, sweepedSolid.center_of_gravity().x());

            // Verification for translated Solid
            Autodesk.LibG.Geometry geometry2 = null;
            var getGeometry2 = model.CurrentWorkspace.NodeFromWorkspace<TranslateNode>("e87d2dee-bf39-46ea-bcd9-b688ab85f821");
            Assert.IsTrue(Utils.Convert(getGeometry2.GetValue(0), ref geometry2));

            Autodesk.LibG.Solid translatedSolid = geometry1 as Autodesk.LibG.Solid;
            Assert.AreNotEqual(null, translatedSolid);
            Assert.AreEqual(8, translatedSolid.area());
            Assert.AreEqual(0, translatedSolid.volume()); // need to revisit this test case, because Volume cannot be zero for Solid.
            Assert.AreEqual(0, translatedSolid.center_of_gravity().x());

        }

        [Test]
        public void Trim_UsingPlaneAndCuboid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Trim_UsingPlaneAndCuboid.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(27, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Will add verification once we fix the defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-501

            //// Verification for Solid created using "Trim" node.
            //Autodesk.LibG.Geometry geometry1 = null;
            //var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<TrimNode>("e48b775c-d469-49d7-9c0b-ccabf4d24d69");
            //Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            //Autodesk.LibG.Solid sweepedSolid = geometry1 as Autodesk.LibG.Solid;
            //Assert.AreNotEqual(null, sweepedSolid);
            //Assert.AreEqual(8, sweepedSolid.area());
            //Assert.AreEqual(0, sweepedSolid.volume()); 
            //Assert.AreEqual(0, sweepedSolid.center_of_gravity().x());

        }

        [Test]
        public void Trim_UsingPSurfaceAndCuboid()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\GeometryTestFiles\Trim_UsingPSurfaceAndCuboid.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(27, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(38, model.CurrentWorkspace.Connectors.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // Will add verification once we fix the defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-501

            //// Verification for Solid created using "Trim" node.
            //Autodesk.LibG.Geometry geometry1 = null;
            //var getGeometry1 = model.CurrentWorkspace.NodeFromWorkspace<TrimNode>("e48b775c-d469-49d7-9c0b-ccabf4d24d69");
            //Assert.IsTrue(Utils.Convert(getGeometry1.GetValue(0), ref geometry1));

            //Autodesk.LibG.Solid sweepedSolid = geometry1 as Autodesk.LibG.Solid;
            //Assert.AreNotEqual(null, sweepedSolid);
            //Assert.AreEqual(8, sweepedSolid.area());
            //Assert.AreEqual(0, sweepedSolid.volume()); // need to revisit this test case, because Volume cannot be zero for Solid.
            //Assert.AreEqual(0, sweepedSolid.center_of_gravity().x());

        }

    }
}
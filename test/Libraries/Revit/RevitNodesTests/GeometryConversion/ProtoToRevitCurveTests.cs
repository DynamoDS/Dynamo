using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class ProtoToRevitCurveTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ToRevitType_ExtensionMethod_DoesExpectedUnitConversion()
        {
            // testing 
            var pts = new[]
            {
                Point.ByCoordinates(10,2,3)
                , Point.ByCoordinates(0,2,2)
                , Point.ByCoordinates(10,4,8)
                , Point.ByCoordinates(10,2,8)
                , Point.ByCoordinates(5,5,5)
            };

            var bspline = NurbsCurve.ByControlPoints(pts, 3);

            // do scaling for check
            var metersToFeet = 1/0.3048;
            var bsplineScaled = (NurbsCurve)bspline.Scale(metersToFeet);

            // by default, performs conversion
            var revitCurve = bspline.ToRevitType();

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline)revitCurve;

            Assert.AreEqual(bsplineScaled.Degree, revitSpline.Degree);
            Assert.AreEqual(bsplineScaled.ControlPoints().Count(), revitSpline.CtrlPoints.Count);

            // We tesselate but not convert units. We are trying to find out if
            // ToRevitType did the conversion properly by comparing to a scaled
            // version of the original bspline (bsplineScaled)
            var tessPts = revitSpline.Tessellate().Select(x => x.ToPoint(false));

            //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = bsplineScaled.ClosestPointTo(pt);
                Assert.Less(closestPt.DistanceTo(pt), 1e-6);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void NurbsCurve_Basic()
        {
            var pts = new[]
            {
                Point.ByCoordinates(10,2,3)
                , Point.ByCoordinates(0,2,2)
                , Point.ByCoordinates(10,4,8)
                , Point.ByCoordinates(10,2,8)
                , Point.ByCoordinates(5,5,5)
            };

            var bspline = NurbsCurve.ByControlPoints(pts, 3);

            var revitCurve = bspline.ToRevitType(false);

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline) revitCurve;

            Assert.AreEqual( bspline.Degree, revitSpline.Degree );
            Assert.AreEqual( bspline.ControlPoints().Count(), revitSpline.CtrlPoints.Count);

            var tessPts = revitSpline.Tessellate().Select(x => x.ToPoint(false));

             //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = bspline.ClosestPointTo(pt);
                Assert.Less( closestPt.DistanceTo(pt), 1e-6 );
            }

        }
       
        [Test]
        [TestModel(@".\empty.rfa")]
        public void NurbsCurve_AcceptsStraightReplicatedDegree3NurbsCurve()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, 0, 0));

            var nurbsCurve = NurbsCurve.ByPoints(points, 3);
            var revitCurve = nurbsCurve.ToRevitType(false);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline)revitCurve;
            Assert.AreEqual(3, revitSpline.Degree);
            var tessPts = revitSpline.Tessellate().Select(x => x.ToPoint(false));

            //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = nurbsCurve.ClosestPointTo(pt);
                Assert.Less(closestPt.DistanceTo(pt), 1e-6);
            }

            revitCurve.GetEndPoint(0).ShouldBeApproximately(nurbsCurve.StartPoint);
            revitCurve.GetEndPoint(1).ShouldBeApproximately(nurbsCurve.EndPoint);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void NurbsCurve_AcceptsStraightReplicatedDegree2NurbsCurve()
        {
            var points =
                Enumerable.Range(0, 10)
                    .Select(x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, 0, 0));

            var nurbsCurve = NurbsCurve.ByPoints(points, 2);
            var revitCurve = nurbsCurve.ToRevitType(false);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline)revitCurve;
            Assert.AreEqual(3, revitSpline.Degree);
            var tessPts = revitSpline.Tessellate().Select(x => x.ToPoint(false));

            //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = nurbsCurve.ClosestPointTo(pt);
                Assert.Less(closestPt.DistanceTo(pt), 1e-6);
            }

            revitCurve.GetEndPoint(0).ShouldBeApproximately(nurbsCurve.StartPoint);
            revitCurve.GetEndPoint(1).ShouldBeApproximately(nurbsCurve.EndPoint);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void NurbsCurve_ProvidesGoodApproximationForDegree2Curve()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0)
                , Point.ByCoordinates(0, 1, 1)
                , Point.ByCoordinates(0, 1, 0)
            };

            var bspline = NurbsCurve.ByPoints(pts, 2);
            var revitCurve = bspline.ToRevitType(false);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline)revitCurve;
            Assert.AreEqual(3, revitSpline.Degree);
            var tessPts = revitSpline.Tessellate().Select(x => x.ToPoint(false));

            //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = bspline.ClosestPointTo(pt);
                Assert.Less( closestPt.DistanceTo(pt), 1e-6 );
            }

            revitCurve.GetEndPoint(0).ShouldBeApproximately(bspline.StartPoint);
            revitCurve.GetEndPoint(1).ShouldBeApproximately(bspline.EndPoint);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void EllipseArc_Basic()
        {
            var o = Point.ByCoordinates(1, 2, 3);
            var n = Vector.ByCoordinates(2, 3, 4, true);
            var pl = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(o, n);
            var ellipseArc = EllipseArc.ByPlaneRadiiStartAngleSweepAngle(pl, 10, 5, 45, 90);

            var revitCurve = ellipseArc.ToRevitType(false);

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.Ellipse>(revitCurve);

            var revitEllipse = (Autodesk.Revit.DB.Ellipse)revitCurve;

            revitEllipse.GetEndParameter(0).ToDegrees().ShouldBeApproximately(ellipseArc.StartAngle);
            revitEllipse.GetEndParameter(1).ToDegrees().ShouldBeApproximately(ellipseArc.StartAngle + ellipseArc.SweepAngle);
            revitEllipse.GetEndPoint(0).ShouldBeApproximately(ellipseArc.StartPoint);
            revitEllipse.GetEndPoint(1).ShouldBeApproximately(ellipseArc.EndPoint);

            revitEllipse.Length.ShouldBeApproximately(ellipseArc.Length);

            // ClosestPointTo fails in ProtoGeometry
            var tessPts = revitEllipse.Tessellate().Select(x => x.ToPoint(false));

            //assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = ellipseArc.ClosestPointTo(pt);
                Assert.Less(closestPt.DistanceTo(pt), 1e-6);
            }
        } 

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Circle_Basic()
        {
            var radius = 4;
            var circ = Autodesk.DesignScript.Geometry.Circle.ByCenterPointRadiusNormal(
                Point.ByCoordinates(1, 2, 3), radius,
                Vector.ByCoordinates(0,0,1));

            var revitCurve = circ.ToRevitType(false);

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc) revitCurve;

            circ.CenterPoint.ShouldBeApproximately(revitArc.Center.ToPoint(false));
            circ.Radius.ShouldBeApproximately(revitArc.Radius);
            Math.Abs(circ.Normal.Dot(revitArc.Normal.ToVector())).ShouldBeApproximately(1);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Arc_Basic()
        {
            var circ = Autodesk.DesignScript.Geometry.Arc.ByCenterPointRadiusAngle(Point.ByCoordinates(1, 2, 3), 4,
                0.4, 1.3, Vector.ByCoordinates(1, 2, 3));

            var revitCurve = circ.ToRevitType(false);

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc) revitCurve;

            circ.CenterPoint.ShouldBeApproximately( revitArc.Center.ToPoint(false) );
            circ.Radius.ShouldBeApproximately( revitArc.Radius );
            Math.Abs(circ.Normal.Dot(revitArc.Normal.ToVector())).ShouldBeApproximately(1);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Line_Basic()
        {
            var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint( Point.ByCoordinates(1, 2, 3), Point.ByCoordinates(2,4,6));

            Console.WriteLine(line.PointAtParameter(0.5).ToXyz());

            var revitCurve = line.ToRevitType(false);

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Line>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Line)revitCurve;

            line.StartPoint.ShouldBeApproximately(revitArc.GetEndPoint(0).ToPoint(false));
            line.EndPoint.ShouldBeApproximately( revitArc.GetEndPoint(1).ToPoint(false) );
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Helix_Basic()
        {
            var sp = Point.Origin();
            var z = Vector.ZAxis();
            var s = Vector.XAxis().AsPoint();
            var p = 5.0;
            var a = 3240;

            var helix = Autodesk.DesignScript.Geometry.Helix.ByAxis(sp, z, s, p, a);

            var revitCurve = helix.ToRevitType(false);

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.CylindricalHelix>(revitCurve);

            helix.StartPoint.ShouldBeApproximately(revitCurve.GetEndPoint(0).ToPoint(false));
            helix.EndPoint.ShouldBeApproximately(revitCurve.GetEndPoint(1).ToPoint(false));

            var revitHelix = (Autodesk.Revit.DB.CylindricalHelix)revitCurve;

            revitHelix.Pitch.ShouldBeApproximately(p);
            revitHelix.Height.ShouldBeApproximately(9 * p);
            revitHelix.GetEndPoint(0).ShouldBeApproximately(s);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Ellipse_Basic()
        {

            var cs = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(Point.ByCoordinates(1, 2, 3),
                Vector.YAxis(), Vector.XAxis().Reverse());
            var ellipse = Autodesk.DesignScript.Geometry.Ellipse.ByCoordinateSystemRadii(cs, 10, 5);

            var revitCurve = ellipse.ToRevitType(false);

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Ellipse>(revitCurve);

            var revitEllipse = (Autodesk.Revit.DB.Ellipse)revitCurve;

            ellipse.StartPoint.ShouldBeApproximately(revitCurve.GetEndPoint(0).ToPoint(false));
            ellipse.EndPoint.ShouldBeApproximately(revitCurve.GetEndPoint(1).ToPoint(false));

            revitEllipse.Center.ShouldBeApproximately(Point.ByCoordinates(1, 2, 3));
            revitEllipse.XDirection.ShouldBeApproximately(Vector.YAxis());
            revitEllipse.YDirection.ShouldBeApproximately(Vector.XAxis().Reverse());
            revitEllipse.RadiusX.ShouldBeApproximately(10);
            revitEllipse.RadiusY.ShouldBeApproximately(5);

        } 
    }
}

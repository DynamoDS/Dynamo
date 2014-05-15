﻿using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.GeometryConversion;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class ProtoToRevitCurveTests : RevitNodeTestBase
    {
        [SetUp]
        public override void Setup()
        {
           HostFactory.Instance.StartUp(); 
        }

        [TearDown]
        public override void TearDown()
        {
            HostFactory.Instance.ShutDown();
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

            var revitCurve = bspline.ToRevitType();

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline) revitCurve;

            Assert.AreEqual( bspline.Degree, revitSpline.Degree );
            Assert.AreEqual( bspline.ControlPoints().Count(), revitSpline.CtrlPoints.Count);

            // ClosestPointTo fails in ProtoGeometry

            var tessPts = revitSpline.Tessellate();

             //assert the tesselation is very close to original curve
             //what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = bspline.GetClosestPoint(pt.ToPoint());
                Assert.Less( closestPt.DistanceTo(pt.ToPoint()), 1e-6 );
            }

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void EllipseArc_Basic()
        {
            var o = Point.ByCoordinates(1, 2, 3);
            var n = Vector.ByCoordinates(2, 3, 4, true);
            var pl = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(o, n);
            var ellipseArc = EllipseArc.ByPlaneRadiiStartAngleSweepAngle(pl, 10, 5, 45, 90);

            var revitCurve = ellipseArc.ToRevitType();

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.Ellipse>(revitCurve);

            var revitEllipse = (Autodesk.Revit.DB.Ellipse)revitCurve;

            revitEllipse.GetEndParameter(0).ToDegrees().AssertShouldBeApproximately(ellipseArc.StartAngle);
            revitEllipse.GetEndParameter(1).ToDegrees().AssertShouldBeApproximately(ellipseArc.StartAngle + ellipseArc.SweepAngle);
            revitEllipse.GetEndPoint(0).AssertShouldBeApproximately(ellipseArc.StartPoint);
            revitEllipse.GetEndPoint(1).AssertShouldBeApproximately(ellipseArc.EndPoint);

            revitEllipse.Length.AssertShouldBeApproximately(ellipseArc.Length);

            // ClosestPointTo fails in ProtoGeometry
            var tessPts = revitEllipse.Tessellate();

            //assert the tesselation is very close to original curve
            //what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = ellipseArc.GetClosestPoint(pt.ToPoint());
                Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
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

            var revitCurve = circ.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc) revitCurve;

            circ.CenterPoint.AssertShouldBeApproximately(revitArc.Center.ToPoint());
            circ.Radius.AssertShouldBeApproximately(revitArc.Radius);
            Math.Abs(circ.Normal.Dot(revitArc.Normal.ToVector())).AssertShouldBeApproximately(1);

        } 

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Arc_Basic()
        {
            var circ = Autodesk.DesignScript.Geometry.Arc.ByCenterPointRadiusAngle(Point.ByCoordinates(1, 2, 3), 4,
                0.4, 1.3, Vector.ByCoordinates(1, 2, 3));

            var revitCurve = circ.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc) revitCurve;

            circ.CenterPoint.AssertShouldBeApproximately( revitArc.Center.ToPoint() );
            circ.Radius.AssertShouldBeApproximately( revitArc.Radius );
            Math.Abs(circ.Normal.Dot(revitArc.Normal.ToVector())).AssertShouldBeApproximately(1);

        } 

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Line_Basic()
        {
            var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint( Point.ByCoordinates(1, 2, 3), Point.ByCoordinates(2,4,6));

            Console.WriteLine(line.PointAtParameter(0.5).ToXyz());

            var revitCurve = line.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Line>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Line)revitCurve;

            line.StartPoint.AssertShouldBeApproximately(revitArc.GetEndPoint(0).ToPoint());
            line.EndPoint.AssertShouldBeApproximately( revitArc.GetEndPoint(1).ToPoint() );
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

            var revitCurve = helix.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.CylindricalHelix>(revitCurve);

            helix.StartPoint.AssertShouldBeApproximately(revitCurve.GetEndPoint(0).ToPoint());
            helix.EndPoint.AssertShouldBeApproximately(revitCurve.GetEndPoint(1).ToPoint());

            var revitHelix = (Autodesk.Revit.DB.CylindricalHelix)revitCurve;

            revitHelix.Pitch.AssertShouldBeApproximately(p);
            revitHelix.Height.AssertShouldBeApproximately(9 * p);
            revitHelix.GetEndPoint(0).AssertShouldBeApproximately(s);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Ellipse_Basic()
        {

            var cs = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(Point.ByCoordinates(1, 2, 3),
                Vector.YAxis(), Vector.XAxis().Reverse());
            var ellipse = Autodesk.DesignScript.Geometry.Ellipse.ByCoordinateSystemRadii(cs, 10, 5);

            var revitCurve = ellipse.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Ellipse>(revitCurve);

            var revitEllipse = (Autodesk.Revit.DB.Ellipse)revitCurve;

            ellipse.StartPoint.AssertShouldBeApproximately(revitCurve.GetEndPoint(0).ToPoint());
            ellipse.EndPoint.AssertShouldBeApproximately(revitCurve.GetEndPoint(1).ToPoint());

            revitEllipse.Center.AssertShouldBeApproximately(Point.ByCoordinates(1, 2, 3));
            revitEllipse.XDirection.AssertShouldBeApproximately(Vector.YAxis());
            revitEllipse.YDirection.AssertShouldBeApproximately(Vector.XAxis().Reverse());
            revitEllipse.RadiusX.AssertShouldBeApproximately(10);
            revitEllipse.RadiusY.AssertShouldBeApproximately(5);

        } 
    }
}

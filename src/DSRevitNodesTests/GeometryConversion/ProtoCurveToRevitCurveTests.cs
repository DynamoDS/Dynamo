﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using NUnit.Framework;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class ProtoCurveToRevitCurveTests
    {

        public void BSplineCurve_Basic()
        {
            var pts = new[]
            {
                Point.ByCoordinates(10,2,3)
                , Point.ByCoordinates(10,2,2)
                , Point.ByCoordinates(10,4,8)
                , Point.ByCoordinates(10,2,8)
            };

            var bspline = BSplineCurve.ByPoints(pts);

            var revitCurve = bspline.ToRevitType();

            Assert.NotNull(revitCurve);
            Assert.IsAssignableFrom<Autodesk.Revit.DB.NurbSpline>(revitCurve);

            var revitSpline = (Autodesk.Revit.DB.NurbSpline) revitCurve;

            Assert.AreEqual( bspline.Degree, revitSpline.Degree );
            Assert.AreEqual( bspline.ControlVertices.Count(), revitSpline.CtrlPoints.Count );

            var tessPts = revitSpline.Tessellate();

            // assert the tesselation is very close to original curve
            // what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = bspline.ClosestPointTo(pt.ToPoint());
                Assert.Less( closestPt.DistanceTo(pt.ToPoint()), 1e-6 );
            }

        } 

        public void Circle_Basic()
        {
            var radius = 4;
            var circ = Autodesk.DesignScript.Geometry.Circle.ByCenterPointRadius(Point.ByCoordinates(1, 2, 3), radius,
                Vector.ByCoordinates(1, 2, 3));

            var revitCurve = circ.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc) revitCurve;

            Assert.AreEqual(circ.CenterPoint, revitArc.Center.ToPoint());
            Assert.AreEqual(circ.Radius, revitArc.Radius);
            Assert.AreEqual(circ.Normal, revitArc.Normal);

            // TODO: test the interval of the revit arc

        } 

        public void Arc_Basic()
        {
            var circ = Autodesk.DesignScript.Geometry.Arc.ByCenterPointRadiusAngle(Point.ByCoordinates(1, 2, 3), 4,
                0.4, 1.3, Vector.ByCoordinates(1, 2, 3));

            var revitCurve = circ.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Arc>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Arc)revitCurve;

            Assert.AreEqual(circ.CenterPoint, revitArc.Center.ToPoint());
            Assert.AreEqual(circ.Radius, revitArc.Radius);
            Assert.AreEqual(circ.Normal, revitArc.Normal);

            // TODO: test the interval of the revit arc
        } 

        public void Line_Basic()
        {
            var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint( Point.ByCoordinates(1, 2, 3), Point.ByCoordinates(2,4,6));

            var revitCurve = line.ToRevitType();

            Assert.NotNull(revitCurve);

            Assert.IsAssignableFrom<Autodesk.Revit.DB.Line>(revitCurve);

            var revitArc = (Autodesk.Revit.DB.Line)revitCurve;

            Assert.AreEqual(line.StartPoint, revitArc.GetEndPoint(0).ToPoint());
            Assert.AreEqual(line.EndPoint, revitArc.GetEndPoint(1).ToPoint());
        } 

    }
}

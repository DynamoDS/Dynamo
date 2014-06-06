﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.GeometryConversion;
using NUnit.Framework;
using RevitTestFramework;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    public class RevitToProtoCurveTests : RevitNodeTestBase
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
        public void NurbSpline_Rational()
        {
            var pts = new List<Autodesk.Revit.DB.XYZ>()
            {
                new Autodesk.Revit.DB.XYZ(1,0,0),
                new Autodesk.Revit.DB.XYZ(1,1,0),
                new Autodesk.Revit.DB.XYZ(0,1,0),
                new Autodesk.Revit.DB.XYZ(0,2,0)
            };

            var wts = new List<double>()
            {
                1, Math.Sqrt(2)/2, 1, 1
            };

            var revitSpline = Autodesk.Revit.DB.NurbSpline.Create(pts, wts);

            var protoCurve = revitSpline.ToProtoType();

            Assert.NotNull(protoCurve);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.NurbsCurve>(protoCurve);

            var protoSpline = (Autodesk.DesignScript.Geometry.NurbsCurve) protoCurve;

            Assert.AreEqual(revitSpline.Degree, protoSpline.Degree);
           
            Assert.IsTrue(protoSpline.IsRational);
            Assert.AreEqual(revitSpline.CtrlPoints.Count, protoSpline.ControlPoints().Count());

            // need more tests here for rational curves
            Assert.AreEqual(revitSpline.Weights.Cast<double>().Count(), protoSpline.Weights().Length);

            var tessPts = revitSpline.Tessellate();

            // assert the tesselation is very close to original curve
            // what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = protoSpline.GetClosestPoint(pt.ToPoint());
                Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void HermiteSpline_Basic()
        {
            var pts = new List<Autodesk.Revit.DB.XYZ>()
            {
                new Autodesk.Revit.DB.XYZ(1,0,0),
                new Autodesk.Revit.DB.XYZ(1,1,0),
                new Autodesk.Revit.DB.XYZ(0.5,1,0),
                new Autodesk.Revit.DB.XYZ(0,1,0)
            };

            var ts = new Autodesk.Revit.DB.HermiteSplineTangents
            {

                StartTangent = new Autodesk.Revit.DB.XYZ(0, 1, 0),
                EndTangent = new Autodesk.Revit.DB.XYZ(-1, 0, 0)
            };

            var revitSpline = Autodesk.Revit.DB.HermiteSpline.Create(pts, false, ts);

            var protoCurve = revitSpline.ToProtoType();

            Assert.NotNull(protoCurve);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.NurbsCurve>(protoCurve);

            var protoSpline = (Autodesk.DesignScript.Geometry.NurbsCurve)protoCurve;

            protoSpline.StartPoint.AssertShouldBeApproximately(pts[0]);
            protoSpline.EndPoint.AssertShouldBeApproximately(pts.Last());

            protoSpline.TangentAtParameter(0.0).AssertShouldBeApproximately(revitSpline.Tangents[0]);
            protoSpline.TangentAtParameter(1.0).AssertShouldBeApproximately(revitSpline.Tangents.Last());

            var tessPts = revitSpline.Tessellate();

            // assert the tesselation is very close to original curve
            // what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = protoSpline.GetClosestPoint(pt.ToPoint());
                Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            }

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Line_Basic()
        {
            var s = new Autodesk.Revit.DB.XYZ(5, 2, 3);
            var e = new Autodesk.Revit.DB.XYZ(10,20,1);

            var rl = Autodesk.Revit.DB.Line.CreateBound(s, e);

            var pc = rl.ToProtoType();


            Assert.NotNull(pc);

            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Line>(pc);

            var pl = (Autodesk.DesignScript.Geometry.Line) pc;

            Assert.AreEqual(rl.GetEndPoint(0).ToPoint(), pl.StartPoint );
            Assert.AreEqual(rl.GetEndPoint(1).ToPoint(), pl.EndPoint );

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Arc_Basic()
        {
            var o = new Autodesk.Revit.DB.XYZ(5,2,3);
            var x = (new Autodesk.Revit.DB.XYZ(0, 0, 1)).Normalize();
            var y = (new Autodesk.Revit.DB.XYZ(0,-1, 0)).Normalize();
            var r = 10;
            var sp = 0.1;
            var ep = 2.5;

            var re = Autodesk.Revit.DB.Arc.Create(o, r, sp, ep, x, y);

            var pc = re.ToProtoType();

            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Arc>(pc);
            var pa = (Autodesk.DesignScript.Geometry.Arc)pc;

            (pa.SweepAngle * Math.PI / 180).AssertShouldBeApproximately(ep - sp);

            pa.StartPoint.AssertShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.AssertShouldBeApproximately(re.GetEndPoint(1));

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void EllipseArc_Basic()
        {
            var c = new Autodesk.Revit.DB.XYZ(5, 2, 3);
            var rx = 10;
            var ry = 2.5;
            var x = new Autodesk.Revit.DB.XYZ(1, 0, 0);
            var y = new Autodesk.Revit.DB.XYZ(0, 1, 0);
            var sp = Math.PI/4;
            var ep = 3 * Math.PI / 4;

            var re = Autodesk.Revit.DB.Ellipse.Create(c, rx, ry, x, y, sp, ep);
            re.MakeBound(sp, ep);

            var pc = re.ToProtoType();

            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.EllipseArc>(pc);
            var pa = (Autodesk.DesignScript.Geometry.EllipseArc)pc;

            pa.StartAngle.ToRadians().AssertShouldBeApproximately(sp);
            (pa.SweepAngle.ToRadians()).AssertShouldBeApproximately(ep - sp);

            pa.StartPoint.AssertShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.AssertShouldBeApproximately(re.GetEndPoint(1));

            pa.MajorAxis.Length.AssertShouldBeApproximately(rx);
            pa.MinorAxis.Length.AssertShouldBeApproximately(ry);
            pa.CenterPoint.AssertShouldBeApproximately(re.Center);

            var tessPts = re.Tessellate();

            // assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = pa.GetClosestPoint(pt.ToPoint());
                Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Ellipse_Basic()
        {
            var c = new Autodesk.Revit.DB.XYZ(5, 2, 3);
            var rx = 10;
            var ry = 2.5;
            var x = new Autodesk.Revit.DB.XYZ(1,0,0);
            var y = new Autodesk.Revit.DB.XYZ(0,1,0);
            var sp = 0;
            var ep = Math.PI * 2;

            var re = Autodesk.Revit.DB.Ellipse.Create(c, rx, ry, x, y, sp, ep);
            re.MakeBound(sp, ep);

            var pc = re.ToProtoType();
            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Ellipse>(pc);
            var pa = (Autodesk.DesignScript.Geometry.Ellipse) pc;

            // no elliptical arcs yet
            //(pa.SweepAngle * Math.PI / 180).AssertShouldBeApproximately(ep - sp);

            pa.StartPoint.AssertShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.AssertShouldBeApproximately(re.GetEndPoint(1));

            pa.MajorAxis.Length.AssertShouldBeApproximately(rx);
            pa.MinorAxis.Length.AssertShouldBeApproximately(ry);
            pa.CenterPoint.AssertShouldBeApproximately(re.Center);

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CylindricalHelix_Basic()
        {
            Assert.Inconclusive();
        }

    }
}

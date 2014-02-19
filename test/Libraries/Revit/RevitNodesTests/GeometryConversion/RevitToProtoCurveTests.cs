using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    public class RevitToProtoCurveTests
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
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
            // Assert.AreEqual(revitSpline.Weights.Cast<double>().Count(), protoSpline.Weights.Count);

            //var tessPts = revitSpline.Tessellate();

            //// assert the tesselation is very close to original curve
            //// what's the best tolerance to use here?
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = protoSpline.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}
        }

        [Test]
        public void HermiteSpline_Basic()
        {
            var pts = new List<Autodesk.Revit.DB.XYZ>()
            {
                new Autodesk.Revit.DB.XYZ(1,0,0),
                new Autodesk.Revit.DB.XYZ(1,1,0),
                new Autodesk.Revit.DB.XYZ(0,1,0)
            };

            var ts = new Autodesk.Revit.DB.HermiteSplineTangents
            {
                StartTangent = new Autodesk.Revit.DB.XYZ(1, 0, 0),
                EndTangent = new Autodesk.Revit.DB.XYZ(0, 1, 0)
            };

            var revitSpline = Autodesk.Revit.DB.HermiteSpline.Create(pts, false, ts);

            var protoCurve = revitSpline.ToProtoType();

            Assert.NotNull(protoCurve);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.NurbsCurve>(protoCurve);

            var protoSpline = (Autodesk.DesignScript.Geometry.NurbsCurve)protoCurve;

            Assert.AreEqual( 2, protoSpline.Degree );
            var start = protoSpline.StartPoint;
            var end = protoSpline.EndPoint;
            var startT = protoSpline.TangentAtParameter(0.0);
            var endT = protoSpline.TangentAtParameter(1.0);

            Assert.AreEqual(pts[0].X, start.X, 1e-6);
            Assert.AreEqual(pts[0].Y, start.Y, 1e-6);
            Assert.AreEqual(pts[0].Z, start.Z, 1e-6);

            Assert.AreEqual(pts[2].X, end.X, 1e-6);
            Assert.AreEqual(pts[2].Y, end.Y, 1e-6);
            Assert.AreEqual(pts[2].Z, end.Z, 1e-6);

            Assert.AreEqual(ts.EndTangent.X, endT.X, 1e-6);
            Assert.AreEqual(ts.EndTangent.Y, endT.Y, 1e-6);
            Assert.AreEqual(ts.EndTangent.Z, endT.Z, 1e-6);

            Assert.AreEqual(ts.StartTangent.X, startT.X, 1e-6);
            Assert.AreEqual(ts.StartTangent.Y, startT.Y, 1e-6);
            Assert.AreEqual(ts.StartTangent.Z, startT.Z, 1e-6);

            //var tessPts = revitSpline.Tessellate();

            //// assert the tesselation is very close to original curve
            //// what's the best tolerance to use here?
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = protoSpline.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}

        }

        [Test]
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

            Assert.AreEqual(sp, pa.StartAngle * 180 / Math.PI);
            Assert.AreEqual(ep, pa.SweepAngle * 180 / Math.PI);

            Assert.AreEqual(x.ToVector(), pa.ContextCoordinateSystem.XAxis);
            Assert.AreEqual(y.ToVector(), pa.ContextCoordinateSystem.YAxis);

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}
        }

        [Test]
        public void Ellipse_Basic()
        {
            var c = new Autodesk.Revit.DB.XYZ(5, 2, 3);
            var rx = 10;
            var ry = 2.5;
            var x = new Autodesk.Revit.DB.XYZ(1,0,0);
            var y = new Autodesk.Revit.DB.XYZ(0,1,0);
            var sp = 0.1;
            var ep = 2.5;

            var re = Autodesk.Revit.DB.Ellipse.Create(c, rx, ry, x, y, 0.1, 2.5);

            var pc = re.ToProtoType();
            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Arc>(pc);
            var pa = (Autodesk.DesignScript.Geometry.Arc) pc;

            Assert.AreEqual(sp, pa.StartAngle);
            Assert.AreEqual(ep, pa.SweepAngle);
            Assert.AreEqual(rx, pa.ContextCoordinateSystem.XAxis.Length);
            Assert.AreEqual(ry, pa.ContextCoordinateSystem.YAxis.Length);

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint());
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint()), 1e-6);
            //}
        }

        [Test]
        public void CylindricalHelix_Basic()
        {
            Assert.Inconclusive();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Revit.GeometryConversion;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    public class RevitToProtoCurveTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ToProtoType_ExtensionMethod_DoesExpectedUnitConversion()
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

            var protoSpline = (Autodesk.DesignScript.Geometry.NurbsCurve)protoCurve;

            Assert.AreEqual(revitSpline.Degree, protoSpline.Degree);

            Assert.IsTrue(protoSpline.IsRational);
            Assert.AreEqual(revitSpline.CtrlPoints.Count, protoSpline.ControlPoints().Count());
            Assert.AreEqual(revitSpline.Weights.Cast<double>().Count(), protoSpline.Weights().Length);

            // We scale the tesselation for comparison
            var feetToMeters = 0.3048;
            var tessPtsProto = revitSpline.Tessellate().Select(x => x.ToPoint(false).Scale(feetToMeters)).Cast<Autodesk.DesignScript.Geometry.Point>();

            // assert the tesselation is very close to original curve
            foreach (var pt in tessPtsProto)
            {
                var closestPt = protoSpline.ClosestPointTo(pt);
                Assert.Less(closestPt.DistanceTo(pt), 1e-6);
            }
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

            var protoCurve = revitSpline.ToProtoType(false);

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
                var closestPt = protoSpline.ClosestPointTo(pt.ToPoint(false));
                Assert.Less(closestPt.DistanceTo(pt.ToPoint(false)), 1e-6);
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

            var protoCurve = revitSpline.ToProtoType(false);

            Assert.NotNull(protoCurve);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.NurbsCurve>(protoCurve);

            var protoSpline = (Autodesk.DesignScript.Geometry.NurbsCurve)protoCurve;

            protoSpline.StartPoint.ShouldBeApproximately(pts[0]);
            protoSpline.EndPoint.ShouldBeApproximately(pts.Last());

            protoSpline.TangentAtParameter(0.0).ShouldBeApproximately(revitSpline.Tangents[0]);
            protoSpline.TangentAtParameter(1.0).ShouldBeApproximately(revitSpline.Tangents.Last());

            var tessPts = revitSpline.Tessellate();

            // assert the tesselation is very close to original curve
            // what's the best tolerance to use here?
            foreach (var pt in tessPts)
            {
                var closestPt = protoSpline.ClosestPointTo(pt.ToPoint(false));
                Assert.Less(closestPt.DistanceTo(pt.ToPoint(false)), 1e-6);
            }

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Line_Basic()
        {
            var s = new Autodesk.Revit.DB.XYZ(5, 2, 3);
            var e = new Autodesk.Revit.DB.XYZ(10,20,1);

            var rl = Autodesk.Revit.DB.Line.CreateBound(s, e);

            var pc = rl.ToProtoType(false);


            Assert.NotNull(pc);

            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Line>(pc);

            var pl = (Autodesk.DesignScript.Geometry.Line) pc;

            Assert.AreEqual(rl.GetEndPoint(0).ToPoint(false), pl.StartPoint );
            Assert.AreEqual(rl.GetEndPoint(1).ToPoint(false), pl.EndPoint );

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

            var pc = re.ToProtoType(false);

            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Arc>(pc);
            var pa = (Autodesk.DesignScript.Geometry.Arc)pc;

            (pa.SweepAngle * Math.PI / 180).ShouldBeApproximately(ep - sp);

            pa.StartPoint.ShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.ShouldBeApproximately(re.GetEndPoint(1));

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint(false));
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint(false)), 1e-6);
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

            var pc = re.ToProtoType(false);

            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.EllipseArc>(pc);
            var pa = (Autodesk.DesignScript.Geometry.EllipseArc)pc;

            pa.StartAngle.ToRadians().ShouldBeApproximately(sp);
            (pa.SweepAngle.ToRadians()).ShouldBeApproximately(ep - sp);

            pa.StartPoint.ShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.ShouldBeApproximately(re.GetEndPoint(1));

            pa.MajorAxis.Length.ShouldBeApproximately(rx);
            pa.MinorAxis.Length.ShouldBeApproximately(ry);
            pa.CenterPoint.ShouldBeApproximately(re.Center);

            var tessPts = re.Tessellate();

            // assert the tesselation is very close to original curve
            foreach (var pt in tessPts)
            {
                var closestPt = pa.ClosestPointTo(pt.ToPoint(false));
                Assert.Less(closestPt.DistanceTo(pt.ToPoint(false)), 1e-6);
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

            var pc = re.ToProtoType(false);
            Assert.NotNull(pc);
            Assert.IsAssignableFrom<Autodesk.DesignScript.Geometry.Ellipse>(pc);
            var pa = (Autodesk.DesignScript.Geometry.Ellipse) pc;

            // no elliptical arcs yet
            //(pa.SweepAngle * Math.PI / 180).AssertShouldBeApproximately(ep - sp);

            pa.StartPoint.ShouldBeApproximately(re.GetEndPoint(0));
            pa.EndPoint.ShouldBeApproximately(re.GetEndPoint(1));

            pa.MajorAxis.Length.ShouldBeApproximately(rx);
            pa.MinorAxis.Length.ShouldBeApproximately(ry);
            pa.CenterPoint.ShouldBeApproximately(re.Center);

            //var tessPts = re.Tessellate();

            //// assert the tesselation is very close to original curve
            //foreach (var pt in tessPts)
            //{
            //    var closestPt = pa.ClosestPointTo(pt.ToPoint(false));
            //    Assert.Less(closestPt.DistanceTo(pt.ToPoint(false)), 1e-6);
            //}
        }

        [Test, Ignore, Category("Failure")]
        [TestModel(@".\empty.rfa")]
        public void CylindricalHelix_Basic()
        {
            Assert.Inconclusive();
        }

    }
}

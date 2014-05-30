using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class ProtoToRevitCurve
    {
        /// <summary>
        /// A PolyCurve is not a curve, this is a special extension method to convert to a Revit CurveLoop
        /// </summary>
        /// <param name="pcrv"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.CurveLoop ToRevitType(this Autodesk.DesignScript.Geometry.PolyCurve pcrv)
        {
            if (!pcrv.IsClosed)
            {
                throw new Exception("The input PolyCurve must be closed");
            }

            var cl = new CurveLoop();

            var crvs = pcrv.Curves();

            foreach (Autodesk.DesignScript.Geometry.Curve curve in crvs)
            {
                Autodesk.Revit.DB.Curve converted = curve.ToNurbsCurve().ToRevitType();
                cl.Append(converted);
            }

            return cl;

        }

        /// <summary>
        /// An extension method for DesignScript.Geometry.Curve to convert to the analogous revit type
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Curve ToRevitType(this Autodesk.DesignScript.Geometry.Curve crv)
        {
            dynamic dyCrv = crv;

            return ProtoToRevitCurve.Convert(dyCrv);

        }

        /// <summary>
        /// Convert a DS BSplineCurve to a Revit NurbSpline
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Curve Convert(Autodesk.DesignScript.Geometry.NurbsCurve crv)
        {
            if (crv.Degree == 1 && crv.ControlPoints().Length == 2 && !crv.IsRational)
            {
                return Autodesk.Revit.DB.Line.CreateBound(crv.ControlPoints()[0].ToXyz(), 
                    crv.ControlPoints()[1].ToXyz());
            }

            if (crv.Degree <= 2)
            {
                throw new Exception("Could not convert the curve to a Revit curve because it is less than or equal to degree 2.");
            }

            // presumably checking if the curve is circular is quite expensive, we don't do it
            return Autodesk.Revit.DB.NurbSpline.Create(crv.ControlPoints().ToXyzs(),
                crv.Weights(),
                crv.Knots(),
                crv.Degree,
                crv.IsClosed,
                crv.IsRational );
        }

        /// <summary>
        /// Convert a DS Arc to a Revit arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Arc arc)
        {
            // convert
            var center = arc.CenterPoint.ToXyz();
            var sp = arc.StartPoint.ToXyz();

            // get the xaxis of the arc base plane
            var x = (sp - center).Normalize();

            // get a second vector in the plane
            var vecY = (arc.PointAtParameter(0.1).ToXyz() - center);

            // get the normal to the plane
            var n2 = x.CrossProduct(vecY).Normalize();

            // obtain the y axis in the plane - perp to x and z
            var y = n2.CrossProduct(x);

            var plane = new Autodesk.Revit.DB.Plane(x, y, center);
            return Autodesk.Revit.DB.Arc.Create(plane, arc.Radius, 0, arc.SweepAngle.ToRadians());
        }

        /// <summary>
        /// Convert a DS Circle to a Revit Arc
        /// </summary>
        /// <param name="circ"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Circle circ)
        {
            // convert
            var center = circ.CenterPoint.ToXyz();
            var sp = circ.StartPoint.ToXyz();

            // get the xaxis of the arc base plane normalized
            var x = (sp - center).Normalize();

            // get a second vector in the plane
            var vecY = (circ.PointAtParameter(0.1).ToXyz() - center);

            // get the normal to the plane
            var n2 = x.CrossProduct(vecY).Normalize();

            // obtain the y axis in the plane - perp to x and z
            var y = n2.CrossProduct(x);

            var plane = new Autodesk.Revit.DB.Plane(x, y, center);
            return Autodesk.Revit.DB.Arc.Create(plane, circ.Radius, 0, 2 * System.Math.PI);
        }

        /// <summary>
        /// Convert a DS Line to a Revit line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Line Convert(Autodesk.DesignScript.Geometry.Line line)
        {
            return Autodesk.Revit.DB.Line.CreateBound(line.StartPoint.ToXyz(), line.EndPoint.ToXyz());
        }

        /// <summary>
        /// Convert a DS Helix to a Revit Helix
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.CylindricalHelix Convert(Autodesk.DesignScript.Geometry.Helix crv)
        {
            var sp = crv.StartPoint.ToXyz();
            var ap = crv.AxisPoint.ToXyz();
            var ad = crv.AxisDirection.ToXyz().Normalize();
            var x = (sp - ap).Normalize();
            var p = crv.Pitch;
            var a = crv.Angle.ToRadians();

            return Autodesk.Revit.DB.CylindricalHelix.Create(ap, crv.Radius, x, ad, p, 0, a);
        }

        /// <summary>
        /// Convert a DS Ellipse to a Revit ellipse
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Ellipse Convert(Autodesk.DesignScript.Geometry.Ellipse crv)
        {
            var center = crv.CenterPoint.ToXyz();
            var x = crv.MajorAxis.ToXyz().Normalize();
            var y = crv.MinorAxis.ToXyz().Normalize();
            var xw = crv.MajorAxis.Length;
            var yw = crv.MinorAxis.Length;

            var e = Autodesk.Revit.DB.Ellipse.Create(center, xw, yw, x, y, 0, 2*Math.PI);
            e.MakeBound(0, 2* Math.PI );
            return e;
        }

        /// <summary>
        /// Convert a DS EllipseArc to a Revit Ellipse
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Ellipse Convert(Autodesk.DesignScript.Geometry.EllipseArc crv)
        {
            var center = crv.CenterPoint.ToXyz();
            var x = crv.MajorAxis.ToXyz().Normalize();
            var y = crv.MinorAxis.ToXyz().Normalize();
            var xw = crv.MajorAxis.Length;
            var yw = crv.MinorAxis.Length;
            var sa = crv.StartAngle.ToRadians();
            var ea = sa + crv.SweepAngle.ToRadians();

            var e = Autodesk.Revit.DB.Ellipse.Create(center, xw, yw, x, y, sa, ea);
            e.MakeBound(sa, ea);
            return e;
        }

        /// <summary>
        /// Convert a generic Circle to a Revit Curve
        /// </summary>
        /// <param name="crvCurve"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Curve Convert(Autodesk.DesignScript.Geometry.Curve crvCurve)
        {
           Autodesk.DesignScript.Geometry.Curve[] curves = crvCurve.ApproximateWithArcAndLineSegments();
           if (curves.Length == 1)
           {
              //line or arc?
              var point0 = crvCurve.PointAtParameter(0.0);
              var point1 = crvCurve.PointAtParameter(1.0);
              var pointMid = crvCurve.PointAtParameter(0.5);
              if (point0.DistanceTo(point1) > 1e-7)
              {
                 var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(point0, point1);
                 if (pointMid.DistanceTo(line) < 1e-7)
                    return Convert(line);
              }
              //then arc
              if (point0.DistanceTo(point1) < 1e-7)
                 point1 = crvCurve.PointAtParameter(0.9);
              var arc = Autodesk.DesignScript.Geometry.Arc.ByThreePoints(point0, pointMid, point1);
              return Convert(arc);
           }
   
           return Convert(crvCurve.ToNurbsCurve());
        }

    }
}

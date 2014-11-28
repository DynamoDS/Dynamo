using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.DesignScript.Runtime;
using ProtoPt = Autodesk.DesignScript.Geometry.Point;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class ProtoToRevitCurve
    {
        public static Autodesk.Revit.DB.Curve ToRevitType(this Autodesk.DesignScript.Geometry.Curve crv,
            bool performHostUnitConversion = true)
        {
            Autodesk.Revit.DB.Curve converted = null;
            if (performHostUnitConversion)
            {
                var newCrv = crv.InHostUnits();
                dynamic dyCrv = newCrv;
                converted = ProtoToRevitCurve.Convert(dyCrv);
                newCrv.Dispose();
            }
            else
            {
                dynamic dyCrv = crv;
                converted = ProtoToRevitCurve.Convert(dyCrv);
            }

            if (converted == null)
            {
                throw new Exception("An unexpected failure occurred when attempting to convert the curve");
            }

            return converted;
        }

        public static Autodesk.Revit.DB.CurveLoop ToRevitType(this Autodesk.DesignScript.Geometry.PolyCurve pcrv,
            bool performHostUnitConversion = true)
        {
            if (!pcrv.IsClosed)
            {
                throw new Exception("The input PolyCurve must be closed");
            }

            Autodesk.DesignScript.Geometry.Curve[] crvs = null;
            if (performHostUnitConversion)
            {
                pcrv = pcrv.InHostUnits();
                crvs = pcrv.Curves();
                pcrv.Dispose();
            }
            else
            {
                crvs = pcrv.Curves();
            }

            var cl = new CurveLoop();
            foreach (Autodesk.DesignScript.Geometry.Curve curve in crvs)
            {
                using (var nc = curve.ToNurbsCurve())
                {
                    Autodesk.Revit.DB.Curve converted = nc.ToRevitType(false);
                    cl.Append(converted);
                }
            }

            crvs.ForEach(x => x.Dispose());

            return cl;
        }

        #region Conversions


        private static Autodesk.Revit.DB.Curve Convert(Autodesk.DesignScript.Geometry.NurbsCurve crv)
        {
            // line
            if (crv.Degree == 1 && crv.ControlPoints().Length == 2 && !crv.IsRational)
            {
                return Autodesk.Revit.DB.Line.CreateBound(crv.ControlPoints()[0].ToXyz(false), 
                    crv.ControlPoints()[1].ToXyz(false));
            }

            // polyline - not allowed
            if (crv.Degree == 1)
            {
                throw new Exception(
                    "Degree 1 Nurbs Curves are not allowed in Revit!  Try splitting the curve into "
                        +
                        "individual linear pieces");
            }

            // bezier
            if (crv.Degree == 2 && crv.ControlPoints().Count() == 3 && !crv.IsRational)
            {
                using (var converted = NurbsUtils.ElevateBezierDegree(crv, 3))
                {
                    return Autodesk.Revit.DB.NurbSpline.Create(converted.ControlPoints().ToXyzs(false),
                            converted.Weights(),
                            converted.Knots(),
                            converted.Degree,
                            converted.IsClosed,
                            converted.IsRational);
                }
            }

            // degree 2 curve
            if (crv.Degree == 2)
            {
                // TODO: general NURBS degree elevation
                var numSamples = crv.ControlPoints().Count() + 1;
                var pts = Enumerable.Range(0, numSamples+1).Select(x => x/(double)numSamples)
                    .Select(crv.PointAtParameter);

                var tstart = crv.TangentAtParameter(0);
                var tend = crv.TangentAtParameter(1);

                using (var resampledCrv = NurbsCurve.ByPointsTangents(pts, tstart.Normalized(), tend.Normalized()))
                {

                    return Autodesk.Revit.DB.NurbSpline.Create(resampledCrv.ControlPoints().ToXyzs(false),
                            resampledCrv.Weights(),
                            resampledCrv.Knots(),
                            resampledCrv.Degree,
                            resampledCrv.IsClosed,
                            resampledCrv.IsRational);
                }
            }

            // general implementation
            return Autodesk.Revit.DB.NurbSpline.Create(crv.ControlPoints().ToXyzs(false),
                crv.Weights(),
                crv.Knots(),
                crv.Degree,
                crv.IsClosed,
                crv.IsRational);

        }

        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Arc arc)
        {
            var sp = arc.StartPoint.ToXyz(false);
            var ep = arc.EndPoint.ToXyz(false);
            var mp = arc.PointAtParameter(0.5).ToXyz(false);

            return Autodesk.Revit.DB.Arc.Create(sp, ep, mp);
        }

        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Circle circ)
        {
            // convert
            var center = circ.CenterPoint.ToXyz(false);
            var sp = circ.StartPoint.ToXyz(false);

            // get the xaxis of the arc base plane normalized
            var x = (sp - center).Normalize();

            // get a second vector in the plane
            var vecY = (circ.PointAtParameter(0.1).ToXyz(false) - center);

            // get the normal to the plane
            var n2 = x.CrossProduct(vecY).Normalize();

            // obtain the y axis in the plane - perp to x and z
            var y = n2.CrossProduct(x);

            var plane = new Autodesk.Revit.DB.Plane(x, y, center);
            return Autodesk.Revit.DB.Arc.Create(plane, circ.Radius, 0, 2 * System.Math.PI);
        }

        private static Autodesk.Revit.DB.Line Convert(Autodesk.DesignScript.Geometry.Line line)
        {
            return Autodesk.Revit.DB.Line.CreateBound(line.StartPoint.ToXyz(false), line.EndPoint.ToXyz(false));
        }

        private static Autodesk.Revit.DB.CylindricalHelix Convert(Autodesk.DesignScript.Geometry.Helix crv)
        {
            var sp = crv.StartPoint.ToXyz(false);
            var ap = crv.AxisPoint.ToXyz(false);
            var ad = crv.AxisDirection.ToXyz(false).Normalize();
            var x = (sp - ap).Normalize();
            var p = crv.Pitch;
            var a = crv.Angle.ToRadians();

            return Autodesk.Revit.DB.CylindricalHelix.Create(ap, crv.Radius, x, ad, p, 0, a);
        }

        private static Autodesk.Revit.DB.Ellipse Convert(Autodesk.DesignScript.Geometry.Ellipse crv)
        {
            var center = crv.CenterPoint.ToXyz(false);
            var x = crv.MajorAxis.ToXyz(false).Normalize();
            var y = crv.MinorAxis.ToXyz(false).Normalize();
            var xw = crv.MajorAxis.Length;
            var yw = crv.MinorAxis.Length;

            var e = Autodesk.Revit.DB.Ellipse.Create(center, xw, yw, x, y, 0, 2*Math.PI);
            e.MakeBound(0, 2* Math.PI );
            return e;
        }

        private static Autodesk.Revit.DB.Ellipse Convert(Autodesk.DesignScript.Geometry.EllipseArc crv)
        {
            var center = crv.CenterPoint.ToXyz(false);
            var x = crv.MajorAxis.ToXyz(false).Normalize();
            var y = crv.MinorAxis.ToXyz(false).Normalize();
            var xw = crv.MajorAxis.Length;
            var yw = crv.MinorAxis.Length;
            var sa = crv.StartAngle.ToRadians();
            var ea = sa + crv.SweepAngle.ToRadians();

            var e = Autodesk.Revit.DB.Ellipse.Create(center, xw, yw, x, y, sa, ea);
            e.MakeBound(sa, ea);
            return e;
        }

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
                  using (var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(point0, point1))
                  {
                      if (pointMid.DistanceTo(line) < 1e-7)
                      {
                          var result = Convert(line);
                          curves[0].Dispose();
                          return result;
                      }
                  }
              }
              //then arc
              if (point0.DistanceTo(point1) < 1e-7)
                 point1 = crvCurve.PointAtParameter(0.9);
              var arc = Autodesk.DesignScript.Geometry.Arc.ByThreePoints(point0, pointMid, point1);
              var resultArc = Convert(arc);
              arc.Dispose();
              curves[0].Dispose();
              return resultArc;
           }
           curves.ForEach(x => x.Dispose());

           using (var nc = crvCurve.ToNurbsCurve())
           {
               return Convert(nc);
           }
        }

        #endregion
    }
}

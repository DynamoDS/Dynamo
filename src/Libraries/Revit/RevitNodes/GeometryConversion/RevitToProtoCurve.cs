using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Arc = Autodesk.DesignScript.Geometry.Arc;
using Plane = Autodesk.DesignScript.Geometry.Plane;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class RevitToProtoCurve
    {

        /// <summary>
        /// An extension method to convert a Revit Curve to a ProtoGeometry Curve.  Note that Bound Revit curves will be returned in trimmed form.
        /// </summary>
        /// <param name="revitCurve"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Curve ToProtoType(this Autodesk.Revit.DB.Curve revitCurve)
        {
            if (revitCurve == null) throw new ArgumentNullException("revitCurve");

            dynamic dyCrv = revitCurve;
            return RevitToProtoCurve.Convert(dyCrv);
        }

        public static PolyCurve ToProtoTypes(this Autodesk.Revit.DB.CurveArray revitCurves)
        {
            if (revitCurves == null) throw new ArgumentNullException("revitCurves");

            var protoCurves = revitCurves.Cast<Autodesk.Revit.DB.Curve>().Select(x => x.ToProtoType());
            return PolyCurve.ByJoinedCurves(protoCurves.ToArray());
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.NurbSpline crv)
        {
            var convert = NurbsCurve.ByControlPointsWeightsKnots(crv.CtrlPoints.Select(x => x.ToPoint()).ToArray(), 
                crv.Weights.Cast<double>().ToArray(), crv.Knots.Cast<double>().ToArray(), crv.Degree );

            if (!crv.IsBound) return convert;

            // bound the curve parametric range
            
            // we first get the full parametric domain from the knots
            // note that some knots be negative and the domain may appear reversed
            var parms = crv.Knots.Cast<double>().ToList();
            var fsp = parms.First();
            var fep = parms.Last();

            // obtain the full domain
            var fd = Math.Abs(fsp - fep);

            // these are the start and end parameters of the bound curve
            var sp = crv.get_EndParameter(0);
            var ep = crv.get_EndParameter(1);

            // get the normalized parameters for trim
            var nsp = Math.Abs(fsp - sp) / fd;
            var nep = Math.Abs(fsp - ep) / fd;

            return convert.ParameterTrim(nsp, nep);
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.HermiteSpline crv)
        {

            var convert = HermiteToNurbs.ConvertExact(crv);

            if (!crv.IsBound) return convert;

            // bound the curve parametric range

            // we first get the full parametric domain from the parameters
            // note that some knots be negative and the domain may appear reversed
            var parms = crv.Parameters.Cast<double>().ToList();
            var fsp = parms.First();
            var fep = parms.Last();

            // obtain the full domain
            var fd = Math.Abs(fsp - fep);

            // these are the start and end parameters of the bound curve
            var sp = crv.get_EndParameter(0);
            var ep = crv.get_EndParameter(1);

            // get the normalized parameters for trim
            var nsp = Math.Abs(fsp - sp)/fd;
            var nep = Math.Abs(fsp - ep)/fd;

            return convert.ParameterTrim(nsp, nep);

        }

        private static Autodesk.DesignScript.Geometry.Line Convert(Autodesk.Revit.DB.Line crv)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                 crv.GetEndPoint(0).ToPoint(), crv.GetEndPoint(1).ToPoint());
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Arc crv)
        {
            var isCircle = !crv.IsBound ||
                           Math.Abs(Math.Abs(crv.GetEndParameter(1) - crv.GetEndParameter(0)) - 2*Math.PI) < 1e-6;

            if ( isCircle )
            {
                return Circle.ByCenterPointRadiusNormal(crv.Center.ToPoint(), crv.Radius, crv.Normal.ToVector());
            }

            return Arc.ByCenterPointStartPointSweepAngle(crv.Center.ToPoint(), crv.GetEndPoint(0).ToPoint(),
                (crv.GetEndParameter(1) - crv.GetEndParameter(0))*180/Math.PI, crv.Normal.ToVector());
        }

        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.PolyLine crv)
        {
            return
                Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(
                    crv.GetCoordinates().Select(x => x.ToPoint()).ToArray(), 1);
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Ellipse crv)
        {
            var isComplete = !crv.IsBound ||
                             Math.Abs(Math.Abs(crv.GetEndParameter(1) - crv.GetEndParameter(0)) - 2*Math.PI) < 1e-6;

            if (!isComplete)
            {
                var pl = Plane.ByOriginXAxisYAxis(crv.Center.ToPoint(),
                    crv.XDirection.ToVector(), crv.YDirection.ToVector());

                var s = crv.GetEndParameter(0).ToDegrees();
                var e = crv.GetEndParameter(1).ToDegrees();

                return EllipseArc.ByPlaneRadiiStartAngleSweepAngle(pl, crv.RadiusX, crv.RadiusY, s, e - s);
            }

            return Autodesk.DesignScript.Geometry.Ellipse.ByOriginVectors(crv.Center.ToPoint(),
                (crv.XDirection*crv.RadiusX).ToVector(), (crv.YDirection*crv.RadiusY).ToVector());
        }

        private static Autodesk.DesignScript.Geometry.Helix Convert(Autodesk.Revit.DB.CylindricalHelix crv)
        {
            if (crv.IsRightHanded)
            {
                // a negative pitch and axis vector produces helix in opposite direction
                return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(), (-1.0 * crv.ZVector).ToVector(),
                    crv.GetEndPoint(0).ToPoint(), -crv.Pitch, (crv.Height / crv.Pitch) * 360.0);
            }

            // clockwise is default
            return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(), crv.ZVector.ToVector(),
                crv.GetEndPoint(0).ToPoint(), crv.Pitch, (crv.Height/crv.Pitch)*360.0);
        }
    }
}

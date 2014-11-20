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
using Revit.GeometryReferences;
using Arc = Autodesk.DesignScript.Geometry.Arc;
using Plane = Autodesk.DesignScript.Geometry.Plane;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class RevitToProtoCurve
    {
        public static Autodesk.DesignScript.Geometry.Curve ToProtoType(this Autodesk.Revit.DB.Curve revitCurve, 
            bool performHostUnitConversion = true, Reference referenceOverride = null)
        {
            if (revitCurve == null)
            {
                throw new ArgumentNullException("revitCurve");
            }

            dynamic dyCrv = revitCurve;
            Autodesk.DesignScript.Geometry.Curve converted = RevitToProtoCurve.Convert(dyCrv);

            if (converted == null)
            {
                throw new Exception("An unexpected failure occurred when attempting to convert the curve");
            }

            converted = performHostUnitConversion ? converted.InDynamoUnits() : converted;

            // If possible, add a geometry reference for downstream Element creation
            var revitRef = referenceOverride ?? revitCurve.Reference;
            if (revitRef != null)
            {
                converted.Tags.AddTag(ElementCurveReference.DefaultTag, revitRef);
            }

            return converted;
        }

        public static Autodesk.DesignScript.Geometry.PolyCurve ToProtoType(this Autodesk.Revit.DB.CurveArray revitCurves, 
            bool performHostUnitConversion = true)
        {
            if (revitCurves == null)
            {
                throw new ArgumentNullException("revitCurves");
            }

            var protoCurves = revitCurves.Cast<Autodesk.Revit.DB.Curve>().Select(x => x.ToProtoType(false));
            var converted = PolyCurve.ByJoinedCurves(protoCurves.ToArray());

            if (converted == null)
            {
                throw new Exception("An unexpected failure occurred when attempting to convert the curve");
            }

            return performHostUnitConversion ? converted.InDynamoUnits() : converted;
        }

        public static Autodesk.DesignScript.Geometry.PolyCurve ToProtoType(this Autodesk.Revit.DB.PolyLine geom,
            bool performHostUnitConversion = true)
        {
            var converted = PolyCurve.ByPoints(geom.GetCoordinates().Select(x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x.X, x.Y, x.Z)).ToArray());
            return performHostUnitConversion ? converted.InDynamoUnits() : converted;
        }

        #region Conversions

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.NurbSpline crv)
        {
            var convert = NurbsCurve.ByControlPointsWeightsKnots(crv.CtrlPoints.Select(x => x.ToPoint(false)).ToArray(), 
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
            var sp = crv.GetEndParameter(0);
            var ep = crv.GetEndParameter(1);

            // get the normalized parameters for trim
            var nsp = Math.Abs(fsp - sp) / fd;
            var nep = Math.Abs(fsp - ep) / fd;

            // if there's no trimming to do, avoid it
            if (Math.Abs(nsp) < 1e-6 && Math.Abs(1 - nep) < 1e-6) return convert;

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
            var sp = crv.GetEndParameter(0);
            var ep = crv.GetEndParameter(1);

            // get the normalized parameters for trim
            var nsp = Math.Abs(fsp - sp)/fd;
            var nep = Math.Abs(fsp - ep)/fd;

            // if there's no trimming to do, avoid it
            if (Math.Abs(nsp) < 1e-6 && Math.Abs(1 - nep) < 1e-6) return convert;

            return convert.ParameterTrim(nsp, nep);

        }

        private static Autodesk.DesignScript.Geometry.Line Convert(Autodesk.Revit.DB.Line crv)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                 crv.GetEndPoint(0).ToPoint(false), crv.GetEndPoint(1).ToPoint(false));
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Arc crv)
        {
            var isCircle = !crv.IsBound ||
                           Math.Abs(Math.Abs(crv.GetEndParameter(1) - crv.GetEndParameter(0)) - 2*Math.PI) < 1e-6;

            if ( isCircle )
            {
                return Circle.ByCenterPointRadiusNormal(crv.Center.ToPoint(false), crv.Radius, crv.Normal.ToVector(false));
            }

            return Arc.ByCenterPointStartPointSweepAngle(crv.Center.ToPoint(false), crv.GetEndPoint(0).ToPoint(false),
                (crv.GetEndParameter(1) - crv.GetEndParameter(0))*180/Math.PI, crv.Normal.ToVector(false));
        }

        private static Autodesk.DesignScript.Geometry.PolyCurve Convert(Autodesk.Revit.DB.PolyLine crv)
        {
            return
                Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(crv.GetCoordinates().Select(x => x.ToPoint(false)));
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Ellipse crv)
        {
            var isFullEllipse = !crv.IsBound ||
                             Math.Abs(Math.Abs(crv.GetEndParameter(1) - crv.GetEndParameter(0)) - 2*Math.PI) < 1e-6;

            if (isFullEllipse)
            {
                return
                    Autodesk.DesignScript.Geometry.Ellipse.ByOriginVectors(
                        crv.Center.ToPoint(false),
                        (crv.XDirection*crv.RadiusX).ToVector(false),
                        (crv.YDirection*crv.RadiusY).ToVector(false));
            }

            // We need to define the major and minor axis as the curve 
            // will be trimmed starting from the major axis (not the xaxis)
            var major = Math.Max(crv.RadiusX, crv.RadiusY);
            var minor = Math.Min(crv.RadiusX, crv.RadiusY);

            Vector majorAxis;
            Vector minorAxis;

            double startParam;

            var span = Math.Abs( crv.GetEndParameter(0) - crv.GetEndParameter(1)).ToDegrees();

            if (crv.RadiusX > crv.RadiusY)
            {
                majorAxis = crv.XDirection.ToVector();
                minorAxis = crv.YDirection.ToVector();
                startParam = crv.GetEndParameter(0).ToDegrees();
            }
            else
            {
                majorAxis = crv.YDirection.ToVector().Reverse();
                minorAxis = crv.XDirection.ToVector();
                startParam = crv.GetEndParameter(0).ToDegrees() + 90;
            }

            var pl = Plane.ByOriginXAxisYAxis(crv.Center.ToPoint(false), majorAxis, minorAxis);

            return EllipseArc.ByPlaneRadiiStartAngleSweepAngle(pl, major, minor, startParam, span);

        }

        private static Autodesk.DesignScript.Geometry.Helix Convert(Autodesk.Revit.DB.CylindricalHelix crv)
        {
            if (crv.IsRightHanded)
            {
                // a negative pitch and axis vector produces helix in opposite direction
                return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(false), (-1.0 * crv.ZVector).ToVector(false),
                    crv.GetEndPoint(0).ToPoint(false), -crv.Pitch, (crv.Height / crv.Pitch) * 360.0);
            }

            // clockwise is default
            return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(false), crv.ZVector.ToVector(false),
                crv.GetEndPoint(0).ToPoint(false), crv.Pitch, (crv.Height/crv.Pitch)*360.0);
        }

        #endregion

    }
}

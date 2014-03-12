using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Revit.GeometryConversion
{
    [Browsable(false)]
    public static class RevitToProtoCurve
    {

        /// <summary>
        /// An extension method to convert a Revit Curve to a ProtoGeometry Curve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Curve ToProtoType(this Autodesk.Revit.DB.Curve crv)
        {
            dynamic dyCrv = crv;
            return RevitToProtoCurve.Convert(dyCrv);
        }

        public static PolyCurve ToProtoTypes(this Autodesk.Revit.DB.CurveArray crvs)
        {
            var protoCurves = new List<Curve>();
            foreach (var crv in crvs)
            {
                dynamic dyCrv = crv;
                protoCurves.AddRange(RevitToProtoCurve.Convert(dyCrv));
            }

            return PolyCurve.ByJoinedCurves(protoCurves.ToArray());
        }

        /// <summary>
        /// Convert a Revit NurbSpline to a ProtoGeometry curve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.NurbSpline crv)
        {
            return NurbsCurve.ByControlPointsWeightsKnots(crv.CtrlPoints.Select(x => x.ToPoint()).ToArray(), crv.Weights.Cast<double>().ToArray(), crv.Knots.Cast<double>().ToArray(), crv.Degree );
        }

        /// <summary>
        /// Convert a Revit HermiteSpline to a ProtoGeometry BSplineCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.HermiteSpline crv)
        {
            return NurbsCurve.ByPointsTangents(crv.ControlPoints.Select(x => x.ToPoint()).ToArray(),
                crv.Tangents.First().ToVector(), crv.Tangents.Last().ToVector());
        }

        /// <summary>
        /// Convert a Revit Line to a ProtoGeometry Line
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Line Convert(Autodesk.Revit.DB.Line crv)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                 crv.GetEndPoint(0).ToPoint(), crv.GetEndPoint(1).ToPoint());
        }

        /// <summary>
        /// Convert a Revit Arc to a ProtoGeometry Arc
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convert a Revit PolyLine to a degree 1 ProtoGeometry BSplineCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.PolyLine crv)
        {
            return
                Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPoints(
                    crv.GetCoordinates().Select(x => x.ToPoint()).ToArray(), 1);
        }

        /// <summary>
        /// Convert a Revit Ellipse to a ProtoGeometry Ellipse
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Ellipse crv)
        {
            var isComplete = !crv.IsBound ||
                             Math.Abs(Math.Abs(crv.GetEndParameter(1) - crv.GetEndParameter(0)) - 2*Math.PI) < 1e-6;

            if (!isComplete)
            {
                throw new Exception("Could not create elliptical arc, only full ellipses are allowed.");
            }

            return Autodesk.DesignScript.Geometry.Ellipse.ByOriginVectors(crv.Center.ToPoint(),
                (crv.XDirection*crv.RadiusX).ToVector(), (crv.YDirection*crv.RadiusY).ToVector());
        }

        /// <summary>
        /// Convert a Revit CylindricalHelix to a ProtoGeometry Helix
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Helix Convert(Autodesk.Revit.DB.CylindricalHelix crv)
        {
            if (crv.IsRightHanded)
            {
                // a negative pitch and axis vector produces helix in opposite direction
                return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(), (-1.0 * crv.ZVector).ToVector(),
                    crv.GetEndPoint(0).ToPoint(), -crv.Pitch, (crv.Height / crv.Pitch) * 360.0);
            }
            else
            {
                // clockwise is default
                return Autodesk.DesignScript.Geometry.Helix.ByAxis(crv.BasePoint.ToPoint(), crv.ZVector.ToVector(),
                    crv.GetEndPoint(0).ToPoint(), crv.Pitch, (crv.Height/crv.Pitch)*360.0);
            }
        }
    }
}

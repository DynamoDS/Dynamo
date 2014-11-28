using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class CurveUtils
    {
        public static readonly double Tolerance = 1e-6;

        public static Plane GetPlaneFromCurve(Curve c, bool planarOnly)
        {
            //find the plane of the curve and generate a sketch plane
            double period = c.IsBound ? 0.0 : (c.IsCyclic ? c.Period : 1.0);

            var p0 = c.IsBound ? c.Evaluate(0.0, true) : c.Evaluate(0.0, false);
            var p1 = c.IsBound ? c.Evaluate(0.5, true) : c.Evaluate(0.25 * period, false);
            var p2 = c.IsBound ? c.Evaluate(1.0, true) : c.Evaluate(0.5 * period, false);

            if (IsLineLike(c))
            {
                XYZ norm = null;

                //keep old plane computations
                if (System.Math.Abs(p0.Z - p2.Z) < Tolerance)
                {
                    norm = XYZ.BasisZ;
                }
                else
                {
                    var v1 = p1 - p0;
                    var v2 = p2 - p0;

                    var p3 = new XYZ(p2.X, p2.Y, p0.Z);
                    var v3 = p3 - p0;
                    norm = v1.CrossProduct(v3);
                    if (norm.IsZeroLength())
                    {
                        norm = v2.CrossProduct(XYZ.BasisY);
                    }
                    norm = norm.Normalize();
                }

                return new Plane(norm, p0);

            }

            var cLoop = new CurveLoop();
            cLoop.Append(c.Clone());
            if (cLoop.HasPlane())
            {
                return cLoop.GetPlane();
            }
            if (planarOnly)
                return null;

            // Get best fit plane using tesselation
            var points = c.Tessellate().Select(x => x.ToPoint(false));

            var bestFitPlane =
                Autodesk.DesignScript.Geometry.Plane.ByBestFitThroughPoints(points);

            return bestFitPlane.ToPlane(false);
        }

        public static bool IsLineLike(Autodesk.Revit.DB.Curve crv)
        {
            if (crv is Line) return true;
            if (crv is HermiteSpline) return IsLineLikeInternal(crv as HermiteSpline);
            if (crv is NurbSpline) return IsLineLikeInternal(crv as NurbSpline);
            
            // This assumes no infinite radius arcs/ellipses
            return false;
        }

        #region IsLineLike Helpers

        private static bool IsLineLikeInternal(Autodesk.Revit.DB.NurbSpline crv)
        {
            return IsLineLikeInternal(crv.CtrlPoints);
        }

        private static bool IsLineLikeInternal(Autodesk.Revit.DB.HermiteSpline crv)
        {
            var controlPoints = crv.ControlPoints;
            var controlPointsAligned = IsLineLikeInternal(controlPoints);

            if (!controlPointsAligned) return false;

            // It's possible for the control points to be straight, but the tangents
            // might not be aligned with the curve.  Let's check the tangents are aligned.
            var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                controlPoints.First().ToPoint(false),
                controlPoints.Last().ToPoint(false));

            var lineDir = line.Direction.Normalized().ToXyz(false);
            line.Dispose();

            var tangents = crv.Tangents;
            var startTan = tangents.First().Normalize();
            var endTan = tangents.Last().Normalize();

            return Math.Abs(startTan.DotProduct(endTan) - 1) < Tolerance &&
                Math.Abs(lineDir.DotProduct(endTan) - 1) < Tolerance;
        }

        private static bool IsLineLikeInternal(IList<XYZ> points)
        {
            if (points.Count == 2) return true;

            using (var line = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                points.First().ToPoint(false),
                points.Last().ToPoint(false)))
            {
                // Are any of the points distant from the line created by connecting the two
                // end points?
                return !points.Any(x => x.ToPoint(false).DistanceTo(line) > Tolerance);
            }
        }

        #endregion

    }
}

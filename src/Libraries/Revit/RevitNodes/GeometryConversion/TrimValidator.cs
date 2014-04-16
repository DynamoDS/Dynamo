using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Face = Autodesk.Revit.DB.Face;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public static class TrimValidator
    {
        public static bool IsValidTrim(PolyCurve pc, Surface srf, Face face, double tol = 1e-5, double thresh = 0.8)
        {
            var pts = pc.DivideByLength(pc.NumberOfCurves * 2 + 1);

            var ptsLeft = new List<Point>();
            var ptsRight = new List<Point>();

            foreach (var pt in pts)
            {
                var srfnorm = srf.NormalAtPoint(pt).Normalized();
                var crvtan = pc.TangentAtPoint(pt).Normalized();

                var offsetDir = crvtan.Cross(srfnorm).Scale(0.001);
                var revoffsetDir = offsetDir.Reverse();

                ptsLeft.Add(pt.Add(offsetDir));
                ptsRight.Add(pt.Add(revoffsetDir));
            }

            var leftPointsInFace = face.MostPointsAreInside(ptsLeft, tol, thresh);
            var leftPointsInSurface = srf.MostPointsAreInside(ptsLeft, tol, thresh);
            var rightPointsInFace = face.MostPointsAreInside(ptsRight, tol, thresh);
            var rightPointsInSurface = srf.MostPointsAreInside(ptsRight, tol, thresh);

            return (leftPointsInFace && leftPointsInSurface) || (rightPointsInFace && rightPointsInSurface);
        }

        public static bool MostPointsAreInside(this Face face, IEnumerable<Point> pts, double tolerance = 1e-4, double threshold = 0.8)
        {
            var numInside = pts.Aggregate(0, (a, x) =>
            {
                var proj = face.Project(x.ToXyz());
                return a + (proj != null && proj.Distance < tolerance && face.IsInside(proj.UVPoint) ? 1 : 0);
            });

            return ((double)numInside) / pts.Count() > threshold - 1e-6;
        }

        public static bool MostPointsAreInside(this Surface surface, IEnumerable<Point> pts, double tolerance = 1e-4, double threshold = 0.8)
        {
            var numInside = pts.Aggregate(0, (a, x) => a + (surface.DistanceTo(x) < tolerance ? 1 : 0));
            return ((double)numInside) / pts.Count() > threshold - 1e-6;
        }

        private static Vector TangentAtPoint(this PolyCurve pc, Autodesk.DesignScript.Geometry.Point pt)
        {
            Vector vec = null;
            foreach (var crv in pc.Curves())
            {
                var closestPt = crv.GetClosestPoint(pt);

                if (pt.DistanceTo(closestPt) > 1e-6) continue;

                var u = crv.ParameterAtPoint(closestPt);
                vec = crv.TangentAtParameter(u);
            }

            return vec;
        }
    }
}

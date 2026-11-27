using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A straight line segment between two points.</summary>
    public class Line : Curve
    {
        private Line(NativeGeometry.CurveHandle handle, Vector direction) : base(handle)
        {
            Direction = direction;
        }

        /// <summary>Gets the non-normalized direction vector from start point to end point.</summary>
        public Vector Direction { get; }

        /// <summary>Creates a straight line segment between two points.</summary>
        /// <param name="startPoint">Start point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns>The created line segment.</returns>
        public static Line ByStartPointEndPoint(Point startPoint, Point endPoint)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(endPoint);
            var handle = NativeGeometry.Line(startPoint.Position, endPoint.Position);
            var direction = Vector.ByTwoPoints(startPoint, endPoint);
            return new Line(handle, direction);
        }

        /// <summary>Creates a straight line segment from a start point, direction vector, and length.</summary>
        /// <param name="startPoint">Start point.</param>
        /// <param name="direction">Direction vector.</param>
        /// <param name="length">Length of the segment.</param>
        /// <returns>The created line segment.</returns>
        public static Line ByStartPointDirectionLength(Point startPoint, Vector direction, double length)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(direction);
            if (!double.IsFinite(length) || length == 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Line length must be finite and non-zero.");

            var dir = direction.Normalized().Scale(length);
            var endPoint = startPoint.Add(dir);
            return ByStartPointEndPoint(startPoint, endPoint);
        }

        /// <summary>Creates a best-fit straight line through a collection of 3D points using principal-axis least squares.</summary>
        /// <param name="bestFitPoints">Collection of at least two non-coincident points.</param>
        /// <returns>The best-fit line segment spanning the projected extents of the input points.</returns>
        public static Line ByBestFitThroughPoints(IEnumerable<Point> bestFitPoints)
        {
            ArgumentNullException.ThrowIfNull(bestFitPoints);
            var pts = bestFitPoints.Where(p => p != null).ToArray();
            if (pts.Length < 2)
                throw new ArgumentException("Line.ByBestFitThroughPoints requires at least 2 points.", nameof(bestFitPoints));

            if (pts.Length == 2)
                return ByStartPointEndPoint(pts[0], pts[1]);

            double cx = 0, cy = 0, cz = 0;
            foreach (var p in pts)
            {
                cx += p.X;
                cy += p.Y;
                cz += p.Z;
            }
            cx /= pts.Length;
            cy /= pts.Length;
            cz /= pts.Length;

            // Build symmetric 3x3 covariance matrix.
            var a = new double[3, 3];
            foreach (var p in pts)
            {
                var dx = p.X - cx;
                var dy = p.Y - cy;
                var dz = p.Z - cz;
                a[0, 0] += dx * dx; a[0, 1] += dx * dy; a[0, 2] += dx * dz;
                a[1, 1] += dy * dy; a[1, 2] += dy * dz;
                a[2, 2] += dz * dz;
            }
            a[1, 0] = a[0, 1];
            a[2, 0] = a[0, 2];
            a[2, 1] = a[1, 2];

            // Jacobi eigendecomposition for symmetric 3x3 matrix.
            var v = new double[3, 3]
            {
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 }
            };
            for (var iter = 0; iter < 30; iter++)
            {
                int p = 0, q = 1;
                var maxOff = Math.Abs(a[0, 1]);
                if (Math.Abs(a[0, 2]) > maxOff) { maxOff = Math.Abs(a[0, 2]); p = 0; q = 2; }
                if (Math.Abs(a[1, 2]) > maxOff) { maxOff = Math.Abs(a[1, 2]); p = 1; q = 2; }
                if (maxOff <= 1e-14)
                    break;

                var tau = (a[q, q] - a[p, p]) / (2.0 * a[p, q]);
                var t = tau >= 0
                    ? 1.0 / (tau + Math.Sqrt(1.0 + tau * tau))
                    : -1.0 / (-tau + Math.Sqrt(1.0 + tau * tau));
                var c = 1.0 / Math.Sqrt(1.0 + t * t);
                var s = t * c;

                var app = a[p, p];
                var aqq = a[q, q];
                var apq = a[p, q];
                a[p, p] = app - t * apq;
                a[q, q] = aqq + t * apq;
                a[p, q] = a[q, p] = 0.0;

                for (var r = 0; r < 3; r++)
                {
                    if (r != p && r != q)
                    {
                        var arp = a[r, p];
                        var arq = a[r, q];
                        a[r, p] = a[p, r] = c * arp - s * arq;
                        a[r, q] = a[q, r] = s * arp + c * arq;
                    }
                    var vrp = v[r, p];
                    var vrq = v[r, q];
                    v[r, p] = c * vrp - s * vrq;
                    v[r, q] = s * vrp + c * vrq;
                }
            }

            var bestCol = 0;
            if (a[1, 1] > a[bestCol, bestCol]) bestCol = 1;
            if (a[2, 2] > a[bestCol, bestCol]) bestCol = 2;

            var dir = Vector.ByCoordinates(v[0, bestCol], v[1, bestCol], v[2, bestCol]).Normalized();
            var centroid = Point.ByCoordinates(cx, cy, cz);

            var minT = double.PositiveInfinity;
            var maxT = double.NegativeInfinity;
            foreach (var pt in pts)
            {
                var proj = Vector.ByTwoPoints(centroid, pt).Dot(dir);
                minT = Math.Min(minT, proj);
                maxT = Math.Max(maxT, proj);
            }
            if (maxT - minT <= 1e-9)
                throw new ArgumentException("Input points are coincident and cannot define a best-fit line.", nameof(bestFitPoints));

            // Preserve overall direction from first point toward last point when clear.
            if (Vector.ByTwoPoints(pts[0], pts[^1]).Dot(dir) < 0)
            {
                (minT, maxT) = (-maxT, -minT);
                dir = dir.Reverse();
            }

            return ByStartPointEndPoint(centroid.Add(dir.Scale(minT)), centroid.Add(dir.Scale(maxT)));
        }

        internal override uint PreviewSegmentCount => 1;
    }
}

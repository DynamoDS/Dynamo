using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A closed polyline polygon defined by an ordered ring of vertices.</summary>
    public class Polygon : PolyCurve
    {
        private readonly Point[] vertices;

        internal Polygon(Point[] vertices, Curve[] edges) : base(edges)
        {
            this.vertices = vertices;
        }

        /// <summary>Gets the ordered vertices of this polygon.</summary>
        public Point[] Points => vertices.ToArray();

        /// <summary>Returns the centroid (average of vertices) of this polygon.</summary>
        public Point Center()
        {
            var x = vertices.Average(v => v.X);
            var y = vertices.Average(v => v.Y);
            var z = vertices.Average(v => v.Z);
            return Point.ByCoordinates(x, y, z);
        }

        /// <summary>Creates a closed polygon from an ordered list of at least three points.</summary>
        /// <param name="points">Ordered corner points.</param>
        /// <returns>The closed polygon.</returns>
        public static Polygon ByPoints(IEnumerable<Point> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).ToList();
            if (pts.Count > 1 && pts[0].DistanceTo(pts[^1]) <= 1e-9)
                pts.RemoveAt(pts.Count - 1);
            if (pts.Count < 3)
                throw new ArgumentException("A Polygon requires at least 3 distinct points.", nameof(points));

            var edges = BuildClosedEdges(pts.ToArray());
            return new Polygon(pts.ToArray(), edges);
        }

        /// <summary>Creates a regular polygon inscribed in <paramref name="circle"/>.</summary>
        /// <param name="circle">Circumscribed circle.</param>
        /// <param name="numberSides">Number of sides (at least 3).</param>
        /// <returns>The regular polygon.</returns>
        public static Polygon RegularPolygon(Circle circle, int numberSides)
        {
            ArgumentNullException.ThrowIfNull(circle);
            if (numberSides < 3)
                throw new ArgumentOutOfRangeException(nameof(numberSides), "A regular polygon requires at least 3 sides.");

            var pts = new Point[numberSides];
            var plane = circle.Plane;
            var r = circle.Radius;
            for (var i = 0; i < numberSides; i++)
            {
                var angle = 2.0 * Math.PI * i / numberSides;
                var dx = Math.Cos(angle) * r;
                var dy = Math.Sin(angle) * r;
                pts[i] = plane.Origin
                    .Add(plane.XAxis.Scale(dx))
                    .Add(plane.YAxis.Scale(dy));
            }
            return new Polygon(pts, BuildClosedEdges(pts));
        }

        internal static Curve[] BuildClosedEdges(Point[] pts)
        {
            var edges = new Curve[pts.Length];
            for (var i = 0; i < pts.Length; i++)
                edges[i] = Line.ByStartPointEndPoint(pts[i], pts[(i + 1) % pts.Length]);
            return edges;
        }
    }
}

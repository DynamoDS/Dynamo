using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A closed rectangular polygon in 3D space.</summary>
    public class Rectangle : Polygon
    {
        private Rectangle(Point[] corners, Curve[] edges, double width, double height)
            : base(corners, edges)
        {
            Width = width;
            Height = height;
        }

        /// <summary>Gets the width (X-axis extent) of the rectangle.</summary>
        public double Width { get; }

        /// <summary>Gets the height (Y-axis extent) of the rectangle.</summary>
        public double Height { get; }

        /// <summary>Creates a rectangle centered at the world origin on the XY plane.</summary>
        /// <param name="width">Extent along the X axis.</param>
        /// <param name="length">Extent along the Y axis.</param>
        /// <returns>The centered rectangle.</returns>
        public static Rectangle ByWidthLength(double width = 1, double length = 1)
        {
            return ByWidthLength(Plane.XY(), width, length);
        }

        /// <summary>Creates a rectangle centered on a coordinate system's XY plane.</summary>
        /// <param name="cs">Coordinate system whose XY plane contains the rectangle.</param>
        /// <param name="width">Extent along the coordinate system's X axis.</param>
        /// <param name="length">Extent along the coordinate system's Y axis.</param>
        /// <returns>The created rectangle.</returns>
        public static Rectangle ByWidthLength(CoordinateSystem cs, double width = 1, double length = 1)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var plane = Plane.ByOriginNormalXAxis(cs.Origin, cs.ZAxis, cs.XAxis);
            return ByWidthLength(plane, width, length);
        }

        /// <summary>Creates a rectangle centered on a plane.</summary>
        /// <param name="plane">Plane containing the rectangle.</param>
        /// <param name="width">Extent along the plane's X axis.</param>
        /// <param name="length">Extent along the plane's Y axis.</param>
        /// <returns>The created rectangle.</returns>
        public static Rectangle ByWidthLength(Plane plane, double width = 1, double length = 1)
        {
            ArgumentNullException.ThrowIfNull(plane);
            if (!double.IsFinite(width) || width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Rectangle width must be positive.");
            if (!double.IsFinite(length) || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Rectangle length must be positive.");

            var hx = plane.XAxis.Scale(width * 0.5);
            var hy = plane.YAxis.Scale(length * 0.5);
            var corners = new[]
            {
                plane.Origin.Subtract(hx).Subtract(hy),
                plane.Origin.Add(hx).Subtract(hy),
                plane.Origin.Add(hx).Add(hy),
                plane.Origin.Subtract(hx).Add(hy)
            };
            return new Rectangle(corners, BuildClosedEdges(corners), width, length);
        }

        /// <summary>Creates a rectangle from four ordered corner points.</summary>
        public static Rectangle ByCornerPoints(Point p1, Point p2, Point p3, Point p4)
        {
            ArgumentNullException.ThrowIfNull(p1);
            ArgumentNullException.ThrowIfNull(p2);
            ArgumentNullException.ThrowIfNull(p3);
            ArgumentNullException.ThrowIfNull(p4);

            var corners = new[] { p1, p2, p3, p4 };
            return new Rectangle(corners, BuildClosedEdges(corners), p1.DistanceTo(p2), p2.DistanceTo(p3));
        }

        /// <summary>Creates a rectangle from a sequence of four ordered corner points.</summary>
        public static Rectangle ByCornerPoints(IEnumerable<Point> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).ToArray();
            if (pts.Length != 4)
                throw new ArgumentException("Rectangle.ByCornerPoints requires exactly 4 points.", nameof(points));
            return ByCornerPoints(pts[0], pts[1], pts[2], pts[3]);
        }
    }
}

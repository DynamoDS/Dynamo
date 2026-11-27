using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A closed circular curve in 3D space.</summary>
    public class Circle : Curve
    {
        private Circle(NativeGeometry.CurveHandle handle, Point center, double radius, Plane plane)
            : base(handle)
        {
            CenterPoint = center;
            Radius = radius;
            Plane = plane;
        }

        /// <summary>Gets the center point of the circle.</summary>
        public Point CenterPoint { get; }

        /// <summary>Gets the radius of the circle.</summary>
        public double Radius { get; }

        /// <summary>Gets the unit normal of the plane containing the circle.</summary>
        public Vector Normal => Plane.Normal;

        internal Plane Plane { get; }

        /// <summary>Creates a circle in the XY plane centered at <paramref name="centerPoint"/>.</summary>
        public static Circle ByCenterPointRadius(Point centerPoint, double radius = 1)
        {
            ArgumentNullException.ThrowIfNull(centerPoint);
            return ByPlaneRadius(Plane.ByOriginNormal(centerPoint, Vector.ZAxis()), radius);
        }

        /// <summary>Creates a circle centered at <paramref name="centerPoint"/> with the given radius and plane normal.</summary>
        public static Circle ByCenterPointRadiusNormal(Point centerPoint, double radius, Vector normal)
        {
            ArgumentNullException.ThrowIfNull(centerPoint);
            ArgumentNullException.ThrowIfNull(normal);
            return ByPlaneRadius(Plane.ByOriginNormal(centerPoint, normal), radius);
        }

        /// <summary>Creates a circle lying in <paramref name="plane"/> centered at the plane origin.</summary>
        public static Circle ByPlaneRadius(Plane plane, double radius = 1)
        {
            ArgumentNullException.ThrowIfNull(plane);
            var handle = NativeGeometry.Circle(
                plane.Origin.Position,
                plane.XAxis.Components,
                plane.YAxis.Components,
                radius);
            return new Circle(handle, plane.Origin, radius, plane);
        }

        /// <summary>Creates a circle passing through three non-collinear points.</summary>
        public static Circle ByThreePoints(Point p1, Point p2, Point p3)
        {
            ArgumentNullException.ThrowIfNull(p1);
            ArgumentNullException.ThrowIfNull(p2);
            ArgumentNullException.ThrowIfNull(p3);

            var (center, radius, plane) = ComputeCircumcircle(p1, p2, p3);
            return ByPlaneRadius(plane, radius);
        }

        internal static (Point Center, double Radius, Plane Plane) ComputeCircumcircle(Point p1, Point p2, Point p3)
        {
            var plane = Plane.ByThreePoints(p1, p2, p3);
            var a = Vector.ByTwoPoints(p1, p2);
            var b = Vector.ByTwoPoints(p1, p3);
            var aLenSq = a.Dot(a);
            var bLenSq = b.Dot(b);
            var cross = a.Cross(b);
            var denom = 2.0 * cross.Dot(cross);
            if (denom <= 1e-18)
                throw new ArgumentException("The three points must not be collinear.");

            var term1 = cross.Cross(a).Scale(bLenSq);
            var term2 = b.Cross(cross).Scale(aLenSq);
            var offset = term1.Add(term2).Scale(1.0 / denom);
            var center = p1.Add(offset);
            var radius = offset.Length;
            var centeredPlane = Plane.ByOriginNormalXAxis(center, plane.Normal, Vector.ByTwoPoints(center, p1));
            return (center, radius, centeredPlane);
        }
    }
}

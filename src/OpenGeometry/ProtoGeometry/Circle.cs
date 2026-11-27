using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A closed circular curve in 3D space.</summary>
    public class Circle : Curve
    {
        internal Circle(NativeGeometry.CurveHandle handle, Point center, double radius, Plane plane)
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
        public override Vector Normal => Plane.Normal;

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

        /// <inheritdoc />
        public override Curve Reverse()
        {
            var reversedHandle = NativeGeometry.CurveReverse(Handle);
            var flippedPlane = Plane.ByOriginNormalXAxis(CenterPoint, Plane.Normal.Reverse(), Plane.XAxis);
            return new Circle(reversedHandle, CenterPoint, Radius, flippedPlane);
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            var newPlane = (Plane)Plane.Translate(xTranslation, yTranslation, zTranslation);
            return new Circle(h, newPlane.Origin, Radius, newPlane);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees);
            var newPlane = (Plane)Plane.Rotate(origin, axis, degrees);
            return new Circle(h, newPlane.Origin, Radius, newPlane);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            if (Math.Abs(xamount - yamount) <= 1e-6 && Math.Abs(xamount - zamount) <= 1e-6 && xamount > 0)
            {
                var h = NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount);
                var newCenter = (Point)CenterPoint.Scale(basePoint, xamount, yamount, zamount);
                var newPlane = Plane.ByOriginNormalXAxis(newCenter, Plane.Normal, Plane.XAxis);
                return new Circle(h, newCenter, Radius * xamount, newPlane);
            }
            return base.Scale(basePoint, xamount, yamount, zamount);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var sx = cs.XScaleFactor;
            var sy = cs.YScaleFactor;
            var sz = cs.ZScaleFactor;
            var isUniformOrtho = Math.Abs(sx - sy) <= 1e-6 && Math.Abs(sx - sz) <= 1e-6
                && cs.XAxis.IsPerpendicular(cs.YAxis) && cs.XAxis.IsPerpendicular(cs.ZAxis) && cs.YAxis.IsPerpendicular(cs.ZAxis);
            if (isUniformOrtho)
            {
                var h = NativeGeometry.CurveTransform(
                    Handle, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components);
                var newPlane = (Plane)Plane.Transform(cs);
                return new Circle(h, newPlane.Origin, Radius * sx, newPlane);
            }
            return base.Transform(cs);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.CurveMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            var newCenter = (Point)CenterPoint.Mirror(mirrorPlane);
            var newStart = (Point)StartPoint.Mirror(mirrorPlane);
            var newX = Vector.ByTwoPoints(newCenter, newStart).Normalized();
            var (_, tan0Native, _) = NativeGeometry.CurveEvaluate(h, 0.0);
            var tan0 = new Vector(tan0Native);
            var newNormal = newX.Cross(tan0);
            var orientedNormal = newNormal.Length > 1e-9
                ? newNormal.Normalized()
                : ((Plane)Plane.Mirror(mirrorPlane)).Normal.Reverse();
            var newPlane = Plane.ByOriginNormalXAxis(newCenter, orientedNormal, newX);
            return new Circle(h, newCenter, Radius, newPlane);
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

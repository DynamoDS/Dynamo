using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A capped circular cylinder.</summary>
    public sealed class Cylinder : Solid
    {
        internal Cylinder(NativeGeometry.SolidHandle handle, Point startPoint, Point endPoint, double radius)
            : base(handle)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Radius = radius;
            Height = startPoint.DistanceTo(endPoint);
        }

        /// <summary>Gets the center of the first cap.</summary>
        public Point StartPoint { get; }

        /// <summary>Gets the center of the second cap.</summary>
        public Point EndPoint { get; }

        /// <summary>Gets the cylinder radius.</summary>
        public double Radius { get; }

        /// <summary>Gets the cylinder height along its axis.</summary>
        public double Height { get; }

        /// <summary>Creates a cylinder between two cap centers.</summary>
        /// <param name="startPoint">Center of the first cap.</param>
        /// <param name="endPoint">Center of the second cap.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <returns>The cylinder between the two points.</returns>
        public static Cylinder ByPointsRadius(Point startPoint, Point endPoint, double radius = 1)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(endPoint);
            var handle = NativeGeometry.Cylinder(startPoint.Position, endPoint.Position, radius);
            return new Cylinder(
                handle,
                Point.ByCoordinates(startPoint.X, startPoint.Y, startPoint.Z),
                Point.ByCoordinates(endPoint.X, endPoint.Y, endPoint.Z),
                radius);
        }

        /// <summary>Creates a cylinder aligned with a CoordinateSystem's Z axis.</summary>
        /// <param name="cs">Base coordinate system.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <param name="height">Height along the Z axis.</param>
        /// <returns>The created cylinder.</returns>
        public static Cylinder ByRadiusHeight(CoordinateSystem cs, double radius = 1, double height = 1)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var start = cs.Origin;
            var end = start.Add(cs.ZAxis.Normalized().Scale(height));
            return ByPointsRadius(start, end, radius);
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.Translate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            return new Cylinder(
                h,
                (Point)StartPoint.Translate(xTranslation, yTranslation, zTranslation),
                (Point)EndPoint.Translate(xTranslation, yTranslation, zTranslation),
                Radius);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.Rotate(Handle, origin.Position, axis.Components, degrees);
            return new Cylinder(
                h,
                (Point)StartPoint.Rotate(origin, axis, degrees),
                (Point)EndPoint.Rotate(origin, axis, degrees),
                Radius);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            if (Math.Abs(xamount - yamount) <= 1e-6 && Math.Abs(xamount - zamount) <= 1e-6 && xamount > 0)
            {
                var h = NativeGeometry.Scale(Handle, basePoint.Position, xamount, yamount, zamount);
                return new Cylinder(
                    h,
                    (Point)StartPoint.Scale(basePoint, xamount, yamount, zamount),
                    (Point)EndPoint.Scale(basePoint, xamount, yamount, zamount),
                    Radius * xamount);
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
                var h = NativeGeometry.Transform(
                    Handle, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components);
                return new Cylinder(
                    h,
                    (Point)StartPoint.Transform(cs),
                    (Point)EndPoint.Transform(cs),
                    Radius * sx);
            }
            return base.Transform(cs);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.BrepMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            return new Cylinder(
                h,
                (Point)StartPoint.Mirror(mirrorPlane),
                (Point)EndPoint.Mirror(mirrorPlane),
                Radius);
        }
    }
}

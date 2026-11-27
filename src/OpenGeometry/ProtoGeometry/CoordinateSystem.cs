using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An affine 3D coordinate system defined by origin and three basis vectors.</summary>
    public sealed class CoordinateSystem : IGraphicItem, IDisposable
    {
        private CoordinateSystem(Point origin, Vector xAxis, Vector yAxis, Vector zAxis)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
        }

        /// <summary>Gets the origin point.</summary>
        public Point Origin { get; }

        /// <summary>Gets the X basis vector.</summary>
        public Vector XAxis { get; }

        /// <summary>Gets the Y basis vector.</summary>
        public Vector YAxis { get; }

        /// <summary>Gets the Z basis vector.</summary>
        public Vector ZAxis { get; }

        /// <summary>Gets the determinant of the 3x3 linear basis matrix.</summary>
        public double Determinant => XAxis.Cross(YAxis).Dot(ZAxis);

        /// <summary>Gets true when the basis vectors are mutually orthogonal unit vectors with positive determinant.</summary>
        public bool IsUnscaledOrtho =>
            Math.Abs(XAxis.Length - 1.0) <= 1e-6
            && Math.Abs(YAxis.Length - 1.0) <= 1e-6
            && Math.Abs(ZAxis.Length - 1.0) <= 1e-6
            && XAxis.IsPerpendicular(YAxis)
            && XAxis.IsPerpendicular(ZAxis)
            && YAxis.IsPerpendicular(ZAxis)
            && Determinant > 0;

        /// <summary>Returns the world identity coordinate system.</summary>
        public static CoordinateSystem Identity() =>
            new(Point.Origin(), Vector.XAxis(), Vector.YAxis(), Vector.ZAxis());

        /// <summary>Creates an axis-aligned coordinate system translated to the given coordinates.</summary>
        /// <param name="x">Origin X.</param>
        /// <param name="y">Origin Y.</param>
        /// <param name="z">Origin Z.</param>
        /// <returns>The translated coordinate system.</returns>
        public static CoordinateSystem ByOrigin(double x = 0, double y = 0, double z = 0) =>
            new(Point.ByCoordinates(x, y, z), Vector.XAxis(), Vector.YAxis(), Vector.ZAxis());

        /// <summary>Creates an axis-aligned coordinate system at the given origin point.</summary>
        /// <param name="origin">Origin point.</param>
        /// <returns>The coordinate system.</returns>
        public static CoordinateSystem ByOrigin(Point origin)
        {
            ArgumentNullException.ThrowIfNull(origin);
            return ByOrigin(origin.X, origin.Y, origin.Z);
        }

        /// <summary>Creates a right-handed coordinate system from an origin and X/Y axes.</summary>
        /// <param name="origin">Origin point.</param>
        /// <param name="xAxis">X axis vector.</param>
        /// <param name="yAxis">Y axis vector.</param>
        /// <returns>The coordinate system.</returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(xAxis);
            ArgumentNullException.ThrowIfNull(yAxis);
            var zAxis = xAxis.Cross(yAxis).Normalized();
            return new CoordinateSystem(Point.ByCoordinates(origin.X, origin.Y, origin.Z), xAxis, yAxis, zAxis);
        }

        /// <summary>Creates a coordinate system from an origin and explicit X/Y/Z basis vectors.</summary>
        /// <param name="origin">Origin point.</param>
        /// <param name="xAxis">X axis vector.</param>
        /// <param name="yAxis">Y axis vector.</param>
        /// <param name="zAxis">Z axis vector.</param>
        /// <returns>The coordinate system.</returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, Vector zAxis)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(xAxis);
            ArgumentNullException.ThrowIfNull(yAxis);
            ArgumentNullException.ThrowIfNull(zAxis);
            return new CoordinateSystem(Point.ByCoordinates(origin.X, origin.Y, origin.Z), xAxis, yAxis, zAxis);
        }

        /// <summary>Creates a coordinate system aligned to a plane.</summary>
        /// <param name="plane">Reference plane.</param>
        /// <returns>The plane's coordinate system.</returns>
        public static CoordinateSystem ByPlane(Plane plane)
        {
            ArgumentNullException.ThrowIfNull(plane);
            return new CoordinateSystem(
                Point.ByCoordinates(plane.Origin.X, plane.Origin.Y, plane.Origin.Z),
                plane.XAxis,
                plane.YAxis,
                plane.Normal);
        }

        /// <summary>Translates this coordinate system by a vector.</summary>
        public CoordinateSystem Translate(Vector direction)
        {
            ArgumentNullException.ThrowIfNull(direction);
            return Translate(direction.X, direction.Y, direction.Z);
        }

        /// <summary>Translates this coordinate system by Cartesian offsets.</summary>
        public CoordinateSystem Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new CoordinateSystem(
                Point.ByCoordinates(Origin.X + xTranslation, Origin.Y + yTranslation, Origin.Z + zTranslation),
                XAxis, YAxis, ZAxis);
        }

        /// <summary>Rotates this coordinate system around an axis through an origin.</summary>
        public CoordinateSystem Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var newOrigin = (Point)Origin.Rotate(origin, axis, degrees);
            var newX = GeometryMath.RotateVector(XAxis, axis, degrees);
            var newY = GeometryMath.RotateVector(YAxis, axis, degrees);
            var newZ = GeometryMath.RotateVector(ZAxis, axis, degrees);
            return new CoordinateSystem(newOrigin, newX, newY, newZ);
        }

        /// <summary>Scales this coordinate system uniformly.</summary>
        public CoordinateSystem Scale(double amount) => Scale(amount, amount, amount);

        /// <summary>Scales this coordinate system non-uniformly along its axes.</summary>
        public CoordinateSystem Scale(double xAmount, double yAmount, double zAmount)
        {
            return new CoordinateSystem(
                Origin,
                XAxis.Scale(xAmount),
                YAxis.Scale(yAmount),
                ZAxis.Scale(zAmount));
        }

        /// <summary>Computes the inverse coordinate system transform.</summary>
        public CoordinateSystem Inverse()
        {
            var det = Determinant;
            if (Math.Abs(det) <= 1e-12)
                throw new InvalidOperationException("Cannot invert a singular coordinate system.");

            // Columns of M are XAxis, YAxis, ZAxis. Inverse rows are cross-products / det.
            var invRow0 = YAxis.Cross(ZAxis).Scale(1.0 / det);
            var invRow1 = ZAxis.Cross(XAxis).Scale(1.0 / det);
            var invRow2 = XAxis.Cross(YAxis).Scale(1.0 / det);

            var invX = Vector.ByCoordinates(invRow0.X, invRow1.X, invRow2.X);
            var invY = Vector.ByCoordinates(invRow0.Y, invRow1.Y, invRow2.Y);
            var invZ = Vector.ByCoordinates(invRow0.Z, invRow1.Z, invRow2.Z);

            var o = Origin.AsVector();
            var invOrigin = Point.ByCoordinates(-invRow0.Dot(o), -invRow1.Dot(o), -invRow2.Dot(o));
            return new CoordinateSystem(invOrigin, invX, invY, invZ);
        }

        internal CoordinateSystem Compose(CoordinateSystem after)
        {
            var newOrigin = after.TransformPoint(Origin.Position);
            var newX = after.TransformVector(XAxis);
            var newY = after.TransformVector(YAxis);
            var newZ = after.TransformVector(ZAxis);
            return new CoordinateSystem(Point.ByCoordinates(newOrigin.X, newOrigin.Y, newOrigin.Z), newX, newY, newZ);
        }

        internal NativePoint TransformPoint(NativePoint p)
        {
            return new NativePoint(
                Origin.X + p.X * XAxis.X + p.Y * YAxis.X + p.Z * ZAxis.X,
                Origin.Y + p.X * XAxis.Y + p.Y * YAxis.Y + p.Z * ZAxis.Y,
                Origin.Z + p.X * XAxis.Z + p.Y * YAxis.Z + p.Z * ZAxis.Z);
        }

        internal Vector TransformVector(Vector v)
        {
            return Vector.ByCoordinates(
                v.X * XAxis.X + v.Y * YAxis.X + v.Z * ZAxis.X,
                v.X * XAxis.Y + v.Y * YAxis.Y + v.Z * ZAxis.Y,
                v.X * XAxis.Z + v.Y * YAxis.Z + v.Z * ZAxis.Z);
        }

        /// <summary>Populates a Dynamo render package with RGB axis lines for this coordinate system.</summary>
        /// <param name="package">Target render package.</param>
        /// <param name="parameters">Tessellation quality parameters.</param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(package);
            ArgumentNullException.ThrowIfNull(parameters);

            EmitAxis(package, Origin, XAxis, 255, 0, 0);
            EmitAxis(package, Origin, YAxis, 0, 255, 0);
            EmitAxis(package, Origin, ZAxis, 0, 0, 255);
        }

        private static void EmitAxis(IRenderPackage package, Point origin, Vector axis, byte r, byte g, byte b)
        {
            package.AddLineStripVertex(origin.X, origin.Y, origin.Z);
            package.AddLineStripVertexColor(r, g, b, 255);
            package.AddLineStripVertex(origin.X + axis.X, origin.Y + axis.Y, origin.Z + axis.Z);
            package.AddLineStripVertexColor(r, g, b, 255);
            package.AddLineStripVertexCount(2);
        }

        /// <summary>Releases resources owned by this coordinate system.</summary>
        public void Dispose() { }
    }
}

using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>Base type for geometry that can appear in Dynamo's preview.</summary>
    public abstract class Geometry : IGraphicItem, IDisposable
    {
        internal Geometry() { }

        /// <summary>Gets the axis-aligned bounding box enclosing this geometry.</summary>
        public abstract BoundingBox BoundingBox { get; }

        /// <summary>Translates this geometry by a vector and returns a new geometry.</summary>
        /// <param name="direction">Translation displacement vector.</param>
        /// <returns>The translated geometry.</returns>
        public Geometry Translate(Vector direction)
        {
            ArgumentNullException.ThrowIfNull(direction);
            return Translate(direction.X, direction.Y, direction.Z);
        }

        /// <summary>Translates this geometry along a direction by a given distance.</summary>
        /// <param name="direction">Direction vector.</param>
        /// <param name="distance">Signed distance along the normalized direction.</param>
        /// <returns>The translated geometry.</returns>
        public Geometry Translate(Vector direction, double distance)
        {
            ArgumentNullException.ThrowIfNull(direction);
            var delta = direction.Normalized().Scale(distance);
            return Translate(delta.X, delta.Y, delta.Z);
        }

        /// <summary>Translates this geometry by Cartesian offsets.</summary>
        /// <param name="xTranslation">Offset along X.</param>
        /// <param name="yTranslation">Offset along Y.</param>
        /// <param name="zTranslation">Offset along Z.</param>
        /// <returns>The translated geometry.</returns>
        public abstract Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0);

        /// <summary>Rotates this geometry around an axis passing through an origin point.</summary>
        /// <param name="origin">Point on the axis of rotation.</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="degrees">Rotation angle in degrees.</param>
        /// <returns>The rotated geometry.</returns>
        public abstract Geometry Rotate(Point origin, Vector axis, double degrees);

        /// <summary>Rotates this geometry in the given base plane about its normal.</summary>
        /// <param name="basePlane">Base plane whose origin and normal define the axis.</param>
        /// <param name="degrees">Rotation angle in degrees.</param>
        /// <returns>The rotated geometry.</returns>
        public Geometry Rotate(Plane basePlane, double degrees)
        {
            ArgumentNullException.ThrowIfNull(basePlane);
            return Rotate(basePlane.Origin, basePlane.Normal, degrees);
        }

        /// <summary>Scales this geometry uniformly about the world origin.</summary>
        /// <param name="amount">Positive uniform scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public Geometry Scale(double amount) => Scale(Point.Origin(), amount, amount, amount);

        /// <summary>Scales this geometry non-uniformly about the world origin.</summary>
        /// <param name="xamount">X scale factor.</param>
        /// <param name="yamount">Y scale factor.</param>
        /// <param name="zamount">Z scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public Geometry Scale(double xamount, double yamount, double zamount) =>
            Scale(Point.Origin(), xamount, yamount, zamount);

        /// <summary>Scales this geometry about a base point.</summary>
        /// <param name="basePoint">Pivot point.</param>
        /// <param name="xamount">X scale factor.</param>
        /// <param name="yamount">Y scale factor.</param>
        /// <param name="zamount">Z scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public abstract Geometry Scale(Point basePoint, double xamount, double yamount, double zamount);

        /// <summary>Transforms this geometry by a coordinate system.</summary>
        /// <param name="cs">Target coordinate system.</param>
        /// <returns>The transformed geometry.</returns>
        public abstract Geometry Transform(CoordinateSystem cs);

        /// <summary>Transforms this geometry from one coordinate system to another.</summary>
        /// <param name="fromCoordinateSystem">Source coordinate system.</param>
        /// <param name="contextCoordinateSystem">Target coordinate system.</param>
        /// <returns>The transformed geometry.</returns>
        public Geometry Transform(CoordinateSystem fromCoordinateSystem, CoordinateSystem contextCoordinateSystem)
        {
            ArgumentNullException.ThrowIfNull(fromCoordinateSystem);
            ArgumentNullException.ThrowIfNull(contextCoordinateSystem);
            return Transform(fromCoordinateSystem.Inverse().Compose(contextCoordinateSystem));
        }

        /// <summary>Computes the shortest Euclidean distance to another geometry.</summary>
        /// <param name="other">Other geometry.</param>
        /// <returns>Minimum distance.</returns>
        public virtual double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            throw new NotSupportedException($"DistanceTo between {GetType().Name} and {other.GetType().Name} is not yet supported.");
        }

        /// <summary>SAT export is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public string ExportToSAT(string filePath) => throw NativeGeometry.UnsupportedFormat("SAT export");

        /// <summary>SAB serialization is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public byte[] SerializeAsSAB() => throw NativeGeometry.UnsupportedFormat("SAB serialization");

        /// <summary>SolidDef serialization is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public string ToSolidDef() => throw NativeGeometry.UnsupportedFormat("SolidDef serialization");

        /// <summary>Releases resources owned by this object.</summary>
        public void Dispose()
        {
            ReleaseResources();
        }

        /// <summary>Populates a Dynamo render package with tessellated preview geometry.</summary>
        /// <param name="package">Target render package.</param>
        /// <param name="parameters">Tessellation quality parameters.</param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(package);
            ArgumentNullException.ThrowIfNull(parameters);
            WritePreview(package, parameters);
        }

        internal abstract void WritePreview(IRenderPackage package, TessellationParameters parameters);

        internal virtual void ReleaseResources() { }
    }

    /// <summary>Helper extension methods on geometry arrays matching Dynamo's ProtoGeometry conventions.</summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryExtension
    {
        /// <summary>Executes an action on each element of an array.</summary>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null || action == null)
                return;
            foreach (var item in array)
                action(item);
        }

        /// <summary>Executes an action on each element of a jagged 2D array.</summary>
        public static void ForEach<T>(this T[][] array, Action<T> action)
        {
            if (array == null || action == null)
                return;
            foreach (var row in array)
            {
                if (row == null)
                    continue;
                foreach (var item in row)
                    action(item);
            }
        }
    }

    internal static class GeometryMath
    {
        internal static Vector RotateVector(Vector vector, Vector axis, double degrees)
        {
            var k = axis.Normalized();
            var radians = degrees * (Math.PI / 180.0);
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            // Rodrigues' rotation formula: v*cos + (k x v)*sin + k*(k.v)*(1 - cos)
            var term1 = vector.Scale(cos);
            var term2 = k.Cross(vector).Scale(sin);
            var term3 = k.Scale(k.Dot(vector) * (1.0 - cos));
            return term1.Add(term2).Add(term3);
        }
    }
}

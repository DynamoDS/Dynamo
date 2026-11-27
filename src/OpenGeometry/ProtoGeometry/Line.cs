using System;
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

        internal override uint PreviewSegmentCount => 1;
    }
}

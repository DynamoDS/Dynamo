using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A non-uniform rational B-spline curve.</summary>
    public class NurbsCurve : Curve
    {
        internal NurbsCurve(NativeGeometry.CurveHandle handle, int degree) : base(handle)
        {
            Degree = degree;
        }

        /// <summary>Gets the polynomial degree of this NURBS curve.</summary>
        public int Degree { get; }

        /// <summary>Returns the ordered 3D control points of this NURBS curve.</summary>
        public Point[] ControlPoints()
        {
            var (pts, _) = NativeGeometry.CurveNurbsControlPoints(Handle);
            return pts.Select(p => new Point(p)).ToArray();
        }

        /// <summary>Returns the control point weights of this NURBS curve.</summary>
        public double[] Weights()
        {
            var (_, wts) = NativeGeometry.CurveNurbsControlPoints(Handle);
            return wts;
        }

        /// <summary>Returns the knot vector of this NURBS curve.</summary>
        public double[] Knots() => NativeGeometry.CurveNurbsKnots(Handle);

        /// <summary>Creates a NURBS curve interpolating a sequence of 3D points.</summary>
        /// <param name="points">Ordered points through which the curve passes (at least 2).</param>
        /// <returns>The interpolated NURBS curve.</returns>
        public static NurbsCurve ByPoints(IEnumerable<Point> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).Select(p => p.Position).ToArray();
            if (pts.Length < 2)
                throw new ArgumentException("At least 2 points are required to create a NurbsCurve.", nameof(points));

            var handle = NativeGeometry.InterpCurve(pts);
            var degree = pts.Length == 2 ? 1 : (pts.Length == 3 ? 2 : 3);
            return new NurbsCurve(handle, degree);
        }

        /// <summary>Creates a cubic (or lower when fewer than 4 points) NURBS curve from control points.</summary>
        /// <param name="points">Control polygon points.</param>
        /// <returns>The created NURBS curve.</returns>
        public static NurbsCurve ByControlPoints(IEnumerable<Point> points) => ByControlPoints(points, 3);

        /// <summary>Creates a NURBS curve from control points and polynomial degree.</summary>
        /// <param name="points">Control polygon points.</param>
        /// <param name="degree">Curve degree (at least 1 and less than the number of control points).</param>
        /// <returns>The created NURBS curve.</returns>
        public static NurbsCurve ByControlPoints(IEnumerable<Point> points, int degree)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).Select(p => p.Position).ToArray();
            if (pts.Length < 2)
                throw new ArgumentException("At least 2 control points are required.", nameof(points));
            var clampedDegree = Math.Min(degree, pts.Length - 1);
            if (clampedDegree < 1)
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be at least 1.");

            var handle = NativeGeometry.ControlPointCurve(pts, clampedDegree);
            return new NurbsCurve(handle, clampedDegree);
        }

        /// <summary>Creates a NURBS curve from control points, polynomial degree, and optional closure.</summary>
        /// <param name="points">Control polygon points.</param>
        /// <param name="degree">Curve degree (at least 1 and less than the control point count).</param>
        /// <param name="closeCurve">If true, closes the curve back to the first control point.</param>
        /// <returns>The created NURBS curve.</returns>
        public static NurbsCurve ByControlPoints(IEnumerable<Point> points, int degree = 3, bool closeCurve = false)
        {
            ArgumentNullException.ThrowIfNull(points);
            var list = points.Where(p => p != null).ToList();
            if (closeCurve && list.Count >= 2 && list[0].DistanceTo(list[^1]) > 1e-6)
                list.Add(list[0]);
            return ByControlPoints(list, degree);
        }

        /// <summary>Creates a rational or non-rational NURBS curve from explicit control vertices, weights, knot vector, and degree.</summary>
        /// <param name="controlVertices">Ordered control points.</param>
        /// <param name="weights">Strictly positive weight per control point.</param>
        /// <param name="knots">Non-decreasing knot vector of length <c>controlVertices.Count + degree + 1</c>.</param>
        /// <param name="degree">Polynomial degree (at least 1 and less than control point count).</param>
        /// <returns>The created rational NURBS curve.</returns>
        public static NurbsCurve ByControlPointsWeightsKnots(
            IEnumerable<Point> controlVertices, double[] weights, double[] knots, int degree)
        {
            ArgumentNullException.ThrowIfNull(controlVertices);
            ArgumentNullException.ThrowIfNull(weights);
            ArgumentNullException.ThrowIfNull(knots);
            var pts = controlVertices.Where(p => p != null).Select(p => p.Position).ToArray();
            if (pts.Length < 2)
                throw new ArgumentException("At least 2 control points are required.", nameof(controlVertices));
            if (weights.Length != pts.Length)
                throw new ArgumentException("Weight count must match control point count.", nameof(weights));
            if (degree < 1 || degree >= pts.Length)
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree must be between 1 and control point count - 1.");
            if (knots.Length != pts.Length + degree + 1)
                throw new ArgumentException("Knot vector length must equal control point count + degree + 1.", nameof(knots));

            var handle = NativeGeometry.RationalCurve(pts, weights, knots, degree);
            return new NurbsCurve(handle, degree);
        }

        /// <inheritdoc />
        public override Curve Reverse() => new NurbsCurve(NativeGeometry.CurveReverse(Handle), Degree);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0) =>
            new NurbsCurve(NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation)), Degree);

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            return new NurbsCurve(NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees), Degree);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return new NurbsCurve(NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount), Degree);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new NurbsCurve(
                NativeGeometry.CurveTransform(
                    Handle, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components),
                Degree);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane) =>
            new NurbsCurve(
                NativeGeometry.CurveMirror(
                    Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components),
                Degree);
    }
}

using System;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A bounded 3D parametric curve backed by SMLib.</summary>
    public class Curve : Geometry
    {
        internal NativeGeometry.CurveHandle Handle { get; }

        internal Curve(NativeGeometry.CurveHandle handle)
        {
            Handle = handle;
        }

        internal Curve()
        {
            Handle = null;
        }

        /// <summary>Gets the start point of this curve (parameter 0).</summary>
        public virtual Point StartPoint => PointAtParameter(0);

        /// <summary>Gets the end point of this curve (parameter 1).</summary>
        public virtual Point EndPoint => PointAtParameter(1);

        /// <summary>Gets the 3D arc length of this curve.</summary>
        public virtual double Length => NativeGeometry.CurveLength(Handle);

        /// <summary>Gets a value indicating whether the start and end points coincide.</summary>
        public virtual bool IsClosed => NativeGeometry.CurveIsClosed(Handle);

        /// <summary>Evaluates a point along the curve at normalized parameter <paramref name="param"/> in [0, 1].</summary>
        public virtual Point PointAtParameter(double param)
        {
            var (pt, _, _) = NativeGeometry.CurveEvaluate(Handle, param);
            return new Point(pt);
        }

        /// <summary>Evaluates the unit tangent vector along the curve at normalized parameter <paramref name="param"/> in [0, 1].</summary>
        public virtual Vector TangentAtParameter(double param)
        {
            var (_, tan, _) = NativeGeometry.CurveEvaluate(Handle, param);
            return new Vector(tan);
        }

        /// <summary>Evaluates the unit principal normal vector along the curve at normalized parameter <paramref name="param"/> in [0, 1].</summary>
        public virtual Vector NormalAtParameter(double param)
        {
            var (_, _, nrm) = NativeGeometry.CurveEvaluate(Handle, param);
            return new Vector(nrm);
        }

        /// <summary>Evaluates the point at a given arc length along the curve from its start point.</summary>
        public virtual Point PointAtSegmentLength(double segmentLength)
        {
            var param = ParameterAtSegmentLength(segmentLength);
            return PointAtParameter(param);
        }

        /// <summary>Returns the normalized parameter in [0, 1] at a given arc length from the start of the curve.</summary>
        public virtual double ParameterAtSegmentLength(double segmentLength)
        {
            return NativeGeometry.CurveParameterAtLength(Handle, segmentLength);
        }

        /// <summary>Returns the normalized parameter in [0, 1] on the curve closest to <paramref name="point"/>.</summary>
        public virtual double ParameterAtPoint(Point point)
        {
            ArgumentNullException.ThrowIfNull(point);
            return NativeGeometry.CurveClosestPoint(Handle, point.Position).Parameter;
        }

        /// <summary>Returns a copy of this curve with reversed parameterization.</summary>
        public virtual Curve Reverse() => new(NativeGeometry.CurveReverse(Handle));

        /// <summary>Splits this curve into two curves at normalized parameter <paramref name="parameter"/> in (0, 1).</summary>
        public virtual Curve[] SplitByParameter(double parameter)
        {
            if (parameter <= 0 || parameter >= 1 || !double.IsFinite(parameter))
                throw new ArgumentOutOfRangeException(nameof(parameter), "Split parameter must be strictly between 0 and 1.");

            return new[]
            {
                TrimByParameter(0, parameter),
                TrimByParameter(parameter, 1)
            };
        }

        /// <summary>Trims this curve to the normalized parameter sub-interval [<paramref name="startParameter"/>, <paramref name="endParameter"/>].</summary>
        public virtual Curve TrimByParameter(double startParameter, double endParameter)
        {
            return new Curve(NativeGeometry.CurveTrim(Handle, startParameter, endParameter));
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox
        {
            get
            {
                var (min, max) = NativeGeometry.CurveBoundingBox(Handle);
                return BoundingBox.ByCorners(new Point(min), new Point(max));
            }
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new Curve(NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation)));
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            return new Curve(NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees));
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return new Curve(NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount));
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new Curve(NativeGeometry.CurveTransform(
                Handle,
                cs.Origin.Position,
                cs.XAxis.Components,
                cs.YAxis.Components,
                cs.ZAxis.Components));
        }

        /// <inheritdoc />
        public override double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is Point pt)
                return NativeGeometry.CurveClosestPoint(Handle, pt.Position).Distance;
            return base.DistanceTo(other);
        }

        internal virtual NativeGeometry.CurveHandle[] GetAtomicHandles() => new[] { Handle };

        internal virtual uint PreviewSegmentCount => 64;

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            var points = NativeGeometry.CurveSample(Handle, PreviewSegmentCount);
            if (points.Length < 2)
                return;

            foreach (var pt in points)
                package.AddLineStripVertex(pt.X, pt.Y, pt.Z);
            package.AddLineStripVertexCount(points.Length);
        }

        internal override void ReleaseResources()
        {
            Handle?.Dispose();
        }
    }
}

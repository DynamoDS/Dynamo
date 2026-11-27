using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A composite curve made of one or more connected curves joined end-to-end.</summary>
    public class PolyCurve : Curve
    {
        private readonly Curve[] segments;

        internal PolyCurve(Curve[] segments)
        {
            if (segments == null || segments.Length == 0)
                throw new ArgumentException("A PolyCurve requires at least one curve.", nameof(segments));
            this.segments = segments;
        }

        /// <summary>Gets the number of component curves in this polycurve.</summary>
        public int NumberOfCurves => segments.Length;

        /// <summary>Returns the ordered component curves making up this polycurve.</summary>
        public Curve[] Curves() => segments.Select(s => (Curve)s.Translate(0, 0, 0)).ToArray();

        /// <summary>Creates a polycurve by joining a collection of curves end-to-end.</summary>
        /// <param name="curves">Curves to join.</param>
        /// <param name="joinTolerance">Maximum endpoint gap allowed when joining curves.</param>
        /// <param name="trimCurves">Reserved trim option.</param>
        /// <param name="trimLength">Reserved trim length.</param>
        /// <returns>The joined polycurve.</returns>
        public static PolyCurve ByJoinedCurves(
            IEnumerable<Curve> curves,
            double joinTolerance = 0.001,
            bool trimCurves = false,
            double trimLength = 0)
        {
            ArgumentNullException.ThrowIfNull(curves);
            if (!double.IsFinite(joinTolerance) || joinTolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(joinTolerance), "Join tolerance must be non-negative.");

            var flat = new List<Curve>();
            foreach (var c in curves)
            {
                if (c == null)
                    continue;
                if (c is PolyCurve pc)
                {
                    foreach (var sub in pc.segments)
                        flat.Add((Curve)sub.Translate(0, 0, 0));
                }
                else
                {
                    flat.Add((Curve)c.Translate(0, 0, 0));
                }
            }

            if (flat.Count == 0)
                throw new ArgumentException("At least one curve is required.", nameof(curves));

            // Order and orient curves into a continuous chain within joinTolerance.
            var ordered = new List<Curve>(flat.Count) { flat[0] };
            var remaining = flat.Skip(1).ToList();
            var tol = Math.Max(joinTolerance, 1e-6);

            while (remaining.Count > 0)
            {
                var tail = ordered[^1].EndPoint;
                var head = ordered[0].StartPoint;
                var matched = false;

                for (var i = 0; i < remaining.Count; i++)
                {
                    var candidate = remaining[i];
                    if (tail.DistanceTo(candidate.StartPoint) <= tol)
                    {
                        ordered.Add(candidate);
                        remaining.RemoveAt(i);
                        matched = true;
                        break;
                    }
                    if (tail.DistanceTo(candidate.EndPoint) <= tol)
                    {
                        var reversed = candidate.Reverse();
                        candidate.Dispose();
                        ordered.Add(reversed);
                        remaining.RemoveAt(i);
                        matched = true;
                        break;
                    }
                    if (head.DistanceTo(candidate.EndPoint) <= tol)
                    {
                        ordered.Insert(0, candidate);
                        remaining.RemoveAt(i);
                        matched = true;
                        break;
                    }
                    if (head.DistanceTo(candidate.StartPoint) <= tol)
                    {
                        var reversed = candidate.Reverse();
                        candidate.Dispose();
                        ordered.Insert(0, reversed);
                        remaining.RemoveAt(i);
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    foreach (var c in ordered) c.Dispose();
                    foreach (var c in remaining) c.Dispose();
                    throw new ArgumentException("Input curves do not form a contiguous chain within the join tolerance.");
                }
            }

            return new PolyCurve(ordered.ToArray());
        }

        /// <summary>Creates a polycurve of line segments connecting a sequence of points.</summary>
        /// <param name="points">Ordered vertices.</param>
        /// <param name="connectLastToFirst">If true, closes the loop from the last point back to the first.</param>
        /// <returns>The created polycurve.</returns>
        public static PolyCurve ByPoints(IEnumerable<Point> points, bool connectLastToFirst = false)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).ToArray();
            if (pts.Length < 2)
                throw new ArgumentException("At least 2 points are required.", nameof(points));

            var lines = new List<Curve>(pts.Length);
            for (var i = 0; i < pts.Length - 1; i++)
                lines.Add(Line.ByStartPointEndPoint(pts[i], pts[i + 1]));
            if (connectLastToFirst && pts[0].DistanceTo(pts[^1]) > 1e-9)
                lines.Add(Line.ByStartPointEndPoint(pts[^1], pts[0]));

            return new PolyCurve(lines.ToArray());
        }

        /// <inheritdoc />
        public override Point StartPoint => segments[0].StartPoint;

        /// <inheritdoc />
        public override Point EndPoint => segments[^1].EndPoint;

        /// <inheritdoc />
        public override double Length => segments.Sum(s => s.Length);

        /// <inheritdoc />
        public override bool IsClosed => StartPoint.DistanceTo(EndPoint) <= 1e-6;

        /// <inheritdoc />
        public override Point PointAtParameter(double param)
        {
            var (seg, localT) = MapParameter(param);
            return seg.PointAtParameter(localT);
        }

        /// <inheritdoc />
        public override Vector TangentAtParameter(double param)
        {
            var (seg, localT) = MapParameter(param);
            return seg.TangentAtParameter(localT);
        }

        /// <inheritdoc />
        public override Vector NormalAtParameter(double param)
        {
            var (seg, localT) = MapParameter(param);
            return seg.NormalAtParameter(localT);
        }

        /// <inheritdoc />
        public override double ParameterAtSegmentLength(double segmentLength)
        {
            var total = Length;
            if (!double.IsFinite(segmentLength) || segmentLength < -1e-9 || segmentLength > total + 1e-6)
                throw new ArgumentOutOfRangeException(nameof(segmentLength), "Segment length must be within [0, Length].");
            if (total <= 0)
                return 0;

            var target = Math.Clamp(segmentLength, 0, total);
            var accumulated = 0.0;
            for (var i = 0; i < segments.Length; i++)
            {
                var segLen = segments[i].Length;
                if (target <= accumulated + segLen || i == segments.Length - 1)
                {
                    var localLen = Math.Clamp(target - accumulated, 0, segLen);
                    var localT = segLen > 0 ? segments[i].ParameterAtSegmentLength(localLen) : 0;
                    return (i + localT) / segments.Length;
                }
                accumulated += segLen;
            }
            return 1.0;
        }

        /// <inheritdoc />
        public override double ParameterAtPoint(Point point)
        {
            ArgumentNullException.ThrowIfNull(point);
            var bestDist = double.PositiveInfinity;
            var bestParam = 0.0;
            for (var i = 0; i < segments.Length; i++)
            {
                var localT = segments[i].ParameterAtPoint(point);
                var pt = segments[i].PointAtParameter(localT);
                var dist = pt.DistanceTo(point);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestParam = (i + localT) / segments.Length;
                }
            }
            return bestParam;
        }

        /// <inheritdoc />
        public override Curve Reverse()
        {
            var reversed = segments.Reverse().Select(s => s.Reverse()).ToArray();
            return new PolyCurve(reversed);
        }

        /// <inheritdoc />
        public override Curve TrimByParameter(double startParameter, double endParameter)
        {
            if (!double.IsFinite(startParameter) || !double.IsFinite(endParameter)
                || startParameter < -1e-9 || endParameter > 1.0 + 1e-9 || endParameter <= startParameter + 1e-9)
                throw new ArgumentOutOfRangeException(nameof(startParameter), "Trim parameters must satisfy 0 <= start < end <= 1.");

            var u0 = Math.Clamp(startParameter, 0.0, 1.0) * segments.Length;
            var u1 = Math.Clamp(endParameter, 0.0, 1.0) * segments.Length;
            var pieces = new List<Curve>();
            for (var i = 0; i < segments.Length; i++)
            {
                var lo = Math.Max(0.0, u0 - i);
                var hi = Math.Min(1.0, u1 - i);
                if (hi - lo > 1e-9)
                    pieces.Add(segments[i].TrimByParameter(lo, hi));
            }
            return pieces.Count == 1 ? pieces[0] : new PolyCurve(pieces.ToArray());
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox => BoundingBox.ByGeometry(segments);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new PolyCurve(segments.Select(s => (Curve)s.Translate(xTranslation, yTranslation, zTranslation)).ToArray());
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            return new PolyCurve(segments.Select(s => (Curve)s.Rotate(origin, axis, degrees)).ToArray());
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            return new PolyCurve(segments.Select(s => (Curve)s.Scale(basePoint, xamount, yamount, zamount)).ToArray());
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            return new PolyCurve(segments.Select(s => (Curve)s.Transform(cs)).ToArray());
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            return new PolyCurve(segments.Select(s => (Curve)s.Mirror(mirrorPlane)).ToArray());
        }

        /// <inheritdoc />
        public override double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is Point)
                return segments.Min(s => s.DistanceTo(other));
            return base.DistanceTo(other);
        }

        internal override NativeGeometry.CurveHandle[] GetAtomicHandles() =>
            segments.SelectMany(s => s.GetAtomicHandles()).ToArray();

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            foreach (var seg in segments)
                seg.WritePreview(package, parameters);
        }

        internal override void ReleaseResources()
        {
            foreach (var seg in segments)
                seg.Dispose();
        }

        private (Curve Segment, double LocalT) MapParameter(double param)
        {
            if (!double.IsFinite(param) || param < -1e-9 || param > 1.0 + 1e-9)
                throw new ArgumentOutOfRangeException(nameof(param), "Curve parameter must be in [0, 1].");
            var scaled = Math.Clamp(param, 0.0, 1.0) * segments.Length;
            var index = Math.Min((int)Math.Floor(scaled), segments.Length - 1);
            var localT = Math.Clamp(scaled - index, 0.0, 1.0);
            return (segments[index], localT);
        }
    }
}

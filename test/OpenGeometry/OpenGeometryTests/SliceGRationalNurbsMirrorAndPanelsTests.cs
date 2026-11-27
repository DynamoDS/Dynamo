using System;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture]
    public sealed class SliceGRationalNurbsMirrorAndPanelsTests
    {
        [Test]
        public void WhenRationalQuarterCircleCreatedByControlPointsWeightsKnotsThenLengthAndMidpointAreExact()
        {
            const double r = 4.0;
            var cps = new[]
            {
                Point.ByCoordinates(r, 0, 0),
                Point.ByCoordinates(r, r, 0),
                Point.ByCoordinates(0, r, 0)
            };
            var weights = new[] { 1.0, Math.Sqrt(0.5), 1.0 };
            var knots = new[] { 0.0, 0.0, 0.0, 1.0, 1.0, 1.0 };

            using var arc = NurbsCurve.ByControlPointsWeightsKnots(cps, weights, knots, degree: 2);
            var mid = arc.PointAtParameter(0.5);
            Assert.Multiple(() =>
            {
                Assert.That(arc.Degree, Is.EqualTo(2));
                Assert.That(arc.Length, Is.EqualTo(0.5 * Math.PI * r).Within(1e-4));
                Assert.That(mid.DistanceTo(Point.Origin()), Is.EqualTo(r).Within(1e-4));
                Assert.That(mid.X, Is.EqualTo(r * Math.Sqrt(0.5)).Within(1e-4));
                Assert.That(mid.Y, Is.EqualTo(r * Math.Sqrt(0.5)).Within(1e-4));
            });
        }

        [Test]
        public void WhenGeometriesMirroredAcrossPlaneThenExactReflectionsPreserveVolumeAndLength()
        {
            using var mirrorPlane = Plane.YZ();

            // Solid mirror (native SMLib SmBrep::Mirror)
            using var box = Cuboid.ByLengths(Point.ByCoordinates(10, 2, 3), 4, 6, 8);
            using var mirroredSolid = (Solid)box.Mirror(mirrorPlane);
            Assert.Multiple(() =>
            {
                Assert.That(mirroredSolid.Volume, Is.EqualTo(192.0).Within(1e-3));
                Assert.That(mirroredSolid.Centroid().X, Is.EqualTo(-10.0).Within(1e-4));
                Assert.That(mirroredSolid.Centroid().Y, Is.EqualTo(2.0).Within(1e-4));
                Assert.That(mirroredSolid.Centroid().Z, Is.EqualTo(3.0).Within(1e-4));
                // Original box unchanged.
                Assert.That(box.Centroid().X, Is.EqualTo(10.0).Within(1e-4));
            });

            // Curve mirror (native SMLib SmCurve::CreateMirrorCurve)
            using var line = Line.ByStartPointEndPoint(Point.ByCoordinates(2, 1, 0), Point.ByCoordinates(6, 4, 0));
            using var mirroredCurve = (Curve)line.Mirror(mirrorPlane);
            Assert.Multiple(() =>
            {
                Assert.That(mirroredCurve.Length, Is.EqualTo(5.0).Within(1e-4));
                Assert.That(mirroredCurve.StartPoint.X, Is.EqualTo(-2.0).Within(1e-4));
                Assert.That(mirroredCurve.EndPoint.X, Is.EqualTo(-6.0).Within(1e-4));
            });

            // Point mirror
            var pt = Point.ByCoordinates(3, -4, 5);
            var mirroredPt = (Point)pt.Mirror(mirrorPlane);
            Assert.That(mirroredPt.X, Is.EqualTo(-3.0).Within(1e-9));
            Assert.That(mirroredPt.Y, Is.EqualTo(-4.0).Within(1e-9));

            // Polymorphic Mesh mirror via Geometry.Mirror
            using var mesh = Mesh.Cuboid(Point.ByCoordinates(10, 0, 0), 4, 4, 4);
            using var mirroredMesh = (Mesh)((Geometry)mesh).Mirror(mirrorPlane);
            Assert.That(mirroredMesh.Volume, Is.EqualTo(64.0).Within(1e-3));
            Assert.That(mirroredMesh.BoundingBox.MaxPoint.X, Is.EqualTo(-8.0).Within(1e-6));
        }

        [Test]
        public void WhenCurveOffsetManyAndSurfaceGuidedLoftRunThenGeometryMatchesExpectedBounds()
        {
            using var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(10, 0, 0));
            var pos = line.OffsetMany(2.0, Vector.ZAxis());
            var neg = line.OffsetMany(2.0, Vector.ByCoordinates(0, 0, -1));
            try
            {
                Assert.That(pos.Length, Is.EqualTo(1));
                Assert.That(neg.Length, Is.EqualTo(1));
                // Reversing planeNormal flips the offset side.
                Assert.That(pos[0].StartPoint.Y, Is.EqualTo(-neg[0].StartPoint.Y).Within(1e-4));
            }
            finally
            {
                foreach (var c in pos) c.Dispose();
                foreach (var c in neg) c.Dispose();
            }

            using var c0 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(4, 0, 0));
            using var c1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 10, 0), Point.ByCoordinates(8, 10, 0));
            using var rail1 = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 10, 0));
            using var rail2 = Line.ByStartPointEndPoint(Point.ByCoordinates(4, 0, 0), Point.ByCoordinates(8, 10, 0));
            using var guided = Surface.ByLoft(new Curve[] { c0, c1 }, new Curve[] { rail1, rail2 });
            Assert.That(guided.Area, Is.EqualTo(60.0).Within(1.0));
        }

        [Test]
        public void WhenPanelSurfacePatternsCreatedThenPanelCountsAndVertexCountsMatch()
        {
            using var srf = Surface.ByPerimeterPoints(new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(10, 0, 0),
                Point.ByCoordinates(10, 10, 0),
                Point.ByCoordinates(0, 10, 0)
            });

            using var diamonds = PanelSurface.ByDiamonds(srf, 4, 3);
            using var hexes = PanelSurface.ByHexagons(srf, 3, 3);
            using var staggered = PanelSurface.ByStaggeredQuads(srf, 4, 4, staggerVertically: false, displacementFactor: 0.5);
            using var para = PanelSurface.ByParallelograms(srf, 4, 3, shearFactor: 0.25, alignWithUAxis: false);

            Assert.Multiple(() =>
            {
                Assert.That(diamonds.NumPanels, Is.EqualTo(12));
                Assert.That(diamonds.GetNumPanelVertices()[0], Is.EqualTo(4));
                Assert.That(hexes.NumPanels, Is.EqualTo(9));
                Assert.That(hexes.GetNumPanelVertices()[0], Is.EqualTo(6));
                Assert.That(staggered.NumPanels, Is.GreaterThanOrEqualTo(12));
                Assert.That(para.NumPanels, Is.EqualTo(12));
                Assert.That(para.GetNumPanelVertices()[0], Is.EqualTo(4));
            });
        }
    }
}

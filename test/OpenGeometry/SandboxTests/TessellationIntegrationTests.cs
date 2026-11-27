using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Tessellation;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class TessellationIntegrationTests
    {
        private const double Tolerance = 1e-3;

        [Test]
        public void WhenSurfaceGetIsolineRunsOnAnisotropicSurfaceThenUAndVIsolineLengthsMatchConvention()
        {
            // U spans [0, 10] along X; V spans [0, 4] along Y.
            using var surf = NurbsSurface.ByControlPoints(new[]
            {
                new[] { Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 4, 0) },
                new[] { Point.ByCoordinates(10, 0, 0), Point.ByCoordinates(10, 4, 0) }
            }, 1, 1);

            using var uIso = surf.GetIsoline(0, 0.5);
            using var vIso = surf.GetIsoline(1, 0.5);

            Assert.Multiple(() =>
            {
                Assert.That(uIso.Length, Is.EqualTo(10.0).Within(Tolerance), "isoDirection 0 must run along U (length 10).");
                Assert.That(vIso.Length, Is.EqualTo(4.0).Within(Tolerance), "isoDirection 1 must run along V (length 4).");
                Assert.That(uIso.PointAtParameter(0.5).Y, Is.EqualTo(2.0).Within(Tolerance));
                Assert.That(vIso.PointAtParameter(0.5).X, Is.EqualTo(5.0).Within(Tolerance));
            });
        }

        [Test]
        public void WhenVoronoiAndDelaunayRunOnSurfaceParametersThenValidCurvesAreGenerated()
        {
            using var surf = NurbsSurface.ByControlPoints(new[]
            {
                new[] { Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 4, 0.5) },
                new[] { Point.ByCoordinates(10, 0, 0.5), Point.ByCoordinates(10, 4, 0) }
            }, 1, 1);

            var uvs = new[]
            {
                UV.ByCoordinates(0.1, 0.1),
                UV.ByCoordinates(0.5, 0.15),
                UV.ByCoordinates(0.9, 0.1),
                UV.ByCoordinates(0.2, 0.5),
                UV.ByCoordinates(0.55, 0.55),
                UV.ByCoordinates(0.85, 0.5),
                UV.ByCoordinates(0.1, 0.9),
                UV.ByCoordinates(0.5, 0.85),
                UV.ByCoordinates(0.9, 0.9)
            };

            var delaunayCurves = Delaunay.ByParametersOnSurface(uvs, surf).ToArray();
            var voronoiCurves = Voronoi.ByParametersOnSurface(uvs, surf).ToArray();

            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(delaunayCurves.Length, Is.GreaterThanOrEqualTo(12));
                    Assert.That(delaunayCurves.All(c => c.Length > 0.01), Is.True);
                    Assert.That(voronoiCurves.Length, Is.GreaterThanOrEqualTo(4));
                    Assert.That(voronoiCurves.All(c => c.Length > 0.01), Is.True);
                });
            }
            finally
            {
                foreach (var c in delaunayCurves) c.Dispose();
                foreach (var c in voronoiCurves) c.Dispose();
            }
        }

        [Test]
        public void WhenDelaunay3DAndConvexHullRunOnPointsThenTetrahedraAndHullEdgesAreGenerated()
        {
            var pts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(4, 0, 0),
                Point.ByCoordinates(4, 4, 0),
                Point.ByCoordinates(0, 4, 0),
                Point.ByCoordinates(0, 0, 4),
                Point.ByCoordinates(4, 0, 4),
                Point.ByCoordinates(4, 4, 4),
                Point.ByCoordinates(0, 4, 4),
                Point.ByCoordinates(2, 2, 2)
            };

            var hullCurves = ConvexHull.ByPoints(pts).ToArray();
            var delaunay3d = Delaunay.ByPoints(pts).ToArray();

            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(hullCurves.Length, Is.GreaterThanOrEqualTo(12));
                    Assert.That(hullCurves.All(c => c.Length >= 3.9), Is.True, "Cube hull edges should lie on outer boundary.");
                    Assert.That(delaunay3d.Length, Is.GreaterThanOrEqualTo(24));
                    Assert.That(delaunay3d.All(c => c.Length > 0.1), Is.True);
                });
            }
            finally
            {
                foreach (var c in hullCurves) c.Dispose();
                foreach (var c in delaunay3d) c.Dispose();
                foreach (var p in pts) p.Dispose();
            }
        }
    }
}

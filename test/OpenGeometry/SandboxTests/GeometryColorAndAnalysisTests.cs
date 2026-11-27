using System;
using System.Collections.Generic;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSCore;
using Dynamo.Wpf.Rendering;
using NUnit.Framework;

namespace OpenGeometry.SandboxTests
{
    [TestFixture, Category("IntegrationTests")]
    public sealed class GeometryColorAndAnalysisTests
    {
        [Test]
        public void WhenGeometryColorByGeometryColorTessellatesSolidSurfaceCurveAndPointThenVertexColorsMatch()
        {
            var factory = new HelixRenderPackageFactory();
            var tessParams = new TessellationParameters { ShowEdges = true };
            var tint = Color.ByARGB(255, 32, 160, 224);

            using var box = Cuboid.ByLengths(4, 4, 4);
            var coloredSolid = Modifiers.GeometryColor.ByGeometryColor(box, tint);
            var solidPkg = (HelixRenderPackage)factory.CreateRenderPackage();
            coloredSolid.Tessellate(solidPkg, tessParams);

            using var line = Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(5, 0, 0));
            var coloredLine = Modifiers.GeometryColor.ByGeometryColor(line, tint);
            var linePkg = (HelixRenderPackage)factory.CreateRenderPackage();
            coloredLine.Tessellate(linePkg, tessParams);

            using var pt = Point.ByCoordinates(1, 2, 3);
            var coloredPt = Modifiers.GeometryColor.ByGeometryColor(pt, tint);
            var ptPkg = (HelixRenderPackage)factory.CreateRenderPackage();
            coloredPt.Tessellate(ptPkg, tessParams);

            Assert.Multiple(() =>
            {
                Assert.That(solidPkg.RequiresPerVertexColoration, Is.True);
                Assert.That(solidPkg.Mesh.Positions.Count, Is.GreaterThan(0));
                Assert.That(solidPkg.Mesh.Colors.Count, Is.EqualTo(solidPkg.Mesh.Positions.Count));
                Assert.That(solidPkg.Mesh.Colors.All(c => Matches(c, tint)), Is.True);
                Assert.That(solidPkg.Lines.Positions.Count, Is.GreaterThan(0));
                Assert.That(solidPkg.Lines.Colors.Count, Is.EqualTo(solidPkg.Lines.Positions.Count));
                Assert.That(solidPkg.Lines.Colors.All(c => Matches(c, tint)), Is.True);

                Assert.That(linePkg.Lines.Positions.Count, Is.GreaterThan(0));
                Assert.That(linePkg.Lines.Colors.Count, Is.EqualTo(linePkg.Lines.Positions.Count));
                Assert.That(linePkg.Lines.Colors.All(c => Matches(c, tint)), Is.True);

                Assert.That(ptPkg.Points.Positions.Count, Is.EqualTo(1));
                Assert.That(ptPkg.Points.Colors.Count, Is.EqualTo(1));
                Assert.That(Matches(ptPkg.Points.Colors[0], tint), Is.True);
            });
        }

        [Test]
        public void WhenGeometryColorBySurfaceColorsTessellatesNurbsSurfaceThenTextureMapAndUvsSpanDomain()
        {
            var factory = new HelixRenderPackageFactory();
            var tessParams = new TessellationParameters { ShowEdges = true };

            using var surf = NurbsSurface.ByControlPoints(new[]
            {
                new[] { Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 10, 1) },
                new[] { Point.ByCoordinates(10, 0, 1), Point.ByCoordinates(10, 10, 0) }
            }, 1, 1);

            var colors = new[]
            {
                new[] { Color.ByARGB(255, 255, 0, 0), Color.ByARGB(255, 0, 255, 0) },
                new[] { Color.ByARGB(255, 0, 0, 255), Color.ByARGB(255, 255, 255, 0) }
            };

            var coloredSurf = Modifiers.GeometryColor.BySurfaceColors(surf, colors);
            var pkg = (HelixRenderPackage)factory.CreateRenderPackage();
            coloredSurf.Tessellate(pkg, tessParams);

            var uvs = pkg.Mesh.TextureCoordinates;
            var uSpan = uvs.Max(uv => uv.X) - uvs.Min(uv => uv.X);
            var vSpan = uvs.Max(uv => uv.Y) - uvs.Min(uv => uv.Y);

            Assert.Multiple(() =>
            {
                Assert.That(pkg.Mesh.Positions.Count, Is.GreaterThan(0));
                Assert.That(pkg.TextureMapsList.Count, Is.EqualTo(1));
                Assert.That(pkg.TextureMapsStrideList[0], Is.EqualTo(8));
                Assert.That(pkg.MeshVerticesRangesAssociatedWithTextureMaps.Count, Is.EqualTo(1));
                Assert.That(pkg.MeshVerticesRangesAssociatedWithTextureMaps[0], Is.EqualTo(Tuple.Create(0, pkg.Mesh.Positions.Count - 1)));
                Assert.That(uvs.Count, Is.EqualTo(pkg.Mesh.Positions.Count));
                Assert.That(uSpan, Is.GreaterThan(0.9f), "Surface UV coordinates should span [0, 1] in U for color maps.");
                Assert.That(vSpan, Is.GreaterThan(0.9f), "Surface UV coordinates should span [0, 1] in V for color maps.");
                Assert.That(uvs.All(uv => uv.X >= -1e-4f && uv.X <= 1.0001f && uv.Y >= -1e-4f && uv.Y <= 1.0001f), Is.True);
            });
        }

        [Test]
        public void WhenGeometryColorByMeshColorsAndByPointsColorsRunThenCustomColorsAreEmitted()
        {
            var factory = new HelixRenderPackageFactory();
            var tessParams = new TessellationParameters();

            using var box = Cuboid.ByLengths(2, 2, 2);
            using var mesh = Mesh.ByGeometry(box);
            var red = Color.ByARGB(255, 220, 40, 20);
            var green = Color.ByARGB(255, 20, 200, 60);
            var blue = Color.ByARGB(255, 30, 80, 230);

            var singleColored = Modifiers.GeometryColor.ByMeshColors(mesh, new[] { red }, smoothShading: false);
            var singlePkg = (HelixRenderPackage)factory.CreateRenderPackage();
            singleColored.Tessellate(singlePkg, tessParams);

            var perVertexColors = Enumerable.Range(0, mesh.VertexCount)
                .Select(i => i % 2 == 0 ? green : blue)
                .ToArray();
            var smoothColored = Modifiers.GeometryColor.ByMeshColors(mesh, perVertexColors, smoothShading: true);
            var smoothPkg = (HelixRenderPackage)factory.CreateRenderPackage();
            smoothColored.Tessellate(smoothPkg, tessParams);

            // Exercise ByPointsColors (which calls Point.IsAlmostEqualTo internally).
            var triPts = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(4, 0, 0),
                Point.ByCoordinates(0, 4, 0)
            };
            var triColors = new[] { red, green, blue };
            var ptsColored = Modifiers.GeometryColor.ByPointsColors(triPts, triColors);
            var triPkg = (HelixRenderPackage)factory.CreateRenderPackage();
            ptsColored.Tessellate(triPkg, tessParams);

            Assert.Multiple(() =>
            {
                Assert.That(singlePkg.Mesh.Positions.Count, Is.EqualTo(mesh.TriangleCount * 3));
                Assert.That(singlePkg.Mesh.Colors.All(c => Matches(c, red)), Is.True);

                Assert.That(smoothPkg.Mesh.Positions.Count, Is.EqualTo(mesh.TriangleCount * 3));
                Assert.That(smoothPkg.Mesh.Colors.Any(c => Matches(c, green)), Is.True);
                Assert.That(smoothPkg.Mesh.Colors.Any(c => Matches(c, blue)), Is.True);

                Assert.That(triPkg.Mesh.Positions.Count, Is.EqualTo(3));
                Assert.That(Matches(triPkg.Mesh.Colors[0], red), Is.True);
                Assert.That(Matches(triPkg.Mesh.Colors[1], green), Is.True);
                Assert.That(Matches(triPkg.Mesh.Colors[2], blue), Is.True);
            });
        }

        [Test]
        public void WhenAnalysisLabelAndGradientUtilsRunThenLabelsAndInterpolatedValuesAreProduced()
        {
            var factory = new HelixRenderPackageFactory();
            using var pt = Point.ByCoordinates(3, 4, 5);
            var label = Label.ByPointAndString(pt, "OpenGeometry");
            var pkg = (HelixRenderPackage)factory.CreateRenderPackage();
            label.Tessellate(pkg, new TessellationParameters());

            var uvs = new[]
            {
                UV.ByCoordinates(0, 0),
                UV.ByCoordinates(1, 0),
                UV.ByCoordinates(0, 1),
                UV.ByCoordinates(1, 1)
            };
            var values = new[] { 0.0, 10.0, 10.0, 20.0 };
            var grid = Utils.CreateGradientValueMap(values, uvs, 2, 2);

            Assert.Multiple(() =>
            {
                Assert.That(pkg.DisplayLabels, Is.True);
                Assert.That(pkg.LabelData.Count, Is.EqualTo(1));
                Assert.That(pkg.LabelData[0].Item1, Is.EqualTo("OpenGeometry"));
                Assert.That(grid.Length, Is.EqualTo(3));
                Assert.That(grid[0][0], Is.EqualTo(0.0).Within(1e-6));
                Assert.That(grid[2][2], Is.EqualTo(20.0).Within(1e-6));
                Assert.That(grid[1][1], Is.EqualTo(10.0).Within(1e-6));
            });

            // Verify ImportHelpers SAT/SAB wrappers surface explicit NotSupportedException.
            Assert.Multiple(() =>
            {
                Assert.That(() => ImportHelpers.ImportFromSATWithUnits("dummy.sat", null), Throws.TypeOf<NotSupportedException>());
                Assert.That(() => ImportHelpers.DeserializeFromSABWithUnits(new byte[] { 1, 2, 3 }, null), Throws.TypeOf<NotSupportedException>());
            });
        }

        private static bool Matches(SharpDX.Color4 actual, Color expected)
        {
            return System.Math.Abs(actual.Red - expected.Red / 255f) < 0.01f
                && System.Math.Abs(actual.Green - expected.Green / 255f) < 0.01f
                && System.Math.Abs(actual.Blue - expected.Blue / 255f) < 0.01f
                && System.Math.Abs(actual.Alpha - expected.Alpha / 255f) < 0.01f;
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using NUnit.Framework;

namespace OpenGeometry.Tests
{
    [TestFixture, Category("UnitTests")]
    public sealed class GeometryTests
    {
        private const double CuboidVolume = 1_000;
        private const double Tolerance = 1e-6;
        private static readonly double CutVolume = CuboidVolume - 40 * Math.PI;

        [Test]
        public void WhenCylinderCrossesCuboidThenDifferenceHasAnalyticVolume()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var tool = ThroughCutter();
            using var cut = box.Difference(tool);

            Assert.That(cut.Volume, Is.EqualTo(CutVolume).Within(CutVolume * Tolerance));
        }

        [Test]
        public void WhenDifferenceResultIsDisposedThenBothInputsRemainUsable()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var tool = ThroughCutter();

            box.Difference(tool).Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(box.Volume, Is.EqualTo(CuboidVolume).Within(CuboidVolume * Tolerance));
                Assert.That(tool.Volume, Is.EqualTo(48 * Math.PI).Within(48 * Math.PI * Tolerance));
            });
        }

        [Test]
        public void WhenCuboidDimensionsAreInvalidThenBridgeReportsAnArgumentError()
        {
            Assert.That(() => Cuboid.ByLengths(0, 10, 10), Throws.ArgumentException);
        }

        [Test]
        public void WhenCutIsTessellatedThenRenderPackageHasAnOpenBore()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var tool = ThroughCutter();
            using var cut = box.Difference(tool);
            var package = new DefaultRenderPackage();

            ((IGraphicItem)cut).Tessellate(package, new TessellationParameters());

            var vertices = package.MeshVertices.ToArray();
            var triangles = vertices.Chunk(9).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(package.MeshVertexCount, Is.GreaterThan(0));
                Assert.That(vertices.Length % 9, Is.Zero);
                Assert.That(package.MeshNormals.Count(), Is.EqualTo(vertices.Length));
                Assert.That(triangles.Any(CoversBoreCenter), Is.False);
                Assert.That(triangles.Any(t => t.Chunk(3).All(p => Math.Abs(p[0] * p[0] + p[1] * p[1] - 4) < Tolerance)),
                    Is.True, "The cylinder wall should appear in the mesh.");
            });
        }

        [Test]
        public void WhenCylinderEndpointsAreHorizontalThenCutterRemovesTheSameVolume()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var start = Point.ByCoordinates(6, 0, 0);
            using var end = Point.ByCoordinates(-6, 0, 0);
            using var tool = Cylinder.ByPointsRadius(start, end, 2);
            using var cut = box.Difference(tool);

            Assert.That(cut.Volume, Is.EqualTo(CutVolume).Within(CutVolume * Tolerance));
        }

        [Test]
        public void WhenManagedThreadsUseTheKernelThenResultsStayConsistent()
        {
            var volumes = new double[8];
            Parallel.For(0, volumes.Length, index =>
            {
                using var box = Cuboid.ByLengths(10, 10, 10);
                using var tool = ThroughCutter();
                using var cut = box.Difference(tool);
                volumes[index] = cut.Volume;
            });

            Assert.That(volumes, Is.All.EqualTo(CutVolume).Within(CutVolume * Tolerance));
        }

        [Test]
        public void WhenGeometryRunsThenOnlyTheOpenKernelIsLoaded()
        {
            using var box = Cuboid.ByLengths(10, 10, 10);
            using var process = Process.GetCurrentProcess();
            var modules = process.Modules.Cast<ProcessModule>().Select(module => module.ModuleName).ToArray();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Assert.Multiple(() =>
            {
                Assert.That(modules, Does.Contain("OpenGeometryBridge.dll").IgnoreCase);
                Assert.That(modules, Does.Contain("SM_API.dll").IgnoreCase);
                Assert.That(modules, Does.Contain("SMLib.dll").IgnoreCase);
                Assert.That(modules.Any(name => name.StartsWith("ASM", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("LibG", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("usd_", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("_omni_solid", StringComparison.OrdinalIgnoreCase)), Is.False);
                Assert.That(assemblies.Where(assembly => assembly.GetName().Name == "ProtoGeometry"),
                    Is.EqualTo(new[] { typeof(Solid).Assembly }));
            });
        }

        [Test]
        public void WhenGeometriesTessellateThenPerVertexPreviewColorsArePopulated()
        {
            var tessParams = new TessellationParameters();

            using var box = Cuboid.ByLengths(4, 4, 4);
            var boxPkg = new DefaultRenderPackage();
            box.Tessellate(boxPkg, tessParams);
            var boxColors = boxPkg.MeshVertexColors.ToArray();

            using var surf = box.Faces[0].SurfaceGeometry();
            var surfPkg = new DefaultRenderPackage();
            surf.Tessellate(surfPkg, tessParams);
            var surfColors = surfPkg.MeshVertexColors.ToArray();

            using var mesh = Mesh.ByGeometry(box);
            var meshPkg = new DefaultRenderPackage();
            mesh.Tessellate(meshPkg, tessParams);
            var meshColors = meshPkg.MeshVertexColors.ToArray();

            using var panels = PanelSurface.ByQuads(surf, 2, 2);
            var panelPkg = new DefaultRenderPackage();
            panels.Tessellate(panelPkg, tessParams);
            var panelColors = panelPkg.MeshVertexColors.ToArray();

            using var line = Line.ByStartPointEndPoint(Point.Origin(), Point.ByCoordinates(5, 0, 0));
            var linePkg = new DefaultRenderPackage();
            line.Tessellate(linePkg, tessParams);
            var lineColors = linePkg.LineStripVertexColors.ToArray();

            using var pt = Point.ByCoordinates(1, 2, 3);
            var ptPkg = new DefaultRenderPackage();
            pt.Tessellate(ptPkg, tessParams);
            var ptColors = ptPkg.PointVertexColors.ToArray();

            using var cs = CoordinateSystem.Identity();
            var csPkg = new DefaultRenderPackage();
            cs.Tessellate(csPkg, tessParams);
            var csColors = csPkg.LineStripVertexColors.ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(boxColors.Length, Is.EqualTo(boxPkg.MeshVertexCount * 4));
                Assert.That(boxColors, Is.All.EqualTo((byte)255), "Solid preview vertices must default to white (255,255,255,255).");
                Assert.That(surfColors.Length, Is.EqualTo(surfPkg.MeshVertexCount * 4));
                Assert.That(surfColors, Is.All.EqualTo((byte)255));
                Assert.That(meshColors.Length, Is.EqualTo(meshPkg.MeshVertexCount * 4));
                Assert.That(meshColors, Is.All.EqualTo((byte)255));
                Assert.That(panelColors.Length, Is.EqualTo(panelPkg.MeshVertexCount * 4));
                Assert.That(panelColors, Is.All.EqualTo((byte)255));
                Assert.That(lineColors.Length, Is.EqualTo(linePkg.LineVertexCount * 4));
                Assert.That(ptColors.Length, Is.EqualTo(ptPkg.PointVertexCount * 4));
                Assert.That(csPkg.LineVertexCount, Is.EqualTo(6));
                Assert.That(csColors.Length, Is.EqualTo(24));
            });
        }

        private static Cylinder ThroughCutter()
        {
            using var start = Point.ByCoordinates(0, 0, -6);
            using var end = Point.ByCoordinates(0, 0, 6);
            return Cylinder.ByPointsRadius(start, end, 2);
        }

        private static bool CoversBoreCenter(double[] t)
        {
            if (Math.Abs(t[2] - t[5]) > Tolerance || Math.Abs(t[2] - t[8]) > Tolerance)
                return false;

            var a = t[0] * t[4] - t[1] * t[3];
            var b = t[3] * t[7] - t[4] * t[6];
            var c = t[6] * t[1] - t[7] * t[0];
            return (a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0);
        }
    }
}

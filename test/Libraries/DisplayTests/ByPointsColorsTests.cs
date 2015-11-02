using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using System;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class ByPointsColorsTests : GeometricTestBase
    {
        [Test]
        public void Construction_AllGood()
        {
            Assert.DoesNotThrow(() => Display.Display.ByPointsColors(
            TestVertices(), TestColors()));
        }

        [Test]
        public void Construction_NullVertices_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByPointsColors(
            null, TestColors()));
        }

        [Test]
        public void Construction_NullColors_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByPointsColors(
            TestVertices(), null));
        }

        [Test]
        public void Construction_EmptyVertices_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            new Point[] {}, TestColors()));
        }

        [Test]
        public void Construction_EmptyColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            TestVertices(), new Color[] { }));
        }

        [Test]
        public void Construction_UnequalPointsAndColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            TestVertices(), new Color[] { Color.ByARGB(), Color.ByARGB(255, 0, 0, 255) }));
        }

        [Test]
        public void Construction_ThreePointsInALine_DoesNotDrawTriangle()
        {
            var displayMesh = Display.Display.ByPointsColors(TestVerticesInALine(), TestColors().Take(3).ToArray());
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(0, package.MeshVertices.Count());
        }

        [Test]
        public void Construction_TWoPointInSamePlace_DoesNotDrawTriangle()
        {
            var displayMesh = Display.Display.ByPointsColors(TestVerticesTwoInSamePlace(), TestColors().Take(3).ToArray());
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();
            displayMesh.Tessellate(package, new TessellationParameters());
            Assert.AreEqual(0, package.MeshVertices.Count());
        }

        private static Point[] TestVerticesInALine()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(1, 0, 0);
            var c = Point.ByCoordinates(2, 0, 0);

            return new[] { a, b, c};
        }

        private static Point[] TestVerticesTwoInSamePlace()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(0, 0, 0);
            var c = Point.ByCoordinates(2, 0, 0);

            return new[] { a, b, c };
        }

        private static Point[] TestVertices()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(1, 0, 0);
            var c = Point.ByCoordinates(1, 1, 0);
            var d = Point.ByCoordinates(0, 1, 1);

            return new[] { a, b, c, a, c, d };
        }

        private static Color[] TestColors()
        {
            var a = Color.ByARGB(255, 0, 0, 255);
            var b = Color.ByARGB(255, 0, 255, 255);
            var c = Color.ByARGB(255, 255, 255, 255);
            var d = Color.ByARGB(255, 255, 0, 0);

            return new[] { a, b, c, a,c, d };
        }

    }
}

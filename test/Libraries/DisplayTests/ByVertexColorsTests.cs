using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using System;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class ByVertexColorsTests : GeometricTestBase
    {
        [Test]
        public void ByMeshVertexColors_Construction_AllGood()
        {
            Assert.DoesNotThrow(() => Display.Display.ByPointsColors(
            TestVertices(), TestColors()));
        }

        [Test]
        public void ByMeshVertexColors_Construction_NullVertices_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByPointsColors(
            null, TestColors()));
        }

        [Test]
        public void ByMeshVertexColors_Construction_NullColors_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByPointsColors(
            TestVertices(), null));
        }

        [Test]
        public void ByMeshVertexColors_Construction_EmptyVertices_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            new Point[] {}, TestColors()));
        }

        [Test]
        public void ByMeshVertexColors_Construction_EmptyColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            TestVertices(), new Color[] { }));
        }

        [Test]
        public void ByMeshVertexColors_Construction_UnequalPointsAndColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByPointsColors(
            TestVertices(), new Color[] { Color.ByARGB(), Color.ByARGB(255, 0, 0, 255) }));
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

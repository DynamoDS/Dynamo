using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using System;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class ByMeshVertexColorsTests : GeometricTestBase
    {
        [Test]
        public void ByMeshVertexColors_Construction_AllGood()
        {
            Assert.DoesNotThrow(() => Display.Display.ByMeshVertexColors(
            TestMesh(), TestColors()));
        }

        [Test]
        public void ByMeshVertexColors_Construction_NullMesh_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByMeshVertexColors(
            null, TestColors()));
        }

        [Test]
        public void ByMeshVertexColors_Construction_NullColors_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.Display.ByMeshVertexColors(
            TestMesh(), null));
        }

        [Test]
        public void ByMeshVertexColors_Construction_EmptyColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByMeshVertexColors(
            TestMesh(), new Color[] { }));
        }

        [Test]
        public void ByMeshVertexColors_Construction_UnequalPointsAndColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.Display.ByMeshVertexColors(
            TestMesh(), new Color[] { Color.ByARGB(), Color.ByARGB(255, 0, 0, 255) }));
        }

        private static Mesh TestMesh()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(1, 0, 0);
            var c = Point.ByCoordinates(1, 1, 0);
            var d = Point.ByCoordinates(0, 1, 1);

            var igA = IndexGroup.ByIndices(0, 1, 2);
            var igB = IndexGroup.ByIndices(0, 2, 3);

            return Mesh.ByPointsFaceIndices(new[] { a, b, c, d }, new[] { igA, igB });
        }

        private static Color[] TestColors()
        {
            var a = Color.ByARGB(255, 0, 0, 255);
            var b = Color.ByARGB(255, 0, 255, 255);
            var c = Color.ByARGB(255, 255, 255, 255);
            var d = Color.ByARGB(255, 255, 0, 0);

            return new[] { a, b, c, d };
        }

    }
}

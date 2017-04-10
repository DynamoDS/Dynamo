using System;
using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class BySurfaceUvsColorsTests : GeometricTestBase
    {
        [Test]
        public void BySurfaceUVsColors_Construction_AllGood()
        {
            Assert.DoesNotThrow(() => Modifiers.GeometryColor.BySurfaceColors(
                CreateOneSurface(), CreateTwoRowsOfColors()));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_NullSurface_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Modifiers.GeometryColor.BySurfaceColors(
                null, CreateTwoRowsOfColors()));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_NullColors_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Modifiers.GeometryColor.BySurfaceColors(
                CreateOneSurface(), null));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_NoColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.BySurfaceColors(
                CreateOneSurface(), new Color[][]{}));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_SingleDimensionColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.BySurfaceColors(
                CreateOneSurface(), CreateOneRowOfColors()));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_JaggedArrayColors_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Modifiers.GeometryColor.BySurfaceColors(
                CreateOneSurface(), CreateOneRowOfColors()));
        }

        private static Surface CreateOneSurface()
        {
            var rect = Rectangle.ByWidthLength(5, 5);
            var surface = Surface.ByPatch(rect);
            return surface;
        }

        private static Color[][] CreateOneRowOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
            };

            return colors;
        }

        private static Color[][] CreateTwoRowsOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
                new []
                {
                    Color.ByARGB(0, 0, 255),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(255, 255, 0, 0),
                }
            };

            return colors;
        }

        private static Color[][] CreateJaggedArrayOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
                new []
                {
                    Color.ByARGB(0, 0, 255),
                    Color.ByARGB(255, 0, 255, 0),
                }
            };

            return colors;
        }
    }
}

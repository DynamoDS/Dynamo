using System;
using System.Collections.Generic;
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
            Assert.DoesNotThrow(()=>Display.BySurfaceUvsColors(
                CreateOneSurface(), CreateThreeUvs(), CreateThreeColors())); 
        }

        [Test]
        public void BySurfaceUvsColors_Construction_NullSurface_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.BySurfaceUvsColors(
                null, CreateThreeUvs(), CreateThreeColors()));
        }

        [Test]
        public void BySurfaceUvsColors_Construction_NullUvs_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Display.BySurfaceUvsColors(
                CreateOneSurface(), null, CreateThreeColors())); 
        }

        [Test]
        public void BySurfaceUvsColors_Construction_EmptyUvs_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => Display.BySurfaceUvsColors(
                CreateOneSurface(), new UV[]{}, CreateThreeColors())); 
        }

        [Test]
        public void BySurfaceUvsColors_Construction_PrecisionTooLow_ThrowsException()
        {
            Assert.Throws<Exception>(() => Display.BySurfaceUvsColors(
                CreateOneSurface(), CreateThreeUvs(), CreateThreeColors(), -42.0)); 
        }

        [Test]
        public void BySurfaceUvsColors_Construction_PrecisionTooHigh_ThrowsException()
        {
            Assert.Throws<Exception>(() => Display.BySurfaceUvsColors(
                CreateOneSurface(), CreateThreeUvs(), CreateThreeColors(), 42.0)); 
        }

        [Test]
        public void BySurfaceUvsColors_Construction_UvsColorsCountMismatch_ThrowsException()
        {
            Assert.Throws<Exception>(() => Display.BySurfaceUvsColors(
                CreateOneSurface(), CreateTwoUvs(), CreateThreeColors(), 42.0)); 
        }

        private static Surface CreateOneSurface()
        {
            var rect = Rectangle.ByWidthLength(5, 5);
            var surface = Surface.ByPatch(rect);
            return surface;
        }

        private static UV[] CreateThreeUvs()
        {
            var uvs = new[]
            {
                UV.ByCoordinates(0.2, 0.5),
                UV.ByCoordinates(0.1, 0.9),
                UV.ByCoordinates(0.9, 0.3)
            };

            return uvs;
        }

        private static UV[] CreateTwoUvs()
        {
            var uvs = new[]
            {
                UV.ByCoordinates(0.2, 0.5),
                UV.ByCoordinates(0.1, 0.9),
            };

            return uvs;
        }

        private static Color[] CreateThreeColors()
        {
            var colors = new[]
            {
                Color.ByARGB(255, 255, 0, 0),
                Color.ByARGB(255, 0, 255, 0),
                Color.ByARGB(0, 0, 255)
            };

            return colors;
        }
    }
}

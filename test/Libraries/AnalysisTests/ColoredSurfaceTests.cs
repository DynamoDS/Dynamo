using System;

using Analysis;

using Autodesk.DesignScript.Geometry;

using DSCore;

using NUnit.Framework;

using TestServices;

namespace AnalysisTests
{
    [TestFixture]
    public class ColoredSurfaceTests  :GeometricTestBase
    {
        [Test]
        public void ColoredSurfaceByPointsAndUVs_GoodArgs()
        {
            var surface = SetupTestSurface();
            var colors = SetupTestColors();
            var uvs = SetupTestUVs();

            var cs = ColoredSurface.ByColorsAndUVs(surface, colors,uvs) ;
            Assert.NotNull(cs);
        }

        [Test]
        public void ColoredSurfaceByPointsAndUVs_BadArgs()
        {
            var surface = SetupTestSurface();
            var colors = SetupTestColors();
            var uvs = SetupTestUVs();

            Assert.Throws<ArgumentNullException>(
                () => ColoredSurface.ByColorsAndUVs(null, colors, uvs));
            Assert.Throws<ArgumentNullException>(
                () => ColoredSurface.ByColorsAndUVs(surface, null, uvs));
            Assert.Throws<ArgumentNullException>(
                () => ColoredSurface.ByColorsAndUVs(surface, colors, null));
            Assert.Throws<ArgumentException>(
                () => ColoredSurface.ByColorsAndUVs(surface, new Color[]{}, uvs));
            Assert.Throws<ArgumentException>(
                () => ColoredSurface.ByColorsAndUVs(surface, colors, new UV[]{}));

        }

        private static UV[] SetupTestUVs()
        {
            var uv1 = UV.ByCoordinates(0, 0);
            var uv2 = UV.ByCoordinates(0.5, 0.5);
            var uv3 = UV.ByCoordinates(1, 1);
            return new []{uv1,uv2,uv3};
        }

        private static Color[] SetupTestColors()
        {
            var c1 = Color.ByARGB(255, 255, 0, 0); // red
            var c2 = Color.ByARGB(255, 0, 255, 0);
            var c3 = Color.ByARGB(255, 0, 0, 255);
            return new[] { c1, c2, c3 };
        }

        private static Surface SetupTestSurface()
        {
            var pt1 = Point.ByCoordinates(0, 0);
            var pt2 = Point.ByCoordinates(0, 1);
            var pt3 = Point.ByCoordinates(1, 1);
            var pt4 = Point.ByCoordinates(1, 0);
            var surface = Surface.ByPerimeterPoints(new[] { pt1, pt2, pt3, pt4 });
            return surface;
        }
    }
}

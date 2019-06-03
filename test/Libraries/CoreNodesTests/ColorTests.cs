using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using TestServices;

namespace DSCoreNodesTests
{
    /// <summary>
    /// PB:  This is just a starting point, just to check that ProtoGeometry is properly working in Dynamo.
    /// 
    /// It should probably get refactored out at some point.
    /// </summary>
    [TestFixture]
    public class ColorTests : GeometricTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestConstructorByARGB()
        {
            Color color = Color.ByARGB(10, 20, 30, 40);
            Assert.AreEqual(10, color.InternalColor.A);
            Assert.AreEqual(20, color.InternalColor.R);
            Assert.AreEqual(30, color.InternalColor.G);
            Assert.AreEqual(40, color.InternalColor.B);
        }

        // [Test]
        // public void TestConstructorBySystemColor()
        // {
        //     System.Drawing.Color original = System.Drawing.Color.FromArgb(10, 20, 30, 40);
        //     Color color = Color.BySystemColor(original);
        //     Assert.AreEqual(10, color.InternalColor.A);
        //     Assert.AreEqual(20, color.InternalColor.R);
        //     Assert.AreEqual(30, color.InternalColor.G);
        //     Assert.AreEqual(40, color.InternalColor.B);
        // }

        [Test]
        [Category("UnitTests")]
        public void TestBrightness()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(0.625490189, Color.Brightness(color), 0.000001);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSaturation()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(1.0, Color.Saturation(color), 0.000001);
        }

        [Test]
        [Category("UnitTests")]
        public void TestHue()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(209.842926, Color.Hue(color), 0.000001);
        }

        [Test]
        [Category("UnitTests")]
        public void TestComponents()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            var components = Color.Components(color);

            Assert.AreEqual(128, components["a"]);
            Assert.AreEqual(64,  components["r"]);
            Assert.AreEqual(160, components["g"]);
            Assert.AreEqual(255, components["b"]);
        }


        [Test]
        [Category("UnitTests")]
        public void TestAddColor()
        {
            Color color = Color.ByARGB(100, 100, 100, 100);
            Color newColor = Color.Add(color, color);
            Assert.AreEqual(newColor.Alpha, 200);
            Assert.AreEqual(newColor.Green, 200);
            Assert.AreEqual(newColor.Blue, 200);
            Assert.AreEqual(newColor.Red, 200);
        }


        [Test]
        [Category("UnitTests")]
        public void TestMultiplyColor()
        {
            Color color = Color.ByARGB(100, 100, 100, 100);
            Color newColor = Color.Multiply(color, 2);
            Assert.AreEqual(newColor.Alpha, 200);
            Assert.AreEqual(newColor.Green, 200);
            Assert.AreEqual(newColor.Blue, 200);
            Assert.AreEqual(newColor.Red, 200);
        }


        [Test]
        [Category("UnitTests")]
        public void TestDivideColor()
        {
            Color color = Color.ByARGB(100, 100, 100, 100);
            Color newColor = Color.Divide(color, 2);
            Assert.AreEqual(newColor.Alpha, 50);
            Assert.AreEqual(newColor.Green, 50);
            Assert.AreEqual(newColor.Blue, 50);
            Assert.AreEqual(newColor.Red, 50);
        }


        [Test]
        [Category("UnitTests")]
        public void TestDivideColorByZero()
        {
            Color color = Color.ByARGB(100, 100, 100, 100);
            Assert.Throws<ArgumentException>(() => Color.Divide(color, 0));
        }


        [Test]
        [Category("UnitTests")]
        public void TestBuildColorFrom1DRange()
        {
            Color color1 = Color.ByARGB(255, 255, 0, 0);
            Color color2 = Color.ByARGB(255, 0, 255, 0);       
            Color color3 = Color.ByARGB(255, 0, 0, 255);
            List<Color> colorList = new List<Color> { color1, color2, color3 };
            List<double> parameters = new List<double> { 0, 0.75, 1 };
            double parameter = 0.75;
            Color colorSample = Color.BuildColorFrom1DRange(colorList, parameters, parameter);

            Assert.AreEqual(colorSample.Alpha, 255);
            Assert.AreEqual(colorSample.Red, 0);
            Assert.AreEqual(colorSample.Green, 255);
            Assert.AreEqual(colorSample.Blue, 0);
        }

        [Test]
        [Category("UnitTests")]
        public void TestBuildColorFrom2DRange()
        {
            Color color1 = Color.ByARGB(255, 255, 0, 0);
            Color color2 = Color.ByARGB(255, 0, 255, 0);
            Color color3 = Color.ByARGB(255, 0, 0, 255);
            Color color4 = Color.ByARGB(255, 255, 255, 255);
            List<Color> colorList = new List<Color> { color1, color2, color3 };
            List<UV> uvParameters = new List<UV>();
            uvParameters.Add(UV.ByCoordinates(0, 0));
            uvParameters.Add(UV.ByCoordinates(0, 1));
            uvParameters.Add(UV.ByCoordinates(1, 0));
            uvParameters.Add(UV.ByCoordinates(1, 1));
            UV parameter = UV.ByCoordinates(0, 1);
            Color colorSample = Color.BuildColorFrom2DRange(colorList, uvParameters, parameter);

            Assert.AreEqual(colorSample.Alpha, 255);
            Assert.AreEqual(colorSample.Red, 0);
            Assert.AreEqual(colorSample.Green, 255);
            Assert.AreEqual(colorSample.Blue, 0);
        }

        [Test]
        [Category("UnitTests")]
        // if UV parameter equals any color parameter
        public void TestBlerp1()
        {
            Color color1 = Color.ByARGB(255, 255, 0, 0);
            Color color2 = Color.ByARGB(255, 0, 255, 0);
            Color color3 = Color.ByARGB(255, 0, 0, 255);
            Color color4 = Color.ByARGB(255, 255, 255, 255);
            UV uv1 = UV.ByCoordinates(0, 0);
            UV uv2 = UV.ByCoordinates(0, 1);
            UV uv3 = UV.ByCoordinates(1, 0);
            UV uv4 = UV.ByCoordinates(1, 1);
            List<Color.IndexedColor2D> colors = new List<Color.IndexedColor2D>();        
            colors.Add(new Color.IndexedColor2D (color1, uv1));
            colors.Add(new Color.IndexedColor2D(color2, uv2));
            colors.Add(new Color.IndexedColor2D(color3, uv3));
            colors.Add(new Color.IndexedColor2D(color4, uv4));
            UV param = UV.ByCoordinates(1, 0);
            Color interpolatedColor = Color.Blerp(colors, param);

            Assert.AreEqual(interpolatedColor.Alpha, 255);
            Assert.AreEqual(interpolatedColor.Red, 0);
            Assert.AreEqual(interpolatedColor.Green, 0);
            Assert.AreEqual(interpolatedColor.Blue, 255);
        }

       
        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        // if UV parameter does not equal any color parameter
        // The expected value is calculated using the bilinear interpolation algorithm taken from wikipedia 
        // https://en.wikipedia.org/wiki/Bilinear_interpolation
        public void TestBlerp2()
        {
            Color color1 = Color.ByARGB(255, 255, 0, 0);
            Color color2 = Color.ByARGB(255, 0, 255, 0);
            Color color3 = Color.ByARGB(255, 0, 0, 255);
            Color color4 = Color.ByARGB(255, 255, 255, 255);
            UV uv1 = UV.ByCoordinates(0, 0);
            UV uv2 = UV.ByCoordinates(0, 1);
            UV uv3 = UV.ByCoordinates(1, 0);
            UV uv4 = UV.ByCoordinates(1, 1);
            List<Color.IndexedColor2D> colors = new List<Color.IndexedColor2D>();
            colors.Add(new Color.IndexedColor2D(color1, uv1));
            colors.Add(new Color.IndexedColor2D(color2, uv2));
            colors.Add(new Color.IndexedColor2D(color3, uv3));
            colors.Add(new Color.IndexedColor2D(color4, uv4));
            UV param = UV.ByCoordinates(0.25, 0.25);
            Color interpolatedColor = Color.Blerp(colors, param);

            Assert.AreEqual(interpolatedColor.Alpha, 255);
            Assert.AreEqual(interpolatedColor.Red, 159);
            Assert.AreEqual(interpolatedColor.Green, 63);
            Assert.AreEqual(interpolatedColor.Blue, 63);
        }


        [Test]
        [Category("UnitTests")]
        // if UV parameter does not equal any color parameter
        public void TestBlerp3()
        {
            Color color1 = Color.ByARGB(255, 255, 0, 0);
            Color color2 = Color.ByARGB(255, 0, 255, 0);
            Color color3 = Color.ByARGB(255, 0, 0, 255);
            Color color4 = Color.ByARGB(255, 255, 255, 255);
            UV uv1 = UV.ByCoordinates(0.2, 0.2);
            UV uv2 = UV.ByCoordinates(0.2, 0.8);
            UV uv3 = UV.ByCoordinates(0.8, 0.2);
            UV uv4 = UV.ByCoordinates(0.8, 0.8);
            List<Color.IndexedColor2D> colors = new List<Color.IndexedColor2D>();
            colors.Add(new Color.IndexedColor2D(color1, uv1));
            colors.Add(new Color.IndexedColor2D(color2, uv2));
            colors.Add(new Color.IndexedColor2D(color3, uv3));
            colors.Add(new Color.IndexedColor2D(color4, uv4));
            UV param = UV.ByCoordinates(0.5, 0.5);
            Color interpolatedColor = Color.Blerp(colors, param);

            Assert.AreEqual(interpolatedColor.Alpha, 255);
            Assert.AreEqual(interpolatedColor.Red, 127);
            Assert.AreEqual(interpolatedColor.Green, 127);
            Assert.AreEqual(interpolatedColor.Blue, 127);
        }

    }
}

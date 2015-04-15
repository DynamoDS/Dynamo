using System.Collections.Generic;

using DSCore;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ColorRangeTests
    {
        private Color red = Color.ByARGB(255, 255, 0, 0);
        private Color blue = Color.ByARGB(255, 0, 0, 255);
        private Color orange = Color.ByARGB(255, 253, 0);

        [Test]
        public void ColorRange1D_ByColorsAndParameters_ValidArgs()
        {
            var red = Color.ByARGB(255, 255, 0, 0);
            var blue = Color.ByARGB(255, 0, 0, 255);

            var colors = new List<Color>{red,blue};
            var parameters = new List<double>(){0.0,1.0};

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_HasSomeColorsIfNoneProvided()
        {
            var colors = new List<Color>();
            var parameters = new List<double>{ 0.0, 1.0 };

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_HasSomeParametersIfNoneProvided()
        {
            var colors = new List<Color>(){red, blue};
            var parameters = new List<double>();

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);
            
            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_HasSomeColorsIfNothingProvided()
        {
            var colors = new List<Color>();
            var parameters = new List<double>();

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_MakesSolidColorRangeWithOneColor()
        {
            var colors = new List<Color>{red};
            var parameters = new List<double> { 0.0 };

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), red);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_RemapsParametersThatAreOutOfRange()
        {
            var colors = new List<Color> { red, blue };
            var parameters = new List<double> { 26.5, -13.2 };

            var range = ColorRange1D.ByColorsAndParameters(colors, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), blue);
            Assert.AreEqual(range.GetColorAtParameter(1), red);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_HandlesNullParameters()
        {
            var colors = new List<Color> { red, orange, blue };

            var range = ColorRange1D.ByColorsAndParameters(colors, null);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(0.5), orange);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

        [Test]
        public void ColorRange1D_ByColorsAndParameters_HandlesNullColors()
        {
            var parameters = new List<double> { 0.0, 0.5, 1.0};

            var range = ColorRange1D.ByColorsAndParameters(null, parameters);

            Assert.AreEqual(range.GetColorAtParameter(0), red);
            Assert.AreEqual(range.GetColorAtParameter(1), blue);
        }

    }
}

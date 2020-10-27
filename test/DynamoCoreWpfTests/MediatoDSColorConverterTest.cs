using System.Windows.Media;
using CoreNodeModelsWpf.Converters;
using Dynamo.Tests;
using NUnit.Framework;
using DSColor = DSCore.Color;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class MediatoDSColorConverterTest
    {
        /// <summary>
        /// This test method will validate the Convert() and ConvertBack() methods from the MediatoDSColorConverterConvertConvertBack class.
        /// </summary>
        [Test]
        public void MediatoDSColorConverterConvertConvertBackTest()
        {
            //Red DSColor
            var redDSColor = DSColor.ByARGB(255, 255, 0, 0);

            var dsColorConverter = new MediatoDSColorConverter();

            //Convert from DSColor to Color
            var colorConverted = (Color)dsColorConverter.Convert(redDSColor, null, null, null);

            //Validates that the convertion to Color was successful
            Assert.IsNotNull(colorConverted);
            Assert.AreEqual(colorConverted.GetType(), typeof(Color));
            Assert.AreEqual(colorConverted.ToString(), "#FFFF0000");

            //Convert back from Color to DSColor
            var dsColorBack = (DSColor)dsColorConverter.ConvertBack(colorConverted, null, null, null);

            //Validates that the convertion to DSColor was successful
            Assert.IsNotNull(dsColorBack);
            Assert.AreEqual(dsColorBack.GetType(), typeof(DSColor));
            Assert.AreEqual(dsColorBack.ToString(), "Color(Red = 255, Green = 0, Blue = 0, Alpha = 255)");

        }
    }
}

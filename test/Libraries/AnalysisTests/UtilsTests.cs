using System.Collections.Generic;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;

namespace AnalysisTests
{
    [TestFixture]
    class UtilsTests
    {
        
        /// <summary>
        /// This test method will execute the CreateGradientValueMap method from the Utils class
        /// </summary>
        [Test, Category("UnitTests")]
        public void CreateGradientValueMapTest()
        {
            //Arrange
            //Creates a IEnumerable range with values from 1 to 20
            IEnumerable<double> values = from value in Enumerable.Range(1, 20)
                                               select (double)value;

            //Creates a IEnumerable locations with coordinate values like (1,1), (2,2) .....
            IEnumerable<UV> locationsUV = from value in Enumerable.Range(1, 20)
                                               select UV.ByCoordinates(value, value);

            //Act
            var colorDouble = Utils.CreateGradientValueMap(values, locationsUV, 0, 0);
            //Pass the values with a empty IEnumerable so it will return a empty value map
            var emptyDouble = Utils.CreateGradientValueMap(new double[0] , locationsUV, 0, 0);

            //Assert
            //Validates that the Gradient Value Map was created sucessfully with 1 value
            Assert.IsNotNull(colorDouble);
            Assert.AreEqual(colorDouble.Count(), 1);
            //Validates that the Gradient Value Map was NOT created
            Assert.AreEqual(emptyDouble.Count(), 0);
        }

        /// <summary>
        /// This test method will execute the CreateGradientColorMap and CreateAnalyticalColorRange methods from the Utils class
        /// </summary>
        [Test, Category("UnitTests")]
        public void CreateGradientColorMapTest()
        {
            //Arrange
            //Create a color range (3 colors)
            var colorRange = Utils.CreateAnalyticalColorRange();

            IEnumerable<UV> locations = new List<UV>() { UV.ByCoordinates(0, 0), UV.ByCoordinates(1, 0), UV.ByCoordinates(0, 1) };

            var colorsArray = (from color in colorRange.IndexedColors select color.Color).ToArray();

            //Act
            var colorMapArray = Utils.CreateGradientColorMap(colorsArray, locations, 2, 2);
            //When passing the Colors array empty it will return an empty Color array
            var empyColor = Utils.CreateGradientColorMap(new Color[0], locations, 2, 2);

            //Assert
            //Validate that the CreateAnalyticalColorRange method returned a valid ColorRange1D
            Assert.IsNotNull(colorRange);
            Assert.AreEqual(empyColor.Count(), 0);
            Assert.AreEqual(colorMapArray.Count(),2);
        }      
    }
}

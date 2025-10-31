using System;
using System.Collections.Generic;
using Analysis;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace AnalysisTests
{
    [TestFixture]
    class SurfaceDataTests : TestServices.GeometricTestBase
    {
        /// <summary>
        /// This test method will execute the BySurfaceAndPoints method from the SurfaceData class passing different parameters
        /// </summary>
        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfaceAndPointsTest_Exceptions()
        {
            //Act
            var sad = SurfaceData.BySurfaceAndPoints(TestSurface(),TestUvs());

            //Assert
            Assert.IsNotNull(sad);
            Assert.IsNotNull(sad.Surface);
            //The BySurfaceAndPoints method creates a surface without values, then we validate that values is null
            Assert.IsNull(sad.Values);

            //It will raise an ArgumentNullException when we pass the Surface as null
            Assert.Throws<ArgumentNullException>(() => SurfaceData.BySurfaceAndPoints(null, TestUvs()));

            //It will raise an ArgumentNullException when we pass the UV values as null
            Assert.Throws<ArgumentNullException>(() => SurfaceData.BySurfaceAndPoints(TestSurface(), null));

            //It will raise an ArgumentNullException when we pass the UV values as empty IEnumerable
            Assert.Throws<ArgumentException>(() => SurfaceData.BySurfaceAndPoints(TestSurface(), new UV[0]));

        }

        /// <summary>
        /// This test method will execute the BySurfacePointsAndValues method from the SurfaceData class passing an empty values List
        /// </summary>
        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointsAndResults_ValuesException()
        {
            //This will raise the exception thrown when the values list passed to BySurfacePointsAndValues method is empty
            Assert.Throws<ArgumentException>(() => SurfaceData.BySurfacePointsAndValues(
                TestSurface(),
                TestUvs(),
                new List<double>()) );
        }

        /// <summary>
        /// This test method will execute the IsAlmostEqualTo method from the AnalysisExtensions class
        /// </summary>
        [Test, Category("UnitTests")]
        public void AnalysisExtensionsIsAlmostEqualToTest()
        {
            //Arrange
            //Define two UV coordinates that are extremely close
            var UVCoord1 = UV.ByCoordinates(0.0000005, 0.0000005);
            var UVCoord2 = UV.ByCoordinates(0.0000004, 0.0000004);

            //Act
            var boolResult = AnalysisExtensions.IsAlmostEqualTo(UVCoord1, UVCoord2);

            //Assert
            //If is true means that the coordinates are almost equal
            Assert.IsTrue(boolResult);
        }      

        private static Surface TestSurface()
        {
            var points = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(0, 1, 0),
                Point.ByCoordinates(1, 1, 0),
                Point.ByCoordinates(1, 0, 0)
            };

            var srf = Surface.ByPerimeterPoints(points);
            return srf;
        }

        private static IEnumerable<UV> TestUvs()
        {
            var uvs = new[]
            {
                UV.ByCoordinates(0, 0),
                UV.ByCoordinates(0.5, 0.5),
                UV.ByCoordinates(1, 1)
            };

            return uvs;
        }
    }
}

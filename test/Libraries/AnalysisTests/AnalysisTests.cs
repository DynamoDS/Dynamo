using Analysis.DataTypes;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

namespace AnalysisTests
{
    [TestFixture]
    public class AnalysisTests
    {
        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfaceAndPoints_ValidArgs()
        {
            var points = new[]
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(0, 1, 0),
                Point.ByCoordinates(1, 1, 0),
                Point.ByCoordinates(1, 0, 0)
            };

            var uvs = new[]
            {
                UV.ByCoordinates(0, 0),
                UV.ByCoordinates(1, 1)
            };

            var srf = Surface.ByPerimeterPoints(points);

            var sad = SurfaceAnalysisData.BySurfaceAndPoints(srf, uvs);
            Assert.NotNull(sad);
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfaceAndPoints_BadArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointsAndResults_ValidArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void SurfaceAnalysisDataBySurfacePointAndResults_BadArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPoints_ValidArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPoints_BadArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_ValidArgs()
        {

        }

        [Test, Category("UnitTests")]
        public void PointAnalysisDataByPointsAndResults_BadArgs()
        {

        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPoints_ValidArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisdataByPoint_BadArgs()
        {
            
        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_ValidArgs()
        {

        }

        [Test, Category("UnitTests")]
        public void VectorAnalysisDataByPointsAndResults_BadArgs()
        {

        }


    }
}

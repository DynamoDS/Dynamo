using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.AnalysisDisplay;
using Revit.Application;
using NUnit.Framework;

namespace DSRevitNodesTests.AnalysisDisplay
{
    [TestFixture]
    public class PointAnalysisDisplayTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByViewPointsAndValues_ValidArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = Document.Current;
            var grid = PointAnalysisDisplay.ByViewPointsAndValues(doc.ActiveView, samplePoints, sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
        [TestModel(@".\Empty.rvt")]
        public void ByViewPointsAndValues_BadArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = Document.Current;
            Assert.Throws(typeof(System.ArgumentNullException), () => PointAnalysisDisplay.ByViewPointsAndValues(null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => PointAnalysisDisplay.ByViewPointsAndValues(doc.ActiveView, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => PointAnalysisDisplay.ByViewPointsAndValues(doc.ActiveView, samplePoints, null));
        }
    }
}

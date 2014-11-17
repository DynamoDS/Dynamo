using System.Collections.Generic;

using Analysis.DataTypes;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.AnalysisDisplay;
using Revit.Application;

using RevitNodesTests;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.AnalysisDisplay
{
    [TestFixture]
    public class VectorAnalysisDisplayTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Empty.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewPointsAndVectorValues_ValidArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                Vector.ByCoordinates(0, 2, 4),
                Vector.ByCoordinates(0, 7, 4),
                Vector.ByCoordinates(0, 19, 4)
            };

            var doc = Document.Current;
            var grid = VectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, samplePoints, sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
        [TestModel(@".\Empty.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewPointsAndVectorValues_BadArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                Vector.ByCoordinates(0, 2, 4),
                Vector.ByCoordinates(0, 7, 4),
                Vector.ByCoordinates(0, 19, 4)
            };

            var doc = Document.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => VectorAnalysisDisplay.ByViewPointsAndVectorValues(null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => VectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => VectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, samplePoints, null));
        }

        [Test]
        [TestModel(@".\Empty.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewAndVectorAnalysisData_ValidArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                Vector.ByCoordinates(0, 2, 4),
                Vector.ByCoordinates(0, 7, 4),
                Vector.ByCoordinates(0, 19, 4)
            };

            var data = VectorAnalysisData.ByPointsAndResults(
                samplePoints,
                new List<string>() { "Test vector data." },
                new List<IList<Vector>>() { sampleValues });

            var doc = Document.Current;
            var grid = VectorAnalysisDisplay.ByViewAndVectorAnalysisData(doc.ActiveView, new []{data});

            Assert.NotNull(grid);
        }

        [Test]
        [TestModel(@".\Empty.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewAndVectorAnalysisData_BadArgs()
        {
            var samplePoints = new[]
            {
                Point.ByCoordinates(0, 2, 4),
                Point.ByCoordinates(0, 7, 4),
                Point.ByCoordinates(0, 19, 4)
            };

            var sampleValues = new[]
            {
                Vector.ByCoordinates(0, 2, 4),
                Vector.ByCoordinates(0, 7, 4),
                Vector.ByCoordinates(0, 19, 4)
            };

            var data = VectorAnalysisData.ByPointsAndResults(
                samplePoints,
                new List<string>() { "Test vector data." },
                new List<IList<Vector>>() { sampleValues });

            var doc = Document.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => VectorAnalysisDisplay.ByViewAndVectorAnalysisData(null, new []{data}));
            Assert.Throws(typeof(System.ArgumentNullException), () => VectorAnalysisDisplay.ByViewAndVectorAnalysisData(doc.ActiveView, null));
            Assert.Throws(typeof(System.Exception), () => VectorAnalysisDisplay.ByViewAndVectorAnalysisData(doc.ActiveView, new VectorAnalysisData[]{}));
        }

    }
}

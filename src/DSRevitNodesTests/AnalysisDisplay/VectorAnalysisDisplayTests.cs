using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.AnalysisDisplay;
using DSRevitNodes.Application;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class VectorAnalysisDisplayTests
    {
        [Test]
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

            var doc = DSDocument.Current;
            var grid = DSVectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, samplePoints, sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
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

            var doc = DSDocument.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => DSVectorAnalysisDisplay.ByViewPointsAndVectorValues(null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSVectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSVectorAnalysisDisplay.ByViewPointsAndVectorValues(doc.ActiveView, samplePoints, null));
        }

    }
}

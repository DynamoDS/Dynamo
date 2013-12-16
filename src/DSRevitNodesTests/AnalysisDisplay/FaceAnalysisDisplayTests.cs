using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes.AnalysisDisplay;
using DSRevitNodes.Application;
using DSRevitNodes.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.AnalysisDisplay
{
    [TestFixture]
    public class FaceAnalysisDisplayTests
    {
        [Test]
        public void ByViewFacePointsAndValues_ValidArgs()
        {
            // get the face from the document
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            var form = ele as DSForm;
            var faceRef = form.FaceReferences.First();

            var samplePoints = new[]
            {
                new double[]{0,0},
                new[]{0.5,0},
                new[]{0,0.5}
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = DSDocument.Current;
            var grid = DSFaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, samplePoints,
                sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
        public void ByViewFacePointsAndValues_BadArgs()
        {
            // get the face from the document
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            var form = ele as DSForm;
            var faceRef = form.FaceReferences.First();

            var samplePoints = new[]
            {
                new double[]{0,0},
                new[]{0.5,0},
                new[]{0,0.5}
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = DSDocument.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => DSFaceAnalysisDisplay.ByViewFacePointsAndValues(null, faceRef, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => DSFaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, samplePoints, null));
        }

    }
}

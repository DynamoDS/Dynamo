using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit.AnalysisDisplay;
using Revit.Application;
using Revit.Elements;
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
            var form = ele as Form;
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

            var doc = Document.Current;
            var grid = FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, samplePoints,
                sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
        public void ByViewFacePointsAndValues_BadArgs()
        {
            // get the face from the document
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            var form = ele as Form;
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

            var doc = Document.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(null, faceRef, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, faceRef, samplePoints, null));
        }

    }
}

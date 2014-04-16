using System.Linq;
using Dynamo.Tests;
using Revit.AnalysisDisplay;
using Revit.Application;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.AnalysisDisplay
{
    [TestFixture]
    public class FaceAnalysisDisplayTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\ColumnFamilyInstance.rvt")]
        public void ByViewFacePointsAndValues_ValidArgs()
        {
            var fams = ElementSelector.ByType<Autodesk.Revit.DB.FamilyInstance>(true);
            var famInst = fams.First() as Revit.Elements.FamilyInstance;

            var faceRef = famInst.FaceReferences.First();

            var samplePoints = new[]
            {
                new double[]{0,0},
                new[]{0.1,0.2},
                new[]{0,0.1}
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
        [TestModel(@".\ColumnFamilyInstance.rvt")]
        public void ByViewFacePointsAndValues_BadArgs()
        {
            var fams = ElementSelector.ByType<Autodesk.Revit.DB.FamilyInstance>(true);
            var famInst = fams.First() as Revit.Elements.FamilyInstance;

            var faceRef = famInst.FaceReferences.First();

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

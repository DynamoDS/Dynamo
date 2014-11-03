using System.Collections.Generic;
using System.Linq;

using Analysis.DataTypes;

using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.AnalysisDisplay;
using Revit.Elements;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

using Document = Revit.Application.Document;
using UV = Autodesk.DesignScript.Geometry.UV;

namespace RevitNodesTests.AnalysisDisplay
{
    internal static class AnalysisDisplayHelpers
    {
        internal static Autodesk.DesignScript.Geometry.Surface GetFirstSurfaceInInPlaceMass()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var fec = new FilteredElementCollector(doc);
            fec.OfClass(typeof(Autodesk.Revit.DB.FamilyInstance));

            var fi = fec.ToElements().FirstOrDefault();
            if (fi == null) return null;

            var inst = (Revit.Elements.FamilyInstance)ElementSelector.ByElementId(fi.Id.IntegerValue);
            var geom = inst.Geometry();
            var solid = (Autodesk.DesignScript.Geometry.Solid)geom.FirstOrDefault(g => g is Autodesk.DesignScript.Geometry.Solid);

            return solid.Faces.First().SurfaceGeometry();
        }
    }

    [TestFixture]
    public class FaceAnalysisDisplayTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewFacePointsAndValues_ValidArgs()
        {
            var surface = AnalysisDisplayHelpers.GetFirstSurfaceInInPlaceMass();

            var samplePoints = new[]
            {
                UV.ByCoordinates(0,0),
                UV.ByCoordinates(0.1,0.2),
                UV.ByCoordinates(0,0.1),
                
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = Document.Current;
            var grid = FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, surface, samplePoints,
                sampleValues);

            Assert.NotNull(grid);
        }

        [Test]
        [TestModel(@".\AnalysisDisplay\Surfaces.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewFacePointsAndValues_BadArgs()
        {
            var surface = AnalysisDisplayHelpers.GetFirstSurfaceInInPlaceMass();

            var samplePoints = new[]
            {
                UV.ByCoordinates(0,0),
                UV.ByCoordinates(0.5,0),
                UV.ByCoordinates(0,0.5)
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var doc = Document.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(null, surface, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, null, samplePoints, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, surface, null, sampleValues));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewFacePointsAndValues(doc.ActiveView, surface, samplePoints, null));
        }

        [Test, TestModel(@".\AnalysisDisplay\Surfaces.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewAndFaceAnalysisData_ValidArgs()
        {
            var surface = AnalysisDisplayHelpers.GetFirstSurfaceInInPlaceMass();

            var samplePoints = new[]
            {
                UV.ByCoordinates(0,0),
                UV.ByCoordinates(0.1,0.2),
                UV.ByCoordinates(0,0.1),
                
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var data = SurfaceAnalysisData.BySurfacePointsAndResults(surface, samplePoints, new List<string>(){"Sample Data"}, new IList<double>[]{sampleValues} );

            var doc = Document.Current;
            var grid = FaceAnalysisDisplay.ByViewAndFaceAnalysisData(doc.ActiveView, new []{data});

            Assert.NotNull(grid);
        }

        [Test, TestModel(@".\AnalysisDisplay\Surfaces.rvt"), Category(ANALYSIS_DISPLAY_TESTS)]
        public void ByViewAndFaceAnalysisData_BadArgs()
        {
            var surface = AnalysisDisplayHelpers.GetFirstSurfaceInInPlaceMass();

            var samplePoints = new[]
            {
                UV.ByCoordinates(0,0),
                UV.ByCoordinates(0.5,0),
                UV.ByCoordinates(0,0.5)
            };

            var sampleValues = new[]
            {
                1.0,
                1092,
                -1
            };

            var data = SurfaceAnalysisData.BySurfacePointsAndResults(surface, samplePoints, new List<string>() { "Sample Data" }, new IList<double>[] { sampleValues });
            var doc = Document.Current;

            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewAndFaceAnalysisData(null, new []{data}));
            Assert.Throws(typeof(System.ArgumentNullException), () => FaceAnalysisDisplay.ByViewAndFaceAnalysisData(doc.ActiveView, null));
            Assert.Throws(typeof(System.Exception), () => FaceAnalysisDisplay.ByViewAndFaceAnalysisData(doc.ActiveView, new SurfaceAnalysisData[]{}));
        }
    }
}

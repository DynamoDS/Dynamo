using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class RevitToProtoSolidTests : RevitNodeTestBase
    {
        private static void AssertAllSolidsAreConvertedProperly(IEnumerable<Autodesk.Revit.DB.Solid> allSolidsInDoc)
        {
            foreach (var solid in allSolidsInDoc)
            {
                var asmSolid = solid.ToProtoType(false);

                // revit can return negative volumes
                var revitVolume = Math.Abs(solid.Volume);
                var revitSurfaceArea = solid.SurfaceArea;
                var revitCentroid = solid.ComputeCentroid();

                var asmVolume = asmSolid.Volume;
                var asmSurfaceArea = asmSolid.Area;
                var asmCentroid = asmSolid.Centroid();

                revitVolume.ShouldDifferByLessThanPercentage(asmVolume, 1);
                revitSurfaceArea.ShouldDifferByLessThanPercentage(asmSurfaceArea, 1);

                revitCentroid.ShouldBeApproximately(asmCentroid, 1);
            }
        }

        [Test]
        [TestModel(@".\Solids.rfa")]
        public void ToProtoType_Boolean_ShouldConvertAllFormsInDocument()
        {
            var formSolids = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true)
                .Cast<Revit.Elements.Form>()
                .SelectMany(x => x.InternalGeometry())
                .OfType<Autodesk.Revit.DB.Solid>();

            AssertAllSolidsAreConvertedProperly(formSolids);
        }

        [Test]
        [TestModel(@".\MoreSolids.rfa")]
        public void ToProtoType_Boolean_ShouldConvertAllHermiteFormsInDocument()
        {
            var formSolids = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true)
                .Cast<Revit.Elements.Form>()
                .SelectMany(x => x.InternalGeometry())
                .OfType<Autodesk.Revit.DB.Solid>();

            AssertAllSolidsAreConvertedProperly(formSolids);
        }

        [Test]
        [TestModel(@".\cutWalls.rvt")]
        public void ToProtoType_Boolean_ShouldConvertCutWallsInDocument()
        {
            var allSolidsInDoc = ElementSelector.ByType<Autodesk.Revit.DB.Wall>(true)
                .Cast<Revit.Elements.Wall>()
                .SelectMany(x => x.InternalGeometry())
                .OfType<Autodesk.Revit.DB.Solid>()
                .ToList();

            Assert.AreEqual(3, allSolidsInDoc.Count);
            AssertAllSolidsAreConvertedProperly(allSolidsInDoc);
        }
    }
}

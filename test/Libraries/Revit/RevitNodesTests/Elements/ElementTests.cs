using System.Linq;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryReferences;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    class ElementTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\materials.rvt")]
        public void SetParameterByName_Element_CanSuccessfullySetMaterialByElement()
        {

            var mat = Revit.Elements.Material.ByName("Glass");

            var ele = ElementSelector.ByType<Autodesk.Revit.DB.FamilyInstance>(true).First();

            var paramName = "Body Material";
            var elemId0 = ele.GetParameterValueByName(paramName);

            Assert.AreNotEqual( mat.Id, elemId0 );

            ele.SetParameterByName(paramName, mat);

            DocumentManager.Regenerate();

            var elemId1 = ele.GetParameterValueByName(paramName);

            Assert.AreEqual(mat.InternalElement.Id, elemId1);

        }

        #region Face/Solid Extraction

        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Solids_ExtractsSolidAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var solids = ele.Solids;

            Assert.AreEqual(1, solids.Length);

            var bbox = BoundingBox.ByGeometry(solids);

            bbox.MaxPoint.ShouldBeApproximately(-64.266, -7.999, 60.693, 1e-3);
            bbox.MinPoint.ShouldBeApproximately(-92.708, -38.479, 0, 1e-3);
        }

        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Faces_ExtractsFacesAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var faces = ele.Faces;

            Assert.AreEqual(6, faces.Length);

            var bbox = BoundingBox.ByGeometry(faces);

            bbox.MaxPoint.ShouldBeApproximately(-64.266, -7.999, 60.693, 1e-3);
            bbox.MinPoint.ShouldBeApproximately(-92.708, -38.479, 0, 1e-3);

            var refs = faces.Select(x => ElementFaceReference.TryGetFaceReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(46874, refer.InternalReference.ElementId.IntegerValue);
            }
        }
        
        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void Geometry_ExtractsSolidAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var objects = ele.Geometry();
            Assert.AreEqual(1, objects.Length);

            var solids = objects.OfType<Autodesk.DesignScript.Geometry.Solid>();
            Assert.AreEqual(1, solids.Count());

            var bbox = BoundingBox.ByGeometry(solids);

            bbox.MaxPoint.ShouldBeApproximately(-64.266, -7.999, 60.693, 1e-3);
            bbox.MinPoint.ShouldBeApproximately(-92.708, -38.479, 0, 1e-3);
        }
        
        [Test]
        [TestModel(@".\AdaptiveComponents.rfa")]
        public void ElementFaceReferences_ExtractsExpectedReferences()
        {
            var ele = ElementSelector.ByElementId(46874, true);
            var refs = ele.ElementFaceReferences;

            Assert.AreEqual(6, refs.Length);

            foreach (var refer in refs)
            {
                Assert.AreEqual(46874, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        #endregion

        #region Curve extraction

        [Test]
        [TestModel(@".\projectWithNestedNonSharedAdaptiveComponent.rvt")]
        public void Curves_ExtractsCurvesFromNestedNonSharedFamilyInstanceAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(186006, true);
            var crvs = ele.Curves;

            Assert.AreEqual(4, crvs.Length);
            Assert.AreEqual(4, crvs.OfType<Autodesk.DesignScript.Geometry.Line>().Count());

            var bbox = BoundingBox.ByGeometry(crvs);

            bbox.MinPoint.ShouldBeApproximately(-31.607, -26.870, 0, 1e-3);
            bbox.MaxPoint.ShouldBeApproximately(25.434, 33.212, 0, 1e-3);

            var refs = crvs.Select(x => ElementCurveReference.TryGetCurveReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(186006, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        [Test]
        [TestModel(@".\GetCurvesFromFamilyInstance.rfa")]
        public void Curves_ExtractsCurvesAccountingForInstanceTransform()
        {
            var ele = ElementSelector.ByElementId(32107, true);
            var crvs = ele.Curves;

            Assert.AreEqual(4, crvs.Length);
            Assert.AreEqual(4, crvs.OfType<Autodesk.DesignScript.Geometry.Line>().Count());

            var bbox = BoundingBox.ByGeometry(crvs);

            bbox.MaxPoint.ShouldBeApproximately(15.240, 0, 0, 1e-3);
            bbox.MinPoint.ShouldBeApproximately(0, -30.480, 0, 1e-3);

            var refs = crvs.Select(x => ElementCurveReference.TryGetCurveReference(x));

            foreach (var refer in refs)
            {
                Assert.AreEqual(32107, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        [Test]
        [TestModel(@".\GetCurvesFromFamilyInstance.rfa")]
        public void ElementCurveReferences_ExtractsExpectedReferences()
        {
            var ele = ElementSelector.ByElementId(32107, true);
            var refs = ele.ElementCurveReferences;

            Assert.AreEqual(4, refs.Length);

            foreach (var refer in refs)
            {
                Assert.AreEqual(32107, refer.InternalReference.ElementId.IntegerValue);
            }
        }

        #endregion


    }
}

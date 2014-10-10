using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.Elements;

using RevitServices.Persistence;

using RTF.Framework;

namespace RevitTestServices.Elements
{
    [TestFixture]
    class ElementTests : GeometricRevitNodeTest
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
        }

    }
}

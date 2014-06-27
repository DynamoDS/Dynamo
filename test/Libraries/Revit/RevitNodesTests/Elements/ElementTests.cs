using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using NUnit.Framework;

using Revit.Elements;

using RevitServices.Persistence;

using RTF.Framework;

namespace DSRevitNodesTests.Elements
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

    }
}

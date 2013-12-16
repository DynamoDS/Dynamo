using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class FormTests
    {
        [Test]
        public void ByLoftingCurveReferences_ValidArgs()
        {
            var eles =
                ElementSelector.ByType<Autodesk.Revit.DB.CurveElement>(true)
                    .Cast<DSModelCurve>()
                    .Select(x => x.CurveReference);

            Assert.AreEqual(2, eles.Count());

            var loft = DSForm.ByLoftingCurveReferences(eles.ToArray(), false);

            Assert.NotNull(loft);
            Assert.IsTrue(DocumentManager.GetInstance().ElementExistsInDocument(loft.InternalElement.Id));
        }

        [Test]
        public void FaceReferencesProperty_ValidObject()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as DSForm;
            var faces = form.FaceReferences;
            Assert.IsTrue(faces.All(x => x != null));
            Assert.AreEqual(6, faces.Length);
        }

        [Test]
        public void SolidsProperty_ValidObject()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as DSForm;
            var solids = form.Solids;
            Assert.IsTrue(solids.All(x => x != null));
            Assert.AreEqual(1, solids.Length);
        }
    }
}

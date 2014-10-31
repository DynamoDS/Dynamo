using System.Collections.Generic;
using System.Linq;

using Revit.Elements;
using NUnit.Framework;
using Revit.GeometryReferences;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;
using Form = Revit.Elements.Form;
using ModelCurve = Revit.Elements.ModelCurve;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class FormTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\modelLines.rfa")]
        public void ByLoftingCurveReferences_ValidArgs()
        {
            IEnumerable<ElementCurveReference> eles =
                ElementSelector.ByType<Autodesk.Revit.DB.CurveElement>(true)
                    .Cast<ModelCurve>()
                    .Select(x => x.ElementCurveReference);

            Assert.AreEqual(2, eles.Count());

            var loft = Form.ByLoftCrossSections(eles.ToArray(), false);

            Assert.NotNull(loft);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID( loft.InternalElement.UniqueId)));
        }

        [Test]
        [TestModel(@".\blockAlone.rfa")]
        public void FaceReferencesProperty_ValidObject()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var faces = form.ElementFaceReferences; 
            Assert.IsTrue(faces.All(x => x != null));
            Assert.AreEqual(6, faces.Length);
        }

        [Test]
        [TestModel(@".\blockAlone.rfa")]
        public void SolidsProperty_ValidObject()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);

            var form = ele as Form;
            var solids = form.Solids;
            Assert.IsTrue(solids.All(x => x != null));
            Assert.AreEqual(1, solids.Length);
        }
    }
}

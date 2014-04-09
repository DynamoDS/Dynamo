using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;
using Form = Revit.Elements.Form;
using ModelCurve = Revit.Elements.ModelCurve;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class FormTests : RevitNodeTestBase
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }
         
        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
        [TestModel(@".\modelLines.rfa")]
        public void ByLoftingCurveReferences_ValidArgs()
        {
            var eles =
                ElementSelector.ByType<Autodesk.Revit.DB.CurveElement>(true)
                    .Cast<ModelCurve>()
                    .Select(x => x.CurveReference);

            Assert.AreEqual(2, eles.Count());

            var loft = Form.ByLoftingCurveReferences(eles.ToArray(), false);

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
            var faces = form.FaceReferences; 
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

using System.Linq;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ElementSelectorTests : RevitNodeTestBase
    {

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void FamilyByElementId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";
            var family = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.Family>()
                                                      .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            // use the element factory to do the same
            var famId = family.Id;
            var famFromFact = Revit.Elements.ElementSelector.ByElementId(famId.IntegerValue, true);

            Assert.NotNull(famFromFact);
            Assert.IsAssignableFrom(typeof(Family), famFromFact);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void FamilyByUniqueId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.Family>()
                                                      .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            // use the element factory to do the same by unique id
            var famUniqueId = family.UniqueId;
            var famFromFact = Revit.Elements.ElementSelector.ByUniqueId(famUniqueId, true);

            Assert.NotNull(famFromFact);
            Assert.IsAssignableFrom(typeof(Family), famFromFact);
            Assert.AreEqual(name, (famFromFact as Family).Name);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void FamilySymbolByElementId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.Family>()
                                          .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            var symbol = family.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == name);
            Assert.NotNull(symbol);

            // use the element factory to do the same
            var famSymEleId = symbol.Id;
            var famSymFromFact = Revit.Elements.ElementSelector.ByElementId(famSymEleId.IntegerValue, true);

            Assert.NotNull(famSymFromFact);
            Assert.IsAssignableFrom(typeof(FamilySymbol), famSymFromFact);
            Assert.AreEqual(name, (famSymFromFact as FamilySymbol).Name);
        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void FamilySymbolByUniqueId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.Family>()
                                        .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            var symbol = family.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == name);
            Assert.NotNull(symbol);

            // use the element factory to do the same
            var famSymUniqueId = symbol.UniqueId;
            var famSymFromFact = Revit.Elements.ElementSelector.ByUniqueId(famSymUniqueId, true);

            Assert.NotNull(famSymFromFact);
            Assert.IsAssignableFrom(typeof(FamilySymbol), famSymFromFact);
            Assert.AreEqual(name, (famSymFromFact as FamilySymbol).Name);
        }

        [Test]
        [TestModel(@".\block.rfa")]
        public void FormByType_ValidArgs()
        {
            var ele = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true).FirstOrDefault();
            Assert.NotNull(ele);
        }

        [Test, Ignore]
        public void ReferencePointByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore]
        public void ReferencePointByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore, Category("Failure")]
        public void FamilyInstanceByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore, Category("Failure")]
        public void FamilyInstanceByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore, Category("Failure")]
        public void DividedPathByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test, Ignore, Category("Failure")]
        public void DividedSurfaceByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

    }
}

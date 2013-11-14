using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Creation;
using DSRevitNodes;
using DSRevitNodes.Elements;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ElementSelectorTests : RevitTestBase
    {

        [Test]
        public void FamilyByElementId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";
            var family = DocumentManager.GetInstance().GetElements<Autodesk.Revit.DB.Family>()
                                                      .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            // use the element factory to do the same
            var famId = family.Id;
            var famFromFact = DSRevitNodes.Elements.ElementSelector.ByElementId(famId.IntegerValue);

            Assert.NotNull(famFromFact);
            Assert.IsAssignableFrom(typeof(DSFamily), famFromFact);
        }

        [Test]
        public void FamilyByUniqueId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.GetInstance().GetElements<Autodesk.Revit.DB.Family>()
                                                      .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            // use the element factory to do the same by unique id
            var famUniqueId = family.UniqueId;
            var famFromFact = DSRevitNodes.Elements.ElementSelector.ByUniqueId(famUniqueId);

            Assert.NotNull(famFromFact);
            Assert.IsAssignableFrom(typeof(DSFamily), famFromFact);
            Assert.AreEqual(name, (famFromFact as DSFamily).Name);
        }

        [Test]
        public void FamilySymbolByElementId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.GetInstance().GetElements<Autodesk.Revit.DB.Family>()
                                          .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            var symbol = family.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == name);
            Assert.NotNull(symbol);

            // use the element factory to do the same
            var famSymEleId = symbol.Id;
            var famSymFromFact = DSRevitNodes.Elements.ElementSelector.ByElementId(famSymEleId.IntegerValue);

            Assert.NotNull(famSymFromFact);
            Assert.IsAssignableFrom(typeof(DSFamilySymbol), famSymFromFact);
            Assert.AreEqual(name, (famSymFromFact as DSFamilySymbol).Name);
        }

        [Test]
        public void FamilySymbolByUniqueId_ValidArgs()
        {
            // obtain the element id for the box family
            var name = "Box";

            // look up the loaded family
            var family = DocumentManager.GetInstance().GetElements<Autodesk.Revit.DB.Family>()
                                        .FirstOrDefault(x => x.Name == name);
            Assert.NotNull(family);

            var symbol = family.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == name);
            Assert.NotNull(symbol);

            // use the element factory to do the same
            var famSymUniqueId = symbol.UniqueId;
            var famSymFromFact = DSRevitNodes.Elements.ElementSelector.ByUniqueId(famSymUniqueId);

            Assert.NotNull(famSymFromFact);
            Assert.IsAssignableFrom(typeof(DSFamilySymbol), famSymFromFact);
            Assert.AreEqual(name, (famSymFromFact as DSFamilySymbol).Name);
        }

        [Test]
        public void ReferencePointByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void ReferencePointByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void FamilyInstanceByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void FamilyInstanceByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void DividedPathByElementId_ValidArgs()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void DividedSurfaceByUniqueId_ValidArgs()
        {
            Assert.Inconclusive();
        }

    }
}

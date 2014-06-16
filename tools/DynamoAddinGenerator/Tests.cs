using System.Collections.Generic;
using System.Linq;
using Autodesk.RevitAddIns;
using Moq;
using NUnit.Framework;

namespace DynamoAddinGenerator
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void GetValidProducts_FindsValidProducts()
        {
            var prods = SomeValidProductsWithDynamo();
            var validProds = Program.GetValidProducts(prods);
            Assert.AreEqual(3, validProds.Count());
        }

        [Test]
        public void GetValidProducts_FindsNoValidProducts()
        {
            var prods = NoValidProductsWithNoDynamo();
            var validProds = Program.GetValidProducts(prods);
            Assert.AreEqual(0, validProds.Count());
        }

        [Test]
        public void BackupOldAddins_BacksUpOldAddins()
        {
            

        }

        public void BackupOldAddins_DoesNothing()
        {
            
        }

        private IEnumerable<IRevitProduct> NoValidProductsWithNoDynamo()
        {
            // Two Revit 2014 products we should handle
            var p1 = new Mock<IRevitProduct>();
            p1.Setup(p => p.ProductName).Returns("Revit1");
            p1.Setup(p => p.InstallLocation).Returns(@"C:\Revit1\");
            p1.Setup(p => p.AddinPath).Returns(@"C:\addins1\");
            p1.Setup(p => p.VersionString).Returns(RevitVersion.Revit2011.ToString);

            var p2 = new Mock<IRevitProduct>();
            p2.Setup(p => p.ProductName).Returns("Revit2");
            p2.Setup(p => p.InstallLocation).Returns(@"C:\Revit2\");
            p2.Setup(p => p.AddinPath).Returns(@"C:\addins2\");
            p2.Setup(p => p.VersionString).Returns(RevitVersion.Revit2012.ToString);

            return new List<IRevitProduct>() { p1.Object, p2.Object};
        }

        private IEnumerable<IRevitProduct> SomeValidProductsWithNoDynamo()
        {
            // Two Revit 2014 products we should handle
            var p1 = new Mock<IRevitProduct>();
            p1.Setup(p => p.ProductName).Returns("Revit1");
            p1.Setup(p => p.InstallLocation).Returns(@"C:\Revit1\");
            p1.Setup(p => p.AddinPath).Returns(@"C:\addins1\");
            p1.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);

            var p2 = new Mock<IRevitProduct>();
            p2.Setup(p => p.ProductName).Returns("Revit2");
            p2.Setup(p => p.InstallLocation).Returns(@"C:\Revit2\");
            p2.Setup(p => p.AddinPath).Returns(@"C:\addins2\");
            p2.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);

            // A Revit 2015 product we should  handle
            var p3 = new Mock<IRevitProduct>();
            p3.Setup(p => p.ProductName).Returns("Revit3");
            p3.Setup(p => p.InstallLocation).Returns(@"C:\Revit3\");
            p3.Setup(p => p.AddinPath).Returns(@"C:\addins3\");
            p3.Setup(p => p.VersionString).Returns(RevitVersion.Revit2015.ToString);

            // A Revit 2011 product we should not handle
            var p4 = new Mock<IRevitProduct>();
            p4.Setup(p => p.ProductName).Returns("Revit4");
            p4.Setup(p => p.InstallLocation).Returns(@"C:\Revit4\");
            p4.Setup(p => p.AddinPath).Returns(@"C:\addins4\");
            p4.Setup(p => p.VersionString).Returns(RevitVersion.Revit2011.ToString);

            return new List<IRevitProduct>() {p1.Object, p2.Object, p3.Object, p4.Object};
        }

        private IEnumerable<IRevitProduct> SomeValidProductsWithDynamo()
        {
            // Two Revit 2014 products we should handle
            var p1 = new Mock<IRevitProduct>();
            p1.Setup(p => p.ProductName).Returns("Revit1");
            p1.Setup(p => p.InstallLocation).Returns(@"C:\Revit1\");
            p1.Setup(p => p.AddinPath).Returns(@"C:\addins1\");
            p1.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);
            p1.Setup(p => p.CurrentDynamoAddinPath).Returns(@"C:\addins1\Dynamo.addin");

            var p2 = new Mock<IRevitProduct>();
            p2.Setup(p => p.ProductName).Returns("Revit2");
            p2.Setup(p => p.InstallLocation).Returns(@"C:\Revit2\");
            p2.Setup(p => p.AddinPath).Returns(@"C:\addins2\");
            p2.Setup(p => p.VersionString).Returns(RevitVersion.Revit2014.ToString);
            p2.Setup(p => p.CurrentDynamoAddinPath).Returns(@"C:\addins2\Dynamo.addin");

            // A Revit 2015 product we should  handle
            var p3 = new Mock<IRevitProduct>();
            p3.Setup(p => p.ProductName).Returns("Revit3");
            p3.Setup(p => p.InstallLocation).Returns(@"C:\Revit3\");
            p3.Setup(p => p.AddinPath).Returns(@"C:\addins3\");
            p3.Setup(p => p.VersionString).Returns(RevitVersion.Revit2015.ToString);
            p3.Setup(p => p.CurrentDynamoAddinPath).Returns(@"C:\addins3\Dynamo.addin");

            // A Revit 2011 product we should not handle
            var p4 = new Mock<IRevitProduct>();
            p4.Setup(p => p.ProductName).Returns("Revit4");
            p4.Setup(p => p.InstallLocation).Returns(@"C:\Revit4\");
            p4.Setup(p => p.AddinPath).Returns(@"C:\addins4\");
            p4.Setup(p => p.VersionString).Returns(RevitVersion.Revit2011.ToString);

            return new List<IRevitProduct>() { p1.Object, p2.Object, p3.Object, p4.Object };
        }
    }
}
